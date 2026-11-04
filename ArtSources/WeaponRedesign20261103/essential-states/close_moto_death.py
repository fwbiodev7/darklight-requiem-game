"""Select intact complete figures; preserve every selected RGBA byte, no anatomy editing."""
import json,hashlib
from pathlib import Path
from PIL import Image,ImageDraw
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
OUT=ROOT/'outputs/retomada-20261103/essential-states'
recipe=json.loads((HERE/'recipe.json').read_text(encoding='utf-8'))
prefix='ArtSources/WeaponRedesign20261103/essential-states/'
original='ReqMotoNormalDeadS_SW_W_NW20261103V1-source.png'
repair='ReqMotoNormalDeadRepair8Figures20261103V2-source.png'
density=157.82828282828282*126/140
phases=['collapse','fall-ground-contact','corpse-rest-hold']
durations=[180,220,1000]
review={'wholeFigures':True,'staticVisualReviewed':True,'loop':False,
 'expressionDirection':'Approved expressive charming character identity preserved, no speculative face redesign.',
 'equipment':'Both black/silver circular saws, black gloves and plain strapped boots. NW fall/rest repaired because original corpse saws were cut at native canvas bottom.',
 'densityGuide':{'referencePPU':157.82828282828282,'referenceNativeCranialWidth':140,'originalSourceNativeCranialWidth':126,
  'method':'Skull/trunk native raster-density comparison, never normalized by bent/lying body height. Selected repair uses its own native density; no source pixel resampling.'},
 'timing':'180ms collapse,220ms ground fall,1000ms final corpse authored hold; consumer clamps final frame indefinitely, no return Idle/no death loop.',
 'nativePixelResampling':False,'imported':False,'runtimeVerified':False,
 'motionReview':'Whole consecutive poses inspected statically, finite GIF authored; Unity/runtime validation deferred.'}

def add(spec):
    recipe['sheets']=[s for s in recipe['sheets'] if s['name']!=spec['name']]+[spec]

def pack(name, entries):
    columns=3
    rows=(len(entries)+columns-1)//columns
    atlas=Image.new('RGBA',(512*columns,384*rows))
    figures=[]
    artifacts=[]
    for i,(source,bounds,anchor,direction,phase,ppu) in enumerate(entries):
        image=Image.open(HERE/source).convert('RGBA')
        x0,y0,x1,y1=bounds
        crop=image.crop((x0,y0,x1,y1))
        cx,cy=(i%columns)*512,(i//columns)*384
        local_anchor=(anchor[0]-x0,anchor[1]-y0)
        paste=(cx+256-local_anchor[0],cy+350-local_anchor[1])
        assert cx <= paste[0] and paste[0]+crop.width <= cx+512
        assert cy <= paste[1] and paste[1]+crop.height <= cy+384
        atlas.paste(crop,paste)
        assert atlas.crop((*paste,paste[0]+crop.width,paste[1]+crop.height)).tobytes()==crop.tobytes()
        figures.append({'rect':[cx,cy,512,384],'anchor':[cx+256,cy+350],
          'anchorMethod':'Manual projected pelvis/torso ground support; collapse/fall/rest have distinct intentional origins. Not automated feet-bottom alignment.',
          'direction':direction,'phase':phases[phase],'durationMs':durations[phase],
          'pixelsPerUnit':ppu,'originalSourcePath':prefix+source,
          'originalSourceRect':[x0,y0,x1-x0,y1-y0],'originalSourceAnchor':anchor,
          'sourceRGBAEqualityVerified':True})
        if prefix+source not in artifacts:
            artifacts.append(prefix+source)
    source_name=name+'-source.png'
    atlas.save(HERE/source_name)
    add({'name':name,'source':source_name,'weapon':3,'action':'Dead','status':'visual-reviewed',
         'pixelsPerUnit':density,'figures':figures,'review':review,'sourceArtifacts':artifacts,
         'generationReferences':artifacts})

bounds=[[64,21,362,306],[436,78,796,314],[877,168,1274,315],
        [59,328,335,598],[396,377,750,606],[844,463,1273,609],
        [58,618,334,891],[396,671,744,903],[840,761,1281,898]]
anchors=[[214,304],[602,312],[1080,313],[216,596],[581,604],[1070,607],
         [221,889],[575,901],[1060,896]]
entries=[]
for i,(b,a) in enumerate(zip(bounds,anchors)):
    x0,y0,x1,y1=b
    entries.append((original,[x0-6,y0-6,x1+6,y1+6],a,['S','SW','W'][i//3],i%3,density))
pack('ReqMotoNormalDeadS_SW_W20261103V1Selected',entries)
# The repair prompt requested NW in row1, but the actual row2 is the genuine NW view.
# Select actual drawn direction, not prompt label; preserve original correct collapse.
pack('ReqMotoNormalDeadNW20261103V2Selected',[
 (original,[43,894,331,1174],[224,1166],'NW',0,density),
 (repair,[59,446,434,734],[255,726],'NW',1,157.82828282828282),
 (repair,[510,533,978,733],[744,725],'NW',2,157.82828282828282)])

name='ReqMotoNormalDeadN_NE_E_SE20261103V1'
bounds=[[122,10,388,264],[566,59,883,266],[1043,140,1442,274],
 [126,279,365,514],[549,316,870,516],[1044,395,1449,525],
 [131,524,337,759],[555,563,843,758],[1038,635,1441,766],
 [126,772,341,1013],[541,809,837,1017],[1032,883,1423,1013]]
anchors=[[255,262],[699,264],[1220,272],[245,512],[692,514],[1220,523],
 [238,757],[690,756],[1210,764],[239,1011],[685,1015],[1205,1011]]
figures=[]
for i,(b,a) in enumerate(zip(bounds,anchors)):
    x0,y0,x1,y1=b
    figures.append({'rect':[x0-4,y0-4,x1-x0+8,y1-y0+8],'anchor':a,
      'anchorMethod':'Manual pelvis/torso support on ground, distinct from aerial/sole endpoints.',
      'direction':['N','NE','E','SE'][i//3],'phase':phases[i%3],'durationMs':durations[i%3]})
add({'name':name,'source':name+'-source.png','weapon':3,'action':'Dead','status':'visual-reviewed',
 'pixelsPerUnit':density,'figures':figures,'review':review,
 'generationReferences':[prefix+'ReqKatanaNormalDeadN_NE_E_SE20261103V1-source.png',
  prefix+'ReqMotoNormalParry8Dirs20261103V1-source.png',prefix+original]})

(HERE/'recipe.json').write_text(json.dumps(recipe,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
points=[[222,185],[612,152],[1017,131],[1465,133],[227,598],[755,577],[1192,575],[1620,593]]
image=Image.open(HERE/'ReqMotoNormalParry8Dirs20261103V1-source.png').convert('RGBA')
guide=Image.new('RGBA',image.size,(240,235,228,255));guide.alpha_composite(image);draw=ImageDraw.Draw(guide)
for x,y in points:
    draw.ellipse((x-5,y-5,x+5,y+5),outline=(255,128,0,255),width=2)
    draw.line((x-10,y,x+10,y),fill=(255,128,0,255))
    draw.line((x,y-10,x,y+10),fill=(255,128,0,255))
guide.convert('RGB').save(OUT/'MotoParry-guard-contact-guides.png')
guide.crop((75,0,375,425)).resize((600,850),Image.Resampling.NEAREST).save(OUT/'MotoParry-S-whole-contact-guide-2x.png')
print('Moto death8 directions selected;10 original complete figures preserved exactly,2 NW whole figures repaired; no source resampling.')
