# Katana / Normal — retomada 13h50 — 25/10/2026

## Pronto para integração incremental

**Rock Normal: 64 figuras inteiras, 6 atlas, 16 páginas**, com `skin=1`, `weapon=0`, `form=0`: 32 de parada/Idle e 32 de Leve1, ambas as ações nas oito direções. Fragmentos: `outputs/redesign-armas-20261025-retomada1350/katana-normal/rock-katana-normal-idle-pages.json` e `rock-katana-normal-leve1-pages.json`. Jobs dos seis atlas, com grade original, seleções e suportes: **`ArtSources/WeaponRedesign20261023/import-jobs-katana-rock-1350.json`**. Fontes, prompts e cópias canônicas preservados em `ArtSources/WeaponRedesign20261025Retomada1350/katana-normal/`; hashes e QA junto dos fragmentos.

Estado deste responsável: produzido/revisado/empacotado, **não importado nem testado em Unity**. Não houve alteração de catálogo compartilhado, código de gameplay, áudio, saves ou fontes aprovadas de Réquiem.

| Direção | Figuras Idle | Revisão física |
|---|---:|---|
| S | 4 | Frente, dois olhos, peito/rosto/quadril concordam; katana direita, corrente esquerda. |
| SW | 4 | Diagonal frontal esquerda distinta de S; espada e mãos coerentes. |
| W | 4 | Perfil esquerdo, um olho, mão próxima esquerda vazia; katana direita parcialmente projetada à frente. |
| NW | 4 | Costas/esquerda, nuca e lateral esquerda; não olha frontalmente para a câmera. |
| N | 4 | Nuca e costas completas, sem face/olhos; parte traseira do colete, sem emblema frontal. |
| NE | 4 | Costas/direita, nuca e lateral direita; cabeça/ombros/tronco/quadril/pés alinhados. |
| E | 4 | Perfil direito, um olho; mão próxima direita segura a katana. |
| SE | 4 | Diagonal frontal direita distinta de E, roupa/empunhadura/corrente sem espelhamento. |

Cada direção tem quatro desenhos: parada neutra, inspiração, expiração/piscada quando visível e retorno. A parada usa o quadro0; não foi contada como uma animação extra. O ciclo preserva 350/250/180/350ms. A blusa/colete/botas/pulseiras/corrente mudam para Rock; rosto, porte, idade, cor/volume do cabelo e corpo seguem o cânone de Réquiem.

QA estrutural: **zero erros; 75 avisos**, sendo 35 de Idle e 40 de Leve1. Avisos de paleta/alpha e altura aparente de pose/arma foram mantidos para revisão, sem quantização/recoloração destrutiva. Igualdade RGBA fonte→atlas confirmada. Contatos, paletas, GIFs e HTML em `qa/`. Muitos tons e alpha parcial não comprovam pixel art nativa indexada. Não houve Play/áudio/teste com controle físico.

## Leve1 e reparo seletivo

Os oito contextos Rock de Leve1 possuem quatro figuras completas por direção, com mapeamento por `Today_RockAttackKatanaYoungCardinal/Diagonal` e `attackKey=Leve1`. Timings do consumidor preservados; prévia55/75/70/70ms. A tabela física de oito vistas acima também foi conferida no ataque, com movimentos próprios de antecipação/contato/seguimento/guarda.

A geração traseira inicial apresentou cinco defeitos concretos: corrente no quadril errado nas quatro figuras NW e katana na mão esquerda na antecipação NE. Foram substituídos **somente os quadros completos0/1/2/3/8**. Os **sete quadros originais4/5/6/7/9/10/11 permaneceram RGBA idênticos**, com prova em `rock-leve1-back-selective-provenance.json`. A fonte técnica selecionada organiza figuras inteiras de duas fontes preservadas, sem recortes anatômicos, redimensionamento, espelhamento, recoloração ou deformação. Os seis jobs públicos usam apenas fontes finais corretas; fontes mistas originais, reparo intermediário e `preselection/` são preservados localmente e excluídos da publicação.

## Escala de mundo e anatomia

`rock-requiem-world-scale-review.png` compara parada0 nas oito direções das duas skins, a **112 pixels por unidade de mundo pelo PPU real**, sem ampliar cada personagem para preencher sua célula. Dados em `rock-requiem-world-scale-review.json`. Um PPU por folha, baseado em coroa→sola na postura relaxada comparável, excluindo a katana. Rock apresenta aproximadamente 1,942–2,021unidades nas poses mostradas; diferenças pequenas de apoio/projeção permanecem registradas.

Os pontos coroa/sola têm incerteza aproximada ±3–8pixels; pescoço/cintura sob roupa têm ±10–18pixels. A comparação visual em escala de mundo não apontou versão infantil/menor nem crescimento do cabelo em Rock. Nenhuma anatomia foi deformada e os pixels-fonte foram preservados. Pivôs/câmera/transições ainda precisam de revisão no consumidor Unity.

`rock-requiem-leve1-world-scale-review.png` compara também as oito guardas recuperadas das duas skins pelo PPU real. As novas guardas Rock variam aproximadamente1,874–2,101unidades na pose; agachamento/projeção e incerteza de cerca de5–12pixels alteram a altura aparente. Cabeça/tronco/pernas foram examinados no painel, sem compensação de escala por quadro e sem usar a arma erguida para medir o corpo. A fonte canônica W anterior também tem pose mais baixa. Essas estimativas não certificam igualdade anatômica exata nem aprovação humana final.

## Caminhada rejeitada e revisão traseira

Foram feitas exatamente **duas novas tentativas** de Walk SW/W com quatro fases por direção. Ambas continuam repetindo a perna líder entre os contatos opostos; a segunda ainda oculta/omite a katana nos quadros de passagem de W. Nenhuma foi empacotada ou integrada. Fontes fora de Assets em `rejected/`, detalhes em `walk-rejected-review.json`. WalkS-v5, Leve1 e Idle de Réquiem foram preservados.

Após a captura humana de Punhos com cabeça frontal sobre corpo de costas, foram reinspecionadas as **24 figuras traseiras existentes de Leve1/Idle Réquiem** e as 12 novas de Idle Rock: cabeça/nuca/pescoço/ombros/tronco/quadril/pés concordam em NW/N/NE. Não foi identificado o defeito de pescoço180° nesses quadros. A ação foi preservar os quadros corretos pixel a pixel; não aplicar rotação de cabeça ou regeneração global. Evidência: `back-direction-review.json`.

## Próximos passos reais

Prioridade humana atual: **completar Normal de Réquiem e Rock em todas as armas/ações, depois transformações, depois Lucius**. Idle e Leve1 Rock estão produzidos/revisados/empacotados; o integrador registra importação/runtime separadamente. Demais ataques/combos, movimentos, esquivas/defesa/dano/morte/transições continuam pendentes. Não contar Réquiem como cobertura de Rock. Transformações não foram ampliadas nesta tarefa. Gerações encerradas após o lote autorizado para permitir integração, verificações, commit/push e parada no limite humano de15% nesta sessão.
