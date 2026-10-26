# AUDITORIA DE FONTES ARTISTICAS CANONICAS — 26/10/2026
**DARKLIGHT: REQUIEM — Etapa B (Antigravity)**

## Objetivo
Mapear detalhadamente todas as fontes artisticas canonicas de **Katana**, **Punhos da Ruina** e **Moto (AbyssRider)** disponiveis no clone ate 25/10/2026, identificando com precisao quais ativos estao validados para reuso seguro, quais sao parciais e quais dependem de definicao ou consolidacao posterior pelo Codex.

---

## 1. KATANA (Arma 0 — Canonica Primaria)

### 1.1 Fontes Brutas Disponiveis em `ArtSources/`
- **`ArtSources/WeaponRedesign20261023/`**:
  - `references/equipment-four-forms.png` (1946241 bytes): Prancha de referencia canonica das 4 formas de equipamento.
  - `references/player-equipment-reference.jpeg` (344327 bytes): Referencia de silhueta e proporcao de empunhadura.
  - `references/approved-player-eyes.png` (33212 bytes): Padrao aprovado de expressao ocular.
  - `import-jobs-katana-normal-20261025.json`, `import-jobs-katana-1850.json`.
- **`ArtSources/WeaponRedesign20261024/`**:
  - `candidates/import-jobs-katana-walkS-v5.json`: Job de caminhada Sul v5.
  - `candidates/ReqKatanaNormalLight1_S_SW_W-v1-manifest.json`: Manifestos das direcoes Sul/Sudoeste/Oeste.
- **`ArtSources/WeaponRedesign20261025/candidates/`**:
  - `ReqKatanaNormalNeutralIdleE_SE20261025-source.png` (1294902 bytes, 1536x1024 RGBA)
  - `ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png` (2209994 bytes, 1536x1024 RGBA)
  - `ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png` (2205354 bytes, 1536x1024 RGBA)
  - `ReqKatanaNormalNeutralLeve1E_SE20261025-source.png` (1364411 bytes, 1536x1024 RGBA)
  - `ReqKatanaNormalNeutralLeve1NE20261025-source.png` (863129 bytes, 1536x1024 RGBA)
- **`ArtSources/WeaponRedesign20261025Retomada1850/katana-normal/`**:
  - `ReqKatanaNormalLeve2E_SE-selected-whole-figures-source.png` (1440303 bytes)
  - `ReqKatanaNormalLeve2NW_N_NE-selected-whole-figures-source.png` (1681366 bytes)
  - `ReqKatanaNormalLeve2S_SW_W-v1-source.png` (2216797 bytes)

### 1.2 Recursos Importados em `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`
- **Requiem / Normal (Forma 0)**:
  - **Idle (8 direcoes)**: Cobertura total via `ReqKatanaNormalNeutralIdleS_SW_W20261025`, `ReqKatanaNormalNeutralIdleNW_N_NE20261025`, `ReqKatanaNormalNeutralIdleE_SE20261025`.
  - **Ataque Leve 1 (8 direcoes)**: Cobertura total via `ReqKatanaNormalNeutralLeve1S_SW_W20261025`, `ReqKatanaNormalNeutralLeve1NW_N20261025`, `ReqKatanaNormalNeutralLeve1NE20261025`, `ReqKatanaNormalNeutralLeve1E_SE20261025`.
  - **Ataque Leve 2 (8 direcoes)**: Cobertura total via `ReqKatanaNormalLeve2S_SW_W1850`, `ReqKatanaNormalLeve2NW_N_NE1850`, `ReqKatanaNormalLeve2E_SE1850`.
  - **Walk**: Cobertura parcial (somente direcao Sul via `ReqKatanaNormalNeutralWalkS20261024V5.png`). Demais direcoes usam fallback estavel do `TodayCharacterBank`.
- **Requiem / Formas Transformadas (Formas 1, 2, 3)**:
  - `ReqKatanaRupturaSouth.png` (Forma 1 — apenas Sul)
  - `ReqKatanaEclipseSouth.png` (Forma 2 — apenas Sul)
  - `ReqKatanaAbsolutoSouth.png` (Forma 3 antiga — apenas Sul)
  - **Lacuna critica**: Nao existem pranchas multidirecionais (E, SE, NE, N, NW, W, SW) para Katana nas formas transformadas no clone. O sistema recorre ao fallback de paleta/legado.

