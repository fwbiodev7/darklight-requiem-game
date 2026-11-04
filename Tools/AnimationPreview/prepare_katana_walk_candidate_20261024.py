"""Stage six selected whole figures without changing Assets or Unity catalogs."""
from pathlib import Path
import hashlib
import importlib.util
import json
import subprocess
import sys

import numpy as np
from PIL import Image

import prepare_weapon_redesign as pipeline
from whole_figure_components import identify

ROOT = Path(__file__).resolve().parents[2]
NAME = 'ReqKatanaNormalNeutralWalkS20261024C'
ART = ROOT / 'ArtSources/WeaponRedesign20261024/candidates'
OUT = ROOT / 'outputs/redesign-armas-20261024/candidates/walk-s'
GENERATED = Path('C:/Users/Usuario/.codex/generated_images/01a16dd8-5d5d-7eb2-bf7c-7821805a43b6')
SOURCES = [
    ('B', 'exec-3195b112-9a24-4a5d-b11b-836ac0c25a6b.png', [0, 1, 2, 3]),
    ('C', 'exec-0682b582-a0fc-4d71-8bd4-f44fa9a5e341.png', [4, 5]),
]
# Sole centers inspected on complete source figures. The horizontal pivot is the
# midpoint of both projected boot supports, rather than the alternating low boot.
SUPPORTS = {
    ('B', 0): [[134, 482], [173, 428]],
    ('B', 1): [[382, 483], [426, 427]],
    ('B', 2): [[638, 483], [676, 438]],
    ('B', 3): [[913, 488], [873, 429]],
    ('C', 4): [[1155, 488], [1190, 438]],
    ('C', 5): [[1425, 490], [1393, 429]],
}


def whole_owners(image):
    """Use the existing complete-component ownership, including all soft alpha."""
    rgba = np.array(image)
    figures, owner = identify(image, rows=2, columns=6, minimum_area=15000, alpha_threshold=128)
    yy, xx = np.where((rgba[:, :, 3] > 0) & (owner == 0))
    best = np.full(xx.shape, np.inf)
    distance_only = np.full(xx.shape, np.inf)
    assigned = np.zeros(xx.shape, dtype=np.uint8)
    for i, figure in enumerate(figures):
        x0, y0, x1, y1 = figure['grid']
        dx = np.maximum(np.maximum(x0 - xx, xx - x1), 0)
        dy = np.maximum(np.maximum(y0 - yy, yy - y1), 0)
        distance = dx * dx + dy * dy
        distance_only = np.minimum(distance_only, distance)
        score = distance + ((xx - (x0 + x1) / 2) ** 2 + (yy - (y0 + y1) / 2) ** 2) * .00001
        closer = score < best
        assigned[closer] = i + 1
        best[closer] = score[closer]
    far = distance_only > 16 ** 2
    if np.any(rgba[yy[far], xx[far], 3] >= 32):
        raise ValueError('Visible pixels outside complete silhouettes; review original source')
    owner[yy, xx] = assigned
    return rgba, figures, owner


