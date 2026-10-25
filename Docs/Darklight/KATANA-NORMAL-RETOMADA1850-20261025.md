# Katana / Normal / Leve2 — retomada 18h50 — 25/10/2026

## Entrega deste responsável

**64 figuras completas finais: 32 Réquiem +32 Rock, quatro fases em oito direções por skin.** Seis atlas,16 páginas, `weapon=0`, `form=0`, `action=Attack`, `attackKey=Leve2`. Produzido, revisado visualmente pelo agente e empacotado; **não importado nem verificado em Unity por este responsável**. Prévia de navegador não substitui importação, gameplay ou aprovação humana global.

- Fragmento final: `outputs/redesign-armas-20261025-retomada1850/katana-normal/katana-normal-leve2-pages.json`.
- Jobs originais de publicação, com `columns`, `sourceRows`, `sourceIndices` e `supportAnchors`: `ArtSources/WeaponRedesign20261023/import-jobs-katana-1850.json`.
- Fontes: `ArtSources/WeaponRedesign20261025Retomada1850/katana-normal/`.
- Hashes, estados, limitações e resultados: `outputs/redesign-armas-20261025-retomada1850/katana-normal/handoff-summary.json`.
- Referências integrais inspecionadas e hashes: `reference-contract.json`; prompts e12 chamadas integradas em `generation-history.json`.

Não houve alteração de catálogo compartilhado, importador, runtime, Idle/Leve1 aprovados, Unity/Play, áudio, saves ou referências privadas. Nenhuma caminhada SW/W rejeitada foi reutilizada. Transformações e Lucius/O Encapuzado não foram ampliados.

## Consumidor real

`Assets/Darklight/Data/Attacks/Leve2.asset` define startup0,065s, active0,08s, recovery0,15s. `TodayCharacterCatalog.cs` usa, no fallback Attack de quatro fases, antecipação → contato → primeira metade da recuperação → segunda metade. A prévia conserva **65/80/75/75ms**, total295ms. O consumidor mantém sua autoridade de contato; estes arquivos não criam outro relógio de dano.

Os `sourceSpriteNames` foram lidos do catálogo real em vez de deduzidos pelo nome da skin. Réquiem usa `Today_RequiemAttackKatanaYoungCardinal/Diagonal`; Rock usa **`Today_RockAttackKatanaYoung_{direction*4+phase}`**, sem sufixos Cardinal/Diagonal. A chave Leve2 diferencia o golpe novo dos quadros Leve1 existentes. A coordenação integra e verifica `ConfirmHit`/transições separadamente.

## Identidade e direções

Réquiem e Rock mantêm rosto, idade, cabeça, corte castanho curto, corpo, pele limpa e olhos vermelho-escuros canônicos; roupa e acessórios mudam. Normal mantém katana de aço neutro com cabo escuro/prateado, sem energia de transformação. O desenho de Leve2 é uma subida reversa: câmara baixa, corte ascendente, seguimento alto e guarda recuperada; não é a reutilização filtrada do primeiro golpe.

| Direção | Conferência física |
|---|---|
| S | Face/peito/quadril frontais coerentes; braço direito empunha, esquerdo equilibra vazio. |
| SW | Base frente/esquerda própria, distinta da frontal; giro de contato não substitui a base diagonal. |
| W | Perfil esquerdo com um olho; braço esquerdo próximo vazio, katana no braço direito distante. Rock W0 final usa reparo v3. |
| NW | Nuca e costas/esquerda, orelha/lateral mínima, sem face frontal sobre costas. |
| N | Nuca e costas completas, sem rosto/olhos; braço direito e katana à direita da tela. |
| NE | Nuca e costas/direita, cabeça/pescoço/ombros/tronco/quadril concordam. |
| E | Perfil direito, braço direito próximo empunha; braço esquerdo distante vazio. |
| SE | Base frente/direita; braço direito parte do ombro à esquerda do observador e pode cruzar o peito; esquerdo permanece vazio. |

Corrente permanece no quadril anatômico esquerdo, com oclusão natural do sobretudo. Não houve espelhamento para preencher direções, transplante de rosto ou montagem anatômica.

## Reparos seletivos e preservação

