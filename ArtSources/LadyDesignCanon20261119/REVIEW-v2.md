# Lady — correção proporcional selecionada, 19/11/2026

Uma segunda e última chamada integrada imagegen usou **as referências inteiras** Lady exata e Fabius Normal S. A candidata V1 permanece intacta, mas a seleção artística desta entrega é **`Lady-Idle-S-candidate-v2.png`**. Nenhuma expansão de animações/direções foi produzida.

PNG RGBA 548×1057; corpo visível 1025 px em alpha>=8; PPU **534.8183825827814**, visualScale 1.325 → altura corporal **2.5394134611477823** unidades, equivalente aos 302 px de Fabius com PPU 157.5757576. Pivô `[0.5255474452554745, 0.015137180700094607]`. GUID single Sprite staging **225b18571b84589087179467642f7d09**.

O bruto nativo RGBA 1199×1312 foi preservado como `Lady-Idle-S-v2-raw.png`. A candidata usa somente recorte técnico da figura inteira `[322,141,870,1198]`, com 16 px de margem em torno da silhueta de alpha>=8; igualdade de todos os pixels RGBA retidos confirmada. Nenhuma edição anatômica, recoloração, espelhamento ou redimensionamento do PNG. O baixo alpha residual 1–7 fora da silhueta foi parcialmente excluído pelo recorte; não houve remoção global de cor/alpha.

## Revisão artística e escolha

A comparação `Lady-v1-v2-Fabius-proportions-2x.png` mostra três figuras inteiras com altura corporal equivalente e linha de apoio comum. Somente a prévia foi uniformemente reduzida em nearest-neighbor; os assets preservam seus pixels nativos.

V2 mantém a expressão austera, olhos vermelho-escuros, cabelo castanho-avermelhado compacto, brincos de cruz, roupa preta/vinho com dourado, pistola na mão direita apontada para baixo e grimório verde-azulado no braço esquerdo junto ao peito. A cabeça ganhou tamanho relativo e as pernas/botas ficaram mais compactas; a medida aproximada manual coroa157→queixo465 é 308 px, **30.05%** do corpo de 1025 px. Isso atende o intervalo visual aproximado de 30–32% indicado pela coordenação e melhora a proximidade da geometria de Fabius. Lady permanece uma mulher adulta própria, mais esguia que Fabius; não se declara igualdade exata de toda anatomia.

Diferenças reais introduzidas no redesenho: o colar tem uma pedra vermelha mais nítida, a organização fina dos botões/fivelas e o emblema do livro variam. A revisão não identificou perda da identidade principal, mas **V2 não é cópia pixel a pixel da referência**. Fonte exata, V1, brutos e comparação permitem revisão/rollback. Não foi feita uma terceira geração.

## Validação e estados

Auditoria estática do helper: **1 quadro, 0 erros, 2 avisos**, em `audit-v2/`. Avisos: mais de 10% das amostras com alpha parcial; leitura textual do meta não verifica importação/runtime. A comparação sobre cinza não mostrou halo grosseiro; não comprova a renderização Unity. PNG exportado, hash, retângulo e pivô constam no JSON. Paleta completa em `palette-v2.json`. Prévia HTML/GIF de hold em `preview-v2/`; é repouso estático de 1000 ms para inspeção, sem novo clip Unity.

**Produzida/revisada estaticamente/selecionada em staging: sim. Integrada/importada/testada em Unity/publicada: não.** Apenas esta pasta ArtSources foi alterada. Nenhum Assets, Runtime, catálogo, Git, Play Mode, build ou teste da Unity foi executado.

O consumidor legado e sua normalização pelo canvas estão documentados em `REVIEW.md`. Ele precisa de tratamento explícito da coordenação para usar um Sprite único com esta escala; não existe vínculo automático criado aqui.

## SHA-256

- Fonte exata: `b21327f940c8c632358cec0a39f15f123b4d376d3e222825ad90414bc6f287bd`.
- V2 bruta: `dc54dfeea8e2cffef3cded9758d4c3030f682c43793e015c012f701ca65845ef`.
- V2 candidata: `06a88b75b7a2ebf57b4332a29b71d50baa680bf952b1ddd03981d6717cfcb340`.
- V1 preservada: `d8568a270af0896421c6be8b34e4de88a3da529149cbc5aa345619edf0a3dad6`.

Prompt exato da segunda chamada em `prompt-v2.txt`; hash do meta e demais entregas em `hashes.json`.
