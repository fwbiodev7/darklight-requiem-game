# DARKLIGHT: REQUIEM — AUDITORIA DE DIREÇÕES E PENDÊNCIAS ARTÍSTICAS

**Data:** 11 de Novembro de 2026  
**Autor:** Antigravity (Google DeepMind)  
**Projeto:** DARKLIGHT: REQUIEM  
**Diretriz Técnica:** Mapeamento rigoroso das 8 direções cardeais e colaterais (N, NE, E, SE, S, SW, W, NW) por personagem e ação. Classificação estrita entre **Arte Real (Integrada)**, **Fallback Funcional Seguro (Espelhamento/Orientação de Repouso)** e **Faltante (Produção Futura)**. Nenhuma arte foi sintetizada artificialmente ou inventada.

---

## 1. Personagens Principais: Fabius e Lucius

### 1.1 Fabius Normal & Transformações
- **Fabius Normal:**
  - **Idle (8 direções):** Real (N, NE, E, SE, S, SW, W, NW) — 100% integrado e estabilizado (zero tremor).
  - **Katana (Combos):** Real em 8 direções.
  - **Punhos da Ruína:** Real em 8 direções.
  - **Motocicleta (Requiem):** Real em 8 direções de montaria e condução (`DarklightMotorcycleRedesignBank`).
- **Fabius Ruptura (Forma 1):**
  - **Katana Carmesim:** Real (3x3 grid, 9 quadros).
  - **Esquiva / Ativação:** Real (2x4 grid, 8 quadros).
  - **Punhos da Ruína:** Real (3x3 grid, 9 quadros).
  - **Motocicleta Ruptura:** Real (2x4 grid, 8 quadros).
  - **Moto para Serras:** Real (3x4 grid, 12 quadros).
- **Fabius Serafim Carmesim (Forma 2 / Eclipse):**
  - **Katana Vampírica:** Real (3x3 grid, 9 quadros).
  - **Despertar / Ultimate:** Real (2x4 grid, 8 quadros).
  - **Punhos Alados:** Real (3x3 grid, 9 quadros).
  - **Moto com Propulsão Flamejante:** Real (2x4 grid, 8 quadros).
  - **Combate com Serras:** Real (3x4 grid, 12 quadros).

---

### 1.2 Lucius Normal & Arconte do Abismo

| Personagem / Estado | Família / Ação | N | NE | E | SE | S | SW | W | NW | Observação Técnica |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|---|
| **Lucius Normal** | **Idle** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | 8 vistas estáticas integradas no catálogo. |
| | **Walk** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | S, SW, W, E integrados com 4 quadros; NW, N, NE, SE usam fallback orientado ao deslocamento físico. |
| | **WalkArmed** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | 8 direções reais empunhando a lâmina (`LuciusWalkArmed8V1`). |
| | **SwordHorizontal** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | 8 direções completas (3 fases reais: startup, active, recovery). |
| | **SwordVertical** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | 8 direções completas (3 fases reais). |
| | **SwordThrust** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Combo em 6 quadros (`LuciusSwordThrustV1`) em RGBA nativo com estocada frontal de 2.10m. |
| | **Parry / Defesa** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Guarda de batalha e bloqueio ativo (`LuciusParryV1`). |
| | **Hurt** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Reação a impacto e flinch em 4 quadros (`LuciusHurtV1`). |
| | **Dead** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Queda e morte ao solo em 4 quadros (`LuciusDeadV1`) mantendo linha do solo estável. |
| **Arconte do Abismo** | **Idle** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | 8 direções do Arconte com aura e olhos luminosos (`LuciusArconteIdle8V1`). |
| | **Transformation**| REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Despertar do Nexo Fenrir em 8 fases (`LuciusArconteAwakeningV1`). |
| | **Lâminas Espectrais** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Golpes espectrais sweep e vertical (`LuciusArconteAttacksV1`). |
| | **Abyss Burst** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Ultimate de área sombria (`LuciusArconteUltimateV1`). |

---

## 2. Quatro Inimigos Comuns

