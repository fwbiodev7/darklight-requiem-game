# Serafim — entrega de prévia 14/11/2026

Estado: **approved-design / preview-pending-temporal**. Candidata; não importada e não verificada em runtime.

A aprovação explícita “serafim tambem curti pode manter essa versão” congelou a geração v2. A edição v3 estava em voo durante esse feedback: foi preservada sem ser eleita. O nome histórico “v2-aura-excessive” registra uma observação anterior à aprovação e não é reprovação posterior. Nenhuma nova geração ou alteração criativa foi realizada depois do congelamento.

## Arquivos de entrega
- Fonte selecionada: `serafim-transition-candidate-approved-design.png`, RGBA 1774×887.
- Atlas técnico: `serafim-transition-atlas-final.png`, RGBA 2480×1240, 4×2 células 620×620.
- Metadados: `serafim-transition-final.json`; recibo e SHA-256: `receipt-final.json`.
- Prévia: `../../previews/serafim/serafim-transition-final.gif`.
- Contatos claros/escuros: `../../previews/serafim/contact-{light,dark}-final.png`.
- HTML com passo de pose, zoom inteiro, velocidade e cor do fundo: `../../previews/serafim/interactive-final/preview.html`.
- Prompts integrais v1/v2/v3 e fontes imutáveis preservadas nesta pasta; ferramenta integrada imagegen utilizada.
- Paleta completa: `palette-final.json`.

## Preservação e validação
O primeiro retângulo INTEIRO [0,0,443,443] da v1 substitui a mesma célula da v2 para manter Normal sem aura. Todos os demais pixels das outras sete células da v2 foram preservados. Asserções de igualdade RGBA executadas e aprovadas; não houve montagem anatômica, deformação, recoloração nem redimensionamento dos desenhos.

As oito figuras completas foram extraídas em retângulos reais, reposicionadas em células mais espaçosas e mantidas em escala constante. Apoio dos pés e centro horizontal do corpo foram anotados manualmente; efeitos e asas não definem o centro do personagem. As fases são concentração, flexão breve, ascensão, recepção, manifestação das asas, consolidação, descida e postura final.

Timings derivados do consumidor atual: 4.600ms, soltura da forma em .52, subida visual .12→.34, sustentação até .76, retorno ao chão em 1.0. O GIF usa 138 amostras de apresentação, armazenadas como 65 imagens GIF após fusão de holds. São **oito poses desenhadas**, e nenhum desses deslocamentos é anunciado como desenho novo. A posição aérea do GIF é deslocamento inteiro de apresentação, amplitude64px; os .62unit do consumidor são referência, não calibração de importação.

Auditoria técnica: **0 erros, 1 aviso** (mais de10% das amostras visíveis têm alpha parcial). Fonte e atlas mantêm alpha real, incluindo aura aprovada. Contato final claro inspecionado; figuras, botas, katana, cabelo e asas completos. Rosto e pele legíveis. Prévia temporal permanece para revisão do coordenador; nenhum teste Unity, Gameplay, áudio ou controle físico foi executado.

Limites reais:340.072 cores RGBA visíveis na paleta, alpha parcial e resolução lógica de pixel art não estabelecida. PPU no manifesto é marcador de prévia herdado do Normal; precisa de calibração caso se autorize integração. Este material não altera fontes Normal calibradas, Assets, Runtime, catálogo nem Git.

SHA-256 fonte selecionada: `ca71d7ece5f71518dc30c8e60c1217df392e8c405158f84b108154a2908e34fe`.
SHA-256 atlas: `7dba4cd4afc3b37008df16922fac4945e575679e51c2140c3fa895cf2cabe1ed`.
SHA-256 GIF: `e703c888dba657f4f29e2fb42f5454657d8246918c6bac2a55c1d7fdab7c093c`.

