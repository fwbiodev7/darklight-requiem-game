'use strict';
const WIDTH=640, HEIGHT=480, PIVOT=260, FOOT=450;
const REFERENCE_WIDTH=320;
// Each rectangle is a complete original figure, including its head, body and weapon.
// No reference image is rewritten, masked, assembled or scaled for this comparison.
const referenceFrames={Requiem:{rect:[0,0,289,312],foot:290},Rock:{rect:[0,312,289,624],foot:612}};
const directionNames={S:'Frente',SW:'Frente/esquerda',W:'Esquerda',NW:'Costas/esquerda',N:'Costas',NE:'Costas/direita',E:'Direita',SE:'Frente/direita'};
const weaponNames={katana:'Katana',adagas:'Adagas',corrente:'Corrente e foice',cavaleiro:'Cavaleiro do Abismo'};
const formNames={normal:'Normal',vermelha:'Vermelha',roxa:'Roxa',branca:'Branca'};
const colors={normal:['231,30,65','255,231,219'],vermelha:['255,30,65','255,231,221'],roxa:['170,55,255','248,232,255'],branca:['211,220,235','255,255,255']};
const comboNames={katana:'Rasante → ascendente → finalizador',adagas:'Direita → esquerda → corte em X → execução dupla',corrente:'Varredura → gancho → colheita',cavaleiro:'Serra direita → esquerda → esmagamento'};
const states=['Requiem','Rock'].map(skin=>({skin,canvas:document.getElementById(skin.toLowerCase()),reference:document.getElementById(skin.toLowerCase()+'-reference'),label:document.getElementById(skin.toLowerCase()+'-frame'),page:null,token:0}));
let pages=[],sheets=new Map(),images=new Map(),prepared=new WeakMap();
let mode='attack',direction='S',weapon='katana',form='normal',variant='melee',effects=true,paused=false,speed=1;
let started=performance.now(),elapsed=0,transition=null;
const status=document.querySelector('.status'), pauseButton=document.getElementById('pause');
const clock=(now=performance.now())=>elapsed+(paused?0:(now-started)*speed);
function reset(){elapsed=0;started=performance.now();}
function pause(value){elapsed=clock();started=performance.now();paused=value;pauseButton.textContent=paused?'Continuar':'Pausar';pauseButton.setAttribute('aria-pressed',String(paused));}
function image(source){
  if(typeof source!=='string'||!source)throw new Error('Caminho de fonte ausente no manifesto');
  if(!images.has(source))images.set(source,(async()=>{
    const im=new Image();im.src=source;
    try{await im.decode();}catch{throw new Error('Fonte ausente ou inválida: '+source);}
    return im;
  })());return images.get(source);
}
function baseline(ctx,width){
  ctx.save();ctx.strokeStyle='#657085';ctx.lineWidth=1;ctx.setLineDash([5,5]);
  ctx.beginPath();ctx.moveTo(0,FOOT+.5);ctx.lineTo(width,FOOT+.5);ctx.stroke();
  ctx.fillStyle='#aeb7c7';ctx.font='11px system-ui,sans-serif';ctx.fillText('Base dos pés',9,470);ctx.restore();
}
async function loadReferences(){
  const source=await image('referencia-original.png');
  if(source.naturalWidth!==289||source.naturalHeight!==624)throw new Error('Dimensões inesperadas da referência original: '+source.naturalWidth+' × '+source.naturalHeight);
  for(const state of states){
    const ctx=state.reference.getContext('2d'),reference=referenceFrames[state.skin];
    const [x0,y0,x1,y1]=reference.rect,w=x1-x0,h=y1-y0;
    ctx.imageSmoothingEnabled=false;ctx.clearRect(0,0,REFERENCE_WIDTH,HEIGHT);
    ctx.drawImage(source,x0,y0,w,h,Math.floor((REFERENCE_WIDTH-w)/2),FOOT-(reference.foot-y0),w,h);
    baseline(ctx,REFERENCE_WIDTH);
  }
}
function findPage(skin,w=weapon,f=form,k=mode==='walk'?'walk':mode==='idle'?'idle':'attack',d=direction){
  const exact=pages.findLast(p=>p.skin===skin&&p.weapon===w&&p.form===f&&p.kind===k&&p.directions?.[d]);
  if(exact||k!=='idle')return exact;
  // Keep the selected weapon and form: use its intact guard before its walk pose.
  return pages.findLast(p=>p.skin===skin&&p.weapon===w&&p.form===f&&p.kind==='attack'&&p.directions?.S)
    ??pages.findLast(p=>p.skin===skin&&p.weapon===w&&p.form===f&&p.kind==='walk'&&p.directions?.S);
}
function correctSkinPixel(data,index,shift){
  const r=data[index],g=data[index+1],b=data[index+2];
  if(data[index+3]>=166&&r>200&&g>75&&g<205&&b>35&&b<165&&r>g*1.25&&g>b*1.2){
    data[index]=Math.max(0,Math.min(255,Math.round(r+shift[0])));
    data[index+1]=Math.max(0,Math.min(255,Math.round(g+shift[1])));
    data[index+2]=Math.max(0,Math.min(255,Math.round(b+shift[2])));
  }
}
async function prepare(page){
  if(prepared.has(page))return prepared.get(page);
  const work=(async()=>{
    const result=[];
    for(const frame of page.frames){
      const sheet=sheets.get(frame.sheet);if(!sheet)throw new Error('Fonte ausente: '+frame.sheet);
      const [source,owner]=await Promise.all([image(sheet.file),image(sheet.ownerFile)]);
      const [x0,y0,x1,y1]=frame.rect,w=x1-x0,h=y1-y0,scale=frame.renderScale,pivot=frame.pivot??frame.walkPivot;
      if(!Number.isFinite(scale)||scale<=0||!Array.isArray(pivot)||pivot.length!==2||!pivot.every(Number.isFinite))throw new Error('Escala ou pivô ausente no quadro: '+frame.sheet+' / '+frame.index);
      if(w<=0||h<=0||x0<0||y0<0||x1>source.naturalWidth||y1>source.naturalHeight||owner.naturalWidth!==source.naturalWidth||owner.naturalHeight!==source.naturalHeight)throw new Error('Limites de quadro ou máscara inválidos: '+frame.sheet+' / '+frame.index);
      const canvas=document.createElement('canvas');canvas.width=w;canvas.height=h;const ctx=canvas.getContext('2d',{willReadFrequently:true});
      ctx.drawImage(owner,x0,y0,w,h,0,0,w,h);const labels=ctx.getImageData(0,0,w,h).data;
      ctx.clearRect(0,0,w,h);ctx.drawImage(source,x0,y0,w,h,0,0,w,h);const data=ctx.getImageData(0,0,w,h);
      if(!sheet.skinPalette?.shiftRgb)throw new Error('Paleta da pele ausente: '+frame.sheet);
      for(let i=0;i<labels.length;i+=4){
        if(labels[i]!==frame.ownerId)data.data[i+3]=0;
        else correctSkinPixel(data.data,i,sheet.skinPalette.shiftRgb);
      }
      ctx.putImageData(data,0,0);
      // Only the declared art calibration is applied. Weapon/viewport bounds never resize a figure.
      const point=p=>[PIVOT+(p[0]-pivot[0])*scale,FOOT+(p[1]-pivot[1])*scale];
      const anchors=frame.weaponAnchors??(frame.bladeTip?[{root:frame.bladeRoot,tip:frame.bladeTip}]:[]);
      result.push({canvas,x:PIVOT-(pivot[0]-x0)*scale,y:FOOT-(pivot[1]-y0)*scale,w:w*scale,h:h*scale,scale,
        anchors:anchors.filter(a=>a.root&&a.tip).map(a=>({root:point(a.root),tip:point(a.tip),center:a.center?point(a.center):null}))});
    }
    return result;
  })();prepared.set(page,work);return work;
}
function timeline(page){
  let indices=mode==='idle'?[page.directions.S[0]]:page.directions[direction];
  let durations=mode==='idle'?[2600]:page.durationsMs;
  const selected=mode==='attack'&&page.variants?.[variant];
  if(selected){indices=selected.indices;durations=selected.durationsMs;}
  let total=0;const phases=indices.map((frame,i)=>{const p={frame,start:total,duration:durations[i]};total+=p.duration;return p;});
  const events=mode==='attack'?(selected?.events??page.events??[]):[];
  if(phases.some(p=>!page.frames[p.frame]||!Number.isFinite(p.duration)||p.duration<=0))throw new Error('Sequência de quadros inválida: '+page.skin+' / '+page.weapon);
  if(events.some(e=>![e.start,e.end,e.impact].every(i=>Number.isInteger(i)&&phases[i])||e.start>e.end||e.impact<e.start||e.impact>e.end))throw new Error('Impacto fora da sequência: '+page.skin+' / '+page.weapon);
  return{phases,motion:total,duration:total+(mode==='attack'?(page.restMs??470):0),active:selected?.activePhases??page.activePhases??[],events};
}
function phaseAt(line,time){const t=Math.max(0,time)%line.duration;let index=line.phases.findIndex(p=>t<p.start+p.duration);if(index<0)index=line.phases.length-1;return{index,t,resting:t>=line.motion};}
function availableWalkDirections(w=weapon,f=form){
  const skins=w==='cavaleiro'?['Rock']:['Requiem','Rock'];
  return Object.keys(directionNames).filter(d=>skins.every(skin=>{
    const page=pages.findLast(p=>p.skin===skin&&p.weapon===w&&p.form===f&&p.kind==='walk'&&p.directions?.[d]);
    const cycle=page?.directions[d];
    return cycle?.length>1&&cycle.every(index=>page.frames[index]);
  }));
}
function syncDirectionAvailability(){
  const select=document.getElementById('direction'),note=document.getElementById('direction-note');
  const available=availableWalkDirections(),priorDirection=direction;
  const walkButton=document.querySelector('[data-mode="walk"]');
  walkButton.disabled=!available.length;
  walkButton.title=available.length?'':'Caminhada desta arma em revisão.';
  select.disabled=!available.length;
  if(mode==='walk'&&available.length&&!available.includes(direction)){
    direction=available.includes('S')?'S':available[0];select.value=direction;
  }
  for(const option of select.options)option.disabled=mode==='walk'?!available.includes(option.value):option.value!=='S';
  let text='';
  if(!available.length)text='Caminhada com '+weaponNames[weapon].toLowerCase()+' em revisão; a seleção de arma é preservada.';
  if(mode==='walk'){
    if(available.length===1&&available[0]==='S')text='Caminhada frontal; demais direções em revisão.';
    else if(!available.length)text='Nenhum ciclo de caminhada disponível nesta seleção para as skins exibidas.';
    else if(available.length<8)text='Caminhada disponível: '+available.map(d=>directionNames[d].toLowerCase()).join(', ')+'. Demais direções em revisão.';
    else text='Oito direções disponíveis nesta forma, em revisão visual.';
    if(direction!==priorDirection)text+=' A direção anterior não está disponível nesta forma; exibindo '+directionNames[direction].toLowerCase()+'.';
  }
  note.textContent=text;note.hidden=!text;
}
async function refresh(){
  transition=null;reset();
  delete status.dataset.error;
  syncDirectionAvailability();
  document.documentElement.style.setProperty('--accent','rgb('+colors[form][0]+')');
  for(const button of document.querySelectorAll('[data-mode]'))button.setAttribute('aria-pressed',String(button.dataset.mode===mode));
  const work=states.map(async state=>{
    const token=++state.token,page=findPage(state.skin);state.page=null;
    state.canvas.closest('section').hidden=weapon==='cavaleiro'&&state.skin==='Requiem';
    if(!page){
      if(weapon==='cavaleiro'&&state.skin==='Requiem'){state.label.textContent='Cavaleiro pertence à skin Rock.';return;}
      state.label.textContent='Fonte desta combinação ausente.';
      throw new Error('Combinação ausente no manifesto: '+state.skin+' / '+weapon+' / '+form+' / '+mode+' / '+direction);
    }
    state.label.textContent='Carregando…';const frames=await prepare(page);if(state.token!==token)return;
    state.page=page;state.frames=frames;state.line=timeline(page);
  });
  await Promise.all(work);reset();
  const frontOnly=mode==='attack'?' Ataques desta prévia usam a frente.':'';
  const neutralNote=mode==='idle'&&states.some(s=>s.page?.kind==='walk')?' Repouso usa o primeiro quadro inteiro da caminhada disponível.':'';
  status.textContent=weaponNames[weapon]+' · '+formNames[form]+'. '+(mode==='walk'?'Caminhada da katana.':mode==='idle'?'Repouso.':variant==='moto'&&weapon==='cavaleiro'?'Montagem → arrancada → frenagem.':comboNames[weapon]+'.')+frontOnly+neutralNote+' Prévia fora do jogo; seleções ainda em revisão visual.';
  document.getElementById('variant-control').hidden=weapon!=='cavaleiro'||mode!=='attack';
}
function crescent(ctx,anchor,color,alpha,sign=1,power=1,sweep=1.25){
  const dx=anchor.tip[0]-anchor.root[0],dy=anchor.tip[1]-anchor.root[1],r=Math.hypot(dx,dy);if(r<12)return;
  const angle=Math.atan2(dy,dx),samples=Array.from({length:37},(_,i)=>{const u=i/36,a=angle+sign*sweep*(1-u),thickness=Math.sin(u*Math.PI)*Math.min(27,r*.2)*power;return{outer:[anchor.root[0]+Math.cos(a)*r,anchor.root[1]+Math.sin(a)*r],inner:[anchor.root[0]+Math.cos(a)*(r-thickness),anchor.root[1]+Math.sin(a)*(r-thickness)]};});
  ctx.save();ctx.globalAlpha=alpha;ctx.beginPath();ctx.moveTo(...samples[0].outer);samples.slice(1).forEach(p=>ctx.lineTo(...p.outer));samples.slice().reverse().forEach(p=>ctx.lineTo(...p.inner));ctx.closePath();ctx.fillStyle=`rgba(${color[0]},.38)`;ctx.fill();
  ctx.lineCap='round';ctx.shadowColor=`rgb(${color[0]})`;ctx.shadowBlur=9*power;for(let i=1;i<samples.length;i++){const strength=i/(samples.length-1);ctx.beginPath();ctx.moveTo(...samples[i-1].outer);ctx.lineTo(...samples[i].outer);ctx.strokeStyle=`rgba(${color[0]},${strength*.85})`;ctx.lineWidth=5*power;ctx.stroke();ctx.shadowBlur=0;ctx.strokeStyle=`rgba(${color[1]},${strength*.95})`;ctx.lineWidth=1.4*power;ctx.stroke();}ctx.restore();
}
function eventsAt(state,phase){return state.line.events.filter(e=>phase.t>=state.line.phases[e.start].start&&phase.t<=state.line.phases[e.end].start+state.line.phases[e.end].duration+85);}
function effectKind(state,event){
  if(['charge','launch','brake','mount'].includes(event.kind))return'moto';
  return event.kind==='serra'?'serra':state.page.weapon==='cavaleiro'?'serra':state.page.weapon;
}
function eventAnchors(pose,kind){return pose.anchors.slice(0,kind==='adagas'||kind==='serra'?2:1);}
function bladeStreak(ctx,anchor,color,alpha,power=1){
  ctx.save();ctx.globalAlpha=alpha;ctx.lineCap='round';ctx.shadowColor=`rgb(${color[0]})`;ctx.shadowBlur=8;
  ctx.beginPath();ctx.moveTo(...anchor.root);ctx.lineTo(...anchor.tip);ctx.strokeStyle=`rgba(${color[0]},.8)`;ctx.lineWidth=4*power;ctx.stroke();
  ctx.shadowBlur=0;ctx.strokeStyle=`rgba(${color[1]},.95)`;ctx.lineWidth=1.2;ctx.stroke();ctx.restore();
}
function chainSweep(ctx,anchor,color,alpha,sign,power){
  const dx=anchor.tip[0]-anchor.root[0],dy=anchor.tip[1]-anchor.root[1],radius=Math.hypot(dx,dy),angle=Math.atan2(dy,dx);
  if(radius<12)return;
  crescent(ctx,anchor,color,alpha*.6,sign,power*.65,2.15);
  ctx.save();ctx.globalAlpha=alpha;ctx.lineCap='round';ctx.setLineDash([4,8]);ctx.lineWidth=2.1;
  ctx.strokeStyle=`rgba(${color[1]},.75)`;ctx.beginPath();ctx.arc(...anchor.root,radius,angle+sign*2.15,angle,sign>0);ctx.stroke();
  ctx.setLineDash([]);ctx.beginPath();ctx.arc(...anchor.tip,4+power,0,Math.PI*2);ctx.fillStyle=`rgba(${color[0]},.8)`;ctx.fill();ctx.restore();
}
function wheel(ctx,anchor,color,alpha,time,power){
  if(!anchor.center)return;const center=anchor.center,r=Math.max(15,Math.hypot(anchor.tip[0]-center[0],anchor.tip[1]-center[1]));
  ctx.save();ctx.translate(...center);ctx.rotate(time*.035);ctx.globalAlpha=alpha;ctx.shadowColor=`rgb(${color[0]})`;ctx.shadowBlur=9;
  for(let i=0;i<9;i++){const a=i/9*Math.PI*2;ctx.beginPath();ctx.arc(0,0,r+3*power,a,a+.34);ctx.strokeStyle=`rgba(${color[0]},.8)`;ctx.lineWidth=3;ctx.stroke();ctx.beginPath();ctx.arc(0,0,r+5*power,a,a+.19);ctx.strokeStyle=`rgba(${color[1]},.95)`;ctx.lineWidth=1.2;ctx.stroke();}ctx.restore();
}
function motoTrail(ctx,anchor,color,alpha,time,event,progress){
  if(event.kind==='mount')return;
  const center=anchor.center??anchor.root,braking=event.kind==='brake'||event.stage==='brake'||(event.kind==='charge'&&progress>.76);
  ctx.save();ctx.globalAlpha=alpha;ctx.lineCap='round';
  if(braking){
    for(let i=0;i<13;i++){
      const angle=Math.PI*.15+i*.105,length=10+((time*.19+i*13)%34);
      ctx.beginPath();ctx.moveTo(...anchor.tip);ctx.lineTo(anchor.tip[0]-Math.cos(angle)*length,anchor.tip[1]+Math.sin(angle)*length*.45);
      ctx.strokeStyle=`rgba(${i%3?color[0]:color[1]},${.8-i*.035})`;ctx.lineWidth=i%3?1.5:1;ctx.stroke();
    }
  }else{
    for(let i=0;i<9;i++){
      const offset=(time*.75+i*29)%90,y=center[1]+(i-4)*7,length=28+progress*46;
      ctx.beginPath();ctx.moveTo(center[0]-12-offset,y);ctx.lineTo(center[0]-12-offset-length,y-1);
      ctx.strokeStyle=`rgba(${i%3?color[0]:color[1]},${.6-Math.abs(i-4)*.065})`;ctx.lineWidth=i%3?2:1;ctx.stroke();
    }
  }
  ctx.restore();
}
function effectsFor(ctx,state,phase,frame){
  if(!effects||mode!=='attack'||phase.resting)return;
  const color=colors[form];
  for(const event of eventsAt(state,phase)){
    const start=state.line.phases[event.start].start,end=state.line.phases[event.end].start+state.line.phases[event.end].duration;
    const fade=phase.t<=end?1:Math.max(0,1-(phase.t-end)/85),progress=Math.min(1,(phase.t-start)/(end-start)),power=event.power??1,sign=event.sign??1;
    const pose=phase.index>event.end?state.frames[state.line.phases[event.end].frame]:frame;
    const kind=effectKind(state,event);
    eventAnchors(pose,kind).forEach((anchor,i)=>{
      if(kind==='moto')motoTrail(ctx,anchor,color,fade,phase.t,event,progress);
      else if(kind==='serra'){wheel(ctx,anchor,color,fade,phase.t,power);crescent(ctx,anchor,color,fade*.7,i?-1:1,power,.85);}
      else if(kind==='corrente')chainSweep(ctx,anchor,color,fade,sign,power);
      else if(kind==='adagas'){crescent(ctx,anchor,color,fade,sign*(i?-1:1),power*.8,1.05);bladeStreak(ctx,anchor,color,fade*.8,.7);}
      else{crescent(ctx,anchor,color,fade,sign,power,2.05);crescent(ctx,anchor,color,fade*.3,sign,power*.42,2.35);bladeStreak(ctx,anchor,color,fade*.55);}
    });
  }
}
function impacts(ctx,state,phase){
  if(!effects||mode!=='attack'||phase.resting)return;
  const color=colors[form];
  for(const event of state.line.events){
    const age=phase.t-state.line.phases[event.impact].start;if(age<0||age>115)continue;
    const pose=state.frames[state.line.phases[event.impact].frame],kind=effectKind(state,event);if(kind==='moto')continue;
    const fade=1-age/115,power=event.power??1;
    eventAnchors(pose,kind).forEach(anchor=>{
      const tip=anchor.tip,angle=Math.atan2(tip[1]-anchor.root[1],tip[0]-anchor.root[0]);
      ctx.save();ctx.globalCompositeOperation='lighter';
      const radius=(8+age*.17)*power,glow=ctx.createRadialGradient(...tip,0,...tip,radius);glow.addColorStop(0,`rgba(${color[1]},${fade*.6})`);glow.addColorStop(.3,`rgba(${color[0]},${fade*.32})`);glow.addColorStop(1,`rgba(${color[0]},0)`);ctx.fillStyle=glow;ctx.fillRect(tip[0]-radius,tip[1]-radius,radius*2,radius*2);
      const count=kind==='adagas'?7:kind==='serra'?22:kind==='corrente'?10:12,lengthScale=kind==='adagas'?.65:kind==='serra'?1.6:1;
      for(let i=0;i<count;i++){const a=i/count*Math.PI*2+angle,distance=(5+age*(.22+(i%3)*.04))*power,length=(3+10*fade)*power*lengthScale;ctx.beginPath();ctx.moveTo(tip[0]+Math.cos(a)*distance,tip[1]+Math.sin(a)*distance);ctx.lineTo(tip[0]+Math.cos(a)*(distance+length),tip[1]+Math.sin(a)*(distance+length));ctx.strokeStyle=`rgba(${i%3?color[0]:color[1]},${fade*.9})`;ctx.lineWidth=i%3?1.5:1;ctx.stroke();}
      if(kind==='serra'||kind==='corrente'){ctx.beginPath();ctx.ellipse(...tip,radius,radius*(kind==='serra'?.7:.35),angle,0,Math.PI*2);ctx.strokeStyle=`rgba(${color[1]},${fade*.65})`;ctx.lineWidth=kind==='serra'?2:1;ctx.stroke();}
      ctx.restore();
    });
  }
}
function drawFigure(ctx,frame,alpha=1){ctx.save();ctx.globalAlpha=alpha;ctx.drawImage(frame.canvas,Math.round(frame.x),Math.round(frame.y),frame.w,frame.h);ctx.restore();}
function draw(now){
  const time=clock(now);
  for(const state of states){
    const ctx=state.canvas.getContext('2d');ctx.clearRect(0,0,WIDTH,HEIGHT);ctx.imageSmoothingEnabled=false;baseline(ctx,WIDTH);
    if(transition?.ready){
      const pair=transition.pairs.get(state.skin);if(!pair)continue;
      const p=Math.min(1,(time-transition.start)/900),mix=p*p*(3-2*p);
      drawFigure(ctx,pair.before,1-mix);drawFigure(ctx,pair.after,mix);
      if(effects){ctx.save();const pulse=Math.sin(p*Math.PI);ctx.strokeStyle=`rgba(${colors[form][0]},${pulse*.6})`;ctx.lineWidth=2;ctx.beginPath();ctx.ellipse(PIVOT,FOOT-3,42+95*p,9+17*p,0,0,Math.PI*2);ctx.stroke();ctx.restore();}
      state.label.textContent='Transformação · '+Math.round(p*100)+'%';continue;
    }
    if(!state.page)continue;
    const phase=phaseAt(state.line,time),frame=state.frames[state.line.phases[phase.index].frame];
    // A brief horizontal recoil is confined to the impact. No zoom, squash or rescale.
    let kick=0;
    if(effects&&mode==='attack')for(const event of state.line.events){const age=phase.t-state.line.phases[event.impact].start;if(age>=0&&age<65)kick+=Math.sin(age*.24)*(event.power??1)*3*(1-age/65);}
    kick=Math.max(-3,Math.min(3,kick));
    ctx.save();ctx.translate(kick,0);effectsFor(ctx,state,phase,frame);drawFigure(ctx,frame);impacts(ctx,state,phase);ctx.restore();
    const action=mode==='walk'?'Caminhada · '+directionNames[direction]:mode==='attack'?(variant==='moto'&&weapon==='cavaleiro'?'Investida':'Combo frontal'):state.page.kind==='attack'?'Repouso · guarda da arma':state.page.kind==='walk'?'Repouso · quadro inicial da caminhada de '+weaponNames[state.page.weapon]:'Repouso · pose neutra';
    state.label.textContent=action+' · '+(phase.index+1)+'/'+state.line.phases.length+' · '+formNames[form];
  }
  if(transition?.ready&&time-transition.start>=900){transition=null;reset();}
  requestAnimationFrame(draw);
}
document.querySelectorAll('[data-mode]').forEach(b=>b.addEventListener('click',()=>{
  if(b.dataset.mode==='walk'&&!availableWalkDirections().length)return;
  mode=b.dataset.mode;if(mode!=='walk'){direction='S';document.getElementById('direction').value='S';}
  refresh().catch(fail);
}));
document.getElementById('direction').addEventListener('change',e=>{if(!availableWalkDirections().includes(e.target.value))return;direction=e.target.value;mode='walk';refresh().catch(fail);});
document.getElementById('weapon').addEventListener('change',e=>{weapon=e.target.value;mode='attack';direction='S';document.getElementById('direction').value='S';refresh().catch(fail);});
document.getElementById('form').addEventListener('change',e=>{form=e.target.value;refresh().catch(fail);});
document.getElementById('variant').addEventListener('change',e=>{variant=e.target.value;refresh().catch(fail);});
pauseButton.addEventListener('click',()=>pause(!paused));
document.getElementById('effects').addEventListener('click',e=>{effects=!effects;e.target.setAttribute('aria-pressed',String(effects));e.target.textContent=effects?'Cortes: ligados':'Cortes: desligados';});
document.getElementById('slow').addEventListener('click',e=>{elapsed=clock();started=performance.now();speed=speed===1?.5:1;e.target.setAttribute('aria-pressed',String(speed===.5));e.target.textContent=speed===.5?'Velocidade: ½':'Câmera lenta';});
document.getElementById('next').addEventListener('click',()=>{pause(true);if(transition){elapsed=Math.min(transition.start+900,clock()+75);return;}const state=states.find(s=>s.page);if(!state)return;const phase=phaseAt(state.line,clock());elapsed=state.line.phases[(phase.index+1)%state.line.phases.length].start+.01;});
document.getElementById('transform').addEventListener('click',async()=>{
  try{
    if(form==='normal'){form='vermelha';document.getElementById('form').value=form;}
    mode='idle';direction='S';document.getElementById('direction').value='S';await refresh();
    const pairs=new Map();
    for(const state of states){const before=findPage(state.skin,weapon,'normal','attack','S'),after=findPage(state.skin,weapon,form,'attack','S');if(!before||!after)continue;const [a,b]=await Promise.all([prepare(before),prepare(after)]);pairs.set(state.skin,{before:a[0],after:b[0]});}
    if(pairs.size)transition={ready:true,start:clock(),pairs};
  }catch(error){fail(error);}
});
function fail(error){console.error(error);status.dataset.error='true';status.textContent='Falha na prévia: '+error.message+'. Nenhuma imagem substituta foi usada.';}
Promise.all([loadReferences(),fetch('pacote-v6/manifesto.json',{cache:'no-store'}).then(r=>{if(!r.ok)throw new Error('Manifesto '+r.status);return r.json();})]).then(async([,data])=>{
  if(!Array.isArray(data.pages)||!Array.isArray(data.sheets))throw new Error('Manifesto sem páginas ou fontes');
  pages=data.pages;sheets=new Map(data.sheets.map(s=>[s.file,s]));await refresh();
}).catch(fail);
requestAnimationFrame(draw);
