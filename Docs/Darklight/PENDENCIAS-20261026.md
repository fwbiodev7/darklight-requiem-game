# Pendências reais — 26/10/2026

Escopo vigente: [ESCOPO-CANONICO-20261026.md](ESCOPO-CANONICO-20261026.md). Os fechamentos de 25/10 permanecem históricos. Esta lista registra o inventário e as fontes produzidas na rodada; resultados finais de implementação, Unity, build e Git devem constar no checkpoint com suas evidências.

## Cobertura recalculada e fontes novas da rodada

Auditoria: `Tools/AnimationPreview/audit_reduced_scope_20261026.py`; relatório/matrizes: `outputs/retomada-20261026/coverage.json`. Estrutura e hashes: zero erros.

| Categoria | Medida comprovada pelo catálogo |
|---|---|
| Banco atual após os quatro clips de Chute | 158 clips, 67 folhas, 582 figuras físicas; o banco anterior de 154 clips/570 figuras foi preservado e ampliado |
| Fontes Réquiem Katana/Punhos aproveitáveis em Normal/Ruptura | 51 clips, 24 folhas, 191 figuras distintas referenciadas |
| Matriz principal reduzida | 384 contextos: uma skin × duas armas principais × três formas × oito direções × oito ações |
| Contextos com alguma fonte nova revisada disponível | 39; outros 345 ainda sem fonte revisada no novo escopo |
| Antigas Eclipse/Absoluto para avaliação da fusão | 8 clips/30 figuras candidatas; não aprovadas como aparência fundida |
| Fora da seleção ativa | 99 clips/358 figuras referenciadas de Rock e/ou Presas, preservados como legado |
| Banco da moto separado | 48 figuras antigas do piloto Rock: quatro formas × oito vistas de avanço + quatro fases de Slam S |
| Moto Réquiem | 24 figuras Normal revisadas tecnicamente e empacotadas: oito vistas estáticas de avanço, Slam S de quatro fases e três ciclos idle S/SW/W de quatro fases; 12 contextos com fonte revisada no checklist separado de 240; importação/runtime registrados separadamente pelo coordenador |

Os 39 contextos não são 39 animações completas: Attack agrupa técnicas distintas. Não somar o checklist da moto à matriz principal para divulgar um percentual de conclusão. O inventário identifica fontes, sem certificar que o novo consumidor runtime já as usa corretamente.

| Forma/arma | Cobertura nova identificada | Pendência prioritária |
|---|---|---|
| Normal/Katana | Idle em oito direções, Leve1/Leve2 em oito, Walk S; fonte Walk SW/W V1 posteriormente aprovada pelo usuário e agora empacotada em fragmento separado | Integrar/importar/testar V1 aprovada SW/W, sem regenerar; depois completar os outros cinco lados além de S/SW/W e demais ações. Somente V2 continua rejeitada |
| Normal/Punhos | Idle e primeiro Direto em oito direções | Locomoção, demais golpes/combos, especiais/aéreos, defesa, esquiva, dano/morte e transições |
| Ruptura/Katana | Fontes para Idle/Attack S | Outras direções e ações, com mechas fortes sem aumentar cabelo |
| Ruptura/Punhos | Idle S, Direto S/SW/W | Restantes vistas/ações e revisão das traseiras retidas |
| Eclipse Absoluto | Nenhuma arte revisada como fusão | Definir/revisar aparência violeta e branco-prateada usando referências anteriores; completar progressivamente, após Normal e Ruptura |
| Moto/Réquiem | 24 figuras Normal revisadas/empacotadas, com idle real S/SW/W e consumidor preparado, além do legado preservado | Confirmar importação/runtime/escala; idle dos cinco outros lados, ciclos de locomoção/transições, demais ações/direções das seis técnicas e formas próprias continuam pendentes; não chamar vistas estáticas de ciclo idle |

## Moto Réquiem Normal produzida nesta rodada

Fonte inteira V2: `ArtSources/WeaponRedesign20261026/sources/RequiemMotoNormal-v2-source.png`, SHA-256 `6379e8ea676f0bd2cb1b9b7ea46dc602c3f959993d9425cd7e25fce902bf30a3`. A correção por Imagegen retirou a camisa de caveira indevida e restaurou mangas compridas, inclusive no levantamento. Revisão visual técnica do coordenador e do empacotador; sem aprovação humana presumida.

