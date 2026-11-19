from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json
R=Path(__file__).resolve().parent
W=R.parent.parent
rel=lambda p:Path(p).relative_to(W).as_posix()
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
rgba=lambda c:hashlib.sha256(c.tobytes()).hexdigest()
def save(p,v):
 p.parent.mkdir(parents=True,exist_ok=True)
 p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
swp=R/'candidates/FabiusFistsCombat-SW-v1-source.png'
nnp=R/'candidates/FabiusFistsPair-N-NE-v2-source.png'
rpp=R/'candidates/SelectiveRepair-SW2-NE2-v1-source.png'
sw=Image.open(swp).convert('RGBA');nn=Image.open(nnp).convert('RGBA');rp=Image.open(rpp).convert('RGBA')
(R/'selected').mkdir(exist_ok=True)
swsel=sw.copy();swsel.paste(rp.crop((384,0,768,512)),(768,512))
sp=R/'selected/FabiusFistsCombatSW-selected-v1.png';swsel.save(sp)
swproof=[]
for i in range(8):
 x=i%4*384;y=i//4*512;c=swsel.crop((x,y,x+384,y+512));o=sw.crop((x,y,x+384,y+512))
 swproof.append({'index':i,'preservedOriginalRGBA':c.tobytes()==o.tobytes(),'selectedRgbaSha256':rgba(c),'sourcePath':rel(rpp if i==6 else swp),'sourceRect':[384,0,384,512] if i==6 else [x,y,384,512]})
save(R/'selected/SW-selection-proof.json',{'selectedSource':rel(sp),'sourceSha256':sha(sp),'preservedOriginalCount':sum(f['preservedOriginalRGBA'] for f in swproof),'frames':swproof})
xs=[round(i*1536/5) for i in range(6)]
combined=Image.new('RGBA',(1920,1024),(0,0,0,0));proof=[];frames=[]
for i in range(10):
 row=i//5;col=i%5;slotx=col*384;sloty=row*512
 if i in [6,9]:
  source=rpp;rect=[746,0,384,512] if i==6 else [1129,0,384,512];sx,sy,w,h=rect;c=rp.crop((sx,sy,sx+w,sy+h));dx,dy=slotx,sloty
 else:
  source=nnp;rect=[xs[col],row*512,xs[col+1]-xs[col],512];sx,sy,w,h=rect;c=nn.crop((sx,sy,sx+w,sy+h));dx,dy=slotx+(384-w)//2,sloty
 combined.paste(c,(dx,dy));a=c.getchannel('A');b=a.point(lambda v:255 if v>=128 else 0).getbbox()
 edges=[max(a.crop(bb).get_flattened_data()) for bb in [(0,0,1,h),(w-1,0,w,h),(0,0,w,1),(0,h-1,w,h)]]
 direction=['N','NE'][row];action='Idle' if col<2 else 'Walk';phase=col if col<2 else col-2
 ground=[slotx+(205 if row==0 else 192),sloty+(462 if row==0 else 471)]
 frames.append({'direction':direction,'action':action,'phase':phase,'rect':[dx,dy,w,h],'ground':ground,'durationMs':[450,100][col] if col<2 else 100})
 proof.append({'index':i,'direction':direction,'sourcePath':rel(source),'sourceRect':rect,'destinationRect':[dx,dy,w,h],'sourceRgbaSha256':rgba(c),'wholeFigureOnly':True,'resampled':False,'alpha128BoundsLocal':list(b),'edgeMaxAlpha':edges,'completeAlpha32':max(edges)<32,'sourcePreservedInSelected':combined.crop((dx,dy,dx+w,dy+h)).tobytes()==c.tobytes()})
np=R/'selected/FabiusFistsPairNNE-selected-v1.png';combined.save(np)
save(R/'selected/NNE-selection-proof.json',{'selectedSource':rel(np),'sourceSha256':sha(np),'sourceSize':[1920,1024],'actualVariableRects':True,'noResampleNoAnatomyAssembly':True,'frames':proof})
recipe=json.loads((R/'recipe.json').read_text())
recipe['sheets']=[s for s in recipe['sheets'] if s['name'] not in ['FistsCombatSW20261118','FistsCombatNNE20261118']]
swframes=[]
for i in range(7):
 x=i%4*384;y=i//4*512
 swframes.append({'direction':'SW','action':'Idle' if i<4 else 'Walk','phase':i if i<4 else i-4,'rect':[x,y,384,512],'ground':[x+175,y+443],'durationMs':[180,120,100,150][i] if i<4 else 100})
