# DARKLIGHT: REQUIEM — Lucius Normal e Arconte · 19/11/2026

## Direção visual

As [duas capturas indicadas pelo usuário](References/LuciusVisual20261119/README.md) regem rosto, cabelo, roupa e a espada longa e larga de Lucius. O Normal conserva a lâmina de aço escuro empunhada com as duas mãos. O Arconte conserva a lâmina grande com energia verde, o sobretudo preto com detalhes dourados finos e a fita azul.

Foram retirados do catálogo os lotes desta rodada que encurtavam a arma ou alteravam a silhueta. Os 12 atlas descartados foram guardados em `../work/lucius-continuidade-20261119/quarentena-identidade/`. A arte já existente voltou a atender dano, derrota e caminhada. Esses estados continuam com as lacunas registradas na revisão anterior.

## Integrado

Doze quadros refeitos de esquiva, seis para cada forma. S e SE têm três quadros de 60, 100 e 70 ms. As demais vistas reaproveitam a locomoção existente. A direção do passo vem do deslocamento comprometido no modelo de combate. A mira e a duração da esquiva de 230 ms mantêm seu comportamento.

| Atlas | SHA-256 |
| --- | --- |
| Normal | `6b755f985e3523a66162dcfa2ec952403aaade1671f421c6cc08823af86f6bf8` |
| Arconte | `524488dcab58704b8cb3328229b728f297303fea91c85aad03c14d20e42dcf07` |

O commit inclui os catálogos e as fontes ativas da rodada anterior, as melhorias de combate que eles usam e as fichas dos quatro próximos personagens. Configuração do Unity-MCP, dependências geradas, caches e candidatos rejeitados ficaram na cópia local.

## Pendências

Conferir os pilotos no próximo passe visual do duelo e completar as seis direções de esquiva que ainda usam caminhada. Revisar novamente os quadros de caminhada armada, dano diagonal e derrota à luz das capturas e da escala da espada. Lucius continua em revisão. Lyra é a primeira prioridade depois da validação dele, na escala e proporção de Fabius Normal. Depois vêm Matriarca Rubra, Kael Voss e Nocthar. Os efeitos dos impactos no jogador seguem pendentes.

## Validação na Unity

- EditMode — 49 aprovados, 0 falhas. Saves preservados e preferências restauradas.
- PlayMode do duelo, portal, transição e pilotos dos inimigos — 8 aprovados, 0 falhas. Saves preservados e preferências restauradas.
- PlayMode isolado de pivôs — 2 aprovados, 0 falhas. Saves preservados e preferências restauradas.
Os recibos estão em `../work/lucius-continuidade-20261119/`. A primeira rodada combinada de PlayMode misturou as fixtures de campanha e de cena isolada, o que falhou no setup dos dois testes de pivô. Estes foram executados em um processo separado. Nenhuma build ou push foi executado.
