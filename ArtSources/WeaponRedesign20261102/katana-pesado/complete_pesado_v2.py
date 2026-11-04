"""Finish authorized two-whole-figure repair and eight E/SE phases; scoped outputs only."""
from pathlib import Path
import json,hashlib,shutil,copy,sys
from PIL import Image
ROOT=Path(__file__).resolve().parents[3]
ART=ROOT/'ArtSources/WeaponRedesign20261102/katana-pesado'
OUT=ROOT/'outputs/retomada-20261102/katana-pesado'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
sys.path.insert(0,str(ROOT/'Tools/Skills/pixel-art-sprite-sheet-master/scripts'))
import prepare_weapon_redesign as pipe
import replace_frames as replacement
D=[100,110,100,100]
PH=['anticipation','active-impact','follow-through','recovery']
FINAL=OUT/'staged'
V2=OUT/'v2'
def write(p,x):pipe.write(p,x)
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def crop(im,f):
 x,y,w,h=f['rect'];return im.crop((x,y,x+w,y+h))
def framedata(m,ppus):
 m.update(status='candidate',staticVisualReviewed=True,visualReviewed=False,motionPreviewReviewed=False,imported=False,runtimeVerified=False,
          attackKey='Pesado',stagingOnly=True,worldVisualScale=1.325,startupFrames=1,activeFrames=1,durationMs=D,framePixelsPerUnit=ppus)
 for i,f in enumerate(m['frames']):
  f.update(durationMs=D[i%4],phase=PH[i%4],attackKey='Pesado',gameplayPhase=['startup','active','recovery','recovery'][i%4])
 return m
def pages_for(m,name,directions,ppus):
 pages=[]
 for row,d in enumerate(directions):
  group=m['frames'][row*4:row*4+4]
  pages.append(dict(id=name+'_Attack_'+pipe.DIRECTIONS[d],skin=0,weapon=0,form=0,direction=d,action='Attack',attackKey='Pesado',
  resourcePath='Darklight/WeaponRedesign20261023/'+name,imageSha256=m['imageSha256'],sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],
  sourceSize=m['sourceSize'],nativeSize=m['nativeSize'],pixelsPerUnit=m['pixelsPerUnit'],framePixelsPerUnit=ppus[row*4:row*4+4],
  status='candidate',staticVisualReviewed=True,visualReviewed=False,motionPreviewReviewed=False,wholeFigures=True,loop=False,
  spriteNames=[f['id'] for f in group],sourceSpriteNames=[],durationMs=D,startupFrames=1,activeFrames=1,
  stagingOnly=True,imported=False,runtimeVerified=False))
 return pages
