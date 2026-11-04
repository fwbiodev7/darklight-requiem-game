# Proposta de reparo localizado das armas

Estado: aprovado diretamente pelo usuário em 12/10: “faz isso entao nao tem outro jeito mesmo né”. Piloto do primeiro golpe Réquiem maduro concluído e importado: 32 quadros/oito direções, com montagem localizada das armas. Próximas correções continuam limitadas às divergências reais, sem refação geral.

## Problema confirmado

As armas estão desenhadas junto com os personagens nas folhas atuais. Reutilizar apenas um novo desenho sobre elas deixaria duas lâminas visíveis. A katana sofreu deriva: superfície vermelha demais, comprimento e guarda diferentes da referência original.

## Solução proposta

1. Preservar corpos, poses, roupas, direções, animações, código e recortes existentes.
2. Usar `WeaponsCanonicalUser.png` como molde visual permanente das três armas, incluindo em toda nova geração. A katana tem superfície escura, fio vermelho fino, lâmina longa e cabo/guarda originais.
3. Começar por um piloto limitado: o golpe leve 1 da Réquiem madura, nas oito direções. Editar somente recortes da arma e da pega necessários para manter contato com as mãos. Agrupar esses recortes numa folha de reparos para edição visual, em vez de gerar novamente personagens inteiros. Preservar os originais e registrar cada área substituída.
4. Reintegrar os reparos na mesma folha por montagem técnica, conferir alinhamento, oclusão, ponta completa e tamanho na escala do jogo. Somente então aplicar o mesmo procedimento às poses em que a divergência prejudica a leitura. Não iniciar uma refação geral de todos os pacotes.
5. Adagas e corrente ficam sujeitas a inspeção da silhueta original, com correção apenas de divergências reais. Novas artes já usam a referência canônica desde a primeira geração.

Não prometer economia medida de tokens ou de custo. O ganho pretendido é reduzir o escopo artístico: retocar regiões específicas e reutilizar todo o trabalho de corpo/animação. Uma arma separada por camada pode ser avaliada depois, mas não é uma solução imediata para folhas em que a arma já está embutida.

## Entrega do piloto

Comparação antes/depois do mesmo golpe, escala do jogo, oito direções, mesmos nomes de quadros e fases de contato. Apenas as regiões aprovadas são substituídas. Nenhuma nova arte do golpe pesado gerada nesta rodada foi importada no Unity.

## Fontes desta rodada

- Referência do usuário SHA256: `35918256FF50BBBE3B23BDB55AB3D7776B9859611CE83B8507EAA347B10CAFCF`.
- Folha pesada com katana escura: `exec-c0ac2d59-3e59-4687-b72b-cbcd8da151a2.png`, candidata, primeira ponta superior precisa reparo; não importada.
- Quatro poses: `exec-89619b9d-bad2-4005-97f8-93579c8fffb2.png`, demonstração do design, terceira pose muda orientação; não usar como pacote pronto.
- Folhas anteriores desta rodada com arma toda vermelha e carregamento com borda cortada foram rejeitadas após a correção do usuário; não importadas.

Verificação: 57/57 PlayMode após o reparo; 32 identificadores de sprites e PPU preservados. Pixels fora das seleções da arma permanecem iguais ao original. Comparação técnica na escala do jogo em `pilot-before-after.gif`; não é captura da campanha. A idade 4 continua protegida por `RenderingStage` até o pacote de animações ficar completo.
