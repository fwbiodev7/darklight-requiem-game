'use strict';
const { chromium }=require('C:/Users/Usuario/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
(async()=>{
 const out=path.resolve('outputs/previa-armas-20261025/browser-qa');fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launchPersistentContext(path.resolve('work/session-20261025/weapon-viewer-browser-profile'),{headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1440,height:1200}}),errors=[],failed=[],checks=[];
 page.on('pageerror',e=>errors.push(e.message));page.on('response',r=>{if(r.status()>=400)failed.push(r.status()+' '+r.url());});
 await page.addInitScript(()=>{const original=CanvasRenderingContext2D.prototype.drawImage;CanvasRenderingContext2D.prototype.drawImage=function(...args){if(this.canvas.isConnected&&args.length===5)this.canvas.lastWorldDraw=args.slice(1);return original.apply(this,args);};});
 async function ready(){await page.waitForFunction(()=>window.weaponReview&&document.querySelector('#loading').textContent==='',{timeout:30000});}
 async function select(weapon,skin,form,direction,action){
  await page.locator(`[data-weapon="${weapon}"]`).click();await ready();
  if(weapon!==3)await page.locator('#skin').selectOption(String(skin));
  else assert.equal(await page.locator('#skin').inputValue(),'1');
  await page.locator('#form').selectOption(String(form));
  await page.locator('#direction').selectOption(String(direction));await page.locator('#action').selectOption(action);await ready();
 }
 async function worldScale(){
  const state=await page.evaluate(()=>{const s=window.weaponReview.state(),c=document.querySelector('canvas');return{...s,draw:c.lastWorldDraw,ratio:c.clientWidth/c.width};});
  assert.ok(state.draw,'Canvas did not draw a complete figure');
  assert.ok(Math.abs(state.draw[2]/(state.frame.rect[2]/state.frame.ppu)-145*1.325)<.001,'Viewer changed character scale to fit canvas');
  assert.ok(Math.abs(state.ratio-Number(await page.locator('#zoom').inputValue()))<.002,'Canvas CSS resized the character');
  return state;
 }
 try{
  await page.goto(process.env.DARKLIGHT_PREVIEW_URL||'http://127.0.0.1:8766/outputs/previa-armas-20261025/index.html');await ready();
  await page.locator('#play').click();assert.equal(await page.locator('#play').innerText(),'Reproduzir');
  for(let form=0;form<4;form++){
   const unique=new Set();
   for(let direction=0;direction<8;direction++){
    await select(3,1,form,direction,'RideActive');const state=await worldScale();
    assert.equal(state.trackLength,1);assert.equal(state.frame.presentation,'redesign');assert.equal(state.frame.mirror,false);
    assert.ok(!unique.has(state.frame.sourceName));unique.add(state.frame.sourceName);checks.push({weapon:3,form,direction,staticRide:true,worldScale:true});
   }
   await select(3,1,form,0,'Slam');assert.equal((await worldScale()).trackLength,4);
   for(let phase=0;phase<4;phase++){await page.locator('#restart').click();for(let i=0;i<phase;i++)await page.locator('#next').click();await ready();assert.equal((await worldScale()).frame.presentation,'redesign');}
   await page.screenshot({path:path.join(out,`motorcycle-form-${form}.png`),fullPage:true});
  }
  for(let direction=0;direction<8;direction++){
   for(const action of ['Idle','StandingPose','Attack:light1']){
    await select(0,0,0,direction,action);const state=await worldScale();assert.equal(state.frame.presentation,'redesign');
    assert.equal(state.trackLength,action==='StandingPose'?1:4);checks.push({weapon:0,direction,action,worldScale:true});
   }
  }
  await select(0,0,0,0,'Idle');await page.locator('#play').click();await page.waitForFunction(()=>window.weaponReview.state().step>0,{},{timeout:6000});await page.locator('#play').click();
  await page.screenshot({path:path.join(out,'katana-idle-south.png'),fullPage:true});
  await page.setViewportSize({width:390,height:844});await select(0,0,0,6,'StandingPose');await worldScale();
  assert.equal(await page.locator('#stage').evaluate(e=>getComputedStyle(e).overflowX),'auto');
  await page.screenshot({path:path.join(out,'mobile-fixed-scale.png'),fullPage:true});
  assert.deepEqual(errors,[]);assert.deepEqual(failed,[]);
  const report={ok:true,checks:checks.length,errors,failed,motorcycleOwnViews:32,frontalSlamPhases:16,fixedWorldScale:true,animationPlaybackObserved:true,audio:false,unityExecution:false};
  fs.writeFileSync(path.join(out,'report.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report));
 }finally{await browser.close();}
})().catch(error=>{console.error(error.stack);process.exitCode=1;});
