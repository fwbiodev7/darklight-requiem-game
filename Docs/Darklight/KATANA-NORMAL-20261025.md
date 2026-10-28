# Katana — Normal / Réquiem — 25/10/2026

Entregue **staging de 52 figuras inteiras novas em 6 atlas e 13 páginas**, revisado visualmente nas fontes completas e no painel de escala de mundo. Não há modificação do catálogo compartilhado, importação Unity, teste de gameplay ou build por este responsável. Rock, transformações e outros golpes continuam fora deste lote.

Fragmento para integração: `outputs/redesign-armas-20261025/katana-normal/katana-normal-pages.json`. Os 13 registros estão em `visual-reviewed`, `imported=false`, `runtimeVerified=false`, `stagingOnly=true`. O integrador deve copiar cada PNG/JSON para o banco existente e mesclar apenas os contextos exatos; preservar GUIDs existentes e timings. Os arquivos `.pages.json` individuais ficam junto dos atlas.

## Direções realmente examinadas

| Direção | Leve1 | Parada / Idle | Revisão geométrica |
|---|---|---|---|
| S | 4 figuras aprovadas de 24/10 preservadas | 4 figuras novas | Frente com dois olhos; espada na direita anatômica. |
| SW | 4 figuras aprovadas de 24/10 preservadas | 4 figuras novas | Frente diagonal à esquerda; cabeça, peito, quadril e pés concordam. |
| W | 4 figuras aprovadas de 24/10 preservadas | 4 figuras novas | Perfil esquerdo e um olho; braço livre e empunhadura distinguíveis. |
| NW | 4 figuras novas, fonte traseira v3 | 4 figuras novas | Costas em diagonal esquerda, nuca e pequena lateral esquerda do rosto. |
| N | 4 figuras novas, fonte traseira v3 | 4 figuras novas | Costas centradas, nuca e calcanhares; sem olhos/rosto frontal. |
| NE | 4 figuras novas, reparo integral da direção v4 | 4 figuras novas | Costas em diagonal direita com lateral direita; mão da espada permanece ligada ao ombro direito. |
| E | 4 figuras novas | 4 figuras novas | Perfil direito e um olho; braço próximo é o direito anatômico. |
| SE | 4 figuras novas | 4 figuras novas | Frente diagonal direita distinta de E; mantém assimetrias e mão dominante. |

Assim, o **Leve1** soma as 12 figuras anteriores com 20 novas para 8 direções. Isso não cobre os demais ataques, combos ou ações. As 32 figuras de Idle têm quatro fases desenhadas por direção: neutra, inspiração, piscada/expiração e retorno. Na vista traseira, respiração/roupa substituem a piscada invisível. O primeiro quadro neutro é a parada estática; não foi contado como uma animação extra.

## Escala e anatomia

Painel: `outputs/redesign-armas-20261025/katana-normal/world-scale-review.png`; dados por quadro em `world-scale-review.json`. Todos os 16 representantes usam **112 pixels de exibição por unidade de mundo**, calculados pelo PPU real da respectiva página, e a mesma linha de apoio. Não há ajuste individual para preencher a célula. O painel mostra guarda recuperada do ataque e parada neutra, mantendo inclusive a postura mais baixa da guarda W aprovada.

O PPU é único por folha e usa coroa até sola, excluindo arma erguida, rastro e extensão da capa. As medições em pixels e PPU inferido/selecionado constam de `ppuProvenance` em cada JSON. Novas guardas representam aproximadamente 1,980–2,037 unidades; novos repousos 1,917–1,998 unidades. A guarda W anterior mede aproximadamente 1,809 unidade **naquela pose flexionada**; isso não é uma medida da altura em pé. Fontes antigas não foram redimensionadas.

Guia segmentado manual, no arquivo `anatomy-landmark-guide.json`: coroa, base do pescoço, cintura visível e sola. Cabeça, tronco e pernas foram comparados na escala do painel com a guarda S canônica. A roupa encobre a articulação do quadril e as poses flexionam joelhos; os segmentos são guias aproximados, não uma certificação de proporções. Incerteza de cerca de ±10–18 pixels nos pontos internos e ±3–8 pixels nas extremidades, correspondendo aproximadamente a ±0,03–0,13 unidade conforme a fonte. Não alterar escala para compensar uma pose curvada nem deformar anatomia. A revisão do painel não identificou uma redução geral para aparência infantil; o enquadramento/câmera e transições ainda precisam de conferência no consumidor Unity.

## Verificação e limites

- 52 figuras, 13 páginas e 6 atlas preparados; **zero erros estruturais**, 60 avisos de paleta/alpha/variação de altura para inspeção.
- Toda amostra RGBA não transparente selecionada foi preservada entre fonte e atlas, com igualdade de reconstrução, contagens e SHA-256. Não há montagem anatômica, espelhamento, rotação plana, recoloração ou deformação.
- Paletas, contatos, HTML e GIFs de inspeção ficam em `qa/<atlas>/`. A fonte grande e os muitos tons/alpha não certificam uma grade nativa de pixel art indexada; não houve quantização destrutiva automática.
- Os timings do Leve1 são preservados por nomes de fase exatos do banco existente. A prévia usa 55/75/70/70 ms. Idle usa 350/250/180/350 ms, separadamente de ataques.
- O catálogo Unity e `prepare_weapon_redesign.py` permaneceram idênticos durante o preparo. Nenhum Play, áudio, save ou teste histórico foi contabilizado nesta validação.

## Rejeitadas e sucessores

As quatro tentativas de Walk SW/W continuam **rejeitadas**: repetem a perna líder ou não distinguem suficientemente a orientação. Não foram integradas nem contadas. Fontes traseiras anteriores com mão trocada e o reparo intermediário de NE sem diagonal clara também não são entregáveis. Foram arquivadas fora de Assets, com mapa de movimentação e fontes preservadas.

Para os próximos responsáveis: Normal conserva castanho natural, sem mechas de transformação/cristais vermelhos. Por novo pedido humano, **Ruptura exige vermelho forte também na nuca e Eclipse violeta forte também na nuca**, seguindo o padrão de Presas Ruptura v2 aprovado. Manter mesma idade, cabeça/tronco/pernas e porte canônico em todas as formas/armas; nenhuma troca de PPU pode ocultar uma mudança de anatomia. Este lote não gera nem altera essas transformações.

Pendentes aqui: Walk SW/W e seis direções restantes; Rock; demais golpes/combos e ações; integração/inspeção em Unity; runtime silencioso e build pela coordenação principal.
