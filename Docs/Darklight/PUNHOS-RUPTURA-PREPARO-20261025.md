# Punhos / Ruptura — preparo em 25/10/2026

## Entrega preparada

Réquiem, Ruptura, Direto da Ruína: **nove figuras completas e três páginas S/SW/W**, com antecipação/contato/recuperação mapeadas aos nomes legados exatos `Combat_02/03/11`. Timings preservados: 75/65/130 ms. Não altera gameplay, colisões ou saves.

- Fonte íntegra: `ArtSources/WeaponRedesign20261025/candidates/ReqFistsRupturaDiretoS_SW_W-v1-source.png`, SHA256 `48190447b0a5c0ead156d37578090c24342eeb9a0a70308890d7b5117719e75d`.
- Fonte preservada de importação: `ArtSources/WeaponRedesign20261023/ReqFistsRupturaDiretoS_SW_W20261025V1-source.png`.
- Atlas/manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsRupturaDiretoS_SW_W20261025V1.png` e `.json`.
- Job e prompt: `ArtSources/WeaponRedesign20261025/candidates/ReqFistsRupturaDiretoS_SW_W-v1-job.json` e `-prompt.txt`.
- Fragmento para coordenação: `outputs/redesign-armas-20261025/punhos-ruptura/ReqFistsRupturaDiretoS_SW_W20261025V1-pages.json`.
- Script específico: `Tools/AnimationPreview/prepare_punhos_ruptura_20261025.py`. O catálogo compartilhado não foi modificado por esta subtarefa.

Inspecionadas fonte inteira e contato empacotado: ambos os braços permanecem dentro de manoplas físicas laminadas, grevas completas nos dois pés, fissuras vermelhas e bordas prateadas. Mechas carmesim fortes nas laterais/camadas inferiores do cabelo, incluindo a nuca que aparece de perfil. Olhos vermelho escuro e pele limpa preservados. Não houve recoloração por script, montagem anatômica, espelhamento ou redução dos sprites.

## Escala

PPU **172.2222222**, calculado pelo corpo de 341 px de coroa a sola na primeira linha, em postura comparável ao Normal, para referência anatômica de 1,98 unidade antes de `WorldVisualScale=1.325`. Não utiliza largura das manoplas, capa, efeitos ou altura de arma erguida. Uma escala por folha; apoios individuais anotados pelo ponto de chão entre os pés, sem reajustar o corpo por quadro. Medição visual tem tolerância aproximada de ±4 px.

Comparação vista em `outputs/redesign-armas-20261025/punhos-ruptura/scale-compare-canonical-normal-ruptura.png`: katana canônica Normal, Punhos Normal e Punhos Ruptura na mesma escala de mundo efetiva, 100 px/unidade e o mesmo `WorldVisualScale`. As miniaturas não foram autoajustadas aos respectivos quadrados. Porte e proporções do novo Ruptura são comparáveis às referências; não foi observada redução corporal ou infantilização nesse lote. Isso não certifica os demais desenhos/formas do jogo.

## Auditoria e limites

Auditoria do atlas: **0 erros / 12 avisos**, referentes a paleta, alpha e variação de altura que requerem inspeção, não apagamento automático. RGBA dos nove quadros completos reconstruído exatamente. PNG RGBA nativo, sem redimensionamento de anatomia; resolução lógica indexada ainda não certificada.

Contato, paleta, GIFs e HTML: `outputs/redesign-armas-20261023/ReqFistsRupturaDiretoS_SW_W20261025V1/`. Fonte e contato estático vistos; movimento HTML/GIF, importação Unity e gameplay **não executados por este agente**. Nenhum Play, som ou commit nesta subtarefa. Aprovação humana anterior se refere ao padrão de cabelo de Presas/Ruptura, não a este atlas ou ao redesign global.

## Oito direções e pendências

| Vista | Direto novo | Parada/idle novo |
|---|---|---|
| S / frente | Preparado, 3 fases | Pendente |
| SW / frente-esquerda | Preparado, 3 fases | Pendente |
| W / esquerda | Preparado, 3 fases | Pendente |
| NW / costas-esquerda | Candidata pendente; suspeita de mão trocada no contato | Pendente |
| N / costas | Fonte candidata produzida, sem integração/revisão final de escala | Pendente |
| NE / costas-direita | Fonte candidata produzida, sem integração/revisão final de escala | Pendente |
| E / direita | Pendente | Pendente |
| SE / frente-direita | Pendente | Pendente |

A folha de nove figuras `ReqFistsRupturaDiretoNW_N_NE-v1-source.png` terminou enquanto a coordenação determinava fechar os lotes em voo para reservar o saldo de uso. Está preservada localmente como **candidata não integrável nesta entrega**: o contato NW aparenta trocar o braço dominante, e o porte relativo do corpo/cabeça deve ser confrontado antes de qualquer importação. Não foi empacotada em Assets e não deve entrar em publicação como arte concluída. Há nove componentes completos tecnicamente, o que não aprova direções/anatomia. Corrigir posteriormente apenas quadros completos afetados, preservando os corretos, sem transplantes ou espelhamento.

Também continuam pendentes Rock/Ruptura, outros golpes/combos/ações, caminhada/corrida e todas as transições pertinentes. Não foram iniciadas outras gerações após o aviso da coordenação para encerramento ao saldo de 10%.
