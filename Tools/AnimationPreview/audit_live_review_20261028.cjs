'use strict';
// Browser test of our own localhost preview. No user browser profile or external site.
const {chromium}=require('playwright');
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const out=path.resolve(__dirname,'../../outputs/retomada-20261028');
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true});
 try{
  const page=await browser.newPage({viewport:{width:1360,height:1000}});
  const errors=[];page.on('pageerror',error=>errors.push(error.message));
  await page.goto('http://127.0.0.1:8764/index.html');
  await page.waitForFunction(()=>window.previewReview&&previewReview.counts.loaded===previewReview.counts.sheets);
  const counts=await page.evaluate(()=>previewReview.counts);assert(counts.sheets>=5);
  const observed=[];
  for(let card=0;card<counts.sheets;card++){
   const clips=await page.evaluate(i=>previewReview.players[i].sheet.clips.length,card);
   for(let clip=0;clip<clips;clip++){
    await page.locator('.card').nth(card).locator('select').selectOption(String(clip));
    const expected=await page.evaluate(([i,c])=>previewReview.players[i].sheet.clips[c].frames.length,[card,clip]);
    const seen=new Set();const until=Date.now()+4000;
    while(seen.size<expected&&Date.now()<until){seen.add(await page.evaluate(i=>previewReview.players[i].frame,card));await page.waitForTimeout(15);}
    assert.equal(seen.size,expected,`Card ${card} clip ${clip}: every whole frame must render`);
    observed.push({card,clip,frames:expected});
   }
  }
  await page.screenshot({path:path.join(out,'live-review.png'),fullPage:true});
  await page.locator('#all').click();
  assert(await page.evaluate(()=>previewReview.players.every(p=>!p.playing)));
  const before=await page.evaluate(()=>previewReview.players[0].frame);
  await page.locator('.card').first().locator('.next').click();
  const after=await page.evaluate(()=>previewReview.players[0].frame);assert.notEqual(before,after);
  assert.deepEqual(errors,[]);
  const result={success:true,counts,observed,clipsObserved:observed.length,pauseAndStep:true,browser:'isolated headless Chrome',
    nativeUnityVerified:false,sourcePixelEdits:false};
  fs.writeFileSync(path.join(out,'live-review.json'),JSON.stringify(result,null,2)+'\n');
  console.log(JSON.stringify({success:true,...counts,clipsObserved:observed.length}));
 }finally{await browser.close()}
})().catch(e=>{console.error(e.message);process.exitCode=1});
