# PLANO MESTRE: RECRIAÇÃO DOS 10 MAPAS EM CAMADAS (DARKLIGHT: REQUIEM)
**Data de Emissão**: 05/11/2026  
**Status**: Aprovado para Execução Sequencial  
**Executor Designado**: Codex (com suporte Antigravity)  
**Escopo Canônico**: Réquiem · Katana / Punhos / Moto · 10 Capítulos Completos  

---

## 1. Visão Geral da Arquitetura em Camadas (Layered Design)

Para eliminar definitivamente sobreposição de sprites, bugs de colisão, paredes escaláveis e ataques no vento, a reconstrução dos 10 mapas deve seguir uma separação rigorosa em **4 Camadas Estruturais**. Cada mapa deve ser reconstruído e validado **um por vez (do Mapa 1 ao 10)**, completando as camadas na seguinte ordem:

```
┌─────────────────────────────────────────────────────────────┐
│ CAMADA 4: Arenas, Selos, Inimigos & Iluminação              │
├─────────────────────────────────────────────────────────────┤
│ CAMADA 3: Itens, Relíquias, Checkpoints, Portal & Aliado    │
├─────────────────────────────────────────────────────────────┤
│ CAMADA 2: Casas, Fachadas, Muros, Oclusão & Colisores       │
├─────────────────────────────────────────────────────────────┤
│ CAMADA 1: Planta Baixa (Chão Limpo, Bounds & Walkmesh)      │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Especificação das 4 Camadas de Cada Mapa

### Camada 1: Planta Baixa (Chão / Piso Limpo)
* **Objetivo:** Estabelecer a superfície transitável fundamental sem qualquer elemento vertical, obstáculo ou mobília.
* **Componentes:**
  * Imagem base de fundo (`map{N}.png` / SpriteRenderer em `sortingOrder = -200`).
  * Dimensões canônicas: 1536 × 1024 px (`pixelsPerUnit = 24`, correspondendo a bounds de 64 × 42.66 unidades de mundo).
  * 4 Paredes invisíveis nos limites externos do mundo (`LimiteDoCenario` com `DarklightNavigation.Frictionless`).
  * Polígonos de caminhada (`walk` shapes em `Map{N}Data.json`) definindo ruas, praças e corredores limpos.
* **Critério de Aceite da Camada 1:**
  * Nenhuma colisão interna. O jogador deve percorrer 100% da malha navegável sem atrito ou travamentos.

### Camada 2: Casas, Fachadas, Estruturas & Colisores
* **Objetivo:** Adicionar a arquitetura urbana e gótica de Vespera, bloqueios físicos e profundidade visual.
* **Componentes:**
  * Paredes e blocos sólidos definidos pelos polígonos `blocked` em `Map{N}Data.json`.
  * Conversão determinística em `BoxCollider2D` via `DarklightPaintedMap.BuildGeometry`.
  * Partes frontais de telhados e pórticos recortados (`foreground` com `foot` e `outline`).
  * Componente `DarklightMapOcclusion` em cada estrutura frontal (desvanece para alpha 0.35 quando o player passa atrás do beiral/telhado).
  * Z-Sorting calibrado via `DarklightSorting.OrderForY(foot.y)` garantindo que o player fique atrás da parede ao caminhar por trás dela e à frente ao caminhar no piso anterior.
* **Critério de Aceite da Camada 2:**
  * Depenetração ativa confirmada: impossível atravessar quinas ou "escalar" fachadas.
  * Oclusão suave sem corte brusco.

### Camada 3: Itens, Relíquias & Pontos Interativos
* **Objetivo:** Preencher o mundo com os pontos de objetivo narrativo, pontos de retorno e transições.
* **Componentes:**
  * Ponto de Spawn do jogador (`spawn` verificado com `DarklightMapDefinition.SafeSpawn`).
  * Posicionamento do NPC Aliado (`ally`):
    * Cap. 2: **Ícaro Nox**
    * Caps. 4, 5, 8, 10: **Kael Aliado**
    * Caps. 1, 3, 6, 7, 9: **Lyra Moreau**
  * Relíquia do Capítulo (`relic`): fragmento de memória ancestral com trigger de coleta.
  * Checkpoints sequenciais (`checkpoints[0..3]`): marcos de progresso que salvam posição em `DarklightProgress`.
  * Portal de Transição (`portal`): ativado somente após a conclusão do chefe ou objetivos do mapa.
  * Baús e pickups com raio de coleta limpo (sem colidir com as paredes da Camada 2).
* **Critério de Aceite da Camada 3:**
  * Todos os pontos interativos estão em solo aberto e livre de sobreposição física.

### Camada 4: Arenas, Inimigos & Cenografia Final
* **Objetivo:** Integrar os desafios de combate, atmosfera e finalização visual.
* **Componentes:**
  * 4 Arenas de combate demarcadas (`arenas[0..3]`).
  * Marco dos Selos (`MarcoDoSelo` no piso em `sortingOrder = -198`).
  * Ondas de inimigos (`RequiemEnemyKind`) e gatilhos de encontro (`StartEncounter`).
  * Chefe do Capítulo posicionado na Arena 4 (`arenas[3]`):
    * Mapa 1: Kael Voss
    * Mapa 2: Cantor do Abismo
    * Mapa 3: Matriarca Rubra
    * Mapa 4: Sentinela Eclipse
    * Mapa 5: Guardião do Véu
    * Mapa 6: Lucien Veyr
    * Mapa 7: Valtherion — Primeiro Guardião
    * Mapa 8: Seraphine — Rainha da Noite
    * Mapa 9: Kael — Cavaleiro Fraturado
    * Mapa 10: Nocthar — Rei do Vazio
  * Iluminação ambiente URP 2D e partículas de atmosfera (névoa de Vespera / distorção do Véu).
  * Minimapa sincronizado (`Resources/Darklight/Minimap/Map{N}.png`).
* **Critério de Aceite da Camada 4:**
  * Inimigos nascem em posições navegáveis e não entram em deadlock de IA.
  * Hitbox dos golpes do jogador atinge com precisão os alvos dentro do arco de ataque.

---

## 3. Matriz dos 10 Mapas de Vespera

| # | Nome do Cenário | Capítulo da Campanha | Chefe Principal | Aliado Presente | Resolução / Asset |
|---|-----------------|----------------------|-----------------|-----------------|-------------------|
| **01** | **Rua das Cinzas** | Cap. 1: A Noite da Ruptura | Kael Voss | Lyra Moreau | 1536×1024 · `map1.png` |
| **02** | **Distrito do Vinil** | Cap. 2: Vozes do Abismo | Cantor do Abismo | Ícaro Nox | 1536×1024 · `map2.png` |
| **03** | **Catedral do Sangue** | Cap. 3: Sangue Desperto | Matriarca Rubra | Lyra Moreau | 1536×1024 · `map3.png` |
| **04** | **Laboratório Eclipse** | Cap. 4: O Projeto Eclipse | Sentinela Eclipse | Kael Aliado | 1536×1024 · `map4.png` |
| **05** | **Jardim dos Ecos** | Cap. 5: Além do Véu | Guardião do Véu | Kael Aliado | 1536×1024 · `map5.png` |
| **06** | **Observatório Vespera** | Cap. 6: O Último Eclipse | Lucien Veyr | Lyra Moreau | 1536×1024 · `map6.png` |
| **07** | **Cripta dos Primeiros** | Cap. 7: Herança Carmesim | Valtherion | Lyra Moreau | 1536×1024 · `map7.png` |
| **08** | **Catedral do Crepúsculo** | Cap. 8: A Cidade Sem Amanhecer | Seraphine | Kael Aliado | 1536×1024 · `map8.png` |
| **09** | **Véu Profundo** | Cap. 9: O Trono do Vazio | Kael Fraturado | Lyra Moreau | 1536×1024 · `map9.png` |
| **10** | **Coração do Eclipse** | Cap. 10: Darklight: Requiem | Nocthar | Kael Aliado | 1536×1024 · `map10.png` |

---

## 4. Roteiro Sequencial de Montagem para o Codex

Para cada mapa $N \in [1..10]$, executar o seguinte ciclo sem pular etapas:

### Passo 1: Isolar e Recriar a Planta
1. Carregar/gerar os contornos de piso limpo em `walk`.
2. Rodar teste de navegação com a cápsula do jogador (`Beta10MapValidation`-style).
3. Confirmar que todas as rotas entre `spawn`, `checkpoints`, `arenas`, `relic` e `portal` estão desimpedidas.

### Passo 2: Erguer Casas, Fachadas e Paredes
1. Aplicar os polígonos de bloqueio em `blocked` (paredes sólidas).
2. Gerar recortes frontais em `foreground` para beirais e telhados.
3. Configurar `DarklightMapOcclusion` e validar Z-sorting milimétrico com `DarklightSorting.OrderForY`.
4. Testar caminhada rente a todas as paredes: nenhuma intrusão da cápsula permitida.

### Passo 3: Distribuir Itens e Pontos de Controle
1. Validar coordenadas de `spawn`, `ally`, `relic`, `checkpoints` e `portal`.
2. Confirmar que cada elemento possui distância mínima de 0.6 unidades de qualquer parede colidível.
3. Testar a coleta da relíquia e o diálogo com o aliado local.

### Passo 4: Configurar Arenas, Selos e Encontros
1. Posicionar os 4 selos de arena.
2. Definir a rota de patrulha e ponto de spawn das waves de inimigos.
3. Testar a transição da Arena 1 até a Arena 4 (derrota do Chefe e ativação do Portal).

### Passo 5: Validação do Mapa e Integração
1. Executar os testes de rotas físicas da fase (`DarklightPhysicalRouteTests`).
2. Gerar commit datado no padrão:  
   `feat(map{N}): layer-by-layer reconstruction of {nome_mapa} (ground, houses, items, encounters)`
3. Avançar para o Mapa $N+1$.

---

## 5. Ferramentas e Scripts de Apoio Disponíveis
* Validador de Rotas de Cápsula: [`Beta10MapValidation.cs`](file:///c:/Users/Usuario/darklight-requiem-game/Assets/Darklight/Editor/Beta10MapValidation.cs)
* Builder de Colisões & Oclusões: [`DarklightPaintedMap.cs`](file:///c:/Users/Usuario/darklight-requiem-game/Assets/Darklight/Runtime/World/DarklightPaintedMap.cs)
* Sistema de Z-Sorting Milimétrico: [`DarklightSorting.cs`](file:///c:/Users/Usuario/darklight-requiem-game/Assets/Darklight/Runtime/Character/DarklightSorting.cs)
* Motor de Depenetração Anti-Stuck: [`DarklightNavigation.ResolvePenetration`](file:///c:/Users/Usuario/darklight-requiem-game/Assets/Darklight/Runtime/Character/DarklightNavigation.cs)
* Validador de Build Windows Standalone: `Tools/Delivery/retry_windows_build_20261101.py`

