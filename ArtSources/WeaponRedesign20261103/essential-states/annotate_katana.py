"""Explicit full-figure source annotations, authored after visual inspection."""
import json
from pathlib import Path

HERE=Path(__file__).resolve().parent
recipe=json.loads((HERE/'recipe.json').read_text(encoding='utf-8'))

def append(name,action,ppu,directions,bounds,anchors,phases,durations,review,margin=10):
    figures=[]
    for i,(box,anchor) in enumerate(zip(bounds,anchors)):
        x0,y0,x1,y1=box
        figures.append({'rect':[max(0,x0-margin),max(0,y0-margin),x1-x0+2*margin,y1-y0+2*margin],
                        'anchor':anchor,'anchorMethod':
                        ('Manual projected pelvis/torso support on ground; collapse, fall and corpse use distinct intentional support anchors, never automated feet-bottom alignment.'
                         if action=='Dead' else 'Manual projected support between grounded soles; body recoil is retained around this ground support.'),
                        'direction':directions[i//len(phases)],'phase':phases[i%len(phases)],
                        'durationMs':durations[i%len(phases)]})
    sheet={'name':name,'source':name+'-source.png','weapon':0,'action':action,
           'status':'visual-reviewed','pixelsPerUnit':ppu,'figures':figures,'review':review}
    recipe['sheets']=[s for s in recipe['sheets'] if s['name']!=name]+[sheet]

hurt_review={'staticVisualReviewed':True,'wholeFigures':True,
 'expressionAuthority':'User explicitly approved these expressive charming Hurt faces; source PNGs are preserved without recoloring, facial edits or anatomical resampling.',
 'timing':'Contact60ms then reaction120ms =180ms; reaction is already visible during shortened120ms ResistsStagger hurt. Runtime owns return to neutral.',
 'loop':False,'density':'Constant168.181818PPU across both phases; matched skull/trunk native density to current approved compact Carregado figures. Never normalized by bent-body bounding height.',
 'nativePixelResampling':False,'imported':False,'runtimeVerified':False,
 'motionReview':'Consecutive full-figure phases inspected statically; finite GIF previews authored, runtime playback deferred.'}
recipe['sheets']=[s for s in recipe['sheets'] if not s['name'].startswith('ReqKatanaNormalParry8Dirs')]
append('ReqKatanaNormalParry8Dirs20261103V3','Parry',178.6,['S','SW','W','NW','N','NE','E','SE'],
 [[103,48,348,393],[539,49,770,396],[970,45,1224,396],[1390,48,1673,394],[114,494,343,840],[521,493,782,839],[957,493,1224,841],[1393,495,1651,844]],
 [[227,391],[650,394],[1093,394],[1546,392],[229,838],[662,837],[1096,839],[1525,842]],
 ['braced-contact-hold'],[260],
 {'staticVisualReviewed':True,'wholeFigures':True,'loop':False,'allowStatic':True,
  'timing':'Single intentionally held260ms defensive contact pose, not a multi-phase animation.',
  'correction':'All eight full figures redrawn in V3 with compact torso/legs faithful to approved Hurt; V1/V2 superseded. No source anatomy resized.',
  'density':'Constant178.6PPU, cranial/native-trunk density matched to approved Hurt reference168.18PPU (approx154px versus145px cranial width); not current pose AABB height.',
  'expressionAuthority':'User-approved expressive Hurt face identity carried into focused defensive expressions.',
  'alpha':'Source composed on light background and reviewed; hidden RGB under alpha0 is not rendered artwork.',
  'nativePixelResampling':False,'imported':False,'runtimeVerified':False})
append('ReqKatanaNormalHurtS_SW_W_NW20261103V2','Hurt',168.1818181818182,['S','SW','W','NW'],
 [[87,33,422,374],[559,24,912,374],[73,418,439,755],[555,407,928,751],[83,798,436,1136],[553,785,910,1128],[95,1177,444,1520],[557,1167,906,1517]],
 [[289,372],[774,372],[272,752],[764,749],[271,1133],[749,1126],[274,1518],[751,1515]],
 ['contact','reaction'],[60,120],hurt_review)
append('ReqKatanaNormalHurtN_NE_E_SE20261103V1','Hurt',168.1818181818182,['N','NE','E','SE'],
 [[128,52,440,379],[555,23,902,379],[126,433,439,766],[579,406,912,765],[127,811,426,1142],[606,787,909,1144],[117,1188,351,1520],[589,1163,870,1520]],
 [[251,377],[702,377],[240,763],[717,762],[245,1139],[738,1141],[240,1518],[747,1518]],
 ['contact','reaction'],[60,120],hurt_review)
dead_review={'staticVisualReviewed':True,'wholeFigures':True,
 'phases':'Three distinct complete poses: knees/torso collapse, hip/forearm ground contact, horizontal settled corpse. Last drawing is held indefinitely by non-looping consumer.',
 'timing':'180ms collapse +220ms fall +1000ms final-frame hold; consumer clamps forever to final frame, never returns to Idle.',
 'loop':False,'density':'Constant139.5PPU across all three phases; native skull/trunk drawing density calibrated against approved Hurt/Carregado, never corpse AABB height.',
 'densityGuide':{'referenceHurtSkullNativeApprox':[145,128],'sourceSkullNativeApprox':[115,111],'referencePPU':168.1818181818182,'method':'Approximate cranial-area density comparison, same constant density through collapse/fall/corpse; corpse remains naturally low.'},
 'nativePixelResampling':False,'imported':False,'runtimeVerified':False,
 'motionReview':'Consecutive full-figure phases inspected statically; finite GIF previews authored, runtime playback deferred.'}
append('ReqKatanaNormalDeadS_SW_W_NW20261103V1','Dead',139.5,['S','SW','W','NW'],
 [[22,26,316,305],[405,86,763,306],[875,179,1247,301],[22,329,329,596],[385,382,760,599],[861,475,1250,595],[32,619,338,885],[377,674,770,885],[863,763,1295,884],[31,903,331,1169],[377,954,761,1165],[853,1057,1297,1168]],
 [[215,303],[600,304],[1080,299],[211,594],[588,597],[1070,593],[220,883],[573,883],[1080,882],[230,1167],[580,1163],[1070,1166]],
 ['collapse','fall-ground-contact','corpse-rest-hold'],[180,220,1000],dead_review,margin=8)
append('ReqKatanaNormalDeadN_NE_E_SE20261103V1','Dead',139.5,['N','NE','E','SE'],
 [[162,12,404,265],[532,63,856,268],[991,140,1372,259],[153,282,390,533],[532,329,867,534],[994,417,1378,529],[141,544,361,778],[527,591,847,781],[988,665,1366,779],[138,784,337,1015],[515,834,778,1019],[980,908,1356,1019]],
 [[255,263],[682,266],[1171,257],[256,531],[685,532],[1175,527],[237,776],[669,779],[1170,777],[237,1013],[651,1017],[1165,1017]],
 ['collapse','fall-ground-contact','corpse-rest-hold'],[180,220,1000],dead_review,margin=2)
prefix='ArtSources/WeaponRedesign20261103/essential-states/'
generation_refs={
 'ReqKatanaNormalParry8Dirs20261103V3':['ReqKatanaNormalHurtS_SW_W_NW20261103V2-source.png','ReqKatanaNormalParry8Dirs20261103V2-source.png'],
 'ReqKatanaNormalHurtS_SW_W_NW20261103V2':['ReqKatanaNormalHurtS_SW_W_NW20261103V1-source.png','ReqKatanaNormalParry8Dirs20261103V2-source.png'],
 'ReqKatanaNormalHurtN_NE_E_SE20261103V1':['ReqKatanaNormalParry8Dirs20261103V2-source.png','ReqKatanaNormalHurtS_SW_W_NW20261103V1-source.png'],
 'ReqKatanaNormalDeadS_SW_W_NW20261103V1':['ReqKatanaNormalParry8Dirs20261103V2-source.png'],
 'ReqKatanaNormalDeadN_NE_E_SE20261103V1':['ReqKatanaNormalDeadS_SW_W_NW20261103V1-source.png','ReqKatanaNormalHurtN_NE_E_SE20261103V1-source.png']}
for s in recipe['sheets']:
    if s['name'] in generation_refs:
        s['generationReferences']=[prefix+p for p in generation_refs[s['name']]]
(HERE/'recipe.json').write_text(json.dumps(recipe,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Explicit annotations saved: Parry/Hurt/Dead eight directions; no source pixels modified.')
