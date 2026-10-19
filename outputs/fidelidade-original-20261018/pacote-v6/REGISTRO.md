# Revisão atual — 19/10/2026

A lista vigente é [PENDENCIAS-20261019.md](PENDENCIAS-20261019.md). A prévia principal já foi promovida e o servidor local recuperado. A pele das duas skins/quatro formas usa a Requiem original como base; nove fontes brancas receberam cristais físicos.

Katana, adagas e corrente passaram por revisão dos rostos usando as caminhadas e os ataques da Cavaleiro. A correção adicional dos olhos das três transformações usa seus repousos: tentativa de conter saturação e brilho. O usuário ainda identificou olheiras vermelhas indevidas e acabamento inferior ao repouso; ambas as correções foram registradas para amanhã. As fontes anteriores permanecem preservadas, sem montagem de partes ou redução de escala.

Foram concluídos 10 casos EditMode, 7 casos PlayMode e a validação do executável nas quatro formas da Cavaleiro. Build Windows 1.0.13 gerada; ela entrega as correções técnicas com a arte integrada anteriormente. As novas folhas desta prévia continuam externas ao Unity. Consulte QA-RUNTIME.json para escopo, evidências e limites.

O texto abaixo é o registro histórico de 18/10; as pendências e o estado de build de hoje estão no documento de 19/10.

# Registro histórico — 18/10/2026

Esta entrega é uma prévia local de movimento e efeitos, fora do jogo Unity. As fontes continuam candidatas; não são uma aprovação visual nem uma nova build.

## Escala corrigida

A comparação mantém o original sem redução em CSS. O canvas de animação também não encolhe conforme a janela ou a arma; telas menores usam rolagem. A resolução da fonte e a altura física são medidas diferentes.

A altura de referência foi marcada manualmente pelo contorno completo: Requiem, coroa em y = 24 até sola em y = 290 (266 px); Rock, y = 334 até y = 612 (278 px). A marca anterior da Rock, y = 604, vinha de um limiar de pixels claros e excluía o contorno escuro inferior. Cada folha recebe uma escala uniforme pela anatomia da pose de repouso, excluindo arma/efeitos. Não há ajuste automático para caber, alteração de escala por quadro nem montagem de partes.

Foram retiradas da seleção as folhas de 64 figuras e as tentativas de caminhada com 16/20/24 figuras que perderam escala, proporções, orientação ou botas. Ampliá-las não resolveria a anatomia. A caminhada frontal seleciona oito figuras grandes por skin/forma; o repouso usa a pose neutra 3.

## Combos e efeitos

- Katana: rasante, ascendente e finalizador; 24 poses, 3 impactos, 862 ms de movimento.
- Adagas: direita, esquerda, corte em X e execução dupla; 24 poses, 4 impactos, 729 ms.
- Corrente/foice: varredura, gancho e colheita; 3 impactos. Requiem usa 20 poses após excluir 1/15, sem arma, e 17/18, unidas; Rock usa 24 poses.
- Cavaleiro do Abismo, somente Rock: 3 impactos de serras e sequência separada de montagem, arrancada, investida e freada. Onze poses por sequência nas formas normal/vermelha/roxa; branca usa 10 nas serras e 11 na moto, pois 8/9 estavam unidas. Quadro 20 excluído em todas por cabelo inconsistente; o quadro 9 também tinha uma serra ausente.
- Vermelha usa rubi; roxa usa violeta; branca usa branco/prateado, sem vermelho nos efeitos. As armas e formas são fontes completas próprias.
- Arcos, núcleos claros, faíscas, anéis e linhas de velocidade têm desenho diferente por arma. Impactos usam duração curta e recuo horizontal de até 3 px; não há zoom nem deformação/escala do corpo.
- Efeitos das formas coloridas usam marcos das armas da pose normal correspondente, registrados proporcionalmente. Isso evita confundir pontas do cabelo/cristais com lâminas. Em adagas, pose 7, a segunda lâmina fica oculta; o ponto falso do olho foi removido nas duas skins e quatro formas, sem inventar uma arma.
- Transformação de 900 ms entre guardas inteiras, com aro na cor da forma. Repouso preserva a arma e a forma, preferindo sua guarda; não troca adagas/corrente por katana.
- Reprodução silenciosa.

O catálogo atual tem 36 fontes PNG e 36 máscaras técnicas de identificação de figuras inteiras, 731 componentes fonte e 711 poses únicas selecionadas. São 44 páginas: 8 caminhadas, 8 repousos e 28 combos. A soma de seleções por página é 719 porque o repouso reutiliza uma figura da caminhada. Componentes unidos permanecem preservados na fonte, mas não são exibidos.

## Verificação realizada

Geometria, transparência, contagem real, limites da prévia, hashes das fontes e escala uniforme passaram. O script de conferência validou timelines, 28 seleções de repouso, 8 variantes de Cavaleiro, eventos, pausa, passo de quadro, laço e transformação, com limites descritos em QA-CONTROLES.json.

No navegador local, as 16 combinações de arma/forma carregaram as 28 páginas de combo sem erros/avisos de console. A comparação mostrou a altura original lado a lado. Efeitos violeta presos ao cabelo foram identificados e corrigidos antes da promoção da página. Verificações adicionais de moto/branca e transformação são registradas em QA-NAVEGADOR.json após sua execução.

Essas verificações não comprovam fidelidade artística, fluidez percebida, aprovação do usuário ou funcionamento em combate Unity.

## Pendências reais

- Caminhadas nas outras sete direções: as novas tentativas SW/SE repetiram a mesma perna; foram rejeitadas e não entram como ciclo concluído. As direções sem ciclo real nas duas skins ficam desabilitadas, com aviso.
- Cabeças ainda apresentam diferenças de largura e Rock tem variação na relação torso/pernas em algumas candidatas. A escala uniforme corrige altura em tela, não anatomia desenhada.
- Revisar cristais salientes nas fontes brancas; a leitura atual se aproxima de tachas, sobretudo Rock. Preservar o cabelo branco aprovado até a parte superior das costas; não encurtá-lo novamente.
- Ataques nas outras direções, continuidade completa entre caminhar/parar/atacar com cada arma e transformação, conferência visual da pele e cabelo em todos os quadros.
- Integrar somente fontes realmente aprovadas/coerentes no Unity e testar combate, efeitos, colisões e desempenho; depois gerar e validar nova build.
- Pendências gerais anteriores do jogo (morte/checkpoint/Game Over, áudio restante, pontes do mapa 5 e teste de combate) devem ser reavaliadas pelo estado atual do projeto; os testes técnicos da 1.0.12 não aprovam esta nova revisão visual.

Os oito arquivos protegidos de morte/reversão e os saves não foram alterados por esta revisão. Fontes originais preservadas. Não houve nova build nem publicação do site nesta etapa.
