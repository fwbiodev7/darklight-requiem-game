# DARKLIGHT: REQUIEM — Feedback do Criador e Diretrizes para o Codex (28/10/2026)

> **Data do Playtest:** 28 de outubro de 2026, às 19:30 (Horário de Brasília)  
> **Build Testada:** Windows Standalone (`DarklightRequiem-Windows-20261027-191416.zip` / `DarklightRequiem.exe`)  
> **Destinatário:** Subagentes e orquestração do Codex para a sessão de amanhã (29/10/2026)  
> **Prioridade:** MÁXIMA — Direcionamento estético e de animação diretamente pelo criador do jogo  

---

## 1. Veredicto Geral do Playtest

O criador executou a build mais recente compilada após os avanços de hoje. O feedback principal é que **as partículas de impacto e o feedback de combate ficaram ótimos**, mas há uma **escassez crítica de animações de locomoção (caminhada) e cobertura de ataques**, tornando a movimentação travada ou com dependência de poses estáticas/fallbacks.

---

## 2. Pontos Aprovados

- **Partículas de Impacto (Pixel Contact FX):**
  - O sistema de burst e partículas de colisão (`Assets/Darklight/Resources/Darklight/CombatContacts20261027/ContactBursts.png` e `DarklightPixelContactBurst.cs`) foi aprovado visualmente. O impacto do golpe transmite o peso necessário sem poluir o visual. Manter essa linha de renderização.

---

## 3. Lacunas e Problemas Críticos Relatados

| Mecânica / Área | Problema Observado | Impacto no Jogo |
|---|---|---|
| **Katana — Locomoção** | Falta animação de caminhada lateral direita (Leste / E e Sudeste / SE). | Ao andar para a direita ou diagonais direitas, o personagem patina ou recorre a poses que não fecham um ciclo de passada fluida. |
| **Moto — Locomoção a pé** | Falta completa de caminhada da moto (dual-saws empunhadas a pé). | O jogador não possui animação de corrida/caminhada natural com as serras duplas em mãos. |
| **Moto — Ataques** | Faltam ataques reais e dedicados da moto (tanto a pé quanto montada). | As 8 vistas estáticas e o slam genérico de 4 quadros são insuficientes; as 20 técnicas da moto precisam de animações próprias. |
| **Punhos — Locomoção** | Falta completa de ciclo de caminhada e corrida para os Punhos da Ruína. | Sem walk/run cycle, o personagem desliza ou congela nas transições de pose. |
| **Outras Transições** | Falta geral de ciclos fluidos de desaceleração (*Brake*), parry/defesa e reações. | O personagem salta abruptamente de ações de ataque para o repouso. |

---

## 4. Plano de Ação Prioritário para o Codex (29/10/2026)

O Codex deve focar a geração e o acabamento dos seguintes lotes específicos amanhã:

### Prioridade 1: Fechar o Ciclo de Caminhada da Katana (8 Direções)
- **Problema atual:** A caminhada Noroeste/Norte (NW/N) e Sudoeste/Oeste (SW/W) foi aprovada, mas a lateral direita (NE, E e SE) ficou apenas em estágio de candidata (`ReqKatanaNormalWalkNE_E` e `ReqKatanaNormalWalkE-v3-guided`).
- **Ação imediata:**
  1. Concluir o ciclo Leste (E) com alternância real entre perna direita e esquerda (não repetir a mesma perna em ambos os lados).
  2. Produzir a folha Sudeste (SE) mantendo o PPU 148~155 e WorldVisualScale 1.325.
  3. Validar a cadência e loop das 8 direções de caminhada da Katana.

### Prioridade 2: Locomoção e Ciclo de Caminhada dos Punhos da Ruína
- **Problema atual:** Em 28/10, as folhas de teste `ReqFistsNormalWalkS20261028V1` e `V2` foram rejeitadas porque todas as figuras lideravam com a mesma bota e espelhavam a corrente, não constituindo um ciclo alternado.
- **Ação imediata:**
  1. Desenhar um ciclo legítimo de caminhada Sul (S) de 8 fases para Punhos, com passadas opostas nítidas (pé esquerdo avança -> contato -> pé direito avança -> contato).
  2. Expandir para as vistas cardeais e colaterais (W, E, N).

### Prioridade 3: Animações de Caminhada e Técnicas da Moto (Dual-Saws)
- **Problema atual:** O lote Oeste (W) de caminhada com dual-saws em 28/10 foi rejeitado por repetição de bota nas duas metades.
- **Ação imediata:**
  1. Resolver o contato oposto do ciclo W e fechar a caminhada a pé com as serras duplas (8 fases).
  2. Avançar nas técnicas principais de ataque da moto a pé:
     - Finalizar as 8 direções de *Dentes Cruzados* (apenas SW e W foram iniciadas em estágio candidato).
     - Implementar *Giro das Engrenagens* e *Esmagamento do Abismo*.

### Prioridade 4: Importação e Ativação no Runtime das 24 Figuras do Giro Devastador
- Os 24 frames do **Giro Devastador dos Punhos** (`ReqFistsNormalGiroSSWWNW20261028V1` e `ReqFistsNormalGiroNNEESE20261028Selected`) e a transição da Moto (`ReqMotoNormalJoinSeparateS20261028V1`) já foram estruturados tecnicamente e verificados em catálogo:
  - Garantir a importação limpa na Unity com `DarklightWeaponRedesignImporter`.
  - Conectar os tempos de *startup (110ms)*, *active (120ms)* e *recovery (200ms)* ao combate.

---

## 5. Diretrizes Rigorosas de Estilo e Preservação

1. **Anatomia e Escala:** Preservar a anatomia adulta (proporção ~1.98 unidades de altura, cabeça e cabelo castanho canônico, casaco bordô/preto com detalhes prateados).
2. **Sem Reintroduzir o Legado do Form 2:** Não ressuscitar as folhas antigas do Form 2/Eclipse anterior para a forma fundida Eclipse Absoluto.
3. **Alternância de Passadas Obrigatória:** Nenhuma folha de locomoção deve ser promovida se ambos os semiciclos utilizarem a mesma perna/pé de apoio.
