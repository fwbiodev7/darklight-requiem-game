# DARKLIGHT: REQUIEM — Etapa 00: fundação e auditoria

Data: 10/10/2026. Escopo autorizado: cópia independente, inspeção, arquitetura proposta e validação da base. A implementação de novas mecânicas, artes e site não foi iniciada.

## Cópia e preservação

Projeto novo: `C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem`.

Origem: `https://github.com/fwbiodev7/segredo-varginha-game-teste-cutscene-gdd-ldd`, branch `main`, commit `412cd5fea9c4f856e7cecf457eaa838a14ba0b19`. Branch de trabalho local: `codex/fundacao-darklight`. O remoto `origin` foi removido somente da cópia; ela não possui remoto configurado. Nenhum commit, push ou publicação foi realizado.

Foram conferidos **1.698 arquivos registrados**, totalizando **462.268.390 bytes** antes dos arquivos novos da auditoria. Não há arquivos registrados faltantes, ponteiros Git LFS pendentes, GUIDs duplicados nos metadados registrados, nem assets registrados sem seu `.meta`. A verificação Git LFS dedicada falhou no ambiente restrito; o levantamento direto confirmou zero arquivos com conteúdo de ponteiro LFS.

O projeto original foi apenas lido. Os assets/cenas/scripts existentes da cópia foram preservados. As configurações tocadas automaticamente pela importação foram restauradas para a versão clonada; a verificação final de diferenças nos arquivos já registrados de `Assets`, `Packages` e `ProjectSettings` terminou com código 0. A ferramenta de auditoria é um arquivo novo, separado do gameplay herdado.

## Ambiente observado

- Unity exigido pela base: **6000.6.0f1**, disponível na máquina. Unity 6000.5.7f1 também está instalado; não foi usado nesta auditoria.
- Unity CLI disponível: **1.0.0-beta.12**. Não foi instalado ou atualizado.
- Input System **1.20.0**, URP **17.6.0**, Timeline **6.6.0** e Test Framework **1.8.0** estão declarados no manifest. As versões foram lidas da base; não são uma recomendação de atualização.
- Entrada ativa: novo Input System (`activeInputHandler: 1`).
- Nome de produto herdado: **Template 2D Kit**. Ainda não é uma versão do jogo com menu/nome DARKLIGHT; a identidade de produto e os caminhos de dados precisam ser isolados antes dos testes que gravam saves/configurações.
- Saída padrão configurada: **1920×1080**. Isso não define resolução interna ou dimensões de sprites.

## Matriz de sistemas verificados

Os caminhos abaixo são relativos à cópia. “Existe” significa evidência em código/asset; não significa que toda a funcionalidade foi exercitada em gameplay.

