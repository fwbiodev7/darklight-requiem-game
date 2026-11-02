"""Read-only complete-figure comparisons at actual Unity visual scale.

Nearest-neighbor sizing is for these diagnostic previews only. No asset pixels,
proportions, import settings, metadata or gameplay are modified.
"""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT=ROOT/'outputs/retomada-20261101/proportion-audit'
DIRS=['S','SW','W','NW','N','NE','E','SE']
LABELS={0:'katana',2:'punhos',3:'moto'}
image_cache={}
CELL_W,CELL_H,BASELINE=360,360,310


def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))


def whole(sheet_path,name):
    sheet=read(sheet_path)
    image_path=sheet_path.with_name(sheet['image'])
    if image_path not in image_cache:image_cache[image_path]=Image.open(image_path).convert('RGBA')
    frame=next(f for f in sheet['frames'] if f['id']==name)
    x,y,w,h=frame['rect']
    return image_cache[image_path].crop((x,y,x+w,y+h)),frame['pivot'],frame.get('renderPixelsPerUnit',sheet['pixelsPerUnit'])


def render(image,pivot,ppu):
    scale=1.325*80/ppu
    size=(max(1,round(image.width*scale)),max(1,round(image.height*scale)))
    scaled=image.resize(size,Image.Resampling.NEAREST)
    canvas=Image.new('RGBA',(CELL_W,CELL_H),(236,236,236,255))
    offset=(round(CELL_W/2-pivot[0]*size[0]),round(BASELINE-(1-pivot[1])*size[1]))
    canvas.alpha_composite(scaled,offset)
    ImageDraw.Draw(canvas).line((0,BASELINE,CELL_W-1,BASELINE),fill=(140,140,140,255))
    return canvas


def main():
    global CELL_W,CELL_H,BASELINE
    catalog=read(BANK/'catalog.json')['pages']
    names=set()
    for day in ('20261031','20261101'):
        names.update(d['name'] for d in read(ROOT/f'outputs/retomada-{day}/art-integration.json')['deliveries'])
    idles={(p['weapon'],p['direction']):p for p in catalog
           if p['skin']==0 and p['form']==0 and p['action']=='Idle'}
    dual=BANK/'ReqMotoNormalDualSaws8AttackS20261027V1.json'
    dual_sheet=read(dual)
    # Fit every complete rectangle, including raised swords and wide lunges.
    # Increasing preview canvas space never changes the artwork or its scale.
    extents=[]
    candidates=[(BANK/(Path(p['resourcePath']).name+'.json'),n) for p in catalog
                if p['skin']==0 and p['form']==0 and (Path(p['resourcePath']).name in names or p['action']=='Idle')
                for n in p['spriteNames']]
    candidates += [(dual,f['id']) for f in dual_sheet['frames'][:8]]
    for path,name in candidates:
        image,pivot,ppu=whole(path,name);scale=1.325*80/ppu
        w,h=round(image.width*scale),round(image.height*scale)
        extents.append((max(pivot[0]*w,(1-pivot[0])*w),(1-pivot[1])*h,pivot[1]*h))
    CELL_W=2*(int(max(e[0] for e in extents))+10)
    BASELINE=int(max(e[1] for e in extents))+20
    CELL_H=BASELINE+int(max(e[2] for e in extents))+10
    pair_w,pair_h=CELL_W*2,CELL_H+35
    report=[]
    for weapon,label in LABELS.items():
        folder=OUT/label;folder.mkdir(parents=True,exist_ok=True)
        groups={}
        for page in catalog:
            if page['skin']!=0 or page['form']!=0 or page['weapon']!=weapon:continue
            name=Path(page['resourcePath']).name
            if name not in names:continue
            sheet_path=BANK/(name+'.json')
            if weapon==3:
                idle_path,idle_name=dual,dual_sheet['frames'][page['direction']]['id']
            else:
                idle=idles[(weapon,page['direction'])]
                idle_path,idle_name=BANK/(Path(idle['resourcePath']).name+'.json'),idle['spriteNames'][0]
            idle_image,idle_pivot,idle_ppu=whole(idle_path,idle_name)
            for phase,sprite in enumerate(page['spriteNames']):
                image,pivot,ppu=whole(sheet_path,sprite)
                pair=Image.new('RGBA',(pair_w,pair_h),(236,236,236,255))
                pair.alpha_composite(render(idle_image,idle_pivot,idle_ppu),(0,35))
                pair.alpha_composite(render(image,pivot,ppu),(CELL_W,35))
                draw=ImageDraw.Draw(pair);draw.text((8,4),'Approved Idle / ready',fill='black')
                draw.text((CELL_W+8,4),f"{page['action']} {page.get('attackKey','')} {DIRS[page['direction']]} phase {phase}",fill='black')
                draw.text((8,20),f'PPU {idle_ppu:.3f}; WVS 1.325',fill='black')
                draw.text((CELL_W+8,20),f'PPU {ppu:.3f}; WVS 1.325',fill='black')
                groups.setdefault(name,[]).append((pair,page,sprite))
        for name,items in groups.items():
            board=Image.new('RGBA',(pair_w*2,pair_h*((len(items)+1)//2)),(236,236,236,255))
            for i,(pair,page,sprite) in enumerate(items):board.alpha_composite(pair,((i%2)*pair_w,(i//2)*pair_h))
            path=folder/(name+'-worldscale.png');board.save(path)
            report.append({'weapon':weapon,'sheet':name,'figures':len(items),'preview':path.relative_to(ROOT).as_posix(),
                           'imageSha256':read(BANK/(name+'.json'))['imageSha256'],
                           'spriteNames':[item[2] for item in items]})
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT/'comparison-index.json').write_text(json.dumps({'worldVisualScale':1.325,'previewPixelsPerWorldUnit':80,
       'sourceAssetsModified':False,'completeRectangleCanvas':[CELL_W,CELL_H],
       'comparisons':report,'standaloneMountedTransitionsNeedSeparateBodyReview':True},indent=2)+'\n')
    print(json.dumps({'sheets':len(report),'wholeFigureComparisons':sum(r['figures'] for r in report),'assetsModified':False}))


if __name__=='__main__':main()
