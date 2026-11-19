# DARKLIGHT: REQUIEM — revisão de Lucius Normal e Arconte

Revisão feita no checkout `codex/menus-combat-20261117` a partir de `dcd3119`. O catálogo inicial tinha 70 páginas no Normal e 72 no Arconte. Cada página foi cruzada com o PNG e o importador. A revisão visual comparou as fases de cada ação com os pivôs. Os dois catálogos corrigidos têm 73 e 72 páginas, respectivamente.

## O que foi corrigido

| Forma | Correção | Material |
| --- | --- | --- |
| Normal | A caminhada agora cobre NW e NE com contatos e passagens alternados. | 8 quadros novos, 4 por direção, a 568 ms por ciclo. |
| Normal | Estocadas S, W e N ganham antecipação, extensão e recuperação próprias. A sequência canônica reaproveitada no SE agora mostra a lâmina estendida durante o contato. | 9 quadros novos e seleção de 3 quadros da fonte existente para SE. |
| Normal | Reação de dano S, W, N e E deixa de reproduzir o saque da espada. A pose de saque recuperada permanece registrada separadamente no catálogo. | 12 quadros novos em quatro vistas. A reação percorre o clipe durante o tempo real de atordoamento. |
| Normal | A guarda usa a pose armada da direção quando falta animação compatível. As vistas N e E ainda usam as sequências existentes. | Sprites reaproveitados. Seis direções usam guarda estática. |
| Arconte | Descartado o enquadramento incorreto que mostrava mais de uma figura no mesmo quadro. A estocada leste e o corte horizontal frontal usam recortes de figuras inteiras. | Seis quadros usados no catálogo, recuperados de dois PNGs canônicos copiados byte a byte. |
| Arconte | Estocada própria para NW, N e NE. A vista N encurta a lâmina em perspectiva, por isso é importante conferir a leitura contra o aviso em combate. | 9 quadros novos. |
| Ambas | A guarda começa pelo relógio da própria reação. O dano encaixa seus quadros no tempo real do estado. A entrada de Arconte respeita a transição oficial e o trecho alternativo usa a sequência de transformação até o fim. | Ajustes no consumo de clipes e relógios existentes. |

Foram aceitos **38 quadros novos** de Lucius. Os PNGs de origem, decisões, pivôs, dimensões, PPU e hashes estão nos manifestos e metadados dos próprios sprites. Os retângulos e pivôs existentes de Fabius ficaram preservados. A revisão local encontra as fontes de arma aprovadas e as prévias em `../work/retomada-20261119/lucius/`.

## Cobertura ainda parcial

- **Normal:** 14 entradas usam uma vista próxima, incluindo quatro estocadas, quatro reações de dano diagonais e seis quedas. Seis guardas são poses estáticas da direção. `WalkArmed` tem poses nas oito direções, mas ainda carece de ciclos armados. A esquiva usa a caminhada na direção do deslocamento. O saque recuperado cobre N e permanece disponível no catálogo como sequência opcional.
- **Arconte:** 12 entradas de ataques usam vistas próximas, sendo cinco cortes horizontais, quatro verticais e três estocadas. Caminhada, repouso, guarda, dano e queda têm páginas nas oito direções. A esquiva continua com locomoção reaproveitada. A transformação oficial usa a apresentação dirigida da cena.
- `directionalFallback` e `sourceDirection` documentam as vistas reaproveitadas. O indicador visual do consumidor permite identificá-las durante a revisão. Ambos os catálogos continuam com `complete=false`.

## Validação

EditMode protegido na Unity 6000.6.0f1: **36 aprovados, zero falhas**, com leitura de cada quadro dos dois catálogos, filtro Point, confirmação das direções reaproveitadas, temporização dos ataques e testes de encontros. Saves preservados e preferências restauradas. Recibo `../work/retomada-20261119/lucius/090439-results.xml.summary.json`.

