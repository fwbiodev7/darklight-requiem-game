# DARKLIGHT: REQUIEM — Diagnóstico QA: Caminhada Punhos/Grevas de Fabius
**Data**: 04/11/2026  
**Auditor**: Antigravity Technical Support Engineering  
**Referência do problema**: Relato de 03/11 — Frame deslocado à frente dos demais na locomoção de Punhos/Grevas.

## 1. Localização e Dados Identificados

- **Fonte**: `outputs/retomada-20261101/punhos-walk/REPORT.md` e `ArtSources/WeaponRedesign20261101/punhos-walk/review.json`
- **Folha**: `staging/ReqFistsNormalWalkS20261101V2Selected.png`
- **Ciclo avaliado**: 4 fases de 100ms (Loop de 400ms):
  - `S00`: Contato esquerdo
  - `S01`: Apoio esquerdo / passagem direita
  - `S02`: Contato direito
  - `S03`: Apoio direito / passagem esquerda

## 2. Diagnóstico da Anomalia

1. **Deslocamento de Pivô Relativo**:
   - O quadro `S01` apresenta um avanço de centro de massa de ~6.5 pixels além do eixo vertical do pé de apoio se comparado ao ciclo base da Katana.
   - Isso decorre do cálculo do pivô ter considerado a projeção da sola sem compensação do impulso frontal da perna direita em passagem.
2. **Efeito Visual no Play Mode**:
   - Durante a reprodução em 10 fps (100ms por frame), o sprite parece "escorregar" ou saltar para frente bruscamente no frame 2 (`S01`), antes de retornar à posição de apoio no frame 3 (`S02`).

## 3. Recomendação de Correção para o Codex

- **NÃO alterar o desenho do sprite**. A arte completa está aprovada em maturidade e proporção.
- **Ajustar apenas o metadado de pivô (`pivot.x`)** no Sprite Editor / arquivo `.meta` correspondente quando integrado:
  - Recuar o pivô horizontal de `S01` em **4 a 6 pixels** (ou ~0.015 em espaço normalizado uv).
  - Manter `S00`, `S02` e `S03` intactos, preservando a base de apoio no solo.
- **Teste de Validação**:
  - Rodar o teste em EditMode de alinhamento (`DarklightAnimationAlignment.cs`) para certificar que a oscilação do delta X não ultrapassa o limiar de translação suave (máximo 2.5px entre passagens).
