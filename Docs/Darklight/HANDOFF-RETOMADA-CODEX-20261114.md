# Handoff e Auditoria Completa de Animações — Retomada Codex (14/11/2026)

**Data do registro:** 14/11/2026 ~18:15 BRT  
**Para:** Codex (retomada pós-limite de cota às 19:42 / ~1h44) e Usuário  
**Status do Projeto:** Baseline intacto, espaço em disco restabelecido (13,86 GiB livres em C:), build executável funcional pré-existente em `outputs/build-windows-gothic-20261114-serafim-saws/Windows/`.

---

## 1. Orientação Expressa do Usuário Nesta Rodada

1. **Preservar candidatas geradas:** As quatro folhas geradas na sessão pelo usuário foram salvas como candidatas em `ArtSources/Candidates20261114/` com manifesto JSON explicativo.
2. **Suspender builds e testes nesta etapa:** Não recompilar nova build nem rodar suítes pesadas agora para economizar recursos e tempo até a retomada do Codex.
3. **Auditoria aprofundada:** Realizada a varredura minuciosa de todo o sistema de animações, catálogos (`catalog.json`), controladores (`DarklightCharacterView.cs`, `DarklightFabiusFormsBank.cs`) e suítes de teste (`DarklightFabiusFormsTests.cs`).
4. **Handoff estruturado:** Deixar o plano exato, lacunas documentadas e os prompts cirúrgicos prontos para o Codex finalizar o fatiamento, zerar as falhas de teste e consolidar a entrega.

---

## 2. Candidatas Salvas em `ArtSources/Candidates20261114/`

| Arquivo Candidato | Forma | Arma | Ação | Dimensões / Grade | Situação Atual |
|---|---|---|---|---|---|
| `Candidate_RupturaFistsWalk_20261114.png` | Ruptura | Punhos | Walk | 767×1024 (4L × 3C, células ~256×256) | **Alpha já isolado (70,37% transparente)**. Pronta para corte e calibração de pivô. |
| `Candidate_SerafimFistsWalk_20261114.jpg` | Serafim | Punhos | Walk | 768×1024 (4L × 3C, células 256×256) | Fundo cinza texturizado. Requer remoção de fundo/máscara antes de integrar. |
| `Candidate_SawsWalk_RupturaSerafim_20261114.jpg` | Ruptura / Serafim | Serras | Walk | 768×1024 (4L × 3C, células 256×256) | Fundo preto puro (`[0,0,0]`). Requer remoção de fundo com preservação de pretos da roupa. |
| `Candidate_SerafimSawsAttackCross_20261114.jpg` | Serafim | Serras | MotoToSaws | 768×1024 (4L × 3C, células 256×256) | Fundo branco (`[254,254,254]`). Requer remoção de fundo branco. |

*Manifesto detalhado:* `ArtSources/Candidates20261114/candidates-manifest.json`.

---

## 3. Matriz Completa de Auditoria de Animações (O Que Existe vs O Que Falta)

### 3.1. Fabius Normal (Skin 0)
- **Status:** **100% COMPLETO**. 698 arquivos de assets preservados sem nenhuma alteração de bytes. Todas as armas (Katana, Punhos, Moto) e 8 direções funcionais.

### 3.2. Lucius Normal e Lucius Arconte (Forma 3)
- **Status:** **100% COMPLETO**. 70 páginas catalogadas em `LuciusNormal20261109` e 39 páginas em `LuciusArconte20261111`. Testes EditMode e PlayMode verdes.

### 3.3. Transições Oficiais de Transformação
- **Status:** **100% COMPLETO**. 3 sequências temporais de 24 quadros (`OfficialTransitions20261114`), 3 AnimationClips bound a `DarklightOfficialTransitionBank`.

### 3.4. Serafim Carmesim (Forma 2) — Diagnóstico Detalhado

