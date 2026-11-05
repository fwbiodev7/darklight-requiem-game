---
name: darklight-animation-qa
description: Inspecionar sprites, clips e prévias do DARKLIGHT para localizar frames deslocados, pivôs incorretos, descontinuidade e regressões de identidade. Use para diagnóstico e evidência visual, sem regenerar arte automaticamente durante a auditoria.
---

# QA de animação DARKLIGHT

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz e delimite personagem/arma/forma/ação/direção. Consulte o recibo mais recente e preserve arquivos corretos. Para critérios de arte, leia apenas a referência pertinente de `Tools/Skills/pixel-art-sprite-sheet-master/SKILL.md`.

## Diagnosticar antes de corrigir

1. Reproduza com clip, direção e frame identificados. Compare atlas/fonte, descriptor, importador, AnimationClip e posição do renderer. Separe deslocamento desenhado, retângulo/pivô errado, movimento do objeto e deslocamento esperado da ação.
2. Use helpers existentes em `Tools/AnimationPreview/` e `Tools/Validation/audit_unity_metadata.py` após conferir entrada/saída e efeitos. Não execute integração/publicação para obter prévia. Prefira saídas pequenas e logs em `work/`.
3. Exiba figuras inteiras alinhadas pelo pivô/contato, sem normalizar bounding boxes automaticamente. Registre coordenadas/timings originais; bounding box crescente por arma/casaco não demonstra deslocamento corporal. Compare tamanho de jogo, loop e frames adjacentes.
4. Verifique alternância de apoio, deslizamento, altura corporal, cabelo, rosto, olhos, mão da arma, trajetória de ataque e retorno ao Idle. Identidade aprovada não é necessariamente referência mecânica de passada: confirme contatos A/B.

## Resultado acionável

Entregue tabela pequena: clip/direção/frame, evidência, origem provável, correção proposta, quadros preservados e validação faltante. Se for metadado, corrija somente o contrato comprovadamente errado; se for desenho, encaminhe a figura completa para `darklight-pixelart-animation`. Não simule correção com PPU, resampling, montagem de pernas ou deslocamento arbitrário.

Para o defeito relatado na build de 03/11, priorize caminhada de Punhos/Grevas e visibilidade da esquiva; não reabra todas as folhas aprovadas. Verifique qual atlas a build realmente usa.

Após correção, compare hashes preservados, prévia afetada e consumidor Unity quando executável. Diferencie inspeção estática, revisão visual, EditMode e PlayMode; XML antigo não prova teste novo. Registre test count, falhas, captura e versão. Sem imagem/runtime, declare a lacuna; não certifique aparência por compilação.
