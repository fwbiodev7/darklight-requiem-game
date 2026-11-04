"""Read-only pose boards in common world scale; neither gameplay captures nor asset edits."""
from collections import defaultdict
from functools import lru_cache
from pathlib import Path
import hashlib,json,re,unicodedata,sys
from PIL import Image,ImageDraw,ImageFont
BASE=Path(__file__).resolve().parents[1];ROOT=BASE/'outputs/darklight-requiem-game'
OUT=BASE/'outputs/fechamento-fabius-20261103/animation-review-normal';OUT.mkdir(exist_ok=True)
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
ARCHIVE=ROOT/'ArtSources/ReviewedDeliveries/FabiusNormal20261103'
FONT=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',14)
DIRS=['S','SW','W','NW','N','NE','E','SE'];WEP={0:'Katana',2:'Punhos',3:'Moto'}
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
@lru_cache(maxsize=2)
def rgba(path):
    with Image.open(path) as im:return im.convert('RGBA')
def loadpage(page,folder,origin):
    name=page['resourcePath'].split('/')[-1];d=read(folder/(name+'.json'));byid={f['id']:f for f in d['frames']}
    frames=[]
    for i,fid in enumerate(page['spriteNames']):
        f=byid[fid];x,y,w,h=f['rect'];im=rgba(str(folder/d['image'])).crop((x,y,x+w,y+h))
        bbox=im.getchannel('A').point(lambda a:255 if a>16 else 0).getbbox() or (0,0,w,h)
        ppu=(page.get('framePixelsPerUnit') or [page['pixelsPerUnit']]*len(page['spriteNames']))[i]
        ratio=56*1.325/ppu
        px=(f['pivot'][0]*w-bbox[0])*ratio;py=((1-f['pivot'][1])*h-bbox[1])*ratio
        cut=im.crop(bbox);size=(max(1,round(cut.width*ratio)),max(1,round(cut.height*ratio)))
        cut=cut.resize(size,Image.Resampling.NEAREST)
        durations=page.get('durationMs') or []
        ms=durations[i] if len(durations)==len(page['spriteNames']) else f.get('durationMs',100)
        frames.append((cut,px,py,ms))
    return {'page':page,'frames':frames,'origin':origin}
groups=defaultdict(list);summary=[]
current=[p for p in read(BANK/'catalog.json')['pages'] if p['skin']==0 and p['form']==0 and p['weapon'] in WEP]
contexts={(p['weapon'],p['direction'],p['action'],p.get('attackKey','')) for p in current}
for p in current:
    action=p['action'];group=(f"attack-{WEP[p['weapon']]}-{p['attackKey']}" if action=='Attack' else f"states-{WEP[p['weapon']]}" if action in ('Dead','Hurt','Parry') else f"walk-{WEP[p['weapon']]}" if action=='Walk' else f"idle-dodge-{WEP[p['weapon']]}")
    groups[group].append(loadpage(p,BANK,'aplicado; Unity nao verificada'))
for p in read(ARCHIVE/'PAGES.json')['pages']:
    if p['action']=='Attack' and (p['weapon'],p['direction'],p['action'],p.get('attackKey','')) not in contexts:
        groups[f"archive-attack-{WEP[p['weapon']]}-{p['attackKey']}"].append(loadpage(p,ARCHIVE/'staging','arquivo revisado; ainda nao aplicado'))
# Normal Moto idle is a separate consumer; include its eight actual source poses.
d=read(BANK/'ReqMotoNormalDualSaws8AttackS20261027V1.json')
for direction,fid in enumerate(d['motorcycleDualSaws']['ready']):
    p=dict(resourcePath='Darklight/WeaponRedesign20261023/ReqMotoNormalDualSaws8AttackS20261027V1',spriteNames=[fid],pixelsPerUnit=d['pixelsPerUnit'],durationMs=[1000],weapon=3,direction=direction,action='Idle',attackKey='')
    groups['idle-dodge-Moto'].append(loadpage(p,BANK,'consumer DualSawsPose; pose estavel'))
back=BASE/'outputs/fechamento-fabius-20261103/moto-qa/corrected-back-v2/approved-attempt1'
for p in read(back/'PAGES-3clips-COORDINATOR-REVIEW.json')['pages']:
    groups['walk-Moto'].append(loadpage(p,back,'arte aprovada; staging; PPU proposto'))
