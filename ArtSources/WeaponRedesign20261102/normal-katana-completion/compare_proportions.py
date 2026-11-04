from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,sys
sys.path.insert(0,str(Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Tools/AnimationPreview')))
from whole_figure_components import identify
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main');S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion';O=R/'outputs/retomada-20261102/normal-katana-completion'
refs={'S':('ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png',0),'SW':('ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png',1),'W':('ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png',2),'NW':('ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png',0),'N':('ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png',1),'NE':('ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png',2)}
results=[]
for pair in [('S','SW'),('W','NW'),('N','NE')]:
 p=S/('ReqKatanaNormalCarregado'+'_'.join(pair)+'20261102V1-source.png');im=Image.open(p).convert('RGBA');w=im.width//4;h=im.height//2
 figures,_=identify(im,rows=2,columns=4,minimum_area=10000)
 out=Image.new('RGB',(4*950,2*560),(62,62,70));dr=ImageDraw.Draw(out)
 for row,d in enumerate(pair):
  rp,rr=refs[d];ref=Image.open(R/'ArtSources/WeaponRedesign20261025/candidates'/rp).convert('RGBA');rh=ref.height/3;rw=ref.width/4; referenceFigures,_=identify(ref,rows=3,columns=4,minimum_area=10000)
  refbox=referenceFigures[rr*4]["rect"];refcrop=ref.crop(refbox)
  for col in range(4):
   cell=im.crop(figures[row*4+col]["rect"]);x=col*950;y=row*560
   # Native complete figure cells, no scaling, anatomy transforms, masks or recoloring.
   out.paste(refcrop,(x+10,y+100),refcrop);out.paste(cell,(x+440,y+30),cell)
   dr.text((x+10,y+8),d+' '+str(col)+' | APPROVED IDLE (left) / CANDIDATE (right)',fill='white')
 dst=O/('proportion-contact-'+'_'.join(pair)+'-v1.png');out.save(dst)
 results.append({'source':str(p.relative_to(R)).replace('\\','/'),'sourceSha256':hashlib.sha256(p.read_bytes()).hexdigest(),'comparison':str(dst.relative_to(R)).replace('\\','/'),'nativePixelPreview':True,'wholeFigureCellsOnly':True,'affectedSourceFrameIndices':list(range(8)),'finding':'All candidate figures show longer exposed thigh/shin and large boot lengths relative to approved compact head/coat/hip proportions; W direction also opens too far toward front. Do not promote.'})
(O/'proportion-review-v1.json').write_text(json.dumps({'status':'rejected-proportions','producedSourceFigures':24,'affectedWholeFigures':24,'approvedPixelsChanged':0,'reviews':results,'next':'Redraw complete affected figures from approved matching-angle Katana refs; compact approved legs, no generic realistic adult anatomy; keep mature approved face.'},indent=2)+'\n')
print(json.dumps({'contacts':len(results),'affectedWholeFigures':24,'promoted':0}))

