---
name: darklight-transformations
description: Integrar aparência, animações, efeitos e gameplay das transformações canônicas de Fabius e Lucius no DARKLIGHT, reutilizando estados e recursos existentes. Use na etapa autorizada de transformações, sem criar formas extras ou refazer sistemas funcionais.
---

# Transformações canônicas

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz e `Docs/Darklight/CORRECAO-CANONICA-FORMAS-20261102.md`. Criar esta skill não inicia a etapa de transformações.

## Contrato

- Fabius: Normal, Ruptura, Eclipse Absoluto. Lucius: Normal, Nexo Fenrir.
- Não existe forma Abissal. Moto/Cavaleiro do Abismo é equipamento/mecânica autorizada, sem quarta forma. Preserve histórico e compatibilidade de saves sem reativar conteúdo descartado.
- Pele, olhos, idade, altura e corte/volume de cabelo permanecem constantes nas versões adultas. Ruptura/Eclipse mudam mechas/cristais conforme referência, sem tingir pele/íris. Normal não herda transformação. Lucius mantém olhos azuis e identidade própria.

## Integrar sem duplicação

Inspecione `Assets/Darklight/Runtime/Character/DarklightFormController.cs`, `Assets/Darklight/Runtime/Campaign/DarklightCampaignTransformations.cs`, `DarklightEnemyTransformation.cs` e consumidores de recursos, habilidades, animações, shaders e persistência. Rastreie qual controlador governa cada contexto; não aplique dois ao mesmo ator sem conferir contratos.

Mapeie entrada, condição, custo, duração, saída, cooldown, animação/efeito e restauração por forma. Reutilize estados/artes compatíveis; revise componentes Antigravity por hashes/conflitos/contrato. Não copie enums/índices antigos. Identifique mapeamento real dos saves antes de mudar valores serializados.

Confira se PropertyBlocks/shaders tingem pele/olhos; use efeitos localizados e restauração completa. Arte ausente não se resolve fingindo conclusão por filtro. Se necessário produzir raster, use `darklight-pixelart-animation`, com figuras inteiras e escopo seletivo.

## Evidência

Teste entrada/saída, custo insuficiente, expiração, pausa, morte, troca de arma/cena, save/load e índice inválido. Verifique dano, colisão, input e retorno ao Normal sem resíduos de aura, multiplicadores, áudio ou materiais. Preserve saves/preferências reais e autorização vigente de áudio.

Revise formas no tamanho de jogo e direções/ações afetadas, com atmosfera ativa. Registre artes disponíveis, mapeadas, importadas, revisadas e executadas separadamente. Testes em `Assets/Darklight/Tests/PlayMode/` são candidatos a execução focada após inspeção; resultados históricos não certificam a integração atual.
