# Teste de gameplay — 25/10/2026 — rodada 18h50

**Estado: cobertura automatizada parcial; campanha humana completa não executada.** O nome deste documento acompanha o pedido de acompanhamento do teste completo e não certifica sua conclusão. Não houve nova build nesta rodada.

## Resultado comprovado antes da nova integração artística

`outputs/redesign-armas-20261025-retomada1850/gameplay-current.xml`: **14/14 testes passaram**, zero falhas/ignorados, execução22:37:02–22:39:08UTC de25/10. Esse resultado antecede a integração das134 figuras novas selecionadas; não certifica automaticamente esses novos arquivos importados.

Cobertura efetivamente exercitada pelos14 casos:

- Ciclos repetidos pelos dez mapas, estabilidade de rotas, flags de leitura e recompensas persistidas.
- Pontes atravessadas nos dois sentidos com física fixa real e câmera; visitantes e desafios secretos com recompensa única/recarregamento.
- Resolução de lançamento, perseguição, combos aéreos, arremesso e impacto; seleção de golpes expandidos por modificador e combinações de teclas.
- Seis movimentos da moto atingindo contato sem dano repetido; moto permanece exclusiva de Rock.
- Teclado virtual percorrendo o caminho real de atualização, incluindo a combinação da serra. Isso não comprova teclado/controlador físicos ou todas as combinações de entrada.
- Hooks/pool silenciosos de efeitos, arma/forma/mapa/morte/retorno e preservação do armazenamento.
- Renderização Normal no consumidor da campanha: Idle, primeiros golpes já declarados e quatro fases de Leve1 Katana Rock nas oito direções.

Resultados específicos preservados em `outputs/redesign-armas-20261025-retomada1850/gameplay/`:

| Arquivo | Medida daquela execução |
|---|---|
| `normal-first-lights-result.json` |32 contextos declarados verificados;16 ainda sem arte nova de ataque;120 figuras distintas observadas; renderização na campanha confirmada; áudio desligado. |
| `normal-idle-result.json` |45 contextos,178 figuras distintas, renderização na campanha, save isolado, áudio desligado e `humanPlay=false`. |
| `rock-katana-first-light-result.json` |Oito direções,32 figuras distintas, renderização na campanha e contato/retorno ao Idle confirmados; áudio desligado. É Leve1, sem validar Leve2. |

Essas contagens pertencem ao estado anterior dos arquivos. Os16 contextos de primeiro golpe pendentes correspondiam à chegada das novas fontes Rock/Presas e Rock/Punhos; as novas fontes ainda precisam de importação e verificação final pela coordenação. Não reinterpretar esse resultado anterior como cobertura das134 figuras novas.

## Arte nova e integração

O lote atual tem **134 figuras selecionadas**. Produção, revisão visual e empacotamento são estados separados de importação e verificação no consumidor. Na elaboração deste relatório, o teste focado final após a importação permanecia pendente; não há resultado final presumido para Katana Leve2, novas Presas/Rock ou novos Punhos/Rock.

Prévia de navegador, atlas sem erros estruturais e nomes/contagens corretos não provam contato de golpe, pivôs, transições, escala, importação Unity ou aprovação artística humana. A coordenação registra esses resultados após executar as verificações pertinentes.

## Não executado ou não certificado

- Percurso humano contínuo da campanha inteira, conclusão manual de todos os capítulos e avaliação visual humana integral.
- Controle físico, ergonomia completa dos menus/entradas ou todas as combinações de teclado/mouse.
- Medição de FPS/performance, memória e estabilidade prolongada em hardware-alvo.
- Nova build, teste da build distribuída ou certificação global do jogo e do redesign.

## Atualização final — estado importado

A importação final foi confirmada em `redesign-1850-final-import-retry.log`: 66 folhas de personagens, mais a folha de ícones, e154 clips exatos; quatro folhas da moto em banco separado. Fontes e atlas SHA256 conferidos. Evidência consolidada: `outputs/redesign-armas-20261025-retomada1850/final-validation.json`.

O teste final `gameplay-final.xml` executou cinco casos: **quatro visuais aprovados e um sonoro com falha**. Foram observados192 quadros de idle/48contextos e176 quadros dos primeiros ataques/48contextos nas duas skins Normais; katanaLeve2 observou64quadros/16contextos. O primeiro contato foi preparado por `ConfirmHit` para testar a transição real ao segundo golpe; isso não comprova colisão física nesse caso específico. O mapeamento corrigido KatanaRockLeve1 passou nas oito direções.

O reteste contextual de áudio falhou em **PC04: real context must start its voice**. Não houve voz/sinal de respiração; investigar tanto o estado real de vida preparado pelo teste quanto o disparador em runtime. PC06 não foi alcançado nesse reteste. Não alterar o WAV saudável por inferência. Arquivo: `audio/contextual-playback-retest.json`. Os17efeitos com DSP comprovado na rodada anterior permanecem válidos; não há certificação subjetiva de qualidade nem de música.

Contagem sem duplicação:17casos distintos nesta rodada,16com aprovação e1falha contextual; os três casos Normais foram repetidos no fechamento. Não houve nova suíte EditMode nem nova build nesta etapa. As limitações anteriores permanecem.


## Build de teste solicitada após o fechamento — 25/10,20h45

Build Windows concluída com zero erros e três avisos,1.750.386.461bytes. Executável: `outputs/redesign-armas-20261023/Windows/DarklightRequiem.exe`. Build anterior preservada em `Windows-previous-20261025-233606-7c32f1`, na mesma pasta principal. Os nomes antigos das pastas não indicam a data desta compilação.

Gate técnico renovado:60/60 EditMode e15/15 cenários PlayMode selecionados aprovados. Esta repetição foi exigida pelo construtor da build após mudança dos recursos. Catálogo154clips,570figuras de personagens+12ícones;48figuras da moto em banco separado.19novos SFX habilitados,151recursos de áudio totais. Executável não foi iniciado automaticamente; esta entrega não é teste da build jogada.

A falha contextual PC04 anterior permanece aberta; PC06 não foi alcançado naquele diagnóstico. Os casos desse diagnóstico não fazem parte dos15cenários aprovados do gate de entrega. Não declarar áudio integralmente aprovado nem apagar a falha. O redesign global segue parcial. Recibos e XML em `outputs/redesign-armas-20261025-retomada1850`. A retomada automática já agendada às00h27 continua válida.
