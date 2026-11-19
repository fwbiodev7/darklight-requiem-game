"""Local art provenance/preview helper; never imports Unity or edits approved sources."""
from pathlib import Path
from PIL import Image, ImageDraw
import argparse, hashlib, json, shutil

ROOT = Path(__file__).resolve().parent
WORKSPACE = ROOT.parent.parent
S = WORKSPACE / 'ArtSources/FistsCombatStance20261117/selected/FabiusFistsCombatS-selected-v1.png'
PAR = WORKSPACE / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalParry8Dirs20261103V1.png'
CANON = WORKSPACE / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
VIEWS = {
 'SW': ('front-three-quarter LEFT, chest/front visible, nose toward lower-left, NOT side profile or back', 'ReqFistsNormalWalkSW20261103V24Closed'),
 'W': ('strict LEFT side profile, nose and toes to screen-left, NOT front-three-quarter', 'ReqFistsNormalWalkW20261102V9Complete'),
 'NW': ('back-three-quarter LEFT, back/coat visible, face mostly hidden with only a left-side sliver, toes upper-left', 'ReqFistsNormalWalkNW20261102NaturalV2Selected'),
 'N': ('straight BACK view, back of head and coat, both shoulders symmetric in perspective, NO face/eyes visible, toes away/up', 'ReqFistsNormalWalkN20261102NaturalV1'),
 'NE': ('back-three-quarter RIGHT, back/coat visible, face mostly hidden with only a right-side sliver, toes upper-right; never back-left', 'ReqFistsNormalWalkNE_SE20261102FeetFixedV3'),
 'E': ('strict RIGHT side profile, nose and toes to screen-right, NOT left profile or front-three-quarter', 'ReqFistsNormalWalkE20261102V11Complete'),
 'SE': ('front-three-quarter RIGHT, chest/front visible, nose toward lower-right; never front-left', 'ReqFistsNormalWalkNE_SE20261102FeetFixedV3'),
}
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def relative(p): return Path(p).relative_to(WORKSPACE).as_posix()
def save(p, data):
 p.parent.mkdir(parents=True, exist_ok=True)
 p.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

