---
name: pixel-art-sprite-sheet-master
description: Crie, edite, reconstrua, anime e valide spritesheets de pixel art 2D para Unity, preservando a identidade das referências e os sprites aprovados. Use para personagens, NPCs, inimigos, chefes, armas e objetos animados; inclui prévias, metadados e auditoria técnica. Não substitui workflows de cenários ou tilesets.
---

# Pixel Art Sprite Sheet Master

Entregue spritesheets utilizáveis, com fontes preservadas, PNG, JSON, prévias estáticas e animadas e relatório técnico. Responda no idioma do usuário. Validação técnica não certifica anatomia, identidade ou aprovação artística.

## Contrato antes da arte

Leia as instruções locais e o consumidor Unity: importador, catálogo, nomes, GUIDs, direções, timings, PPU, pivôs e eventos. Inspecione visualmente o repouso aprovado e os estados pertinentes. Reutilize sprites aprovados antes de gerar substitutos. Registre referência e SHA-256, resolução **nativa** e resolução da fonte separadamente, identidade, câmera, proporções, roupa, arma, mão dominante, idade, paleta, luz, ações, direções e skins. Não adote automaticamente a perspectiva de outro jogo.

Cabeça, corpo e arma permanecem juntos em cada quadro inteiro. Nunca recorte membros, transplante rostos nem monte personagens por partes, inclusive no preparo de referências. Identificar e organizar quadros inteiros é permitido. Para corrigir desenho, prefira regenerar o quadro/folha completo com referências. Preserve a arte original e trabalhe em candidatos versionados.

## Escolha o workflow necessário

- Referências, paletas e identidade: [workflows/identity.md](workflows/identity.md).
- Idle, caminhada, corrida, ataques, combos, esquiva, defesa, dano e morte: [workflows/animation.md](workflows/animation.md).
- Reconstrução e correções sem montagem anatômica: [workflows/repair.md](workflows/repair.md).
- Aseprite, LibreSprite e Unity: [workflows/tools-unity.md](workflows/tools-unity.md).
- Contrato de arquivos e scripts: [references/metadata.md](references/metadata.md).
- Critérios e relatório obrigatório: [references/quality.md](references/quality.md).

Carregue somente os documentos necessários à tarefa. Use scripts existentes do projeto antes dos helpers desta skill. Dependência dos helpers: Python 3.10+ e Pillow; não instalar editores silenciosamente.

## Execução e entrega

1. Faça inventário e selecione a autoridade visual. Gere `palette.json` com `sprite_pipeline.py palette`. A frequência das cores não identifica sozinha pele, olhos ou roupa; anote os materiais após inspeção.
2. Para raster novo/edição visual, use a ferramenta integrada de imagens disponível e suas instruções. Inclua referências **inspecionadas**, quadros completos, poses realmente distintas e transparência real; não substitua arte por formas geométricas ou prometa grade exata porque está no prompt. Se indisponível, conclua as análises e informe o bloqueio artístico sem fingir geração.
   Para lotes da mesma identidade, prefira folhas de **8–12 sprites por geração**, preservando a qualidade do sprite isolado e espaço entre figuras. Essa é a preferência explícita atual de DARKLIGHT; não gastar uma chamada por sprite nesses lotes. Registre retângulos reais e confira cada quadro. Correções de poucos defeitos preservam os demais sprites.
3. Extraia somente quadros inteiros, sem redimensionamento anatômico. Não use rotações planas, espelhamento de assimetrias ou deformação de imagem como nova direção/ação. Ajustes de atlas e pivô usam coordenadas inteiras, sem alterar os pixels do desenho. Mantenha fontes, hashes e vínculo fonte→quadro→clip.
4. Execute `audit`, gere `preview` e examine o contato em 1× e ampliação nearest-neighbor e cada clip em movimento. Corrija o defeito específico e confira os estados vizinhos; não repita toda a suíte sem motivo.
5. Integre quando solicitado, reutilizando o sistema existente. Preserve GUIDs e eventos de gameplay. Teste importação, transições e contato nas ações afetadas. Não iniciar Play, música ou testes sonoros onde houver suspensão local de áudio.
6. Entregue PNG RGBA, JSON, paleta, contato, prévia animada e relatório com falhas e limites. Separe `candidate`, `visual-reviewed`, `imported` e `runtime-verified`; um não implica o outro. Não solicitar nova aprovação se o usuário já autorizou a ação; prévia obrigatória não é um checkpoint de permissão.

## Pixel art e economia de contexto

Mantenha pixels quadrados, clusters intencionais, rampa de sombras e direção da luz coerentes. Dithering só quando melhora a textura na resolução nativa; não adicionar ruído a rostos. Antialiasing seletivo **desenhado com cores da paleta**, nunca blur ou suavização global. Alpha parcial e halos são sinais para inspeção, não motivo para apagar automaticamente cores ou bordas. Uma fonte grande não comprova pixel art nativa.

Salve logs completos em arquivo; mostre contagens, caminhos, exit codes e falhas relevantes. Nunca resuma a ponto de ocultar testes não executados, erros ou perda de fidelidade. Não prometa economia percentual ou integração com modelos locais sem medição real.
