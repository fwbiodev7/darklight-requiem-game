# Handoff de build — Antigravity — 14/11/2026

## Orientação expressa do usuário

Orientação inicial: perto de 10% do limite disponível, preparar handoff se não houver build. **Ajuste mais recente: o usuário autorizou gastar todo o saldo restante de 16% para entregar a build nesta rodada.** Priorizar compilação e verificação do executável, sem iniciar UI 2.0. Se a entrega não for possível, preservar o passo a passo abaixo e a falha exata. Não estimar saldo sem indicador disponível.

## Estado registrado nesta orientação

- Fontes originais: 28 PNGs recuperados e validados; 26 fontes ativas integradas. Preservar fontes e recibos em `ArtSources/Session20261114/`.
- Três transformações oficiais integradas, com 33/33 EditMode e 9/9 PlayMode/captura focados passando. Evidências e limitações: `CHECKPOINT-TRANSFORMACOES-OFICIAIS-20261114.md`.
- Gothic Eclipse: divergência visual corrigida pela vinculação da textura de cada SpriteRenderer. Validação final 4/4 PlayMode passou; inclui luz lateral dinâmica, reversibilidade e presets dia/interior. Consultar `CHECKPOINT-GOTHIC-ECLIPSE-20261114.md`.
- UI 2.0 suspensa expressamente pelo usuário para priorizar a build agora. Esta orientação supera a condição anterior de aguardar UI 2.0.
- Sete falhas conhecidas relacionadas a folhas Ruptura ausentes permanecem documentadas; não declarar suíte global verde.
- Nenhuma build nova consolidada concluída nesta rodada. Nenhuma limpeza de disco executada; consultar `LIMPEZA-DISCO-20261114.md`.

## Conteúdo obrigatório do handoff final ao atingir o limite

1. Registrar estado exato de cada etapa: produzido, integrado, revisado, testado e pendente; listar arquivos pertinentes e fontes oficiais sem incluir candidatas superadas.
2. Informar editor Unity/versão e plataforma, método de build existente e comando exato verificado, diretório de saída e requisitos de espaço. Reutilizar o pipeline do projeto; não inventar comando ainda não verificado.
3. Listar os comandos exatos para os testes necessários, respectivos logs/XML e falhas remanescentes, distinguindo falhas preexistentes de regressões.
4. Descrever a ordem para resolver bloqueios restantes de Gothic Eclipse/UI 2.0 e os critérios de validação visual antes da build.
5. Orientar uma única instância da Unity, áudio silencioso e saves isolados. Preservar shaders, mapas, partículas, fontes e baseline de Fabius Normal.
6. Gerar Windows x64 pelo pipeline existente somente quando apto; verificar exit code, log, executável, arquivos de distribuição e smoke test. Registrar evidência real e limitações.
7. Revisar diff e conjunto de arquivos antes de Git normal. Nunca usar `git add .` indiscriminadamente, criar arquivos manualmente em `.git` ou contornar bloqueios. Se necessário, deixar a lista pertinente pronta para commit pelo Antigravity.

## Passo a passo confirmado do pipeline

Na raiz do projeto, PowerShell:

1. Verificar que não existe outra instância do Editor com este projeto aberto; não remover lock de editor ativo. Preservar todas as alterações. Conferir espaço livre em C: (o build usa cache e gera aproximadamente 2 GiB de distribuição).
2. Consultar o resultado da tentativa atual em `work/session20261114/build-2026-11-14T184734-600536+0000/` e `outputs/build-windows-gothic-20261114/Windows/build-result.json`. Não repetir uma build já concluída.
3. Se ainda não existe executável no destino: `python Tools/Testing/run_session_20261114.py build --unrestricted-memory`. O uso de memória sem o limite conservador foi expressamente autorizado pelo usuário nesta rodada; nenhuma configuração global de segurança foi alterada.
4. O runner chama Unity **6000.6.0f1**, método existente `Darklight.Editor.DarklightLuciusTestBuild.Build`, com `-darklightLuciusOutput` apontando ao destino novo. Apesar do nome histórico, esse método compila a campanha inteira e o laboratório atuais para **StandaloneWindows64**, LZ4; não é uma build só de Lucius.
5. Se houver executável parcial/necessidade de novo destino, não sobrescrever builds antigas. Executar com diretório novo:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe' -batchmode -nographics -quit -darklightSilent -job-worker-count 1 -projectPath 'C:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main' -executeMethod Darklight.Editor.DarklightLuciusTestBuild.Build -darklightLuciusOutput 'outputs/build-windows-gothic-20261114-retry/Windows' -logFile 'work/session20261114/build-gothic-retry.log'
```

6. Exigir exit code 0 e `build-result.json` com `result: Succeeded`, `errors: 0`. Conferir EXE, UnityPlayer.dll e pasta DarklightRequiem_Data. Não distribuir só o EXE.
7. **Última ordem: entregar somente a build; o usuário ou Antigravity executará os testes. Não iniciar smoke test nesta rodada.** Quando solicitado depois, o comando de captura silenciosa com saves isolados e restauração das preferências de janela é:

```powershell
python Tools/Delivery/check_standalone_20261027.py --auto-run --capture-mode standard --executable outputs/build-windows-gothic-20261114/Windows/DarklightRequiem.exe --output outputs/gothic-eclipse-20261114/standalone
```

8. Somente após execução autorizada daquele teste, conferir `storage-preservation.json` (exitCode 0, saves inalterados, preferências preservadas), `abertura.txt`, PNGs reais e ausência de erros relevantes no `player.log`. Captura padrão não certifica campanha completa nem controle físico. Não declarar que esses arquivos existem nesta build sem executá-la.
9. Pelo espaço limitado, entregar a pasta Windows; não criar ZIP adicional sem necessidade. Registrar resultado final, limitações e caminho de execução. A lista de limpeza fica em `LIMPEZA-DISCO-20261114.md`; nenhuma exclusão foi realizada ou deve contornar políticas.

Fontes aprovadas, 28 originais, clips, bancos e controllers de hoje permanecem no checkout; builds, caches, saves e candidatas superadas não pertencem ao commit. Git ainda requer revisão do conjunto pertinente; não se afirma bloqueio de comandos normais que não foram tentados.

Segunda tentativa em execução: work/session20261114/build-2026-11-14T190442-218182+0000/unity.log, destino outputs/build-windows-gothic-20261114-final/Windows. A primeira terminou Succeeded mas errors=2 e data.unity3d.tmp: NÃO ENTREGAR. O usuário liberou mais de 3GB excluindo ZIPs. Helper work/session20261114/finalize_build_when_ready.py aguarda resultado e só registra entrega se errors=0 e data.unity3d completo; não executa testes. Verificar ENTREGA-BUILD-GOTHIC-20261114.md antes de repetir build. Comando repetível em destino novo: python Tools/Testing/run_session_20261114.py build --unrestricted-memory --build-output outputs/build-windows-gothic-20261114-final/Windows.


## Resultado da segunda tentativa

Build final gerada sem erros. Consultar ENTREGA-BUILD-GOTHIC-20261114.md. Não recompilar antes de conferir esse resultado; testes do executável ficam a cargo do usuário/Antigravity.