def prepare(direction):
 view, canon = VIEWS[direction]
 refs = [S, PAR, CANON/(canon+'.png')]
 prompt = f'''Use case: identity-preserve gameplay pixel-art animation. NEW additional sheet only; approved originals must never change.
Create ONE transparent PNG of exactly EIGHT COMPLETE adult Fabius Requiem Normal figures in direction {direction}: {view}. All eight are the SAME direction, not a turnaround. Draw each whole intact body including hair, face when visible, hands/gauntlets, coat and every toe/greave. Same slight top-down RPG camera as the refs. FOUR columns x TWO rows, requested1536x1024, one full body per384x512cell, transparent margins, no figure touches cell edges, no text/grid/floor/shadow.

Reference1 is the APPROVED SOUTH high-guard Walk and reviewed SOUTH bounce sheet. This is the primary authority for EXACT mature Fabius face, age, dark-red eyes/clean original skin, original canonical dark-brown hair cut and compact volume, compact ADULT gameplay proportions, black coat with burgundy inner tails, shoulder ornaments, chain, neutral silver gauntlets and clawed greaves, anatomy and armor size. Keep the SAME person and same physical scale; ONLY viewing angle and movement change. Do not reinterpret him as the miniature/chibi UI mascot, a younger boy, a new face, a larger muscular body, a longer hairstyle, bigger hands/boots, or redesigned armor.
Reference2 is the approved full8direction Parry sheet, supplied for actual {direction} orientation and armor/coat occlusion. Its crossed forearms are NOT the new stance. Reference3 is the approved complete old walking sheet for true {direction} orientation and real alternating steps, but NOT for low arms. Preserve asymmetric hair part and chain on their physical side; NEVER mirror a complete figure to create the opposite view. Do not transplant/assemble heads, limbs or clothing.

STANCE in ALL8figures: both fists held separately high beside the cheeks, elbows folded protecting ribs, chin slightly tucked, knees soft, compact trained unarmed-combat guard. Never cross forearms, swing/drop an arm below chest, or imitate a sword stance. In back/side views the far fist may be naturally occluded but both arms anatomically remain raised. Maintain exact reference face/age/head/body proportions and armor, no sword/aura/flames/transformation/energy.

Same body/head size in all cells, ready pose about386px hair-to-lowest-toe. Keep torso/hip projection horizontally centered at cellx192. Conceptual fixed floor y441 in EVERY cell, not drawn. Preserve real ground/pose relationships. Normal perspective depth can put the far foot slightly higher in the image.
TOP ROW four professional fighter preparatory bounce Idle phases, left-to-right:
0 ready high guard, softly bent knees, staggered feet supported on fixed ground;
1 compression through bent knees/ankles and relaxed hips, planted feet remain on SAME floor, upper body lowers subtly through joints, not canvas translation;
2 tiny rebound/push-off, knees gently UNCOIL, ankles extend down, both soles only5–9px above fixed floor (1–3% body height), not a high tucked-knee jump; overall body remains nearly full ready height. Draw real whole-body articulation/coat response, never shift/scale an identical pose;
3 soft landing, one forefoot on fixed floor, opposite heel light, knees absorb contact and exchange support smoothly back to0. No comic bounce, no giant hair/armor movement. Small motion, high guard constant. The ground pivot must remain constant across all four; visibly raised soles must NOT be pushed back down by reanchoring each frame.

BOTTOM ROW four genuinely distinct guarded walking phases, left-to-right. Maintain high fists throughout and SAME body size/camera. Move in actual {direction} orientation, not a differently facing torso with flat rotated feet.
0 first leg leads in CONTACT, opposite leg trails at depth;
1 that first leg now SUPPORTS under hip while the OTHER leg PASSES forward, knee/ankle visibly flexing, passing foot slightly lifted;
2 the OTHER leg now leads in opposite CONTACT, first leg trails;
3 the OTHER leg SUPPORTS under hip while first leg PASSES, opposite passing pose from1, then loops to0. The order is contactA/passingB/contactB/passingA. Both contacts must show different leading legs, both passing frames different supporting legs. NOT three copies of one leading foot, not marching, not just translating a still picture. Natural depth/occlusion per view, feet/coat complete. Shoulder balance subtle, guard never drops.

Pixel-art clarity and material ramps must match approved S, crisp clusters with no blur. Brown hair only; eyes when visible remain dark red (#C40814 / #5A0000), never blue/purple or bleeding into skin. Original warm skin, black/burgundy garment and neutral silver armor. No extra props, mini UI style, labels, watermark, painted checkerboard or background. Real transparent alpha including gutters. Whole characters only, no anatomy collage, no auto-mirroring asymmetric details, no flat rotation, no stretching or pose duplication disguised by bobbing.
'''
 pp = ROOT/'prompts'/f'FabiusFistsCombat-{direction}-v1.txt'
 pp.parent.mkdir(exist_ok=True); pp.write_text(prompt, encoding='utf-8')
 save(ROOT/'preflight'/f'{direction}.json', {'direction':direction,'references':[{'path':relative(p),'sha256':sha(p),'wholeImageInspected':True} for p in refs],'prompt':relative(pp),'promptSha256':sha(pp),'requestedGrid':[4,2],'requestedSize':[1536,1024],'sourceSProtectedSha256':sha(S),'scope':'new candidate only; no Assets/catalog/old pivots/Unity/Git changes'})
 print(json.dumps({'prompt':str(pp),'references':[str(p) for p in refs]}))

