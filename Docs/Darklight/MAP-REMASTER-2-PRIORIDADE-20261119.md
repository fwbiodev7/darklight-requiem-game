# DARKLIGHT: REQUIEM — MAP REMASTER 2.0

Solicitação do usuário registrada em 17/11/2026 para a próxima sessão após a renovação semanal. Este documento preserva o escopo, restrições, referências e critérios do pedido; não declara a frente iniciada.

## Condições e prioridade

- Não iniciar antes de **19/11/2026 às 21:11 America/Sao_Paulo**. O pedido não autoriza antecipar a retomada nem ignorar as fases oficiais.
- Consultar a cota real: preservar no mínimo **20% da semanal e 10% da janela de cinco horas**. A exceção de cota concedida em 17/11 foi apenas para o loading.
- Ler AGENTS.md, checkpoints, branch, mudanças locais e `weekly_reset_prompt.md`. Planejar mapas como frente prioritária dentro do plano vigente de oito fases, respeitando conclusão, validação e autorizações da fase corrente.
- Unity 6000.6.0f1; 2D top-down pixel art, dark fantasy, RPG de ação. Repositório indicado: https://github.com/fwbiodev7/darklight-requiem-game — conferir o remoto real antes de publicar.
- Melhorar os dez mapas existentes: colisão, tamanho, coerência ambiental, navegabilidade, objetos e direção artística. Preservar o que funciona; nenhuma reconstrução generalizada sem evidências.

## 1. Estudo obrigatório antes de modificar mapas

Consultar de fato os materiais, registrar eventuais páginas inacessíveis e escrever um resumo curto de técnicas aplicáveis ao projeto. Não alegar leitura que não ocorreu; sem internet, usar documentação local e registrar a limitação.

| Material indicado pelo usuário | Aplicação esperada |
| --- | --- |
| https://unity.com/blog/level-design-basics | Rotas principais/secundárias, landmarks, composição, ritmo de encontros, densidade, progressão e leitura das rotas. |
| https://docs.unity.com/en-us/engine/6000.6/manual/unity2d/tilemaps/tilemap-collider-2d/2d | Tilemap/Composite Collider 2D, formas físicas, estruturas, organização e desempenho. |
| https://github.com/Unity-Technologies/2d-extras | Rule Tiles, auto-tiling, paredes, esquinas e organização de objetos. |
| https://github.com/Owen-Jackson/2D-Procedural-Tilemap-Tool | Salas, conexões, corredores e distribuição modular dos espaços. |
| https://docs.unity.com/en-us/engine/6000.6/manual/unity2d/sprite/sort-sprites/2d-renderer-sorting | Sorting, profundidade aparente e integração personagem/estruturas. |

São referências educacionais, não dependências obrigatórias. Não instalar plugins, copiar código ou migrar arquitetura sem necessidade demonstrada, compatibilidade e licença adequada. URLs são as fornecidas; confirmar sua disponibilidade na sessão de estudo.

## 2. Diagnóstico dos dez mapas

1. Rua das Cinzas.
2. Distrito do Vinil.
3. Catedral do Sangue.
4. Laboratório Eclipse.
5. Jardim dos Ecos.
6. Observatório Vespera.
7. Cripta dos Primeiros.
8. Catedral do Crepúsculo.
9. Véu Profundo.
10. Coração do Eclipse.

Confirmar nomes, objetivos, chefes e eventos com o GDD canônico atual. O plano de 05/11 contém dados superados e não redefine a história.

Para cada mapa, registrar dimensões e área utilizada; vazios e viagens sem acontecimentos; arquitetura sem função; paredes/objetos atravessáveis; elementos aparentemente sólidos sem colisão; bloqueios invisíveis; duplicação/desconexão de objetos; escala, perspectiva e sobreposição; localização dos objetivos; espaço/distribuição das arenas e continuidade ambiental.

Classificar com evidências: **Preservar / Ajustar / Remodelar parcialmente / Reconstruir extensamente**. Não decidir reconstruir tudo por padrão.

## 3. Colisões: prioridade técnica

Investigar a implementação real de `DarklightPaintedMap.BuildGeometry`, `DarklightMapDefinition`, `DarklightNavigation`, `DarklightSorting`, `DarklightMapOcclusion`, `Beta10MapValidation` e `DarklightDeliveryCollisionTests`.

O projeto já usa mapas ilustrados, células caminháveis e colisores combinados: corrigir essa implementação antes de propor substituição por Tilemaps. Comparar imagem e geometria física; distinguir atravessamento físico de sobreposição causada por sorting/oclusão.

- Paredes, pilares e volumes sólidos devem bloquear.
- Copas podem permitir circulação atrás; troncos sólidos devem bloquear.
- Decoração pequena pode ser atravessável quando coerente.
- Telhados/frentes precisam de sorting e oclusão corretos.
- Portais/interativos usam triggers próprios, separados de bloqueios.
- Evitar caixas retangulares enormes sobre espaço visualmente livre.

Investigar a falha do capítulo zero em `Docs/Darklight/RELATORIO-TESTES-FINAIS-20261115.md`. Confirmar a cápsula `(0.42, 0.28)`, offset `(0, 0.14)`, contra o jogador real e reconciliar planejamento de rotas, volume físico e deslizamento. Não relaxar tolerâncias para fabricar aprovação. A falha anterior não validou as demais nove regiões.

## 4. Escala e densidade

