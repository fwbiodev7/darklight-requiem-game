"""Review delivery for Punhos da Ruína (formerly Eclipse Fangs). Whole figures only."""
from pathlib import Path
from types import SimpleNamespace
import gzip
import hashlib
import json
import shutil
import sys

import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/Skills/pixel-art-sprite-sheet-master/scripts'))
import sprite_pipeline as sp
from whole_figure_components import identify

ART = ROOT / 'ArtSources/EclipseFangs20261021/candidate-v1'
OUT = ROOT / 'outputs/eclipse-fangs-20261021/candidate-v1'
GENERATED = Path('C:/Users/Usuario/.codex/generated_images/01a16064-ee60-7061-99aa-d477920f92fa')
SOURCES = {
    'requiem-combo': 'exec-aa1f95b7-fc6b-4cf3-ba8a-fb15d262a7ff.png',
    'rock-combo': 'exec-a8fb9237-e4e1-4db0-aad2-66e286ec78c6.png',
    'formas': 'exec-6daa4612-8ec3-4fb9-8e0a-94b69ab67cc4.png',
    'oito-direcoes': 'exec-4b0c8920-ec9d-4d32-af20-e495d68491f6.png',
    'correcao-sw': 'exec-18e07d4e-da5f-41a8-a3ed-bd486d2b09b1.png',
}
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']
ACTIONS = ['guarda', 'respiracao', 'antecipacao_soco', 'soco', 'preparo_chute',
           'chute_lateral', 'giro', 'preparo_uppercut', 'uppercut_aereo',
           'chute_aereo', 'finalizacao', 'recuperacao']
DURATIONS = [550, 300, 160, 100, 130, 140, 150, 160, 170, 160, 230, 260]
ANCHORS = {
    'requiem-combo': [(202,323),(585,323),(958,323),(1342,320),(188,652),(507,647),
                      (945,652),(1337,653),(188,1070),(529,1070),(959,999),(1350,1000)],
    'rock-combo': [(202,324),(568,327),(945,327),(1328,319),(181,647),(491,641),
                   (926,645),(1355,654),(180,1065),(530,1065),(958,1005),(1340,994)],
}


def relative(path):
    return path.relative_to(ROOT).as_posix()


def protect():
    paths = set()
    for path in (ROOT / 'Assets/Darklight/Resources/Darklight').rglob('*'):
        if path.is_file() and any(s in path.name for s in ('Katana', 'Daggers', 'Catalog')):
            paths.add(path)
    for folder in ('Assets/Darklight/Runtime/Combat', 'Assets/Darklight/Runtime/Character'):
        paths.update(path for path in (ROOT / folder).glob('*.cs'))
    return {relative(path): sp.digest(path) for path in sorted(paths)}


def extract(path, rows, columns=None, minimum_area=1500):
    """Assign every nonzero-alpha source pixel to an intact identified figure.

    Solid connected silhouettes are identified by the established project helper.
    Only ownership extends to the faint antialiased/aura fringe. RGBA is preserved.
    No semantic body part is selected, transplanted, repainted, rotated or mirrored.
    """
    im = Image.open(path).convert('RGBA')
    figures, owners = identify(im, rows=rows, columns=columns if columns is not None else 4 if rows > 1 else 1,minimum_area=minimum_area)
    rgba = np.array(im)
    visible = rgba[:, :, 3] > 0
    for _ in range(max(im.size)):
        if np.all(owners[visible] > 0):
            break
        expanded = np.array(Image.fromarray(owners).filter(ImageFilter.MaxFilter(3)))
        owners = np.where(owners > 0, owners, expanded).astype(np.uint8)
    assert np.all(owners[visible] > 0), 'Unowned source pixels'
    frames = []
    restored = np.zeros_like(rgba)
    for i, figure in enumerate(figures):
        selected = visible & (owners == i + 1)
        yy, xx = np.where(selected)
        x0, y0, x1, y1 = int(xx.min()), int(yy.min()), int(xx.max()+1), int(yy.max()+1)
        tile = rgba[y0:y1, x0:x1].copy()
        tile[~selected[y0:y1, x0:x1]] = 0
        restored[selected] = rgba[selected]
        frames.append(dict(image=Image.fromarray(tile), sourceRect=[x0,y0,x1-x0,y1-y0],
                           bodyHeight=figure['bodyHeight'], solidRect=figure['rect'],
                           sourceGroundAnchor=figure['anchor'],
                           sourceVisiblePixels=int(selected.sum())))
    assert np.array_equal(restored[visible], rgba[visible]), 'Visible RGBA conservation failed'
    return im, frames


