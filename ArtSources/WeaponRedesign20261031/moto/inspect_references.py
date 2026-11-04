from pathlib import Path
import json
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'outputs/retomada-20261031/moto'
path=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqMotoNormalDualSaws8AttackS20261027V1.json'
m=json.loads(path.read_text());im=Image.open(path.with_suffix('.png')).convert('RGBA')
frames=m['frames'][:8]
canvas=Image.new('RGBA',(im.width,im.height),(210,210,210,255))
canvas.alpha_composite(im)
canvas.save(OUT/'canonical-dual-saws-whole-figures-light.png')
for index in [0,3,4,5]:
    f=frames[index];x,y,w,h=f['rect'];piece=im.crop((x,y,x+w,y+h));bg=Image.new('RGBA',piece.size,(215,215,215,255));bg.alpha_composite(piece)
    bg.resize((w*2,h*2),Image.Resampling.NEAREST).save(OUT/('canonical-whole-figure-%02d-2x.png'%index))
