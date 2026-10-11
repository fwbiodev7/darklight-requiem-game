# HUD pixel art — DARKLIGHT: REQUIEM

Visual aprovado pelo usuário em 10/10/2026. Criado com a skill segredo-varginha-pixelart e a ferramenta integrada de imagens; fonte PNG original preservada. O prompt final está em `prompt-arte.txt`.

O kit contém moldura com retrato de Fábio e vida, moldura da Onda do Véu, ícones de katana/parry/esquiva e moldura de vida dos alvos. As dimensões e os seis recortes reais constam em `manifesto.json`; a fonte é 1536×1024 RGBA.

Integração: HUD em Canvas, fonte PressStart2P já existente no projeto, filtro Point, sem mipmaps ou compressão na arte. A interface usa o mesmo espaço lógico 640×360 do mundo, amplia em escala inteira e mantém margens nas proporções de janela diferentes. O shader descarta alpha difuso na exibição sem alterar o PNG aprovado.

Vida do protagonista, vida dos alvos e prontidão/cooldown da Onda do Véu usam os dados existentes do combate. Comandos mudam entre teclado/mouse e controle. Não há barra fictícia de transformação nem implementação de Dark Resonance nesta entrega.

`tela-windows.png` é a captura real com HUD da versão Windows. O executável atualizado fica em `../etapa02/Windows/DarklightTreino.exe`, acompanhado de sua pasta de dados. Os testes de integração ficam em `testes-hud.xml`.

Para testar no Unity, abra a cena `Assets/Darklight/Scenes/DarklightCombatLab.unity` e pressione Play. WASD move, mouse mira e ataca, Espaço esquiva, Q faz parry, F usa a onda e Esc pausa. No menu de pausa, alterne o treino de defesa para ativar os pulsos dos alvos. O projeto aberto é a cópia independente DARKLIGHT.