fists=BASE/'outputs/fechamento-fabius-20261103/punhos-sw/cycle-v2'
groups['walk-Punhos'].append(loadpage(read(fists/'proposed-page-NOT-INTEGRATED.json'),fists,'correcao do quadro03; revisao'))
front=BASE/'outputs/fechamento-fabius-20261103/moto-six-directions/MotoWalkS_SW_SE-proportion-candidate'
fd=read(front/'MotoWalkS_SW_SE-proportion-candidate.json')
for clip in fd['clips']:
    direction=DIRS.index(clip['name'].split('_')[-1]);p=dict(resourcePath='Darklight/WeaponRedesign20261023/MotoWalkS_SW_SE-proportion-candidate',spriteNames=clip['frames'],pixelsPerUnit=fd['pixelsPerUnit'],durationMs=[120]*4,weapon=3,direction=direction,action='Walk',attackKey='')
    groups['walk-Moto'].append(loadpage(p,front,'candidato; densidade provisoria'))
index=[]
for group,entries in sorted(groups.items()):
    if '--walk-only' in sys.argv and not group.startswith('walk-'):continue
    bydir=defaultdict(list)
    for entry in entries:bydir[entry['page']['direction']].append(entry)
    rows=[]
    for direction in range(8):
        cells=[]
        for entry in sorted(bydir[direction],key=lambda x:x['page']['action']):
            for i,(im,px,py,ms) in enumerate(entry['frames']):cells.append((im,px,py,entry['page']['action']+' '+str(i)+' '+str(ms)+'ms',entry['origin']))
        rows.append(cells)
    allcells=[c for row in rows for c in row]
    if not allcells:continue
    left=max(c[1] for c in allcells)+8;right=max(c[0].width-c[1] for c in allcells)+8
    top=max(c[2] for c in allcells)+12;bottom=max(c[0].height-c[2] for c in allcells)+8
    cw=max(110,int(left+right));ch=max(150,int(top+bottom+44));cols=max(len(row) for row in rows)
    board=Image.new('RGB',(60+cols*cw,70+8*ch),(34,39,45));draw=ImageDraw.Draw(board)
    draw.text((8,5),group,fill='white',font=FONT);draw.text((8,25),'SIMULACAO NN: escala1.325,56px/un; pixels fonte intactos; NAO e gameplay',fill='#efca65',font=FONT)
    for row,cells in enumerate(rows):
        yy=70+row*ch;draw.text((5,yy+5),DIRS[row],fill='white',font=FONT)
        for col,(im,px,py,label,origin) in enumerate(cells):
            xx=60+col*cw;anchorx=xx+left;anchory=yy+top
            draw.line((xx,anchory,xx+cw-3,anchory),fill='#6c8b9d');board.paste(im,(round(anchorx-px),round(anchory-py)),im)
            draw.text((xx+3,yy+ch-38),label,fill='white',font=FONT)
            draw.text((xx+3,yy+ch-20),origin,fill='#afbdc6',font=FONT)
    slug=unicodedata.normalize('NFKD',group).encode('ascii','ignore').decode();slug=re.sub('[^A-Za-z0-9_-]','_',slug)
    dest=OUT/(slug+'.png');board.save(dest)
    index.append({'group':group,'file':dest.name,'pages':len(entries),'poses':len(allcells),'size':board.size})
if '--walk-only' in sys.argv:
    previous=read(OUT/'index.json')['boards'];index=[x for x in previous if not x['group'].startswith('walk-')]+index
(OUT/'index.json').write_text(json.dumps({'currentNormalPages':len(current),'boards':index,'scope':'All current Normal pages plus archived attack families, latest selected walk candidates/approvals and separate Moto idle; static pose overview only; no Unity/runtime certification'},indent=2,ensure_ascii=False),encoding='utf-8')
(OUT/'index.html').write_text('<!doctype html><meta charset="utf-8"><title>Fabius pose review</title><style>body{background:#20252b;color:white;font:16px sans-serif}img{max-width:100%;image-rendering:pixelated}</style><h1>Fabius - revisao de poses, nao gameplay</h1>'+''.join('<h2>'+x['group']+'</h2><img src="'+x['file']+'">' for x in index),encoding='utf-8')
print(json.dumps({'boards':len(index),'currentPages':len(current),'allPages':sum(x['pages'] for x in index),'poseInstances':sum(x['poses'] for x in index)}))