def palette(path, target):
    full = target / 'palette-full.json'
    result = sp.palette(SimpleNamespace(image=path, out=full))
    data = sp.read(full)
    raw = full.read_bytes()
    with gzip.open(target / 'palette-full.json.gz', 'wb') as stream:
        stream.write(raw)
    sp.write(target / 'palette.json', dict(source=data['source'], sha256=data['sha256'],
             colors=data['colors'], entries=data['entries'][:256], completeHistogram='palette-full.json.gz',
             completeHistogramSha256=hashlib.sha256(raw).hexdigest(), truncated=True,
             note='256 most frequent RGBA samples; full histogram preserved, no quantization.'))
    full.unlink()
    return result['colors']


def translate_preview(path, title):
    text = path.read_text(encoding='utf-8')
    replacements = {
        'Sprite animation review': title,
        'Candidate preview. Inspect identity, feet, weapons and loop. No audio.':
            'Visual aprovado em 21/10/2026. Poses principais de inspeção; intermediários e gameplay pendentes. Sem áudio.',
        '>Pause<': '>Pausar<', '>Next frame<': '>Próximo quadro<',
        '>Frame <': '>Quadro <', '>Speed <': '>Velocidade <',
        '>Background <': '>Fundo <', '>Contact sheet<': '>Contato completo<',
        "play.textContent='Play'": "play.textContent='Reproduzir'",
        "running?'Pause':'Play'": "running?'Pausar':'Reproduzir'",
        '<option selected>3</option>': '<option>3</option>',
        '<option>1</option>': '<option selected>1</option>',
    }
    for old, new in replacements.items():
        text = text.replace(old, new)
    path.write_text(text, encoding='utf-8')


