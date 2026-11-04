from pathlib import Path
from PIL import Image
import numpy as np,json,sys,hashlib
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main');S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion';sys.path.insert(0,str(R/'Tools/AnimationPreview'));from whole_figure_components import identify
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def owned(p,rows,cols):
 im=Image.open(p).convert('RGBA');a=np.array(im);fs,o=identify(im,rows=rows,columns=cols,minimum_area=10000);yy,xx=np.where((a[:,:,3]>0)&(o==0));best=np.full(xx.shape,np.inf);assigned=np.zeros(xx.shape,dtype=np.uint8)
 for i,f in enumerate(fs):
  x0,y0,x1,y1=f['grid'];dx=np.maximum(np.maximum(x0-xx,xx-x1),0);dy=np.maximum(np.maximum(y0-yy,yy-y1),0);score=dx*dx+dy*dy+((xx-(x0+x1)/2)**2+(yy-(y0+y1)/2)**2)*.00001;c=score<best;assigned[c]=i+1;best[c]=score[c]
 o[yy,xx]=assigned;return a,fs,o
receipts=[]
for pair,rows,reps,fixrows,fixcols in [('NW_N_NE',3,{0:0,3:3},3,4),('E_SE',2,{1:1},2,4)]:
 stem='ReqKatanaNormalKatanaAerial'+pair+'20261102';original=S/(stem+'V1-source.png');fix=S/(stem+('V4' if pair=='NW_N_NE' else 'V2')+'-source.png');dest=S/(stem+'V5-source.png');a,fs,o=owned(original,rows,4);b,bfs,bo=owned(fix,fixrows,fixcols);tiles=[];items=[]
 for i in range(rows*4):
  data,owners,si,origin=(b,bo,reps[i],fix) if i in reps else (a,o,i,original)
  assert not (bfs[si] if i in reps else fs[si])['touchesExtractionEdge'];mask=(owners==si+1)&(data[:,:,3]>0);yy,xx=np.where(mask);box=[int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)];x0,y0,x1,y1=box;px=data[y0:y1,x0:x1].copy();px[~mask[y0:y1,x0:x1]]=0;tiles.append(Image.fromarray(px));items.append({'targetSourceIndex':i,'origin':origin.relative_to(R).as_posix(),'originComponentIndex':si,'originRect':box,'nonzeroPixels':int(mask.sum()),'rgbaNonzeroSha256':hashlib.sha256(data[mask].tobytes()).hexdigest(),'rgbaExact':True,'replaced':i in reps})
 cw=max(t.width for t in tiles)+32;ch=max(t.height for t in tiles)+32;out=Image.new('RGBA',(cw*4,ch*rows))
 for i,t in enumerate(tiles):
  x=i%4*cw+(cw-t.width)//2;y=i//4*ch+ch-t.height-16;out.paste(t,(x,y));items[i]['derivedRect']=[x,y,t.width,t.height];check=np.array(out.crop((x,y,x+t.width,y+t.height)));assert hashlib.sha256(check[check[:,:,3]>0].tobytes()).hexdigest()==items[i]['rgbaNonzeroSha256']
 out.save(dest);allfs,_=identify(out,rows=rows,columns=4,minimum_area=10000);assert not any(f['touchesExtractionEdge'] for f in allfs);receipts.append({'derivedSource':dest.relative_to(R).as_posix(),'sha256':sha(dest),'operation':'Register intact selected full figures on separate transparent cells using integer positions only; no resampling/anatomical assembly','originalSource':original.relative_to(R).as_posix(),'originalSha256':sha(original),'repairSource':fix.relative_to(R).as_posix(),'repairSha256':sha(fix),'preservedCompleteFigures':rows*4-len(reps),'replacedCompleteFigures':len(reps),'preservedEverySelectedNonzeroAlphaRgba':True,'frames':items})
(S/'katanaaerial-whole-figure-replacements.json').write_text(json.dumps({'receipts':receipts},indent=2),encoding='utf-8');print(json.dumps({'derivedSources':2,'correctedWholeFigures':3,'otherOriginalFiguresPreserved':17,'sourceResampling':False}))
