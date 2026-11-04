"""Manual complete-figure annotations; never edit or reassemble raster anatomy."""
import json
from pathlib import Path
HERE=Path(__file__).resolve().parent
p=HERE/'recipe.json'
recipe=json.loads(p.read_text(encoding='utf-8'))
prefix='ArtSources/WeaponRedesign20261103/essential-states/'
canonical='Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalWalkS20261101V2Selected.png'
base_ppu=170.95959595959596

def append(name,action,skull,directions,bounds,anchors,margin,reference):
    phases={'Parry':['braced-contact-hold'],'Hurt':['contact','reaction'],
            'Dead':['collapse','fall-ground-contact','corpse-rest-hold']}[action]
    durations={'Parry':[260],'Hurt':[60,120],'Dead':[180,220,1000]}[action]
    density=base_ppu*skull/147
    figures=[]
    for i,(b,a) in enumerate(zip(bounds,anchors)):
        x0,y0,x1,y1=b
        figures.append({'rect':[x0-margin,y0-margin,x1-x0+2*margin,y1-y0+2*margin],
            'anchor':a,'anchorMethod':('Manual grounded pelvis/torso support; distinct collapse/fall/corpse origins, never feet-bottom autoalignment.' if action=='Dead' else 'Manual support midpoint between grounded soles, retaining in-place recoil.'),
            'direction':directions[i//len(phases)],'phase':phases[i%len(phases)],'durationMs':durations[i%len(phases)]})
    sheet={'name':name,'source':name+'-source.png','weapon':2,'action':action,
      'status':'visual-reviewed','pixelsPerUnit':density,'figures':figures,
      'generationReferences':[prefix+reference,canonical],
      'review':{'staticVisualReviewed':True,'wholeFigures':True,'loop':False,
        'expressionDirection':'Retain user-approved expressive charming Hurt identity consistently; no facial recolor, head/torso redesign or anatomical assembly.',
        'equipment':'Two silver claw gauntlets and two silver layered clawed greaves, all complete figures newly drawn; no concealed incompatible katana/saw equipment reused.',
        'densityGuide':{'referenceCranialWidth':147,'referencePPU':base_ppu,'sourceCranialWidth':skull,
          'method':'Native cranial-width guide within complete figure, checked against torso density. Same fixed density through every phase; never normalized by current crouch/corpse AABB height.'},
        'timing':('Intentional single260ms guard hold.' if action=='Parry' else '60ms contact then120ms reaction; reaction visible during shortened120ms stagger.' if action=='Hurt' else '180ms collapse,220ms ground fall, final corpse1000ms authored hold then clamped indefinitely by consumer; no return to Idle.'),
        'nativePixelResampling':False,'imported':False,'runtimeVerified':False,
        'motionReview':'Consecutive complete poses inspected statically; finite GIF authored; Unity/runtime review deferred.'}}
    recipe['sheets']=[s for s in recipe['sheets'] if s['name']!=name]+[sheet]

append('ReqFistsNormalParry8Dirs20261103V1','Parry',159,['S','SW','W','NW','N','NE','E','SE'],
 [[91,35,356,410],[548,33,805,411],[1005,33,1228,411],[1443,33,1682,404],[92,478,356,853],[535,478,765,845],[988,478,1203,855],[1416,479,1669,856]],
 [[233,408],[665,409],[1108,409],[1550,402],[234,851],[653,843],[1108,853],[1541,854]],8,
 'ReqKatanaNormalHurtS_SW_W_NW20261103V2-source.png')
append('ReqFistsNormalHurtS_SW_W_NW20261103V1','Hurt',160,['S','SW','W','NW'],
 [[149,29,412,384],[606,23,885,384],[169,417,442,761],[614,404,921,760],[163,791,442,1141],[632,782,889,1138],[166,1172,447,1524],[624,1163,884,1524]],
 [[286,382],[741,382],[276,759],[760,758],[253,1139],[741,1136],[254,1522],[737,1522]],8,
 'ReqKatanaNormalHurtS_SW_W_NW20261103V2-source.png')
append('ReqFistsNormalHurtN_NE_E_SE20261103V1','Hurt',160,['N','NE','E','SE'],
 [[128,51,358,380],[528,18,807,381],[114,428,358,767],[560,403,813,768],[113,803,371,1144],[586,784,820,1145],[114,1182,356,1523],[569,1160,862,1523]],
 [[247,378],[679,379],[249,765],[710,766],[240,1142],[723,1143],[234,1521],[739,1521]],8,
 'ReqKatanaNormalHurtN_NE_E_SE20261103V1-source.png')
append('ReqFistsNormalDeadS_SW_W_NW20261103V1','Dead',121,['S','SW','W','NW'],
 [[104,19,318,311],[454,82,749,312],[846,175,1234,309],[114,326,334,601],[452,372,730,606],[820,474,1224,600],[101,617,344,891],[435,667,749,890],[816,759,1270,891],[88,901,341,1173],[418,951,740,1167],[813,1053,1274,1175]],
 [[214,309],[581,310],[1057,307],[217,599],[577,604],[1053,598],[225,889],[576,888],[1050,889],[220,1171],[574,1165],[1046,1173]],4,
 'ReqKatanaNormalDeadS_SW_W_NW20261103V1-source.png')
append('ReqFistsNormalDeadN_NE_E_SE20261103V1','Dead',121,['N','NE','E','SE'],
 [[162,10,338,268],[532,63,764,269],[989,141,1339,261],[153,281,342,534],[528,327,759,535],[990,417,1356,531],[138,544,350,779],[527,589,770,781],[985,669,1364,780],[136,784,338,1016],[513,835,771,1015],[979,907,1358,1013]],
 [[255,266],[653,267],[1155,259],[257,532],[650,533],[1160,529],[243,777],[650,779],[1160,778],[237,1014],[650,1013],[1150,1011]],2,
 'ReqKatanaNormalDeadN_NE_E_SE20261103V1-source.png')
p.write_text(json.dumps(recipe,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Punhos complete: Parry/Hurt/Dead eight directions,48 native figures; no source pixels changed.')
