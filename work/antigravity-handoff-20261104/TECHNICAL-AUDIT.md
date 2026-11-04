# TECHNICAL AUDIT — DARKLIGHT: REQUIEM
**Data**: 04/11/2026  
**Auditor**: Antigravity Technical Support Engineering  
**Branch de trabalho**: `antigravity/support-20261104`  
**Commit base**: `601a62a`

## 1. Visão Geral da Arquitetura C#

- **Sistema Central**: Implementado primariamente como partial classes sob o namespace `Darklight` no MonoBehaviour central `DarklightCampaign`.
- **Combat Loop**: Totalmente desacoplado via `CombatModel`, operando com clock próprio (`CombatDelta`), sem dependência direta de corrotinas ou timescale global.
- **Entidades e Inimigos**: Gerenciados por `DarklightEnemy` (com extensões táticas e aéreas em `DarklightEnemyTactics`), suportando chefes de múltiplas fases e até 15 variantes canônicas de inimigos (`RequiemEnemyKind`).
- **Persistência / Save & Load**: Gerenciado por `DarklightProgress` (versão 2) com suporte a 10 capítulos, marcos narrativos em flags binárias (`narrativeMilestones`), habilidades vampirescas e Dark Resonance.

## 2. Diagnóstico dos Módulos Principais

| Módulo | Estado Atual | Pontos de Risco / Gargalos | Suporte Desenvolvido |
| :--- | :--- | :--- | :--- |
| **Cutscenes** | Não havia infraestrutura modular isolada de cinematics; diálogos e transições eram geridos via eventos ad-hoc na campanha. | Acoplamento excessivo ao fechar/abrir diálogos; risco de travar input no skip. | Desenvolvido conjunto completo em `work/antigravity-cutscenes/` com `CutsceneManager`, `CutsceneInputGate`, `CutsceneEventBridge`, `CutsceneSkipController`. |
| **Transformações** | Operavam com timers em `DarklightCampaignTransformations.cs`. Formas canônicas: Ruptura, Eclipse Absoluto, Nexo Fenrir. | Multiplicação acidental de atributos em transições rápidas; risco de persistência de buffs após morte. | Desenvolvido `TransformationSafetyController.cs` com máquina de estados desacoplada e remoção garantida de modificadores. |
| **Locomoção / Sprites** | Catálogo amplo com PPU ~170.96 e controle de pivôs nos pés. | Anomalia detectada em 03/11 na caminhada de Punhos/Grevas (quadro adiantado). | Desenvolvida ferramenta Python `animation_qa_inspector.py` e diagnóstico em `PUNHOS-WALK-DIAGNOSIS.md`. |
| **Gothic Eclipse (VFX/Luzes)** | Custo de renderização 2D e partículas em fases densas (Jardim dos Ecos / Catedral). | Overdraw em partículas e 2D Lights ativas simultâneas fora do frustum. | Desenvolvido `GothicEclipseOptimizer.cs` com culling por distância e métricas em tempo real. |
| **Campanha & 10 Fases** | 10 capítulos estruturados via matriz de títulos e diálogos em `DarklightCampaignNarrative.cs`. | Risco de duplicação de conclusão de missão ou perda de progresso após reload. | Desenvolvido `CampaignProgressionController.cs` com validação de pré-requisitos e prevenção de duplo disparo. |
| **Menus & Configurações** | Gerenciado via PlayerPrefs diretamente em `DarklightCampaign.cs`. | Falta de abstração para controle desacoplado de preferências e estados de navegação. | Desenvolvido `UIPreferencesBridge.cs` com serialização e fallback seguro. |
| **IA de Combate** | Máquina de estados em `DarklightEnemy.cs` (0=Vigiando, 1=Preparando, 2=Ataque). | Risco de loop infinito em windup caso o player saia do alcance durante a preparação. | Desenvolvido `AISafetyValidator.cs` para desvencilhamento de estados bloqueados. |

