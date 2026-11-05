# Inventário de Skills Locais e Externas — DARKLIGHT: REQUIEM
**Data:** 05/11/2026  
**Branch de Apoio:** `antigravity/support-skills-expansion`  
**Escopo:** Catálogo Unificado de Especialistas e Ferramentas  

---

## 1. Classificação das Ferramentas por Natureza

Para evitar confusão entre arquivos de documentação, servidores de ferramentas e pacotes Unity:

* **Plugins Oficiais de Agente (Antigravity Plugins):** Pacotes instalados no ambiente do Antigravity contendo conjuntos de skills (`.gemini/config/plugins/`).
* **Skills Markdown Locais:** Guias instrucionais acionáveis em `Tools/Skills/{nome}/SKILL.md` que governam a geração e validação de código.
* **Extensões Unity Open MCP:** Especificações de ferramentas que se comunicam via protocolo MCP com uma instância do Unity Editor em execução.
* **Pacotes Nativos Unity (UPM Packages):** Dependências C# registradas em `Packages/manifest.json`.

---

## 2. Tabela Mestra de Skills do Ecossistema

| Nome da Skill | Origem / Repositório | Tipo | Estado de Ativação | Finalidade Principal |
|---|---|---|---|---|
| **`unity` (Oficial Agent Plugin)** | Unity-Technologies / `unity-agent-plugin` | Plugin Antigravity | **Instalado & Ativo** (`v0.1.8-beta`) | 28 skills curadas para URP, UI, física 2D, profiling e CLI. |
| **`darklight-cutscene-engineering`** | AlexeyPerov / `Unity-Open-MCP` (adaptada) | Skill Local + C# Fallback | **Disponível & Validada** | Timeline, PlayableDirectors, faixas, skip seguro e transições de cutscene. |
| **`darklight-cinematic-camera`** | AlexeyPerov / `Unity-Open-MCP` (adaptada) | Skill Local + Standalone C# | **Disponível & Validada** | Câmera ortográfica 2D, Cinemachine 3.x, blends, trauma shake e foco de chefe. |
| **`darklight-pixel-perfect-qa`** | Unity-Technologies / `2d-pixel-perfect` | Skill Local Especializada | **Disponível & Validada** | URP 2D Pixel Perfect, Point filtering, PPU 24, WorldVisualScale 1.325. |
| **`pixel-art-sprite-sheet-master`** | Local + ProkStudio / `pixel-art-fundamentals` | Skill Local + Referências | **Disponível & Validada** | Criação, animação, auditoria de silhueta, olhos `#C40814` e pele `[246,174,130]`. |
| **`create-2d-physics`** | Plugin Oficial Unity | Skill Integrada Antigravity | **Ativo** | Física 2D, colisores, materiais atômicos e anti-stuck. |
| **`urp-postprocessing`** | Plugin Oficial Unity | Skill Integrada Antigravity | **Ativo** | Configuração de Volumes URP, Bloom, Vinheta e Color Grading. |
| **`unity-cli`** | Plugin Oficial Unity | Skill Integrada Antigravity | **Ativo** | Controle do Editor via linha de comando e builds headless. |
| **`unity-package-management`** | Plugin Oficial Unity | Skill Integrada Antigravity | **Ativo** | Gerenciamento de dependências UPM fora do editor. |

---

## 3. Localização dos Arquivos Físicos no Repositório

```
darklight-requiem-game/
├── Tools/
│   └── Skills/
│       ├── darklight-cutscene-engineering/
│       │   └── SKILL.md
│       ├── darklight-cinematic-camera/
│       │   └── SKILL.md
│       ├── darklight-pixel-perfect-qa/
│       │   └── SKILL.md
│       └── pixel-art-sprite-sheet-master/
│           ├── SKILL.md
│           └── references/
│               ├── pixel-art-fundamentals.md   <-- Integrado com regras canônicas
│               ├── metadata.md
│               └── quality.md
└── work/
    └── antigravity-skills-handoff/
        ├── references/                         <-- Arquivos originais externos salvos
        │   ├── timeline_skill.md
        │   ├── cinemachine_skill.md
        │   └── pixel_art_fundamentals_skill.md
        ├── INVENTARIO-SKILLS.md
        ├── LISTA-SUBAGENTES.md
        ├── TESTES-COMPATIBILIDADE-E-CONFLITOS.md
        ├── CONFIGURACOES-FUNCIONAIS.md
        ├── GUIA-DE-UTILIZACAO-SKILLS.md
        └── RELATORIO-RECURSOS-CODEX.md
```
