"""Behavioral checks on synthetic fixtures, not delivered game art."""
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

from PIL import Image
import sprite_pipeline as sp
import replace_frames as rf


class PipelineTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        im = Image.new('RGBA', (16, 8))
        for x, y, color in [(2, 3, 'red'), (3, 4, 'red'), (10, 3, 'blue'), (12, 4, 'blue')]:
            im.putpixel((x, y), (255, 0, 0, 255) if color == 'red' else (0, 0, 255, 255))
        im.save(self.root / 'source.png')
        self.manifest = self.root / 'source.json'
        self.data = dict(schemaVersion=1, image='source.png', imageSha256=sp.digest(self.root / 'source.png'),
                         nativeSize=[16, 8], pixelsPerUnit=8, frames=[
                             dict(id='a', rect=[0, 0, 8, 8], pivot=[.5, .125], durationMs=100),
                             dict(id='b', rect=[8, 0, 8, 8], pivot=[.25, .25], durationMs=150)],
                         clips=[dict(name='walk_S', frames=['a', 'b'], loop=True)])
        self.save()

    def tearDown(self):
        self.temp.cleanup()

    def save(self):
        self.manifest.write_text(json.dumps(self.data), encoding='utf-8')

    def report(self):
        self.save()
        return sp.inspect(self.manifest)[2]

    def run_cli(self, *args, expected=0):
        result = subprocess.run([sys.executable, str(Path(sp.__file__)), *map(str, args)],
                                capture_output=True, text=True)
        self.assertEqual(result.returncode, expected, result.stdout + result.stderr)
        return json.loads(result.stdout)

    def test_valid_sheet(self):
        self.assertTrue(self.report()['ok'])

    def test_source_hash_change_detected(self):
        self.data['imageSha256'] = '0' * 64
        self.assertIn('imageSha256 missing or mismatched', self.report()['errors'])

    def test_bounds_rejected(self):
        self.data['frames'][1]['rect'][0] = 15
        self.assertFalse(self.report()['ok'])

    def test_fractional_rect_rejected(self):
        self.data['frames'][0]['rect'][0] = .5
        self.assertFalse(self.report()['ok'])

    def test_bad_pivot_and_duration(self):
        self.data['frames'][0].update(pivot=[float('nan'), 0], durationMs=0)
        self.assertGreaterEqual(len(self.report()['errors']), 2)

    def test_static_walk_rejected(self):
        self.data['clips'][0]['frames'] = ['a', 'a']
        self.assertFalse(self.report()['ok'])

    def test_intentional_hold_accepted(self):
        self.data['clips'][0].update(frames=['a'], allowStatic=True)
        self.assertTrue(self.report()['ok'])

    def test_missing_clip_and_frame_rejected(self):
        self.data['requiredClips'] = ['run_N']
        self.data['clips'][0]['frames'].append('missing')
        self.assertGreaterEqual(len(self.report()['errors']), 2)

    def test_protected_baseline_detected(self):
        self.data['baseline'] = dict(image='source.png', sha256='0' * 64)
        self.assertIn('Approved baseline missing or SHA256 changed', self.report()['errors'])

    def test_pack_preserves_all_rgba_pixels_and_alignment(self):
        original_hash = sp.digest(self.root / 'source.png')
        self.run_cli('pack', '--manifest', self.manifest, '--out', self.root / 'packed')
        packed = sp.read(self.root / 'packed/sheet.json')
        pi = Image.open(self.root / 'packed/sheet.png').convert('RGBA')
        expected = sp.aligned(self.data, Image.open(self.root / 'source.png').convert('RGBA'))
        for f in packed['frames']:
            self.assertEqual(sp.crop_frame(pi, f).tobytes(), expected[f['id']].tobytes())
            self.assertEqual(f['provenance']['sourceSha256'], original_hash)
        self.assertEqual(original_hash, sp.digest(self.root / 'source.png'))

    def test_unity_y_conversion(self):
        self.run_cli('unity-json', '--manifest', self.manifest, '--out', self.root / 'unity.json')
        d = sp.read(self.root / 'unity.json')
        self.assertEqual(d['coordinateSystem'], 'bottom-left')
        self.assertEqual(d['frames'][0]['rect'], [0, 0, 8, 8])
        self.assertFalse(d['automaticUnityImport'])

    def test_unity_image_path_from_different_directory(self):
        self.run_cli('unity-json', '--manifest', self.manifest, '--out', self.root / 'nested/unity.json')
        d = sp.read(self.root / 'nested/unity.json')
        self.assertEqual((self.root / 'nested' / d['image']).resolve(), self.root / 'source.png')

    def test_opaque_sheet_rejected_for_isolated_sprites(self):
        im = Image.open(self.root / 'source.png').convert('RGB')
        im.save(self.root / 'source.png')
        self.data['imageSha256'] = sp.digest(self.root / 'source.png')
        self.assertIn('No fully transparent background pixels', self.report()['errors'])

    def test_wrong_frame_type_is_diagnostic(self):
        self.data['frames'][0] = 'bad'
        self.assertFalse(self.report()['ok'])

    def test_palette_counts_and_no_overwrite(self):
        self.run_cli('palette', '--image', self.root / 'source.png', '--out', self.root / 'palette.json')
        d = sp.read(self.root / 'palette.json')
        self.assertEqual(sum(c['count'] for c in d['entries']), 4)
        self.run_cli('palette', '--image', self.root / 'source.png', '--out', self.root / 'palette.json', expected=2)

    def test_preview_generates_contact_gif_html(self):
        self.run_cli('preview', '--manifest', self.manifest, '--out', self.root / 'preview')
        with Image.open(self.root / 'preview/clip-00.gif') as im:
            self.assertEqual(im.n_frames, 2)
            self.assertEqual(im.info['loop'], 0)
        self.assertTrue((self.root / 'preview/contact.png').is_file())
        self.assertIn('data:image/png;base64,', (self.root / 'preview/preview.html').read_text())

    def test_reverse_and_pingpong_editor_tags(self):
        frames = [dict(filename=f'f{i}', frame=dict(x=i*4, y=0, w=4, h=8), duration=100)
                  for i in range(4)]
        sp.write(self.root / 'editor.json', dict(frames=frames, meta=dict(image='source.png', frameTags=[
            dict(name='reverse', **{'from': 0, 'to': 3}, direction='reverse'),
            dict(name='pingpong', **{'from': 0, 'to': 3}, direction='pingpong')])))
        self.run_cli('from-aseprite', '--input', self.root / 'editor.json', '--out', self.root / 'normalized.json')
        d = sp.read(self.root / 'normalized.json')
        self.assertEqual(d['clips'][0]['frames'], ['f3', 'f2', 'f1', 'f0'])
        self.assertEqual(d['clips'][1]['frames'], ['f0', 'f1', 'f2', 'f3', 'f2', 'f1'])

    def test_trimmed_export_refused(self):
        sp.write(self.root / 'editor.json', dict(frames=[dict(trimmed=True)], meta={}))
        self.run_cli('from-aseprite', '--input', self.root / 'editor.json', '--out', self.root / 'normalized.json', expected=2)

    def test_audit_exit_status_and_report(self):
        self.data['frames'][0]['durationMs'] = -1
        self.save()
        result = self.run_cli('audit', '--manifest', self.manifest, '--out', self.root / 'audit', expected=2)
        self.assertGreater(result['errors'], 0)
        self.assertTrue((self.root / 'audit/REPORT.md').is_file())

    def repair_plan(self, size=(8, 8)):
        im = Image.new('RGBA', size)
        im.putpixel((size[0] // 2, size[1] // 2), (0, 255, 0, 255))
        im.save(self.root / 'replacement.png')
        path = self.root / 'plan.json'
        path.write_text(json.dumps(dict(wholeFramesConfirmed=True,
                                       replacements=[dict(frameId='a', image='replacement.png')])))
        return path

    def test_selective_repair_preserves_other_frames_and_source(self):
        plan = self.repair_plan()
        before = sp.digest(self.root / 'source.png')
        result = rf.replace(self.manifest, plan, self.root / 'repaired')
        self.assertEqual(result['replaced'], 1)
        self.assertEqual(result['untouchedFrames'], 1)
        self.assertTrue(result['outsidePixelsUnchanged'])
        source = Image.open(self.root / 'source.png').convert('RGBA')
        repaired = Image.open(self.root / 'repaired/sheet.png').convert('RGBA')
        self.assertEqual(sp.crop_frame(source, self.data['frames'][1]).tobytes(),
                         sp.crop_frame(repaired, self.data['frames'][1]).tobytes())
        self.assertEqual(before, sp.digest(self.root / 'source.png'))

    def test_selective_repair_refuses_unequal_resize(self):
        plan = self.repair_plan((16, 8))
        with self.assertRaisesRegex(ValueError, 'uniform integer'):
            rf.replace(self.manifest, plan, self.root / 'repaired')

    def test_selective_repair_refuses_overlapping_frames(self):
        plan = self.repair_plan()
        self.data['frames'][1]['rect'] = [4, 0, 8, 8]
        self.save()
        with self.assertRaisesRegex(ValueError, 'overlaps'):
            rf.replace(self.manifest, plan, self.root / 'repaired')

    def test_selective_repair_refuses_clipped_replacement(self):
        plan = self.repair_plan()
        im = Image.open(self.root / 'replacement.png').convert('RGBA')
        im.putpixel((0, 4), (0, 255, 0, 255))
        im.save(self.root / 'replacement.png')
        with self.assertRaisesRegex(ValueError, 'canvas edge'):
            rf.replace(self.manifest, plan, self.root / 'repaired')


if __name__ == '__main__':
    unittest.main()
