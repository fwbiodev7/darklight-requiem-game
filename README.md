# DARKLIGHT: REQUIEM

RPG 2D pixel art hack 'n' slash de Fábio Ronaldo. **Alpha 0.1 — em desenvolvimento.**

Um eclipse abre fendas em Vespera. A katana Requiem escolhe Fábio como seu Portador; Lyra e Kael o acompanham pelos sinais do Projeto Eclipse até o destino da ponte para o Véu.

- Campanha em seis capítulos, encontros, chefes, memórias, diálogos, progressão e dois finais.
- Katana Requiem, adagas Presas do Véu e corrente Laço da Ruptura, com desbloqueios na campanha.
- Skins Requiem e Rock, compatíveis com as três transformações.
- Evolução física em cinco estágios durante um ano narrativo, dos 17 aos 18 anos, com cabelo, cavanhaque discreto e físico proporcional nas duas skins.
- Dark Resonance opcional; HUD e menus góticos; teclado/mouse e controle XInput.
- Mapas ilustrados com colisões, navegação e oclusão. Renderização do mundo em 1280×720 com filtro Point e enquadramento mais amplo; UI em coordenadas lógicas 640×360.

## Jogar

Baixe [a Alpha 0.1 para Windows 64 bits](https://github.com/fwbiodev7/darklight-requiem-game/releases/tag/v0.1.0-alpha), extraia o pacote completo e abra `Windows/DarklightRequiem.exe`.

Leia [os comandos, desbloqueios e transformações](ALPHA-0.1.md). [Site do jogo](https://darklight-requiem.zfabiobrronaldo.chatgpt.site).

## Abrir o projeto

Unity **6000.6.0f1**, Input System atual. Abra esta pasta na Unity Hub e carregue `Assets/Darklight/Scenes/DarklightCampaign.unity`. O laboratório de combate continua disponível separadamente. Dependências no diretório `Packages`.

Para montar e gerar a build, use o método de editor `Darklight.Editor.DarklightCampaignBuilder.AssembleAndBuild`. A saída fica em `../outputs/campanha/Windows`. Não há arquivos de cache da Unity ou histórico do projeto de origem neste repositório.

## Verificação e limites da Alpha

A compilação foi verificada. As capturas do site são renderizações das cenas e interfaces reais na Unity em **modo de edição**, sem gameplay ou áudio. Conforme solicitado pelo criador, não foram executados testes de gameplay ou de controle físico nesta entrega. Balanceamento, campanha completa e navegação precisam de avaliação manual.

Menu musical ES_Metal, configurações completas de áudio/vídeo/acessibilidade e demais propostas cinematográficas permanecem em planejamento separado. A fonte das formas Rock possui sete orientações distintas; o corpo voltado a leste usa a pose direcional Rock com os efeitos da transformação, enquanto as asas usam a projeção mais próxima. As ações das armas possuem folhas direcionais próprias.

Arte original, fontes e importadores preservados. A fonte Press Start 2P acompanha sua licença OFL. Os cinco arquivos de música foram fornecidos pelo criador para o protótipo. Não há concessão de licença de redistribuição para recursos de terceiros implícita neste repositório.
