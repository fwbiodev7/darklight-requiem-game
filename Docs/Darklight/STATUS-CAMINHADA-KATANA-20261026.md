# STATUS E ESPECIFICACAO DA CAMINHADA KATANA (SW/W) — 26/10/2026
**DARKLIGHT: REQUIEM — Etapa C (Antigravity)**

## Objetivo
Registrar formalmente o status de auditoria e a decisao tecnica referente a animacao de **Caminhada da Katana nas direcoes Sudoeste (SW) e Oeste (W)** para a skin Requiem / Normal, documentando os motivos da suspensao preventiva do lote e os parametros de aceitacao para futura integracao.

---

## 1. HISTORICO E MOTIVO DA SUSPENSAO PREVENTIVA

### 1.1 Diretriz Canonica da Etapa C
> *"A caminhada Katana SW/W teve duas versoes rejeitadas por alternancia incorreta das pernas. Nao importar essas versoes. Se existirem referencias canonicas suficientes, preparar uma correcao com alternancia verdadeira de pernas, mesma silhueta e movimento continuo. Se as referencias atuais estiverem ausentes, suspender esse lote e avancar para auditoria ou correcao de codigo independente."*

### 1.2 Auditoria de Fontes no Clone
- **Fontes analisadas**:
  - `ArtSources/WeaponRedesign20261024/candidates/ReqKatanaNormalWalkS-v5-source.png`: Contem **apenas a direcao Sul (0)** com 6 quadros e ancoras calibradas (`anchor: [324, 482]`, etc.).
  - `ArtSources/Darklight/Etapa02/FabioWalk-v1.png` e `FabioWalk.png`: Versoes antigas com descontinuidades de silhueta e escala divergente da padronizacao 1.325f.
  - `ArtSources/RuinFists20261021/references/Requiem_W_Walk_Four_Whole.png`: Referencia de caminhada Oeste autoral para Punhos, nao para Katana com empunhadura bimanual.
- **Conclusao de Auditoria**: Nao existem no repositorio fontes brutas aprovadas de caminhada Katana para SW e W que apresentem a alternancia verdadeira de membros inferiores e correspondam a silhueta consolidada da V5.

### 1.3 Decisao Operacional
**O lote de geracao/importacao de Caminhada Katana SW/W esta SUSPENSO**, em estrita obediencia a diretriz de seguranca. Nenhuma arte improvisada ou derivada de bases rejeitadas foi inserida no projeto. O runtime continua utilizando com seguranca o fallback do `TodayCharacterBank`.

---

## 2. ESPECIFICACAO TECNICA PARA INTEGRACAO FUTURA

Quando as pranchas canonicas com alternancia correta de pernas forem disponibilizadas pelo Codex, os parametros exatos para sua ingestao estao consolidados no template:
`ArtSources/WeaponRedesign20261023/import-jobs-normal-neutral-walk-20261024.template.json`

### 2.1 Parametros Rigorosos de Aceitacao
1. **Ciclo de 6 Quadros**:
   - `durationMs`: `[66.67, 66.67, 66.67, 66.67, 66.67, 66.67]` ms por quadro (15 fps nominais).
   - Sequencia de contato do solo:
     - Quadro 0: Contato calcanhar perna lider.
     - Quadro 1: Carga total e empuxo.
     - Quadro 2: Passagem do membro traseiro.
     - Quadro 3: Contato calcanhar perna oposta (alternancia verdadeira).
     - Quadro 4: Carga total perna oposta.
     - Quadro 5: Retorno ao ciclo fechado.
2. **Ancoras e Linha de Solo (`supportAnchors`)**:
   - Manter a linha de base inferior da sola (`lower sole baseline`) nivelada para evitar trepidacao vertical (bobbing artificial).
   - Escala de renderizacao travada em `WorldVisualScale = 1.325f`.
3. **Empunhadura e Bainha**:
   - A Katana deve permanecer com guarda posicionada e empunhadura firme, com a bainha no quadril esquerdo preservando orientacao angular estavel.
