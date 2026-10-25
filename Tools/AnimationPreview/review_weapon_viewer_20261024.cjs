'use strict';
const {chromium}=require('C:/Users/Usuario/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
(async()=>{
 const out=path.resolve('outputs/previa-armas-20261024/browser-qa');fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launchPersistentContext(path.resolve('work/session-20261024/weapon-viewer-browser-profile'),{headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1440,height:1200}}),errors=[],failed=[],checks=[];
 page.on('pageerror',e=>errors.push(e.message));page.on('response',r=>{if(r.status()>=400)failed.push(r.status()+' '+r.url());});
 async function ready(){await page.waitForFunction(()=>window.weaponReview&&document.querySelector('#loading').textContent==='',{timeout:20000});}
 try{
  await page.goto(process.env.DARKLIGHT_PREVIEW_URL||'http://127.0.0.1:8766/outputs/previa-armas-20261024/index.html');await ready();
  await page.locator('#play').click();assert.equal(await page.locator('#play').innerText(),'Reproduzir');
  const summary=await page.evaluate(()=>window.weaponReview.data.summary);assert.equal(summary.redesignFiguresReferenced,158);
  for(const weapon of [0,1,2,3]){
   await page.locator(`[data-weapon="${weapon}"]`).click();await ready();
   for(const form of ['0','1','2','3']){
    await page.locator('#form').selectOption(form);await page.locator('#direction').selectOption('0');await page.locator('#action').selectOption(weapon===3?'Attack':'Attack:light1');await ready();
    const state=await page.evaluate(()=>window.weaponReview.state());
    if(weapon<3&&weapon!==1)assert.equal(state.frame.presentation,'redesign');
    await page.screenshot({path:path.join(out,`weapon-${weapon}-form-${form}-attack.png`),fullPage:true});
    if(state.trackLength>1){await page.locator('#next').click();assert.equal((await page.evaluate(()=>window.weaponReview.state())).step,1);await ready();}
    await page.locator('#mode').selectOption('legacy');await ready();
    if(weapon===2&&(form==='1'||form==='2'))assert.match(await page.locator('#badge').innerText(),/com filtro/);
    await page.locator('#mode').selectOption('current');await ready();
    checks.push({weapon,form,action:'first attack',frames:state.trackLength,ownSource:state.frame.presentation==='redesign',controls:true});
   }
   if(weapon<3){
    await page.locator('#form').selectOption('0');await page.locator('#action').selectOption('Combo:combo');await ready();
    assert.match(await page.locator('#sequence').innerText(),/→/);
    await page.locator('#restart').click();await page.locator('#play').click();await page.waitForFunction(()=>window.weaponReview.state().step>0,{},{timeout:3000});await page.locator('#play').click();
    await page.locator('#speed').selectOption('0.5');await page.locator('#zoom').selectOption('1.5');await page.locator('#background').selectOption('#eeeae4');await ready();
    await page.locator('#zoom').selectOption('1');await ready();
    await page.screenshot({path:path.join(out,`weapon-${weapon}-combo-controls.png`),fullPage:true});
    await page.locator('#skin').selectOption('1');await page.locator('#direction').selectOption('7');await ready();
    assert.equal((await page.evaluate(()=>window.weaponReview.state())).frame.presentation.startsWith('legacy'),true);
    await page.locator('#skin').selectOption('0');await page.locator('#direction').selectOption('0');await page.locator('#speed').selectOption('1');await page.locator('#zoom').selectOption('1');await page.locator('#background').selectOption('checker');
   }else assert.equal(await page.locator('#skin').isDisabled(),true);
  }
  assert.deepEqual(errors,[]);assert.deepEqual(failed,[]);
  const result={date:'2026-10-24',ok:true,method:'Headless Edge browser test of the actual local viewer; sources read through localhost.',checks,errors,failedRequests:failed,summary,controls:['weapon','skin','form','direction','single attack','combo sequence','current/legacy comparison','pause/play','frame stepping','speed','zoom','background','moto restricted to Rock'],audio:false,unityExecution:false};
  fs.writeFileSync(path.join(out,'results.json'),JSON.stringify(result,null,2)+'\n');console.log(JSON.stringify({ok:true,contexts:checks.length,errors:errors.length,failedRequests:failed.length,clips:summary.clips}));
 }finally{await browser.close();}
})().catch(e=>{console.error(e.stack);process.exitCode=1;});
