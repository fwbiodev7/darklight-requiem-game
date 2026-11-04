---
name: darklight-gothic-eclipse
description: Melhorar e validar a atmosfera Gothic Eclipse do DARKLIGHT nos mapas existentes, com luar, chuva, névoa, profundidade e efeitos reversíveis. Use para direção visual e integração atmosférica, preservando pixel art e leitura do combate.
---

# Gothic Eclipse

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz. Consulte `Docs/Darklight/GOTHIC-ECLIPSE-REFERENCIAS-20261102.md` para localizar a referência aprovada e `GOTHIC-ECLIPSE-ESCOPO-ENXUTO-20261102.md` para limites ainda compatíveis com a ordem atual. Inspecione a imagem inteira antes de julgar semelhança.

## Reutilizar infraestrutura

Inspecione `Assets/Darklight/Runtime/Campaign/DarklightAtmosphereController.cs`, shader/atlas, câmera e render pipeline assets realmente usados por Graphics/Quality Settings. URP no manifesto não comprova Renderer2D ativo em toda qualidade. Não recrie o controlador nem instale outra pipeline por conveniência.

Pacotes Antigravity são insumos: conferir relatório, manifesto, hashes, bases e conflitos antes de integrar somente o necessário. Patch com preflight falho não é implementação validada. Evite Attach, materiais e renderizadores duplicados.

## Comparação visual

Capture região 0 no mesmo enquadramento com efeitos desligados/ligados. Trabalhe luar azul-prateado, sombras/profundidade, névoa rasteira/em camadas, chuva em planos e iluminação dramática; reflexos molhados e partículas devem servir à referência. Relâmpagos controlados não podem ocultar telegráficos de combate. Use camadas/máscaras que preservem pele, olhos, personagens e HUD.

Não refaça mapas, geometria, colisores ou enquadramento aprovado. Não aplique tint global que transforme o personagem. Preserve contornos, clusters e resolução; não copie defaults de câmera externos. No caminho URP, verifique seu PixelPerfectCamera antes de cogitar pacote standalone.

## Integração

Compare referência e cena anterior na resolução de jogo. Valide desligar, intensidade/qualidade zero, pausa, mudança de região, load e reentrada: restauração de cores, sem renderizadores órfãos/componentes acumulados. Confira teto existente de 88 renderizadores de chuva antes de alterar orçamento; documente eventual necessidade de mudança.

Meça custo informando hardware, cena, duração e condições; não declare 60 FPS por estimativa. Só estenda para regiões 1–9 após revisão autônoma e correção da região 0. Entregue comparação visual, parâmetros, reversibilidade, medições e lacunas. Código compilado ou preset criado não certifica atmosfera aprovada.
