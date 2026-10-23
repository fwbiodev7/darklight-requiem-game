const {chromium}=require('C:/Users/Usuario/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('fs'),path=require('path');
(async()=>{
 const out=path.resolve('outputs/expansao-20261023/browser-review');fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1440,height:1000}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 const results=[];
 try{
  for(const name of ['MossDetonator','NeonRevenant','AshRonin','Relics','MotorcycleSouth','Maelis']){
   await page.goto('http://127.0.0.1:8766/outputs/expansao-20261023/'+name+'/preview/preview.html');
   await page.waitForFunction(()=>document.querySelector('canvas')?.width>0);await page.waitForTimeout(150);
   if(await page.locator('#play').innerText()==='Pause')await page.locator('#play').click();
   const clips=await page.locator('#clip option').evaluateAll(items=>items.map(o=>({value:o.value,label:o.textContent})));
   for(const clip of clips){
    await page.locator('#clip').selectOption(clip.value);
    if(await page.locator('#play').innerText()==='Pause')await page.locator('#play').click();
    const frames=await page.locator('#frame option').count();
    for(let f=0;f<frames;f++){
     await page.locator('#frame').selectOption(String(f));
     await page.locator('canvas').screenshot({path:path.join(out,name+'-'+clip.label+'-'+f+'.png')});
    }
    if(frames>1){
     await page.locator('#frame').selectOption('0');await page.locator('#step').click();if(await page.locator('#frame').inputValue()!=='1')throw Error('Frame step failed: '+name);
     await page.locator('#frame').selectOption('0');await page.locator('#play').click();
     await page.waitForFunction(()=>document.querySelector('#frame').value!=='0',{},{timeout:2000});
     if(await page.locator('#play').innerText()==='Pause')await page.locator('#play').click();
    }
    results.push({sheet:name,clip:clip.label,frames,controls:true});
   }
   await page.screenshot({path:path.join(out,name+'-controls.png')});
  }
  await page.goto('http://127.0.0.1:8766/outputs/expansao-20261023/index.html');
  await page.getByRole('button',{name:'Moto · poses novas',exact:true}).click();
  if(!(await page.locator('iframe').getAttribute('src')).includes('MotorcycleSouth'))throw Error('Hub navigation failed');
  if(errors.length)throw Error(errors.join('\n'));
  fs.writeFileSync(path.join(out,'results.json'),JSON.stringify({ok:true,errors,clips:results,playMode:false,audio:false},null,2));
  console.log(JSON.stringify({ok:true,clips:results.length,frames:results.reduce((n,c)=>n+c.frames,0),errors:errors.length}));
 }finally{await browser.close();}
})().catch(e=>{console.error(e.stack);process.exitCode=1});
