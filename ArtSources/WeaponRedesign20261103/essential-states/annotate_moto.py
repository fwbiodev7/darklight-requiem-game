"""Approved Moto guard is immutable; only authored anchors and contacts are added."""
import json
from pathlib import Path
HERE=Path(__file__).resolve().parent
p=HERE/'recipe.json'
recipe=json.loads(p.read_text(encoding='utf-8'))
name='ReqMotoNormalParry8Dirs20261103V1'
bounds=[[84,10,364,414],[513,13,816,416],[946,13,1261,414],[1388,11,1712,414],[83,463,362,864],[507,459,813,864],[956,461,1267,865],[1405,461,1706,865]]
anchors=[[224,412],[654,414],[1100,412],[1544,412],[224,862],[663,862],[1104,863],[1545,863]]
contacts=[[222,185],[612,152],[1017,131],[1465,133],[227,598],[755,577],[1192,575],[1620,593]]
directions=['S','SW','W','NW','N','NE','E','SE']
figures=[]
for d,b,a,c in zip(directions,bounds,anchors,contacts):
    x0,y0,x1,y1=b
    figures.append({'rect':[x0-8,y0-8,x1-x0+16,y1-y0+16],'anchor':a,
      'anchorMethod':'Manual projected support midpoint between grounded soles; guard posture preserved.',
      'direction':d,'phase':'dual-saw-braced-contact-hold','durationMs':260,
      'guardContactSource':c,
      'guardContactVisibility':('occluded-projected-in-front-of-torso' if d=='N' else 'visible-tooth-junction')})
sheet={'name':name,'source':name+'-source.png','weapon':3,'action':'Parry',
 'status':'visual-reviewed','pixelsPerUnit':157.82828282828282*157/140,'figures':figures,
 'generationReferences':[
  'ArtSources/WeaponRedesign20261102/normal-moto-completion/ReqMotoNormalSerraEntradaS_SW_W20261102V1-source.png',
  'ArtSources/WeaponRedesign20261103/essential-states/ReqKatanaNormalHurtS_SW_W_NW20261103V2-source.png'],
 'review':{'wholeFigures':True,'staticVisualReviewed':True,'userVisualApproved':True,
  'userApproval':'User explicitly said Moto defense is being made and looks beautiful; preserve current approved appearance. No regeneration or pixel modification after approval.',
  'equipment':'Both black/silver circular saws present, plain black strapped boots, compact Requiem identity.',
  'densityGuide':{'referenceNativeCranialWidth':140,'referencePPU':157.82828282828282,'sourceNativeCranialWidth':157,
    'method':'Native cranial/trunk raster-density comparison; constant density across directions, never guard/body/weapon AABB normalization.'},
  'guardContact':'Manual tooth-tooth encounter point; normalized per SPRITE RECT with Unity bottom-left origin. N contact is hidden by torso and explicitly marked projected/occluded, not falsely measured as visible.',
  'timing':'Single intentional braced contact guard held260ms, non-looping, not a multi-phase animation.',
  'loop':False,'nativePixelResampling':False,'imported':False,'runtimeVerified':False}}
recipe['sheets']=[s for s in recipe['sheets'] if s['name']!=name]+[sheet]
for suffix,directions,bounds,anchors,ref in [
 ('S_SW_W_NW',['S','SW','W','NW'],
  [[82,29,474,373],[548,13,929,370],[88,408,462,756],[560,395,934,753],[101,790,458,1139],[557,776,903,1138],[94,1168,433,1522],[558,1160,925,1522]],
  [[279,371],[732,368],[267,754],[743,751],[269,1137],[731,1136],[254,1520],[732,1520]],
  'ReqKatanaNormalHurtS_SW_W_NW20261103V2-source.png'),
 ('N_NE_E_SE',['N','NE','E','SE'],
  [[98,75,431,382],[552,19,895,383],[99,446,421,758],[581,396,921,760],[117,820,387,1137],[599,775,879,1136],[124,1197,430,1524],[577,1160,898,1525]],
  [[256,380],[713,381],[244,756],[724,758],[247,1135],[726,1134],[256,1522],[739,1523]],
  'ReqKatanaNormalHurtN_NE_E_SE20261103V1-source.png')]:
    n='ReqMotoNormalHurt'+suffix+'20261103V1'
    figures=[]
    for i,(b,a) in enumerate(zip(bounds,anchors)):
        x0,y0,x1,y1=b
        figures.append({'rect':[x0-8,y0-8,x1-x0+16,y1-y0+16],'anchor':a,
          'anchorMethod':'Manual support midpoint between grounded soles; torso/head recoil retained.',
          'direction':directions[i//2],'phase':['contact','reaction'][i%2],'durationMs':[60,120][i%2]})
    recipe['sheets']=[s for s in recipe['sheets'] if s['name']!=n]+[{
      'name':n,'source':n+'-source.png','weapon':3,'action':'Hurt','status':'visual-reviewed',
      'pixelsPerUnit':157.82828282828282*161/140,'figures':figures,
      'generationReferences':['ArtSources/WeaponRedesign20261103/essential-states/'+ref,
        'ArtSources/WeaponRedesign20261103/essential-states/ReqMotoNormalParry8Dirs20261103V1-source.png'],
      'review':{'wholeFigures':True,'staticVisualReviewed':True,'loop':False,
        'expressionDirection':'Preserve approved expressive charming Hurt identity across equipment. Moto Parry user-approved equipment/appearance authority retained.',
        'equipment':'Two separate black/silver circular saws safely outboard of body throughout contact and recoil, black gloves and plain black strapped boots; no incompatible sword/gauntlets reused.',
        'densityGuide':{'referencePPU':157.82828282828282,'referenceNativeCranialWidth':140,'sourceNativeCranialWidth':161,
          'method':'Native skull/trunk density, same fixed PPU across both phases; never crouch/weapon AABB height.'},
        'timing':'Contact60ms, reaction120ms; default180ms Hurt and shortened120ms resistance both show reaction. Consumer owns neutral return.',
        'nativePixelResampling':False,'imported':False,'runtimeVerified':False,
        'motionReview':'Consecutive complete poses inspected statically; finite GIF authored; runtime/Unity review deferred.'}}]
p.write_text(json.dumps(recipe,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Moto guard eight directions annotated; native approved PNG unchanged; eight authored saw contacts.')
