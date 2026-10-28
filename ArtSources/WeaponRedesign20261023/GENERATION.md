# Redesign de armas — fontes de 23/10/2026

Ferramenta: geração integrada de imagens. As fontes originais são cópias exatas dos PNG gerados, sem remoção de fundo, recorte anatômico, recoloração ou reescala. Os hashes reais constam dos manifestos JSON das folhas e das referências. `import-jobs.json` registra a origem de cada fonte.

Primeiro lote: Réquiem com Presas do Caos nas quatro formas. Cada fonte possui 12 personagens completos: três linhas com direções S/SO/O; quatro colunas neutral/inhale/blink/return. A revisão visual do agente responsável pela geração autorizou preparar estes lotes. A revisão técnica verificou figuras completas e alpha sem fundo visível; os RGB marrons da fonte Ruptura ficam transparentes. Os pixels de alpha 1 distantes permanecem preservados, sem limpeza global.

- `ReqDaggersNormalIdleA-source.png`: fonte 1448×1086; gerada em `exec-079b935e-17a7-4cc0-9adc-6ac17ae64b2a.png`.
- `ReqDaggersRupturaIdleA-source.png`: fonte 1536×1024; gerada em `exec-2cec9fbf-9bf7-4b86-841d-f4f08c336179.png`.
- `ReqDaggersEclipseIdleA-source.png`: fonte 1448×1086; gerada em `exec-a24e4019-2598-4187-8a3f-74de86d62087.png`.
- `ReqDaggersAbsolutoIdleA-source.png`: fonte 1448×1086; gerada em `exec-1fb8c1c5-069e-4c8e-b86c-5614ffb901fd.png`.

Segundo lote: katana e Punhos Normal S, 12 figuras por fonte 1448×1086. As linhas são Idle/Walk/Attack. A linha Walk de ambas foi rejeitada para runtime porque as pernas não alternam corretamente; fica preservada nas fontes completas, sem apagamento, recoloração ou substituição dos demais quadros. Os atlas selecionam somente as figuras inteiras de índices fonte 0–3 e 8–11.

- `ReqKatanaNormalSouth-source.png`: gerada em `exec-dc380c49-fde9-4f0f-a83e-825fa8464ab3.png`. Ataque substitui apenas `Leve1` e as quatro fases S da página `RequiemAttackKatanaYoungCardinal`.
- `ReqFistsNormalSouth-source.png`: gerada em `exec-f6d5b331-1fd0-4cc4-a026-39f2474f6af8.png`. Ataque substitui apenas `Direto da Ruína`, com três fases existentes distintas: `Combat_02` startup, `Combat_03` contato, `Combat_11` recuperação. A quarta figura nova de retorno é preservada no atlas/prévia, mas não associada artificialmente a outra técnica. Chutes, uppercut, slam e contragolpe continuam nas fontes anteriores.

A preparação usa `whole_figure_components.identify` para localizar somente figuras inteiras. O empacotamento alinha o chão por translação inteira, sem alterar a relação cabeça/corpo/armas. Retângulos, âncoras e offsets registram a correspondência fonte→atlas. Cada pixel RGBA de alpha positivo é reconstruído e comparado exatamente à fonte; contagem de pixels também é conservada. PPU fixo por atlas = mediana da altura corporal da primeira direção /1.98. Durações do ciclo: 350/250/180/350 ms.

O catálogo é parcial, com fallback para o mesmo contexto original quando não há redesenho. As fontes não cobrem toda a caminhada, os ataques, as direções nem todas as armas/formas. Não marcar o redesign completo como concluído. `visual-reviewed` indica inspeção documentada, sem aprovação artística explícita do usuário e sem prova de importação/runtime. Resolução nativa de pixel art indexada permanece não certificada; as fontes têm cores contínuas e bordas com alpha parcial.

As referências completas e seus hashes estão em `references.json` e `references/`. Não produzir personagens montados com partes dessas referências. Os originais de Downloads e ArtSources anteriores foram preservados.