Atlas/manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/RequiemMotoForm0Ride8SlamS20261026V1.{png,json}`. Nome V1 identifica o primeiro pacote aprovado tecnicamente, produzido a partir da fonte V2. Atlas SHA-256 `b5e7b3d1fedfe484e8e5d0b9051f9a64f7610443c166bd11eb15dde3320d56a3`. Uma PPU de 127.272727 por folha, pelo corpo em pé no quadro 9 (cabelo ~742, sola 994; 252 px/1.98, incerteza ~4 px), com `WorldVisualScale = 1.325` preservado. Calibração visual em mundo ainda necessária.

Empacotamento reutiliza os helpers existentes, conserva todo RGBA não transparente selecionado e não monta partes, redimensiona anatomia ou remove pixels. Recibo: `outputs/retomada-20261026/moto/prepared.json`; guias: `ArtSources/WeaponRedesign20261026/requiem-motorcycle-normal-guides.json`. QA técnico: zero erros e 14 avisos (12 de paleta acima de 256 cores, variação de altura com moto erguida, alpha parcial). Esses avisos impedem certificar pixel art nativa indexada ou ausência de halos somente pelo inventário. Contato/HTML/GIFs técnicos foram gerados; GIFs são quantizados para prévia. Estados no recibo: produzido/revisado/empacotado; **importado=false, runtime=false** até evidência posterior do coordenador.

### Idle montado real — S/SW/W

Nova geração integrada de imagens, com as duas referências inteiras (moto V2 e katana canônica), produziu `ArtSources/WeaponRedesign20261026/sources/RequiemMotoNormalIdle-S-SW-W-v1-source.png`, SHA-256 `bb9d909cb46d383e866d644d8cc301b8d74e218182d60f349a7369c24abb39e9`. São 12 figuras completas novas: três direções e quatro fases distintas por direção, com repouso, inalação sutil, piscada/expiração e retorno; não há translação ou montagem anatômica para simular o ciclo.

Pacote separado `RequiemMotoForm0IdleSSWW20261026V1.{png,json}`, atlas SHA-256 `bac1ae553b1ba2aa39377e966abce87482c89d0164ef5c409fb510910a464719`, uma PPU 132.597946 pela proporção anatômica sentada comparada na fonte inteira. Apoios registrados por rodas/botas; PPU e escala em mundo ainda exigem verificação. Tempos 350/250/180/350 ms, sem alterar timing de ataque. QA: zero erros/13 avisos (12 de paleta e alpha parcial). Recibos/guias/contato/GIFs em `outputs/retomada-20261026/moto-idle/`; prompt e referências em `ArtSources/WeaponRedesign20261026/requiem-motorcycle-idle-generation.json`.

Extensão do banco e do consumidor preparada para usar esse ciclo somente em Réquiem Normal parado/Neutral nas direções S/SW/W. Ao mover ou atacar, o idle não substitui a ação. Cinco direções e formas sem idle próprio mantêm fallback explícito. O teste focado `RequiemMountedIdleAnimatesThreeAuthoredDirectionsAndStopsWhenMoving` confere os 12 quadros reais no consumidor e a saída ao mover; resultado deve constar no gate final. Importação é feita por `DarklightScope26Delivery.Import`, sem alterar o catálogo de armas nem eliminar as 12 figuras anteriores. Fontes e contato foram inspecionados; playback do HTML local foi bloqueado pela política de protocolo do navegador e não foi contornado. Não declarar prévia em movimento ou runtime verificados sem o gate correspondente.

## Trabalho técnico da rodada

Atualização após a interrupção: a aprovação visual humana posterior da **Katana Walk SW/W V1** supera sua rejeição técnica inicial. Foram empacotadas suas 12 figuras intactas e dois clips de seis fases/400 ms em `outputs/retomada-20261026/katana-normal/catalog-fragment.json`, **sem mudar o catálogo compartilhado**. Fonte SHA-256 `233f498a8e69f2ed2b2fe3b06baf9b069a4df82a069bb930b616e6ec2eed00e5`; atlas `ReqKatanaNormalWalkSW_W20261026ApprovedV1.png`, PPU 162.373737, QA zero erros/13 avisos. Importação e runtime continuam pendentes. A V2 permanece rejeitada. Documento consolidado solicitado pelo usuário: `Docs/Darklight/FECHAMENTO-20261026.md`.

Os XMLs finais da rodada interrompida existem: 71/71 EditMode; 18/19 PlayMode, com falha em `WeaponFormMapDeathAndReturnKeepTheSameSilentPoolAndPreserveStorage` (Expected 1, was 0). Os três casos do novo escopo passaram, incluindo as oito vistas da moto e o idle de 12 quadros. Nova build e ZIP não foram confirmados; não tratá-los como concluídos.

1. Retirar Rock e Presas das opções ativas, liberar moto para Réquiem, consolidar as formas/poderes e migrar saves sem renumerar destrutivamente IDs.
2. Impedir mistura parcial de arte antiga/nova na mesma sequência, mantendo fallback coerente quando faltarem fases. Conferir escala/PPU e preservação dos oito lados reais.
3. **PC04/PC06: reteste contextual aprovado 2/2 em 26/10**, `outputs/retomada-20261026/audio-context-tests.xml`, 20:54:23–20:54:41 UTC. Casos `BreathRequiresItsRealLowHealthConditionAndStopsAfterHealing` e `ClothRequiresItsRealWalkingConditionAndStopsAtRest`. Preservar a falha histórica de 25/10 e registrar a correção/causa no relatório final; o WAV saudável não foi substituído por inferência. Essa aprovação técnica não equivale à avaliação auditiva subjetiva ou validação de toda a trilha.
4. Rodar testes focados após alterações, depois gate necessário da build. Exercitar os fluxos do LDD e registrar o que foi observado no standalone.
5. Produzir build Windows em pasta nova, preservar a anterior, iniciar partida fora do Editor, corrigir bloqueantes, ZIP e relatório curto com caminhos exatos.
6. Criar/validar backup GitHub PRIVATE antes de integrar branches. Auditar refs e alterações, preservar todas as branches, documentar decisões e enviar a branch de integração/PR se os testes essenciais passarem; não integrar a main.

## Etapas posteriores

Somente após o redesign simplificado: Combat AI 2.0, Sangue do Abismo com Lucius Normal/Nexo Fenrir e O Encapuzado, depois site/material promocional. Esses itens não estão concluídos pelo presente inventário. Minecraft permanece pausado.
