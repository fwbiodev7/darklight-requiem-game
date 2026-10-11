# DARKLIGHT: REQUIEM — laboratório de combate

Entrega de desenvolvimento de 10/10/2026. Projeto independente; a campanha ainda está em montagem.

## Jogar

Abra `Windows/DarklightTreino.exe`. Mantenha o executável junto da pasta `DarklightTreino_Data` e dos demais arquivos de Windows. A janela usa 1280×720, com mundo interno 640×360 e ampliação sem suavização.

| Ação | Teclado/mouse | Controle compatível com Input System |
|---|---|---|
| Mover | WASD ou setas | Analógico esquerdo / direcional |
| Mirar | Cursor | Analógico direito |
| Ataque leve / combo | Mouse esquerdo | X |
| Pesado / carregar | Mouse direito / segurar | Y / segurar |
| Esquiva | Espaço | B |
| Parry | Q | LB |
| Onda do Véu | F | RB |
| Reiniciar parado | E | A |
| Pausa | Esc | Start |
| Menu | Setas, Enter, Esc / mouse | Direcional, A, B |

No menu de pausa, escolha **Alternar treino de defesa** para os alvos emitirem pulsos de treino próximos ao personagem. Isso permite testar esquiva e parry. Os bonecos são objetos de treino, sem inteligência de inimigos.

## Enquadramento solicitado

A câmera foi aproximada: tamanho ortográfico de 5,625 para 3,75, produzindo aumento de 50% na dimensão do personagem em tela. A câmera acompanha o protagonista, respeita os limites do laboratório e encaixa o desenho na grade de pixels. Proporção física, alcance e arquivos originais da arte foram preservados.

`laboratorio.png` é uma captura real da câmera do Unity, sem HUD. `Windows/jogo-windows.png` e `Windows/tela-windows.png`, quando presentes, vêm da execução do jogo compilado; a segunda inclui a interface.

## Entrega atual

- Protagonista com rosto/cabelo da referência, sobretudo preto, katana e spikes; repouso em oito direções, caminhada e golpe com quatro quadros por direção.
- Movimento e mira separados; três leves encadeados, pesado/carregado, contra-ataque após parry e habilidade com cooldown.
- Dano único por alvo a cada golpe, janela de contato, buffer, recuperação, esquiva com invulnerabilidade, pausa, morte e reinício.
- Três alvos de 150 de vida; a primeira katana leve causa 14 de dano. Feedback de impacto e vibração curta quando há controle.
- Mapas de entrada separados para jogo e menu, troca de prompts e tratamento de desconexão/perda de foco.
- Identidade própria do aplicativo: `Fabio/DARKLIGHT REQUIEM`, separada da pasta de dados da base.

## Validação e limites

25 testes de regras/entrada e três testes de cena passaram. Os testes cobrem timing, combos, carregamento, dano único, defesa, pausa, reinício e entrada simulada. Isso não substitui validação com um controle físico específico. A evidência da execução Windows fica em `Windows/teste-executavel.json`.

O build Windows passou, com zero erros. O aviso registrado informa que o Pipeline de automação não tem configuração de execução no Player e fica desativado; ele não é necessário para jogar. O pacote ainda inclui recursos herdados da base e tem aproximadamente 551 MB antes de compressão; a limpeza de distribuição pertence ao polimento posterior.

A execução final com janela visível confirmou golpe (alvo de 150 para 136), pausa congelando o combate e captura completa com interface. O teste também exige pixels visíveis na tela, além da câmera fora da tela. A janela oculta do Windows não produziu repaints e não serviu para validar a interface; a captura final foi obtida com a janela real. A cena usa uma câmera de apresentação sem objetos para exibir o mundo interno. Após essa alteração, os três testes de cena passaram novamente, conforme `testes-playmode-final.xml`.

As folhas de caminhada e golpe têm 32 quadros distintos cada, com recortes medidos e pivôs explícitos. A pose de repouso NW usa o quadro corrigido de caminhada; a fonte estática aprovada continua arquivada. Um shader descarta alpha difuso ao desenhar; não houve limpeza destrutiva do PNG. Há sete avisos de borda nos recortes estáticos estreitos, registrados na inspeção de arte.

O golpe atual é reutilizado pelos perfis de ataque. Animações próprias de pesado, esquiva, parry, dano e morte ainda precisam de produção e revisão. Knockback de inimigos, remapeamento pela interface e o porte integral das melhorias locais de controle de Varginha também estão pendentes.

O piso é técnico. Ainda faltam Ecos, estilo D–SSS, transformações, Vespera, Kael, progressão, narrativa, saves de campanha e os seis capítulos. Os problemas herdados da auditoria da Etapa 00 continuam documentados e não são considerados resolvidos pelos testes novos.

Dark Resonance permanece somente planejado. Os cinco MP3 recebidos foram copiados para o acervo do projeto, sem importar ou tocar neste laboratório.

## Continuação

Fechar animações de combate e validação visual da Etapa 02; depois montar Ecos e sistema de estilo da Etapa 03, seguindo os marcos de aprovação do plano original. Os relatórios de testes e arte acompanham esta entrega.
