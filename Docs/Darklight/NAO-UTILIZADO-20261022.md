# Inventário de uso — 22/10/2026

Análise estática, sem apagar arquivos e sem iniciar o jogo. GUIDs, cenas habilitadas e caminhos de recursos foram confrontados. Caminhos construídos em código impedem certificar desuso só pela ausência de uma referência.

| Classificação | Arquivos | MiB |
|---|---:|---:|
| candidato-sem-referencia | 48 | 39.7 |
| dependencia-de-recurso-empacotado | 5 | 3.1 |
| editor-ou-testes | 1 | 0.0 |
| ligado-a-cena-ou-configuracao | 81 | 117.1 |
| recurso-com-carregamento-dinamico | 927 | 842.2 |
| recurso-sem-uso-confirmado | 76 | 55.4 |
| referencia-indireta-nao-alcancada | 1 | 0.0 |
| referenciado-por-ferramenta | 530 | 154.8 |

## Legado substituído com condição explícita

- `DarklightChainView` desativa as 17 partes da corrente quando `DarklightRuinFistsBank.Ready` é verdadeiro. As manoplas e grevas vêm dos quadros completos dos Punhos.
- `LacoDaRuptura.asset` conserva o GUID e o índice 2 dos saves; o perfil da sessão recebe os ataques dos Punhos. Os ataques `Laco*` ficam como fonte legada/fallback.
- As antigas folhas do terceiro estilo permanecem nos catálogos de recuperação, mas o catálogo completo dos Punhos tem prioridade. Não foram removidas automaticamente.

## Arquivos de trabalho fora do jogo

Há 31 fontes geradas dos Punhos que não integram a seleção final. Incluem estudos, propostas superadas e guias; o JSON registra os caminhos. Fontes aprovadas e guias usados foram preservados.

`outputs/` contém prévias e auditorias; `ArtSources/` preserva originais e referências. Esses diretórios não são carregados diretamente pelo Unity.

## Maiores candidatos sem ligação confirmada

- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRequiemStage1.png` — 2.81 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRockStage4.png` — 2.59 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRequiemStage4.png` — 2.52 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkKatanaYoung_White.png` — 2.37 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkChainYoungDiagonal_White.png` — 2.28 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRockStage3.png` — 2.28 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemCombatKatanaYoungCardinal_White.png` — 2.21 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkChainYoungCardinal_White.png` — 2.12 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRequiemStage2.png` — 2.12 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRockStage0.png` — 2.08 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRockStage2.png` — 2.07 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/ArtSources/UserWholeSprites20261012/requiemfoice1.png` — 2.05 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkDaggersYoungDiagonal_White.png` — 2.04 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRequiemStage3.png` — 2.04 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkKatanaYoung_Violet.png` — 1.98 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/Resources/Darklight/AnimationCatalog/UserCharacterSheet.png` — 1.97 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/ArtSources/UserWholeSprites20261012/rockfoice1.png` — 1.96 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/ArtSources/UserAdaptation20261012/DraftCatalog/UserRequiemStage0.png` — 1.95 MiB; candidato-sem-referencia. no-serialized-reference-found; code-generated paths possible.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkDaggersYoungCardinal_White.png` — 1.95 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.
- `Assets/Darklight/Resources/Darklight/TransformedYoung/RequiemWalkDaggersYoungCardinal_Violet.png` — 1.94 MiB; recurso-sem-uso-confirmado. packaged-by-Resources; dynamic usage unresolved.

Lista completa: `outputs/preview-darklight-20261022/reports/unused-candidates.csv` e `unused-inventory.json`. Não há autorização inferida para excluir qualquer candidato.
