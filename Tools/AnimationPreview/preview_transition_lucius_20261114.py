"""Preview approved whole Lucius figures; no Unity assets or source pixels changed."""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'ArtSources/TransformationTransitions20261114'
SOURCE=BASE/'candidates/lucius/LuciusNormalToArconte-v3-approved-design-source.png'
OUT=BASE/'previews/lucius'


def smooth(value):
    value=max(0,min(1,value))
    return value*value*(3-2*value)


def lift(progress):
    if progress<.12:return 0
    if progress<.34:return smooth((progress-.12)/.22)
    if progress<.76:return 1
    return 1-smooth((progress-.76)/.24)


def main():
    OUT.mkdir(parents=True,exist_ok=True)
    image=Image.open(SOURCE).convert('RGBA')
    assert image.size==(1536,1024)
    anchors=[457,457,454,435,430,441,449,449]
    durations=[420,400,520,750,650,650,500,310]
    phases=['Normal','Concentration','Ascent','Receiving power','Awakening','Consolidation','Descent','Arconte']
    figures=[]
    for index in range(8):
        row,column=divmod(index,4)
        rect=[column*384,row*512,384,512]
        figures.append({'index':index,'phase':phases[index],'sourceRectTopLeft':rect,
                        'supportAnchorInCell':[192,anchors[index]],'durationMs':durations[index]})
    frames=[]
    for index in range(63):
        elapsed=min(4200,index*1000/12)
        cursor=0;pose=7
        for j,duration in enumerate(durations):
            cursor+=duration
            if elapsed<cursor:pose=j;break
        x,y,w,h=figures[pose]['sourceRectTopLeft']
        whole=image.crop((x,y,x+w,y+h))
        canvas=Image.new('RGBA',(512,672),(25,28,32,255))
        vertical=round(600-anchors[pose]-.62*152*lift(elapsed/4200))
        canvas.alpha_composite(whole,(64,vertical))
        frames.append(canvas.convert('RGB'))
    frames[0].save(OUT/'lucius-transition-final.gif',save_all=True,append_images=frames[1:],
                   duration=[80 if i%3!=2 else 90 for i in range(len(frames))],loop=0,disposal=2)
    contact=Image.new('RGB',(4*384,2*560),(25,28,32))
    draw=ImageDraw.Draw(contact)
    for i,figure in enumerate(figures):
        x,y,w,h=figure['sourceRectTopLeft'];whole=image.crop((x,y,x+w,y+h))
        column,row=i%4,i//4
        contact.paste(whole,(column*384,row*560+30),whole)
        draw.text((column*384+12,row*560+7),f'{i+1}. {phases[i]}',fill='white')
    contact.save(OUT/'contact-final.png')
    report={'schemaVersion':1,'status':'design-approved-temporal-preview-pending',
            'approval':'User 2026-11-14: boa com lucius pode manter essa curti',
            'image':SOURCE.relative_to(ROOT).as_posix(),'imageSha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
            'sourceSize':list(image.size),'frameCount':8,'durationMs':4200,'reviewHoldMs':1050,
            'unityImported':False,'runtimeVerified':False,
            'method':'Eight generated complete temporal poses; continuous presentation lift matches prototype envelope. Not a Unity runtime recording.',
            'pixelsModified':False,'wholeFigures':True,'frames':figures,
            'limits':['Single fixed frontal three-quarter view; not eight directions.',
                      'GIF quantizes colours; original PNG alpha and pixels are authoritative.',
                      'Artwork approved; new temporal sequence is a candidate pending review.']}
    (BASE/'candidates/lucius/transition-final.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    html='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>Lucius — revisão da transição</title>
<style>body{background:#191c20;color:#eee;font:16px system-ui;margin:24px}canvas{display:block;image-rendering:pixelated;max-height:75vh;width:auto}button,select,input{margin:8px}input{width:320px}</style>
<p>Lucius: desenho aprovado; prévia temporal candidata, sem importação no Unity.</p>
<button id="play">Pausar</button><button id="prev">Pose anterior</button><button id="next">Próxima pose</button>
<label>Velocidade<select id="speed"><option>.5</option><option selected>1</option><option>2</option></select></label>
<input id="time" type="range" min="0" max="4200" value="0"><span id="label"></span><canvas width="512" height="672"></canvas>
<script>const figures=FIGURES,canvas=document.querySelector('canvas'),ctx=canvas.getContext('2d'),image=new Image();
image.src='../../candidates/lucius/LuciusNormalToArconte-v3-approved-design-source.png';let playing=true,time=0,last=0;
const slider=document.querySelector('#time'),label=document.querySelector('#label');
function smooth(x){x=Math.max(0,Math.min(1,x));return x*x*(3-2*x)}
function lift(p){return p<.12?0:p<.34?smooth((p-.12)/.22):p<.76?1:1-smooth((p-.76)/.24)}
function pose(){let elapsed=0;for(let i=0;i<8;i++){elapsed+=figures[i].durationMs;if(time<elapsed)return i}return 7}
function draw(){let f=figures[pose()],r=f.sourceRectTopLeft;ctx.fillStyle='#191c20';ctx.fillRect(0,0,512,672);ctx.imageSmoothingEnabled=false;
ctx.drawImage(image,...r,64,Math.round(600-f.supportAnchorInCell[1]-.62*152*lift(time/4200)),384,512);slider.value=time;label.textContent=f.phase+' — '+Math.round(time)+' ms'}
function pause(){playing=false;document.querySelector('#play').textContent='Reproduzir'}
document.querySelector('#play').onclick=()=>{playing=!playing;document.querySelector('#play').textContent=playing?'Pausar':'Reproduzir'};
slider.oninput=()=>{pause();time=+slider.value;draw()};
function step(delta){pause();let index=Math.max(0,Math.min(7,pose()+delta));time=figures.slice(0,index).reduce((s,f)=>s+f.durationMs,0);draw()}
document.querySelector('#prev').onclick=()=>step(-1);document.querySelector('#next').onclick=()=>step(1);
function tick(now){if(last&&playing){time=(time+(now-last)*+document.querySelector('#speed').value)%5250;if(time>4200)time=4200}last=now;if(image.complete&&image.naturalWidth)draw();requestAnimationFrame(tick)}requestAnimationFrame(tick);
</script></html>'''.replace('FIGURES',json.dumps(figures))
    # Playback uses a separate clock so the final review hold can loop correctly.
    html=html.replace('playing=true,time=0,last=0','playing=true,time=0,clock=0,last=0').replace("time=(time+(now-last)*+document.querySelector('#speed').value)%5250;if(time>4200)time=4200", "clock=(clock+(now-last)*+document.querySelector('#speed').value)%5250;time=Math.min(4200,clock)")
    (OUT/'preview.html').write_text(html,encoding='utf-8')
    print(json.dumps({'figures':8,'previewFrames':len(frames),'durationMs':4200,'unityImported':False}))


if __name__=='__main__':main()
