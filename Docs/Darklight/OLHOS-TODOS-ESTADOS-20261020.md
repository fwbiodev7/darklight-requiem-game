# Olhos e pele — 20/10/2026

Autoridade dos olhos: `ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png`, captura 19/10 às 20:10:51. Preferência mais recente: **mais vermelho**, próximo à captura, base #C40814 e sombra #5A0000. Os guias de vinho/marrom e #80172C/#450B18 foram superados. Preservar desenho, pupilas, branco/brilhos e pele limpa. Mechas/cristais roxos, roupa e armas não acompanham a recoloração da íris.

Inventário anterior à integração: 61 páginas normais, 183 descritores transformados, 260 texturas distintas, 5.367 retângulos. Todas as fontes encontradas e seus PNGs preservados por SHA-256. As duas folhas adicionais têm 12 figuras completas.

Aplicado: o canal R semântico de íris usa a mesma rampa nos três shaders e quatro formas. Renderização testada com pupila/branco, cabelo roxo, arma vermelha, pele e alpha. Não foi usado filtro global de vermelho. As novas folhas foram inspecionadas completas, incluindo a limpeza facial da folha de oito adagas v2.

Pele: `CanonicalComplexion.json` cobre as 260 fontes anteriores e duas novas. Mediana-base [246,174,130] do repouso Requiem; correção de pigmento na renderização, preservando fontes e sombras, desativada no estado de morte. NPCs não receberam essa paleta.

**Ainda falta:** muitos sprites autorados têm somente ownership, RGB zero na máscara, e não fornecem o canal de íris. Suas cores antigas e eventuais olheiras precisam de correção explícita. Propostas automáticas de região facial não foram aplicadas: havia falsos positivos, sobretudo mãos confundidas com rostos em costas. Atlas físicos Support têm oito direções/quatro fases, distintos dos clips compostos. Conferir oclusão, olhos fechados e costas antes de utilizar anotações. O pedido global de olhos ainda está aberto; seguir `PENDENCIAS-20261020.md`.
