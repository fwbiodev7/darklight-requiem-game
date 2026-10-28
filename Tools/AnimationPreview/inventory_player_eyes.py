"""Inventory actual player source textures and full sprite rectangles for eye QA."""
import hashlib
import argparse
import json
from pathlib import Path
import re
import sys

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'work/skill-validation-deps'))
import yaml

RES = ROOT / 'Assets/Darklight/Resources'
OUT = ROOT / 'outputs/olhos-todas-formas-20261020'


def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def load_asset(p):
    return parse_asset(p.read_text(encoding='utf-8-sig'))


def parse_asset(text):
    text = text.replace('\r\n','\n')
    text = re.sub(r'^%.*\n', '', text, flags=re.M)
    text = re.sub(r'^--- !u!\d+ &\d+', '---', text, flags=re.M)
    return yaml.safe_load(text)['MonoBehaviour']


def main():
    parser=argparse.ArgumentParser();parser.add_argument('--out',type=Path,default=OUT);args=parser.parse_args()
    output=args.out.resolve();output.mkdir(parents=True,exist_ok=True)
    metadata = output / 'metadata'
    metadata.mkdir(exist_ok=True)
    normal = load_asset(RES / 'Darklight/TodayCharacterCatalog.asset')['pages']
    transformed = load_asset(RES / 'Darklight/TransformedCatalog.asset')['pages']
    selected = {}
    normal_by_name = {p['name']:p for p in normal}
    def add(resource, role, form=None, base=None):
        if not resource:
            return
        entry = selected.setdefault(resource, dict(roles=[], forms=[], baseNames=[]))
        if role not in entry['roles']:
            entry['roles'].append(role)
        if form is not None and form not in entry['forms']:
            entry['forms'].append(form)
        if base and base not in entry['baseNames']:
            entry['baseNames'].append(base)
    for page in normal:
        add(page['resourcePath'], 'normal-catalog', 0, page['name'])
    for page in transformed:
        add(page.get('resourcePath'), 'transformed-catalog', page['form'], page['baseName'])
        for ref in page.get('references') or []:
            add(ref.get('resourcePath'), 'transformed-reference', page['form'], page['baseName'])
    ruin_catalog=RES/'Darklight/RuinFists/catalog.json'
    if ruin_catalog.exists():
        for page in json.loads(ruin_catalog.read_text(encoding='utf-8-sig'))['pages']:
            add(page['resourcePath'],'ruin-fists',page['form'])
            selected[page['resourcePath']]['authoredDirections']=[page['direction']]
    if (RES/'Darklight/Multiverse/MotorcycleSouth.png').exists():
        add('Darklight/Multiverse/MotorcycleSouth','motorcycle-expansion',0)
        selected['Darklight/Multiverse/MotorcycleSouth']['authoredDirections']=[0]
    for p in (RES/'Darklight/AbyssPrototype').glob('*.png'):
        if not any(k in p.stem for k in ['Owner','Palette','Aura','Wing','Shadow']):
            add(p.relative_to(RES).with_suffix('').as_posix(), 'abyss-bank')
    for p in (RES/'Darklight/UserWholeSprites').glob('*.png'):
        add(p.relative_to(RES).with_suffix('').as_posix(), 'user-fallback')
    legacy = load_asset(RES/'Darklight/OriginalVampireCatalog.asset')
    guids = set()
    def visit(value):
        if isinstance(value, dict):
            if isinstance(value.get('source'), dict) and value['source'].get('guid'):
                guids.add(value['source']['guid'])
            for v in value.values():
                visit(v)
        elif isinstance(value, list):
            for v in value:
                visit(v)
    visit(legacy)
    for p in RES.rglob('*.png.meta'):
        with p.open(encoding='utf-8-sig') as stream:
            first = ''.join(next(stream,'') for _ in range(3))
        match = re.search(r'guid: ([0-9a-f]+)', first)
        if match and match.group(1) in guids:
            add(Path(str(p)[:-5]).relative_to(RES).with_suffix('').as_posix(), 'legacy-catalog')
    records, failures = [], []
    for resource, roles in sorted(selected.items()):
        png = RES/(resource+'.png')
        meta_path = Path(str(png)+'.meta')
        if not png.is_file() or not meta_path.is_file():
            failures.append(dict(resource=resource, error='PNG or meta missing'))
            continue
        meta = yaml.safe_load(meta_path.read_text(encoding='utf-8-sig'))['TextureImporter']
        with Image.open(png) as im:
            width, height = im.size
        sprites = meta.get('spriteSheet', {}).get('sprites', [])
        if not sprites and meta.get('spriteMode') == 1:
            sprites = [dict(name=png.stem,rect=dict(x=0,y=0,width=width,height=height),pivot=meta.get('spritePivot',dict(x=.5,y=.5)))]
        if not sprites:
            failures.append(dict(resource=resource,error='No whole-sprite rectangles'))
            continue
        directions, kind, blink, phases = [], '', -1, 0
        if roles.get('authoredDirections'):
            directions=roles['authoredDirections'];phases=len(sprites);kind='combat-or-motion'
        for name in roles['baseNames']:
            page = normal_by_name.get(name)
            if page:
                packed = re.match('[0-9a-fA-F]*', str(page.get('directions',''))).group()
                if len(packed) % 8 == 0:
                    directions = [int.from_bytes(bytes.fromhex(packed[i:i+8]), 'little') for i in range(0,len(packed),8)]
                kind, blink, phases = page.get('kind',''), page.get('idleBlinkFrame',-1), page.get('phases',0)
                break
        # These physical support atlases have four phases per direction. Catalog
        # pages compose eight-phase clips from them and do not describe this grid.
        if 'Support' in png.stem and len(sprites) == 32:
            directions, phases, blink = list(range(8)), 4, -1
        frames = []
        for index, sprite in enumerate(sprites):
            rect = sprite['rect']
            x,y,w,h = [int(rect[k]) for k in ('x','y','width','height')]
            pivot = sprite.get('pivot',dict(x=.5,y=.5))
            number_match = re.search(r'_(\d+)$',sprite['name'])
            phase_index = int(number_match.group(1)) if number_match else index
            direction = directions[phase_index//phases] if phases and phase_index//phases < len(directions) else None
            frames.append(dict(id=sprite['name'], rect=[x,height-y-h,w,h],
                               pivot=[float(pivot['x']),float(pivot['y'])],durationMs=100,
                               direction=direction, kind=kind,
                               expectedClosed=bool(phases and blink>=0 and phase_index%phases==blink),
                               expectedBack=direction==4))
        output_name = resource.replace('/','__')+'.json'
        d = dict(schemaVersion=1,image=str(png.resolve()),imageSha256=sha(png),nativeSize=[width,height],
                 pixelsPerUnit=meta.get('spritePixelsToUnits',100),status='candidate',frames=frames,
                 clips=[dict(name='source',frames=[f['id'] for f in frames],loop=True,allowStatic=len(frames)==1)],
                 resourcePath=resource,roles=roles)
        (metadata/output_name).write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
        mask_report=[]
        for suffix in ['_Owner','_Palette']:
            candidate=png.with_stem(png.stem+suffix)
            if not candidate.exists():continue
            with Image.open(candidate) as mask:
                histogram=mask.convert('RGBA').getchannel('R').histogram()
            mask_report.append(dict(resource=resource+suffix,redChannelSamples=sum(histogram[128:]),sha256=sha(candidate)))
        records.append(dict(resource=resource,texture=png.stem,frames=len(frames),sha256=d['imageSha256'],
                            metadata=output_name,selectorCandidates=mask_report,**roles))
    report=dict(normalPages=len(normal),transformedPages=len(transformed),textures=len(records),
                frames=sum(r['frames'] for r in records),sources=records,failures=failures,
                sourcePixelsModified=False,productionApplied=False)
    report['selectorLimit']='Nonzero R samples describe mask data, not verified eye anatomy or complete iris coverage.'
    (output/'INVENTARIO.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='sources'}))
    return 0 if not failures else 2


if __name__ == '__main__':
    raise SystemExit(main())
