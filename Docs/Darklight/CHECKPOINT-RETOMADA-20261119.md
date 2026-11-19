# DARKLIGHT: REQUIEM — retomada de 19/11/2026

## Lucius 2.0

Revisão estática concluída sobre `dcd3119`, na branch `codex/menus-combat-20261117`. A campanha e a arena de treino agora usam as mesmas três bases da Katana para Lucius, independentemente da arma equipada por Fabius. A seleção considera o alcance real de cada golpe, libera a estocada na distância própria e varia os cortes quando existe alternativa ao alcance.

- O aviso aparece desde o início da preparação e usa a direção comprometida, o alcance e o ângulo de cada golpe. O contato fica limitado a uma tentativa por ataque. Esquivas deixam uma abertura após o erro, e o parry interrompe Lucius imediatamente e abre uma janela para lançá-lo.
- A defesa exige ameaça visível, ao alcance e fora da recuperação, com atraso mínimo de 120 ms. A mesma ameaça tem direito a uma reação durante a carga e a soltura, mesmo se sair do campo de visão e voltar. A esquiva procura espaço dos dois lados. Na guarda, Lucius mantém a direção que mostrou na pose.
- Acertos sucessivos continuam causando dano e acumulando postura. A reação a um acerto comum tem intervalo próprio para que uma sequência de golpes rápidos não congele a IA. Quebra de postura e parry mantêm suas aberturas. O lançamento consome a janela e usa a gravidade, o limite do combo aéreo e a proteção de aterrissagem existentes. Técnicas reservadas a inimigos comuns deixam de afetá-lo como se fosse um Crawler.
- A transformação começa aos 60% de vida e o último confronto aos 25%. Ao voltar da transformação e ao entrar na última fase, Lucius aguarda antes de voltar a esquivar, defender ou atacar. Os três golpes, as poses e a trilha existente permanecem como referências do duelo.

O parser local de C# verificou seis arquivos alterados e retornou zero erros de sintaxe. A compilação na Unity, a avaliação de dificuldade e os testes de gameplay continuam pendentes. Próxima conferência autorizável na build ou na arena de treino: entrar com Katana e Punhos, comparar a estocada à distância, esquivar e aparar cada golpe, lançar após a quebra de postura e atravessar os dois limiares de vida.

## Plano de redesign dos inimigos e demais chefes

A cena atual referencia cinco inimigos comuns e seis chefes com quatro poses base e quatro de ataque por personagem. Os quatro chefes da expansão e a forma final de Nocthar carregam oito quadros por página pelo catálogo. A inspeção cobriu as 16 folhas ativas de combate, incluindo a forma final. O atlas comprova o material disponível para o plano. Leitura durante animação e proporção em arena ainda dependem de avaliação em jogo.

| Ordem | Grupo | Reaproveitamento e proposta | Geração prevista |
| --- | --- | --- | --- |
| 1 | Armored, Sentinela Eclipse e Guardião do Véu | Preservar a escala e as poses pesadas. Piloto proposto no Armored, com um ciclo de guarda, preparação, impacto e recuperação usando os quadros existentes. Usar o resultado para avaliar a leitura do arco e da área nos chefes. | Nenhuma |
| 2 | Shooter, Spectral e Cantor do Abismo | Preservar as silhuetas de conjuração e levitação. Vincular a antecipação ao quadro de carga e manter um tempo de recuperação legível entre disparo único, leque e rajadas do chefe. Reusar os avisos de projétil existentes. | Nenhuma |
| 3 | Crawler e Hunter | Manter as figuras inteiras. Os dois usam garras vermelhas e silhuetas parecidas. Diferenciar a aproximação baixa do Crawler da arrancada do Hunter com duração das poses, preparação e recuperação. | Nenhuma. Se a distinção continuar insuficiente, propor um único quadro de preparação do Hunter. |
| 4 | Matriarca Rubra, Lucien Veyr, Valtherion e Seraphine | Preservar asas, roupas, lâminas e poses de conjuração. Organizar o contraste entre eles pelos passos, o momento de emissão do efeito e a leitura de golpe, projétil ou área. Na expansão, conferir a apresentação de cada golpe contra o padrão ativo da fase. | Nenhuma para o primeiro passe |
| 5 | Kael Voss e Kael Fraturado | Reusar as poses de espada e a rendição, preservando a purificação. Avaliar a continuidade da passada de Voss antes de produzir arte. A auditoria anterior marcou a caminhada dedicada como lacuna. | Se a passada falhar em movimento, propor um piloto de dois quadros completos em uma direção, com custo apresentado antes da expansão. |
| 6 | Nocthar e forma final | Preservar a folha exclusiva da última fase. Conferir a leitura de área, direção e simultaneidade dos perigos usando os avisos e efeitos existentes. | Nenhuma para o primeiro passe |

