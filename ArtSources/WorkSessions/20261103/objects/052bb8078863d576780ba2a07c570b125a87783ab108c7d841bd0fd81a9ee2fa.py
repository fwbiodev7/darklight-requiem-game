from pathlib import Path
from PIL import Image,ImageDraw
import json
ROOT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba');WORK=ROOT/'work/moto-resume1311';WORK.mkdir(exist_ok=True)
candidate=ROOT/'outputs/fechamento-fabius-20261103/moto-six-directions/MotoWalkNW_N_NE-proportion-candidate/MotoWalkNW_N_NE-proportion-candidate.json'
canonical=ROOT/'outputs/darklight-requiem-game/Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqKatanaNormalWalkNW_N20261027Selected.json'
ne=ROOT/'outputs/darklight-requiem-game/ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/ReqKatanaNormalWalkNE_E_SE20261102WeaponRepairSelectedCompact.json'
for label,p,i in [('MotoNW',candidate,0),('MotoN',candidate,4),('MotoNE',candidate,8),('KatanaNW',canonical,0),('KatanaN',canonical,6),('KatanaNE',ne,0),('KatanaNE1',ne,1)]:
    d=json.loads(p.read_text(encoding='utf-8-sig'));f=d['frames'][i];img=Image.open(p.parent/d['image']).convert('RGBA');x,y,w,h=f['rect'];fr=img.crop((x,y,x+w,y+h));out=Image.new('RGBA',(w+40,h+24),'#dfe3e8');out.alpha_composite(fr,(35,20));dr=ImageDraw.Draw(out)
    for yy in range(0,h,20):dr.line((28,yy+20,w+35,yy+20),fill='#8c9eb5');dr.text((0,yy+14),str(yy),fill='#111111')
    out.convert('RGB').save(WORK/(label+'.png'))
