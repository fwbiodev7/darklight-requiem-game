# Relatório de testes finais — 15/11/2026

## Escopo e proteção dos dados

Rodada autorizada pelo pedido de testar e produzir relatório. Testes direcionados no checkout atual da Unity; nenhuma build nova, geração de arte, alteração de segurança ou reprodução de música. Nenhuma alteração de runtime foi realizada nesta rodada. Fabius Normal e seus pivôs foram preservados.

As verificações de gameplay usam o isolamento de armazenamento e as verificações de preferências/saves da fixture existente. A nova fixture de colisão instancia somente geometria e corpos físicos, sem campanha ou saves, e restaura as opções globais de física/áudio ao terminar.

## Resultados reais

| Grupo | Aprovados | Falhas |
| --- | ---: | ---: |
| Formas de Fabius — EditMode | 28 | 0 |
| Estabilidade das animações de Fabius — EditMode | 3 | 0 |
| Combate — EditMode | 27 | 0 |
| Orientação de movimento — EditMode | 1 | 0 |
| Deslizamento nas paredes — EditMode | 3 | 0 |
| Estabilidade da raiz visual — PlayMode | 3 | 0 |
| Saída de sobreposição inicial com parede — PlayMode | 1 | 0 |
| Percurso automático de objetivos com cápsula dos pés — PlayMode | 0 | 1 |
| Gothic Eclipse — PlayMode, primeira execução | 3 | 1 |

**EditMode: 62/62 aprovados.** O catálogo e os mapeamentos cobertos pelos 28 testes de formas passaram. Isso não substitui a avaliação visual de cada frame.

**Resultado final por teste único: 70 aprovados e 1 falha pendente, entre 71 testes distintos.** O teste de iluminação passou na repetição após criar a pasta de capturas. Houve 72 execuções no total: 70 aprovações e duas falhas iniciais, sendo uma resolvida. Não somar a repetição como um novo teste distinto.

## Falha de percurso: evidência e limites

A nova verificação `DarklightDeliveryCollisionTests.AllTenMapsHaveTraversableObjectiveRoutesWithTheActualFeetCapsule` foi planejada para percorrer dez regiões, mas interrompeu na primeira. **As demais nove não foram validadas por esse teste.**

Mensagem real:

```text
Blocked: chapter 0, target (-24.73, 2.98), position (-2.98, -3.67), waypoint (-2.98, -3.64)
Expected: less than 0.0149999997f
But was: 0.0217995644f
```

A cápsula da fixture usa tamanho `(0.42, 0.28)` e offset `(0, 0.14)`, conforme o construtor existente do jogador. O planejamento de rotas consulta um círculo de raio `0.3` centrado na posição do corpo, enquanto `SafeStep` consulta o colisor real. Essa diferença é uma hipótese para o desvio de aproximadamente 0,022 unidade em relação ao waypoint, não uma causa definitivamente demonstrada.

O teste exige alcançar cada waypoint com tolerância de 0,015 unidade; o jogo permite controle livre. Portanto, a falha demonstra que esse percurso automatizado não foi concluído, mas não prova, isoladamente, que o jogador fique permanentemente preso. Preservada para investigar a compatibilidade entre planejamento, colisor e movimento, sem relaxar a tolerância para obter aprovação artificial.

## Falha de captura Gothic Eclipse

O teste `GothicEclipse20261114ActorEdgeLightFollowsLocalSourceAndRestores` encontrou `DirectoryNotFoundException` ao salvar `outputs/gothic-eclipse-20261114/gameplay/actor-light-left.png`. A pasta foi criada e somente esse teste foi repetido: **1/1 aprovado, exit code 0**, sem alteração do código de produção ou das asserções. Gothic Eclipse terminou com quatro testes distintos aprovados.

As outras três verificações Gothic Eclipse passaram: camadas/limites/reversibilidade da região zero e arena, distinção entre dia/interiores e persistência explícita da intensidade. Isso não equivale a uma auditoria visual completa das névoas em todos os mapas.

## Evidências locais

- EditMode: `work/session20261114/EditMode-2026-11-15T212306-569118+0000/tests.xml` e `receipt.json`.
- Física/estabilidade PlayMode: `work/session20261114/PlayMode-2026-11-15T212653-042643+0000/tests.xml` e `receipt.json`.
- Gothic Eclipse, primeira execução: `work/session20261114/PlayMode-2026-11-15T212906-049000+0000/tests.xml` e `receipt.json`.
- Gothic Eclipse, repetição isolada aprovada: `work/session20261114/PlayMode-2026-11-15T213139-015111+0000/tests.xml` e `receipt.json`.
- Novo teste reproduzível: `Assets/Darklight/Tests/PlayMode/DarklightDeliveryCollisionTests.cs`.

Os resultados pertencem ao checkout atual, que contém trabalho local; não certificam automaticamente um executável previamente empacotado ou exatamente o commit publicado.

## Entrega e continuidade

A build de referência continua em `outputs/build-windows-gothic-20261114-feedback/Windows/`, já testada manualmente pelo usuário. Não foi recompilada ou executada nesta rodada. O fechamento anterior publicou `413f637` na branch `codex/finalizacao-animacoes-colisoes-20261114`; esta rodada acrescenta diagnóstico e documentação locais.

Prioridades de produto permanecem: HUD/diálogos → dificuldade de Lucius → design de inimigos/bosses → gráficos gerais → cutscenes do GDD. Orientação nos mapas e músicas já fornecidas para bossfights permanecem registradas no documento de direção de 14/11.

Consulta de uso durante o encerramento: **96% semanal usado, aproximadamente 4% restante**. Este é um retrato da consulta, não garantia do saldo após as últimas operações. Encerrar sem iniciar novo lote de produção.
