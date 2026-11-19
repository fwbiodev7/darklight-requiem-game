from pathlib import Path
from PIL import Image
import json,hashlib
p=Path(__file__).resolve().parent
f=p/'serafim-transition-final.json'
d=json.loads(f.read_text(encoding='utf-8'))
d.update(status='approved-design-preview-pending-temporal',visualApproval=True,
 visualReview='All eight full figures and final light contact inspected; temporal playback pending root review',
 sourceImage='serafim-transition-candidate-approved-design.png',
 approval={'userQuote':'serafim tambem curti pode manter essa versão','selectedGeneration':'v2',
 'selectionNote':'v3 was in flight during explicit user approval and is not selected',
 'normalCorrection':'Full first cell [0,0,443,443] from v1 preserved pixel-for-pixel; all seven v2 transformed cells unchanged',
 'integrationApproval':False})
d['limitations'][-1]='No Unity import, runtime or gameplay event validation; design approved, temporal preview still pending review.'
f.write_text(json.dumps(d,indent=2,ensure_ascii=False),encoding='utf-8')
s=(p/'build-preview-final.py').read_text(encoding='utf-8')
s=s.replace('serafim-transition-source-final.png','serafim-transition-candidate-approved-design.png').replace("status='candidate'","status='approved-design-preview-pending-temporal'").replace('visualApproval=False','visualApproval=True')
(p/'build-preview-final.py').write_text(s,encoding='utf-8')
gif=p.parent.parent/'previews/serafim/serafim-transition-final.gif'
im=Image.open(gif); durations=[]
for i in range(im.n_frames):
 im.seek(i);durations.append(im.info.get('duration',0))
files=[*p.glob('*.png'),*p.glob('prompt*.txt'),gif,f]
hashes={str(x.relative_to(p.parent.parent)):hashlib.sha256(x.read_bytes()).hexdigest()for x in files}
receipt={'status':'approved-design-preview-pending-temporal','sourcePoseCount':8,'gifStoredFrames':im.n_frames,
 'gifDurationMs':sum(durations),'candidateOnly':True,'imported':False,'runtimeVerified':False,
 'normalWholeCellPreserved':True,'transformedSevenCellsV2Preserved':True,'v3Selected':False,'hashes':hashes}
(p/'receipt-final.json').write_text(json.dumps(receipt,indent=2,ensure_ascii=False),encoding='utf-8')
print(json.dumps({'gifStoredFrames':im.n_frames,'gifDurationMs':sum(durations),'selectedSourceSha256':d['sourceSha256'],'gifSha256':hashlib.sha256(gif.read_bytes()).hexdigest()}))

