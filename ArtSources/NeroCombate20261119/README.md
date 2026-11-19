# Nero Fraturado — piloto de combate S, 19/11/2026

Produzido e revisado como **candidato com ressalvas**, somente em `ArtSources/NeroCombate20261119/`. Uma geração de oito figuras completas e uma correção localizada da figura inteira StepB. Nenhuma alteração de Assets, Runtime, catálogo, Git ou execução da Unity.

## Entrega

- `staging/NeroFraturadoCombatS20261119.png`: atlas RGBA 2048×1280, 4×2 células512×640, sem escala/rotação/retoque dos desenhos.
- `.png.meta`: TextureImporter Multiple, Point, sem mipmaps/compressão, max2048, oito nomes únicos `NeroFraturadoCombatS20261119_00` a `_07`, GUID `ce10fe36c9ff45a4ad4415aa44fbc810`.
- `.json`: retângulos top-left, pivôs bottom-left, ordem explícita, tempos, procedência e limitações.
- `sources/`: duas fontes integrais1448×1086; `whole-frame-provenance.json` comprova igualdade RGBA dos sete quadros v1 e do quadro2 inteiro extraído da correção v2.
- `previews/contact-compact.png`: contato visto; `previews/interactive/preview.html` e quatro GIFs gerados.
- `audit-final/report.json`: oito quadros, zero erros, dois avisos reais. `palette.json`: resumo das64 amostras RGBA mais frequentes; extração completa observou317440 cores, incluindo alpha baixo. Não representa paleta restrita de pixel art.
- `references.json`, prompts e `delivery-hashes.json`: fontes e hashes reais.

## Ordem e consumidor

| Índice | Uso |
|---|---|
|00|Idle candidato de transição/referência. A coordenação manterá o repouso canônico OfficialCast/Nero como poses[0].|
|01|StepA, perna direita adiante|
|02|StepB corrigido, perna esquerda adiante|
|03|Guarda|
|04|Preparação inicial|
|05|Antecipação tardia|
|06|Contato/seguimento decisivo|
|07|Recuperação|

Consumidor real consultado: `Assets/Darklight/Runtime/Campaign/DarklightEnemy.cs` (`PresentEnemyLocomotion`, `Show`, `ShowAttack`) e `DarklightEnemyAttacks.cs` (`EnemyAttackTiming`, `PresentEnemyWindup`, `TickEnemyAttack`). Identificador técnico vigente: KaelFractured. Preparação600ms: quadro04 por432ms e05 por168ms; contato06 por250ms; recuperação07 por220ms. O dano continua na entrada do contato06. Nenhum evento ou tempo foi alterado.

A caminhada real troca quadro pela distância de0,36 unidade por passada. Os180ms no JSON/GIF são somente demonstração da alternância, não novo relógio de gameplay. O runtime existente usa flipX para orientar inimigos; isso pode inverter cicatriz/tatuagem e não constitui cobertura direcional aprovada.

## Escala e identidade

Corpo idle medido do cabelo Y55 à solaY480=425px; espada e fitas excluídas. PPU **221.75396350993378** =425×1,325÷2,5394134611477823. Escala visual **1,325**, altura corporal **2,5394134611477823**, igual à base Fabius indicada pela coordenação. Pivô de cada célula **(0,5;0,125)**, apoio localtop-left(256;560). Todas as figuras foram deslocadas por offsets inteiros; pixels de desenho preservados.

Referências integrais inspecionadas: desenho oficial Nero e idle aprovado OfficialCast/Nero. Cabelo prateado/lavanda, olhos azuis, cicatriz à esquerda anatômica, ombro esquerdo exposto/tatuado, roupa azul/preta com dourado, amuleto e espada azul preservados. Não há nova armadura nem grandes arcos de aviso. Corrupção permanece discreta; esta folha não introduz uma transformação extensa.

## Revisão e limites

Contato completo inspecionado: oito corpos/armas inteiros, sem lâmina/pé cortado, apoios coerentes, alternância real corrigida. Os quadros04 e05 são antecipações próximas intencionais;06 mostra transferência de peso e término do corte;07 retorna à guarda. StepB flexiona mais o joelho queStepA: é piloto de dois contatos, não ciclo completo de quatro fases.

**Comprimento projetado da lâmina varia entre poses**: down-left nos passos aparenta menor que guarda/contato. A cor e a arma permanecem identificáveis, mas rigidez exata de comprimento não está certificada. Esta ressalva deve ser avaliada antes de aceitar integração definitiva; não houve terceira geração.

Avisos técnicos finais: mais de10% dos pixels visíveis têm alpha parcial; inspeção de meta é apenas textual, sem importação/runtime. Pixels residuais alpha1–7 foram mantidos, sem corte destrutivo. Os sprites exibem clusters legíveis em escala de jogo, mas a fonte gerada tem muitas cores/alpha e não é paleta nativa estrita.

A prévia temporal foi gerada, mas **não foi assistida**: o navegador recusou URL file por política de segurança. Não houve contorno do bloqueio. Revisão quadro a quadro/contato foi realizada; reprodução temporal e avaliação em jogo continuam pendentes.

Estados: produzido=sim; revisado visualmente com ressalvas=sim; staging=sim; copiadoAssets=não; importadoUnity=não; runtime=não; publicado=não. Decisão de integração e conferência temporal pertencem à coordenação. Não é conclusão global do pacote Nero.