| Ação / Mecânica | Arma | Estado Atual no Catálogo | Cobertura Direcional | O Que Falta / Problema |
|---|---|---|---|---|
| **Caminhada (Walk)** | Katana (0) | `FabiusSerafimKatanaWalkEN20261114` e `...WS20261114` | E, NE, SE, W, NW, SW (+ N/S em `WalkNSV1`) | **Completo**. 8 direções funcionais. |
| **Caminhada (Walk)** | Serras (3) | `FabiusSerafimSawsWalk20261114` e `...WS20261114` | E, NE, SE, W, NW, SW (+ N/S fallback) | **Completo**. Funcional a pé. |
| **Caminhada (Walk)** | **Punhos (2)** | **NENHUMA ENTRADA NO CATÁLOGO** | **0%** | **FALTANTE CRÍTICO:** Ao equipar Punhos da Ruína, o jogo cai no fallback da Katana Walk (bug visto na print do usuário). *Candidata disponível:* `Candidate_SerafimFistsWalk_20261114.jpg`. |
| **Ataque (Attack)** | Katana (0) | `Session14SerafimKatanaFront` e `Back` | 8 direções completas | **Completo**. |
| **Ataque (Attack)** | Serras (3) | `Session14SerafimSawsFront` e `Back` | 8 direções completas | **Completo**. |
| **Ataque (Attack)** | **Punhos (2)** | Apenas `Session14SerafimFistsBack` | **Apenas 4 direções (N, NW, SW, W)** | **INCOMPLETO:** Falta `Session14SerafimFistsFront` com as 4 direções restantes (S, SE, E, NE). |
| **Guarda / Parry** | Todas | `FabiusSerafimGuardSE20261112` | Apenas SE (Sul-Leste) | Falta guarda lateral/frontal específica. |
| **Dano / Morte** | Todas | `FabiusSerafimHurtSE20261112`, `DeadSE`, `HurtKnockdownV1` | SE e E | Poses temporais funcionais, falta 8D. |
| **Voo / Esquiva** | N/A | `FlyGlideV1`, `Fly8DV1` | Apenas E | Funcional com espelhamento. |

### 3.5. Ruptura (Forma 1) — Diagnóstico Detalhado

| Ação / Mecânica | Arma | Estado Atual no Catálogo | Cobertura Direcional | O Que Falta / Problema |
|---|---|---|---|---|
| **Ataque (Attack)** | Katana (0) | `Session14RupturaKatanaFront` e `Back` | 8 direções completas | **Completo**. |
| **Ataque (Attack)** | Punhos (2) | `Session14RupturaFistsFront` e `Back` | 8 direções completas | **Completo**. |
| **Ataque (Attack)** | Serras (3) | `Session14RupturaSawsFront` e `Back` | 8 direções completas | **Completo**. |
| **Caminhada (Walk)** | Katana (0) | `FabiusRupturaWalkV1`, `WalkNV1`, `WalkWV1`, `WalkSV1`, etc. | 8 direções base | Funcional (caminhada base compartilhada). |
| **Caminhada (Walk)** | **Punhos (2)** | **NENHUMA ENTRADA NO CATÁLOGO** | **0%** | **FALTANTE HISTÓRICO:** Causa **6 das 7 falhas** no teste `RupturaLocomotionSwitchesToTheRequestedWeaponAndAuthoredDirection` (`DiagS`, `Lateral`, `DiagN`). O teste cobra prefixos `FabiusRupturaFistsWalk{view}20261113_`. *Candidata disponível:* `Candidate_RupturaFistsWalk_20261114.png`. |
| **Caminhada (Walk)** | **Serras (3)** | **NENHUMA ENTRADA NO CATÁLOGO** | **0%** | **FALTANTE HISTÓRICO:** Causa **1 das 7 falhas** no teste `GuardRetainsBasePoseAndSawsUseTheTransformedLocomotion`. O teste cobra prefixo `FabiusRupturaSawsWalkLateral20261113_`. *Candidata disponível:* `Candidate_SawsWalk_RupturaSerafim_20261114.jpg`. |
| **Guarda / Parry** | Todas | `FabiusRupturaGuardNS13` | Apenas N e S | Falta guarda lateral/diagonal. |
| **Dano / Morte** | Todas | `FabiusRupturaHurtKnockdownV1` | Apenas E | Poses temporais funcionais. |
| **Esquiva** | N/A | `FabiusRupturaActivationDodgeV1` | Apenas E | Funcional. |

