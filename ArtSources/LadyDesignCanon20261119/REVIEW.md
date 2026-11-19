# Lady — piloto frontal em staging, 19/11/2026

Fonte direta: `C:/Users/Usuario/Downloads/Lyra Gótica com Pistola e Grimório.png`, preservada integralmente como `Lady-Reference-Exact.png`. Lady substitui o nome Lyra na direção atual; esta entrega não altera a campanha.

**Atualização final:** houve uma segunda e última chamada integrada, autorizada pela coordenação para corrigir a proporção corporal. **V2 é a candidata selecionada**; ler `REVIEW-v2.md`. A seção abaixo preserva a evidência da primeira candidata, que permanece separada como alternativa supersedida. Nenhuma delas foi integrada.

## Resultado e estados

- Uma única chamada à ferramenta integrada imagegen, solicitando extração do fundo. O original é RGB, 1367×1150, fundo preto; não possui transparência nativa.
- Resultado bruto preservado: `Lady-Idle-S-raw.png`, RGBA 1199×1312.
- Candidata: `Lady-Idle-S-candidate-v1.png`, RGBA 549×996. Recorte técnico inteiro `[320,160,869,1156]` da imagem bruta; igualdade RGBA de todos os pixels retidos confirmada. Sem montagem anatômica, espelhamento, recoloração ou redimensionamento do asset.
- Produzido: sim. Revisão visual estática: sim. Integrado/Unity importado/runtime testado/publicado: **não**. Sem edição de Assets, Runtime, catálogo, Git ou execução da Unity.
- `Lady-Idle-S-candidate-v1.png.meta` configura Sprite único (spriteMode 1), filtro Point, sem mipmaps ou compressão, pivô de apoio e PPU calibrado. É staging; inspeção de texto não comprova importação real.

## Revisão da figura inteira

Figura completa, mãos e botas preservadas. Expressão austera, cabelo castanho-avermelhado, olhos vermelho-escuros, roupa preta/vinho com detalhes dourados, brincos de cruz, pistola na mão direita apontada para baixo e grimório verde-azulado no braço esquerdo. Sem chão ou sombra externa evidente. Há pequenas diferenças de detalhe introduzidas pelo gerador: o resultado **não é cópia pixel a pixel do original**.

`Lady-Fabius-proportions-1x.png` e `Lady-Fabius-proportions-2x.png` mostram figuras inteiras com redução uniforme nearest-neighbor e apoio comum. Altura corporal equivalente: 302 px. A silhueta de Lady permanece mais esguia e sua cabeça proporcionalmente menor que Fabius. Portanto, a altura está calibrada, mas **a equivalência de proporções anatômicas não está concluída**. Não deformar cabeça/torso/pernas por pós-processamento para fingir equivalência. Caso a direção exija a mesma proporção cabeça/corpo, a próxima ação é um redesenho de uma figura inteira referenciado nas duas imagens, preservando rosto, roupa e pose de Lady.

## Escala e consumidor consultado

Fabius Normal: corpo 302 px, PPU 157.5757576, visualScale 1.325. Lady: corpo visível 964 px, limiar alpha>=8; PPU **502.9901666437086**, visualScale 1.325. Ambos resultam em altura corporal **2.5394134611477823 unidades**. O PPU calibra escala uniforme; não corrige proporção.

Pivô normalizado da candidata: `[0.5282331511839709, 0.01606425702811245]`, apoio bruto estimado em `[610,1140]`. GUID staging: `165aea7b3de058b2898664e25b115119`.

Consumidor legado lido: `Assets/Darklight/Runtime/Campaign/DarklightCampaign.cs`, `AddProp` (linhas 145–153): LyraMoreau recebe altura 2.5 e escala `height / sprite.bounds.size.y`, com oito direções no `DarklightNPCView`. Ele normaliza pelo canvas completo, portanto copiar o PPU isoladamente não garante altura corporal naquele caminho. O coordenador deve tratar o consumidor da candidata explicitamente; um repouso S único não entrega oito direções.

Tempos: este é um único repouso estático. O manifesto de prévia usa hold de 1000 ms com `allowStatic: true`, exclusivamente para inspeção; não existe clip de animação novo na Unity nem alteração dos timings de gameplay.

## Validação e limites

Helper Sprite Sheet Master: audit passou com **1 quadro, 0 erros, 2 avisos**. Avisos reais: alpha parcial em mais de 10% das amostras e inspeção de meta apenas textual. A comparação em fundo cinza não mostrou halo grosseiro, mas a aparência final ainda depende de importação e renderização. Alpha 1–7 contém resíduos de baixa opacidade; o bruto completo é preservado e os limites da figura foram medidos em alpha>=8. Não foi feita limpeza global de alpha nem de cor que pudesse apagar roupa escura ou detalhes.

A paleta completa do helper tem 141807 cores RGBA, incluindo alpha e detalhes da fonte gerada. Este número não é uma paleta artística controlada nem comprova resolução lógica pixel art nativa. O PNG entregue preserva a resolução nativa gerada; a comparação reduzida não é um novo asset.

Prévia estática/HTML/GIF de hold em `preview/`; ela não comprova movimento. Nenhum teste Unity, Play Mode ou suíte foi executado.

## Hashes SHA-256

| Arquivo | SHA-256 |
| --- | --- |
| Original exato | b21327f940c8c632358cec0a39f15f123b4d376d3e222825ad90414bc6f287bd |
| Bruto integrado | fd7fbe07b0a1b3b2aad351620c310ecf9b13152d66933242495d226102e5a0a7 |
| Candidata | d8568a270af0896421c6be8b34e4de88a3da529149cbc5aa345619edf0a3dad6 |
| Fabius referência | ce5c79d346c7ba7cbceb0eacfa69f2107d9543a66855b82071edb511657c891a |

Prompt exato em `prompt.txt`. A ferramenta usada foi a integrada, sem CLI/API alternativa.