PlayMode com gráficos habilitados: **3 aprovados, zero falhas** em estabilidade da caminhada e dos golpes e no cancelamento da entrada de Arconte. Saves preservados e preferências restauradas. Recibo `../work/retomada-20261119/lucius/090659-results.xml.summary.json`. A primeira execução com `-nographics` falhou na criação de RenderTexture. A tentativa também misturou a fixture de campanha com as de cena isolada. As verificações foram refeitas em processos separados.

O teste do duelo no portal chegou ao desfecho em memória para a primeira postura, mas falhou ao ler o arquivo na linha 80. `SaveCheckpoint` ignora gravações em modo silencioso, inclusive no caminho isolado usado pela fixture, portanto `DarklightProgress.Load()` retornou `null`. A persistência do desfecho e a segunda postura seguem sem comprovação nesse teste. Os saves reais e as preferências foram preservados. Recibo `../work/retomada-20261119/lucius/090741-results.xml.summary.json`.

A continuação corrigiu a gravação no caminho isolado da fixture. O teste passou para as duas posturas. O resultado atualizado do portal e do duelo nas três fases está em [CHECKPOINT-DUELO-LUCIUS-20261119.md](CHECKPOINT-DUELO-LUCIUS-20261119.md).

Pausa no limiar combinado de **30% restantes na janela de cinco horas**, com **60% na semanal**. A cobertura original e os novos quadros estão disponíveis na cópia local. Nenhuma build de distribuição foi gerada.

## Trabalho anterior dos inimigos

Hunter e Armored receberam um sprite novo cada, integrado à preparação da arrancada e à guarda, respectivamente. A execução em arena confirmou e capturou a preparação do Hunter e o retorno ao quadro de impacto existente. A etapa de Armored do mesmo teste falhou com o jogador mirando para sul em vez de para o alvo. A checagem de combate através de paredes passou. A revisão de Lucius assumiu a prioridade antes da repetição do caso Armored. O pedido de efeitos de impacto no Fabius permanece pendente.

## Rodada adicional após o teste do duelo

- **Normal:** as seis guardas que usavam pose estática foram substituídas por 18 quadros novos. Oito direções têm agora sequência de guarda. A execução em arena confirmou que Lucius escolhe guardar quando ambos os lados da esquiva estão bloqueados e mostra os três quadros sem mover os pés. PlayMode com 1 aprovado e zero falhas em `../work/retomada-20261119/lucius/092432-results.xml`.
- **Arconte:** 9 quadros de corte horizontal em W, N e E e 9 de estocada em S, W e SE foram integrados. A estocada passa a cobrir as oito direções sem reutilizar vistas, e o corte horizontal passa a seis. O corte vertical mantém quatro vistas próprias e quatro reaproveitadas. Os manifestos ao lado dos atlas registram fontes, hashes, retângulos e pivôs.

O fechamento acrescentou mais 8 quadros do corte horizontal do Arconte (SW e NE), 12 do corte vertical do Arconte (SW, NW, NE e SE) e 12 da estocada do Normal (SW, NW, NE e E). Ao todo foram **68 novos quadros integrados nesta rodada adicional**. Os três golpes de espada têm agora **oito sequências direcionais próprias em cada forma**. A guarda do Normal também tem oito sequências animadas. As vistas reaproveitadas que ainda restam no Normal são quatro reações diagonais de dano e seis vistas de derrota. A esquiva ainda reaproveita a caminhada e o ciclo armado do Normal permanece pendente. Os catálogos seguem com `complete=false`.

O teste de arena que verificou a guarda foi concluído antes dos lotes finais de espada. A importação dos catálogos finais passou em **4 testes EditMode, zero falhas**, incluindo as três técnicas com oito direções distintas por forma e o carregamento de cada quadro declarado. Saves preservados e preferências restauradas. Recibo `../work/retomada-20261119/lucius/093425-results.xml`. Nenhuma build de distribuição foi gerada. A retomada das **13h15 de Brasília** deve seguir pela cobertura que ainda falta no parágrafo anterior.
