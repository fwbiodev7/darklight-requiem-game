# Lucius Walk — revisão incremental, 10/11/2026

**Estado atual supera o incremento histórico abaixo:** usuário rejeitou marcha/high-knees. Walk S antigo foi retirado do gate e seus PNG/JSON/fragmento foram removidos de Assets por coordenação explícita. Nenhuma página Walk pronta neste instante. W/E/SW/NW metade B corretiva V2 foi produzida e preservada, mas W/E passagem B elevam excessivamente os joelhos; não integrada. Substituição S/SW V3 em geração: caminhada baixa, curta e relaxada, pés próximos do chão, heel-to-toe, braços discretos, boca fechada. Durante importação Horizontal V3, Assets congelados pelo coordenador; somente fontes/review/prévias podem avançar.

Contrato consultado: AGENTS.md, ESCOPO-CANONICO-20261026.md, SKILLS-CONTEXTO-20261104.md, CHECKPOINT-LUCIUS-ARTE-20261110.md. Consumidores consultados: DarklightLuciusSpriteBank.cs e prepare_lucius_idle_20261109.py. Ferramenta: imagegen integrado, cinco chamadas, sem fallback/API externa.

Autoridade: folha inteira `ArtSources/LuciusNormal20261109/selected/LuciusNormalIdle8Final-source.png`, SHA256 `68947a7143047ed76221ef7038726fcc9fd3e9b8403727e697f6aee2e7cfb761`. Candidatos OpenBot indicados no checkpoint não existem neste computador; nenhum foi reutilizado.

## Incremento preparado

Somente **S, quatro quadros**: contato A → passagem A → contato B → passagem B, 140/130/140/130 ms, loop 540 ms. Revisão do contato em piso fixo confirmou a troca de apoio frontal, e o casaco/espada permanecem juntos em cada figura. Pivô horizontal segue o eixo da pelve; vertical usa a linha real das solas, sem centro de bounding box da espada/casaco. PPU preservado: 214.97090106361628. Alturas nativas 421/421/425/425 px, dentro do envelope próximo da referência (412–431 px), sem resize.

`selected/LuciusWalkS20261110V1-source.png` organiza quatro **células inteiras** 384×512: colunas 0/1 vêm da fonte S_SW-v1 e colunas 2/3 da correção S_SW-v2. Fonte original/correção preservadas; nenhuma montagem anatômica, espelhamento, rotação ou resampling. Os pixels RGBA de cada célula selecionada são idênticos à sua origem.

Limites da revisão: contato estático registrado e GIF gerado; a reprodução animada e o comportamento na Unity cabem ao coordenador. Há fundo residual gerado de alpha muito baixo (principalmente 1–8) nos quadros 2/3, perceptível como retângulo tênue no contato escuro; foi preservado para evitar limpeza automática destrutiva de bordas. Conferir no consumidor. A cadência da passagem A é mais baixa que a passagem B; o ciclo não é uma certificação de polimento final.

Assets preparados: `LuciusWalkS20261110V1.png`, JSON de slicing e fragmento `LuciusWalkPages20261110.json` no Resources atribuído. O fragmento não substitui `catalog.json`: contém somente uma página S/Walk e precisa ser anexado pelo coordenador ao catálogo existente.

## Pendências exatas

| Fonte | Revisão | Correção restante |
|---|---|---|
| S_SW-v1 | Candidato não liberado integralmente | S contato B repetia a perna A; SW alternância não demonstrada. Preservadas figuras S0/1. |
| S_SW-v2 | Correção parcial | S alterna pernas; SW contato B ainda mantém silhueta/oclusão da metade A. Corrigir figuras SW2/3 completas e resíduos de fundo dos S2/3 quando houver geração disponível. |
| W_E-v1 | Rejeitado para integração | Contatos A/B e passagens A/B repetem o padrão da mesma perna. Redesenhar figuras completas W2/3 e E2/3 com oclusão oposta e oposição dos braços. |
| NW_N-v1 | Rejeitado para integração | NW mantém a mesma perna na metade B; N alterna contatos, mas passagem B levanta o mesmo pé da passagem A. Corrigir NW2/3 e N3 completos; revalidar escala e bob vertical. |
| NE_SE-v1 | Rejeitado para integração | Linha solicitada NE veio voltada NW; SE repete contatos/passagens da metade A. Redesenhar quatro NE completos para direção correta e SE2/3 completos com pernas opostas. |

Produzido: cinco folhas de oito figuras (40 células geradas), incluindo uma correção. Revisado: cinco fontes e contato S selecionado. Preparado em Assets: uma direção/quatro quadros. Catálogo compartilhado: não editado. Unity/importação/runtime/Git: não executados por este agente. Sete direções permanecem pendentes; esta entrega não resolve sozinha o deslizar em toda a arena. Novas chamadas encerradas pela reserva informada pelo coordenador.

O script `Tools/AnimationPreview/prepare_lucius_walk_20261110.py` valida hashes, limites dos retângulos, existência de alpha, duplicação exata RGBA, durações, nomes e gate visual; gera GIF e contato. Essas guardas não aprovam anatomia automaticamente. `hashes.json`, `review.json` e `package-report.json` são as evidências estruturadas.
