from pathlib import Path
import sys,json,hashlib,argparse,base64,math
from collections import Counter
from PIL import Image,ImageDraw
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261103/moto-walk-close';O=R/'outputs/retomada-20261103/moto-walk-close'
sys.dont_write_bytecode=True;sys.path.insert(0,str(R/'Tools/AnimationPreview'))
import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'staging';p.OUT=O
D=['S','SW','W','NW','N','NE','E','SE']
def main():
 cfg=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'));name=cfg['name'];dirs=cfg['directions'];status='visual-reviewed' if cfg.get('visualReviewed') else 'candidate'
 job=dict(name=name,source=str(A/cfg['source']),preservedSourceName=cfg['source'],skin=0,weapon=3,form=0,columns=4,sourceRows=cfg['sourceRows'],minimumArea=5000,rows=[dict(sourceRow=i,sourceIndices=indices,direction=D.index(d),action='Walk',loop=True,durationMs=[120]*4) for i,(d,indices) in enumerate(zip(dirs,cfg['sourceIndices']))])
 if cfg.get('supportAnchors'):job['supportAnchors']=cfg['supportAnchors']
 m,pages=p.prepare(job);ppu=cfg.get('densityPixelsPerUnit') or [h/1.98 for h in cfg['frameBodyHeightPixels']];assert len(ppu)==len(m['frames'])
 phases=['contact_A','passing_A_low','contact_B_opposite','passing_B_low']
 m.update(status=status,presentation='OnFoot',worldVisualScale=1.325,motorcycleContext='onFoot-DualSaws',pixelsPerUnit=ppu[0],framePixelsPerUnit=ppu,pixelsPerUnitCalibration=dict(method='Whole standing crown-to-sole source density per compatible pair. Equipment excluded. No source resampling or anatomical resizing.',bodyHeightUnits=1.98,bodyHeightPixels=cfg['frameBodyHeightPixels'],uncertaintyPixels=5),review=dict(wholeSourceSeen=True,nearestWholeFiguresSeen=cfg.get('nearestWholeFiguresSeen',False),staticContactSeen=cfg.get('staticContactSeen',False),continuousMotionViewed=False,imported=False,runtimeVerified=False,humanApproved=False),reviewNotes=cfg.get('reviewNotes',[]),integrationBlocked=status!='visual-reviewed',phases=phases)
 if cfg.get('densityCalibration'):m['pixelsPerUnitCalibration']=cfg['densityCalibration']
 for i,f in enumerate(m['frames']):f.update(phase=phases[i%4],pixelsPerUnit=ppu[i],durationMs=120)
 for i,page in enumerate(pages):page.update(status=status,presentation='OnFoot',pixelsPerUnit=ppu[0],framePixelsPerUnit=ppu[i*4:(i+1)*4],durationMs=[120]*4,loop=True,integrationBlocked=status!='visual-reviewed')
 p.write(p.RES/(name+'.json'),m);p.write(O/(name+'-pages.json'),dict(schemaVersion=1,complete=False,status=status,integrationBlocked=status!='visual-reviewed',pages=pages));p.write(A/(name+'-job.json'),job)
 im=Image.open(p.RES/(name+'.png')).convert('RGBA');figs=[];pivots=[]
 for i,f in enumerate(m['frames']):
  x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));box=q.getbbox();q=q.crop(box);pi=(f['pivot'][0]*w-box[0],(1-f['pivot'][1])*h-box[1]);figs.append(q);pivots.append(pi)
  bg=Image.new('RGBA',q.size,(175,175,175,255));bg.alpha_composite(q);bg.resize((q.width*2,q.height*2),Image.Resampling.NEAREST).save(O/(name+f'-whole{i:02}-2x.png'))
 for label,target in [('native',None),('worldscale',150)]:
  qs=[];ps=[]
  for i,q in enumerate(figs):
   scale=1 if target is None else target*1.325/ppu[i];qs.append(q if scale==1 else q.resize((round(q.width*scale),round(q.height*scale)),Image.Resampling.NEAREST));ps.append(tuple(round(v*scale) for v in pivots[i]))
  left=max(pi[0] for pi in ps)+20;top=max(pi[1] for pi in ps)+32;right=max(q.width-pi[0] for q,pi in zip(qs,ps))+20;bottom=max(q.height-pi[1] for q,pi in zip(qs,ps))+10;cw=left+right;ch=top+bottom
  board=Image.new('RGBA',(cw*4,ch*len(dirs)),(160,160,160,255));draw=ImageDraw.Draw(board)
  for i,(q,pi) in enumerate(zip(qs,ps)):
   dx=(i%4)*cw+left-pi[0];dy=(i//4)*ch+top-pi[1];board.alpha_composite(q,(dx,dy));draw.text(((i%4)*cw+4,(i//4)*ch+3),dirs[i//4]+' '+phases[i%4],fill='black')
  board.save(O/(name+'-'+label+'-contact.png'))
 colors=Counter(c for c in im.get_flattened_data() if c[3]>0);p.write(O/(name+'-palette.json'),dict(imageSha256=m['imageSha256'],uniqueColorCount=len(colors),paletteQuantized=False,top256=[dict(rgba=list(c),count=n) for c,n in colors.most_common(256)]))
 receipt=dict(name=name,status=status,directions=dirs,figures=len(m['frames']),clips=len(pages),sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceSize=m['sourceSize'],nativeSize=m['nativeSize'],imageSha256=m['imageSha256'],descriptorSha256=p.sha(p.RES/(name+'.json')),fragmentSha256=p.sha(O/(name+'-pages.json')),phaseDurationMs=[120]*4,presentation='OnFoot',worldVisualScale=1.325,framePixelsPerUnit=ppu,sourceResampled=False,wholeFigureRGBAExact=True,continuousMotionViewed=False,testsRun=False,imported=False,runtimeVerified=False,assetsModified=False,unityExecuted=False,gitModified=False)
 p.write(O/(name+'-receipt.json'),receipt);print(json.dumps(receipt))
if __name__=='__main__':main()