---

## 4. O Que o Codex Precisa Fazer na Retomada (Passo a Passo)

### Passo 1: Processar e Fatiar as Candidatas Prioritárias
1. **`Candidate_RupturaFistsWalk_20261114.png`:**
   - Já possui canal alpha transparente.
   - Fatiar em 12 sprites (4 linhas × 3 colunas, células ~256×256).
   - Gerar as páginas no catálogo com nomes alinhados ao teste:
     - Linha 0 (E) e Linha 3 (W) -> `FabiusRupturaFistsWalkLateral20261113`
     - Linha 2 (SE) e Linha 3 espelhada (SW) -> `FabiusRupturaFistsWalkDiagS20261113`
     - Linha 1 (NE) e Linha 1 espelhada (NW) -> `FabiusRupturaFistsWalkDiagN20261113`
   - *Ao registrar isso, as 6 falhas históricas de EditMode serão ZERADAS.*
2. **`Candidate_SawsWalk_RupturaSerafim_20261114.jpg`:**
   - Isolar fundo preto preservando o preto das roupas/manto.
   - Fatiar em células 256×256 e registrar:
     - Para Ruptura: `FabiusRupturaSawsWalkLateral20261113` (Zera a 7ª e última falha histórica de EditMode!).
3. **`Candidate_SerafimFistsWalk_20261114.jpg`:**
   - Isolar fundo cinza.
   - Fatiar em células 256×256 e registrar no catálogo como:
     - `form: "Serafim"`, `action: "Walk"`, `weapon: "Fists"`.
   - *Isso resolve definitivamente a consistência visual no jogo ao andar com os Punhos da Ruína equipados.*

### Passo 2: Gerar a Única Arte de Ataque Restante (Opcional se desejar 100% de Ataques 8D)
- Gerar `Session14SerafimFistsFront` cobrindo as 4 direções frontais de ataque (S, SE, E, NE) usando o Prompt do item 5 abaixo.

### Passo 3: Validação da Suíte de Testes
Executar:
```powershell
python Tools/Testing/run_session_20261114.py EditMode --filter Darklight.Tests.DarklightFabiusFormsTests --unrestricted-memory
```
**Meta esperada:** **28/28 PASSED (0 FAILURES)**. Pela primeira vez na história do projeto, a suíte de formas de Fabius ficará 100% verde!

### Passo 4: Compilar e Entregar a Build Consolidada
Executar o comando oficial do projeto em pasta limpa:
```powershell
python Tools/Testing/run_session_20261114.py build --unrestricted-memory --build-output outputs/build-windows-gothic-20261114-consolidated/Windows
```

---

## 5. Prompts Prontos para Geração Adicional

### Prompt para `Session14SerafimFistsFront` (Ataque 8D Frontal Faltante de Punhos Serafim)
```text
Identity-preserve 2D pixel art combat sprite sheet for DARKLIGHT: REQUIEM.
Using the approved Serafim Carmesim references (black leather buckled coat, red hair streak, red eyes, crimson wings):
Draw 12 COMPLETE figures in 4 rows by 3 columns performing COMBAT PUNCH AND STRIKE COMBOS WITH SPIKED BRAWLER GAUNTLETS (Punhos da Ruína).
NO SWORDS, NO WEAPONS. Clenched glowing demonic gauntlets.
- Col 1: Windup / anticipation guard
- Col 2: Active punch / cross impact with red spark burst
- Col 3: Follow-through and recovery
DIRECTIONS:
- ROW 1: SOUTH (Frontal view facing player)
- ROW 2: SOUTHEAST (Diagonal front-right)
- ROW 3: EAST (Right profile punch)
- ROW 4: NORTHEAST (Diagonal back-right strike)
True alpha transparent background, 256x256 uniform cells, 2D RPG top-down pixel clusters.
```
