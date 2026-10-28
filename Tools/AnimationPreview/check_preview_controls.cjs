'use strict';
// Execute the actual local frontend logic with a small DOM/clock mock.
// No browser automation, Unity, audio, saves, network or generated image edits.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'outputs/fidelidade-original-20261018'),pack=path.join(out,'pacote-v6');
const hash=s=>crypto.createHash('sha256').update(s).digest('hex');
const raw=fs.readFileSync(path.join(pack,'manifesto.json')),data=JSON.parse(raw),source=fs.readFileSync(path.join(out,'combat-preview.js'),'utf8');
let now=0;
function element(){return {textContent:'',disabled:false,hidden:false,value:'S',title:'',dataset:{},attrs:{},listeners:{},style:{setProperty(){}},setAttribute(k,v){this.attrs[k]=v},addEventListener(k,v){this.listeners[k]=v},closest(){return element()}};}
const elements=new Map(),modes=['idle','walk','attack'].map(mode=>Object.assign(element(),{dataset:{mode}}));
const get=id=>{if(!elements.has(id))elements.set(id,element());return elements.get(id)};
get('direction').options=['S','SW','W','NW','N','NE','E','SE'].map(value=>({value,disabled:false}));
const context=vm.createContext({console,Map,WeakMap,Math,Number,Object,Array,Error,performance:{now:()=>now},document:{getElementById:get,querySelector:s=>s==='.status'?get('status'):s==='[data-mode="walk"]'?modes[1]:null,querySelectorAll:()=>modes,documentElement:{style:{setProperty(){}}}},requestAnimationFrame(){}});
const marker='Promise.all([loadReferences(),fetch(';assert(source.includes(marker));
vm.runInContext(source.slice(0,source.lastIndexOf(marker))+`\nglobalThis.api={correctSkinPixel,timeline,phaseAt,availableWalkDirections,findPage,syncDirectionAvailability,clock,pause,reset,states,set(p,w,f,k,v='melee'){pages=p;weapon=w;form=f;mode=k;variant=v;direction='S'},setElapsed(v){elapsed=v},get(){return{weapon,form,mode,paused,speed,elapsed,transition}},transition(t){transition=t}};`,context);
const api=context.api;let timelines=0,steps=0,idles=0,variants=0,disabledWalkChecks=0;
for(const page of data.pages.filter(p=>p.kind==='attack')){
 for(const variant of page.variants?Object.keys(page.variants):['melee']){
  api.set(data.pages,page.weapon,page.form,'attack',variant);const line=api.timeline(page);timelines++;if(page.variants)variants++;
  assert(line.phases.length>1);assert.equal(line.events.length,page.weapon==='adagas'?4:3);
  assert.equal(api.phaseAt(line,line.duration+.01).index,0);
  assert(api.phaseAt(line,line.motion+.01).resting);
  api.states[0].page=page;api.states[0].line=line;api.states[1].page=null;
  for(let i=0;i<line.phases.length;i++){
   api.setElapsed(line.phases[i].start+.01);get('next').listeners.click();
   assert.equal(api.phaseAt(line,api.clock()).index,(i+1)%line.phases.length);steps++;
  }
 }
 api.set(data.pages,page.weapon,page.form,'idle');const idle=api.findPage(page.skin);
 assert.equal(idle.weapon,page.weapon);assert.equal(idle.form,page.form);assert.equal(api.timeline(idle).phases.length,1);idles++;
}
for(const form of ['normal','vermelha','roxa','branca'])for(const weapon of ['katana','adagas','corrente','cavaleiro']){
 api.set(data.pages,weapon,form,'attack');api.syncDirectionAvailability();
 const dirs=Array.from(api.availableWalkDirections());assert.deepEqual(dirs,weapon==='katana'?['S']:[]);
 assert.equal(modes[1].disabled,weapon!=='katana');assert.equal(get('direction').disabled,weapon!=='katana');disabledWalkChecks++;
}
api.set(data.pages,'katana','normal','attack');api.reset();api.pause(false);now=100;assert.equal(api.clock(),100);api.pause(true);now=200;assert.equal(api.clock(),100);
get('slow').listeners.click({target:get('slow')});api.pause(false);now=400;assert.equal(api.clock(),200);api.pause(true);
api.setElapsed(0);api.transition({start:0});for(let i=0;i<20;i++)get('next').listeners.click();assert.equal(api.clock(),900);
const palette=JSON.parse(fs.readFileSync(path.join(pack,'paleta-pele.json'),'utf8'));
for(const entry of palette.entries){
 const pixel=new Uint8ClampedArray([...entry.sourceMedian,255]);api.correctSkinPixel(pixel,0,entry.shiftRgb);
 assert.deepEqual(Array.from(pixel),[...palette.referenceMedian,255]);
 for(const rgb of [[145,110,75],[185,130,90],[245,30,50],[235,240,250],[150,30,220],[20,20,25]]){
  const untouched=new Uint8ClampedArray([...rgb,255]);api.correctSkinPixel(untouched,0,entry.shiftRgb);assert.deepEqual(Array.from(untouched),[...rgb,255]);
 }
 const transparent=new Uint8ClampedArray([250,150,90,0]);api.correctSkinPixel(transparent,0,entry.shiftRgb);assert.deepEqual(Array.from(transparent),[250,150,90,0]);
}
const report={date:'2026-10-19',method:'Actual combat-preview.js executed in Node vm with DOM and clock mocks. No browser, Unity gameplay, audio or controller.',manifestSha256:hash(raw),frontendSha256:hash(source),timelines,nextFrameSteps:steps,idleSelections:idles,knightVariantTimelines:variants,availabilitySelections:disabledWalkChecks,skinSources:palette.entries.length,referenceMedian:palette.referenceMedian,checks:['event bounds and hit counts','frame stepping and loop','rest interval','same weapon/form idle','only available walk directions','unavailable walks disabled without weapon substitution','pause and half speed','transition clamps at900ms','both skins/all forms skin medians match original Requiem','dark/brown hair, red/violet/silver weapons, clothing and alpha unchanged in control samples'],errors:0,visualApproval:false,productionImported:false};
fs.writeFileSync(path.join(pack,'QA-CONTROLES-20261019.json'),JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify(report));