O primeiro piloto proposto abrange uma classe e um ataque. Pode ser revertido restaurando o uso dos tempos atuais. Novas direções, sequências e lotes artísticos exigem decisão explícita depois da avaliação do piloto. A evidência visual está em `../work/retomada-20261119/inimigos-atlas.png`, fora do repositório.

## Estado da entrega

Implementação de Lucius e plano disponíveis na cópia de trabalho. Arte, mapas, saves e áudio ficaram fora da rodada. O plano encerra o escopo autorizado de retomada registrado em 18/11. A execução do piloto e a build ficam para a próxima decisão. Nenhum teste da Unity, Play Mode, build ou push foi executado. A cota medida após a revisão era de 93% restantes na janela de cinco horas e 70% na semanal. O limiar de 30% da janela ainda estava distante.

## Continuação autorizada — inimigos e demais chefes

O primeiro passe foi implementado nos cinco tipos comuns e nos dez chefes do catálogo, incluindo a folha da forma final de Nocthar. Cada classe reutiliza suas figuras e pivôs importados. Nenhuma folha, direção, animação dedicada ou ataque novo foi produzido.

| Grupo | Alteração implementada | Conferência pendente em jogo |
| --- | --- | --- |
| Armored, Sentinela Eclipse e Guardião do Véu | Armored segura o chão, reage com guarda voltada para a ameaça, desloca-se menos ao receber golpes e tem preparação de 700 ms e recuperação imóvel de 310 ms no Normal. Sentinela e Guardião mostram as trajetórias extras da segunda fase. | Comparar guarda frontal, ataque pelos flancos, quebra de postura, alcance da área e espaço para contra-atacar. |
| Shooter, Spectral e Cantor do Abismo | A preparação usa o quadro de carga. As setas mostram as direções do disparo individual ou do leque real. O recuo das classes à distância usa os quadros de movimento. | Conferir clareza do disparo em campo aberto e perto de paredes, com diferentes quantidades de tiros. |
| Crawler e Hunter | O Crawler prepara o bote por mais tempo. O Hunter faz um avanço mais rápido durante o contato e para para recuperar. O avanço usa a colisão existente. O passo lateral só conta como esquiva se o corpo se deslocar. | Distinguir os dois pela preparação e deslocamento. Verificar a esquiva do Hunter entre paredes e o fim da arrancada. |
| Matriarca Rubra, Lucien Veyr, Valtherion e Seraphine | A Matriarca anuncia a rajada radial da segunda fase. Lucien mostra o leque. Valtherion e Seraphine alinham o impacto corpo a corpo ao quadro em que a lâmina completa o arco, usando a pose anterior no fim da preparação. | Conferir a leitura com os efeitos da arena e o tempo de reação depois de cada transição. |
| Kael Voss e Kael Fraturado | A caminhada alterna quadros pela distância percorrida. Voss anuncia a rajada em leque. O Fraturado alinha o arco da espada ao impacto e mantém a purificação existente. | Avaliar se a passada de Voss precisa de arte dedicada e conferir a chegada à purificação. |
| Nocthar | Corte, disparo e área são definidos no início de cada preparação. O disparo aproveita a pose de mão elevada. O segundo círculo da última fase nasce quando há espaço entre as áreas. Avisos de área terminam com a fase que os criou. | Conferir os três padrões, o intervalo entre fases e a leitura conjunta das áreas quando ambas estão presentes. |

O aviso, o efeito e o dano agora usam o padrão assumido no início do golpe. Combos só continuam com alcance, linha de visão e vaga do tipo de ataque. A caminhada usa o deslocamento do corpo para alternar os quadros. O clarão de dano respeita a opção de flashes reduzidos. Ao morrer durante um golpe, o inimigo normal passa a um quadro de recuperação antes do fade. As duas fontes históricas com pose de derrota de Crawler e Cantor seguem como candidatas para avaliação separada.

A prévia estática está em `../work/retomada-20261119/pilotos-inimigos-quadros-tempos.png`. Ela mostra quadros inteiros, pivôs e durações do código em dificuldade Normal. A análise semântica em memória usou os parâmetros e as 314 referências disponíveis da Unity para os 181 arquivos C# de runtime e retornou zero erros. Nenhum binário foi produzido. A aparência e a jogabilidade na Unity continuam por avaliar.

Última medição da rodada de inimigos: **82% restantes na janela de cinco horas e 69% na semanal**. O trabalho autorizado de reaproveitamento e apresentação está pronto para avaliação. A ampliação dos pilotos em direções ou animações e a execução da Unity, testes, build e push aguardam autorização.

Depois desse registro, o usuário autorizou dois sprites individuais e pediu prioridade para revisar Lucius Normal e Arconte por completo. O resultado atualizado, as lacunas direcionais e a validação protegida estão em [REVISAO-LUCIUS-20261119.md](REVISAO-LUCIUS-20261119.md).
