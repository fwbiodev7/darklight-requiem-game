# Validação da continuação — 24/10/2026

## Estado

**Rodada final aprovada: 72/72 em edição e 16/16 em Play, 88 casos distintos.** Ela inclui as correções de gravação Windows/Mono e do rastro dos Punhos descritas abaixo. As execuções Unity foram coordenadas pela sessão principal, com `-batchmode -noaudio`, `DarklightAutomation.Silent`, listener pausado/volume zero e saves isolados. A autorização persistente de Play silencioso consta em `EXPANSAO-20261023.md`. Não houve audição automática nem execução de música.

## Verificações novas

| Área | Cobertura | Limite da evidência |
|---|---|---|
| Persistência em edição | 100 gravações/carregamentos reais, 12 flags de leitura, três recompensas únicas, capítulos/checkpoints, armas/formas, backup atômico e recuperação de arquivo principal corrompido. Quatro casos de visitante inválido. | Seis casos automatizados. Arquivos apenas em `work/session-20261024/persistence/`; o save pessoal não é selecionado. |
| Punhos em Play | Réquiem, Sul, `Direto da Ruína`, quatro formas. Exige desenhos completos independentes, fases de preparo/contato/recuperação e capturas da câmera do jogo. Verifica `_RuinFists`, `_VampireForm` e `_WholeFrameId` zerados, máscara preta, ausência de espelhamento e de geometria ativa do rastro antigo sobre o desenho novo. | Um caso novo em `DarklightWeaponRedesignGameplayTests`; não amplia a cobertura para todas as técnicas/direções nem transforma paletas do banco legado em desenhos novos. |
| Três visitantes em Play | 15 segundos reais por IA, pelo menos três ciclos de ataque, estados de vigília/telegráfico/contato/recuperação, dano real, pausa, recompensa repetida 20 vezes sem duplicação, gravação e retorno ao mapa. | Ensaio automatizado curto. Não equivale a combate humano prolongado. O personagem recebe dano com vida elevada para observar ciclos sem encerrar a sessão. |
| Mapas em Play | Três voltas nos dez mapas atuais, rotas de navegação até aliado/relíquia/portal, limites de câmera, leituras, visitantes, recompensas e saves em disco. | Trinta recargas e chamadas reais da API de interação; não são trinta fases vencidas nem percurso humano completo. |
| Pontes em Play | Três voltas, três pontes do mapa 5, ida e volta: 18 travessias por `Rigidbody2D`, `FixedUpdate` e `SlidingVelocity`, com câmera habilitada. | Percurso automatizado; não é validação por controle físico. |

Os três últimos cenários pertencem à fixture `DarklightContinuationGameplayTests`. Os relatórios incrementais ficam em `work/session-20261024/gameplay/`: `visitors.json`, `campaign-cycles.json` e `bridges.json`. Existência de relatório parcial não substitui resultado final do NUnit.

As métricas registram tempo decorrido, intervalos entre frames do Editor, objetos ativos e memória alocada observada. Não estabelecem FPS sustentado do executável nem ausência de vazamentos em sessão longa. Carga e renderização do Editor afetam esses valores.

## Compilação e ambiente

- Unity configurada e instalada: **6000.6.0f1**; pacote Unity Test Framework **1.8.0**.
- Nenhuma Unity/Hub estava ativa na inspeção inicial; disco C: tinha **15,44 GiB** livres naquele momento. Não é uma medição posterior à build.
- Compilação direta das fontes atuais com o compilador e referências da Unity: **Runtime, EditModeTests e PlayModeTests, exit 0, zero erros**.
- Evidência: `work/session-20261024/compact/20261024T122446Z-2774f8e0-continuation-compile-render.json`; logs completos na mesma pasta, assemblies temporários em `work/session-20261024/compile/`.
- Compilar fontes não executa testes, não produz build e não confirma inspeção visual. O agente de validação não iniciou Unity, Play ou o executável; a sessão principal coordena a execução única para evitar competição de processos.

## Defeito de gravação encontrado e corrigido

O primeiro XML `edit-final.xml` registrou **71/72**, com falha real em `OneHundredDiskReloadsPreserveEveryReadAndUniqueVisitorReward`: `System.IO.IOException: Não foi possível remover o arquivo a ser substituído`, em `File.Replace(temp, Path, Path+".bak")`. O arquivo principal, o backup e o temporário permaneciam íntegros. Não eram arquivos somente leitura.

Um ensaio separado usou o `mono.exe` já instalado com a mesma Unity, sem iniciar outro Editor nem acessar saves pessoais. Em **1.500 trocas com backup existente**, ocorreram **7 falhas transitórias**, todas recuperadas na primeira repetição após 10 ms; em **1.500 trocas com backup exclusivo**, ocorreram zero. O HResult observado foi **-2147023721**, código Windows **1175**. A cada troca, o programa conferiu o conteúdo principal e o conteúdo anterior do backup. Evidência e código ficam em `work/session-20261024/replace-probe/`, `SaveReplaceProbe.cs` e `compact/*save-replace-mono-probe*`; o executável do ensaio terminou com exit 0. O helper de resumo teve erro de codificação ao imprimir um caractere BOM do log, depois de salvar o JSON completo; esse erro do helper não é resultado do ensaio.