def record(direction, source, version):
 dst=ROOT/'candidates'/f'FabiusFistsCombat-{direction}-v{version}-source.png'
 dst.parent.mkdir(exist_ok=True); shutil.copy2(source,dst)
 im=Image.open(dst).convert('RGBA'); w,h=im.size
 if w%4 or h%2:
  save(ROOT/'review'/f'{direction}-v{version}.json',{'source':relative(dst),'sha256':sha(dst),'sourceSize':[w,h],'produced':True,'reviewed':False,'importReady':False,'gridFailure':'source not divisible4x2; inspect actual full figure rectangles'})
  print('Nonregular source; no assumed slicing.');return
 cw,ch=w//4,h//2;frames=[]
 qa=ROOT/'qa'/direction;qa.mkdir(parents=True,exist_ok=True)
 for i in range(8):
  x=i%4*cw;y=i//4*ch;c=im.crop((x,y,x+cw,y+ch));a=c.getchannel('A');bb=a.point(lambda v:255 if v>=128 else 0).getbbox()
  edge=[max(a.crop(b).get_flattened_data()) for b in [(0,0,1,ch),(cw-1,0,cw,ch),(0,0,cw,1),(0,ch-1,cw,ch)]]
  frames.append({'index':i,'action':'Idle' if i<4 else 'Walk','frame':i%4,'direction':direction,'rectTopLeft':[x,y,cw,ch],'rgbaDecodedSha256':hashlib.sha256(c.tobytes()).hexdigest(),'alpha128Bounds':list(bb) if bb else None,'bodySpanPx':bb[3]-bb[1] if bb else None,'edgeMaxAlpha':edge,'wholeFigureContainedAlpha32':max(edge)<32,'pivotTopLeftProposed':[cw//2,round(ch*441/512)],'pivotState':'nominal fixed floor; manual whole-figure review pending'})
 for bg,name in [((255,255,255,255),'white'),((24,24,24,255),'dark')]:
  cv=Image.new('RGBA',im.size,bg);cv.alpha_composite(im);cv.convert('RGB').save(qa/f'v{version}-{name}.png')
 for row,action,durations in [(0,'idle',[180,120,100,150]),(1,'walk',[100]*4)]:
  rendered=[]
  for col in range(4):
   c=im.crop((col*cw,row*ch,(col+1)*cw,(row+1)*ch));cv=Image.new('RGBA',(cw,ch),(145,145,145,255));cv.alpha_composite(c);d=ImageDraw.Draw(cv);gy=round(ch*441/512);d.line((10,gy,cw-10,gy),fill=(240,240,240));d.text((10,10),f'{direction} {action} {col}',fill=(15,15,15));rendered.append(cv.convert('RGB'))
  rendered[0].save(qa/f'v{version}-{action}.gif',save_all=True,append_images=rendered[1:],duration=durations,loop=0,disposal=2)
 a=im.getchannel('A');hist=a.histogram();total=w*h
 data={'direction':direction,'source':relative(dst),'sourceSha256':sha(dst),'sourceSize':[w,h],'grid':[4,2],'cellSize':[cw,ch],'nativePixelGrid':'unverified, source size not native-grid evidence','frames':frames,'alpha':{'extrema':list(a.getextrema()),'zeroFraction':hist[0]/total,'1to31Fraction':sum(hist[1:32])/total,'32to223Fraction':sum(hist[32:224])/total,'224PlusFraction':sum(hist[224:])/total},'timingsProposed':{'Idle':[180,120,100,150],'Walk':[100]*4},'groundContract':'Idle floor pivot remains fixed; never reanchor raised airborne soles','worldVisualScale':1.325,'bodyAuthoritySReadyPx':386,'states':{'produced':True,'visualReviewed':False,'selected':False,'importReady':False,'imported':False,'runtimeVerified':False,'userVisualApproval':False},'preview':{'directory':relative(qa),'nearest':True,'continuousPlaybackViewed':False,'reason':'static frame inspection only unless explicitly updated; no runtime/Unity','gifQuantization':'PNG/RGBA hashes authoritative'},'sourceSUnchanged':sha(S)=='202f33f42d937ab55cc5cc2162b41c8d1881ea3ea329e8d18e04c895de70740e'}
 save(ROOT/'review'/f'{direction}-v{version}.json',data)
 print(json.dumps({'direction':direction,'source':relative(dst),'sha256':sha(dst),'size':[w,h],'bodySpans':[f['bodySpanPx'] for f in frames],'soleBounds':[f['alpha128Bounds'][3] for f in frames],'hairTop':[f['alpha128Bounds'][1] for f in frames],'allCellsContained':all(f['wholeFigureContainedAlpha32'] for f in frames)}))

if __name__=='__main__':
 ap=argparse.ArgumentParser();ap.add_argument('mode',choices=['prepare','record']);ap.add_argument('direction',choices=list(VIEWS));ap.add_argument('--source');ap.add_argument('--version',type=int,default=1);args=ap.parse_args()
 if args.mode=='prepare':prepare(args.direction)
 else:record(args.direction,args.source,args.version)
