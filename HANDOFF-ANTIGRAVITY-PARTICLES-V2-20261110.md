# DARKLIGHT: REQUIEM — Relatório Técnico de Integração VFX 2.0
## Atualização Visual de Partículas de Combate (56 Quadros Canônicos)

**Data:** 10/11/2026  
**Engenharia / VFX:** Antigravity  
**Repositório:** `https://github.com/fwbiodev7/darklight-requiem-game`  
**Branch:** `codex/retomada-fabius-20261109` & `antigravity/lucius-integration-delivery-20261110`  
**Status:** **100% CONCLUÍDO E APROVADO**

---

### 1. Resumo Executivo
Conforme autorização formal do usuário na etapa final do redesenho de partículas, foi realizada a extração determinística, processamento de transparência alfa real RGBA, importação no Unity com filtragem *Point (no filter)*, vinculação nos sistemas de combate e compilação do executável Windows x64.

Nenhuma arte foi redesenhada por IA ou reprocessada com filtros borrados; a imagem mestre aprovada (`Darklight_Combat_Particles_v2_Master.jpg`) foi aproveitada integralmente com máxima fidelidade aos pixels originais. Os 522 pivôs anatômicos de Fabius Normal permaneceram intocados.

---

### 2. Assets Integrados (7 Famílias / 56 Quadros)

Localização no projeto Unity: `Assets/Darklight/Resources/Darklight/CombatParticlesV2/`

| Família VFX | Resolução Base | Frames | Pivô Unity (X, Y) | Comportamento na Gameplay |
| :--- | :--- | :--- | :--- | :--- |
| **Contato / Impacto** | 32x32 / 48x48 / 64x64 | 01 a 08 | `(0.50, 0.50)` | Flash estelar imediato no ponto do impacto confirmado |
| **Ataque Katana** | 64x64 / 96x96 / 128x128 | 01 a 08 | `(0.50, 0.50)` | Arco de corte crescente branco-azulado acompanhando a lâmina |
| **Ataque Punhos** | 40x40 / 56x56 / 72x72 | 01 a 08 | `(0.50, 0.50)` | Explosão seca, bruta e concentrada no local do soco/chute |
| **Ataque Moto** | 96x64 / 128x96 / 160x128 | 01 a 08 | `(0.50, 0.20)` | Fumaça blocada e faíscas de atrito com o chão na arrancada/golpe |
| **Esquiva Katana** | 64x32 / 96x48 / 128x64 | 01 a 08 | `(0.50, 0.50)` | Rastro fino e ágil de corte de vento azul-prateado atrás do corpo |
| **Esquiva Punhos** | 64x32 / 96x48 / 128x64 | 01 a 08 | `(0.50, 0.25)` | Deslocamento físico com poeira e pedregulhos no nível do solo |
| **Esquiva Moto** | 96x48 / 128x64 / 160x80 | 01 a 08 | `(0.50, 0.20)` | Derrapagem violenta com faíscas e queima de pneu sem cobrir o piloto |

#### Arquivos Entregues em Resources:
- `Sheet_Contato_Impacto_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `Sheet_Ataque_Katana_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `Sheet_Ataque_Punhos_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `Sheet_Ataque_Moto_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `Sheet_Esquiva_Katana_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `Sheet_Esquiva_Punhos_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `Sheet_Esquiva_Moto_v2.png` (+ `.meta` multi-sprite fatiado em 8 quadros)
- `CombatParticlesV2_Unified_Master.png` (+ `.meta`)
- `Frames/` (Todos os 56 quadros individuais exportados como PNG RGBA puro com metadados próprios)
- `CombatParticlesV2.json` (Manifesto estruturado com durações de 35ms a 45ms por frame, escalas e âncoras)

---

### 3. Sistemas de Gameplay Atualizados

1. **`DarklightCombatParticlesV2.cs` (Novo Sistema Central):**
   - Pool de renderizadores de sprite de zero GC com reciclagem automática.
   - Sincronização temporal frame a frame em delta time não escalado.
   - Posicionamento e ordenação de profundidade vinculados à base física via `DarklightSorting.OrderForY`.
   - Orientação vetorial conforme o `Facing` do personagem.
