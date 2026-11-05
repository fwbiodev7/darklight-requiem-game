# Lista de Subagentes e Responsabilidades — DARKLIGHT: REQUIEM
**Data:** 05/11/2026  
**Status de Registro:** Definidos e Registrados no Ambiente Antigravity  
**Limite Operacional:** Máximo de 3 subagentes simultâneos (Regra Canônica de `AGENTS.md`)  

---

## 1. Quadro de Subagentes Especialistas

Os 4 subagentes foram configurados via `define_subagent` com prompts de sistema estritos e escopos de escrita isolados:

### 1. `cinematic-systems-agent`
* **Nome de Registro:** `cinematic-systems-agent`
* **Missão Principal:** Timeline, PlayableDirectors, faixas de animação/ativação, sincronização de efeitos sonoros e sinais cinematográficos.
* **Tarefas Imediatas:**
  * Estruturar a cutscene de infância: `Assets/Darklight/Cutscenes/ChildhoodTraining.playable` (Fábio e Lucius com espadas de madeira).
  * Montar os clipes de transição de entrada de chefe (`BossEncounter_{Boss}.playable`).
  * Implementar o sistema de Skip Gate seguro com catch-up de estado.
* **Escopo de Escrita Permitido:** `Assets/Darklight/Cutscenes/`, `Assets/Darklight/Runtime/Cutscenes/`, `work/cutscenes/`.

---

### 2. `camera-pixel-perfect-agent`
* **Nome de Registro:** `camera-pixel-perfect-agent`
* **Missão Principal:** Câmera ortográfica 2D, estabilidade sub-pixel, Cinemachine 3.x, tremores de impacto (screen shake) e auditoria URP Pixel Perfect.
* **Tarefas Imediatas:**
  * Manter a estabilidade do controlador de câmera atual (`DarklightCombatLab.Player`), sem regressões.
  * Mapear perfis de trauma e ruído para impactos de Katana, Punhos e Moto.
  * Garantir conformidade com `WorldVisualScale = 1.325` e `PPU = 24`, eliminando qualquer jitter em movimento diagonal.
* **Escopo de Escrita Permitido:** `Assets/Darklight/Runtime/Camera/`, `Assets/Darklight/Editor/Camera/`, `work/camera/`.

---

### 3. `pixel-art-qa-agent`
* **Nome de Registro:** `pixel-art-qa-agent`
* **Missão Principal:** Auditoria de spritesheets, qualidade de pixel art, proporções anatômicas e proteção intransigente do cânone visual.
* **Tarefas Imediatas:**
  * Auditar folhas de animação contra jaggies, banding, pillow shading e ruído de dithering.
  * Garantir a identidade de Fábio: Caçador maduro e adulto (sem infantilização).
  * Verificar a conformidade da íris em vermelho escuro visível (`#C40814` / `#5A0000`) em todas as formas (Normal, Ruptura, Eclipse Absoluto).
  * Validar pigmento de pele padrão `[246, 174, 130]` e figuras completas (sem recortes anatômicos).
* **Escopo de Escrita Permitido:** `ArtSources/`, `Tools/Skills/pixel-art-sprite-sheet-master/`, `work/art-qa/`.

---

### 4. `unity-engineering-agent`
* **Nome de Registro:** `unity-engineering-agent`
* **Missão Principal:** Pacotes UPM, integridade de builds headless, compatibilidade com Unity 6000.6.0f1, execução de testes PlayMode/EditMode e pontes técnicas.
* **Tarefas Imediatas:**
  * Verificar dependências e flags de compilação sem desestabilizar o runtime.
  * Executar rotinas de validação em segundo plano e monitorar consumo de memória.
  * Gerar relatórios técnicos consolidados para o coordenador central.
* **Escopo de Escrita Permitido:** `Tools/Delivery/`, `Tools/Validation/`, `work/engineering/`.

---

## 2. Regras de Orquestração e Concorrência

1. **Limite de 3 Agentes Concorrentes:** No máximo 3 subagentes podem estar executando ao mesmo tempo. Exemplo de alocação recomendada:
   * **Cenário A (Cinemática & Câmera):** `cinematic-systems-agent` + `camera-pixel-perfect-agent` + `unity-engineering-agent`.
   * **Cenário B (Arte & Sprites):** `pixel-art-qa-agent` + `unity-engineering-agent`.
2. **Isolamento de Arquivos:** Nenhum subagente tem permissão para alterar arquivos sob a responsabilidade de outro.
3. **Coordenação Central:** Apenas o agente coordenador principal realiza commits na branch de apoio e executa a pipeline final de compilação do player.
