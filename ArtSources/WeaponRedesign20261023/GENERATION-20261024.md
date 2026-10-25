# Fontes e geração — 24/10/2026

Ferramenta usada: geração integrada de imagens (ImageGen). As fontes de 23/10 foram recuperadas integralmente, sem montar personagens por partes. Cabeça, corpo, equipamento e armas permanecem no mesmo quadro. A preparação conserva cada pixel RGBA selecionado, registra retângulos reais e usa somente translação inteira para organizar os atlas.

## Fontes novas

- `ReqDaggersNormalIdleC-source.png`: oito figuras completas, quatro fases de repouso L e SE; fonte original `exec-546f5739-4a7d-49dd-8f47-f6e32f90bf54.png`, gerada nesta conversa.
- `ReqDaggersNormalIdleB2-source.png`: doze figuras completas, quatro fases de repouso NO/N/NE; fonte original `exec-b19dae37-260e-47e1-8461-bfe8506004cc.png`, gerada nesta conversa.
- A fonte anterior `ReqDaggersNormalIdleB-source.png` foi preservada localmente, mas não integra o catálogo: as orientações divergiram entre colunas e duas figuras se conectaram. Não é candidata aprovada nem fonte de produção/publicação.

Referências visuais inspecionadas antes da geração: `ReqDaggersNormalIdleA-source.png` (identidade, roupa, equipamento e proporções), `references/equipment-four-forms.png` (construção das adagas) e `ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png` (olhos/pele). A fonte completa do idle e a referência completa dos olhos foram examinadas; nenhum componente anatômico foi recortado ou transplantado.

## Especificação dos prompts

As duas chamadas pedem figuras inteiras com cabelo castanho/mechas vermelhas, cristais vermelhos, casaco preto com forro carmesim, cinto/corrente, botas, duas adagas curtas serrilhadas de metal escuro e fissuras carmesim, câmera elevada RPG e clusters de pixels nítidos. Pele base `#F6AE82`, íris `#C40814`, sombra `#5A0000`, pupilas/brilho preservados. Fundo realmente transparente, sem chão, sombra projetada, texto, grade ou aura pintada; espaços transparentes largos entre figuras.

1. **L/SE, 8 figuras em 2 linhas × 4 colunas:** linha L com nariz/botas para a direita e perfil direito; linha SE com peito/nariz/botas para baixo-direita. Colunas repouso, inspiração, piscar/expiração, retorno. Pés e empunhaduras estáveis; nenhuma orientação oposta misturada na linha.
2. **NO/N/NE, 12 figuras em 3 linhas × 4 colunas:** costas/esquerda, costas diretas e costas/direita; rosto/olhos ausentes quando ocultos. Quatro desenhos de repouso com respiração/tecido discretos, sem deslocamento de canvas. Margens e gutters amplos para evitar contato entre figuras.

Os prompts proíbem alterar idade/roupa/proporções, alongar as adagas, espelhar sprites para fingir direções e compor anatomia por partes. A especificação não comprova resultado: fontes e contatos foram inspecionados e a auditoria de RGBA, alpha, contagem, pivôs e contexto foi executada.

## Integração

Jobs reproduzíveis: `import-jobs-20261024-daggers.json`, `import-jobs-20261024-normal-back.json` e `import-jobs-south-20261024.json`. O último recupera seis fontes de katana/punhos feitas em 23/10. Catálogo total: 17 folhas, 159 figuras completas e 48 páginas de contexto. Os 12 ícones de equipamento são separados.

A fonte original não é alterada para corrigir arte. PPUs fixos por folha, pivôs no apoio dos pés, Point, sem compressão/mipmaps. Sem recoloração, quantização global, suavização ou deformação anatômica. Fontes grandes não certificam pixel art indexada: os avisos técnicos de cores/alpha parcial permanecem documentados.

A prévia de navegador passou para 17 folhas, 48 clips e 159 quadros; esse resultado não substitui a importação e a apresentação real no Unity. Os resultados finais Unity/build e as limitações constam no relatório da rodada de 24/10.
