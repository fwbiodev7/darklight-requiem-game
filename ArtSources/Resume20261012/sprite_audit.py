from pathlib import Path
import json
import numpy as np
from PIL import Image, ImageDraw

def components_for(mask):
    # Label contiguous opaque runs; no additional dependencies or image changes.
    runs, previous, parents = [], [], [0]
    def root(i):
        while parents[i]!=i:
            parents[i]=parents[parents[i]];i=parents[i]
        return i
    for y,row in enumerate(mask):
        edges=np.diff(np.r_[False,row,False].astype(np.int8))
        current=[]
        for a,b in zip(np.flatnonzero(edges==1),np.flatnonzero(edges==-1)):
            idx=len(parents);parents.append(idx)
            for pa,pb,pi in previous:
                if pb<a:continue
                if pa>b:break
                ra,rb=root(idx),root(pi)
                if ra!=rb:parents[max(ra,rb)]=min(ra,rb)
            current.append((a,b,idx));runs.append((y,a,b,idx))
        previous=current
    labels=np.zeros(mask.shape,np.int32);ids={};bounds={}
    for y,a,b,i in runs:
        r=root(i)
        if r not in ids:ids[r]=len(ids)+1;bounds[ids[r]]=[a,y,b,y+1]
        k=ids[r];labels[y,a:b]=k
        box=bounds[k];box[0]=min(box[0],a);box[1]=min(box[1],y);box[2]=max(box[2],b);box[3]=max(box[3],y+1)
    boxes=[(slice(b[1],b[3]),slice(b[0],b[2])) for b in bounds.values()]
    return labels,len(ids),boxes

ROOT = Path(r'C:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem')
OUT = Path(__file__).parent / 'sprite-audit'
OUT.mkdir(exist_ok=True)
records = []
for folder in ('Art', 'Art/Campaign', 'Art/User'):
    for metadata in (ROOT / 'Assets/Darklight' / folder).glob('*Rects.json'):
        name = metadata.name[:-10]
        if name=='ui' or any(t in name.lower() for t in ('hud', 'world', 'floor', 'architecture', 'weaponprops', 'appearance', 'extras')):
            continue
        png = metadata.with_name(name + '.png')
        if not png.exists():
            continue
        im = Image.open(png).convert('RGBA')
        data = np.asarray(im)
        labels, n, components = components_for(data[:, :, 3] >= 166)
        sizes = np.bincount(labels.ravel()); sizes[0] = 0
        frames = json.loads(metadata.read_text(encoding='utf-8-sig'))['frames']
        sheet = Image.new('RGBA', (len(frames) // 8 * 190, 8 * 165), (29, 32, 38, 255))
        draw = ImageDraw.Draw(sheet)
        entries = []
        for i, f in enumerate(frames):
            x,y,w,h = map(int, f['rect'])
            ids = labels[y:y+h, x:x+w]
            counts = np.bincount(ids.ravel(), minlength=n+1); counts[0] = 0
            main = int(counts.argmax())
            ys,xs = components[main-1]
            rect = list(map(int,[xs.start,ys.start,xs.stop-xs.start,ys.stop-ys.start]))
            contamination = int(counts.sum()-counts[main])
            clipped = int(sizes[main]-counts[main])
            entries.append({'i':i,'old':f['rect'],'main':rect,'clipped':clipped,'foreign':contamination,'label':main})
            crop = im.crop((x,y,x+w,y+h)); crop.thumbnail((170,138), Image.Resampling.NEAREST)
            cx,cy = (i % (len(frames)//8))*190, (i//(len(frames)//8))*165
            sheet.alpha_composite(crop,(cx+(190-crop.width)//2,cy+20))
            draw.text((cx+4,cy+3),f'{i}: clipped {clipped}, other {contamination}',fill='white')
        sheet.convert('RGB').save(OUT/(name+'-before.png'))
        large=[]
        for k,(ys,xs) in enumerate(components,1):
            if sizes[k]>300:
                large.append({'label':k,'size':int(sizes[k]),'rect':list(map(int,[xs.start,ys.start,xs.stop-xs.start,ys.stop-ys.start]))})
        records.append({'metadata':str(metadata),'png':str(png),'entries':entries,'large':large})
        bad = [e for e in entries if e['clipped']>10 or e['foreign']>20]
        print(name, 'frames',len(frames),'bad',len(bad),'clipped',sum(e['clipped'] for e in bad),'foreign',sum(e['foreign'] for e in bad))
        if '_' in name:print('  full silhouettes',len(large),'max component',max(c['size'] for c in large))
(OUT/'audit.json').write_text(json.dumps(records,indent=2),encoding='utf8')