2. **`DarklightPixelContactBurst.cs`:**
   - Vinculado para disparar `DarklightCombatParticlesV2.TryEmitContact` nos acertos confirmados de Fabius e inimigos, preservando a retrocompatibilidade com o burst legado.
3. **`DarklightActionParticles.cs`:**
   - Vinculado para disparar as esquivas personalizadas por arma (`EsquivaKatana`, `EsquivaPunhos`, `EsquivaMoto`) a cada transição para o estado `CombatState.Dodge`.
4. **`DarklightCharacterView.cs`:**
   - Vinculado para instanciar os efeitos de corte e impacto no momento do golpe ativo (`CombatState.Attack`), acompanhando o vetor de ataque.
5. **Preservação Obrigatória Garantida:**
   - Nenhum dos 522 pivôs recalibrados de Fabius foi alterado.
   - Nenhuma animação ou hitbox foi alterada.
   - O combate Fabius vs Lucius e a estabilidade de locomoção permanecem 100% íntegros.

---

### 4. Validação e Testes Automatizados (136/136 Aprovados)

Executados de forma headless via Unity Test Runner (`-batchmode -nographics`):

| Suíte de Testes | Plataforma | Filtro de Execução | Casos | Status |
| :--- | :--- | :--- | :--- | :--- |
| **VFX2_Tests** | EditMode | `Darklight.Tests.DarklightCombatParticlesV2Tests` | 4/4 | **PASSED (100%)** |
| **Fabius_Stability_Tests** | EditMode | `Darklight.Tests.DarklightFabiusAnimationStabilityTests` | 3/3 | **PASSED (100%)** |
| **Lucius_Combat_Tests** | EditMode | `Darklight.Tests.DarklightLucius*` (5 classes) | 54/54 | **PASSED (100%)** |
| **Fabius_Combat_Regression** | EditMode | `Recovery, Combat, Motorcycle, WeaponTechnique` | 72/72 | **PASSED (100%)** |
| **PlayMode_Stability_Tests**| PlayMode | `Darklight.Tests.DarklightSpriteRootStabilityTests` | 3/3 | **PASSED (100%)** |
| **TOTAL GERAL** | — | — | **136/136** | **100% APROVADO** |

---

### 5. Compilação e Build Windows x64

- **Pipeline:** Unity 6000.6.0f1 headless batchmode (`DarklightLuciusTestBuild.Build`) com compressão LZ4.
- **Resultado:** `Succeeded` (0 erros, 0 falhas).
- **Tempo de Compilação:** 407.2s.
- **Tamanho dos Dados (`data.unity3d`):** 1.76 GB (1.768,7 MB).
- **Executável:** `outputs/build-windows-vfx2-20261110/Windows/DarklightRequiem.exe`
- **Pacote Distribuível (ZIP):** `outputs/build-windows-vfx2-20261110/DarklightRequiem-Windows-VFX2-20261110.zip` (1.510,7 MB).
- **Teste de Inicialização:** Executado com sucesso em RAM (PID 31508 verificado em execução e encerrado de forma limpa).
- **Launchers Criados na Raiz:**
  - `run-game.bat` (Inicialização direta com 1 clique pelo CMD/Explorer).
  - `run-game.ps1` (Inicialização via PowerShell com diretório de trabalho resolvido).

---

### 6. Como Abrir o Jogo pelo Terminal
Para evitar o erro de `Data folder not found` (que ocorre ao executar o `.exe` fora da pasta onde reside o `DarklightRequiem_Data`), utilize os scripts fornecidos na raiz:

```cmd
:: Pelo Prompt de Comando (CMD) na raiz do projeto:
run-game.bat

:: Ou pelo PowerShell:
.\run-game.ps1
```

---

### 7. Próximos Passos (Para o Codex quando a cota renovar)
1. Dar sequência à integração das animações pendentes de Lucius (`Draw`, `WalkArmed`) utilizando o gabarito oficial em `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png`.
2. As partículas de combate v2.0 já estão ativas e responderão nativamente aos novos ataques assim que conectados.
