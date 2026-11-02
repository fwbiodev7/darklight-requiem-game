# Punhos Normal — caminhada parcial, 01/11/2026

Entrega de preparação: **4 figuras inteiras S, 1 clip, visual-reviewed**. Nenhuma importação Unity ou alteração de Assets/catalog/Git nesta tarefa. Não é conclusão global de locomoção.

## Fontes e tentativa

- Ferramenta integrada imagegen, duas tentativas de folha12:3direções×4fases. Prompts integrais e fontes originais preservados em `ArtSources/WeaponRedesign20261101/punhos-walk/`.
- V1 SHA256 `9c27b193fe0d20dc51c9676d01ab5b29f6ddd1be7eaffbec210daaf59b9618c0`:12figuras, rejeitada como caminhada completa; SW/W repetem perna, passagensS insuficientes.
- V2 SHA256 `69df7ad59d4c7e5e66f5a6b74cce721db9310f3fe480b06ee642c426b9dc1103`:12figuras; somente índices0–3/S selecionados. Oito SW/W rejeitados por repetição de perna dianteira. Não gerar mais a mesma correção.
- Referências integrais inspecionadas: Punhos Idle S/SW/W de25/10; Katana Idle S/SW/W de25/10 (identidade/maturidade); olhos20/10. Seus caminhos e hashes estão no manifesto selecionado.
- Alternativa sugerida pelo coordenador, converter KatanaWalkSW_W aprovada: fonte inteira inspecionada, mas também repete a bota dianteira. A condição de contatos opostos comprovados não se confirmou; conversão não executada. Original intacto.

## Entrega selecionada

- `staging/ReqFistsNormalWalkS20261101V2Selected.png` + `.json`:RGBA nativo1880×365; fonte1536×1024. Não houve redução, espelhamento, montagem anatômica ou alteração dos pixels desenhados. Resolução lógica/indexação de pixel art não comprovada.
- Fonte→retângulos reais→atlas→clip documentados por quadro no JSON; igualdadeRGBA exata e contagem de pixels preservadas pelo preparador existente.
- PPU170.9595959596, alturaS mediana338,5px/1,98unidades; incerteza3px. PPU baseado no corpo cabelo–solas, sem usar largura de manoplas/casaco. Não corrige proporção anatômica; escala no consumidor pendente.
- Quatro fases100ms propostas, loop400ms. S00 contato esquerdo anatômico; S01 apoio esquerdo/passagem direita; S02 contato direito; S03 apoio direito/passagem esquerda. Pernas e botas opostas distinguíveis. A quarta pose repete tendência de braço da segunda; coordenador aceitou a repetição dos braços desde que o ciclo das pernas fosse legítimo. Manoplas permanecem nas mãos corretas.
- Pivôs: projeção do eixo corporal no plano da sola apoiada, coordenadas inteiras da fonte. Sem usar centro variável do casaco. Ajuste de contato em movimento depende do consumidor.
- `catalog-fragment.json`:somente1página Walk/S/skin0/weapon2/form0, sem figuras rejeitadas. Caminho de recurso é destino proposto; arquivo ainda está em staging.
- Contato, GIF, HTML interativo sem áudio, paleta e auditoria em `ReqFistsNormalWalkS20261101V2Selected/`.

## Revisão e limites

- Auditoria estrutural executada:0erros,5avisos — mais256cores em cada figura e alpha parcial. Isso não certifica pixel art indexada. Alpha distante além16px tem máximo1/255; nenhum fundo visível detectado; nenhuma remoção global de alpha/cor.
- Fonte completa e contato nativo/zoom nearest-neighbor inspecionados. Cabelo curto castanho, olhos vermelho escuro, pele limpa, roupa/preto-vermelho, corrente à esquerda anatômica e equipamento prata conferidos.
- GIF gerado e aberto por view_image, que mostra um quadro estático. **Movimento contínuo ainda não visto nesta tarefa.** Navegador recusou file:// pela política; nenhum contorno tentou abrir a mesma página por outra superfície.
- Importado=false; runtimeVerified=false; visualApproval=false. Coordenador deve testar movimento contínuo, apoio e transição com Idle em Unity antes de afirmar verificação em jogo. Nenhum teste de áudio ou controle físico.
- Pendentes:SW/W corretos; NW/N/NE/E/SE não produzidos aqui. Candidatas completasV1/V2 e seus previews ficam arquivados para diagnóstico, fora do fragmento selecionado.

Recibos estruturados: `preparation.json`, `visual-review.json`, `ReqFistsNormalWalkS20261101V2Selected-prepared.json` e `ArtSources/WeaponRedesign20261101/punhos-walk/review.json`.
