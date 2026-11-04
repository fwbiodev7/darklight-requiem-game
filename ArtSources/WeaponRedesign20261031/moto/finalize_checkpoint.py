"""Compact local checkpoint; no Assets, Runtime, Unity or Git mutation."""
from pathlib import Path
import json,hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[3];ART=Path(__file__).resolve().parent;OUT=ROOT/'outputs/retomada-20261031/moto'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def write(path,obj):path.write_text(json.dumps(obj,ensure_ascii=False,indent=2),encoding='utf-8')
ready=[]
for name in ['ReqMotoNormalJoinSeparateS20261028V1','ReqMotoNormalDentesSW_W20261028V1','ReqMotoNormalDentesNW_N_NE20261031V1']:
    path=OUT/'staging'/(name+'.json');m=json.loads(path.read_text(encoding='utf-8'))
    assert sha(path.with_suffix('.png'))==m['imageSha256']
    if 'Dentes' in name:
        fragment=OUT/(name+'-pages.json');p=json.loads(fragment.read_text(encoding='utf-8'))
        for page in p['pages']:page.update(startupFrames=2,activeFrames=1)
        write(fragment,p)
    ready.append(dict(name=name,png=str(path.with_suffix('.png').relative_to(ROOT)),json=str(path.relative_to(ROOT)),
        imageSha256=m['imageSha256'],sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],
        figures=len(m['frames']),clips=len(m['clips']),status=m['status'],importedByThisAgent=False,runtimeVerifiedByThisAgent=False))
sources=[]
for path in sorted(ART.glob('*-source.png')):
    with Image.open(path) as image:
        size=list(image.size);image.verify()
    sources.append(dict(path=path.relative_to(ROOT).as_posix(),sha256=sha(path),sourceSize=size,
        state='candidate-unprepared' if 'DentesS_E_SE' in path.name else ('rejected-for-cycle' if 'WalkW' in path.name else 'visual-reviewed')))
checkpoint=dict(date='2026-10-31',agent='moto',complete=False,ready=ready,sources=sources,
    reviewedFigures=sum(x['figures'] for x in ready),reviewedClips=sum(x['clips'] for x in ready),
    assetsModified=False,runtimeModified=False,unityExecuted=False,gitModified=False,generationInFlight=False,
    importedByThisAgent=False,runtimeVerifiedByThisAgent=False,
    failedWalk=dict(attempts=2,selected=False,reason='Same foreground boot still leads left in both semicircles; no true opposite contact. Two complete eight-figure sources retained locally and excluded from staging/publication.'),
    candidateFront=dict(source='ArtSources/WeaponRedesign20261031/moto/ReqMotoNormalDentesS_E_SE20261031V1-source.png',
        produced=True,preserved=True,visualReviewed=False,staged=False,imported=False,runtimeVerified=False,
        recovery='Source returned in memory during full-disk event, then persisted byte-exact after coordinator freed space. PNG verified readable. No new generation.',
        next='Inspect source completely at native/nearest scale; if valid, use prepare_dentes_batch.py with new S/E/SE config and four phases per direction. Keep current file candidate until review.'),
    pending=['Dentes Cruzados S/E/SE source review/preparation; currently only SW/W/NW/N/NE reviewed.',
        'Dual-saw walk W and all other directions remain missing; do not count rejected sheets as locomotion.',
        'Other motorcycle attacks, movement, mounted choreography and Ruptura/Eclipse Absoluto remain incomplete.',
        'Coordinator alone imports Assets/catalog, executes Unity/gates/build and commits/pushes.'],
    nextExact='Integrate reviewed rear fragment and recovered sheets with source hashes; run focused Unity import/runtime checks. Source-only front candidate remains excluded from selected publication until reviewed.')
write(OUT/'checkpoint-final.json',checkpoint)
report='''# Moto Réquiem Normal — checkpoint31/10/2026

32 figuras completas/7 clips revisados em staging: reunião/separaçãoS (12figuras/2clips recuperados28), Dentes CruzadosSW/W (8figuras/2clips recuperados28), DentesNW/N/NE (12figuras/3clips novos31). Isto não conclui a arma, as20técnicas ou a locomoção.

Autoridade: Katana aprovada para rosto/roupa/maturidade/cabelo; fonte canônica de duas serras27 para equipamento e vistas. Comparação de figuras inteiras traseiras confirmou a corrente como desenhada na autoridade; nenhuma direção foi produzida por espelhamento. PNGs de produção preservam RGBA das fontes selecionadas, sem montagem anatômica, recoloração ou redimensionamento de figuras.

DentesSW/W: QA0erros11avisos; DentesNW/N/NE:0erros14avisos; transiçãoS:0erros15avisos. Avisos de paleta>256, alpha parcial e altura por pose continuam registrados. Raster gerado nativo; grade indexada não certificada. Fontes/atlas/contatos e figuras completas ampliadas vistos; GIFs decodificados em quatro fases150/150/280/400ms por ataque. startupFrames2/activeFrames1; gameplay startup300/active280/recovery400ms. Sem reprodução contínua ao vivo ou Unity por este agente.

WalkW31: duas folhas de8figuras geradas e rejeitadas por repetirem a bota próxima à frente nos dois semiciclos. Não houve terceira tentativa; fontes locais preservadas, sem staging nem cobertura.

DentesS/E/SE31:12figuras produzidas na chamada que já estava em voo ao aviso de encerramento. DiscoC cheio impediu salvar o arquivo default; após liberação pela coordenação, a imagem em memória foi transferida e preservada exatamente no projeto,1967212bytes, SHA256dc7f8c1eab5ce4fb7e3b2dcdaa4135425f2ce0ae2afa0f4c80a0518ad01d699e. PNG legível verificado. Estado candidato, não preparado/revisado/importado/testado. Excluir de publicação selecionada até revisão. Não houve regeneração.

Paths/hash e próximos passos exatos: checkpoint-final.json. Nenhuma geração/ferramenta em voo ao encerramento. Assets, Runtime, catálogo, Unity, Git e build pertencem à coordenação e não foram alterados por este agente.
'''
(OUT/'REPORT.md').write_text(report,encoding='utf-8')
print(json.dumps(dict(ready=len(ready),figures=checkpoint['reviewedFigures'],clips=checkpoint['reviewedClips'],sources=sources,checkpoint=str((OUT/'checkpoint-final.json').relative_to(ROOT)))))