### 1.3 Veredito de Reuso — Katana
| Item | Status | Pode reusar com seguranca? |
|---|---|---|
| Katana Normal Requiem (Idle 8 dir) | Validado e importado | SIM |
| Katana Normal Requiem (Leve 1 e 2, 8 dir) | Validado e importado | SIM |
| Katana Normal Requiem (Walk Sul) | Validado e importado | SIM (outras 7 dir usam fallback) |
| Katana Ruptura/Eclipse/Absoluto (Sul) | Validado e importado | SIM (somente vista Sul) |
| Katana Transformaçoes (outras 7 dir) | Ausente | REQUER fontes canonicas do Codex |

---

## 2. PUNHOS DA RUINA (Arma 2 / Ruin Fists)

### 2.1 Fontes Brutas Disponiveis em `ArtSources/`
- **`ArtSources/RuinFists20261021/`**:
  - `production-imported-20261023.json` e `production-source-index-v2.json`.
  - `references/` (28 arquivos): Pranchas completas de Requiem em 8 direcoes (`Requiem_Normal_S_Whole.png`, `_E_`, `_W_`, `_N_`, `_NE_`, `_NW_`, `_SE_`, `_SW_`), alem de `Requiem_Ruptura_Whole`, `Requiem_Eclipse_Whole`, `Requiem_Absoluto_Whole` e versoes V2 direcionadas de Absoluto.
  - `generated/` (65 arquivos): Bancos de animacao `Combat_BankV4` e `Motion_BankV4` para direcoes E, NE, NW, N, etc.
- **`ArtSources/WeaponRedesign20261024/candidates/`**:
  - `ReqFistsNormalDireto_S_SW_W-v2-source.png` (2300932 bytes, SHA-256 `569470770dc7793b22cc4f3aeb7dded4ea102a8ce647d07338fb7ce1a599f7d6`)
  - `ReqFistsNormalDireto_S_SW_W-v2-job.json`
- **`ArtSources/WeaponRedesign20261025/candidates/`**:
  - `ReqFistsNormalDiretoE_SE-v1-source.png`, `ReqFistsNormalDiretoNW-v4-source.png`
  - `ReqFistsNormalIdleE_SE-v1-source.png`, `ReqFistsNormalIdleN-v2-source.png`, `ReqFistsNormalIdleS_SW_W-v1-source.png`

### 2.2 Recursos Importados em `Assets/Darklight/Resources/Darklight/RuinFists/`
- **154 arquivos PNG** presentes no diretorio `Resources/Darklight/RuinFists/`.
- **`catalog.json`**:
  - `complete: true`
  - **128 paginas mapeadas**:
    - Skin 0 (Requiem) x 4 formas (0=Normal, 1=Ruptura, 2=Eclipse, 3=Absoluto) x 8 direcoes x 2 bancos (Combat e Motion) = 64 paginas.
    - Skin 1 (Rock) x 4 formas x 8 direcoes x 2 bancos = 64 paginas.
  - Cada pagina contem 12 sprites point-sampled (RGBA32, PPU=110.5527f) e mascara de paleta descompactada (`_Owner.png`).
  - As formas 1 (Ruptura) e 2 (Eclipse) sao tratadas via `paletteVariant: true` (reusam a geometria da forma 0 com canal de mascara), enquanto a forma 3 (Absoluto) possui prancha e silhueta autorais completas independentes (`paletteVariant: false`).

### 2.3 Veredito de Reuso — Punhos da Ruina
| Item | Status | Pode reusar com seguranca? |
|---|---|---|
| Catalogo RuinFists (128 paginas) | Validado e verificado por testes de integracao | SIM |
| Requiem Combat & Motion (8 direcoes, 4 formas) | 100% presente em Resources | SIM |
| Mascaras de paleta de Ruptura e Eclipse | Point-filtered, mipmap=1 | SIM |
| Pranchas Absoluto autoral | Independentes | SIM |

