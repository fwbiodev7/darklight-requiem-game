from pathlib import Path
import json,sys,hashlib,argparse
from collections import Counter
from PIL import Image,ImageDraw
sys.dont_write_bytecode=True
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261102/normal-moto-completion';O=R/'outputs/retomada-20261102/normal-moto-completion'
sys.path.insert(0,str(R/'Tools/AnimationPreview'))
import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'staging';p.OUT=O
def sha(q):return hashlib.sha256(q.read_bytes()).hexdigest()
def main():
 cfg=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'));n=cfg['name'];dirs=['S','SW','W','NW','N','NE','E','SE'];rows=[]
 for i,d in enumerate(cfg['directions']):
  row=dict(sourceRow=i,direction=dirs.index(d),action=cfg.get('action','Attack'),attackKey=cfg.get('attackKey'),loop=cfg.get('loop',False),durationMs=cfg['durationMs'])
  if cfg.get('sourceIndices'):row['sourceIndices']=cfg['sourceIndices'][i]
  rows.append(row)
 job=dict(name=n,source=str(A/cfg['source']),preservedSourceName=cfg['source'],skin=0,weapon=3,form=0,columns=cfg.get('columns',4),sourceRows=cfg.get('sourceRows',len(rows)),minimumArea=5000,rows=rows)
 if 'supportAnchors' in cfg:job['supportAnchors']=cfg['supportAnchors']
 m,pages=p.prepare(job);assert max(m['nativeSize'])<=4096
 body=cfg['bodyMeasurements'];ppu=sum(v['bodyHeightPixels'] for v in body)/len(body)/1.98
 status='visual-reviewed' if cfg.get('visualReviewed') else 'candidate';presentation=cfg.get('presentation','OnFoot')
 m.update(status=status,presentation=presentation,motorcycleContext='onFoot-DualSaws' if presentation=='OnFoot' else 'mounted',worldVisualScale=1.325,pixelsPerUnit=ppu,pixelsPerUnitCalibration=dict(bodyHeightUnits=1.98,method='Native crown-to-sole density from indicated recovered/standing full figures; saws excluded; action foreshortening not normalized away',measures=body,uncertaintyPixels=5),review=dict(wholeSourceSeen=cfg.get('wholeSourceSeen',False),nearestWholeFiguresSeen=cfg.get('nearestWholeFiguresSeen',False),staticContactSeen=cfg.get('staticContactSeen',False),continuousMotionViewed=False,humanApproved=False,imported=False,runtimeVerified=False),deferredValidation='User requested ALL TESTS AFTER package implementation; no audits, automated tests, Unity or motion playback executed',authoredPhases=dict(startupFrames=cfg.get('startupFrames',0),activeFrames=cfg.get('activeFrames',0),recoveryFrames=cfg.get('columns',4)-cfg.get('startupFrames',0)-cfg.get('activeFrames',0),runtimeTimingsMs=cfg.get('runtimeTimingsMs'),eventsChanged=False))
 if cfg.get('framePixelsPerUnit'):m['framePixelsPerUnit']=cfg['framePixelsPerUnit']
 if cfg.get('sourceFigureProvenance'):m['sourceFigureProvenance']=cfg['sourceFigureProvenance']
 for i,f in enumerate(m['frames']):
  k=i%cfg.get('columns',4);f['durationMs']=cfg['durationMs'][k];f['phase']=cfg['phaseNames'][k]
  if cfg.get('framePixelsPerUnit'):f['pixelsPerUnit']=cfg['framePixelsPerUnit'][i]
 for pageIndex,page in enumerate(pages):
  if cfg.get('framePixelsPerUnit'):page['framePixelsPerUnit']=cfg['framePixelsPerUnit'][pageIndex*cfg.get('columns',4):(pageIndex+1)*cfg.get('columns',4)]
  page.update(status=status,presentation=presentation,pixelsPerUnit=ppu,loop=cfg.get('loop',False),durationMs=cfg['durationMs'])
  if cfg.get('attackKey'):page.update(startupFrames=cfg['startupFrames'],activeFrames=cfg['activeFrames'])
 p.write(p.RES/(n+'.json'),m);p.write(O/(n+'-pages.json'),dict(schemaVersion=1,complete=False,pages=pages));p.write(A/(n+'-job.json'),job)
 image=Image.open(p.RES/(n+'.png')).convert('RGBA');colors=Counter(c for c in image.get_flattened_data() if c[3]>0)
 p.write(O/(n+'-palette.json'),dict(imageSha256=m['imageSha256'],uniqueColorCount=len(colors),paletteQuantized=False,top256=[dict(rgba=list(c),count=v) for c,v in colors.most_common(256)]))
 for i,f in enumerate(m['frames']):
  x,y,w,h=f['rect'];q=image.crop((x,y,x+w,y+h));bbox=q.getbbox();q=q.crop(bbox);bg=Image.new('RGBA',q.size,(170,170,170,255));bg.alpha_composite(q);bg.resize((q.width*2,q.height*2),Image.Resampling.NEAREST).save(O/(n+f'-whole{i:02}-2x.png'))
 bg=Image.new('RGBA',image.size,(170,170,170,255));bg.alpha_composite(image);bg.save(O/(n+'-native-contact.png'))
 receipt=dict(name=n,status=status,directions=cfg['directions'],figures=len(m['frames']),clips=len(pages),presentation=presentation,pixelsPerUnit=ppu,sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceSize=m['sourceSize'],nativeSize=m['nativeSize'],imageSha256=m['imageSha256'],manifestSha256=sha(p.RES/(n+'.json')),pagesSha256=sha(O/(n+'-pages.json')),phaseDurationMs=cfg['durationMs'],wholeSourcePixelsPreserved=True,testsRun=False,continuousMotionViewed=False,imported=False,runtimeVerified=False,assetsModified=False,runtimeModified=False,unityExecuted=False,gitModified=False)
 p.write(O/(n+'-receipt.json'),receipt);print(json.dumps(receipt))
if __name__=='__main__':main()

