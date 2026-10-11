# Evolução física de Fábio

Sistema aprovado para a Alpha 0.1. Fábio começa com **17 anos** e a campanha representa aproximadamente **um ano**, conforme definido pelo criador.

| Marco existente | Dia narrativo configurado | Estágio |
|---|---:|---|
| Rua das Cinzas | 0 | Inicial |
| Distrito do Vinil | 73 | Desenvolvimento |
| Catedral do Sangue | 146 | Intermediário |
| Laboratório Eclipse | 219 | Intermediário |
| Jardim dos Ecos | 292 | Avançado |
| Observatório Vespera | 365 | Final |

Os intervalos intermediários são valores de configuração distribuídos ao longo do ano; não acrescentam acontecimentos ou cenas à história. São editáveis no asset `Assets/Darklight/Data/Appearance.asset`. XP, tempo de sessão, morte e repetição de uma fase não contam como passagem de tempo. O save guarda dias narrativos, maior capítulo alcançado e estágio visual. Uma nova campanha reinicia a evolução.

O estágio inicial usa o visual original. Os demais acrescentam cabelo castanho ondulado mais longo, bigode e cavanhaque ralos e pequenas mudanças no contorno dos ombros e no sombreado muscular. Não há barba cheia. A altura permanece igual: o sistema aceita variantes desenhadas, mas não estica sprites nem modifica hitboxes, alcance, câmera ou colisões.

## Arte e integração

- `appearanceHair.png`: 32 camadas de cabelo — quatro comprimentos adicionais × oito direções.
- `appearanceWind.png`: 32 quadros alternativos para movimento e vento leve.
- `appearanceBeard.png`: 20 camadas — quatro estágios × cinco direções em que o rosto é visível. Vistas de costas ficam sem pelos faciais visíveis.
- `appearanceShoulders.png`: 16 camadas — duas roupas × oito direções, limitadas ao contorno da silhueta original.
- Os arquivos `*Rects.json` registram os recortes reais, pivôs, dimensões das fontes e ordem. As imagens foram copiadas sem edição ou redimensionamento por scripts; somente os metadados foram calculados a partir do alpha.
- Os 100 sprites adicionais são compartilhados entre as três armas, ambas as skins e os quadros existentes. O perfil registra pontos de encaixe de 527 sprites distintos. Recortes e pontos são corrigíveis no editor.

Geração realizada pela ferramenta integrada ImageGen, seguindo `segredo-varginha-pixelart`, com os atlas aprovados `FabioIdle.png` e `skinIdle.png` como referências. Briefs: cabelo separado com face transparente e comprimentos crescentes; segundo quadro mantendo a coroa e movendo apenas pontas; bigode/cavanhaque juvenil ralo sem pele ou rosto; contornos de ombros discretos para sobretudo e colete. As correções pediram crescimento maior nas últimas linhas e remoção de pixels de pele da camada de cabelo.

## Prévia

Na Unity: **Darklight → Aparência de Fábio**. Escolha estágio, skin, arma, direção e pose. A prévia não inicia Play Mode, não toca áudio nem altera o save. Remova-a pelo botão ou fechando a janela.

`estagios.png`: renderização em modo de edição; linha superior Requiem, inferior Rock; colunas Inicial, Desenvolvimento, Intermediário, Avançado e Final.

## Verificação e limites

Compilação e imagens estáticas são verificadas durante a montagem. Não foram executadas partidas, testes de animação em movimento ou testes de controle físico, conforme a preferência do criador. Conferir manualmente corridas, esquivas, parry, dano, morte, golpes das três armas, transformações, troca de skin e retratos. Pontos de encaixe são derivados das cores da arte e podem precisar de ajuste em poses extremas; a prévia permite revisar cada uma.
