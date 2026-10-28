const {chromium}=require('C:/Users/Usuario/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('fs'),path=require('path');
(async()=>{
 const root=path.resolve('Assets/Darklight/Resources/Darklight/WeaponRedesign20261023');
 const catalog=JSON.parse(fs.readFileSync(path.join(root,'catalog.json'),'utf8'));
 const names=[...new Set(catalog.pages.map(page=>page.resourcePath.split('/').pop()))];
 const out=path.resolve('outputs/redesign-armas-20261023/browser-review');fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1440,height:1100}}),errors=[],clips=[];
 page.on('pageerror',error=>errors.push(error.message));
 try{
  for(const name of names){
   await page.goto('http://127.0.0.1:8766/outputs/redesign-armas-20261023/'+name+'/preview/preview.html');
   await page.waitForFunction(()=>document.querySelector('canvas')?.width>0);
   if(await page.locator('#play').innerText()==='Pause')await page.locator('#play').click();
   const options=await page.locator('#clip option').evaluateAll(items=>items.map(item=>({value:item.value,name:item.textContent})));
   for(const clip of options){
    await page.locator('#clip').selectOption(clip.value);
    if(await page.locator('#play').innerText()==='Pause')await page.locator('#play').click();
    const frames=await page.locator('#frame option').count();
    for(let frame=0;frame<frames;frame++){
     await page.locator('#frame').selectOption(String(frame));
     await page.locator('canvas').screenshot({path:path.join(out,name+'-'+clip.name+'-'+frame+'.png')});
    }
    await page.locator('#frame').selectOption('0');await page.locator('#step').click();
    if(await page.locator('#frame').inputValue()!=='1')throw Error('Step failed: '+name+' '+clip.name);
    await page.locator('#frame').selectOption('0');await page.locator('#play').click();
    await page.waitForFunction(()=>document.querySelector('#frame').value!=='0',{},{timeout:3000});
    if(await page.locator('#play').innerText()==='Pause')await page.locator('#play').click();
    clips.push({sheet:name,clip:clip.name,frames,controls:true,playbackAdvanced:true});
   }
   await page.screenshot({path:path.join(out,name+'-controls.png')});
  }
  if(errors.length)throw Error(errors.join('\n'));
  const result={ok:true,errors,clips,unity:false,audio:false,
   note:'Browser playback/controls verified. Screenshots require separate visual inspection; browser QA does not certify Unity integration.'};
  fs.writeFileSync(path.join(out,'results.json'),JSON.stringify(result,null,2));
  console.log(JSON.stringify({ok:true,sheets:names.length,clips:clips.length,frames:clips.reduce((count,clip)=>count+clip.frames,0),errors:errors.length}));
 }finally{await browser.close();}
})().catch(error=>{console.error(error.stack);process.exitCode=1});
