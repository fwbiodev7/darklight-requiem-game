"""Write the factual handoff; optionally publish only the prepared allowlist."""
from pathlib import Path
from datetime import datetime
import argparse
import hashlib
import json
import subprocess
import sys
import time
import zipfile

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261027'
PUB=ROOT/'work/publicacao-20261021'
BRANCH='integracao-completa-20261026'

def git(*args,check=True):
    return subprocess.run(['git','-C',str(PUB),*args],check=check,capture_output=True,text=True,encoding='utf-8')

def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def document():
    release=load(OUT/'release-progress.json')
    rows=[]
    for stage in release.get('stages',[]):
        value=(f"{stage['passed']}/{stage['cases']} aprovados" if 'cases' in stage else 'concluída, saída zero') if stage.get('completed') else 'em andamento / sem aprovação final'
        if stage.get('exitCode',0)!=0:value=f"FALHOU, saída {stage['exitCode']}; ver log preservado"
        rows.append(f"| {stage['stage']} | {value} |")
    native=[]
    for file in sorted(OUT.glob('standalone-auto-*/result.json')):
        result=load(file)
        storage=file.with_name('storage-preservation.json')
        private_storage=load(storage) if storage.exists() else {}
        public_storage={k:private_storage.get(k) for k in ['exitCode','realSavesUnchanged','preferencesUnchangedBeforeRestoration','preferencesPreservedAfterRestoration','timedOut']}
        native.append({'folder':file.parent.name,'success':result.get('success'), 'samples':len(result.get('samples',[])),
            'exception':result.get('exception'),'storage':public_storage})
    automatic=native[-1] if native else None
    summary={'buildSucceeded':release.get('buildSucceeded',False),'standaloneAutomatic':automatic,
        'manualFullCampaignTested':False,'physicalControllerTested':False,'mainMerged':False}
    (OUT/'delivery-summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    archive=release.get('zip')
    zip_text=f"ZIP: `{Path(archive).name}`; {release.get('zipBytes',0)} bytes; SHA-256 `{release.get('zipSha256','pendente')}`." if archive else 'ZIP desta revisão: ainda pendente.'
    native_text=(f"Standalone automático: {automatic['samples']} cenários, success={automatic['success']}. Execução sem ativar a janela; medir foco separadamente. Recibo em `{automatic['folder']}/result.json`." if automatic else 'Standalone desta revisão: pendente. A build anterior abriu o menu fora do Editor; Computer Use foi interrompido pelo Escape humano e não certifica gameplay completo. Os seis arquivos de save da campanha permaneceram idênticos; somente um arquivo de analytics Unity mudou. As quatro chaves de sessão Unity alteradas foram restauradas; preferências preservadas após restauração.')
    text=f'''# DARKLIGHT: REQUIEM — concluídos e pendências para 28/10/2026

Atualizado em {datetime.now():%d/%m/%Y %H:%M:%S}, Brasília. Documento único da rodada. O redesign completo continua parcial.

## Entregas e decisões

- Checkpoint enviado ao GitHub às21h39: `ce985ad2f6cd5148f7dcea51d8b911b7807b5fee`, branch `{BRANCH}`. A revisão posterior deve usar os resultados abaixo.
- Antigravity integrado por fast-forward até `f26ecf4`, dez commits preservados e backup PRIVATE validado antes da integração. O diff original de apoio não alterava Runtime nem artes. Corrigidas as incompatibilidades encontradas com o escopo simplificado e os fixtures locais.
- Arsenal ativo: Katana (0), Punhos da Ruína (2), Moto/duas serras (3). Fabius somente Réquiem. Formas 0 Normal, 1 Ruptura, 2 Eclipse Absoluto fundido. Rock/Presas/form3 permanecem arquivos históricos, sem seleção ativa.
- Regra humana mais recente: excluir 100% dos quadros antigos da apresentação ativa do jogador. O consumidor atual busca somente figuras do redesign; lacunas usam pose/ciclo novo da mesma arma e direção. Técnicas sem fonte própria compartilham golpe novo; formas ainda sem fonte conservam a figura Normal nova sem filtro de transformação. Isso exclui mistura antiga, mas não conclui a coreografia de cada técnica.
- Moto: a configuração a pé empunha duas serras circulares; investidas e técnicas montadas exibem a moto e retornam às serras. A separação visual completa e as demais animações ainda precisam de folhas específicas.
- Katana continua autoridade de rosto, roupa, maturidade, cabelo, olhos e proporção. WorldVisualScale 1.325 mantido. Correções por figuras completas; sem montagem anatômica.
- Configuração de subagentes aplicada no projeto: três perfis, contexto curto, até três agentes, modelos por função e permissões herdadas. Artistas e auditor foram utilizados; nenhuma configuração global alterada.
- Limpeza segura já executada: 19.703.023.347 bytes removidos em caches de cópia antiga e oito builds superadas. Fontes, saves, histórico e build anterior preservados.

## Arte produzida, revisada e selecionada

| Lote | Conteúdo / estado |
|---|---|
| Katana SW/W V1 | 12 figuras; aprovação humana preservada, dois clips integrados. |
| Katana NW/N | 12 figuras; nove originais preservadas, três reparadas inteiras; dois clips integrados. |
| Katana NE/E e SE | Candidatas preservadas fora da seleção ativa; repetição da perna dianteira impede declarar ciclo completo. |
| Punhos Chute | 24 figuras nas oito direções, três fases por direção; integração de 26/27 preservada. |
| Punhos Ascensão da Ruína | 24 figuras novas, oito direções, antecipação/uppercut/recuperação; 140/120/280 ms. Duas folhas revisadas e adicionadas ao catálogo. |
| Moto Normal V3 | Oito vistas montadas e quatro fases Slam S, identidade adulta revisada. |
| Moto idle MatureV1 | 32 figuras, oito direções, quatro fases. Fontes mantidas para modo montado; não são idle a pé com serras. |
| Duas serras Normal V1 | Oito vistas a pé e quatro fases de ataque Sul. Oito figuras originais preservadas e quatro reparadas inteiras. Demais ataques/direções continuam compartilhando poses, sem declaração de animação exclusiva. |
| Contatos pixel art | 12 efeitos Punch/Katana/Heavy. Escala pelo conteúdo visível, diâmetros 1.4/1.65/2.1 unidades; quatro fases em 260 ms, partículas pixeladas e redução de efeitos respeitada. Disparo somente após acerto real. |

Catálogo principal: 174 clips de contexto; Moto e contatos têm bancos separados. Contagem não significa conclusão. Fontes, prompts, hashes, rects, PPU, avisos e prévias ficam em `outputs/retomada-20261027/` e nos manifests dos assets. Rejeitadas e intermediários ficam fora da publicação. O helper da cópia publicada rebaseia fontes mistas para atlas contendo somente figuras selecionadas; o workspace ativo e fontes originais são preservados.

## Validação real desta revisão

| Etapa | Resultado |
|---|---|
{chr(10).join(rows)}

Falha reportada pelo processo: `{release.get('error','nenhuma registrada nesta execução')}`.

{zip_text}

{native_text}

Os gates anteriores passaram 82/82 EditMode e21/21 PlayMode, mas antecedem a exclusão total do legado, Heavy novo e duas serras. Só os gates frescos acima validam esta revisão. Novos testes percorrem todos os golpes Katana/Punhos e os estados das três armas nas oito direções/formas, além do fluxo real serras→investida→serras e dos impactos maiores. Testes automáticos não equivalem a campanha humana completa ou controle físico. PC04/PC06 seguem 2/2 pela evidência de26/10; não repetidos sem mudança sonora.

## Pendências prioritárias para amanhã

1. Ler o resultado mais recente do gate/build/standalone e corrigir qualquer falha registrada; não repetir gates aprovados sem mudança relevante. Abrir a build mais recente, não o ZIP anterior.
2. Completar ciclos próprios de idle/caminhada/corrida/transições de Punhos e serras a pé; Katana ainda precisa NE/E e SE com alternância real.
3. Desenhar cada técnica ainda compartilhada, em oito direções: combos restantes, pesados/carregados, contra-ataques, habilidades, aéreos, finalizadores e técnicas especiais. A matriz detalhada abaixo é a autoridade da cobertura.
4. Moto: desenhar separação/reunião das duas serras, locomoção a pé e montada, transições e fases/direções próprias dos 20 ataques. Oito poses estáticas e um corte Sul não fecham essa lista.
5. Completar defesa/parry, esquiva, dano, morte, alertas e transformação. Poses novas de continuidade não contam como esses ciclos completos.
6. Depois do Normal, completar Ruptura e Eclipse Absoluto fundido violeta/prata, com anatomia/cabelo constantes. Fontes antigas chamadas Eclipse não certificam a fusão.
7. Auditar semanticamente as 220 diferenças das branches antigas17/20 sem merge cego. Não reintroduzir Rock/Presas nem animação superada. Nunca merge main sem autorização.
8. Validar campanha humana, audição subjetiva e controle físico quando disponível. Depois do redesign: Combat AI2.0, Sangue do Abismo/Lucius/O Encapuzado e site; Minecraft pausado.

## Trabalho independente para Antigravity

Usar clone próprio baseado no HEAD remoto de `{BRANCH}`. Pode auditar diferenças17/20, CI/portabilidade, runner/relatórios, GUIDs/referências e smoke de entrada com evidências. Não alterar artes, Runtime, checkout original ou integrar/publicar. Entregar propostas com hashes e caminhos para revisão. Handoff operacional: `Docs/Darklight/DIVISAO-TRABALHO-ANTIGRAVITY-20261027.md`.

## Publicação e retomada

Somente a cópia `work/publicacao-20261021` publica para `fwbiodev7/darklight-requiem-game`, branch `{BRANCH}`. Git da raiz pertence a projeto legado. Excluir builds/ZIPs, saves, caches, MP3 originais, referências privadas e artes rejeitadas. O recibo local do envio fica em `outputs/retomada-20261027/final-git-receipt.json`. Nenhum merge main autorizado.

Não iniciar novos lotes ao atingir o limiar de uso; preservar fontes e tarefas em voo. Em22h03 o saldo consultado era16%na janela e8%semanal. Documento pronto para retomada manual em28/10; retomada automática agendada para02/11 às07h45, após a renovação semanal informada pelo serviço. O lembrete21h40 foi concluído. Começar lendo este documento e os recibos reais.

## Matriz completa de cobertura artística

'''
    audit=OUT/'animation-scope-audit.md'
    if audit.exists():text+=audit.read_text(encoding='utf-8')
    (ROOT/'Docs/Darklight/FECHAMENTO-20261027.md').write_text(text,encoding='utf-8')

def publish():
    assert git('branch','--show-current').stdout.strip()==BRANCH
    assert git('remote','get-url','origin').stdout.strip()=='https://github.com/fwbiodev7/darklight-requiem-game.git'
    assert not git('diff','--cached','--name-only').stdout.strip(),'Preserve an existing index; do not overwrite it'
    subprocess.run([sys.executable,str(ROOT/'Tools/AnimationPreview/prepare_scope_publication_20261026.py'),'--copy'],cwd=ROOT,check=True,stdout=(ROOT/'work/retomada-20261027/publication-final.log').open('w'))
    spec=OUT/'publication-pathspec.nul'
    scope={p.decode('utf-8') for p in spec.read_bytes().split(b'\0') if p}
    for name in scope:
        low=name.lower()
        assert not low.startswith(('work/','library/','temp/')) and not any(t in low for t in ('__pycache__','/windows/','/revisions/')),name
        assert not (low.startswith('artsources/') and '/references/' in low),name
        assert not low.endswith(('.mp3','.mp3.meta','.exe','.zip','.pyc','.bundle')),name
    # Only explicit prepared paths are added, including small ignored receipts.
    result=git('add','-f','--pathspec-from-file='+str(spec),'--pathspec-file-nul',check=False)
    (ROOT/'work/retomada-20261027/final-stage.log').write_text(result.stdout+result.stderr,encoding='utf-8')
    result.check_returncode()
    staged=git('diff','--cached','--name-only').stdout.splitlines()
    assert set(staged)<=scope
    if staged:git('commit','-m','fix: retire legacy player animations and restore dual-saw weapon presentation')
    head=git('rev-parse','HEAD').stdout.strip()
    pushed=git('push','origin',BRANCH,check=False)
    (ROOT/'work/retomada-20261027/final-push.log').write_text(pushed.stdout+pushed.stderr,encoding='utf-8')
    pushed.check_returncode()
    remote=git('ls-remote','origin','refs/heads/'+BRANCH).stdout.split()[0]
    assert head==remote
    receipt={'commit':head,'remoteSha':remote,'branch':BRANCH,'pushed':True,'mainMerged':False,'changedFiles':len(staged),'timestamp':datetime.now().isoformat()}
    (OUT/'final-git-receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(receipt),flush=True)

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--publish',action='store_true');parser.add_argument('--wait-release',action='store_true');args=parser.parse_args()
    if args.wait_release:
        deadline=time.monotonic()+2400
        while True:
            try:release=load(OUT/'release-progress.json')
            except (OSError,ValueError):release={}
            if release.get('finished') or release.get('error'):break
            if time.monotonic()>deadline:raise TimeoutError('Preserve running release; do not publish an invented result')
            time.sleep(10)
        if release.get('buildSucceeded') and release.get('zip'):
            with zipfile.ZipFile(release['zip']) as archive:
                if archive.testzip() is not None:raise RuntimeError('ZIP CRC validation failed')
            subprocess.run([sys.executable,str(ROOT/'Tools/Delivery/check_standalone_20261027.py'),'--auto-run'],cwd=ROOT,check=True)
            release['standaloneExecuted']=True;release['zipCrcVerified']=True
            (OUT/'release-progress.json').write_text(json.dumps(release,indent=2)+'\n',encoding='utf-8')
    document()
    if args.publish:publish()
    else:print('Consolidated handoff updated')
