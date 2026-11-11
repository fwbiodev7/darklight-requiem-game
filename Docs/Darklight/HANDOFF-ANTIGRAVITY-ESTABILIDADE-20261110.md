# DARKLIGHT: REQUIEM — Relatório de Handoff Técnico
## Operação Estabilidade Visual + Polimento Técnico

**Data:** 10/11/2026  
**Auditor / Engenharia:** Antigravity  
**Repositório:** `https://github.com/fwbiodev7/darklight-requiem-game`  
**Branch:** `codex/retomada-fabius-20261109` & `antigravity/lucius-integration-delivery-20261110`  

---

### 1. Causa Raiz Identificada do Tremor de Fabius

Após inspeção minuciosa dos metadados, scripts de runtime (`DarklightCharacterView.cs`, `DarklightWeaponRedesignBank.cs`) e matrizes de pixels das 108 folhas de sprites de Fabius Normal em `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`, constatou-se que:

1. **Inconsistência Numérica de Pivôs (`pivot.x` e `pivot.y`):**
   - Os sprites PNG desenhados pelo Codex possuem pixels consistentes, maturidade visual aprovada e apoio no solo firme.
   - Contudo, os metadados dos pivôs nos arquivos `.json` e `.png.meta` foram gerados com um heurístico rascunho (`"Manual projected foot-support midpoint in intact full source; Unity calibration pending."`), variando significativamente entre quadros consecutivos da mesma animação.
   - **Exemplo Real no Idle S:** No ciclo de repouso frontal, o pé no desenho permaneceu cravado em `Y = 19` e `X ≈ 263.0`, mas o `pivot.x` saltava entre `264px` (quadro 00), `277px` (quadro 01, +13px para a direita), `272px` (quadro 02) e `256px` (quadro 03, -16px para a esquerda). A cada respiração, o personagem sofria um solavanco de até 21 pixels no espaço de mundo.
   - **Exemplo Crítico no Walk W (Punhos):** Em `ReqFistsNormalWalkW20261102V9Complete`, os quadros 02 e 03 possuíam `pivot.y` configurado com salto de 144 pixels em relação aos quadros 00 e 01, provocando um teletransporte vertical violento durante o passo.
   - **Exemplo nos Ataques Pesados e Cortes Amplos:** Em quadros com arcos de corte largos (com largura de 1300+ pixels para comportar o efeito de lâmina), o pivô foi centrado próximo ao meio do efeito visual em vez de permanecer ancorado na postura/corpo do personagem, fazendo o corpo saltar de 100 a 200 pixels para trás para centrar a lâmina.

---

### 2. Normalização e Correção Realizada

1. **Ferramenta de Calibração Determinística:**
   - Criada e executada a automação `work/fix_fabius_stability.py`, auditando célula a célula os 962 quadros de Fabius Normal.
   - **Idle (todas as 8 direções, Katana, Daggers e Punhos):** Todos os quadros de uma direção compartilham exatamente a mesma coordenada de contato no chão (`pivot.y`) e o mesmo prumo médio da postura (`pivot.x`). A oscilação entre quadros foi reduzida para **0.0 pixels**.
   - **Walk (todas as 8 direções, Katana, Punhos e Moto):** Linha de apoio no solo travada com precisão no plano físico dos pés; eixo horizontal calibrado pelo centro pélvico/torso, eliminando solavancos e descontinuidades bruscas.
   - **Ataques, Parry, Hurt, Dodge, Dead:** Ponto de apoio mantido na base do corpo, permitindo que arcos cinéticos e faíscas se expandam a partir da pose do personagem sem deslocar artificialmente a sua posição no cenário.
2. **Atualização Abrangente de Metadados:**
   - **95 folhas de sprites** atualizadas.
   - **522 quadros** calibrados bit a bit nos arquivos `.json` e em seus respectivos `.png.meta` (preservando GUIDs, IDs internos e fatiamentos do Unity).

---

### 3. Aprimoramentos Técnicos de Lucius

