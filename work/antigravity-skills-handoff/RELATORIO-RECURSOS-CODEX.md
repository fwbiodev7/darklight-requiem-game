# Relatório de Recursos e Handoff para o Codex — DARKLIGHT: REQUIEM
**Data:** 05/11/2026  
**De:** Antigravity Engineering Swarm  
**Para:** Codex (quando retornar a cota semanal de tokens)  
**Branch de Trabalho Isolada:** `antigravity/support-skills-expansion`  

---

## 1. Sumário Executivo da Expansão

Para acelerar a engenharia e garantir que as próximas fases de desenvolvimento transcorram com zero conflitos, zero retrabalho e sem desperdício de tokens, o Antigravity implementou um ecossistema completo de skills, referências canônicas e subagentes especializados.

Todas as ferramentas foram auditadas contra a versão exata do projeto (**Unity 6000.6.0f1**, **URP 17.6**, **Timeline 6.6**).

---

## 2. Inventário de Recursos Disponíveis para o Codex

### A. Skills Especializadas Adicionadas / Aprimoradas:
1. [`Tools/Skills/darklight-cutscene-engineering/SKILL.md`](file:///c:/Users/Usuario/darklight-requiem-game/Tools/Skills/darklight-cutscene-engineering/SKILL.md)
   * Domínio: Unity Timeline (`.playable`), PlayableDirectors, faixas de animação/ativação/áudio/sinais, skip atômico e transições cinematográficas.
   * Prioridade: Cutscene de infância (Fábio e Lucius com espadas de madeira) e confronto final com Lucien / Nocthar.
2. [`Tools/Skills/darklight-cinematic-camera/SKILL.md`](file:///c:/Users/Usuario/darklight-requiem-game/Tools/Skills/darklight-cinematic-camera/SKILL.md)
   * Domínio: Câmera ortográfica 2D, blends, screen shake matrix (impactos, parry, despertar), apresentação dramática de chefes e retorno suave ao controle do jogador.
   * Proteção: Não quebra o controlador de câmera atual.
3. [`Tools/Skills/darklight-pixel-perfect-qa/SKILL.md`](file:///c:/Users/Usuario/darklight-requiem-game/Tools/Skills/darklight-pixel-perfect-qa/SKILL.md)
   * Domínio: Auditoria URP 2D Pixel Perfect, `Point (no filter)`, `PPU = 24`, Mipmaps desativados, `WorldVisualScale = 1.325`, sem tremor ou jitter sub-pixel.
   * Proteção: Previne a instalação acidental do pacote Built-in `com.unity.2d.pixel-perfect`.
4. [`Tools/Skills/pixel-art-sprite-sheet-master/references/pixel-art-fundamentals.md`](file:///c:/Users/Usuario/darklight-requiem-game/Tools/Skills/pixel-art-sprite-sheet-master/references/pixel-art-fundamentals.md)
   * Domínio: Fundamentos de pixel art (silhueta, jaggies, banding, pillow shading, dithering) integrados ao cânone soberano de DARKLIGHT (Fábio maduro/adulto, olhos `#C40814`/`#5A0000`, pele `[246, 174, 130]`, quadros inteiros em folhas de 8-12 figuras).

### B. Subagentes Definidos no Sistema Antigravity:
1. `cinematic-systems-agent`: Especialista em cutscenes, Timeline e transições narrativas.
2. `camera-pixel-perfect-agent`: Especialista em Cinemachine, câmera ortográfica e estabilidade visual.
3. `pixel-art-qa-agent`: Especialista em garantia de qualidade e obediência ao cânone visual.
4. `unity-engineering-agent`: Especialista em pacotes UPM, integridade de compilação e testes.

---

## 3. Instruções de Continuidade para o Codex

Quando o Codex iniciar sua próxima sessão:
1. **Verificação de Branch:** Os artefatos e ferramentas de apoio residem na branch `antigravity/support-skills-expansion`. O Codex pode incorporar os módulos necessários sem alterar a `main` precipitadamente.
2. **Execução Sequencial dos Mapas:** Siga rigorosamente o [`PLANO-RECRIACAO-10-MAPAS-EM-CAMADAS.md`](file:///c:/Users/Usuario/darklight-requiem-game/Docs/Darklight/PLANO-RECRIACAO-10-MAPAS-EM-CAMADAS.md):
   * Passo 1: Planta Baixa (chão limpo, bounds e walkmesh livre);
   * Passo 2: Casas, Fachadas e Paredes (polígonos 'blocked', colliders BoxCollider2D, oclusão de telhados e Z-sorting com DarklightSorting);
   * Passo 3: Itens, Relíquia, Checkpoints, Aliado e Portal (coordenadas seguras sem overlap em paredes);
   * Passo 4: Arenas, Selos e Inimigos (waves de combate e chefe na Arena 4).
3. **Reutilização Inteligente:** Não reescreva do zero sistemas de câmera ou colisão. Reutilize `DarklightSorting.cs` para profundidade e `DarklightNavigation.ResolvePenetration` para evitar paredes escaláveis.
