"use strict";
const nav=document.getElementById("nav"),menu=document.getElementById("menu-toggle");
const closeMenu=()=>{nav.classList.remove("open");menu.setAttribute("aria-expanded","false")};
menu.addEventListener("click",()=>{const isOpen=nav.classList.toggle("open");menu.setAttribute("aria-expanded",String(isOpen))});
nav.querySelectorAll("a").forEach(a=>a.addEventListener("click",closeMenu));
const modal=document.getElementById("lightbox"),modalImage=document.getElementById("lightbox-image"),modalTitle=document.getElementById("lightbox-title");
let previousFocus=null;
document.querySelectorAll("[data-zoom]").forEach(btn=>btn.addEventListener("click",()=>{previousFocus=btn;const img=btn.querySelector("img");modalImage.src=img.currentSrc||img.src;modalImage.alt=img.alt;modalTitle.textContent=btn.dataset.caption||img.alt;modal.classList.toggle("poster-view",img.hasAttribute("data-poster"));if(typeof modal.showModal==="function")modal.showModal()}));
document.getElementById("lightbox-close").addEventListener("click",()=>modal.close());
modal.addEventListener("click",e=>{if(e.target===modal)modal.close()});
modal.addEventListener("close",()=>{modalImage.removeAttribute("src");previousFocus?.focus()});
document.addEventListener("keydown",e=>{if(e.key==="Escape")closeMenu()});

// Fallback caso a hospedagem sirva uma versão antiga dos assets locais.
const fabiusNormalArt=document.getElementById("fabius-normal-oficial");
if(fabiusNormalArt) fabiusNormalArt.addEventListener("error",()=>{
  if(fabiusNormalArt.dataset.fallbackAttempted==="1") return;
  fabiusNormalArt.dataset.fallbackAttempted="1";
  fabiusNormalArt.src="https://raw.githubusercontent.com/fwbiodev7/darklight-requiem-game/main/Website/dist/assets/fabius-normal-oficial-20261116.webp";
});

// Exibe os novos pôsteres quando os assets estão disponíveis;
// mantém as artes antigas como fallback sem mostrar imagens quebradas.
document.querySelectorAll("img[data-poster]").forEach(img=>{
  const poster=img.closest(".poster-feature");
  const fallback=document.getElementById(img.dataset.posterTarget);
  const activate=()=>{poster.hidden=false;if(fallback)fallback.hidden=true};
  const restore=()=>{poster.hidden=true;if(fallback)fallback.hidden=false};
  img.addEventListener("load",activate,{once:true});
  img.addEventListener("error",restore,{once:true});
  if(img.complete)(img.naturalWidth>0?activate:restore)();
});
