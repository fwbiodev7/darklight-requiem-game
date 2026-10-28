"""Create a review-only delivery from a newly generated intact 24-pose sheet."""
import argparse
import json
from pathlib import Path
from types import SimpleNamespace
import sys

from PIL import Image
from audit_sprite_skill import ROOT, OUT, PREVIEW, canonical
from whole_figure_components import identify
import sprite_pipeline as sp


def main():
    manifest = sp.read(PREVIEW / 'pacote-v6/manifesto.json')
    page = next(p for p in manifest['pages'] if p['skin'] == 'Requiem' and p['weapon'] == 'adagas'
                and p['form'] == 'roxa' and p['kind'] == 'attack')
    source = OUT / 'Requiem-adagas-roxa-candidata-v3.png'
    with Image.open(source) as im:
        frames, _ = identify(im.convert('RGBA'), rows=6, columns=4)
    target = OUT / 'candidata-v3.json'
    canonical(page, source, target, candidate_frames=frames)
    result = sp.audit(SimpleNamespace(manifest=target, out=OUT / 'audit-candidata-v3', unity_meta=None))
    preview = sp.preview(SimpleNamespace(manifest=target, out=OUT / 'previa-candidata-v3-final', zoom=2))
    unity = sp.unity_json(SimpleNamespace(manifest=target, out=OUT / 'candidata-v3-unity.json'))
    pal = sp.palette(SimpleNamespace(image=source, out=OUT / 'paleta-candidata-v3.json'))
    report = dict(date='2026-10-20', completeFigures=len(frames),
                  extractionEdgeFigures=sum(f['touchesExtractionEdge'] for f in frames),
                  sourceSha256=sp.digest(source), audit=result, preview=preview,
                  unityMetadata=unity, palette=pal,
                  productionImported=False, visualApproval=False, unityRuntimeVerified=False,
                  notes=['24 poses retained, whole head/body/weapons, no part assembly.',
                         'PPU and foot anchors are for candidate preview; production calibration pending.',
                         'Under-eye color and face finishing still require comparison to idle at native scale.',
                         'Do not propagate this candidate to other skins or production yet.'])
    sp.write(OUT / 'QA-CANDIDATA-V3.json', report)
    print(json.dumps({k: v for k, v in report.items() if k not in ('notes', 'palette', 'unityMetadata', 'preview', 'audit')}))
    return 0 if result['ok'] else 2


if __name__ == '__main__':
    raise SystemExit(main())
