"""Isolated Moto Normal whole-figure staging; never edits Assets/Runtime/catalog."""
from pathlib import Path
from collections import Counter
import argparse, hashlib, json, sys, subprocess
from PIL import Image
ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261102/normal-moto-completion'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline
pipeline.ART=ART;pipeline.RES=OUT/'staging';pipeline.OUT=OUT/'qa'
DIRS=['S','SW','W','NW','N','NE','E','SE']
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,v): pipeline.write(p,v)
def main():
 ap=argparse.ArgumentParser();ap.add_argument('config',type=Path);args=ap.parse_args()
 cfg=json.loads(args.config.read_text(encoding='utf-8-sig')); name=cfg['name']
 assert len(name)<150 and name.replace('_','').isalnum()
 source=(ART/cfg['source']).resolve();assert source.is_relative_to(ART.resolve())
 action=cfg['action']; rows=[]
 for i,d in enumerate(cfg['directions']):
  row=dict(sourceRow=i,direction=DIRS.index(d),action=action,loop=cfg.get('loop',action in ['Walk','Run','Idle','Charging']))
  if cfg.get('sourceIndices'):row['sourceIndices']=cfg['sourceIndices'][i]
  if cfg.get('durationMs'):row['durationMs']=cfg['durationMs']
  if cfg.get('attackKey'):row['attackKey']=cfg['attackKey']
  rows.append(row)
 job=dict(name=name,source=str(source),preservedSourceName=source.name,skin=0,weapon=3,form=0,columns=cfg.get('columns',4),sourceRows=cfg.get('sourceRows',len(rows)),minimumArea=5000,rows=rows)
 if cfg.get('supportAnchors'):job['supportAnchors']=cfg['supportAnchors']
 manifest,pages=pipeline.prepare(job)
 if max(manifest['nativeSize'])>4096:raise ValueError('Pack exceeds4096; preserve candidate and repack integer coordinates only')
 status='visual-reviewed' if cfg.get('visualReviewed',False) else 'candidate'
 measures=cfg.get('bodyMeasurements',[])
 if not measures:raise ValueError('Native body-only PPU measurement required; do not normalize by saw reach')
 ppu=sum(m['bodyHeightPixels'] for m in measures)/len(measures)/1.98
 manifest.update(status=status,pixelsPerUnit=ppu,worldVisualScale=1.325,pixelsPerUnitCalibration=dict(method='Manually inspected crown-to-sole native body; equipment excluded',measures=measures,bodyHeightUnits=1.98,worldVisualScale=1.325),motorcycleContext=cfg.get('context','onFoot-DualSaws'),referenceInventory='outputs/retomada-20261102/normal-moto-completion/inventory-pre-gate.json',review=cfg.get('review',dict(wholeSourceSeen=False,nativeContactSeen=False,nearestWholeFiguresSeen=False,continuousMotionViewed=False,imported=False,runtimeVerified=False)))
 frames=manifest['frames']
 count=len(frames)//len(rows)
 for page in pages:
  page.update(status=status,pixelsPerUnit=ppu)
  if action=='Attack':
   page.update(startupFrames=cfg['startupFrames'],activeFrames=cfg['activeFrames'],durationMs=cfg['durationMs'])
   assert 0<page['startupFrames'] and 0<page['activeFrames'] and page['startupFrames']+page['activeFrames']<len(page['spriteNames'])
 for i,f in enumerate(frames):
  phase=i%count
  if cfg.get('durationMs'):f['durationMs']=cfg['durationMs'][phase]
  if cfg.get('phaseNames'):f['phase']=cfg['phaseNames'][phase]
  if cfg.get('attackKey'):f['attackKey']=cfg['attackKey']
 manifest['clips']=[dict(name=f'{action}_{d}',frames=[f['id'] for f in frames[i*count:(i+1)*count]],loop=rows[i]['loop'],allowStatic=False) for i,d in enumerate(cfg['directions'])]
 manifest['requiredClips']=[c['name'] for c in manifest['clips']]
 if cfg.get('attackKey'):manifest['authoredPhases']=dict(startupFrames=cfg['startupFrames'],activeFrames=cfg['activeFrames'],recoveryFrames=count-cfg['startupFrames']-cfg['activeFrames'],runtimeTimingsMs=cfg['runtimeTimingsMs'],eventsChanged=False)
 png=pipeline.RES/(name+'.png'); meta=png.with_suffix('.json')
 write(meta,manifest);write(OUT/(name+'-pages.json'),dict(schemaVersion=1,complete=False,pages=pages));write(ART/(name+'-job.json'),job)
 rgba=Image.open(png).convert('RGBA');colors=Counter(c for c in rgba.getdata() if c[3]>0);ordered=sorted(colors.items())
 paletteHash=hashlib.sha256(json.dumps(ordered,separators=(',',':')).encode()).hexdigest()
 write(OUT/(name+'-palette.json'),dict(imageSha256=sha(png),uniqueColorCount=len(colors),fullColorTableSha256=paletteHash,top256=[dict(rgba=list(c),count=n) for c,n in colors.most_common(256)],paletteQuantized=False))
 folder=OUT/'qa'/name;folder.mkdir(parents=True,exist_ok=True)
 for mode in ['audit','preview']:
  dest=folder/mode
  args2=[sys.executable,str(pipeline.PIPE),mode,'--manifest',str(meta),'--out',str(dest)]
  if mode=='preview':args2+=['--zoom','2']
  run=subprocess.run(args2,capture_output=True,text=True,encoding='utf-8',errors='replace')
  (folder/(mode+'.log')).write_text(run.stdout+run.stderr,encoding='utf-8')
  if run.returncode:raise RuntimeError(f'{mode} exit{run.returncode}; see local log')
 report=json.loads((folder/'audit/report.json').read_text(encoding='utf-8'))
 receipt=dict(name=name,status=status,completeActionEightDirections=False,directions=cfg['directions'],figures=len(frames),clips=len(manifest['clips']),nativeSize=manifest['nativeSize'],sourceSize=manifest['sourceSize'],pixelsPerUnit=ppu,sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],manifest=meta.relative_to(ROOT).as_posix(),pages=(OUT/(name+'-pages.json')).relative_to(ROOT).as_posix(),qa=dict(errors=len(report['errors']),warnings=len(report['warnings'])),imported=False,runtimeVerified=False,assetsModified=False,runtimeModified=False,unityExecuted=False,gitModified=False)
 write(OUT/(name+'-receipt.json'),receipt);print(json.dumps(receipt))
if __name__=='__main__':main()