| Sistema | Evidência principal | Reaproveitamento / limite |
|---|---|---|
| Movimento e física 2D | `Assets/Scripts/Game/Varginha/EdelzioTopDownController.cs` | Rigidbody2D, aceleração, colisão contínua, interpolação, corrida e esquiva; separar sanidade/inventário/narrativa específicos |
| Animação direcional | `VarginhaPlayerSpriteAnimation.cs`, `Experiment/CampaignTeamEdelzio.cs`, `VarginhaStudentSprites.cs` | Consumidores cardeais; movimento diagonal não equivale a oito vistas animadas |
| Câmera | `Assets/Scripts/Game/Level/CameraFollow2D.cs`, `VarginhaCameraFraming.cs`, `VarginhaCameraShake.cs` | Suavização, limites e shake; desacoplar áreas/nomes antigos e faixa reservada ao inventário |
| Interação e triggers | `InteractableProp.cs`, `FuscaLevelExit.cs` | Eventos e detecção existem; tipos/condições estão vinculados às missões de Varginha |
| Diálogos e retratos | `VarginhaGameHUD.cs`, `VarginhaDialoguePortraits.cs`, `Assets/Scripts/Game/UI/TypewriterText.cs` | Base útil; trocar elenco/textos e separar modelo de apresentação |
| Cutscenes | `Experiment/CampaignCinematics.cs`, `VarginhaTravelCinematic.cs`, `Experiment/CampaignFlashEncounter.cs` | Corrotinas, transições e enquadramento; não assumir direção cinematográfica genérica pronta só porque Timeline está instalado |
| Transições/portais | `Experiment/CampaignStory.cs`, `CampaignSave.GoTo`, `CampaignCinematics.Load` | Carregamento e posição de entrada; separar IDs e gate de progressão antigos |
| Iluminação colorida | `Experiment/CampaignDynamicLight.cs`, `CampaignIllustratedLighting.cs` | Campo de luz, oclusão e shader de atores; não comprova Light2D/URP ativo |
| Sombras/ambiente | `Experiment/CampaignCharacterShadows.cs`, `VarginhaContactShadow.cs`, `VarginhaEnvironmentPolish.cs` | Reutilizar técnicas, ajustar escala/custo e arte de Vespera |
| Menus/configurações | `VarginhaMainMenu.cs`, `Experiment/VarginhaGameSettings.cs`, `Experiment/ExperimentGUI.cs` | IMGUI, resolução/volume/acessibilidade/vibração; tema, textos e chaves precisam ser próprios |
| Hover/seleção | `Assets/Scripts/Game/UI/PixelMenuTheme.cs`, `VarginhaGamepadUI.cs`; melhorias na branch local | A main tem base visual; o hover moderno do checkout local tem dependências adicionais |
| Áudio/volumes | `Experiment/CampaignSoundscape.cs`, `VarginhaSettingsAudio` em `VarginhaGameSettings.cs` | Ambiência/efeitos e ganho por categoria; trilha metal com camadas por rank ainda é trabalho novo |
| Save/load | `Experiment/CampaignStory.cs`, `CampaignSequence.cs` | JSON, versão 3/migração e escrita temporária com substituição; payload é específico da campanha anterior |
| Controle | `VarginhaInputRuntime.cs`, `VarginhaGamepadBindings.cs`, `VarginhaGamepadUI.cs` | Remapeamento, troca de dispositivo, navegação e vibração; novo mapeamento de combate ainda não aplicado |
| Mapas/colisão | `Experiment/CampaignMapConstruction.cs`, `CampaignIllustratedMaps.cs`, `CampaignArchitectureRenderer.cs` | Muitos mapas são imagens/recortes e colliders BoxCollider2D separados; pacote Tilemap instalado não significa campanha inteira em Tilemaps |
| Combate/IA | `VarginhaPlayerAttack.cs`, `VarginhaCombatEnemy.cs`, `VarginhaCombatTarget.cs` | Combo, buffer, impacto, stagger e knockback; são base de socos, sem espada/parry/rank/transformações implementados para DARKLIGHT |
| Efeitos/shaders | `VarginhaEnemyFeedback.cs`, `VarginhaAmbientPixelEffect.cs`, `Assets/Resources/Varginha/IllustratedMaps/ActorLighting.shader` | Reaproveitar feedback; não tratar efeitos criados/destruídos por ataque como pooling pronto |
| Organização | `Assets/Scripts/Game/`, `Assets/Scripts/Editor/`, `Assets/Tests/`, `Assets/Resources/Varginha/`, `ArtSources/`, `Docs/`, `Website/` | Estrutura existe; muitas dependências são estáticas, `Resources` e nomes de cenas; migrar por consumidor |

Na tabela, arquivos sem prefixo completo estão em `Assets/Scripts/Game/Varginha/`.

## Escala e arte

A auditoria de **186 TextureImporters** encontrou 159 com filtro Point e 27 com Bilinear; todos têm mipmaps desligados. Compressão: 158 sem compressão, 27 comprimidos e um sem campo explícito no metadata. Importadores declaram PPU 100 em 176 casos, 44,1379 em quatro e 32 em três; três não possuem esse campo explícito.

Esses valores não representam sozinhos a escala final: a base recorta sprites em runtime, inclusive alunos de 256×256 com células de 64×64 e PPU `64 / 1.35`, além de outros contratos. Não converter todo o projeto com base no PPU do importador.

O código `VarginhaPixelPresentation.Configure` preserva enquadramentos e PPUs existentes e não adiciona Pixel Perfect Camera. Não foi identificado um contrato único de resolução interna. Recomenda-se **640×360 com escala 3× para 1080p**, sujeito à aprovação visual. A alternativa 512×288 exige barras ou escala fracionária para preencher 1080p.

A skill `segredo-varginha-pixelart` e os documentos `references/estilo-do-jogo.md` e `references/direcoes-e-atlas.md` foram lidos. Nenhuma ferramenta de geração artística foi executada. A imagem enviada contém quatro fotos em uma montagem; será a referência de identidade de Fábio. O personagem Fábio herdado de Varginha não foi adotado como protagonista oficial de DARKLIGHT.

## Inconsistências da base

