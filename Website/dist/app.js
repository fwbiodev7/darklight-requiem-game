'use strict';
const q=(s,r=document)=>r.querySelector(s),qa=(s,r=document)=>[...r.querySelectorAll(s)];
qa('img[data-fallback]').forEach(img=>{const fallback=()=>{const f=img.dataset.fallback;if(f&&img.getAttribute('src')!==f){img.removeAttribute('data-fallback');img.src=f}};img.addEventListener('error',fallback);if(img.complete&&img.naturalWidth===0)fallback()});
const nav=q('#nav'),toggle=q('#menu-toggle'),bar=q('#reading-bar');
function closeNav(){nav.classList.remove('open');toggle.setAttribute('aria-expanded','false');toggle.setAttribute('aria-label','Abrir menu')}
toggle.addEventListener('click',()=>{let open=!nav.classList.contains('open');nav.classList.toggle('open',open);toggle.setAttribute('aria-expanded',String(open));toggle.setAttribute('aria-label',open?'Fechar menu':'Abrir menu')});
qa('a',nav).forEach(a=>a.addEventListener('click',closeNav));
addEventListener('keydown',e=>{if(e.key==='Escape')closeNav()});
function onScroll(){const d=Math.max(1,document.documentElement.scrollHeight-innerHeight);bar.style.width=String(Math.min(100,scrollY/d*100))+'%'}addEventListener('scroll',onScroll,{passive:true});onScroll();
const tabs=qa('[data-weapon]'),panels=qa('.weapon-panel');
function selectWeapon(i,focus=false){tabs.forEach((t,n)=>{t.setAttribute('aria-selected',String(i===n));t.tabIndex=i===n?0:-1;panels[n].hidden=i!==n});if(focus)tabs[i].focus()}
tabs.forEach((tab,i)=>{tab.addEventListener('click',()=>selectWeapon(i));tab.addEventListener('keydown',e=>{let next=i;if(e.key==='ArrowRight'||e.key==='ArrowDown')next=(i+1)%3;else if(e.key==='ArrowLeft'||e.key==='ArrowUp')next=(i+2)%3;else if(e.key==='Home')next=0;else if(e.key==='End')next=2;else return;e.preventDefault();selectWeapon(next,true)})});
const modal=q('#lightbox'),modalImg=q('#lightbox-img'),title=q('#lightbox-title');let origin;
qa('[data-zoom]').forEach(b=>b.addEventListener('click',()=>{origin=b;const i=q('img',b);modalImg.src=i.currentSrc||i.src;modalImg.alt=i.alt;title.textContent=b.dataset.caption||i.alt;modal.showModal();document.body.classList.add('locked')}));
q('#lightbox-close').addEventListener('click',()=>modal.close());modal.addEventListener('click',e=>{if(e.target===modal)modal.close()});modal.addEventListener('close',()=>{document.body.classList.remove('locked');origin?.focus()});
