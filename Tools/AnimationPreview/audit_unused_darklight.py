"""Conservative static inventory. Dynamic Resources loading prevents a deletion verdict."""
from pathlib import Path
from collections import defaultdict,deque
import csv,json,re

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/preview-darklight-20261022/reports'
TEXT={'.cs','.json','.unity','.prefab','.mat','.asset','.controller','.anim','.shader','.hlsl','.txt','.shadervariants'}
def main():
    OUT.mkdir(parents=True,exist_ok=True);assets=ROOT/'Assets';guids={};reverse=defaultdict(set);contents={}
    for meta in assets.rglob('*.meta'):
        match=re.search(r'^guid:\s*([a-f0-9]{32})',meta.read_text(encoding='utf-8',errors='replace'),re.M)
        if match:guids[match[1]]=meta.with_suffix('').relative_to(ROOT).as_posix()
    for p in (assets/'Darklight').rglob('*'):
        if p.is_file() and p.suffix in TEXT:contents[p.relative_to(ROOT).as_posix()]=p.read_text(encoding='utf-8',errors='replace')
    dependencies={}
    for path,text in contents.items():
        dependencies[path]={guids[g] for g in re.findall(r'guid:\s*([a-f0-9]{32})',text) if g in guids}
        for target in dependencies[path]:reverse[target].add(path)
        if path.endswith(('.shader','.hlsl')):
            for include in re.findall(r'#include\s+"([^"]+)"',text):
                target=(ROOT/path).parent/include
                if target.is_file():
                    dep=target.resolve().relative_to(ROOT).as_posix();dependencies[path].add(dep);reverse[dep].add(path)
    build=(ROOT/'ProjectSettings/EditorBuildSettings.asset').read_text(encoding='utf-8')
    scenes=re.findall(r'enabled:\s*1\s+path:\s*([^\r\n]+)',build)
    roots=set(scenes)|{p for p in contents if '/Runtime/' in p and p.endswith('.cs')}
    reachable=set();pending=deque(roots)
    while pending:
        p=pending.popleft()
        if p in reachable:continue
        reachable.add(p);pending.extend(dependencies.get(p,set())-reachable)
    runtime='\n'.join(text for p,text in contents.items() if '/Runtime/' in p or '/Resources/' in p and p.endswith(('.json','.asset')))
    editor='\n'.join(text for p,text in contents.items() if '/Editor/' in p)
    literals=set(re.findall(r'Darklight/[A-Za-z0-9_./-]+',runtime))
    editor_literals=set(re.findall(r'"(Assets/Darklight/[A-Za-z0-9_./-]+)"',editor))
    packaged=set();pending=deque(roots|{p for p in contents if '/Resources/' in p})
    while pending:
        p=pending.popleft()
        if p in packaged:continue
        packaged.add(p);pending.extend(dependencies.get(p,set())-packaged)
    rows=[]
    for p in sorted((assets/'Darklight').rglob('*')):
        if not p.is_file() or p.suffix in {'.meta','.cs','.asmdef'}:continue
        rel=p.relative_to(ROOT).as_posix();resource=rel.split('/Resources/',1)[1].rsplit('.',1)[0] if '/Resources/' in rel else None
        refs=sorted(reverse.get(rel,set()))
        if rel in reachable:category='ligado-a-cena-ou-configuracao';certainty='serialized-reference'
        elif resource and any(resource.startswith(literal) for literal in literals):category='recurso-com-carregamento-dinamico';certainty='literal-or-dynamic-prefix'
        elif resource:category='recurso-sem-uso-confirmado';certainty='packaged-by-Resources; dynamic usage unresolved'
        elif rel in packaged:category='dependencia-de-recurso-empacotado';certainty='serialized dependency of a Resources asset'
        elif refs and all('/Editor/' in source or '/Tests/' in source for source in refs):category='editor-ou-testes';certainty='only-editor-test-serialized-references'
        elif rel in editor or any(rel.startswith(literal) for literal in editor_literals):category='referenciado-por-ferramenta';certainty='editor-path-literal-or-dynamic-prefix'
        elif not refs:category='candidato-sem-referencia';certainty='no-serialized-reference-found; code-generated paths possible'
        else:category='referencia-indireta-nao-alcancada';certainty='references exist outside the scanned entry graph'
        rows.append(dict(path=rel,bytes=p.stat().st_size,category=category,evidence=certainty,references=refs[:12]))
    index=json.loads((ROOT/'ArtSources/RuinFists20261021/production-source-index-v2.json').read_text(encoding='utf-8'))
    selected={p['source'] for p in index['pages']}
    for page in index['pages']:
        selected.update(r['source'] for r in page.get('replacements',[]))
    # Reference sources used by whole-character appearance/direction guides are retained as dependencies.
    selected.update('ArtSources/RuinFists20261021/generated/'+name for name in ['walk-west-guided-v9.png','Rock_Normal_Walk_W_GuidedV10.png','absoluto-cardinal-hair-v2.png'])
    generated=[dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size) for p in (ROOT/'ArtSources/RuinFists20261021/generated').glob('*.png') if p.relative_to(ROOT).as_posix() not in selected]
    counts=defaultdict(lambda:dict(files=0,bytes=0))
    for row in rows:counts[row['category']]['files']+=1;counts[row['category']]['bytes']+=row['bytes']
    report=dict(date='2026-10-22',method='serialized GUID graph + enabled scenes + runtime resource literals/prefixes; no gameplay profiler',
        scenes=scenes,categories=dict(counts),assets=rows,generatedNotSelected=generated,deletedFiles=0,
        limits=['Resources folders are packaged even when no direct load was found. Dynamic names, reflection, Addressables and editor builders can invalidate a static orphan guess.',
                'ArtSources, outputs and Tools are production/review inputs rather than runtime art. Absence from a build does not mean they are disposable.',
                'The legacy chain is intentionally bypassed when the complete RuinFists catalog is active; its identifiers and source assets remain for save compatibility/recovery.'])
    (OUT/'unused-inventory.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    with (OUT/'unused-candidates.csv').open('w',newline='',encoding='utf-8-sig') as f:
        writer=csv.DictWriter(f,fieldnames=['path','bytes','category','evidence']);writer.writeheader()
        writer.writerows({k:r[k] for k in writer.fieldnames} for r in rows if r['category'] in {'candidato-sem-referencia','recurso-sem-uso-confirmado','editor-ou-testes','referencia-indireta-nao-alcancada'})
    lines=['# Inventário de uso — 22/10/2026','','Análise estática, sem apagar arquivos e sem iniciar o jogo. GUIDs, cenas habilitadas e caminhos de recursos foram confrontados. Caminhos construídos em código impedem certificar desuso só pela ausência de uma referência.','','| Classificação | Arquivos | MiB |','|---|---:|---:|']
    for category,data in sorted(counts.items()):lines.append(f"| {category} | {data['files']} | {data['bytes']/1048576:.1f} |")
    lines+=['','## Legado substituído com condição explícita','','- `DarklightChainView` desativa as 17 partes da corrente quando `DarklightRuinFistsBank.Ready` é verdadeiro. As manoplas e grevas vêm dos quadros completos dos Punhos.',
            '- `LacoDaRuptura.asset` conserva o GUID e o índice 2 dos saves; o perfil da sessão recebe os ataques dos Punhos. Os ataques `Laco*` ficam como fonte legada/fallback.',
            '- As antigas folhas do terceiro estilo permanecem nos catálogos de recuperação, mas o catálogo completo dos Punhos tem prioridade. Não foram removidas automaticamente.','','## Arquivos de trabalho fora do jogo','',f"Há {len(generated)} fontes geradas dos Punhos que não integram a seleção final. Incluem estudos, propostas superadas e guias; o JSON registra os caminhos. Fontes aprovadas e guias usados foram preservados.",
            '','`outputs/` contém prévias e auditorias; `ArtSources/` preserva originais e referências. Esses diretórios não são carregados diretamente pelo Unity.','','## Maiores candidatos sem ligação confirmada','']
    candidates=sorted((r for r in rows if r['category'] in {'candidato-sem-referencia','recurso-sem-uso-confirmado'}),key=lambda r:-r['bytes'])
    for r in candidates[:20]:lines.append(f"- `{r['path']}` — {r['bytes']/1048576:.2f} MiB; {r['category']}. {r['evidence']}.")
    lines+=['','Lista completa: `outputs/preview-darklight-20261022/reports/unused-candidates.csv` e `unused-inventory.json`. Não há autorização inferida para excluir qualquer candidato.']
    (ROOT/'Docs/Darklight/NAO-UTILIZADO-20261022.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(json.dumps(dict(categories=counts,unselectedGenerated=len(generated),deleted=0),ensure_ascii=False))
if __name__=='__main__':main()
