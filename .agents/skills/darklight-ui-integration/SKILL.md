---
name: darklight-ui-integration
description: Integrar os menus pixel art aprovados do DARKLIGHT à interface existente, com navegação por teclado, mouse e controle e preservação de dados. Use para menu principal, mapa, configurações, arsenal, diário e missões.
---

# Integração dos menus

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz. Localize/inspecione referências aprovadas e assets antes de substituir visual. Sem referência acessível, registre a lacuna; não invente aprovação.

## Reutilizar a interface

Inspecione `Assets/Darklight/Runtime/Campaign/DarklightCampaignMenu.cs`, `DarklightCampaignUI.cs`, `DarklightCampaignSettings.cs`, `DarklightCampaignBindings.cs` e `Assets/Darklight/Editor/DarklightMenuBuilder.cs`. Identifique o caminho que gera a interface da cena/build atual; editar prefab não usado não integra menu. Preserve HUD e artes aprovados.

Mapeie telas, rotas, ações, retorno, seleção/foco, dados e persistência. Integre principal com Fabius no canto, mapa redesenhado, configurações, Arsenal (Katana, Punhos/Grevas, Moto), diário e missões existentes. Use fontes pixel art licenciadas já disponíveis e identidade gótica consistente.

Remova entradas e rotas visíveis de Artefatos e Coleções conforme ordem de 04/11; preserve dados necessários e compatibilidade de save. Não apague inventários/enums/recompensas só porque a navegação mudou.

## Entrada e apresentação

Reutilize Input System/bindings atuais. Confira foco inicial, ordem de navegação, voltar/cancelar, confirmar, seleção após modal e retorno à tela chamadora. Alternar mouse/controle não deve disparar ações duas vezes; menus pausados usam timing adequado e bloqueiam input de combate.

Confira Canvas Scaler, anchors, fontes, contraste, clipping, estados normal/foco/pressionado/desabilitado e proporções de janela. PixelPerfectCamera de mundo não substitui dimensionamento UI. Evite escalas que borrem raster/fontes.

## Validar

Percorra principal → continuar/carregar → pausa → mapa/arsenal/diário → opções → voltar. Teste configurações com preferências isoladas, estados vazios/indisponíveis e preservação de dados ocultos. Inspecione `Assets/Darklight/Tests/PlayMode/DarklightMenuTests.cs` antes de executar casos pertinentes.

Registre screenshots, rotas, entrada, resolução e falhas. Teste simulado não é teste com controle físico. Respeite restrição vigente de sons. Reporte arte copiada, bindings integrados, revisão visual e runtime como estados separados.
