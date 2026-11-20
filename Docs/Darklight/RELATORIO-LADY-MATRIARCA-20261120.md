# DARKLIGHT: REQUIEM — caminhada da Lady e interface da Matriarca Rubra

20/11/2026

## Lady

O projeto já tinha cinco sprites de caminhada para cada uma das oito direções. A apresentação zerava a fase da passada em quadros renderizados entre passos da física. A correção reaproveita os 40 sprites existentes, mantém a pose entre passos e alterna quadros conforme a distância real percorrida. A direção da passada usa o deslocamento observado. Quando Lady para, a apresentação volta ao repouso.

## Matriarca Rubra

A bossbar usa uma nova moldura pixel art carmesim e bronze, com rosa central, nome completo, vida atual e máxima e indicação do dano recente. No diálogo, o retrato usa um enquadramento da arte oficial que valoriza o rosto, o cabelo e o halo. A imagem da moldura está em BarraMatriarcaRubra.png.

## Arquivos de runtime alterados

- DarklightCampaignAlly.cs
- DarklightLadyPresentation.cs
- DarklightCampaignDialogueUI.cs
- DarklightCampaignUI.cs

## Build Windows x64

O código foi compilado na Unity 6000.6.0f1 para Windows x64. A entrega reaproveita os dados da build anterior e inclui a DLL recém-compilada. A build de 20/11 fica na área de releases.

A validação em jogo fica para o teste manual. Nenhum teste automático ou Play Mode foi executado.
