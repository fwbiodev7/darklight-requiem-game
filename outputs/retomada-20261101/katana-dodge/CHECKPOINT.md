# Katana Normal Dodge — checkpoint01/11/2026

Estado atual após renovação: **24 figuras completas/8 direções em4 folhas `visual-reviewed`**, incluindo novo conjunto NEV5 aprovado expressamente pelo usuário. As21 figuras anteriores permanecem pixel a pixel intactas. NEV5 está preparado para integração,sem importação/runtime executados por este agente. Sem Unity/Git/Assets/Runtime/catálogo compartilhado alterados por este agente. Os relatos históricos abaixo preservam falhas e estados anteriores.

| Folha em `staged/` | Direções | Figuras | Atlas | PPU pelo corpo | QA |
|---|---|---:|---|---:|---|
| ReqKatanaNormalDodgeS_SW_W20261101V1 | S/SW/W |9|2463×1140|154.5454545|0 erros/11 avisos|
| ReqKatanaNormalDodgeNW_N20261101Selected | NW/N |6|2241×824|161.3636364|0 erros/8 avisos|
| ReqKatanaNormalDodgeE_SE20261101V1 | E/SE |6|2010×1102|201.7676768|0 erros/7 avisos|
| ReqKatanaNormalDodgeNE20261101V5 | NE |3|2754×649|242.9292929|0 erros/5 avisos|

Cada folha tem PNG+JSON,página por direção,fragmento de catálogo,paleta,contato,HTML,GIF e tiras de fases. Todos usam40/150/40ms=230ms,sem loop. Invulnerabilidade Normal20≤t<190ms é regra do consumidor,não alegação do GIF. Todos os pixels RGBA selecionados preservados; nenhum recolor,apagamento de alpha,resampling anatômico,espelhamento ou transplante. Pivôs manuais pelos apoios das botas; PPU por mediana de corpos de preparação/recuperação,excluindo investida baixa.

Hashes SHA256 dos atlas:

- S/SW/W: `c9dcb3731dc99d1d19878594a6906c15c5a68282fc6b3329fc1c0aa66d026a37`
- NW/N: `187f654a786407eaf7466dbddaa714b6dd5d13e5e3812b1ff4a9194300fc7c20`
- E/SE: `0ca59df6487ead5aa456a2b099f00e9b2b15c23b9c387428b805130466873e7a`

Fragmentos prontos:

- `dodge-catalog-fragment.json`
- `ReqKatanaNormalDodgeNW_N20261101Selected-catalog-fragment.json`
- `ReqKatanaNormalDodgeE_SE20261101V1-catalog-fragment.json`

Revisão: fontes completas e contatos claros/escuros,1×/nearest2×,fases inteiras dos7 GIFs inspecionadas. Confirmados rosto/cabelo/pele/roupa/proporções/katana fiéis às referências pertinentes e sequências baixando centro de massa com apoio/recuperação. Avisos técnicos são paleta ampla,alpha parcial e variação intencional de altura; pixel grid indexado estrito não certificado. Playback interativo `motionPreviewViewed=false`: CUA neste subagente não permite IAB visível e localhost está bloqueado por socket; limitação já comprovada nesta sessão. Não alegar observação humana do movimento ou teste Unity.

## NE pendente — não integrar candidatas defeituosas

1. `ReqKatanaNormalDodgeNW_N_NE20261101V1-source.png`: NW/N índices0–5 corretos,extraídos intactos na folha Selected acima. NE6–8 usava mão esquerda. Não promover NE dessa fonte.
2. `ReqKatanaNormalDodgeNW_N_NE20261101V2Repair-source.png`: NE preparação6 e recuperação8 corrigidas para mão direita; investida7 ainda incorreta. Preservar figuras6/8 inteiras. Retângulos observados:6=`[49,669,497,1006]`,8=`[1105,672,1524,1007]` em coordenadas esquerda/topo/direita/base; a extração final precisa manter ownership de alpha,sem corte anatômico. Fonte1536×1024.
3. `ReqKatanaNormalDodgeNW_N_NE20261101V3LowRepair-source.png`, SHA256 `ccf6ca441d7a469375b8367466526191707ed1dce84620e4072a5a8da3aa5672`: mão direita da investida corrigida,mas grandes blocos/sombras cinza no alpha. Fonte possui apenas148 pixels totalmente transparentes; identify falhou `Expected 9 complete connected figures; found 5; areas=[71478, 72171, 88677, 217496, 224459]`. Fundo branco inspecionado em `NE-V3-source-light.png` comprova manchas. Rejeitada por estrutura/transparência; não promover nem apagar alpha globalmente.