swSheet={'name':'FistsCombatSW20261118','sourcePath':rel(sp),'sourceSha256':sha(sp),'status':'visual-reviewed','wholeFigures':True,'bodyHeightPx':400,'bodyMeasurement':'Ready originalSW hair42 to sole442=400px; fixed projected floor443. Original aerial sole428 gives15px3.75%, accepted as natural small rebound after coordinator clarified1-3%approximate guide. No pose translation to manufacture lift.','frames':swframes,'clips':[{'direction':'SW','action':'Walk','sequence':[0,1,2,1],'durationMs':[100]*4}],'reviewNotes':'7/8source cells RGBA exact; only opposite contactB replaced with complete new drawing. Alternate passing cell7 stays intact inPNG, outside simplified3Walk playback. Projected ground under body, not each nearest-toe AABB; no runtime claim.'}
nnSheet={'name':'FistsCombatNNE20261118','sourcePath':rel(np),'sourceSha256':sha(np),'status':'visual-reviewed','wholeFigures':True,'bodyHeightPx':421,'bodyMeasurement':'Representative readyN hair41 to ground462=421px. NEready435px (+3.33%diagonal boot depth), onePPUforPNG. Nair451 vs462=11px2.61%; NEair465 vs471=6px1.38%. Horizontal whole-figure ROI registration aligns belt/root; noYshift/resampling.','frames':frames,'clips':[{'direction':d,'action':'Walk','sequence':[0,1,2,1],'durationMs':[100]*4} for d in ['N','NE']],'reviewNotes':'Eight correct whole source cells preserved, two NEcomplete figures selectively replaced. Alpha0 background, no body/face/armor redesign, opposite contacts and highguard static-reviewed; no Unity/continuous-browser claim.'}
recipe['sheets'].extend([swSheet,nnSheet]);save(R/'recipe.json',recipe)
for label,png,sheet in [('SW',swsel,swSheet),('NNE',combined,nnSheet)]:
 qa=R/'qa'/('Selected'+label);qa.mkdir(parents=True,exist_ok=True)
 for bg,name in [((255,255,255,255),'white'),((24,24,24,255),'dark')]:
  cv=Image.new('RGBA',png.size,bg);cv.alpha_composite(png);cv.convert('RGB').save(qa/f'{name}.png')
 for direction in sorted(set(f['direction'] for f in sheet['frames'])):
  for action in ['Idle','Walk']:
   fs=sorted([f for f in sheet['frames'] if f['direction']==direction and f['action']==action],key=lambda f:f['phase'])
   sequence=list(range(len(fs))) if action=='Idle' else [0,1,2,1];imgs=[];dur=[]
   for phase in sequence:
    f=fs[phase];x,y,w,h=f['rect'];gx,gy=f['ground'];c=png.crop((x,y,x+w,y+h));cv=Image.new('RGBA',(384,512),(145,145,145,255));cv.alpha_composite(c,(192-(gx-x),471-(gy-y)))
    d=ImageDraw.Draw(cv);d.line((10,471,374,471),fill=(240,240,240));d.text((10,10),f'{direction} {action} phase{phase}',fill=(15,15,15));imgs.append(cv.convert('RGB'));dur.append(f['durationMs'])
   imgs[0].save(qa/f'{direction}-{action.lower()}.gif',save_all=True,append_images=imgs[1:],duration=dur,loop=0,disposal=2)
print(json.dumps({'SWselectedSha256':sha(sp),'SWoriginalWholeFramesPreserved':sum(f['preservedOriginalRGBA'] for f in swproof),'NNEselectedSha256':sha(np),'NNEallWholeCellsExact':all(f['sourcePreservedInSelected'] for f in proof),'NNEallFiguresContained':all(f['completeAlpha32'] for f in proof),'recipeSha256':sha(R/'recipe.json'),'recipeDirections':['S','SW','N','NE']}))
