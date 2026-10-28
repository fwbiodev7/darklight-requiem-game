"""Silent asset QA and review index for the 23 October expansion."""
from pathlib import Path
import json,subprocess,sys
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/expansao-20261023'
ASSETS=ROOT/'Assets/Darklight/Resources/Darklight/Multiverse'
PIPE=ROOT/'Tools/Skills/pixel-art-sprite-sheet-master/scripts/sprite_pipeline.py'
NAMES=['MossDetonator','NeonRevenant','AshRonin','Relics','MotorcycleSouth','Maelis']
for name in NAMES:
    # Existing unchanged previews/palettes are reused; audit every final manifest.
    manifest=json.loads((ASSETS/(name+'.json')).read_text());prior=OUT/name/'audit/report.json'
    unchanged=prior.exists() and json.loads(prior.read_text())['sourceSha256']==manifest['imageSha256']
    modes=([] if unchanged else ['audit'])+[m for m in ['preview','palette'] if not (OUT/name/('palette.json' if m=='palette' else 'preview/preview.html')).exists()]
    for mode in modes:
        args=[sys.executable,str(PIPE),mode,'--'+('image' if mode=='palette' else 'manifest'),str(ASSETS/(name+('.png' if mode=='palette' else '.json'))),'--out',str(OUT/name/('palette.json' if mode=='palette' else mode))]
        result=subprocess.run(args,capture_output=True,text=True,encoding='utf-8',errors='replace')
        print(name,mode,'exit='+str(result.returncode))
        if result.returncode:print(result.stdout[-1000:],result.stderr[-1000:]);raise SystemExit(result.returncode)
reports=[json.loads((OUT/name/'audit/report.json').read_text()) for name in NAMES]
summary=dict(sheets=len(NAMES),frames=sum(len(r['frames']) for r in reports),errors=sum(len(r['errors']) for r in reports),warnings=sum(len(r['warnings']) for r in reports),
             playMode=False,audio=False,artMode='built-in image_gen',note='South-facing enemy animations; eight static NPC views; new motorcycle drawings limited to Rock Normal south. Other rider directions/forms retain existing authored art. Indexed native pixel grid unverified.')
(OUT/'asset-qa.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
labels=['Detonador Musgoso','Espectro Neon','Ronin das Cinzas','Vestígios e cartazes','Moto · poses novas','Maelis · oito vistas']
links=''.join(f'<button data-src="{n}/preview/preview.html">{label}</button>' for n,label in zip(NAMES,labels))
captures=''.join(f'<a href="unity-editmode/{p.name}"><img alt="Captura real em edição: {p.stem}" loading="lazy" src="unity-editmode/{p.name}"></a>' for p in sorted((OUT/'unity-editmode').glob('*.png')))
html='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>DARKLIGHT · expansão 23/10/2026</title>
<style>body{background:#12141c;color:#e9e7ee;font:16px system-ui;margin:24px}h1{font-size:26px}p{max-width:1000px;line-height:1.5;color:#c5c4ce}nav{display:flex;gap:8px;flex-wrap:wrap}button{background:#322332;color:#fff;border:1px solid #a74e67;padding:10px;cursor:pointer}iframe{width:100%;height:1000px;border:1px solid #555;margin-top:16px}#captures{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:16px}img{width:100%;image-rendering:pixelated}a{color:#cf8ba9}</style>
<h1>DARKLIGHT: REQUIEM · expansão de 23/10/2026</h1><p>Arte importada e revisão silenciosa. As imagens dos mapas abaixo foram capturadas pelo Unity em edição. Prévia visual e testes de edição não certificam a experiência em gameplay: Play continua suspenso. A moto nova cobre Rock Normal de frente; as outras vistas e formas conservam as folhas anteriores.</p>
<nav>'''+links+'''</nav><iframe id="preview" title="Prévia dos sprites completos" src="MossDetonator/preview/preview.html"></iframe>
<h2>Mapas reais · capturas em edição</h2><div id="captures">'''+captures+'''</div><script>document.querySelectorAll('button').forEach(b=>b.onclick=()=>document.querySelector('iframe').src=b.dataset.src)</script></html>'''
(OUT/'index.html').write_text(html,encoding='utf-8')
print(json.dumps(summary))
