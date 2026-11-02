# Auditoria de checkpoint — 01/11/2026

## Git / publicação

- Checkout principal: branch `codex/fundacao-darklight`, HEAD `412cd5fea9c4f856e7cecf457eaa838a14ba0b19`, sem remotes listados. Há alterações locais extensas; não usar este checkout como fonte de publicação.
- Cópia isolada de publicação: `work/publicacao-20261021`, branch `integracao-completa-20261026`, HEAD e `origin/integracao-completa-20261026` em `6a5d2fada55e1f006f7f3c07728a7709e2e6a16e`. Remote `origin` = `https://github.com/fwbiodev7/darklight-requiem-game.git`.
- A retomada documenta o commit de implementação enviado; o checkout isolado confirma o HEAD. Não há `final-git-receipt.json` nos outputs de 31/10, então não há recibo final desta tentativa. O branch da cópia isolada tem arquivos não rastreados; revisar allowlist e preservar esses itens antes de qualquer nova publicação.

## Execução e build

- Em `outputs/retomada-20261031/release-progress.json`: import completado (exit 0); EditMode 137/137; PlayMode 25/25; build iniciado às 23:46:02Z, sem `completed`, `buildSucceeded=false`, `standaloneExecuted=false`, `zip=null`.
- Os XMLs `final-edit-204214.xml` e `final-play-204214.xml` têm `result=Passed`, com 137/137 e 25/25 casos passados, zero não aprovados. Isso valida os gates registrados, mas não a build.
- `work/retomada-20261031/release-204214/build.log` termina durante BuildPlayerDataGenerator com `[Error] Failed to free block of size 9164 - size delta was 44!` e stack nativo Unity em `BuildPlayerData`; processo encerrado sem recibo. Não há `outputs/retomada-20261031/Windows/DarklightRequiem.exe` nem ZIP encontrado.
- Estado observado agora: nenhum processo `Unity`/`UnityHub` e `Temp/UnityLockfile` ausente. É seguro avaliar uma nova execução sequencial; não é prova de build aprovada. A suspensão de áudio permanece aplicável.

## Continuidade segura

1. Conferir integridade e espaço livre antes do build; guardar log datado novo e verificar exit code, `build-result.json`, executável e hash do pacote. O log sugere falha nativa de memória/alocador durante BuildPlayer; não presumir falha de teste ou erro C#.
2. Usar a cópia isolada `work/publicacao-20261021`; conferir base/branch e allowlist/hash de `outputs/retomada-20261031/publication-preparation.json`, `publication-hashes.json` e `publication-pathspec.nul` antes de qualquer alteração Git.
3. Não usar XMLs antigos como prova de uma reexecução; só reaproveitar os gates de 31/10 se a árvore importada permanecer idêntica às hashes do checkpoint e a configuração do build for coerente. Caso contrário executar gates frescos, sequencialmente, nunca duas instâncias Unity.
4. Registrar build, execução standalone e pacote em estados separados. Não declarar campanha manual, controle físico ou teste auditivo.

## Fontes primárias

- `Docs/Darklight/DARKLIGHT_RETOMADA_30-10-2026.md`
- `Tools/Delivery/run_release_20261031.py` (import → EditMode → PlayMode → build; bloqueia se `Temp/UnityLockfile` existir)
- `Tools/Delivery/finalize_20261027.py` (runner de gates e validação XML)
- `outputs/retomada-20261031/release-progress.json`, `final-edit-204214.xml`, `final-play-204214.xml`, `release-attempt5-failed.json`
- `work/retomada-20261031/release-204214/build.log` e `build-stdout.log`

## Auditoria adicional de remediação da build (01/11)

- Causa mais forte encontrada: antes da build, o próprio `work/retomada-20261031/release/import.log` registrou `Disk space low` (<1.00 GB) e depois `available disk space (0 MB)`; a build prosseguiu sob condição inadequada de disco. Ao final do log da build, Unity reporta uma falha do allocator (`Failed to free block ...`) durante leitura síncrona do ArtifactFileSystem (`AsyncReadManagerThreaded::SyncRequest`). Isso é compatível com o alerta de disco/cache sob pressão, mas o log sozinho não prova causalidade nem permite distinguir bug nativo. Não encontrei crash dump no caminho padrão `LocalAppData/Temp/Unity/Editor/Crashes` consultado.
- Espaço observado agora: unidade C: tem 13,845,577,728 bytes livres (~12.9 GiB). A limpeza registrada em `outputs/pc-cleanup-20261031/removed-old-build-zips.json` removeu da lista seis ZIPs de builds antigos, totalizando 5,086,011,982 bytes (~4.74 GiB); nenhum arquivo/cache foi removido nesta auditoria. Espaço atual maior torna uma tentativa razoável, mas reservar margem: o zip histórico de `retomada-20261027` tem 1,415,597,713 bytes e a pasta Windows tinha 1,796,752,009 bytes no último build bem-sucedido observado. A estimativa de 6.13 GB não foi confirmada pelos outputs locais que medi aqui.
- Há mensagens `Native extension for WindowsStandalone target not found` no log. Elas antecedem o build e não são o erro terminal; conferir instalação do módulo Windows Build Support se uma nova build falhar por falta de plataforma. O erro terminal ocorreu depois de iniciar BuildPlayerData.
- `DarklightWeaponRedesignBuild.ValidateFreshness` (linhas 61–72) exige que XMLs Edit/Play sejam posteriores a todos os scripts `Assets/Darklight/**/*.cs`, recursos `WeaponRedesign20261023`, `CombatContacts20261027`, áudio WAV de entrega e manifesto de áudio. A arte recém-integrada na pasta `WeaponRedesign20261023` está coberta. O runner `run_release_20261031.py` importa e então executa, em sequência, os filtros selecionados em `work/retomada-20261026/final-gate-selection.json` mais regressões de buffer/combate e gameplay; mínimo do builder é 57 EditMode / 12 PlayMode, exigindo classes específicas. O registro anterior de 137/25 satisfaz esses mínimos, mas para retentar após alterações/importações, use o runner completo que recria XMLs e timestamps, em vez de chamar diretamente só o build.
- Retry mínimo recomendado, somente com Unity fechada e C: com folga confirmada: `py Tools/Delivery/run_release_20261031.py` (executa import → EditMode → PlayMode → Windows build; cria log timestamped e faz o runner validar XML/recibo/EXE). Sem execução standalone automática embutida. Após build comprovada, o harness é `py Tools/Delivery/check_standalone_20261027.py --auto-run --executable outputs/retomada-20261031/Windows/DarklightRequiem.exe --output outputs/retomada-20261101/standalone-auto-HHMMSS`; ele oculta a janela, usa saves isolados, compara/restaura preferências e encerra após até 600 s. Não simula controle/campanha humana.

