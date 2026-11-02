# Partículas rápidas por arma — 01/11/2026

Estado: **visual-reviewed; pronto para integração pela coordenação**. Produzido e revisado em staging; não importado nem testado no Runtime/Unity. Dois usos da ferramenta integrada image_gen: fonte inicial e correção dos halos/gutters. Não foram produzidos personagens ou partes anatômicas.

Entrega: `staged/FastWeaponDodge.png` e `staged/FastWeaponDodge.json`. Fonte selecionada preservada em `ArtSources/FastWeaponParticles20261101/FastWeaponDodgeV2-source.png`. PNG copiado byte a byte, SHA-256 `e4da6b903b2b7aec37d9a084408cf8c9ef9e8162f25c5f0087414fda684a39a6`. V1 foi superada; não integrar/publicar V1.

Nove efeitos completos: Katana prateada/azul-gelo fina; Punhos em poeira quente terrosa com detritos; Moto com fumaça azul-escura, metal prateado e faíscas laranja. Cada clip usa onset25/peak50/dissipation70ms = **145ms**, sem loop. Contato em resolução nativa e ampliação nearest2x de um quadro inteiro revisados. O pico tem mais massa que o início e a dissipação nas três armas. Não há caracteres, armas, rodas, mapa, texto ou grade na fonte.

Recortes reais: x `[0,450,1090,1536]`, y `[0,380,680,1024]`; limites escolhidos por inspeção e medição. Todos os nove perímetros têm alpha máximo1/255; nenhum conteúdo alpha>=8 cruza esses limites. Preservados inclusive os pixels residuais alpha1, sem apagar background, quantizar, recolorir ou redimensionar arte. A composição sobre fundo opaco confirma ausência de halo difuso visível. O visualizador da fonte pode exibir RGB de pixels quase transparentes e dar impressão de halo que desaparece na composição correta.

Auditoria de metadados/hash/bounds: **PASS, 9frames/3clips, zero erros**. Dez avisos brutos: nove sobre resíduos alpha1 nos perímetros e um sobre alpha parcial. Estes foram examinados visualmente e pelas métricas em `receipt.json`; não houve limpeza destrutiva para silenciar avisos. Resolução nativa1536x1024; grade lógica de pixel art e paleta indexada não certificadas.

Provas: `preview/contact-native.png`, três GIFs aproximados140ms e três APNGs com25/50/70ms exatos. A conferência de fases foi estática; reprodução animada observada e teste de contato/escala/oclusão no jogo permanecem para a coordenação. GIF não suporta25ms exatos; não usar seus20/50/70ms como timing de gameplay.

Consumidores lidos: `DarklightWeaponRedesignImporter.ImportStandalone` e `DarklightActionParticles`. O importador aceita o contrato entregue. O Runtime genérico atual consome12frames e quatro ações; a coordenação precisa vincular os três novos perfis de Dodge por arma. PPU256 herdado do efeito básico existente; pivôs0.5/0.5 solicitados. Nenhuma edição em Assets, catálogo, Runtime, Unity ou Git foi feita aqui.
