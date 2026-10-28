# DARKLIGHT: REQUIEM — concluídos e pendências de 26/10/2026

**Documento consolidado único desta sessão, atualizado após a interrupção por limite do serviço.** XMLs finais existem: EditMode 71/71; PlayMode 18/19 com uma falha ainda aberta. Build nova e ZIP não foram confirmados. O helper de publicação não chegou a ser executado naquela interrupção. Os resultados abaixo distinguem produção, aprovação humana, revisão técnica, empacotamento, integração, testes e entrega. O redesign completo e a unificação de todas as branches **não estão concluídos**.

## Escopo oficial adotado hoje

- Fabius: somente Réquiem; katana, Punhos da Ruína e moto liberada para Réquiem.
- Três formas: Normal, Ruptura e Eclipse Absoluto, fusão violeta/branco-prateada das antigas Eclipse e Absoluto. Preservar os poderes úteis e remover redundâncias.
- Rock, Presas e formas antigas permanecem como legado, sem exclusão destrutiva.
- Lucius: somente Normal e Nexo Fenrir; O Encapuzado mantém rosto oculto. Implementação e revelação aguardam o redesign de Fabius e Sangue do Abismo.
- Prioridade: Normal → Ruptura → Eclipse Absoluto → Combat AI 2.0 → expansão → site. Minecraft pausado.

Contrato atual: `Docs/Darklight/ESCOPO-CANONICO-20261026.md`. GDD, novo LDD, README e registros históricos receberam a atualização de escopo. As instruções antigas de duas skins/quatro formas e moto exclusiva de Rock foram superadas.

## O que foi concluído — sistemas e preservação

| Trabalho | Estado comprovado até este registro | Validação restante |
|---|---|---|
| Seletores ativos | Código adaptado para Réquiem e arsenal Katana/Punhos/Moto; Rock/Presas retirados das opções ativas | Gate final e execução standalone |
| Formas e poderes | Código consolidado em Normal/Ruptura/Eclipse Absoluto, preservando benefícios úteis e IDs legados | Gate final e conferência visual; aparência fundida ainda parcial |
| Saves | Migração compatível implementada, com valores legados preservados e armazenamento de teste isolado | Resultado final dos testes e standalone; saves originais não descartados |
| Moto | Réquiem liberada mantendo as seis técnicas e o combate aéreo existente | Testes finais de renderização/combate e standalone |
| Coerência visual | Seleção/fallback adaptados para evitar substituição parcial incoerente; fases novas são usadas apenas onde o consumidor consegue resolvê-las | Arte completa das ações e direções restantes |
| Escala e identidade | `WorldVisualScale = 1.325`, PPU por folha, apoios e figuras completas preservados | Conferência final em mundo; revisão técnica não equivale a aprovação humana |

As seis técnicas da moto preservadas são Serra Infernal, Giro Devastador, Esmagamento Vertical, Roda Destruidora, Cavalgada Brutal e Execução Aérea Motorizada. Manter uma pose montada para uma técnica sem ciclo novo não significa que sua animação completa esteja pronta.

## O que foi produzido — arte, revisão e integração

| Lote | Produzido/revisado | Empacotado | Importado/testado |
|---|---|---|---|
| Moto Réquiem Normal | 12 figuras completas: oito vistas estáticas e quatro fases de Slam S; fonte V2 corrigiu camisa/mangas | PNG/JSON, apoios, PPU 127.272727 e preservação exata de RGBA; QA zero erros/14 avisos | O teste real das oito vistas Réquiem passou no XML final; isso não certifica seis técnicas com ciclos novos |
| Idle montado Réquiem Normal | 12 figuras completas novas, S/SW/W × quatro fases: repouso, inalação, piscada/expiração e retorno | Descriptor separado, PPU 132.597946, tempos 350/250/180/350 ms; QA zero erros/13 avisos | Teste real dos 12 quadros, seleção parado e saída ao mover passou no XML final; cinco lados permanecem em fallback |
| Punhos Normal — Chute Quebra-Osso | 12 figuras selecionadas, S/SW/W/NW × antecipação/contato/recuperação; contato frontal reparado por figura inteira | Quatro clips, fases 4/5/11, tempos 75/85/160 ms, PPU 165.151515; prova de preservação de figuras inteiras; QA zero erros/13 avisos | Quatro clips presentes no catálogo de 158 entradas; não presumir teste específico de todos os contatos do Chute sem evidência |
| Katana Normal — Walk SW/W | V1 de 12 figuras **aprovada visualmente pelo usuário após a revisão técnica anterior**; V2 continua rejeitada | Na retomada, V1 empacotada sem regenerar: dois clips de seis fases/400 ms, PPU 162.373737; QA zero erros/13 avisos | Fragmento separado, **não integrado ao catálogo**, importação/runtime pendentes |

