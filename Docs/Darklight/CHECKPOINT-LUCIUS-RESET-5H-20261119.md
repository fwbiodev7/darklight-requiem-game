# DARKLIGHT: REQUIEM — retomada na renovação de cinco horas

O ponto de partida é o commit `1550b34` e o [relatório da rodada](RELATORIO-LUCIUS-20261119-FIDELIDADE.md). Foram verificados 49 testes EditMode e 10 PlayMode, incluindo as três fases do duelo.

As [capturas canônicas](References/LuciusVisual20261119/README.md) são a referência de Lucius Normal e Arconte. Preserve rosto, cabelo, proporções, traje e a espada longa e larga. No Normal, mantenha as duas mãos no mesmo punho. No Arconte, preserve o sobretudo com detalhes dourados finos e a energia verde da lâmina.

A esquiva tem S e SE integrados nas duas formas. Nas outras direções, a animação usa a caminhada e segue a direção efetiva do passo. Avance as lacunas restantes em lotes pequenos. Preserve o candidato separado, conforme a instrução mais recente do usuário. Ainda faltam os ciclos de caminhada armada, as reações diagonais de dano e as derrotas. Rejeite figuras que alterem a silhueta ou encurtem a espada. Teste por escopo, em silêncio e com uma instância da Unity. Preserve as alterações locais e os saves. Siga sem build ou push.

A direção de personagens foi atualizada para Lady, Matriarca Rubra e Nero. Lady parte da escala e das proporções de Fabius Normal. Nocthar foi retirado da rota jogável. Consulte o [checkpoint da campanha oficial](CAMPANHA-OFICIAL-20261119.md) para a nova ordem. Os impactos dos inimigos no jogador seguem pendentes.

A janela indicou renovação às 18h34m11s de Brasília em 19/11/2026. A retomada foi agendada para 18h37 no mesmo chat.

## Candidato Esquiva melhorada Lucius

As duas folhas foram aprovadas visualmente pelo usuário para arquivo como candidato em [ArtSources/EsquivaMelhoradaLucius20261119](../../ArtSources/EsquivaMelhoradaLucius20261119). A preparação técnica tem seis quadros Leste/Oeste do Normal e três Leste do Arconte. O Oeste de Arconte permanece na fonte completa para revisar a continuidade da mão da espada. A aura do Arconte fica para depois.

A importação temporária passou quatro testes EditMode. Os atlas preparados foram movidos para ArtSources e os dois catálogos ativos voltaram byte a byte ao commit 1550b34. A retomada recebeu a instrução de preservar o candidato fora do catálogo.

## Folhas de três direções

Quando faltarem três direções da mesma ação e forma de Lucius, gere uma folha **5×3** em uma única chamada. Cada linha contém uma direção e as cinco colunas mostram os quadros em ordem. Registre a ordem das direções no manifesto. Mantenha figuras inteiras, incluindo a espada longa e larga, com margens transparentes entre as células. Confira identidade, pivôs e ritmo no tamanho de jogo antes de importar. Preserve os quadros já aprovados e use folhas menores quando houver menos direções a fazer.

O primeiro teste desse formato está em `ArtSources/EsquivaMelhoradaLucius20261119/Retomada-5x3-Normal-source.png` (SHA-256 `70c9c381ba91036b3ef82ecb653780c618b1fb361f9345b92a815cbc1dc58363`). A linha pedida para SW saiu virada à direita. A folha fica em revisão, fora de Assets. A retomada única foi executada e o agendamento foi encerrado.
