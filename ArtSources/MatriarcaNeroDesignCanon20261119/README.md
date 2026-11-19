# Matriarca Rubra e Nero — pilotos canônicos Idle-S

Produzidos e revisados visualmente em 19/11/2026. Escopo: **uma figura completa estática por personagem**, sem outras direções, ataques, variante Nero Fraturado ou animação. O nome Nero segue a direção mais recente; Kael permanece apenas como identificador histórico do consumidor consultado.

## Arquivos e estados

- `sources/*-Idle-S-generated-v1.png`: fontes RGBA integrais da ferramenta integrada ImageGen, uma geração por personagem.
- `staging/Resources/Darklight/CampaignDesignCanon20261119/*IdleS20261119.png` e `.png.meta`: cópias byte a byte das fontes, importador Sprite Single, Point, sem mipmaps/compressão, pivô de solo e PPU corporal.
- `previews/Comparacao-Fabius-Matriarca-Nero-1x.png` e `2x.png`: figuras completas no mesmo solo; somente a apresentação usa escala uniforme nearest-neighbor.
- `previews/*-landmarks.png`: réguas para conferir os marcos manuais, sem alterar as fontes.
- `pilot-manifest.json`: hashes SHA-256 reais das referências, fontes, PNGs de staging e metas; GUIDs, resoluções, marcos e estados separados.
- `generation-prompts.txt`: prompts utilizados e papel de cada referência.

Produzido: sim. Revisado visualmente: sim. Staging: sim. Assets/importação Unity/runtime/build/publicação: **não executados**. Nenhum arquivo compartilhado, Runtime, catálogo, Git ou Unity foi alterado/executado por esta frente.

## Referências e escala

Autoridades de desenho completas: `Docs/Darklight/References/CampanhaOficial20261119/Matriarca-Rubra.png` e `Nero.png`. Ambas são RGB opacas, por isso não foram apresentadas como sprites transparentes prontos. Fabius Normal: `../work/lyra-piloto-20261119/Fabius-Normal-S.png`, corpo de 302 pixels, PPU 157.5757576, escala visual 1.325. Nenhum pixel/pivô de Fabius foi modificado.

| Piloto | Canvas fonte/staging | Corpo nativo | Relação corporal | PPU | Pivô normalizado (bottom-left) |
| --- | --- | --- | --- | --- | --- |
| Matriarca Rubra | 1309 × 1201 | 832 px | 1.08 × Fabius | 401.959254118224 | 0.504201680672, 0.069109075770 |
| Nero | 1415 × 1112 | 950 px | 1.00 × Fabius | 495.685330198676 | 0.572438162544, 0.075539568345 |

PPU = altura corporal nativa × PPU de Fabius ÷ (302 × relação corporal). Usar escala visual 1.325. Marcos manuais: Matriarca cabelo Y286, sola Y1118, eixo X660; Nero cabelo Y78, sola Y1028, eixo X810. Halo e espada não determinam altura corporal. A revisão compara desenho/proporções antes da calibração: PPU não foi usado para corrigir anatomia.

Matriarca preserva vestido aristocrático preto/carmesim com dourado, prata no cabelo longo, olhos rubros, asas, halo de espinhos com rosa/crânio e espada rubi. Nero preserva cabelo prateado, olhos azuis, cicatriz, ombro tatuado, casaco azul/preto com dourado, fitas e espada azul. A pose mantém arma na mão direita, à esquerda do observador. As figuras não foram montadas por partes.

## Consumidor e limites reais

Consultados `DarklightEnemy.cs` (`Show`, `ShowAttack`, `ShowEnemyPilot`) e `DarklightEnemyAttacks.cs` (`EnemyAttackTiming`). O consumidor legado normaliza pelo bounds completo de `poses[0]`. Aplicar essa normalização novamente pode invalidar a escala corporal, principalmente por causa do halo. A integração coordenada deve respeitar PPU corporal e escala 1.325. O staging não muda o consumidor.

Tempos atuais de ataque de chefe: preparação 0.60 s, impacto 0.25 s, recuperação 0.22 s. Nenhum tempo/evento foi criado ou alterado. Os pilotos não representam cobertura de caminhada/combate e não devem substituir sequências inteiras sem integração específica.

Alpha real confirmado: existem pixels 0 e 255. A ferramenta gerou alpha parcial em bordas e pixels residuais de alpha 1–7 fora da silhueta principal (Matriarca 72.953; Nero 35.440). Foram preservados integralmente, sem limiar destrutivo ou chroma key. A prévia em fundo cinza não mostrou borda opaca preta; shader/fundo final e importação Unity continuam pendentes. Os metas receberam conferência estática; isso não comprova importação.

Pendências: integração pela coordenação, avaliação na Unity quando autorizada e decisão explícita antes de ampliar para direções/ações ou Nero Fraturado. Não houve teste automatizado, Play Mode, build ou push.
