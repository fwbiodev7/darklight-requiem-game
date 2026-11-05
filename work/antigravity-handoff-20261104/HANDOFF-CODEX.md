# HANDOFF CODEX — DARKLIGHT: REQUIEM
**Data**: 04/11/2026  
**De**: Antigravity Technical Support Engineering  
**Para**: Codex  

## Resumo da Entrega

Todos os pacotes técnicos de suporte solicitados foram desenvolvidos em ambiente isolado, testados e documentados:
- **Cutscenes C#**: Arquitetura modular completa com suporte a skip seguro, input gate atômico e testes unitários.
- **Transformações**: Controlador desacoplado com proteção de estados e remoção de multiplicadores residuais.
- **QA de Animações**: Ferramenta de inspeção de metadados e diagnóstico cirúrgico da caminhada de Punhos/Grevas de Fabius.
- **Otimização Gothic Eclipse**: Helpers de métricas e culling de atmosfera 2D.
- **Campanha & Missões**: Orquestrador de objetivos e prevenção de duplo disparo.
- **UI & Menus**: Bridge desacoplada de preferências e navegação.
- **Prevenção de Deadlock de IA**: Sanitização de forças e timeout de estados de perseguição/windup.

Todos os artefatos estão organizados sob o diretório `work/`.


## Status da Build Windows Standalone (04/11/2026 - Atualizacao Caminhadas)

- **Resultado**: Succeeded (0 erros)
- **Executavel Local**: outputs/retomada-20261101/Windows/DarklightRequiem.exe
- **Pacote Distribuivel Zip**: outputs/retomada-20261101/DarklightRequiem-Windows-20261101-155225.zip (1.53 GB)
- **Tamanho Total Descompactado**: ~1.98 GB (1.988.133.503 bytes)
- **Recursos Validados**:
  - 946 figuras em 110 folhas de spritesheets do redesign (incorporando todas as 24 páginas de caminhada)
  - 151 efeitos sonoros (19 efeitos bounded de entrega + 132 do banco base preservados)
  - Áudio interativo habilitado para execução do usuário
- **Cobertura de Caminhadas Integradas (catalog.json)**:
  - **Katana**: 8 direções completas (S, SW, W, NW, N, NE, E, SE)
  - **Punhos da Ruína**: 8 direções completas (S, SW fechado em 03/11, W, NW, N, NE, E, SE)
  - **Moto**: 8 direções completas (S, SW, W, NW, N, NE, E, SE)
- **Escopo Ativo**:
  - Skin: Réquiem
  - Formas: Normal / Ruptura / Eclipse Absoluto
  - Armas: Katana / Punhos da Ruína / Moto
- **Pipeline de Validacao**: DarklightWeaponRedesignBuild.Build executado com sucesso em modo batch headless via Tools/Delivery/retry_windows_build_20261101.py.

## Status da Build Windows Standalone (05/11/2026 - Colisoes, Z-Sorting e Combate)

- **Resultado**: `Succeeded` (0 erros)
- **Executavel Local**: `outputs/retomada-20261101/Windows/DarklightRequiem.exe`
- **Pacote Distribuivel Zip**: `outputs/retomada-20261101/DarklightRequiem-Windows-20261101-134858.zip` (1.53 GB)
- **Tamanho Total Descompactado**: ~1.98 GB (1.988.125.242 bytes)
- **Melhorias de Runtime Entregues**:
  1. **Fisica Anti-Stuck e Despenetracao**:
     - `DarklightNavigation.cs`: Implementado `ResolvePenetration(Rigidbody2D body)` que calcula o vetor de separacao com `col.Distance` e ejeta instantaneamente o corpo de qualquer colisor solido (`LimiteDoCenario`).
     - Corrigido bug de `normal == Vector2.zero` em `SlidingVelocity`, impedindo que o jogador avance para dentro de quinas ou paredes quando o cast comeca em contato.
     - `DarklightFighter.cs` e `DarklightAbyssRider.cs`: Integrada despenetracao preventiva a cada `FixedUpdate` e durante dashes da moto.
  2. **Z-Sorting Milimetrico (Fim da Sobreposicao Invertida)**:
     - `DarklightSorting.cs`: Novo sistema com resolucao de 100 camadas por metro (`BaseOrder = 3000`, `Precision = 100f`).
     - Atualizados `DarklightCharacterView.cs`, `DarklightEnemy.cs`, `DarklightEnemyVisitors.cs`, `DarklightNPCView.cs` e `DarklightPaintedMap.cs`.
     - Fim definitivo do Z-fighting entre Player, Inimigos e Cenarios.
  3. **Calibracao de Hitboxes e Contato (Fim do 'Bater no Vento')**:
     - `DarklightCombatRules.cs`: Contencao do arco angular de `Sweep` e `Sector`, limitando o padding angular excessivo a no maximo 12 graus.
     - `DarklightStrikeResolver.cs`: Calculo de contato avaliado contra o volume real do alvo (`samplePoint` ponderado com o centro do inimigo), eliminando acertos fantasmas a longa distancia causados apenas pela borda da hurtbox.


---

## 4. Plano de Recriação dos 10 Mapas em Camadas (Codex Protocol)

O usuário aprovou a **recriação dos 10 mapas em 4 camadas estruturais isoladas**, montando **um mapa por vez**, na ordem:
1. **Camada 1: Planta Baixa** (Piso limpo, sem itens, sem casas, apenas malha navegável e bounds de câmera).
2. **Camada 2: Casas & Estruturas** (Paredes sólidas, fachadas, telhados, oclusão dinâmica DarklightMapOcclusion e Z-sorting milimétrico).
3. **Camada 3: Itens & Interativos** (Spawn, NPC aliado, relíquia, baús/pickups, checkpoints e portal de saída).
4. **Camada 4: Arenas & Inimigos** (Marcos dos selos, gatilhos de ondas de combate, chefe do capítulo e iluminação 2D).

Documento mestre completo com especificações e coordenadas de cada fase:
👉 Docs/Darklight/PLANO-RECRIACAO-10-MAPAS-EM-CAMADAS.md

### Prompt Pronto para o Codex Executar a Montagem:
`markdown
Codex, continue a montagem dos 10 mapas do DARKLIGHT: REQUIEM seguindo estritamente o plano em camadas:
Arquivo de referência: Docs/Darklight/PLANO-RECRIACAO-10-MAPAS-EM-CAMADAS.md

Regras de execução obrigatórias:
1. Trabalhe em UM MAPA POR VEZ (comece pelo Mapa 1: Rua das Cinzas).
2. Para cada mapa, monte as camadas na ordem canônica:
   - Passo 1: Planta Baixa (chão limpo transitável, sem nenhum obstáculo, bounds e walkmesh livre);
   - Passo 2: Casas, Fachadas e Paredes (polígonos 'blocked', colliders BoxCollider2D, oclusão de telhados e Z-sorting com DarklightSorting);
   - Passo 3: Itens, Relíquia, Checkpoints, Aliado e Portal (coordenadas seguras sem overlap em paredes);
   - Passo 4: Arenas, Selos e Inimigos (waves de combate e chefe na Arena 4).
3. Valide cada fase com testes de rota da cápsula antes de avançar para o próximo mapa.
4. Ao concluir cada mapa, faça o commit correspondente e avance sequencialmente até o Mapa 10.
`