def main():
 oldfront=FINAL/'ReqKatanaNormalPesadoS_SW_W20261102V1.png'
 oldrear=FINAL/'ReqKatanaNormalPesadoNW_NE20261102V1.png'
 protected={p:pipe.sha(p) for p in [oldfront,oldrear,ART/'ReqKatanaNormalPesadoNW_N_NE20261102V1-source.png']}
 oldjob=read(ART/'import-jobs-pesado.json')['jobs'][1]
 supports={**oldjob['supportAnchors'],**{str(i):dict(anchor=a,method='Manual grounded two-boot support midpoint in intact source.') for i,a in zip([4,5,6,7],[[183,674],[603,675],[972,676],[1346,675]])}}
 basejob=copy.deepcopy(oldjob)
 basejob.update(name='ReqKatanaPesadoRearOriginal12Baseline20261102',supportAnchors=supports,excludedReason='')
 basejob['rows']=[dict(sourceRow=i,sourceIndices=list(range(i*4,i*4+4)),direction=d,action='Attack',attackKey='Pesado',loop=False,durationMs=D) for i,d in enumerate([3,4,5])]
 pipe.ART=ART;pipe.RES=V2/'original-baseline'
 base,_=pipe.prepare(basejob)
 pipe.RES=V2/'repair-input'
 repairjob=dict(name='ReqKatanaPesadoTwoWholeRepairs20261102V2',source=str(ART/'ReqKatanaPesadoTwoWholeRepairs20261102V2-source.png'),
  preservedSourceName='ReqKatanaPesadoTwoWholeRepairs20261102V2-source.png',skin=0,weapon=0,form=0,columns=1,sourceRows=2,minimumArea=10000,
  rows=[dict(sourceRow=i,direction=d,action='Attack',attackKey='Pesado',durationMs=[110],loop=False) for i,d in enumerate([4,5])],
  supportAnchors={str(i):dict(anchor=a,method='Manual projected boot-support midpoint in intact repaired source.') for i,a in enumerate([[660,581],[688,991]])})
 rep,_=pipe.prepare(repairjob)
 bi=Image.open(V2/'original-baseline'/base['image']).convert('RGBA')
 ri=Image.open(V2/'repair-input'/rep['image']).convert('RGBA')
 oldw,oldh=base['frames'][0]['rect'][2:]
 mapping=[(5,0),(9,1)]
 offsets={}
 minx=miny=0;maxx=oldw;maxy=oldh
 for oldindex,newindex in mapping:
  bf=base['frames'][oldindex];rf=rep['frames'][newindex]
  rw,rh=rf['rect'][2:]
  bx,by=round(bf['pivot'][0]*oldw),round((1-bf['pivot'][1])*oldh)
  rx,ry=round(rf['pivot'][0]*rw),round((1-rf['pivot'][1])*rh)
  dx,dy=bx-rx,by-ry
  offsets[oldindex]=(newindex,dx,dy)
  minx=min(minx,dx);miny=min(miny,dy);maxx=max(maxx,dx+rw);maxy=max(maxy,dy+rh)
 padx,pady=-minx,-miny
 cw,ch=maxx-minx,maxy-miny
 expanded=Image.new('RGBA',(cw*4,ch*3))
 bm=copy.deepcopy(base)
 baseline_proof=[]
 for i,f in enumerate(bm['frames']):
  original=crop(bi,base['frames'][i])
  cx,cy=i%4*cw,i//4*ch
  expanded.paste(original,(cx+padx,cy+pady))
  f['rect']=[cx,cy,cw,ch]
  oldpivot=base['frames'][i]['pivot']
  f['pivot']=[(oldpivot[0]*oldw+padx)/cw,1-((1-oldpivot[1])*oldh+pady)/ch]
  baseline_proof.append(dict(index=i,id=f['id'],originalRect=base['frames'][i]['rect'],newRect=[cx+padx,cy+pady,oldw,oldh],
   rgbaSha256=hashlib.sha256(original.tobytes()).hexdigest()))
 expandedpath=V2/'expanded-baseline'
 expandedpath.mkdir(parents=True,exist_ok=True)
 expanded.save(expandedpath/'baseline.png')
 bm.update(image='baseline.png',imageSha256=pipe.sha(expandedpath/'baseline.png'),nativeSize=list(expanded.size))
 write(expandedpath/'baseline.json',bm)
 entries=[]
 for oldindex,(newindex,dx,dy) in offsets.items():
  replacement_canvas=Image.new('RGBA',(cw,ch))
  replacement_canvas.paste(crop(ri,rep['frames'][newindex]),(dx+padx,dy+pady))
  rel='replacement-'+str(oldindex)+'.png'
  replacement_canvas.save(expandedpath/rel)
  entries.append(dict(frameId=bm['frames'][oldindex]['id'],image=rel))
 write(expandedpath/'plan.json',dict(wholeFramesConfirmed=True,replacements=entries))
 replacement.replace(expandedpath/'baseline.json',expandedpath/'plan.json',V2/'whole-frame-replacement')
 corrected=read(V2/'whole-frame-replacement/sheet.json')
 ci=Image.open(V2/'whole-frame-replacement/sheet.png').convert('RGBA')
 unchanged=[]
 for p in baseline_proof:
  if p['index'] in offsets:continue
  x,y,w,h=p['newRect']
  actual=ci.crop((x,y,x+w,y+h)).tobytes()
  assert hashlib.sha256(actual).hexdigest()==p['rgbaSha256']
  unchanged.append(p)
 assert all(pipe.sha(p)==value for p,value in protected.items())
 name='ReqKatanaNormalPesadoNW_N_NE20261102V2'
 source=ART/(name+'-source.png')
 shutil.copy2(V2/'whole-frame-replacement/sheet.png',source)
 shutil.copy2(source,FINAL/(name+'.png'))
 oldppu=read(FINAL/'ReqKatanaNormalPesadoNW_NE20261102V1.json')['pixelsPerUnit']
 nppu=271/1.98
 ppus=[oldppu]*4+[nppu,nppu*330/244,nppu,nppu]+[oldppu,oldppu*336/262,oldppu,oldppu]
 corrected.update(image=name+'.png',imageSha256=pipe.sha(FINAL/(name+'.png')),pixelsPerUnit=oldppu,
  sourcePath=source.relative_to(ROOT).as_posix(),sourceSha256=pipe.sha(source),sourceSize=list(ci.size),
  originalSources=[dict(path=p.relative_to(ROOT).as_posix(),sha256=pipe.sha(p)) for p in [ART/'ReqKatanaNormalPesadoNW_N_NE20261102V1-source.png',ART/'ReqKatanaPesadoTwoWholeRepairs20261102V2-source.png']],
  sourceRgbaExactReconstruction=True,anatomyEdits=False,sourceScale=1,
  replacedSourceFrameIndices=[5,9],retainedSourceFrameIndices=[i for i in range(12) if i not in [5,9]],
  reviewFindings=['User screenshot maps N anticipation/impact and NE anticipation/impact, not N four-phase clip.',
  'Only N source5 and NE source9 replaced as complete figures: double blade removed; firm attached right wrist/grip.',
  'Other ten rear figures copied byte-exact with integer padding, front twelve atlas unchanged.',
  'User explicitly requested original N phases retained; prior differentiation-only rejection is superseded.',
  'Repaired native figures were not resized. Per-frame PPU uses intact same-pose hair-to-sole ratio, excludes blade.',
  'Static contact checked; motion preview remains blocked by browser URL policy; no Unity tests.'],
  ppuProvenance=dict(method='Preserve prior PPU for every untouched frame; repaired body same-pose hair-to-sole calibration, no bitmap resizing.',
  measurements=[dict(sourceFrameIndex=5,oldHairY=431,oldSoleY=675,oldBody=244,newHairY=251,newSoleY=581,newBody=330,ppu=ppus[5]),
                dict(sourceFrameIndex=9,oldHairY=726,oldSoleY=988,oldBody=262,newHairY=655,newSoleY=991,newBody=336,ppu=ppus[9])]))
 framedata(corrected,ppus)
 write(FINAL/(name+'.json'),corrected)
 rear_pages=pages_for(corrected,name,[3,4,5],ppus)
 write(FINAL/(name+'.pages.json'),dict(schemaVersion=1,stagingOnly=True,pages=rear_pages))
 proof=dict(ok=True,wholeFiguresOnly=True,replacedFrameIds=[bm['frames'][i]['id'] for i in [5,9]],
  screenshotMapping=dict(topLeftSourceIndex=4,topRightSourceIndex=5,bottomLeftSourceIndex=8,bottomRightSourceIndex=9),
  sourceSha256=protected[ART/'ReqKatanaNormalPesadoNW_N_NE20261102V1-source.png'],
  originalFrontAtlasSha256=protected[oldfront],originalRearAtlasSha256=protected[oldrear],
  originalFilesUnchanged=True,unchangedRearFrames=unchanged,unchangedFrontFigures=12,unchangedOriginalFigures=22,
  note='Exact RGBA equality on original complete-frame canvas region after integer padding; no anatomical edits or resampling.',
  helperProof='v2/whole-frame-replacement/replacement-proof.json')
 write(OUT/'preservation-proof-v2.json',proof)
 eastname='ReqKatanaNormalPesadoE_SE20261102V1'
 pipe.RES=FINAL;pipe.OUT=OUT/'qa-v2'
 eastjob=dict(name=eastname,source=str(ART/(eastname+'-source.png')),preservedSourceName=eastname+'-source.png',
 skin=0,weapon=0,form=0,columns=4,sourceRows=2,minimumArea=10000,
 rows=[dict(sourceRow=i,direction=d,action='Attack',attackKey='Pesado',durationMs=D,loop=False,startupFrames=1,activeFrames=1) for i,d in enumerate([6,7])],
 supportAnchors={str(i):dict(anchor=a,method='Manual projected grounded two-boot midpoint, intact full source.') for i,a in enumerate([[302,427],[726,427],[1172,425],[1526,435],[274,830],[722,835],[1133,833],[1528,839]])})
 east,_=pipe.prepare(eastjob)
 east.update(pixelsPerUnit=359/1.98,reviewFindings=['Eight complete E/SE figures inspected; broad transverse heavy arc, distinct four phases, stable right grip, whole single katana.',
 'Source body hair-to-sole at recovery E=354px, SE=364px; weapon excluded; original raster untouched.'])
 eastppus=[east['pixelsPerUnit']]*8
 framedata(east,eastppus)
 write(FINAL/(eastname+'.json'),east)
 eastpages=pages_for(east,eastname,[6,7],eastppus)
 write(FINAL/(eastname+'.pages.json'),dict(schemaVersion=1,stagingOnly=True,pages=eastpages))
 qas=[pipe.qa(name),pipe.qa(eastname)]
 frontpages=read(FINAL/'ReqKatanaNormalPesadoS_SW_W20261102V1.pages.json')['pages']
 allpages=frontpages+rear_pages+eastpages
 write(OUT/'pages-fragment-v2.json',dict(schemaVersion=1,status='candidate',stagingOnly=True,pages=allpages,
   note='Supersedes prior partial fragment. Exactly eight Pesado clips; preserve candidate/motion-pending states.'))
 rearjob=dict(name=name,source=str(source),preservedSourceName=source.name,skin=0,weapon=0,form=0,columns=4,sourceRows=3,minimumArea=10000,
  rows=[dict(sourceRow=i,direction=d,action='Attack',attackKey='Pesado',durationMs=D,loop=False,startupFrames=1,activeFrames=1,framePixelsPerUnit=ppus[i*4:i*4+4]) for i,d in enumerate([3,4,5])],
  pixelsPerUnit=oldppu,framePixelsPerUnit=ppus,
  supportAnchors={str(i):dict(anchor=[round(f['rect'][0]+f['pivot'][0]*cw),round(f['rect'][1]+(1-f['pivot'][1])*ch)],method='Exact packed whole-frame support anchor.') for i,f in enumerate(corrected['frames'])})
 frontjob=read(ART/'import-jobs-pesado.json')['jobs'][0]
 write(ART/'import-jobs-pesado-v2.json',dict(schemaVersion=1,stagingOnly=True,manifestsAuthoritative=True,jobs=[frontjob,rearjob,eastjob],
 note='Manifests/pages preserve per-frame PPU. Preparing source again requires restoring these explicit phase counts and densities.'))
 cp=dict(date='2026-11-02',status='candidate',staticVisualReviewed=True,visualReviewed=False,motionPreviewReviewed=False,
  generatedSheetsTotal=4,generatedFiguresTotal=34,selectedWholeFigures=32,selectedClips=8,directions=pipe.DIRECTIONS,
  correctedOnlySourceIndices=[5,9],preservedOriginalFigures=22,newEastFigures=8,
  framesPerClip=4,durationMs=D,startupFrames=1,activeFrames=1,recoveryFrames=2,totalMs=410,
  imported=False,runtimeVerified=False,stagingOnly=True,qa=qas,
  priorScopeSuperseded='User explicitly requested repair only the two screenshot right-hand figures, then continue E/SE; no regeneration of other phases.',
  pending=['Motion-preview review blocked earlier by uncertain Windows browser URL; no further Computer Use attempted.','Unity import and gameplay contact/transition tests not run.'],
  defectReview='N5 double blade removed; NE9 connected wrist and firm handle. No other original figures redrawn.',
  limits='Eight-direction Pesado candidate coverage only. Does not complete all Fabius actions or forms.')
 write(OUT/'checkpoint-v2.json',cp)
 print(json.dumps(dict(selected=32,clips=8,replaced=2,preserved=22,newEast=8,errors=sum(q['errors'] for q in qas),warnings=sum(q['warnings'] for q in qas),status='candidate',
  newSourceHashes={p.name:pipe.sha(p) for p in [ART/'ReqKatanaPesadoTwoWholeRepairs20261102V2-source.png',ART/(eastname+'-source.png')]},
  proofSha256=pipe.sha(OUT/'preservation-proof-v2.json'))))
if __name__=='__main__':main()