Próxima correção artística exata: **uma figura NE baixa inteira**,com cabeça/corpo/katana juntos,mão direita e alpha limpo; preservar preparação/recuperação NEV2 pixel a pixel e NW/N6 já corretos. Depois empacotar somente figuras completas de fontes rastreadas,3 fases40/150/40ms,calibrar PPU pelo corpo e revisar o clip inteiro. NE nunca foi promovida ou declarada completa.

Prompts/fontes/hashes e receitas locais preservados em `ArtSources/WeaponRedesign20261101/katana-dodge/` e nesta pasta. Trabalho global Fabius Normal continua incompleto; não avançar transformações ou declarar encerramento do redesign.

## Retomada após renovação — tentativa isolada V4

Uma única tentativa diferente,conforme atribuição: alvo passou a ser somente a figura NE baixa inteira,isolada em canvas1024×1024 com escala nativa1; as guardas V2 completas foram referências separadas e ficaram intactas. Proveniência das extrações técnicas em `NE-whole-reference-provenance.json`. Guardas também inspecionadas compostas sobre branco: sem os blocos externos da V3.

Nova fonte `ArtSources/WeaponRedesign20261101/katana-dodge/ReqKatanaNormalDodgeNELow20261101V4Isolated-source.png`, SHA256 `bd170568a3ed55e2e33733397111e458adc6b66dbd31d81d226cfd8eaa674ef1`:1254×1254,uma figura completa,mão direita corrigida,alpha limpo (nenhum pixel externo além16px com alpha≥32;máximo1). **Rejeitada para o clip por escala nativa**: figura baixa ficou489px de altura,contra301px anterior e guardas333/331px. O banco atual requer um PPU único por página; empacotar junto faria a investida crescer. Não aplicar redução do desenho nem editar as guardas corretas. Registro em `NE-isolated-v4-review.json`;prévia em branco `NE-V4-isolated-light.png`.

Não houve segunda tentativa sequencial desta retomada. Nenhum clip NE pronto ou página nova promovida.21 figuras corretas e guardas V2 preservadas. Aguardar próximo gap delimitado da Katana pela coordenação. Ordem mais recente transmitida: **Fabius → partículas → atmosfera → Lucius por último**;nenhuma partícula ou transformação nova gerada.

## NE resolvida artisticamente — conjunto integral V5 aprovado

Após nova atribuição e insistência expressa do usuário,foi produzido um **novo conjunto NE de3 figuras completas juntas**,sem sobrescrever as guardas V2,as candidatas anteriores ou as21 figuras já importadas. A fonte V4 serviu somente como referência de pose/anatomia,sem impor tamanho pixels a esse novo conjunto. Guardas V5 têm483/479px (diferença<1%);investida428px por agachamento,intencional. Mão direita,head/body ratio,cabelo/roupa/arma coerentes nas3 figuras.

**Aprovação humana explícita em01/11/2026** transmitida pela coordenação: “aprovado essa versao do conjunto NE tambem de fabius audit”. Referência exata: fonte2172×724 `ArtSources/WeaponRedesign20261101/katana-dodge/ReqKatanaNormalDodgeNE20261101V5-source.png`, SHA256 `277c5cfda3da67a4501a01798eb7fa389d048d19b6cb57c594002ff3ad6eaa75`. Registro `NE-V5-human-approval.json`; `humanApproved=true` no manifesto,recibo e fragmento. Nenhuma alteração estética após aprovação.

Entrega integrável:

- `staged/ReqKatanaNormalDodgeNE20261101V5.png` e `.json`.
- `ReqKatanaNormalDodgeNE20261101V5-catalog-fragment.json` e `ReqKatanaNormalDodgeNE20261101V5-receipt.json`.
- Atlas2754×649,PPU242.9292929 pelo corpo,3 figuras/1 clipNE,40/150/40ms=230ms,sem loop.
- SHA256 atlas `6132fa3bae8f41c770df736a60a6f7607e800225c2e923f9705f508c8e17eb79`;pixelsRGBA da fonte preservados exatamente.
- QA0 erros/5 avisos (paleta ampla,alpha parcial,altura intencional de agachamento). Apenas39 pixels fora16px da silhueta,alpha máximo1;nenhuma limpeza de alpha aplicada. Contatos claros/escuros,1×/nearest2× e tira das3 fasesGIF vistos;timings230ms confirmados. Pixel grid indexado estrito não certificado.
- Importação/runtime e playback Unity pertencem à coordenação;`unityImported=false`,`runtimeVerified=false` neste recibo. Playback interativo não observado pelo subagente;aprovação artística do usuário e revisão estática/sequencial são estados separados do teste real.

Assim a cobertura artística Dodge Katana Normal passa a24 figuras/8 direções. Isso não declara concluídos todos os gaps de Fabius Normal.