def deliver(name, source, frames, rows):
    target = OUT / name
    target.mkdir(parents=True)
    if name in ANCHORS:
        anchors = ANCHORS[name]
    else:
        anchors = []
        for f in frames:
            x0,y0,x1,y1 = f['solidRect']
            anchors.append((round((x0+x1)/2), y1-2))
    # Integer padding around whole figures; no per-frame scale for animation.
    for f, (ax, ay) in zip(frames, anchors):
        x,y,w,h = f['sourceRect']
        f['localAnchor'] = [ax-x, ay-y]
    if name == 'oito-direcoes':
        # Only the wrong SW figure is replaced. Seven source figures stay RGBA-identical.
        _, corrected = extract(ART / 'correcao-sw-source.png', 1)
        fix = corrected[0]
        original = frames[1]
        scale = original['bodyHeight'] / fix['bodyHeight']
        size = tuple(round(value*scale) for value in fix['image'].size)
        image = fix['image'].resize(size, Image.Resampling.NEAREST)
        replacement = dict(original)
        replacement.update(image=image, localAnchor=[size[0]//2, size[1]-1],
                           replacementSource=relative(ART / 'correcao-sw-source.png'),
                           replacementRect=fix['sourceRect'], wholeFigureScale=scale,
                           replacementSize=list(size), replacementSourceSize=list(fix['image'].size))
        frames[1] = replacement
    left = max(f['localAnchor'][0] for f in frames) + 16
    top = max(f['localAnchor'][1] for f in frames) + 16
    right = max(f['image'].width-f['localAnchor'][0] for f in frames) + 16
    bottom = max(f['image'].height-f['localAnchor'][1] for f in frames) + 16
    cell = (left+right, top+bottom)
    atlas = Image.new('RGBA', (cell[0]*4, cell[1]*rows))
    descriptors, provenance = [], []
    for i, f in enumerate(frames):
        canvas = Image.new('RGBA', cell)
        offset = [left-f['localAnchor'][0], top-f['localAnchor'][1]]
        assert min(offset) >= 0
        canvas.paste(f['image'], tuple(offset))
        x,y = (i%4)*cell[0], (i//4)*cell[1]
        atlas.paste(canvas, (x,y))
        if name in ANCHORS:
            action, direction, skin, form = ACTIONS[i], 'NW' if i==6 else 'SE', 'Rock' if name.startswith('rock') else 'Requiem', 'normal'
        elif name == 'formas':
            action, direction, skin, form = 'repouso_estatico', 'S', 'Requiem' if i<4 else 'Rock', ['normal','ruptura','eclipse','absoluto'][i%4]
        else:
            action, direction, skin, form = 'repouso_estatico', DIRECTIONS[i], 'Requiem', 'normal'
        fid = f'{name}_{i:02}_{action}_{direction}'
        descriptors.append(dict(id=fid,rect=[x,y,*cell],pivot=[left/cell[0],1-top/cell[1]],
                                durationMs=DURATIONS[i] if name in ANCHORS else 1000,
                                action=action,direction=direction,skin=skin,form=form))
        provenance.append({k:v for k,v in f.items() if k != 'image'})
        # Equality includes RGBA of all retained original visible pixels.
        cropped = canvas.crop((*offset, offset[0]+f['image'].width, offset[1]+f['image'].height))
        assert cropped.tobytes() == f['image'].tobytes()
    atlas.save(target / 'sheet.png')
    ids = [d['id'] for d in descriptors]
    if name in ANCHORS:
        clips = [dict(name='repouso_curto', frames=[ids[i] for i in [0,1]],loop=True),
                 dict(name='combo_estudo_de_poses',frames=[ids[i] for i in [0,2,3,4,5,6,7,8,9,10,11,0]],loop=True),
                 dict(name='soco_estudo',frames=[ids[i] for i in [0,2,3,11,0]],loop=True),
                 dict(name='chute_estudo',frames=[ids[i] for i in [0,4,5,11,0]],loop=True),
                 dict(name='aereo_finalizacao_estudo',frames=[ids[i] for i in [0,7,8,9,10,11,0]],loop=True)]
    else:
        clips = [dict(name=f'{d["skin"]}_{d["form"]}_{d["direction"]}',frames=[d['id']],loop=False,allowStatic=True) for d in descriptors]
    ppu = frames[0]['bodyHeight']/1.99
    manifest = dict(schemaVersion=1,image='sheet.png',imageSha256=sp.digest(target/'sheet.png'),
                    nativeSize=list(atlas.size),sourceSize=list(source.size),logicalPixelResolution=None,
                    nativePixelArtCertified=False,pixelsPerUnit=ppu,status='candidate',visualApproval=False,
                    imported=False,runtimeVerified=False,frames=descriptors,clips=clips,
                    requiredClips=[c['name'] for c in clips],quality=dict(maxColors=256,maxHeightRatio=1.3),
                    provenance=dict(source=relative(ART/f'{name}-source.png'),sourceSha256=sp.digest(ART/f'{name}-source.png'),
                                    frames=provenance,rgbaPreservedExceptSWUniformScale=True),
                    notes=['Whole figures only. No anatomical assembly, planar rotation or mirroring.',
                           'Airborne frames have annotated ground-projection anchors; no artificial bobbing.',
                           'Animation is a principal-pose study; missing inbetweens are not interpolated.',
                           'PPU is provisional from approved 1.99-unit body height; Unity calibration pending.'])
    sp.write(target/'sheet.json',manifest)
    audit = sp.audit(SimpleNamespace(manifest=target/'sheet.json',out=target/'audit',unity_meta=None))
    assert audit['ok'], audit
    sp.preview(SimpleNamespace(manifest=target/'sheet.json',out=target/'preview',zoom=1))
    translate_preview(target/'preview/preview.html',f'Punhos da Ruína — {name}')
    colors = palette(target/'sheet.png',target)
    sp.unity_json(SimpleNamespace(manifest=target/'sheet.json',out=target/'unity-contract.json'))
    return dict(name=name,frames=len(frames),sourceSize=list(source.size),atlasSize=list(atlas.size),
                cell=list(cell),ppu=ppu,audit=audit,rgbaColors=colors,preview=relative(target/'preview/preview.html'))


def main():
    if OUT.exists():
        raise ValueError('Use a new versioned candidate destination')
    before = protect()
    OUT.mkdir(parents=True)
    for name, filename in SOURCES.items():
        shutil.copy2(GENERATED/filename,ART/f'{name}-source.png')
    references = [ROOT/'Assets/Darklight/Resources/Darklight/TodayCharacterAnimations'/f'{skin}IdleLivingCardinal20261017.png' for skin in ('Requiem','Rock')]
    references += [ROOT/'ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png',
                   ROOT/'ArtSources/PlayerStyleReference20261020/referencia-pixel-art-aprovada.png',
                   Path('C:/Users/Usuario/Pictures/Screenshots/Captura de tela 2026-10-21 182016.png')]
    refs = [dict(path=str(path),sha256=sp.digest(path),sourceSize=list(Image.open(path).size)) for path in references]
    rows = {'requiem-combo':3,'rock-combo':3,'formas':2,'oito-direcoes':2}
    deliveries = []
    for name, count in rows.items():
        source, frames = extract(ART/f'{name}-source.png',count)
        deliveries.append(deliver(name,source,frames,count))
    after = protect()
    assert before == after, 'Protected production resources changed'
    report = dict(date='2026-10-21',status='candidate',visualApproval=False,imported=False,runtimeVerified=False,
                  completeFigures=40,referenceInventory=refs,deliveries=deliveries,
                  protectedFiles=len(before),protectedSha256=before,protectedFilesUnchanged=True,
                  generatedTool='image_gen.imagegen',soundPlayed=False,unityPlayStarted=False,
                  sourcePreserved=True,wholeFiguresOnly=True,contactsVisuallyReviewed=False,
                  animationVisuallyReviewed=False,
                  pending=['User approval of equipment, guard, strike poses and aura palettes.',
                           'Inbetween animation phases and exact attack contacts.',
                           'Rock idle eight directions; transformed action/direction matrices for both skins.',
                           'Darker distinction between normal crimson and Ruptura scarlet aura.',
                           'Pixel-level face/eye/ramp consistency and indexed native pixel-art finishing.',
                           'Replace weapon2 combat definitions, labels/icons/help/VFX and chain renderer with Eclipse Fangs.',
                           'Preserve weapon index/save compatibility, katana, daggers and AbyssRider.',
                           'Unity import, phase/event alignment, focused silent EditMode checks and runtime validation after approval.'])
    sp.write(OUT/'QA-DELIVERY.json',report)
    sp.write(ART/'reference-inventory.json',refs)
    labels = [('requiem-combo','Requiem · repouso e ataques'),('rock-combo','Rock · repouso e ataques'),
              ('formas','Duas skins · quatro formas'),('oito-direcoes','Requiem · oito vistas de repouso')]
    options=''.join(f'<option value="{key}/preview/preview.html">{label}</option>' for key,label in labels)
    page='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Punhos da Ruína — prévia aprovada</title><style>body{margin:0;background:#121419;color:#eee;font:15px system-ui}header{padding:18px 24px;border-bottom:1px solid #444}h1{font-size:25px;margin:0 0 8px}p{margin:6px 0;max-width:1000px;color:#c7cbd3}select{margin-top:12px;padding:10px;background:#242831;color:white;border:1px solid #787e8b;border-radius:7px}a{color:#d7a3ff}iframe{border:0;width:100%;height:calc(100vh - 220px);min-height:680px}</style>
<header><h1>PUNHOS DA RUÍNA</h1><p>Visual aprovado · 21/10/2026 · 40 figuras completas · sem áudio</p><p>Manoplas e grevas em repouso e nas poses principais dos ataques. Katana e adagas preservadas. Ainda não integrado ao jogo.</p><p>Formas: normal em carmesim, Ruptura em escarlate, Eclipse em violeta e Absoluto em branco azulado. Na folha de formas: Requiem acima, Rock abaixo.</p><p>Use o seletor de sequência e os controles abaixo para pausar, avançar quadro a quadro, trocar o fundo ou ampliar. A animação é um estudo de poses; os intermediários finais estão pendentes.</p><select aria-label="Escolher folha" id="sheet">OPTIONS</select> · <a href="QA-DELIVERY.json">Auditoria e pendências</a></header>
<iframe title="Prévia de sprites" id="preview" src="requiem-combo/preview/preview.html"></iframe><script>document.getElementById('sheet').onchange=e=>document.getElementById('preview').src=e.target.value;</script></html>'''.replace('OPTIONS',options)
    (OUT/'index.html').write_text(page,encoding='utf-8')
    print(json.dumps(dict(completeFigures=40,protectedFiles=len(before),protectedFilesUnchanged=True,
                         deliveries=[dict(name=d['name'],frames=d['frames'],errors=d['audit']['errors'],warnings=d['audit']['warnings']) for d in deliveries],
                         preview=relative(OUT/'index.html'),status='candidate',unityImported=False)))


if __name__ == '__main__':
    main()