---

## 3. MOTO / ABYSSRIDER (Arma 3)

### 3.1 Fontes Brutas Disponiveis em `ArtSources/`
- **`ArtSources/WeaponRedesign20261023/`**:
  - `RockMotoForm0Ride8SlamS20261025V2-source.png` (1536x1024 RGBA, 2635225 bytes)
  - `RockMotoForm1Ride8SlamS20261025V2-source.png` (1536x1024 RGBA)
  - `RockMotoForm2-RockMotorcycleEclipseRide8SlamS-v1-source.png` (1536x1024 RGBA)
  - `RockMotoForm3-RockMotorcycleAbsolutoRide8SlamS-v1-source.png` (1536x1024 RGBA)
- **`ArtSources/WeaponRedesign20261024/candidates/`**:
  - `RockMotorcycleNormalRedesign_Ride8_SlamS-v2-source.png` (2300932 bytes)
- **`ArtSources/WeaponRedesign20261025/references/`**:
  - `fabius-transformations-motorcycle.png` (1448x1086 RGBA, 2590474 bytes): Referencia visual de Fabius na moto.
- **`ArtSources/WeaponRedesign20261025/candidates/`**:
  - Fontes de moto preparadas em 25/10 documentadas em `Docs/Darklight/MOTO-*-PREPARO-20261025.md`.

### 3.2 Recursos Importados em `Assets/Darklight/Resources/`
- **`Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`**:
  - `RockMotoForm0Ride8SlamS20261025V2.png` + `.json` (8 direcoes de pilotagem + 4 fases de GroundSlam)
  - `RockMotoForm1Ride8SlamS20261025V2.png` + `.json`
  - `RockMotoForm2Ride8SlamS20261025V2.png` + `.json`
  - `RockMotoForm3Ride8SlamS20261025V2.png` + `.json`
- **`Assets/Darklight/Resources/Darklight/AbyssPrototype/`**:
  - 24 PNGs cobrindo as acoes de Join, AttackSix, Dismount, Slam, Finisher para as formas 0..3 (todos usando a silhueta prototipo).

### 3.3 A BARREIRA DE REQUIEM NA MOTO
- **Constatacao critica de auditoria**:
  - **TODAS as pranchas importadas e candidatas de moto existentes ate 25/10 sao exclusivamente da skin Rock** (`RockMotoForm*`, `RockMotorcycle*`).
  - Nao existe **nenhuma prancha autoral completa de moto com a figura de Requiem** no clone.
  - As figuras sao "figuras completas piloto+moto" (`whole rider/bike figures`), de forma que o piloto nao e uma camada destacavel separada da moto nos sprites de redesign.
  - Portanto, a adaptacao da Moto para Requiem depende ou de novas pranchas desenhadas para Requiem, ou de remap de paleta/mascara autoral sobre a base existente, respeitando a silhueta de Requiem.

### 3.4 Veredito de Reuso — Moto
| Item | Status | Pode reusar com seguranca? |
|---|---|---|
| Sprites de pilotagem Rock (8 dir, 4 formas) | Validados tecnicamente | SIM (apenas para Rock) |
| GroundSlam Rock (4 fases, Sul) | Validado tecnicamente | SIM (apenas para Rock) |
| Sprites de pilotagem REQUIEM | **INEXISTENTE no clone** | NAO (bloqueia desvinculacao imediata) |
| Prototipos AbyssPrototype (24 paginas) | Validados para testes | SIM (legado/fallback) |

---

## 4. Resumo de Reusabilidade para o Codex
1. **Katana**: O pacote Normal (Idle 8 dir, Leve1 8 dir, Leve2 8 dir) e 100% confiavel e utilizavel. Formas transformadas precisam de expansao multidirecional alem da vista Sul.
2. **Punhos da Ruina**: Sistema mais maduro do projeto. Catalogo de 128 paginas completo e funcional.
3. **Moto**: O motor funcional e a logica estao prontos para 8 direcoes e 4 fases de impacto, mas o acervo grafico ativo e 100% atrelado a Rock. Para suportar Requiem, e mandatorio obter as fontes canonicas de Requiem piloto.
