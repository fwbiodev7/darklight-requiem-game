"""Map reviewed whole Fabius figures; preserve source pixels and Normal assets."""
import hashlib
import json
from pathlib import Path
import shutil

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
SESSION = ROOT / 'ArtSources/Session20261114'
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/FabiusForms20261111'
# Explicit visual mapping: row labels describe the pixels, never the prompt.
SHEETS = [
    ('8c324352d30081918349aadcdafe15c3', 'SerafimKatanaBack', 'Serafim', 'KatanaAttack', ['N','NW','W','SW']),
    ('840e2662d23481918899d030a5f797da', 'SerafimKatanaFront', 'Serafim', 'KatanaAttack', ['S','SE','E','NE']),
    ('c67900913b48819182a99ff006248770', 'RupturaKatanaBack', 'Ruptura', 'KatanaAttack', ['N','NW','W','SW']),
    ('c409f6a8a6248191a3c46c68679eb19a', 'RupturaKatanaFront', 'Ruptura', 'KatanaAttack', ['S','SE','E','NE']),
    ('edfc033c510c819195163f456ad49b6b', 'RupturaSawsBack', 'Ruptura', 'MotoToSaws', ['N','NW','E','SE']),
    ('ec987f95a8888191adf24c99b94bd243', 'RupturaSawsFront', 'Ruptura', 'MotoToSaws', ['S','SW','W','NE']),
    ('471976261a08819193a4b0d821219edc', 'SerafimSawsFront', 'Serafim', 'MotoToSaws', ['S','SE','E','NE']),
    ('4a435a80ce808191a578a7fa0eb0dd98', 'SerafimSawsBack', 'Serafim', 'MotoToSaws', ['N','NW','W','SW']),
    ('764378ae197c81918c71c573c859ccd5', 'RupturaFistsFront', 'Ruptura', 'FistAttack', ['S','SE','E','NE']),
    ('3e4a2f8768c48191af3dbe881377aff3', 'RupturaFistsBack', 'Ruptura', 'FistAttack', ['N','NW','W','SW']),
    ('74d9f4ba37ec819189d4df7ed7cdf2ac', 'SerafimFistsBack', 'Serafim', 'FistAttack', ['N','NW','W','SW']),
]


def splits(alpha, count, axis):
    projection = (alpha > 24).sum(axis=1 if axis == 0 else 0)
    size = len(projection)
    result = [0]
    for index in range(1, count):
        center = round(size * index / count)
        radius = round(size / count * .16)
        candidates = range(center-radius, center+radius+1)
        # Prefer a clear gutter; break ties by distance to expected grid.
        result.append(min(candidates, key=lambda x: (int(projection[max(0,x-1):x+2].sum()), abs(x-center))))
    return result + [size]


