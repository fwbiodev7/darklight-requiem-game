from pathlib import Path
import json,hashlib
from PIL import Image,ImageDraw
OUT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba/outputs/fechamento-fabius-20261103/moto-qa');OUT.mkdir(parents=True,exist_ok=True)
ROOT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba')
ST=ROOT/'outputs/darklight-requiem-game/ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging'
WE=ROOT/'outputs/continuacao-escola-20261103/moto-walk/ReqMotoNormalWalkW_E20261103SchoolV2.json'
items=[('Moto W A',WE,0),('Moto W B',WE,2),('Moto E A',WE,4),('Moto E B',WE,6),('Moto S A',ST/'ReqMotoNormalWalkS20261103ClosedV1.json',0),('Moto SW A',ST/'ReqMotoNormalWalkSW_SE20261103ClosedV2.json',0),('Katana W standing',ST/'ReqKatanaNormalCarregadoW_NW20261102V2.json',2),('Katana E walking A',ST/'ReqKatanaNormalWalkNE_E_SE20261102WeaponRepairSelectedCompact.json',4),('Katana E walking B',ST/'ReqKatanaNormalWalkNE_E_SE20261102WeaponRepairSelectedCompact.json',6)]
for title,mp,i in items:
    d=json.loads(mp.read_text(encoding='utf-8-sig'));f=d['frames'][i];im=Image.open(mp.parent/d['image']).convert('RGBA');x,y,w,h=f['rect'];fr=im.crop((x,y,x+w,y+h));name=title.replace(' ','-')
    fr.save(OUT/(name+'.png'))
    # Full figure retained: ruler annotates original canvas only; no anatomical crop.
    bg=Image.new('RGBA',(w+50,h+30),'#d9dce1');bg.alpha_composite(fr,(40,20));dr=ImageDraw.Draw(bg)
    for yy in range(0,h,20):
        dr.line((30,yy+20,w+40,yy+20),fill=(90,110,140,80));dr.text((0,yy+15),str(yy),fill='#101010')
    for xx in range(0,w,40):dr.text((xx+40,0),str(xx),fill='#101010')
    bg.convert('RGB').save(OUT/(name+'-ruler.png'))
mp=ST/'ReqKatanaNormalCarregadoW_NW20261102V2.json';d=json.loads(mp.read_text(encoding='utf-8-sig'));im=Image.open(mp.parent/d['image']).convert('RGBA')
# The archived metadata frame02 is not the upright pictured figure; identify that whole figure visually.
fr=im.crop((884,100,1246,494));fr.save(OUT/'Katana-W-upright-manual-whole.png')
bg=Image.new('RGBA',(412,424),'#d9dce1');bg.alpha_composite(fr,(40,20));dr=ImageDraw.Draw(bg)
for yy in range(0,394,20):
    dr.line((30,yy+20,402,yy+20),fill=(90,110,140,80));dr.text((0,yy+15),str(yy),fill='#101010')
for xx in range(0,362,40):dr.text((xx+40,0),str(xx),fill='#101010')
bg.convert('RGB').save(OUT/'Katana-W-upright-manual-whole-ruler.png')
print('Extracted full frames and rulers:',len(items))
