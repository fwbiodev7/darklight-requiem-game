from pathlib import Path
from datetime import datetime,timezone
import json,hashlib,shutil,subprocess
B=Path(__file__).resolve().parents[1];R=B/'outputs/darklight-requiem-game';O=B/'outputs/fechamento-fabius-20261103'
def git(*a):return subprocess.check_output(['git','-C',str(R),*a])
def sha(b):return hashlib.sha256(b).hexdigest()
remote=git('ls-remote','origin','refs/heads/integracao-completa-20261026').decode().split()[0];head=git('rev-parse','HEAD').decode().strip()
assert remote==head
proof={'checkedUtc':datetime.now(timezone.utc).isoformat(),'branch':'integracao-completa-20261026','remoteHead':remote,'localHead':head,'commit':git('log','-1','--format=%ci | %s').decode().strip(),'approvedReferences':[],'excluded':'Legacy artwork with red katana in user screenshot codex-clipboard-2c49488e-6bb9-4ab6-af47-dc0f1893085c.png; do not use as anatomy/palette/pose authority.'}
for name in ('ReqKatanaNormalParry8Dirs20261103V3','ReqMotoNormalParry8Dirs20261103V1'):
    rel=f'ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/{name}.png';blob=git('show','HEAD:'+rel);actual=(R/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'/(name+'.png')).read_bytes()
    assert sha(blob)==sha(actual),name
    proof['approvedReferences'].append({'path':rel,'committedSha256':sha(blob),'currentAssetsPngExact':True})
(O/'source-audit/remote-authority-confirmed.json').write_text(json.dumps(proof,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
out=O/'moto-qa/parry-anatomy-correction';s=Path('C:/Users/Usuario/.codex/generated_images/01a1a117-ff4b-72b3-9c2a-e2735c75b8ff/exec-e6a76f5a-30a0-41e9-a053-0ebc84cf40f2.png');target=out/'attempt3-dark-chestnut-candidate.png'
if not target.exists():shutil.copy2(s,target)
records=json.loads((out/'attempts.json').read_text())
records=[r for r in records if r['file']!=target.name]+[{'file':target.name,'generatedSource':str(s),'sha256':sha(target.read_bytes()),'status':'CANDIDATE: darker chestnut hair pass; same intended Katana-defense anatomy as attempt2. Full pixel/metadata/temporal QA not completed; do not import automatically.','appliedInAssets':False,'unityImported':False}]
(out/'attempts.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
cpath=O/'CONTROLE-LOCAL.json';c=json.loads(cpath.read_text(encoding='utf-8-sig'))
c['state']='SAFE_CHECKPOINT_AT_REQUESTED_10_PERCENT_WINDOW';c['latestUsageRead']={'primaryRemainingPercent':10,'weeklyRemainingPercent':23,'source':'codex-app-tools get_usage_limits','reason':'Stop new tasks at requested10%; preserve results only.'}
c['referenceAuthority']={'verifiedRemoteHead':remote,'commitDate':'2026-11-02T22:08:02-03:00','sourceReceipt':'source-audit/remote-authority-confirmed.json','use':'Reviewed deliveries committed from home','excluded':'Legacy red-katana screenshot/art','noResetOrOverwrite':True}
c['anatomyAndPaletteCorrections']['state']='GENERATED_CANDIDATES_NOT_APPLIED';c['anatomyAndPaletteCorrections']['next']='Use verified home-commit reviewed atlases as authority; inspect Parry attempt3 hair/anatomy and exact frame layout; finish only valid whole-figure replacements. KatanaN candidate requires hair matching; PunchW candidates fail opposite support. Preserve approved V3.'
cpath.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
with (O/'RETOMADA.md').open('a',encoding='utf-8') as f:
    f.write('\n\n## Autoridade de casa e fechamento no limite solicitado\n\n')
    f.write(f'- Remoto verificado ao vivo: integracao-completa-20261026 = {remote}, igual ao HEAD local, commit de02/11/2026 22:08:02-03. Já é o trabalho de ontem commitado em casa. Os PNGs KatanaDefesaV3/MotoDefesaV1 usados como referência foram comparados com os blobs HEAD do arquivo revisado: bytes exatos. Recibo: source-audit/remote-authority-confirmed.json. A arte antiga com katana vermelha da captura foi excluída como referência; não confundir sua inspeção de busca com uso em geração ou integração.\n')
    f.write('- Novas gerações: MotoParry tentativa1 foi REJEITADA pelo usuário por corpo anão/criança, nunca integrar. Tentativa2 refeita diretamente pela KatanaDefesa do commit: anatomia revisada, cabelo ainda claro. Tentativa3 fez passe de cabelo castanho escuro, preservada em moto-qa/parry-anatomy-correction/attempt3-dark-chestnut-candidate.png; ainda requer conferência final de geometria/cores/atlas/metadados, não aplicada. Prompts e fontes preservados. Não tratar aprovação técnica da estrutura da tentativa2 como aprovação humana de toda a3.\n')
    f.write('- PunhosW: lote8 não resolveu apoio oposto; rejeitado para integração, preserva fontes corretas. KatanaW aprovado histórico também repete apoio e não deve ser tratado como referênciaB garantida. HipóteseSWB→W NÃO executada após ordem de conferir fonte de casa. KatanaN: lote9 gerado,3 selecionados e3 antigos exatos em staging; controle de cor detectou cabelo novo mais claro, portanto candidato bloqueado para integração até correção de paleta. Ler agentes/checkpoints atuais antes de gerar de novo.\n')
    f.write('- Leitura real de saldo: 10% restantes na janela5h,23% no semanal. Encerramento seguro conforme pedido; nenhuma nova geração/Unity/build iniciada no limiar. Novas correções Fabius desta rodada:0 clips/0 frames aplicados. Gothic primeiro incremento anterior permanece4arquivos aplicados, não validado na Unity. Nada de certificar Fabius/Gothic finalizados.\n')
print(json.dumps({'remoteEqualsLocal':remote==head,'verifiedHomeCommitReferences':len(proof['approvedReferences']),'preservedCandidate':target.name,'newFabiusCorrectionsApplied':0,'remainingWindowPercent':10,'remainingWeeklyPercent':23}))