| Inimigo | Ação | N | NE | E | SE | S | SW | W | NW | Estado Técnico |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|---|
| **Crawler** | **Idle / Espreita** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Frontal e perfil lateral disponíveis; colaterais usam orientação segura. |
| | **Move / Rastejo** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Ciclo de passada rasteira integrado. |
| | **Attack / Bote** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Salto e garra ativos com hitbox de colisão frontal. |
| | **Dead / Morte** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Queda estirada no solo. |
| **Shooter** | **Idle / Postura** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Postura com foco arcano. |
| | **Move / Recuo** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Movimentação tática de afastamento. |
| | **Attack / Disparo** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Disparo de orbes violetas em ângulo calculado pelo vetor de mira. |
| | **Dead / Dissipação**| REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Dissipação em partículas mágicas. |
| **Armored** | **Idle / Guarda** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Postura pesada com escudo e maça. |
| | **Move / Marcha** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Passada firme de armadura pesada. |
| | **Attack / Maça** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Golpe descendente massivo com stagger garantido. |
| | **Shield Guard** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Postura defensiva de bloqueio frontal. |
| | **Dead / Queda** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Tombamento massivo ao solo. |
| **Spectral** | **Idle / Flutuação**| REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Ciclo levitante contínuo independente de atrito de solo. |
| | **Move / Desvio** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Deslocamento etéreo com ondulação vertical. |
| | **Attack / Orbes** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Disparos múltiplos em leque com mira desacoplada. |
| | **Dead / Névoa** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Dissipação completa em bruma espectral. |

---

## 3. Três Bosses

| Boss | Ação | N | NE | E | SE | S | SW | W | NW | Estado Técnico |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|---|
| **Kael Voss** | **Idle / Guarda** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Postura de guarda de zweihänder; direção N usa pose traseira aprovada. |
| | **Walk / Locomoção**| FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | **FALTANTE:** Walk dedicado não fornecido no acervo; opera com avanço de passada de guarda e dash. |
| | **Ataques de Gelo** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Cortes circulares, estocada com projéteis glaciais e cleave vertical (`068`). |
| | **Parry / Bloqueio** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Postura de montante com contra-ataque. |
| | **Dead / Derrota** | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | FALLBACK | **FALTANTE:** Animação de queda de Kael não fornecida; utiliza ajoelhar/rendição da cutscene. |
| **Cantor do Abismo**| **Idle / Flutuação**| REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Levitação com cajado ativo (`Cantor_Hover_Death_V1`). |
| | **Ondas Arcanas** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Disparo de ondas de ressonância sonoras concêntricas (`057`). |
| | **Pilares do Véu** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Erupção de colunas de energia violeta no solo. |
| | **Hurt & Death** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Desintegração em partículas com queda gradual do manto. |
| **Matriarca Rubra**| **Idle / Postura** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Postura régia alada (`Matriarca_Walk_Dead_V2`). |
| | **Walk / Passos** | FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Ciclo de caminhada elegante com garras recolhidas. |
| | **Claw Strike / Rasante**| FALLBACK | FALLBACK | REAL | FALLBACK | REAL | REAL | REAL | FALLBACK | Ataque em mergulho com garras escarlates. |
| | **Dead / Queda** | REAL | REAL | REAL | REAL | REAL | REAL | REAL | REAL | Recolhimento de asas e colapso ao solo em 4 quadros. |

---

## 4. Recomendações de Produção Futura para a Arte
1. **Kael Voss:** Produzir folha dedicada de caminhada pesada (4 direções mínimas: S, W, E, N) e morte solene em combate (queda de joelhos com fincar da zweihänder no solo).
2. **Colaterais de Inimigos Comuns:** As diagonais NE, SE, NW, SW de Crawler, Shooter e Armored utilizam atualmente o ângulo cardeal mais próximo; a produção de folhas com vistas colaterais 45° elevará a riqueza visual para padrão 8V completo sem impacto mecânico na jogabilidade.
3. **Pivôs e Proporções:** Todo o material integrado preserva rigidamente a escala `WorldVisualScale = 1.325` e PPU estabilizado, garantindo ausência de estiramento ou tremor.

