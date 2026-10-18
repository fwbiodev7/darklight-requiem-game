"""Inspect existing audio files and routing without playback or downloads."""
from pathlib import Path
import json,wave
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
audio=ROOT/'Assets/Darklight/Resources/Darklight/Audio'
out=ROOT/'outputs/finalizacao-20261018'
report={'playbackPerformed':False,'downloadedFiles':0,'music':[],'clips':[],'issues':[]}
for p in sorted((audio.parent/'Music').glob('*.mp3')):report['music'].append(p.name)
if len(report['music'])!=5:report['issues'].append('Expected the five supplied music files')
for p in sorted(audio.rglob('*.wav')):
    with wave.open(str(p),'rb') as wav:
        channels=wav.getnchannels();rate=wav.getframerate();width=wav.getsampwidth();frames=wav.getnframes();peak=0.
        while block:=wav.readframes(65536):
            if width==2:values=np.frombuffer(block,dtype='<i2').astype(float)/32768
            elif width==1:values=(np.frombuffer(block,dtype=np.uint8).astype(float)-128)/128
            elif width==3:
                packed=np.frombuffer(block,dtype=np.uint8).reshape(-1,3).astype(np.int32)
                integers=packed[:,0]|(packed[:,1]<<8)|(packed[:,2]<<16)
                values=((integers^0x800000)-0x800000).astype(float)/8388608
            elif width==4:values=np.frombuffer(block,dtype='<i4').astype(float)/2147483648
            else:raise ValueError('Unsupported PCM width: '+str(p))
            peak=max(peak,float(np.max(np.abs(values))))
    row=dict(file=p.relative_to(audio).as_posix(),channels=channels,rate=rate,seconds=frames/rate,peak=peak,bytes=p.stat().st_size)
    report['clips'].append(row)
    if peak<=0 or frames<=0:report['issues'].append('Empty/silent clip: '+row['file'])
report['clipCount']=len(report['clips']);report['otherFormatsForUnityValidation']=[p.relative_to(audio).as_posix() for p in audio.rglob('*') if p.suffix.lower() in {'.ogg','.mp3'}];report['success']=not report['issues']
out.mkdir(parents=True,exist_ok=True);(out/'auditoria-audio.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:report[k] for k in ['success','clipCount','downloadedFiles','playbackPerformed','issues']}))
