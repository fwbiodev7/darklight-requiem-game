# DARKLIGHT: REQUIEM — teste do duelo de Lucius 2.0

Retomada autorizada em 19/11/2026 sobre `dcd3119`, branch `codex/menus-combat-20261117`. Revisão visual e cobertura de arte registradas em [REVISAO-LUCIUS-20261119.md](REVISAO-LUCIUS-20261119.md).

## Duelo em PlayMode

A primeira execução protegida passou três testes. O duelo completo usou a IA e o relógio reais da arena com um jogador comandado pela fixture. O jogador aplicou dano controlado para atravessar os limiares e executou parry e esquiva no momento do aviso. A sequência aérea verificou diretamente o receptor de lançamento de Lucius, a suspensão máxima de dez acertos, o arremate descendente e a proteção de aterrissagem. Durante o voo, Lucius não iniciou outro ataque.

- **Normal:** Lucius iniciou o corte horizontal e o parry abriu a janela de lançamento sem dano ao jogador.
- **Arconte:** a mudança aos 60% de vida bloqueou dano durante a transformação. O corte vertical confirmado após a transformação atingiu Fabius.
- **Último confronto:** a mudança após 25% de vida abriu um intervalo antes do primeiro ataque. O jogador esquivou do corte horizontal e Lucius escolheu o vertical na sequência. A derrota encerrou o movimento e os avisos.
- **Leitura da IA:** ignorou carga apontada para longe e ameaça atrás de uma parede. Em linha de visão, respondeu após o atraso mínimo.
- **Portal:** as duas posturas narrativas foram verificadas até salvar e reler o desfecho em arquivos isolados. `SaveCheckpoint` permite escrita em modo silencioso apenas quando existe um caminho de QA isolado. Os saves reais e as preferências foram preservados.

- **Estocada à distância:** no alcance de 1,98 unidade, a IA escolheu a estocada de 2,10 em vez dos cortes de até 1,85. A parede impediu a decisão de atacar. Sem parede, o aviso indicou o cone de 35° para cada lado e a mira permaneceu fixa quando Fabius saiu da linha. O erro abriu uma janela de punição. Ao repetir a estocada contra o alvo parado, o dano foi confirmado.

Nesta rodada foram **4 testes PlayMode aprovados, 0 falhas**. Resultados estruturados em `../work/retomada-20261119/lucius/091312-results.xml` e `../work/retomada-20261119/lucius/091521-results.xml`. Ambas as execuções registraram saves reais preservados e preferências restauradas. O jogador foi comandado pela fixture e a avaliação manual de dificuldade segue pendente.

## Retomada

A cota real indicou renovação da janela às 13h12m56 de Brasília. A automação existente foi atualizada para executar uma vez neste chat em **19/11/2026 às 13h15**. O próximo lote deve ler este checkpoint, avaliar as pendências direcionais de Lucius em pequenos grupos e preservar o material aprovado. A sequência de efeitos de impacto no Fabius e a validação da guarda do Armored ainda aguardam a prioridade registrada para a arte de Lucius.

A cota no fechamento era de 21% da janela de cinco horas e 59% da semanal. Nenhuma build de distribuição foi gerada.

Após este checkpoint, a continuação a pedido do usuário integrou as guardas do Normal e completou as vistas dos três golpes de espada de ambas as formas. Na retomada das 13h15, use a cobertura atualizada no fim de [REVISAO-LUCIUS-20261119.md](REVISAO-LUCIUS-20261119.md).