def main():
    ART.mkdir(parents=True, exist_ok=True)
    OUT.mkdir(parents=True, exist_ok=True)
    selected, provenance = [], []
    for label, filename, indices in SOURCES:
        preserved = ART / f'ReqKatanaNormalWalkS_SW-{label}-original.png'
        pipeline.preserved_copy(GENERATED / filename, preserved)
        rgba, figures, owner = whole_owners(Image.open(preserved).convert('RGBA'))
        source_hash = pipeline.sha(preserved)
        for index in indices:
            figure = figures[index]
            if figure['touchesExtractionEdge']:
                raise ValueError('Selected whole figure touches original image edge')
            ys, xs = np.where((owner == index + 1) & (rgba[:, :, 3] > 0))
            x0, y0, x1, y1 = int(xs.min()), int(ys.min()), int(xs.max()+1), int(ys.max()+1)
            mask = owner[y0:y1, x0:x1] == index + 1
            pixels = rgba[y0:y1, x0:x1].copy()
            pixels[~mask] = 0
            supports = SUPPORTS[label, index]
            anchor = [round((supports[0][0]+supports[1][0])/2), max(point[1] for point in supports)]
            selected.append(dict(pixels=pixels, mask=mask, rect=[x0,y0,x1-x0,y1-y0], anchor=anchor,
                                 bodyHeight=figure['bodyHeight'], label=label, sourceIndex=index))
            provenance.append(dict(source=preserved.relative_to(ROOT).as_posix(), sourceSha256=source_hash,
                                   sourceFrameIndex=index, sourceRect=[x0,y0,x1-x0,y1-y0],
                                   supportPoints=supports, sourceAnchor=anchor,
                                   selectedNonzeroAlphaPixels=int((pixels[:,:,3]>0).sum()),
                                   selectedRgbaSha256=hashlib.sha256(pixels[mask].tobytes()).hexdigest(),
                                   wholeFigure=True, anatomicalAssembly=False, resized=False, mirrored=False, rotated=False))
    extents = [(f['anchor'][0]-f['rect'][0], f['anchor'][1]-f['rect'][1],
                f['rect'][0]+f['rect'][2]-f['anchor'][0], f['rect'][1]+f['rect'][3]-f['anchor'][1]) for f in selected]
    left, top, right, bottom = [max(e[k] for e in extents)+16 for k in range(4)]
    cw, ch = left+right, top+bottom
    composite = Image.new('RGBA', (cw*6, ch))
    support_anchors = {}
    for i, (figure, record) in enumerate(zip(selected, provenance)):
        x0,y0,_,_ = figure['rect']; ax,ay = figure['anchor']
        dx,dy = i*cw+left-(ax-x0), top-(ay-y0)
        composite.paste(Image.fromarray(figure['pixels']), (dx,dy))
        actual = np.array(composite)[dy:dy+figure['pixels'].shape[0], dx:dx+figure['pixels'].shape[1]]
        if not np.array_equal(actual, figure['pixels']):
            raise ValueError('Whole-figure source RGBA changed while organizing the sheet')
        record.update(outputFrameIndex=i, compositeOffset=[dx,dy], rgbaExact=True)
        support_anchors[str(i)] = dict(anchor=[i*cw+left,top], method='Integer midpoint of two projected boot supports; lower sole baseline; complete original source inspected.', confidence='visual-whole-source-support')
    source = ART / (NAME+'-source.png')
    if source.exists():
        with Image.open(source) as previous:
            if previous.size!=composite.size or previous.convert('RGBA').tobytes()!=composite.tobytes():
                raise ValueError('Preserved candidate differs; create a new version')
    else:
        composite.save(source)
    job = dict(name=NAME, source=str(source), skin=0, weapon=0, form=0, columns=6, sourceRows=1,
               durationMs=[400/6]*6, supportAnchors=support_anchors,
               rows=[dict(sourceRow=0, direction=0, action='Walk', sourceIndices=list(range(6)),
                          sourceSpriteNames=[f'Today_RequiemWalkConsistentKatanaCardinal20261017_{i}' for i in range(6)])])
    pipeline.write(ART/'import-jobs-katana-walkS-20261024C.json', dict(jobs=[job]))
    pipeline.ART=ART
    pipeline.RES=OUT
    manifest,pages = pipeline.prepare(job)
    manifest.update(status='candidate', provenanceFile=(ART/(NAME+'-provenance.json')).relative_to(ROOT).as_posix(),
                    userVisualApprovalReported=True, imported=False, runtimeVerified=False)
    for frame, record in zip(manifest['frames'], provenance):
        frame['originalSource']=record
    for page in pages:
        page['status']='candidate'
    pipeline.write(OUT/(NAME+'.json'), manifest)
    pipeline.write(OUT/(NAME+'-pages.json'), dict(pages=pages, candidate=True, imported=False))
    report = dict(schemaVersion=1, name=NAME, source=source.relative_to(ROOT).as_posix(),
                  sourceSha256=pipeline.sha(source), sourceSize=list(composite.size),
                  selectedWholeFigures=6, selectedSourceFrames=['B0','B1','B2','B3','C4','C5'],
                  excludedDirections=['SW'], sourcesPreserved=True, allSelectedRgbaExact=True,
                  anatomyEdits=False, scale=1, records=provenance, pixelsPerUnit=manifest['pixelsPerUnit'],
                  pivotMethod='Horizontal midpoint of both projected boot supports; lower sole baseline.',
                  status='candidate', userVisualApprovalReported=True, imported=False, runtimeVerified=False,
                  htmlCycleMs=400, gifCycleMs=400, gifQuantizedFrameMs=[70,60,70,70,60,70])
    pipeline.write(ART/(NAME+'-provenance.json'), report)
    pipe=ROOT/'Tools/Skills/pixel-art-sprite-sheet-master/scripts/sprite_pipeline.py'
    for mode in ['audit','palette','preview']:
        target=OUT/('palette.json' if mode=='palette' else mode)
        result=subprocess.run([sys.executable,str(pipe),mode,'--image' if mode=='palette' else '--manifest',
                               str(OUT/(NAME+('.png' if mode=='palette' else '.json'))),'--out',str(target)],
                              capture_output=True,text=True,encoding='utf-8',errors='replace')
        (OUT/(mode+'.log')).write_text(result.stdout+result.stderr,encoding='utf-8')
        if result.returncode:
            raise RuntimeError(f'{mode} failed: {result.returncode}; see {OUT}')
    atlas=Image.open(OUT/(NAME+'.png')).convert('RGBA')
    spec=importlib.util.spec_from_file_location('sprite_pipeline',pipe)
    helper=importlib.util.module_from_spec(spec);spec.loader.exec_module(helper)
    cells=helper.aligned(manifest,atlas)
    gifs=[]
    for frame in manifest['frames']:
        aligned=cells[frame['id']]
        bg=Image.new('RGB',aligned.size,'#30343a')
        bg.paste(aligned,(0,0),aligned.getchannel('A'))
        gifs.append(bg.resize((bg.width*2,bg.height*2),Image.Resampling.NEAREST))
    gifs[0].save(OUT/'preview/clip-00.gif',save_all=True,append_images=gifs[1:],duration=report['gifQuantizedFrameMs'],
                 loop=0,disposal=2,optimize=False)
    audit=json.loads((OUT/'audit/report.json').read_text(encoding='utf-8'))
    print(json.dumps(dict(name=NAME,frames=6,pages=1,errors=len(audit['errors']),warnings=len(audit['warnings']),
                          preview=str(OUT/'preview/preview.html'),contact=str(OUT/'preview/contact.png'),imported=False)))


if __name__=='__main__':
    main()