A correção final mantém o mesmo `File.Replace` atômico e o mesmo `.bak`. Somente o erro 1175 recebe três novas tentativas, após **10/20/40 ms**, enquanto origem e destino ainda existem. Outros erros, ou tentativas esgotadas, continuam propagados. Nenhum arquivo de save é apagado pela correção. O caso de 100 gravações e a recuperação de backup corrompido foram mantidos sem reduzir as exigências.

## Origem do rastro indevido dos Punhos

A inspeção das quatro capturas de contato confirmou desenhos independentes, incluindo cristais/manoplas roxos do Eclipse e cabelo/equipamento próprios do Absoluto. Encontrou também uma barra rosa/cinza na cintura. A origem era `DarklightCombatTrail.PresentRuin`: o segmento do soco antigo usava o eixo central do corpo e uma altura fixa de 1,25, enquanto a mão dos novos desenhos está deslocada. Não era parte do PNG nem uma lâmina antiga.

Para Punhos com figura reconhecida por `DarklightWeaponRedesignBank.IsAuthored`, o rastro antigo fica oculto e sua geometria é limpa. As figuras já incluem sua energia. Os outros contextos conservam os rastros anteriores; aura, regras de acerto e tempos de combate não foram alterados. O teste das quatro formas exige renderer oculto e zero vértices da malha antiga durante todas as fases e passou na rodada final. As quatro capturas de contato finais foram inspecionadas novamente: a barra desapareceu, mantendo player inteiro, equipamentos e identidade de cada forma. As 12 capturas de preparo/contato/recuperação estão em `outputs/redesign-armas-20261023/unity-play/fists-south-form-*.png`.

## Resultados da execução

| Evidência de 24/10 | Resultado | Situação |
|---|---|---|
| `work/session-20261024/edit-final.xml` inicial | 71/72 | Revelou o defeito de gravação; não é resultado final aprovado. |
| `work/session-20261024/edit-verified.xml` | 72/72 | Inclui **8/8** de `DarklightAudioCompletionTests`, seis casos de persistência e oito de redesign; anterior ao ajuste do rastro. |
| `work/session-20261024/play-final.xml` inicial | 16/16 | Inclui as IAs, 30 recargas e 18 travessias; anterior ao ajuste do rastro. |
| `work/session-20261024/edit-delivery.xml` | **72/72** | Posterior às duas correções; término em **24/10 às 09:43:43**, horário de Brasília. |
| `work/session-20261024/play-delivery.xml` | **16/16** | Posterior às duas correções; término em **24/10 às 09:47:10**, horário de Brasília. |

Na rodada final das IAs, o Detonador completou cinco ataques, o Espectro oito e o Ronin sete, em aproximadamente 15 segundos por cenário. Os JSONs confirmam **30 amostras de recarga dos mapas** e **18 travessias**. Não somar as rodadas anteriores como casos novos. Os relatórios `.txt` herdados de redesign ainda imprimem a data fixa 23/10 no cabeçalho; a data desta execução é comprovada pelos XMLs de 24/10 acima.

## Dados da prévia completa de armas e combos

`Tools/AnimationPreview/check_weapon_preview_manifest.py` auditou o `outputs/previa-armas-20261024/manifest.json`: **5.460 clips**, **5.527 descritores de figuras**, todos referenciados, e **501 PNGs**, incluindo seletores de materiais. As referências locais, dimensões, retângulos, pivôs, PPU, durações e IDs passaram com **zero erros e zero avisos**. O resultado completo está em `outputs/previa-armas-20261024/data-qa.json`.

Os **224 contextos** correspondem às duas skins, quatro formas e oito direções, com katana/adagas/Punhos para ambas e moto exclusiva de Rock. Todos têm repouso/movimento; os 192 contextos das três armas possuem as três sequências de combo declaradas. Há **72 clips cuja sequência atual difere da comparação anterior**; isso não representa 72 desenhos inéditos nem 5.460 animações novas. A camada de redesign referencia **158 figuras únicas** dos novos atlas; o restante preserva o catálogo anterior e as limitações registradas na interface.

Os Punhos anteriores em Ruptura/Eclipse usam PNGs capturados pelo shader real em 21/10, identificados como comparação histórica. Essa auditoria é de dados/referências; a revisão da interface e do navegador é registrada separadamente pelo agente responsável. Ela não certifica colisão, dano, cancelamentos de combo nem equivalência de uma sessão completa de gameplay.

## Pendências que esses testes não encerram

- Sensação e ritmo de combate por uma pessoa, sessão humana longa, controle físico e medição sustentada do executável.
- Audição manual de efeitos, mixagem, sincronismo percebido e bossfights.
- Arte de técnicas/direções/formas sem novos desenhos efetivamente importados e vistos.
- Inspeção visual das demais técnicas/direções e recibo da build atualizada, sob coordenação da sessão principal.
