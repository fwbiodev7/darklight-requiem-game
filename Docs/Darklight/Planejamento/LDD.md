# DARKLIGHT: REQUIEM — LDD vigente em 26/10/2026

Autoridade de personagens, formas, equipamentos e ordem de produção: [escopo canônico de 26/10](../ESCOPO-CANONICO-20261026.md). Este documento consolida requisitos de níveis sem declarar uma campanha manualmente validada.

## Continuidade dos níveis

Preservar os dez mapas/capítulos atualmente enumerados por `DarklightStory.Chapters` em `Assets/Darklight/Runtime/Campaign/DarklightProgress.cs`, os seis capítulos originais e suas ampliações: A Noite da Ruptura, Vozes do Abismo, Sangue Desperto, O Projeto Eclipse, Além do Véu, O Último Eclipse, Herança Carmesim, A Cidade sem Amanhecer, O Trono do Vazio e Darklight: Requiem. Os títulos narrativos não tornam Eclipse/Absoluto transformações separadas novamente.

Preservar mapas, colisões, pontes, câmera, NPCs, encontros, chefes, cutscenes, checkpoints, saídas, segredos, flags e recompensas existentes. Não reconstruir níveis durante a redução de skins/arsenal. A mudança de escopo deve permitir carregar progresso anterior sem duplicar recompensas ou corromper o save.

## Contrato de progressão após a fusão

Fabius usa Réquiem em todos os mapas, com katana, punhos e moto conforme integração concluída. Desbloqueios antigos de Eclipse/Absoluto alimentam a progressão de Eclipse Absoluto; IDs persistidos devem ser migrados de forma compatível, sem apagar saves. Não exigir a skin Rock para atravessar uma rota ou acessar uma técnica de moto.

Manter as seis técnicas expandidas de moto: Serra Infernal, Giro Devastador, Esmagamento Vertical, Roda Destruidora, Cavalgada Brutal e Execução Aérea Motorizada. A mecânica da moto não autoriza alterar escala/colliders para acomodar uma folha visual, atravessar paredes ou contornar pré-requisitos de progressão.

O Encapuzado só pode ser identificado pelo nome público durante a campanha. A identidade de Lucius e seu parentesco não devem aparecer em diálogos, HUD, seletores ou recompensas anteriores a Sangue do Abismo. A expansão e a implementação visual do personagem aguardam o redesign de Fabius.

## Verificação necessária para a build

| Fluxo | Evidência exigida | Limite |
|---|---|---|
| Iniciar/continuar | Menu, novo jogo e carregamento de save isolado | Abertura do executável não comprova a partida |
| Exploração | Movimento, colisões, câmera, pontes, interação e saídas acessíveis | Não extrapolar um trecho para todos os mapas |
| Combate | Katana/combos, punhos, seis técnicas de moto, aéreo, esquiva/defesa/parry, inimigos e chefes | Simulação automatizada não equivale a campanha humana |
| Transições | Ida/volta e mudanças de forma/arma, sem perder flags e recompensas | Registrar mapas e rotas realmente exercitados |
| Persistência | Salvar/recarregar, morte/respawn e preferências isoladas/restauradas | Preservar os saves originais |
| Apresentação | HUD, controles, escala 1.325, áudio contextual, ausência de erros bloqueantes | Teste virtual não certifica controle físico ou avaliação auditiva |

Guardar resultados por execução, com data, caminho de log e estado aprovado/falho/não executado. O checkpoint de 25/10 é histórico; sua falha PC04 continua aberta até um reteste específico comprovado.
