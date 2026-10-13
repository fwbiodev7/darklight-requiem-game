"""Original procedural sound design. No samples, model voices, downloads or playback.
Run with output directory as argv[1]; stable seeds make all WAVs reproducible.
"""
import hashlib, json, math, random, struct, sys, wave
from pathlib import Path
RATE = 22050
OUT = Path(sys.argv[1]); OUT.mkdir(parents=True, exist_ok=True)
records=[]
def save(name, values, description):
    peak=max(abs(x) for x in values) or 1
    scale=min(1, .72/peak)
    payload=b''.join(struct.pack('<h',round(max(-1,min(1,x*scale))*32767)) for x in values)
    p=OUT/(name+'.wav')
    with wave.open(str(p),'wb') as f:
        f.setnchannels(1);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(payload)
    records.append(dict(file=p.name,author='DARKLIGHT project / Codex procedural sound design',origin='generate_audio_expansion.py',license='Original generated project asset; no third-party recording',attribution='No external attribution obligation',description=description,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),duration=round(len(values)/RATE,3),sampleRate=RATE,channels=1))
def noise(seed,n,smoothing=.4):
    rng=random.Random(seed);last=0;data=[]
    for _ in range(n):
        last=last*smoothing+rng.uniform(-1,1)*(1-smoothing);data.append(last)
    return data
for material,(freq,grain,decay) in {'dirt':(92,.34,18),'sand':(160,.14,13),'water':(430,.64,10),'metal':(740,.85,17),'building':(180,.68,21),'mud':(72,.8,11)}.items():
    for variant in range(3):
        duration=.32 if material in ('water','mud') else .23
        n=int(RATE*duration);ns=noise(301+variant+int(freq),n,grain);v=[]
        for i,x in enumerate(ns):
            t=i/RATE;env=min(1,t/.003)*math.exp(-t*decay)*(1-t/duration)
            tone=math.sin(2*math.pi*(freq+variant*11)*t)*(.25 if material=='metal' else .09)
            splash=math.sin(2*math.pi*(freq*t+130*t*t))*math.exp(-t*8)*.11 if material in ('water','mud') else 0
            v.append((x*.52+tone+splash)*env)
        save(f'dl-step-{material}_{variant:02}',v,'Original '+material+' foot contact; three distinct samples')
kinds=['crawler','shooter','armored','hunter','spectral','kael','cantor','matriarch','sentinel','warden','lucien','valtherion','seraphine','kaelfractured','nocthar']
for k,kind in enumerate(kinds):
    base=[69,138,52,114,241,94,185,156,46,82,212,61,232,106,37][k]
    for variant in range(2):
        duration=.36+(.18 if k>=5 else 0)+variant*.04;n=int(RATE*duration);ns=noise(800+k*13+variant,n,.85);v=[]
        for i,x in enumerate(ns):
            t=i/RATE;p=t/duration;env=math.sin(math.pi*p)**.8
            f=base*(1+.12*math.sin(p*math.pi*2)+variant*.035)
            tone=sum(math.sin(2*math.pi*f*h*t)*(.18/h) for h in (1,2,3,5))
            v.append((tone*(.65+.35*math.sin(2*math.pi*(13+k)*t))+x*.23)*env)
        save(f'dl-enemy-{kind}_{variant:02}',v,'Stylized synthetic creature texture; not a human vocal performance')
    if k>=5:
        n=int(RATE*.8);ns=noise(1700+k,n,.87);v=[]
        for i,x in enumerate(ns):
            t=i/RATE;p=t/.8;f=base*2*(1-p*.45);env=math.sin(math.pi*p)**.7
            v.append((math.sin(2*math.pi*f*t)*.2+math.sin(2*math.pi*(f*1.51)*t)*.11+x*.16)*env)
        save('dl-boss-'+kind,v,'Exclusive boss energy/phase signature')
for j,name in enumerate(['claws','shadow','embrace','judgment','rupture','eclipse','absolute','parry','counter','critical','finisher','dodge']):
    duration=.42 if j<7 else .23;n=int(RATE*duration);ns=noise(2400+j,n,.5 if j<7 else .25);v=[]
    for i,x in enumerate(ns):
        t=i/RATE;p=t/duration;env=math.sin(math.pi*p)*math.exp(-p*(1 if j<7 else 3))
        f=92+j*63+(1-p)*(370 if j in (1,7,11) else 110)
        v.append((x*.32+math.sin(2*math.pi*f*t)*.24+math.sin(2*math.pi*f*1.7*t)*.07)*env)
    save('dl-power-'+name,v,'Original ability / combat transient')
n=int(RATE*.5);v=[]
for i in range(n):
    t=i/RATE;beats=0
    for onset in (0,.18):
        u=t-onset
        if u>=0:beats+=math.sin(2*math.pi*63*u)*math.exp(-u*35)*min(1,u/.004)
    v.append(beats*.5)
save('dl-heartbeat',v,'Original two-part low-health heartbeat')
for j,name in enumerate(['rain','leaves','water','cave','urban','veil']):
    duration=8;n=int(RATE*duration);ns=noise(3000+j,n,.985 if name in ('cave','veil','urban') else .6);v=[]
    for i,x in enumerate(ns):
        t=i/RATE
        # Zero-value + zero-slope boundaries make the generated layer loop continuous.
        edge=math.sin(math.pi*t/duration)**.7
        tone=sum(math.sin(2*math.pi*f*t)*a for f,a in [(48+j*8,.05),(72+j*12,.025)]) if j>=3 else 0
        ripple=math.sin(2*math.pi*3*t)**2*.04 if name=='water' else 0
        v.append((x*.48+tone+ripple)*edge)
    save('dl-ambient-'+name,v,'Seam-safe original regional ambience layer')
for j,name in enumerate(['thunder','drip','creature','whisper','portal']):
    duration=1.6 if name=='thunder' else .8;n=int(RATE*duration);ns=noise(4000+j,n,.97 if j==0 else .88);v=[]
    for i,x in enumerate(ns):
        t=i/RATE;p=t/duration;env=math.sin(math.pi*p)*math.exp(-p*2)
        tone=math.sin(2*math.pi*(75+j*61)*t)*(.12 if j!=1 else .04)
        v.append((x*.42+tone)*env)
    save('dl-ambient-'+name,v,'Original occasional environmental cue; no speech')
(OUT/'EXPANSION-PROVENANCE.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'clips':len(records),'bytes':sum(p.stat().st_size for p in OUT.glob('*.wav')),'maxPeak':.72,'monoRate':RATE}))
