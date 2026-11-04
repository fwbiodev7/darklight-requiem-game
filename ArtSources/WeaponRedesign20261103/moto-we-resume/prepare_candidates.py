from pathlib import Path
import sys,json,hashlib,subprocess
from PIL import Image,ImageDraw
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261103/moto-we-resume';O=R/'outputs/retomada-20261103/moto-we-resume'
sys.dont_write_bytecode=True
sys.path.insert(0,str(R/'Tools/AnimationPreview'))
import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'staging';p.OUT=O
results=[]
for suffix,cols,ppu in [('ResumeV1',4,218),('ResumeBOnlyV2',2,326)]:
 name='ReqMotoNormalWalkWE20261103'+suffix;source=name+'-source.png'
 job=dict(name=name,source=str(A/source),preservedSourceName=source,skin=0,weapon=3,form=0,columns=cols,sourceRows=2,minimumArea=5000,rows=[dict(sourceRow=r,direction=d,action='Walk',loop=False,durationMs=[120]*cols) for r,d in enumerate([2,6])])
 m,pages=p.prepare(job)
 m.update(status='candidate',integrationBlocked=True,rejectionReason='No unambiguous opposite contact B: near hip-knee-ankle chain repeats dominant near-forward support.',presentation='OnFoot',worldVisualScale=1.325,pixelsPerUnit=ppu,framePixelsPerUnit=[ppu]*len(m['frames']),pixelsPerUnitCalibration=dict(provisional=True,method='Invariant source crown-to-belt density estimate against approved Moto S reference. Uniform per source, not pose-fit; body gait unresolved. Source1 approx202px crown-to-belt versus178px/195.96PPU; source2 approx1.495x source1.',baseline='outputs/retomada-20261103/moto-walk-close/ReqMotoNormalWalkS20261103ClosedV1-worldscale-contact.png',anatomicalRepair=False),review=dict(wholeSourceSeen=True,staticContactSeen=True,nearestWholeFiguresSeen=False,continuousMotionViewed=False,imported=False,runtimeVerified=False,humanApproved=False))
 phases=['contact_A','passing_A_low','contact_B_UNPROVEN','passing_B_UNPROVEN'] if cols==4 else ['contact_B_UNPROVEN','passing_B_UNPROVEN']
 for i,f in enumerate(m['frames']):f.update(phase=phases[i%cols],pixelsPerUnit=ppu,durationMs=120)
 for page in pages:page.update(status='candidate',integrationBlocked=True,pixelsPerUnit=ppu,framePixelsPerUnit=[ppu]*cols,presentation='OnFoot',loop=False)
 p.write(p.RES/(name+'.json'),m);p.write(A/(name+'-job.json'),job)
 p.write(O/(name+'-pages.json'),dict(schemaVersion=1,complete=False,status='candidate',integrationBlocked=True,pages=pages))
 im=Image.open(A/source).convert('RGBA')
 contact=Image.new('RGBA',(im.width,im.height+24),(150,150,150,255));contact.alpha_composite(im,(0,24));ImageDraw.Draw(contact).text((8,6),'REJECTED CANDIDATE - W top / E bottom - opposite contact B unproven',fill='black')
 contact.save(O/(name+'-native-contact.png'))
 qa=p.qa(name)
 results.append(dict(name=name,status='rejected-candidate',sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceSize=m['sourceSize'],atlasNativeSize=m['nativeSize'],atlasSha256=m['imageSha256'],descriptorSha256=p.sha(p.RES/(name+'.json')),pagesSha256=p.sha(O/(name+'-pages.json')),figures=len(m['frames']),pixelsPerUnit=ppu,ppuProvisional=True,rgbaExact=m['sourceRgbaExactReconstruction'],sourceResampled=False,contact=(O/(name+'-native-contact.png')).relative_to(R).as_posix(),preview=(O/name/'preview/preview.html').relative_to(R).as_posix(),qa=qa))
p.write(O/'artifacts.json',dict(complete=False,deliveries=results))
print(json.dumps(results))