1. **Fallback de Combate Suave:**
   - Em `DarklightLuciusSpriteBank.TryPose`, implementada transição segura para ataques direcionais: se o golpe 3 do combo (`SwordThrust`) for solicitado em uma direção ainda sem arte integrada, o sistema transita automaticamente para `SwordHorizontal` ou `SwordVertical` na mesma direção, mantendo o combate fluido e sem travamentos estáticos em Idle.
2. **Suporte Antecipado para Próximas Entregas do Codex:**
   - O validador do catálogo e as ações reconhecidas de `DarklightLuciusSpriteBank` foram estendidos para incluir nativamente as ações `"Draw"` (saque de espada) e `"WalkArmed"` (caminhada com espada em punho), permitindo integração imediata assim que as folhas forem concluídas.
3. **Novas Referências Canônicas de Ataque Salvas para o Codex:**
   - Conforme orientação do usuário, foram salvas as referências visuais oficiais de ataque de Lucius para uso imediato pelo Codex no reinício da cota:
     - **Referência Primária Oficial:** `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png` (apresenta a sequência de estocada e corte com postura, efeitos luminosos e recuperação canônicos).
     - **Referência Secundária:** `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Secondary_20261110.jpg`.

---

### 4. Testes e Validação de Estabilidade

1. **Novo Teste de Estabilidade Dedicado:**
   - Implementado `DarklightFabiusAnimationStabilityTests.cs` em `Assets/Darklight/Tests/EditMode/`:
     - `FabiusIdleEightDirectionsHaveStablePivots`: **PASSED** (0% jitter em todas as direções).
     - `FabiusWalkCyclesHaveBoundedPivotDelta`: **PASSED** (locomoção contínua e sem teletransportes).
     - `FabiusAttackFramesMaintainValidAnchors`: **PASSED** (âncoras anatômicas preservadas nos golpes).
2. **Testes de Regressão EditMode de Fabius e Combate:**
   - `DarklightFabiusRecoveryTests`, `DarklightCombatTests`, `DarklightMotorcycleRedesignTests`, `DarklightWeaponTechniqueTests`: **72/72 PASSED**.
3. **Testes EditMode de Lucius:**
   - `DarklightLuciusAnimationIntegrationTests`, `DarklightLuciusCombatEncounterTests`, `DarklightLuciusSpriteBankTests`, `DarklightLuciusHoodedTests`, `DarklightLuciusCombatProfileTests`: **54/54 PASSED**.
4. **Testes PlayMode de Estabilidade:**
   - `DarklightSpriteRootStabilityTests`: **3/3 PASSED**.
5. **Total Geral de Testes:** **132 testes executados e 100% aprovados.**

---

### 5. Build Standalone Windows x64

- **Pipeline Utilizado:** Execução batchmode headless (`-nographics`) via `DarklightLuciusTestBuild.Build` com compressão LZ4.
- **Localização dos Executáveis:** `outputs/build-windows-estabilidade-20261110/Windows/DarklightRequiem.exe`
- **Pacote Distribuível:** `outputs/build-windows-estabilidade-20261110/DarklightRequiem-Windows-Estabilidade-20261110.zip` (328 MB)
- **SHA-256 do ZIP:** `EE760B4D2A0D5D522BC0E49632213639D76F9AA2D1461DB6A1D15DEA7A3AA028`

---

### 6. Roteiro Direto para o Codex (Quando a Cota Renovar)

1. **Nova Versão de Ataques de Lucius (Estocada / Combos):**
   - Utilizar como gabarito oficial: `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png`.
   - Gerar e integrar as folhas de ataque de Lucius seguindo as poses e arcos dessa referência canônica.
2. **Caminhada Restante de Lucius:**
   - Concluir as 8 direções de caminhada com espada nas costas e com espada empunhada (`WalkArmed`).
3. **Transição de Saque (`Draw`):**
   - Integrar a folha de saque já mapeada em `DarklightLuciusSpriteBank`.