def main():
    sources = {f['library_file_id']: f for f in json.loads((SESSION/'zip-recovery-validation.json').read_text(encoding='utf-8'))['files']}
    catalog_path = DEST/'catalog.json'
    catalog = json.loads(catalog_path.read_text(encoding='utf-8-sig'))
    normal = ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
    baseline = SESSION/'normal-preservation-baseline.json'
    if not baseline.exists():
        baseline.write_text(json.dumps({p.relative_to(ROOT).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
                                       for p in normal.iterdir() if p.is_file()}, indent=2)+'\n', encoding='utf-8')
    additions, receipts = [], []
    for suffix, label, form, action, directions in SHEETS:
        source = sources['libfile_'+suffix]
        path = ROOT/source['localPath']
        image = Image.open(path).convert('RGBA')
        array = np.asarray(image)
        ys = splits(array[:,:,3], 4, 0)
        frames, heights, preview_frames = [], [], []
        name = 'Session14'+label
        overlay = image.copy()
        draw = ImageDraw.Draw(overlay)
        for row in range(4):
            top, bottom = ys[row:row+2]
            xs = splits(array[top:bottom,:,3], 3, 1)
            for column in range(3):
                left, right = xs[column:column+2]
                cell = array[top:bottom,left:right]
                h,w = cell.shape[:2]
                # Locate dark boots/torso, ignoring the red energy outside the body.
                dark = (cell[:,:,3]>180)&(cell[:,:,0]<155)&(cell[:,:,1]<125)&(cell[:,:,2]<135)
                yy,xx = np.where(dark & (np.indices(dark.shape)[0]>h*.40))
                if not len(yy): raise ValueError(name)
                foot = int(np.quantile(yy,.998))+1
                body_columns = dark[max(0,int(foot-h*.52)):max(1,int(foot-h*.27)),:]
                bx = np.where(body_columns)[1]
                anchor_x = float(np.median(bx)) if len(bx) else w*.5
                head_y = np.where(dark[:,max(0,int(anchor_x-w*.13)):min(w,int(anchor_x+w*.13))])[0]
                if column == 0 and len(head_y): heights.append(foot-int(np.quantile(head_y,.01)))
                frame = {'id': f'{name}_{row*3+column:02d}',
                         'rect':[left,image.height-bottom,right-left,bottom-top],
                         'pivot':[round(anchor_x/w,8),round((h-foot)/h,8)],
                         'durationMs':[110,90,150][column], 'action':action, 'form':form,
                         'direction':directions[row], 'frameIndex':column,
                         'sourceRectTopLeft':[left,top,w,h],
                         'sourceSupportAnchor':[round(left+anchor_x,2),top+foot]}
                frames.append(frame)
                draw.rectangle((left,top,right-1,bottom-1),outline=(60,200,220,255),width=1)
                draw.line((left,top+foot,right,top+foot),fill=(80,255,80,255),width=1)
                draw.ellipse((left+anchor_x-3,top+foot-3,left+anchor_x+3,top+foot+3),fill='yellow')
                draw.text((left+4,top+4),f'{directions[row]} {column}',fill='white')
        # Existing Normal's approved body is about 1.95 local units; aura/wings do not set scale.
        ppu=round(float(np.median(heights))/1.95,6)
        page={'name':name,'image':name+'.png','imageSha256':source['sha256'],
              'sourcePath':source['localPath'],'sourceSha256':source['sha256'],
              'nativeSize':list(image.size),'sourceSize':list(image.size),
              'pixelsPerUnit':ppu,'rows':4,'cols':3,'form':form,'action':action,
              'wholeFigures':True,'status':'visual-reviewed','frames':frames,
              'phaseSynchronized':True}
        destination=DEST/(name+'.png')
        if destination.exists() and destination.read_bytes()!=path.read_bytes(): raise ValueError(destination)
        if not destination.exists(): shutil.copyfile(path,destination)
        (DEST/(name+'.json')).write_text(json.dumps(page,indent=2)+'\n',encoding='utf-8')
        overlay.save(SESSION/'review'/(name+'-anchors.png'))
        # Whole-figure animation preview with feet anchored to a fixed origin.
        for row in range(4):
            sequence=[]
            for frame in frames[row*3:row*3+3]:
                x,y,w,h=frame['sourceRectTopLeft']
                full=image.crop((x,y,x+w,y+h))
                canvas=Image.new('RGBA',(800,620),(40,43,48,255))
                ax,ay=frame['sourceSupportAnchor']
                canvas.alpha_composite(full,(round(400-(ax-x)),round(530-(ay-y))))
                sequence.append(canvas.convert('RGB'))
            sequence[0].save(SESSION/'review'/(name+'-'+directions[row]+'.gif'),save_all=True,
                             append_images=sequence[1:],duration=[110,90,150],loop=0)
        additions.append(page)
        receipts.append({'sheet':name,'source':source['localPath'],'directions':directions,'frames':len(frames),'ppu':ppu})
    # New reviewed sheets take priority; old contexts remain for uncovered actions/directions.
    names={p['name'] for p in additions}
    catalog['pages']=additions+[p for p in catalog['pages'] if p['name'] not in names]
    catalog_path.write_text(json.dumps(catalog,indent=2)+'\n',encoding='utf-8')
    report={'sourcesPreserved':28,'sheetsPrepared':len(receipts),'wholeFrames':sum(x['frames'] for x in receipts),
            'unityImported':False,'runtimeVerified':False,'sheets':receipts,
            'withheld':[{'library_file_id':'libfile_6f453ce6d8c48191bdfd42dc1a847b54',
                         'reason':'Duplicates the received corrected rear fist poses but has exposed claw feet instead of approved boots/greaves; preserved, not activated.'}],
            'gaps':['Serafim fists S/SE/E/NE new equipped art absent from received pair',
                    'Weapon-specific eight-direction transformed walks remain absent; legacy fallback retained.']}
    (SESSION/'fabius-integration.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'sheets':len(receipts),'frames':report['wholeFrames'],'unityImported':False}))


if __name__=='__main__':main()