Antes da interrupção foram preparadas **36 figuras novas selecionadas** — 24 da moto e 12 de Punhos. Na retomada foram empacotadas as **12 figuras Katana V1 já produzidas e posteriormente aprovadas**, sem nova geração: total de 48 figuras selecionadas/empacotadas, com estados de integração distintos na tabela. Não somar versões rejeitadas, cópias de fonte, previews ou legado. Somente a aprovação humana explicitamente registrada da fonte Katana V1 é alegada; ela não certifica a campanha.

### Fontes e hashes principais

| Lote | Fonte selecionada / SHA-256 | Atlas / SHA-256 |
|---|---|---|
| Moto vistas/Slam | `ArtSources/WeaponRedesign20261026/sources/RequiemMotoNormal-v2-source.png` — `6379e8ea676f0bd2cb1b9b7ea46dc602c3f959993d9425cd7e25fce902bf30a3` | `RequiemMotoForm0Ride8SlamS20261026V1.png` — `b5e7b3d1fedfe484e8e5d0b9051f9a64f7610443c166bd11eb15dde3320d56a3` |
| Moto idle | `ArtSources/WeaponRedesign20261026/sources/RequiemMotoNormalIdle-S-SW-W-v1-source.png` — `bb9d909cb46d383e866d644d8cc301b8d74e218182d60f349a7369c24abb39e9` | `RequiemMotoForm0IdleSSWW20261026V1.png` — `bac1ae553b1ba2aa39377e966abce87482c89d0164ef5c409fb510910a464719` |
| Punhos selecionado | `ArtSources/WeaponRedesign20261026/punhos-normal/ReqFistsNormalChuteSSWWNW20261026Selected-source.png` — `9475d46b1246d02ed19b687b9de72eaa434ef47702a7665f3c910e5288eeb0a9` | `ReqFistsNormalChuteSSWWNW20261026Selected.png` — `2eb38887ca38b73a8acce03d98a9d9c39e8012d3eb5535ca73c41a4063f35bbc` |
| Katana V1 aprovada | `ArtSources/WeaponRedesign20261026/katana-normal/ReqKatanaNormalWalkSW_W-v1-source.png` — `233f498a8e69f2ed2b2fe3b06baf9b069a4df82a069bb930b616e6ec2eed00e5` | `ReqKatanaNormalWalkSW_W20261026ApprovedV1.png` — `f0a5691ede675c0eab44d9e0b417c27d3417442eb3d127ba8c81082bdb68ec17` |

Os quatro atlas estão em `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`. Os nomes históricos das pastas não indicam a data de produção. Fontes e hashes completos, retângulos, pivôs, timings e estados ficam nos manifestos e em `outputs/retomada-20261026/{moto,moto-idle,punhos-normal,katana-normal}/`. O preparo da Katana manteve o catálogo compartilhado inalterado, SHA-256 `bb899a298880b62129ed3186dd382495f40b8e5dbe6b860a458d4b478f30ca9f`; sua inclusão depende da coordenação.

### Rejeitadas, limitações e preservadas

- A revisão técnica inicial rejeitou Katana Walk SW/W V1/V2 por leitura da alternância das botas. **Depois disso o usuário aprovou visualmente a V1 exibida na captura; essa aprovação humana prevalece para selecionar a V1**, SHA-256 `233f498a8e69f2ed2b2fe3b06baf9b069a4df82a069bb930b616e6ec2eed00e5`, `ArtSources/WeaponRedesign20261026/katana-normal/ReqKatanaNormalWalkSW_W-v1-source.png`. V2 permanece rejeitada e fora da entrega. Aprovação da fonte V1 não certifica empacotamento, importação, transições ou runtime; não repetir a antiga rejeição da V1 como decisão vigente. A caminhada S aprovada foi preservada.
- A primeira tentativa de moto com camisa de caveira/mangas inadequadas foi superada pela fonte V2. Somente a fonte selecionada e seus pacotes entram no escopo de publicação.
- Punhos preservou as figuras corretas e selecionou o reparo de figura inteira do contato; fontes intermediárias com figuras rejeitadas não fazem parte da entrega.
- Os avisos de QA incluem paletas acima de 256 cores e alpha parcial; a moto erguida também provoca aviso de altura aparente. Não remover pixels, redimensionar anatomia nem certificar pixel art nativa indexada por inferência. Fontes/contatos inteiros foram inspecionados; prévias GIF são quantizadas. O navegador bloqueou o protocolo local do HTML, sem contorno; movimento em Unity depende do gate real.

