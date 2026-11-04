import json, hashlib, re
from pathlib import Path
from collections import Counter
from PIL import Image
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main')
S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion'
O=R/'outputs/retomada-20261102/normal-katana-completion'
S.mkdir(parents=True,exist_ok=True); O.mkdir(parents=True,exist_ok=True)
def read(p):return json.loads((R/p).read_text(encoding='utf-8-sig'))
def save(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def sha(p):return hashlib.sha256((R/p).read_bytes()).hexdigest()
dirs=['S','SW','W','NW','N','NE','E','SE']
consumer=['Assets/Darklight/Runtime/Combat/DarklightCombatProfiles.cs','Assets/Darklight/Runtime/Combat/AttackDefinition.cs','Assets/Darklight/Runtime/Combat/CombatModel.cs','Assets/Darklight/Runtime/Character/DarklightAirCombo.cs','Assets/Darklight/Runtime/Character/DarklightLivingPresentation.cs','Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs','Assets/Darklight/Data/Weapons/Requiem.asset','Docs/Darklight/ESCOPO-CANONICO-20261026.md']
refs=['ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png','ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png','ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleE_SE20261025-source.png','ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png']
refrecords=[]; palette=Counter()
for p in refs:
 im=Image.open(R/p).convert('RGBA');palette.update(px for px in im.getdata() if px[3]);refrecords.append({'path':p,'sha256':sha(p),'sourceSize':list(im.size),'nativeRasterSize':list(im.size),'logicalPixelGrid':'unverified','inspectedWholeImage':True})
mat=read('outputs/retomada-20261101/normal-gap-matrix.json')['loadouts']['katana']
keys=['Carregado','ContraAtaque','OndaDoVeu','KatanaAerial','Corte do Silencio','Ruptura Precisa','Corte do Crepúsculo','Eclipse Partido']
current=read('Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json')['pages']
kat=[p for p in current if p['skin']==0 and p['weapon']==0 and p['form']==0]
coverage={}
for p in kat:coverage.setdefault(p['action']+':'+p.get('attackKey',''),[]).append(dirs[p['direction']])
actions=[]
for m in mat['runtimeNamedAttacks']:
 if m['key'] not in keys:continue
 k=m['key']; ms=[round(t*1000) for t in m['phases']]
 actions.append({'attackKey':k,'phaseMs':ms,'directions':dirs,'framePlan':{'startupFrames':1,'activeFrames':1,'recoveryFrames':2,'durationMs':[ms[0],ms[1],ms[2]//2,ms[2]-ms[2]//2]},'catalogDirections':coverage.get('Attack:'+k,[]),'status':'planned','sourceTimingsVerifiedAgainstCurrentCode':True,'notes':'Normal appearance regardless of technique name; no new damage/contact events'})
styles={
'Carregado':'A compact charged katana release: wind sword back into a loaded high guard, commit a strong diagonal cut through the target plane, carry the blade through, recover to the approved low guard.',
'ContraAtaque':'A precise counter from a compact defensive katana guard: intercept line with the steel blade, immediate crisp riposte at target height, controlled follow-through, recover. Right hand remains primary grip.',
'OndaDoVeu':'A full-body circular katana technique matching the existing 360 degree sector: coil torso and knees, cut through a sweeping arc at the active instant, settle the coat and blade, regain low guard. Do not add magic glow or transformation.',
'KatanaAerial':'Aerial launcher: grounded coiled anticipation, rising katana cut at active contact, suspended follow-through, controlled landing recovery. Entire body, boots and full katana stay visible. Vertical travel is driven by runtime; use intentional pivot anchors.',
'Corte do Silencio':'A compact pause-branch katana cut with a small controlled step forward: still coiled guard, decisive precise lateral cut, restrained follow-through, low guard recovery.',
'Ruptura Precisa':'A precise heavy finisher: stable high preparation, forceful diagonal steel cut with adult body mechanics, braced follow-through, deliberate return to guard. Technique name does not change hair, eyes, skin or clothing.',
'Corte do Crepúsculo':'A double-contact katana finisher: compact loaded guard, two clearly ordered intersecting cuts across a 155 degree sweep, settle blade and coat, return to guard. Both contacts occur within one existing 160 ms active window, without adding gameplay events.',
'Eclipse Partido':'A broad double-contact katana special: weight shifts into a loaded guard, two distinct connected sweeping cuts through a 190 degree plane, controlled follow-through, regain low guard. Normal brown hair and neutral steel only, no eclipse glow, no transformations.'}
base='''Use case: stylized-concept
Asset type: DARKLIGHT Requiem Normal katana animation candidate, RGBA transparent background.
Subject: Fabius adult Requiem exactly matching the inspected approved Katana references. Preserve mature face, adult proportions, compact natural brown hair silhouette, dark-red irises only where visible, clean warm skin, black coat with existing dark-red lining and shoulder details, black gloves, belt and boots, exact neutral silver katana. Anatomical right hand is the dominant sword grip; off hand may support the same hilt only where appropriate. Maintain camera, proportions and pixel clusters of approved references; no chibi exaggeration, hair enlargement or invented accessories.
Composition: 8 complete figures in two rows of four, ample transparent separation. Each row is one direction, left-to-right anticipation, active contact, follow-through recovery, final guard recovery. All blade tips, hands, hair, coat and boots fully inside the frame. Same body scale in all figures. Real directional drawings, never mirrored asymmetries. No captions, grids, scenery, ground shadows, glow or lettering. Crisp deliberately drawn raster, no blur.
Runtime contract: source raster resolution is separate from WorldVisualScale 1.325. No resizing or anatomy assembly is authorized. Keep correct existing figures untouched; these are new complete figures for missing clips only.
'''
for a in actions:
 safe=re.sub('[^A-Za-z0-9]+','-',a['attackKey']).strip('-')
 a['promptId']=safe
 for pair in [dirs[i:i+2] for i in range(0,8,2)]:
  promptbase=base if a['attackKey'] not in ['Corte do Crepúsculo','Eclipse Partido'] else base.replace('8 complete figures in two rows of four','10 complete figures in two rows of five').replace('anticipation, active contact, follow-through recovery, final guard recovery','anticipation, first active contact, second active contact, follow-through recovery, final guard recovery')
  text=promptbase+'Directions: top row '+pair[0]+', bottom row '+pair[1]+'.\nTechnique: '+a['attackKey']+'; '+styles[a['attackKey']]+'\nExisting phase times (ms): '+str(a['phaseMs'])+('. Five poses each direction; two contacts share the existing active window; no invented events.\n' if a['attackKey'] in ['Corte do Crepúsculo','Eclipse Partido'] else '. Four poses each direction; no invented events.\n')
  (S/(safe+'-'+('_'.join(pair))+'-planned-prompt.txt')).write_text(text,encoding='utf-8')
# Double-contact actions use 5 frames, with the active phase divided between the two distinct contacts.
for a in actions:
 if a['attackKey'] in ['Corte do Crepúsculo','Eclipse Partido']:
  x,y,z=a['phaseMs'];a['framePlan']={'startupFrames':1,'activeFrames':2,'recoveryFrames':2,'durationMs':[x,y//2,y-y//2,z//2,z-z//2]}
  a['batchFigures']=10
 else:a['batchFigures']=8
states=[{'action':'Charging','loop':True,'runtime':'heavyHeld until release; chargeSeconds=450ms','plan':'load stance and steady held tension; no charged release contact'}, {'action':'Parry','loop':False,'runtimeMs':260,'perfectWindowMs':140}, {'action':'Hurt','loop':False,'runtimeDefaultMs':180,'runtime':'variable hurtDuration set by caller; final frame holds'}, {'action':'Dead','loop':False,'runtime':'unbounded final-pose hold, no synthetic rotated idle'}, {'action':'StartMove','loop':False,'runtimeMs':120}, {'action':'Brake','loop':False,'runtimeMs':140}, {'action':'Turn','loop':False,'runtimeMs':140}, {'action':'IdleAlert','loop':True,'runtime':'threat while neutral'}, {'action':'IdleWounded','loop':True,'runtime':'critical hysteresis <=25%, remains until >=32%'}, {'action':'IdleInspect','loop':False,'runtimeMs':1100}, {'action':'IdleImpatient','loop':False,'runtimeMs':1100}, {'action':'IdleFatigued','loop':False,'runtimeMs':1100}]
contract={'date':'2026-11-02','status':'planned-waiting-GATE15','scope':{'skin':0,'weapon':0,'form':0,'directions':dirs,'excludedExistingActions':['Leve1','Leve2','Leve3','Pesado','Idle','Walk','Dodge']},'worldVisualScale':1.325,'identity':'Approved Requiem Katana adult body, compact natural brown hair, dark-red eyes, warm approved skin and original coat; Normal only','rightHandDominant':True,'references':refrecords,'consumerSources':[{'path':p,'sha256':sha(p)} for p in consumer],'timingSource':'Current assets + DarklightCombatProfiles + DarklightAirCombo; older matrix counts ignored','actions':actions,'airCombo':mat['airComboKeys'],'airChaseException':'Perseguição Aérea has active=0, damage=0. Preserve zero-contact semantics; existing phase selector skips active. Metadata compatibility to be coordinated before generation.','statesAndTransitions':states,'currentCatalogSnapshot':{'allPages':len(current),'katanaNormalPages':len(kat),'coverage':coverage},'generationGate':'No imagegen call until coordinator sends GATE15 LIBERADO','deliveryStates':{'candidate':False,'visualReviewed':False,'imported':False,'runtimeVerified':False},'packing':'whole-figure native RGBA only, compact atlas <4096 per side, integer positions, source hashes and real bounds. PPU calibrates raster density only. No resampling, body assembly, mirror or recolor.','separateLayers':'Character animation here only. Runtime effects and gameplay events stay existing; no invented effect/contact layers.'}
save(S/'contract.json',contract)
save(S/'palette.json',{'source':'inspected whole references','uniqueRgbaCount':len(palette),'top256':[{'rgba':list(c),'count':n} for c,n in palette.most_common(256)],'roleNotes':{'skinBase':[246,174,130],'irisBase':'#C40814','irisShadow':'#5A0000','other':'material roles require visual judgment; frequencies are not semantic labels'}})
save(O/'inventory.json',contract)
save(O/'checkpoint.json',{'date':'2026-11-02','state':'planned-waiting-GATE15','producedImages':0,'reviewedImages':0,'importedPages':0,'runtimeTests':0,'next':'Receive GATE15 LIBERADO; generate Carregado 8 directions using pair batches, inspect and correct whole affected figures; complete action before starting next.'})
print(json.dumps({'actions':len(actions),'plannedDirectionClips':8*len(actions),'promptFiles':32,'catalogPages':len(current),'katanaNormalPages':len(kat),'contractSha256':sha(str((S/'contract.json').relative_to(R))),'inventorySha256':sha(str((O/'inventory.json').relative_to(R))),'gate':'waiting'}))

