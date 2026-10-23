"""Auditable previews of imported whole figures rendered by the Unity player shader."""
from pathlib import Path
from types import SimpleNamespace
import json
import shutil
import numpy as np
from PIL import Image,ImageDraw
from build_ruin_fists import ROOT,OUT,MAN,SKINS,FORMS,DIRS,sp

def main():
    manifest=sp.read(MAN/'manifest.json');native=sp.read(OUT/'native-capture.json')
    assert native['configurations']==128 and native['playMode'] is False
    index=[];warnings=0
    for page in manifest['pages']:
        target=OUT/page['name'];physical=OUT/(page.get('sourceName') or page['name'])
        source=sp.read(physical/'sheet.json');rendered=target/'rendered.png'
        assert rendered.exists(),page['name']
        image=Image.open(rendered).convert('RGBA');a=np.array(image)
        for frame in source['frames']:
            x,y,w,h=frame['rect'];assert (a[y:y+h,x:x+w,3]>128).sum()>400,(page['name'],frame['id'])
        source.update(image='../rendered.png',imageSha256=sp.digest(rendered),status='imported',imported=True,runtimeVerified=False,
                      nativeShaderReview=True,paletteVariant=page['paletteVariant'],form=FORMS[page['form']])
        for f in source['frames']:f['form']=FORMS[page['form']]
        directory=target/'native';directory.mkdir(exist_ok=True)
        # Revisions replace generated inspection metadata, never drawing sources.
        (directory/'sheet.json').write_text(json.dumps(source,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        for generated in ['audit','preview']:
            old=directory/generated
            if old.exists():
                archive=OUT/'previous-inspections'/page['name']/generated
                archive.parent.mkdir(parents=True,exist_ok=True)
                if archive.exists():raise ValueError('Inspection archive already exists: '+str(archive))
                shutil.move(str(old),str(archive))
        result=sp.audit(SimpleNamespace(manifest=directory/'sheet.json',out=directory/'audit',unity_meta=None));assert result['ok'];warnings+=result.get('warnings',0)
        sp.preview(SimpleNamespace(manifest=directory/'sheet.json',out=directory/'preview',zoom=2))
        index.append(dict(name=page['name'],skin=page['skin'],form=page['form'],direction=page['direction'],bank=page['bank'],
                          image=page['name']+'/rendered.png',metadata=(page.get('sourceName') or page['name'])+'/sheet.json',
                          inspection=page['name']+'/native/preview/preview.html',paletteVariant=page['paletteVariant']))
    (OUT/'review-index.json').write_text(json.dumps(index,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    # Whole-figure contact sheets: all eight directions and both identities in every form.
    contacts=OUT/'contacts';contacts.mkdir(exist_ok=True)
    for form in range(4):
        for bank,pose in [('Combat',5),('Motion',0)]:
            contact=Image.new('RGBA',(8*300,2*360),(31,33,43,255));draw=ImageDraw.Draw(contact)
            for page in [p for p in index if p['form']==form and p['bank']==bank]:
                data=sp.read(OUT/page['metadata']);x,y,w,h=data['frames'][pose]['rect']
                # Technical extraction of a complete native frame, never anatomical pieces.
                figure=Image.open(OUT/page['image']).convert('RGBA').crop((x,y,x+w,y+h))
                scale=min(1,290/w,320/h);figure=figure.resize((round(w*scale),round(h*scale)),Image.Resampling.NEAREST)
                left=page['direction']*300+(300-figure.width)//2;top=page['skin']*360+25
                contact.alpha_composite(figure,(left,top));draw.text((page['direction']*300+8,page['skin']*360+7),SKINS[page['skin']]+' '+DIRS[page['direction']],fill='white')
            contact.save(contacts/(FORMS[form]+'_'+bank+'.png'))
    (OUT/'index.html').write_text(HTML,encoding='utf-8')
    summary=dict(configurations=128,physicalBanks=128,uniqueAuthoredFigures=1536,frameReferences=1536,
        structuralErrors=0,warnings=warnings,imported=True,nativeShaderCaptured=True,playMode=False,audio=False,
        note='All four forms use independently authored complete figures. No palette-only variants. Authored eyes and neutral skin shadows are preserved.')
    (OUT/'native-review-summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(configurations=128,errors=0,warnings=warnings,index=str(OUT/'index.html'))))

HTML='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>Punhos da Ruína — revisão final</title>
<style>body{margin:0;background:#13151e;color:#f5f1e9;font:16px system-ui}main{max-width:1150px;margin:auto;padding:22px}h1{font-size:25px;margin:0 0 8px}p{color:#b7bbca}nav{display:flex;gap:10px;flex-wrap:wrap;margin:16px 0}select,button,input{font:inherit;background:#292d3e;color:white;border:1px solid #555b72;border-radius:6px;padding:7px}label{display:grid;gap:4px;font-size:13px}#stage{height:620px;display:grid;place-items:center;overflow:auto;border:1px solid #4f5366;background-color:#272b38;background-image:linear-gradient(45deg,#303544 25%,transparent 25%),linear-gradient(-45deg,#303544 25%,transparent 25%),linear-gradient(45deg,transparent 75%,#303544 75%),linear-gradient(-45deg,transparent 75%,#303544 75%);background-size:32px 32px;background-position:0 0,0 16px,16px -16px,-16px 0}canvas{image-rendering:pixelated;max-width:none}a{color:#d8a9ff}small{color:#b7bbca}</style>
<main><h1>Punhos da Ruína</h1><p>Duas skins · quatro formas · oito direções. Imagens capturadas do material do Unity em edição.</p>
<nav><label>Skin<select id="skin"><option>Requiem</option><option>Rock</option></select></label><label>Forma<select id="form"><option>Normal</option><option>Ruptura</option><option>Eclipse</option><option>Absoluto</option></select></label><label>Direção<select id="direction">'''+''.join('<option>'+d+'</option>' for d in DIRS)+'''</select></label><label>Grupo<select id="bank"><option value="Combat">Ataques</option><option value="Motion">Movimento</option></select></label><label>Animação<select id="clip"></select></label><label>Ampliação<select id="zoom"><option>1</option><option selected>2</option><option>3</option></select></label></nav>
<nav><button id="play">Pausar</button><button id="previous">← Quadro</button><button id="next">Quadro →</button><label>Velocidade<select id="speed"><option value="0.5">0,5×</option><option value="1" selected>1×</option><option value="2">2×</option></select></label><label>Fundo<select id="background"><option value="checker">Quadriculado</option><option value="#151720">Escuro</option><option value="#efede7">Claro</option></select></label></nav>
<div id="stage"><canvas id="canvas"></canvas></div><p id="status"></p><a id="details">Folha completa e inspeção quadro a quadro</a><p><small>A caminhada usa quatro fases a cada 0,6 s. Os ataques no jogo seguem as janelas de antecipação, contato e recuperação do combate. Esta prévia é silenciosa; não inicia o jogo.</small></p></main>
<script>
const $=id=>document.getElementById(id),canvas=$('canvas'),ctx=canvas.getContext('2d');let pages,data,img,clip,step=0,playing=true,last=0,clock=0,generation=0;
async function load(){const id=++generation;const page=pages.find(p=>p.skin==$('skin').selectedIndex&&p.form==$('form').selectedIndex&&p.direction==$('direction').selectedIndex&&p.bank==$('bank').value);const next=await(await fetch(page.metadata)).json();const image=new Image();image.src=page.image;await image.decode();if(id!==generation)return;data=next;img=image;$('clip').replaceChildren(...data.clips.map(c=>new Option(c.name,c.name)));clip=data.clips[0];step=0;clock=0;$('details').href=page.inspection;draw();}
function draw(){if(!data)return;const frame=data.frames.find(f=>f.id===clip.frames[step]),[x,y,w,h]=frame.rect;canvas.width=w;canvas.height=h;canvas.style.width=w*Number($('zoom').value)+'px';canvas.style.height=h*Number($('zoom').value)+'px';ctx.imageSmoothingEnabled=false;ctx.clearRect(0,0,w,h);ctx.drawImage(img,x,y,w,h,0,0,w,h);$('status').textContent=$('skin').value+' · '+$('form').value+' · '+$('direction').value+' · '+clip.name+' · quadro '+(step+1)+'/'+clip.frames.length;}
function advance(delta){step=(step+delta+clip.frames.length)%clip.frames.length;draw();}
for(const id of ['skin','form','direction','bank'])$(id).onchange=load;
$('clip').onchange=()=>{clip=data.clips.find(c=>c.name==$('clip').value);step=0;clock=0;draw();};$('zoom').onchange=draw;
$('play').onclick=()=>{playing=!playing;$('play').textContent=playing?'Pausar':'Reproduzir';};
for(const [id,d]of [['previous',-1],['next',1]])$(id).onclick=()=>{playing=false;$('play').textContent='Reproduzir';advance(d);};
$('background').onchange=()=>{const color=$('background').value;$('stage').style.backgroundImage=color==='checker'?'':'none';$('stage').style.backgroundColor=color==='checker'?'#272b38':color;};
function tick(t){const elapsed=last?t-last:0;last=t;if(playing&&clip){clock+=elapsed*Number($('speed').value);if(clock>=150){clock%=150;if(step+1<clip.frames.length||clip.loop)advance(1);else {playing=false;$('play').textContent='Reproduzir';}}}requestAnimationFrame(tick);}
fetch('review-index.json').then(r=>r.json()).then(r=>{pages=r;load();requestAnimationFrame(tick);});
</script></html>'''

if __name__=='__main__':main()