## Cobertura real e pendências artísticas

A auditoria `Tools/AnimationPreview/audit_reduced_scope_20261026.py` gera `outputs/retomada-20261026/coverage.json` sem modificar catálogos ou pixels. Na conferência após o Chute, há **51 clips/191 figuras Réquiem Katana/Punhos aproveitáveis**, 39 dos 384 contextos principais com alguma fonte revisada e 345 sem fonte. Os quatro clips novos do Chute acrescentam técnicas em contextos Attack já existentes, mantendo 39 contextos; não presumir animações completas. Catálogo: 158 clips/582 figuras físicas. Os dois clips da Katana V1 continuam em fragmento separado e não entram nessa contagem até integração real.

O legado original tinha 154 clips/570 figuras físicas; 99 clips/358 figuras referenciadas saíram da seleção ativa, preservados como legado. Oito clips/30 figuras antigas de Eclipse/Absoluto são candidatos para revisão da fusão, sem aprovação automática como aparência fundida. O banco da moto é separado: 48 figuras Rock legadas e 24 figuras Réquiem Normal novas; estas cobrem oito vistas estáticas e quatro clips animados (Slam S e três idles). O checklist da moto não deve ser somado à matriz principal para divulgar percentual de conclusão.

| Prioridade | Pendência real |
|---|---|
| Normal/Katana | Empacotar/importar/testar V1 aprovada SW/W, sem regenerar; depois completar os outros cinco lados além de S/SW/W, corrida, demais combos, pesados/carregados/aéreos, defesa/parry, esquiva, dano/morte e transições |
| Normal/Punhos | Locomoção e demais golpes/combos/especiais/aéreos; completar Chute nos outros quatro lados, além de defesa/esquiva, dano/morte e transições |
| Normal/Moto | Idle dos outros cinco lados, ciclos reais de locomoção, montar/desmontar e transições; demais ações/direções das seis técnicas; conferir apoios/escala em mundo |
| Ruptura | Completar Katana/Punhos/Moto nas direções/ações restantes, mantendo mechas fortes sem aumentar o corte canônico |
| Eclipse Absoluto | Aparência fundida violeta/branco-prateada consistente e ações/direções próprias; materiais antigos são referências/candidatos, não conclusão |
| Depois do redesign | Combat AI 2.0, Sangue do Abismo com Lucius Normal/Nexo Fenrir e O Encapuzado, site/material promocional |

## Gameplay e áudio de hoje

**PC04 e PC06 aprovados no reteste contextual: 2/2**, sem apagar a falha histórica de 25/10. Evidência: `outputs/retomada-20261026/audio-context-tests.xml`, execução 20:54:23–20:54:41 UTC, casos `BreathRequiresItsRealLowHealthConditionAndStopsAfterHealing` e `ClothRequiresItsRealWalkingConditionAndStopsAtRest`. A coordenação confirmou vozes e sinal DSP nos contextos preparados; avaliação auditiva subjetiva e transições de toda a trilha não foram certificadas. Os WAV saudáveis foram preservados.

Os XMLs da rodada interrompida foram inspecionados: `outputs/retomada-20261026/final-edit-tests.xml` terminou em 26/10 às 21:00:41 UTC com **71/71 aprovados**, zero falhas/ignorados; `final-play-tests.xml` terminou às 21:03:57 UTC com **18/19 aprovados e uma falha**, sem ignorados. A falha é `DarklightDeliveredSfxGameplayTests.WeaponFormMapDeathAndReturnKeepTheSameSilentPoolAndPreserveStorage`; precisa de diagnóstico e reteste. Não declarar gate completo ou build liberada enquanto a falha estiver aberta. As suítes de 25/10 são históricas. Testes automatizados não equivalem a campanha humana completa nem a controle físico.

## Backup privado, branches e publicação

