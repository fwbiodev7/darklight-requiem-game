from pathlib import Path
from PIL import Image
import json
O=Path('outputs/retomada-20261102/normal-katana-completion');tiles=[]
for n,indices in [('ReqKatanaNormalKatanaAerialNW_N_NE20261102V5',[0,3]),('ReqKatanaNormalKatanaAerialE_SE20261102V5',[1])]:
 m=json.loads((O/'staged'/(n+'.json')).read_text(encoding='utf-8-sig'));im=Image.open(O/'staged'/(n+'.png')).convert('RGBA')
 for i in indices:
  x,y,w,h=m['frames'][i]['rect'];tiles.append(im.crop((x,y,x+w,y+h)))
out=Image.new('RGB',(sum(t.width+12 for t in tiles),max(t.height for t in tiles)),(70,70,78));x=0
for t in tiles:out.paste(t,(x,0),t);x+=t.width+12
out.save(O/'katanaaerial-three-repairs-contact.png')