1. `Assets/Scenes/SampleScene.unity` está habilitada no Build Settings, mas não existe entre os arquivos clonados.
2. As entradas de `TestScene.unity` e `Fase1_deTestes.unity` têm GUIDs diferentes dos metadados dos arquivos existentes.
3. Os seis níveis de qualidade apontam para o GUID de pipeline `681886c5eb7344803b6206f758bf0b1c`, ausente nos metadados registrados. A configuração global é nula. A detecção real no Editor confirmou **BuiltIn**, sem asset de pipeline ativo. URP está instalado, mas não ativo na base auditada.
4. A referência GUID `2bcd2660ca9b64942af0de543d8d7100` do asmdef não foi resolvida por `AssetDatabase.GUIDToAssetPath` mesmo após importação. A compilação passou e o asmdef também referencia `Unity.InputSystem` por nome. Portanto, há uma referência não resolvida a revisar, sem evidência de bloqueio de compilação neste levantamento.
5. O checkout local de Varginha tem melhorias de prompts e menus que não estão na main; o porte precisa incluir as dependências de hover.
6. Nome de produto/persistência e scripts que instalam componentes em runtime ainda pertencem à base. Antes de criar cenas novas, verificar os bootstraps para não instalar comportamento antigo no jogo novo.
7. A suíte de sprites apresentou dez falhas: nove casos esperam os pés no pixel 6 e encontram pixel 4; um caso espera diferença de altura de pelo menos 4 pixels e encontra -2. Os scripts/assets examinados não foram alterados para obter esses resultados. Isso documenta uma inconsistência da base, não validação de artes novas.

Nenhuma cena foi removida ou reconstruída nesta etapa. As correções de configuração devem ser propostas no próximo plano que precisar usar essas cenas/pipeline.

## Validação

A primeira tentativa de importação no ambiente restrito não terminou devido ao acesso ao serviço de licença. Apenas o processo iniciado para a cópia foi encerrado; o Editor aberto no projeto original foi preservado. A repetição com acesso necessário concluiu importação e compilação com código de saída **0** e `Exiting batchmode successfully now!`. O refresh inicial levou **330,927 segundos**, conforme log do Unity; isso mede importação, não desempenho do jogo.

A ferramenta de auditoria executou com saída 0 e chamou `VarginhaPixelPresentation.DetectPipeline()`: **BuiltIn**; anti-aliasing 0; filtragem anisotrópica `ForceEnable`. O caminho de persistência herdado é `C:/Users/Usuario/AppData/LocalLow/DefaultCompany/Template 2D Kit`, reforçando a necessidade de isolamento antes dos testes que gravam dados.

Foram executadas as classes existentes `VarginhaCameraFramingTests` e `VarginhaStudentSpriteTests`: **16 testes, 6 aprovados e 10 reprovados**, com relatório NUnit e saída 8 da CLI (`TESTS_FAILED`). Os três casos de câmera passaram. Os três outros testes aprovados cobrem o atlas canônico de Fábio e aliases de nomes; os dez reprovados verificam alinhamento/altura dos alunos. As mensagens de licença no log não impediram a execução da suíte; o XML confirma falhas de assertions, não uma suíte inconclusiva.

Evidências: [detecção no Editor](runtime.json), [resultado NUnit](testes-editmode.xml), [validação consolidada](validacao.json), [inventário](inventario.json) e [importadores](importadores.json).

Não foi executada a campanha completa, teste físico de controle, medição de 60 FPS, build Windows, geração de arte ou validação de duração da slice. Os testes de remapeamento existentes foram inspecionados; não executar testes que gravam preferências/saves antes de isolar a identidade do novo produto.

## Arquivos novos e próximos passos

- `AGENTS.md`: limites de trabalho e escopo autorizado.
- `Assets/Scripts/Editor/DarklightFoundationAudit.cs` e metadata gerado: ferramenta somente de leitura para consultar o pipeline e ambiente reais no Editor.
- `Docs/Darklight/Etapa00/`: relatório, arquitetura, roadmap, porte, inventários e evidências.

Consultar [arquitetura](arquitetura.md), [roadmap](roadmap.md) e [porte de controle](porte-controle.md). O porte final está previsto junto à validação da vertical slice; a cópia preserva desde já o suporte herdado de Varginha.

A próxima etapa proposta é identidade visual: concept normal/com óculos e folha de oito vistas para aprovação. A autorização atual não inicia essa etapa automaticamente.

**Estado final:** auditoria da Etapa 00 concluída, com problemas da base registrados. A base importa/compila, mas a suíte parcial não está inteiramente aprovada e ainda não representa um protótipo de DARKLIGHT jogável.