- Backup: [fwbiodev7/darklight-requiem-backup](https://github.com/fwbiodev7/darklight-requiem-backup), visibilidade **PRIVATE**, validado pela coordenação. **Cinco branches e sete tags; 12 hashes de referência conferidos sem divergência.** O repositório principal não teve a visibilidade alterada.
- Referências analisadas: main, `codex/punhos-da-ruina-20261021`, `codex/expansao-aerea-moto-20261023`, `codex/animacoes-pele-20261017` e `codex/padronizacao-visual-20261020`.
- A branch `integracao-completa-20261026` foi criada na cópia isolada `work/publicacao-20261021`, a partir de `b719c527c736009e3ff560fd124ad07f67ae30d0`.
- **Nenhuma branch antiga foi mesclada.** Main/Punhos não têm trabalho exclusivo a reaplicar; animações é ancestral da padronização. Restam 220 caminhos diferentes/ausentes frente à padronização para análise por finalidade, incluindo scripts, shaders, metadados e arte. Não usar merge cego ou `-s ours` para ocultar essa revisão.
- A raiz `.git` do workspace contém histórico legado de outro projeto, preservado exclusivamente em bundle local. Não transferir esse histórico ao GitHub do DARKLIGHT.
- Bundles locais verificados, auditoria de metadados/LFS/submódulos e triagem de padrões de credenciais estão em `work/retomada-20261026/git-backup/`. Zero achados nos padrões de texto examinados não certifica ausência de segredos arbitrários.
- Preparação de cópia/pathspec, commit/push e PR ainda são etapas separadas. Root fará stage/commit/push apenas do escopo permitido. Não enviar builds, saves, caches, MP3 originais, referências privadas, fontes rejeitadas ou outros projetos. Não fazer merge definitivo na main sem autorização.

Detalhes da auditoria histórica: `Docs/Darklight/AUDITORIA-GIT-20261026.md`. Backup validado não significa branches unificadas, e integração Git não equivale a integração Unity testada.

## Build Windows e entrega

**Build nativa nova e ZIP não confirmados na interrupção por limite.** O executável `outputs/retomada-20261026/Windows/DarklightRequiem.exe` não estava presente na conferência posterior e nenhum ZIP foi encontrado na saída da rodada. Isso não apaga nem substitui a build anterior preservada. Só registrar entrega após gates válidos, executável/dados confirmados, inicialização fora do Editor, teste dos fluxos acessíveis com saves isolados e ZIP.

| Entrega | Estado inicial |
|---|---|
| Importação final de moto/idle/Punhos | XMLs finais disponíveis; consultar casos aprovados e recibo de importação sem extrapolar validação artística |
| EditMode/PlayMode finais | 71/71 EditMode; 18/19 PlayMode, uma falha de pool/arma/forma/save aberta |
| Build Windows | Não confirmada; gate PlayMode ainda tem falha |
| Inicialização/partida fora do Editor | Não executada nesta compilação ainda |
| Controles/combate/moto/transições standalone | Não executados nesta compilação ainda |
| ZIP e caminho exato | Ainda não produzido |
| Commit/push da integração e PR | Ainda não confirmados |

## Continuidade já programada

A automação existente foi atualizada para **27/10/2026 às 17h50, America/Sao_Paulo**, neste mesmo chat, sem criar duplicata. O usuário autorizou usar o saldo restante desta sessão após salvar essa retomada. Não iniciar novas gerações no fechamento; preservar QA, build, fontes e recibos.

Na retomada: ler este arquivo, contrato canônico e recibos finais; conferir ZIP/build/publicação reais; continuar Normal pelas lacunas válidas, evitando refazer idles/golpes já selecionados. Reabrir a análise das 220 diferenças de branches antes de integrar histórico antigo. Preservar fontes corretas, saves originais, HUD e escala. Nenhum trabalho artístico/narrativo futuro deve ser declarado concluído por agendamento.

## Atualização final da coordenação

O limite do serviço interrompeu os agentes e a aprovação automática de ferramentas; a preparação de publicação não foi executada naquele momento. A retomada atual recebeu novo pedido de limpeza segura e análise/integração da branch Antigravity. Root coordena essas operações e um agente separado revisa os testes/tools/docs. Nenhum merge Antigravity, nova build, ZIP, commit/push ou PR é presumido neste registro. Atualizar este mesmo documento quando existirem recibos reais.

O Python bundled também estava sem fontes da biblioteca padrão (`Lib/encodings/__init__.py` ausente), antes de qualquer limpeza da coordenação. Foi criado, com autorização, o ambiente isolado `work/tool-runtime-20261026`, Python 3.14, Pillow 13.0.0 e NumPy 2.5.4. O Python global e o bundled não foram alterados. Esse ambiente recuperou o empacotamento técnico da Katana V1; não é parte da build ou da publicação. `Tools/AnimationPreview/prepare_scope_publication_20261026.py` foi criado e teve sintaxe verificada; **--copy não executado**, aguardando a conclusão da integração Antigravity. O helper exclui referências privadas, V2 rejeitada, builds, saves, MP3 e caches e não faz stage/commit/push.
