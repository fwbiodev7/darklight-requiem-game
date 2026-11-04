from pathlib import Path
import hashlib
import json
import shutil
import subprocess

BASE = Path(__file__).resolve().parents[1]
OUT = BASE / 'outputs/continuacao-escola-20261103/punhos-sw-inspecao'
OUT.mkdir(parents=True, exist_ok=True)
SOURCE = Path('C:/Users/Usuario/.codex/generated_images/01a1a117-ff4b-72b3-9c2a-e2735c75b8ff')
versions = [('exec-79cbd8e7-9a08-42f3-8d59-2c098eea3383.png', 'v1-oito-poses.png'),
            ('exec-6635fb68-1928-4356-9460-9ec508576950.png', 'v2-correcao-apoio.png'),
            ('exec-079927c8-031f-4800-a55b-2554f3a05b9a.png', 'v3-quatro-poses.png')]
for source, name in versions:
    shutil.copy2(SOURCE / source, OUT / name)
name = 'ReqFistsNormalWalkSW20261103SchoolInspection'
shutil.copy2(OUT / 'v1-oito-poses.png', OUT / (name + '.png'))
frames = []
for i in range(8):
    x1, x2 = (i % 4) * 1774 // 4, (i % 4 + 1) * 1774 // 4
    y1, y2 = (i // 4) * 887 // 2, (i // 4 + 1) * 887 // 2
    w, h = x2-x1, y2-y1
    frames.append(dict(id=f'Redesign_{name}_{i:02d}', rect=[x1,y1,w,h],
                       pivot=[.5,(h-405)/h], durationMs=50, action='walk', direction='SW',
                       skin='Requiem', form='Normal', phase='unconfirmed_'+str(i),
                       sourceRect=[x1,y1,w,h], sourceFrameIndex=i, sourceScale=1))
descriptor = dict(schemaVersion=1, image=name+'.png',
                  imageSha256=hashlib.sha256((OUT/(name+'.png')).read_bytes()).hexdigest(),
                  nativeSize=[1774,887], sourceSize=[1774,887], pixelsPerUnit=170.95959595959596,
                  status='candidate', wholeFigures=True, integrationEligible=False,
                  rejectionReason='Same leading leg persists; opposite support not established in three generated attempts.',
                  provisionalRegistration=True, worldVisualScale=1.325,
                  frames=frames, clips=[dict(name='inspection_walk_SW', frames=[f['id'] for f in frames], loop=True, allowStatic=False)],
                  requiredClips=['inspection_walk_SW'], quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=1.3))
(OUT/(name+'.json')).write_text(json.dumps(descriptor,indent=2)+'\n',encoding='utf-8')
tool=BASE/'work/darklight-requiem-game/Tools/Skills/pixel-art-sprite-sheet-master/scripts/sprite_pipeline.py'
commands = [('audit',['--manifest',str(OUT/(name+'.json')),'--out',str(OUT/'audit')]),
            ('palette',['--image',str(OUT/(name+'.png')),'--out',str(OUT/'palette.json')]),
            ('preview',['--manifest',str(OUT/(name+'.json')),'--out',str(OUT/'preview'),'--zoom','2']),
            ('unity-json',['--manifest',str(OUT/(name+'.json')),'--out',str(OUT/'unity-slices-INSPECTION-ONLY.json')])]
results=[]
with (OUT/'commands.log').open('w',encoding='utf-8') as log:
    for command,args in commands:
        result=subprocess.run(['python',str(tool),command,*args],stdout=log,stderr=subprocess.STDOUT)
        results.append(dict(command=command,exitCode=result.returncode))
(OUT/'command-results.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
print(results)