Identificar espaços que possam ser encurtados/reorganizados, virar praças/pátios/corredores úteis, receber arquitetura relevante, tornar-se rotas secundárias com propósito ou sair da área caminhável sem destruir a composição. Priorizar densidade de acontecimentos sobre tamanho bruto.

Cada mapa deve ter entrada reconhecível, caminho principal coerente, marcos memoráveis, encontros distribuídos, alternativas úteis, checkpoints adequados e aproximação natural do chefe. Preservar espaço para oito direções, esquivas, Katana, Punhos/Grevas e combate aéreo; evitar corredores que inviabilizem combate.

Não mudar resolução, PPU, escala global ou posições canônicas sem verificar todas as coordenadas dependentes. Preferir compactar a área efetivamente jogável preservando imagem-base e bons trechos.

## 5. Coerência visual e narrativa

Cada ambiente deve ter função dentro de Vespera, sem objetos aleatórios: Rua das Cinzas com urbanismo plausível; Distrito do Vinil com comércio musical e espaços próprios; Catedral do Sangue com arquitetura cerimonial; Laboratório Eclipse com salas operacionais e equipamentos lógicos; Jardim dos Ecos com caminhos naturais, vegetação e ruínas. Aplicar o mesmo cuidado aos outros cinco conforme o GDD.

Revisar casas sem acesso, muros sem continuidade, escadas sem destino, árvores/estruturas arbitrárias, caminhos inúteis, objetos flutuantes e escalas incompatíveis. Preservar Gothic Eclipse, iluminação, chuva, névoa e efeitos aprovados; ajustar somente o necessário.

## 6. Quatro camadas conceituais

Consultar `Docs/Darklight/PLANO-RECRIACAO-10-MAPAS-EM-CAMADAS.md` e atualizar instruções históricas incompatíveis.

1. **Planta baixa:** piso caminhável, caminhos, praças, limites e geometria navegável.
2. **Arquitetura/colisões:** paredes, casas, móveis grandes, obstáculos, telhados, frentes; colisores e sorting associados.
3. **Interativos:** preservar spawns, NPCs, relíquias, checkpoints, portais, objetivos e regras de ativação.
4. **Gameplay/atmosfera:** arenas, inimigos, selos, bossfights, iluminação e partículas.

Separação conceitual não obriga trocar a implementação por Tilemaps. Preservar/reorganizar/complementar conforme necessidade comprovada.

## 7. Composição e orientação

Tornar compreensíveis rotas, objetos sólidos, alternativas, início de arenas e direção provável dos objetivos. Guiar por luz, arquitetura, composição e landmarks; evitar poluição visual; equilibrar abertos/estreitos/interesse. A câmera deve permitir planejamento sem precisar mostrar o mapa inteiro. Atualizar o minimapa conforme a geometria definitiva.

## 8. Validação de cada mapa

- **Física:** sólidos bloqueiam, esquinas não prendem, deslizamento funciona e não há bloqueios invisíveis injustificados.
- **Navegação:** spawn, arenas, checkpoints, NPCs, relíquia e portal alcançáveis; ida/volta quando cabível; IA fora de estruturas; saída após batalhas/transições.
- **Combate:** espaço suficiente, ataques/esquivas não atravessam paredes indevidas, chefes conseguem circular.
- **Visual:** ordenação/oclusão corretas, sem cortes graves, sprites nítidos e rotas legíveis sob a iluminação.
- **Progressão:** saves/checkpoints válidos, objetivos acessíveis, transições funcionando e fase concluível.

Testes isolados, silenciosos e sem tocar dados reais. Gerar antes/depois e, quando útil, sobreposição de depuração da colisão. Compilar não comprova validação de mapa.

## 9. Execução incremental

**A — Diagnóstico geral:** auditar dez mapas e priorizar problemas.

**B — Piloto:** Rua das Cinzas; melhorar tamanho, composição, colisão e rotas sem descaracterizar. Validar física, gameplay, objetivos e aparência antes de generalizar.

**C — Generalizar:** reutilizar soluções sistêmicas, evitando corrigir o mesmo bug dez vezes.

**D — Mapas 2 a 10:** avançar sequencialmente, em lotes pequenos com progresso documentado.

**E — Final:** progressão completa e build Windows somente quando autorizadas e com cota suficiente. Se a reserva chegar, parar com checkpoint funcional e mapas concluídos claramente listados. Não substituir versão funcional por reconstrução incompleta.

## 10. Preservação e entrega

Não regenerar Fabius Normal/alterar pivôs; não mudar combate/HUD fora de compatibilidade estrita; não reintroduzir Moto/Serras, formas/chefes/sistemas descartados; não usar placeholders permanentes; não alterar história/GDD sem autorização; não destruir originais/saves/backups; não instalar bibliotecas desnecessárias; não usar Git destrutivo; não gastar grande cota recriando o que já funciona.

Por mapa registrar problemas originais, alterações, melhorias visuais, colisão, testes reais, capturas antes/depois, pendências e commit/push efetivamente confirmados.

Critério final: mapas coerentes, compactos quando necessário, navegáveis, interessantes, estáveis e fiéis a DARKLIGHT; sem barreiras invisíveis injustas, atravessamento de sólidos ou rotas improvisadas obrigatórias.

## Estado ao salvar

**Aguardando a próxima sessão e a fase autorizada.** Nenhuma pesquisa desses links, auditoria dos dez mapas ou alteração de mapa foi executada para este pedido em 17/11. A sessão corrente permanece restrita ao loading, seus mascotes, capturas e documentação.
