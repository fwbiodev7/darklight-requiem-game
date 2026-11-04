# Réquiem Normal — Katana — Leve 1 (candidato, 24/10/2026)

Seleção para revisão/integração: **S e W da v1; somente SW da v2**. Preservar os quadros S/W v1 pixel a pixel. A v1 SW foi rejeitada por frontalidade excessiva. A v2 foi gerada antes de chegar a orientação de corrigir apenas os quatro SW; seus demais oito quadros não substituem os v1.

- Fontes originais: `ReqKatanaNormalLight1_S_SW_W-v1-source.png` e `ReqKatanaNormalLight1_S_SW_W-v2-source.png`, 1536×1024 RGBA cada. Nenhum pixel de fonte alterado.
- SHA-256 v1: `0c701bcea3da3682349d3d5567d99be05d7261df14b4f016d5f5a7c99af47795`.
- SHA-256 v2: `67dca8434d9abdcc847d595eb9b174e801e4b9d9c5376615644932e26360611a`.
- Cada linha contém antecipação, contato, seguimento e recuperação; durações de contrato `[55,75,70,70]` ms. As fases selecionadas mudam articulação e apoio; katana na mão direita.
- Katana aço/preto/prata da coluna NORMAL, cabelo castanho escuro, casaco preto/forro vinho, olhos vermelhos escuros, sem fissuras ou aura energética.
- Os 24 desenhos completos das duas fontes foram inspecionados. Composições técnicas claras/escuras e contatos foram vistos. GIF/HTML foram gerados; a animação não foi assistida no navegador.
- Auditoria de cada fonte: 12 quadros, 0 erros estruturais, 1 aviso de alpha parcial. Alpha real 0..254, sem pixels plenamente opacos; fundos compostos ficaram visualmente limpos. Não foi feita remoção global de alpha/cores.
- Retângulos irregulares em `*-manifest.json`; âncoras/PPU servem apenas à prévia e **não foram verificados na Unity**. Atenção às margens curtas em alguns quadros W v1; ampliar canvas transparente ao empacotar, preservando todo desenho.
- Fontes, prompts completos, funções das referências, hashes e avaliação em `ReqKatanaNormalLight1-review.json`; medidas de alpha/retângulos em `ReqKatanaNormalLight1-source-audit.json`.
- Resolução lógica nativa ainda não comprovada; as paletas completas são grandes por causa da variação de alpha e não certificam uma paleta nativa limitada.

Status: candidato inspecionado pelo agente. **Sem aprovação visual do usuário, importação, edição de catálogo ou teste de runtime.** Nenhum Play/som foi iniciado.