**56 figuras originais corretas permanecem RGBA idênticas; oito figuras completas foram substituídas.** Quatro fontes técnicas finais registram quadros inteiros, sem escalar, recolorir, espelhar, deformar ou montar partes. Cada seleção possui arquivo `*-selection-provenance.json` com fonte/hash, grade original, índice, retângulo do quadro inteiro e hash RGBA escolhido.

- Réquiem NW/N/NE: substituição apenas de0/4/8, antecipações que empunhavam indevidamente pela esquerda; preservadas nove.
- Réquiem E/SE: substituição apenas de5/6/7, fases SE com braço de empunhadura trocado; preservadas cinco.
- Rock E/SE: substituição apenas de2, seguimento E; preservadas sete.
- Rock S/SW/W: substituição apenas de8, antecipação W; preservadas onze. O reparo v2 ainda deixava o braço próximo ambíguo; v3 usa o W canônico, com antebraço esquerdo próximo vazio descendo diante do corpo e mão direita distante empunhando.

Uma folha mista de reparo produziu tamanhos incompatíveis e contaminou uma manga de Réquiem com braço/roupa Rock. Foi retida em `rejected/MixedLeve2HandRepair-v1-source.png`; **nenhum pixel dela foi selecionado**. Rock W0 v2 ficou superado e também não fornece pixels finais. Fontes mistas/parciais, rejeitada, superada e `preselection/` são preservadas localmente e excluídas da publicação. Publicar apenas as seis fontes efetivamente apontadas pelos jobs finais, atlas, manifestos, fragmento e provas pertinentes.

## Escala e QA

Um PPU por folha, medido pela coroa→sola na guarda recuperada comparável, excluindo arma, rastro e cauda do casaco. Âncoras de apoio foram anotadas nas botas. Medidas aproximadas têm incerteza de5–12pixels; segmentos internos sob roupa têm10–18pixels. Inclinação, agachamento e projeção mudam a altura aparente sem autorizar deformação por quadro.

`katana-normal-leve2-world-scale-review.png` compara32 figuras (Idle0 canônico e Leve2 guarda3, ambas as skins/oito direções), usando o PPU efetivo e **WorldVisualScale=1,325**, a112 pixels por unidade real. Painéis separados `requiem-leve2-world-scale-review.png` e `rock-leve2-world-scale-review.png`; medidas em JSON. Nenhuma miniatura é ampliada individualmente para caber na célula. A inspeção não apontou versão infantil ou crescimento do cabelo; não é certificação de igualdade anatômica exata ou aprovação humana.

Auditoria estrutural: **zero erros;77 avisos**, sendo64 de paleta acima de256cores, sete de altura aparente de pose/arma e seis de alpha parcial. Todos preservados para revisão; não houve quantização destrutiva, limpeza automática da pele/olhos ou remoção global de cor/alpha. Raster grande e muitos tons/alpha parcial não comprovam pixel art nativa indexada. Igualdade fonte→atlas RGBA confirmada.

Prévia silenciosa Chrome: **16 clipes,64 quadros**, controles de quadro/passo, zoom1×, fundos claro/escuro e avanço de reprodução confirmados em `browser-review/results.json`. O último reparo revalidou apenas o atlas Rock S/SW/W afetado (três clipes/doze quadros); os demais resultados válidos foram preservados. A primeira abertura oculta do Edge falhou com `browserType.launch: Target page, context or browser has been closed`; log preservado, sem inferir sucesso dessa tentativa. Contatos, GIFs, paletas e HTML em `qa/`; capturas1× em `browser-review/`.

## Próximos passos reais

Coordenação: integrar fragmento e seis manifestos/atlas finais **preservando PPU, pivôs e metadados manuais**, conferir nomes reais/Leve2, testar contato e retorno ao Idle no consumidor, registrar os estados de importação e runtime. Não refazer escala automaticamente pela bounding box da arma. Este responsável encerrou gerações após o lote Leve2 Normal; outros ataques/combos, locomoção, esquiva/defesa/dano/morte/transições continuam na matriz real. Prioridade: terminar Normal de Réquiem/Rock em todas as armas/ações, depois transformações, depois Lucius.
