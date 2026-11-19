# Lucius Arconte — estocadas de costas, 19/11/2026

Responsabilidade delimitada: uma folha nova NW/N/NE, três fases por direção. Produzida com uma chamada da ferramenta integrada ImageGen e referências completas inspecionadas. Nenhum catálogo, Runtime, Git ou Unity alterado/executado.

## Arquivos e estados

- Fonte preservada: `source-v1.png`, 1247×1261 RGBA, SHA-256 `c74c5b2c8834bcd1eb7d11b6b4844c7bae82045168c051b619f9cb598c7397a8`.
- PNG/JSON/PNG.meta/JSON.meta preparados em `Assets/Darklight/Resources/Darklight/LuciusArconte20261111/LuciusArconteSwordThrustBack20261119V1.*`. PNG idêntico byte a byte à fonte.
- Nove figuras completas revisadas, zero quadros rejeitados. Estado `visual-reviewed`, sem aprovação humana alegada. Copiado a Assets, **sem importação confirmada**, sem integração ao catálogo e sem teste de gameplay.
- `catalog-snippet.json` contém somente as três páginas propostas. A coordenação deve substituir as páginas Attack/SwordThrust/direções 3,4,5 correspondentes, se decidir integrar; não deve acrescentar contextos duplicados.
- Contatos `review-contact-native.png` e `review-contact-game.png` vistos. Fonte inteira e retângulos de apoio vistos. Três GIFs e HTML em `preview/` gerados; reprodução em tempo real não foi assistida. Revisão temporal local feita comparando fases e duração declarada. Importação, playback Unity, contato e transição para Idle continuam pendentes com a coordenação.

## Contrato e revisão

Linhas NW, N, NE. Colunas antecipação, contato reto, recuperação. A extensão diagonal acompanha o mesmo eixo da lâmina e a recuperação a retrai, sem corte horizontal/crescente. Costas permanecem visíveis; rosto não aparece no N. Cabelo branco compacto, sobretudo preto/dourado/verde e lenço azul seguem AttacksBack/Idle13. Cabeça, mãos, corpo, botas, espada e efeitos permanecem em cada figura completa. Não houve colagem anatômica, espelhamento, alteração de escala ou alteração de pixels da fonte.

N tem encurtamento forte: a espada projetada sobe visualmente acima da cabeça durante a extensão. Não há arco de golpe, mas a leitura como estocada em jogo precisa ser conferida; este é o principal ponto de atenção artístico.

Retângulos medidos na fonte preservada, com divisão maior à direita do contato NE para incluir o flash inteiro. Pivôs anotados pelo centro projetado do corpo/apoio; arma e aura excluídas da âncora. Alturas cabelo→bota anotadas: 272,278,269,284,284,285,281,284,282 px. PPU único da folha **147.85803439**, derivado da mediana282px e da estatura física de referência410px/214.97090106361628PPU. PPU e pivôs novos não alteram os assets anteriores.

Durações propostas `[120,200,220]`ms, total540ms igual ao total antigo; `startupFrames=1`, `activeFrames=1`. O consumidor real `DarklightLuciusSpriteBank.AttackTime` remapeia as três fases para startup/active/recovery do ataque, preservando os timings do gameplay. Nenhum evento ou hitbox alterado.

Auditoria final: nove quadros, três clips, **zero erros / dois avisos** (`audit-final/report.json`). Avisos: alpha parcial em mais de10% das amostras visíveis; conferência textual de `.meta` não confirma importação/overrides/runtime. A fonte contém pixels quase transparentes de geração, preservados integralmente. Limites das figuras/efeitos conferidos em alpha>8; nenhuma figura significativa foi cortada. Resolução lógica nativa de pixel não comprovada; não declarar paleta indexada ou ausência de antialiasing. `palette-summary.json` registra128 cores frequentes com alpha>96 e materiais; histograma completo324534cores guardado localmente em `../work/lucius-arconte-thrust-20261119/palette-full.json`.

Proveniência, hashes das três referências e estados em `provenance.json` e JSON do asset. O relatório inicial `audit/` contém um alerta de diferença de PPU devido somente à precisão decimal; manifesto e `.meta` foram alinhados e `audit-final/` é a evidência vigente. Prévia HTML continua com a mesma arte e pivôs; a correção de precisão de PPU não muda seus pixels.

Próximo passo: coordenação assistir os três clips, decidir a integração das páginas propostas, importar uma única vez com os demais trabalhos e conferir estocada/recuperação/Idle nas três direções. Sem conclusão global de Lucius alegada.
