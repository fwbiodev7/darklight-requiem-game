# Contrato visual da retomada — 25/10/2026

Pedido ativo: continuar o jogo Unity e terminar o escopo de ontem pelas referências reanexadas. Minecraft permanece pausado. Fontes, sprites corretos, saves, eventos, colisões e identidades aprovadas são preservados. Não iniciar Play nem áudio por conta própria.

## Referências e alterações expressas de hoje

- `ArtSources/WeaponRedesign20261025/references/lucius-four-forms.png`: Lucius, cabelo prateado curto e sobretudo preto/azul; formas **Normal, Ascensão, Fenrir e Nexo**, conforme os títulos da ficha atual. Isso atualiza o planejamento visual antigo Fractura/Nexus. Não representa expansão implementada.
- `ArtSources/WeaponRedesign20261025/references/fabius-transformations-motorcycle.png`: equipamentos e moto de Fabius nas formas Ruptura, Eclipse e Absoluto. Normal segue a coluna neutra da ficha de equipamentos anterior e a identidade canônica aprovada.
- Réquiem e Rock são o mesmo Fabius, mudando roupas e acessórios; não alterar rosto, idade, corpo, pele ou olhos.
- Após ver Presas/Ruptura v2, o usuário aprovou o padrão de cabelo e pediu aplicá-lo em todas as armas/skins: **Ruptura com mais mechas vermelhas, incluindo laterais e nuca; Eclipse com mechas violetas igualmente evidentes**. A base castanha permanece visível. Normal mantém castanho natural; Absoluto mantém prateado. Não tingir pele/olhos pela forma.
- A aprovação citada se refere ao padrão do cabelo visto, sem aprovação global de direções, animações, importação ou gameplay.
- Correção posterior expressa do usuário: o cabelo das Presas/Ruptura cresceu indevidamente. Aumentar COR/saturação/distribuição das mechas sem aumentar volume, comprimento, nuca ou silhueta em relação ao corte canônico. PresasV4 frontal é o reparo seguro; lotes restantes afetados ficam retidos. MotoRuptura/Eclipse retornam às fontesV1 enquanto a cor mais forte sem crescimento não passar revisão.

## Oito posições reais por contexto

Ordem técnica: **S frente, SW frente/esquerda, W esquerda, NW costas/esquerda, N costas, NE costas/direita, E direita, SE frente/direita**. Conferir o desenho de cada direção: rosto/nuca, tronco, mãos, equipamento, corrente e demais assimetrias. Nomes e contagem não comprovam vistas corretas. Não espelhar assimetrias para preencher lacunas; giro de tronco legítimo durante golpe não substitui a orientação de base correta.

Essa exigência vale por skin × arma × forma × ação. Uma folha de oito poses estáticas não completa os ataques, caminhada ou idle animado.

## Parada e idle

O usuário reiterou expressamente pose parada **e ciclo idle em todas as formas**, além das ações/combos anteriores. Registrar os dois estados por skin/arma/forma/direção. O primeiro quadro neutro de um idle pode servir como pose parada; isso não acrescenta uma figura inédita à contagem. Respiração, piscada, peso e apoio devem ser desenhados e conferidos. Repetir uma figura ou mover o canvas não constitui ciclo novo.

Na moto, oito poses montadas durante avanço não são repouso montado nem ciclo idle. Não associar as novas poses a ações sem arte correspondente. Réquiem/moto continua N/A conforme o gameplay existente.

## Escala e maturidade da identidade

O usuário reiterou que todas as animações e formas precisam do mesmo porte e estilo, sem figura com aparência infantil ou menor. Usar a identidade canônica como autoridade de idade, cabeça, tronco e membros. Calibrar PPU pela anatomia em postura comparável; arma erguida, cristais, cabelo, capa, aura ou a moto acima da cabeça não definem a altura do personagem. Não reajustar o corpo a cada quadro para igualar bounding boxes; agachar ou inclinar legitimamente altera a projeção da altura.

Registrar referências anatômicas, medida e incerteza, mantendo um PPU por folha e pivôs nos apoios reais. Comparações visuais devem usar a mesma escala de mundo com os PPUs efetivos e o WorldVisualScale do jogo, nunca autoajustar cada miniatura ao seu quadrado. Detectores de altura são avisos e não substituem conferir proporção/idade/estilo. Corrigir desenhos de proporção errada como figuras completas.

## Responsabilidade e entrega

Subagentes responsáveis por arma e forma, processados conforme os três slots paralelos disponíveis. A coordenação revisa fontes e consolida catálogo/importação. Cada entrega distingue produzido, revisado, importado e verificado em gameplay. Não rotular o redesign integral como concluído enquanto faltarem contextos.

Prioridade permanece: redesign → Combat AI 2.0 → expansão Sangue do Abismo → site/material promocional. As novas referências não autorizam anunciar esses conteúdos futuros como disponíveis.
