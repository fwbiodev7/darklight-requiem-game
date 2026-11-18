# DARKLIGHT: REQUIEM — PROMPT MESTRE DE RETOMADA APÓS RESET SEMANAL

> **Atualização final sobre build, 17/11:** usuário **cancelou a build nesta sessão**, para continuar nela amanhã. Prosseguir nas artes de Punhos com máxima fidelidade e preservar WalkS frontal aprovado; encerrar lotes e fazer commit local/relatório aos **15% restantes da janela**. **Sem novos testes e sem push.** Não executar a antiga instrução de build aos25% nem incluir build inexistente no relatório.

> **Restrição mais recente, 17/11:** usuário pediu **sem testes em tudo** nesta rodada. Não executar novas suítes/Play/smoke tests; parar a rodada pendente própria e preservar evidências anteriores. A build Windows explicitamente pedida aos25% permanece autorizada e terá a compilação necessária, sem alegar validação de gameplay/menus. Relatório deve distinguir implementado, revisado, compilado e testado anteriormente.

> **Encerramento vigente desta sessão — 17/11:** usuário dispensou publicação e testes dos menus (avaliação manual será dele). Gerar build Windows ao chegar a **25% restantes da janela de cinco horas**; aos **15%**, relatório e commit local conjunto das mudanças prontas e da build, **sem push**. Manter reserva semanal mínima de 20% e exclusão de MP3 privados/saves/caches/alterações alheias. A inclusão local da build é uma exceção expressa para esta entrega; não autoriza redistribuição pública de binários ou músicas.

> **Ordem mais recente, autorizada pelo usuário em 17/11/2026:** iniciar agora pelo **redesign de todos os menus**, depois **Combate 2.0/combos**, depois **bossfight Lucius 2.0**, seguindo as demais frentes deste plano. Esta ordem troca a precedência histórica das Fases 1 e 2: executar primeiro o escopo autorizado de menus da Fase 2, validar, então Fase 1 e Fase 3. Não declarar HUD/diálogos concluídos por terminar menus. O loading gótico e os dois mascotes foram aprovados, testados (2 EditMode + 2 PlayMode) e publicados em `c56d829`; a documentação de Map Remaster está em `a441778`. Preservar esses resultados e não refazê-los. Cota real consultada novamente nesta retomada: 100% semanal disponível; manter reserva mínima de 20% a partir daqui. A restrição de data do Map Remaster abaixo continua vigente.

> **Prioridade registrada em 17/11/2026 — Map Remaster 2.0:** ler `Docs/Darklight/MAP-REMASTER-2-PRIORIDADE-20261119.md` na próxima sessão. Não iniciar essa frente antes de **19/11/2026 às 21:11 America/Sao_Paulo** e da conferência da cota real (reservas: 20% semanal e 10% da janela). Planejar a frente prioritária de mapas dentro da sequência oficial de oito fases, sem antecipar fases ou dispensar autorizações. Primeiro estudar as referências e diagnosticar os dez mapas; depois piloto Rua das Cinzas, correções sistêmicas e avanço sequencial. Nenhuma remasterização de mapa foi iniciada ao salvar este pedido.

**Engine:** Unity 6000.6.0f1  
**Workspace:** `C:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main`

**Missão:** Evoluir o combate, reformular HUD e menus, aprimorar Lucius e concluir as melhorias visuais e narrativas planejadas, preservando o estado funcional do jogo.

**Regra principal:** ECONOMIZAR AO MÁXIMO OS TOKENS SEMANAIS DO CODEX.

---

## 0. PREPARAÇÃO E SEGURANÇA

O teste manual de 14/11/2026 confirmou que DARKLIGHT já apresenta gameplay divertido, combate funcional, transformações jogáveis, bons gráficos e colisões aparentemente corrigidas.

Preservar esse progresso.

Antes de modificar qualquer arquivo:

1. Ler `AGENTS.md` e o checkpoint técnico mais recente.
2. Consultar a cota real do Codex, quando possível.
3. Confirmar o reset semanal, sem confundi-lo com o reset de cinco horas.
4. Inspecionar branch, commits, alterações locais e backups.
5. Verificar o espaço livre em disco.
6. Auditar os sistemas existentes e identificar o que pode ser reutilizado.
7. Localizar as referências visuais aprovadas da HUD, menus, Fabius e Lucius.

Não utilizar comandos Git destrutivos, sobrescrever alterações não auditadas ou apagar assets históricos.

A sessão anterior registrou aproximadamente 2,8 GiB livres e problemas no BuildPipeline. Verificar a situação atual antes de gerar builds.

### Economia obrigatória

- Preservar pelo menos 20% da cota semanal, sempre que possível.
- Reutilizar animações, sprites, controladores e componentes.
- Evitar auditorias repetidas e agentes paralelos desnecessários.
- Preferir implementações pequenas, testáveis e reversíveis.
- Não gerar dezenas de sprite sheets sem necessidade.
- Não gastar tokens com reformulações que não tragam melhoria perceptível.
- Interromper novos lotes antes de comprometer a reserva semanal.

**Nenhuma fase posterior deve começar automaticamente sem verificar a conclusão e a autorização para prosseguir.**

---

# FASE 1 — COMBATE 2.0: PUNHOS E KATANA

**PRIORIDADE MÁXIMA DA RETOMADA.**

## 1.1 Remoção definitiva de Moto e Serras

Fabius passa a utilizar exclusivamente:

- Katana.
- Punhos/Grevas Abissais.

Desativar Moto e Serras no gameplay, arsenal, HUD, seleção de armas, ataques, controles e transformações.

Garantir compatibilidade com saves antigos que contenham essas armas.

Preservar arquivos históricos e backups. Não apagar fontes ou sprites desnecessariamente.

## 1.2 Nova postura dos Punhos/Grevas

**Referência:** Dante utilizando o estilo de combate de manoplas e grevas em Devil May Cry 5, especialmente a postura de combate corpo a corpo.

Criar uma identidade de luta completamente distinta da Katana.

Implementar ou adaptar:

- Guarda com braços levantados.
- Pernas posicionadas para combate.
- Idle próprio dos Punhos.
- Caminhada de combate com postura consistente nas oito direções.
- Socos rápidos e pesados.
- Chutes, joelhadas e ataques giratórios.
- Ataques carregados.
- Finalizações com impacto.
- Transições naturais entre movimentos.

Evitar simplesmente substituir a espada por Punhos mantendo a mesma postura.

A postura deve transmitir a identidade de um lutador experiente.

**Regra:** reutilizar os sprites existentes sempre que possível. Criar somente os quadros indispensáveis, preservando proporções, pixel art e identidade visual de Fabius.

## 1.3 Combos terrestres e aéreos — Punhos

Expandir a variedade de movimentos:

- Sequências rápidas de socos.
- Combos misturando socos e chutes.
- Uppercut que lança o inimigo ao ar.
- Perseguição aérea.
- Combos de socos e chutes durante o salto.
- Golpes descendentes.
- Finalizadores que arremessam o inimigo ao chão.
- Cancelamentos controlados entre ataques, esquivas e movimentação.

Garantir hitboxes coerentes, feedback de impacto, animações fluidas e recuperação adequada.

## 1.4 Combos terrestres e aéreos — Katana

Inspirar‑se na precisão e velocidade do combate de Vergil em DMC5, mantendo identidade própria.

Implementar:

- Cortes rápidos encadeados.
- Combos alternativos.
- Cortes ascendentes.
- Lançamento de inimigos ao ar.
- Perseguição aérea.
- Cortes consecutivos durante o combate aéreo.
- Golpes descendentes.
- Contra‑ataques.
- Finalizadores com maior impacto visual.

As duas armas devem oferecer estilos distintos:

**Punhos:** agressividade, impacto e combate físico.

**Katana:** precisão, velocidade, alcance e cortes elegantes.

## 1.5 Serafim Carmesim — combos especiais

A transformação Serafim Carmesim deve ampliar a profundidade dos combos existentes.

Exemplo com Punhos:

1. Fabius aplica um uppercut.
2. Lucius é lançado ao ar, quando estiver vulnerável.
3. Fabius acompanha o lançamento.
4. O jogador executa uma sequência aérea.
5. Fabius finaliza com um golpe descendente poderoso.

Criar equivalente com Katana, utilizando cortes ascendentes e sequências aéreas.

Estudar também a possibilidade de combos híbridos:

**Punhos → Uppercut → Perseguição aérea → Troca para Katana → Cortes aéreos → Finalização.**

Implementar a troca aérea somente se a arquitetura atual permitir sem refatoração excessiva.

Aplicar essas mecânicas às demais formas quando compatíveis, preservando as particularidades de cada transformação.

Não criar combos infinitos. Respeitar gravidade, resistência, recuperação, limites de juggle e invulnerabilidades.

## 1.6 Validação do combate

Testar:

- Posturas e animações nas oito direções.
- Troca entre Katana e Punhos.
- Combos terrestres.
- Combos aéreos.
- Hitboxes e colisões.
- Interrupções e esquivas.
- Transformações.
- Compatibilidade com inimigos e bosses.
- Ausência de combos infinitos.

Preservar o sistema de combate que já funciona.

---

# FASE 2 — HUD 2.0, MENUS 2.0 E DIÁLOGOS

**SEGUNDA PRIORIDADE.**

Usar a última referência visual aprovada pelo usuário.

Se a referência não estiver disponível, solicitar o arquivo original em vez de improvisar outro design.

## 2.1 Direção artística

- Pixel art nítida, estilo 16/32‑bit.
- Dark fantasy gótico.
- Molduras pretas com detalhes roxos e vermelhos.
- Elementos discretos e organizados.
- Fontes pixeladas legíveis.
- Compatibilidade com Gothic Eclipse.
- Interface compacta, sem excesso de ornamentos.

Preservar pixel perfect, PPU, escala e proporções aprovadas.

## 2.2 HUD de Fabius

Reformular o painel superior esquerdo com:

- Retrato pixel art.
- Nome FABIUS.
- Barra vermelha de HP com valores.
- Barra de recurso existente.
- Indicador SANGUE DO ABISMO.
- Transformação ativa.
- Tempo restante da transformação, quando aplicável.

Auditar sistemas de sangue, fúria ou recursos já existentes.

Se houver sistema equivalente, integrar a HUD ao recurso real.

Se não houver, preparar somente a interface sem inventar regras de gameplay.

## 2.3 Bossbar de Lucius

Criar uma bossbar digna do confronto final:

- Centralizada no topo.
- Nome LUCIUS — ARCONTE, conforme o estado correspondente.
- Barra vermelha com moldura gótica pixelada.
- Indicador da fase atual.
- Detalhes discretos violeta e esmeralda.
- Animação de perda de HP.
- Transições entre fases.

Nesta fase, alterar apenas a apresentação visual. O balanceamento será trabalhado na Fase 3.

## 2.4 HUD de combos

Melhorar o indicador de estilo:

- Classificações D, C, B, A, S e SS.
- Contador de acertos.
- Feedback visual dos combos.
- Pequenas animações de progressão.
- Posicionamento compacto no canto direito.

Reutilizar as regras reais de pontuação, sem inventar multiplicadores.

## 2.5 Minimapa

Preservar o sistema funcional.

Melhorar molduras, ícones, contraste e legibilidade, mantendo os controles de zoom e mapa.

## 2.6 HUD de armas

Mostrar somente Katana e Punhos/Grevas Abissais.

Utilizar ícones pixelados e destaque discreto da arma equipada.

## 2.7 Caixas de diálogo

Implementar:

- Caixa inferior preta semitransparente.
- Moldura gótica pixelada.
- Retrato do personagem à esquerda.
- Nome destacado.
- Texto branco legível.
- Efeito opcional de digitação.
- Indicador de continuação.
- Transições suaves.

Preservar diálogos e eventos narrativos existentes.

Verificar especialmente Fabius, Lucius e Nocthar.

Evitar que diálogos cubram os personagens durante combate ativo.

## 2.8 Menu principal

- Logo oficial.
- Fundo atmosférico com eclipse.
- Estética gótica pixel art.
- Botões organizados.
- Animações discretas.
- Navegação funcional.

## 2.9 Menu de pausa

Preservar as opções:

- Continuar.
- Arsenal.
- Mapa.
- Configurações.
- Menu principal.

## 2.10 Arsenal

Exibir apenas Katana e Punhos/Grevas.

Apresentar informações e características existentes das armas.

Não reintroduzir Moto, Serras, artefatos ou coleções.

## 2.11 Configurações

Preservar:

- Áudio.
- Vídeo.
- Efeitos.
- Controles.
- Resolução.
- Tela cheia.

Aplicar a mesma identidade visual às telas adicionais existentes.

## 2.12 Testes da interface

Verificar:

- Resoluções 1280×720 e 1920×1080.
- Navegação por teclado e controle.
- Sobreposições.
- Legibilidade em cenários claros e escuros.
- HUD nas transformações Normal, Ruptura e Serafim Carmesim.
- Menus, diálogos, arsenal e minimapa.

Evitar substituir a arquitetura inteira da UI.

---

# FASE 3 — BOSSFIGHT DE LUCIUS 2.0

**TERCEIRA PRIORIDADE.**

Aprimorar o confronto final entre Fabius e Lucius.

**Referências:** Vergil vs. Dante em DMC5 e duelos cinematográficos de Ghost of Tsushima.

Objetivos:

- Aumentar a dificuldade de forma justa.
- Melhorar a IA e a tomada de decisões.
- Criar maior variedade de ataques e contra‑ataques.
- Melhorar esquivas e defesa.
- Adaptar Lucius aos novos combos aéreos.
- Implementar respostas aos lançamentos e perseguições.
- Evitar que Lucius fique permanentemente vulnerável.
- Refinar transições entre fases.
- Melhorar a sensação de duelo entre dois personagens poderosos.
- Integrar a música já existente, quando disponível e adequada.

Priorizar comportamento inteligente em vez de simplesmente aumentar HP ou dano.

Preservar a identidade visual e os movimentos aprovados de Lucius.

Validar dificuldade, telegráficos, recuperação, hitboxes e oportunidades de reação.

---

# FASE 4 — REDESIGN DOS INIMIGOS E BOSSES

Melhorar os inimigos comuns e os bosses anteriores a Lucius.

Prioridades:

- Design visual mais consistente.
- Sprites proporcionais aos personagens principais.
- Melhorias nas animações de caminhada.
- Ataques com antecipação, impacto e recuperação.
- Hitboxes mais coerentes.
- Maior variedade de comportamento.
- Efeitos de dano e morte aprimorados.

Preservar as identidades e os designs aprovados.

Evitar recriar todas as animações do zero.

Priorizar os inimigos e bosses com problemas perceptíveis no gameplay.

---

# FASE 5 — GOTHIC ECLIPSE: EVOLUÇÃO GRÁFICA

Continuar o polimento gráfico já iniciado.

Preservar os avanços existentes:

- Chuva em camadas.
- Névoa.
- Luar.
- Reflexos discretos.
- Iluminação dinâmica.
- Presets atmosféricos por região.

Melhorar:

- Coerência visual dos cenários.
- Contraste dos personagens.
- Atmosfera dark fantasy.
- Iluminação das arenas.
- Integração entre sprites e ambientes.
- Qualidade visual geral.

Não refazer os dez mapas sem necessidade.

Não modificar câmera, PPU, pivôs, colisões ou escalas aprovadas sem justificativa técnica.

Priorizar otimização e estabilidade.

---

# FASE 6 — CUTSCENES CINEMATOGRÁFICAS

**Referências principais:** Phantom Blade Zero e Ghost of Tsushima.

Utilizar o GDD oficial como autoridade narrativa.

## 6.1 Phantom Blade Zero

Inspiração para:

- Introdução sombria.
- Câmeras dinâmicas.
- Combates coreografados.
- Cortes cinematográficos.
- Transições entre narrativa e gameplay.
- Sensação de perigo e intensidade.

## 6.2 Ghost of Tsushima

Inspiração para:

- Enquadramentos elegantes.
- Silêncio dramático.
- Duelos tensos.
- Uso da atmosfera e iluminação.
- Composição visual.
- Narrativa ambiental.

## 6.3 Aplicação em DARKLIGHT

Priorizar:

1. Introdução do jogo e tutorial narrativo.
2. Momentos importantes dos capítulos.
3. Apresentação dos bosses.
4. Revelações sobre Fabius e Lucius.
5. Introdução do confronto final.
6. Transições entre fases da bossfight.
7. Desfecho narrativo conforme o GDD.

Integrar trilhas, diálogos, animações e efeitos existentes.

Utilizar Timeline e Cinemachine quando forem adequados.

Não copiar cenas diretamente das referências.

Manter a identidade própria de DARKLIGHT e sua estética pixel art.

---

# FASE 7 — POLIMENTO, NAVEGAÇÃO E ÁUDIO

Após as fases anteriores:

- Melhorar referências e orientação ao longo dos mapas.
- Refinar indicações de objetivos.
- Integrar músicas já salvas nas principais bossfights.
- Ajustar volumes e transições musicais.
- Corrigir bugs identificados nos testes.
- Aprimorar feedback de golpes e movimentação.
- Revisar performance e uso de memória.
- Otimizar efeitos e partículas.
- Verificar a estabilidade das dez fases.

Não adicionar sistemas complexos desnecessários.

---

# FASE 8 — TESTES, BUILD E ENTREGA

Ao concluir cada lote:

1. Verificar compilação.
2. Executar testes direcionados.
3. Corrigir regressões.
4. Revisar `git diff`.
5. Registrar checkpoint objetivo.

Antes da build final:

1. Confirmar espaço suficiente em disco.
2. Executar testes EditMode e PlayMode relevantes.
3. Validar as duas armas e as transformações.
4. Verificar menus e HUD.
5. Testar Lucius e os bosses disponíveis.
6. Verificar compatibilidade com saves.
7. Gerar build Windows.
8. Inspecionar logs e arquivos gerados.
9. Executar a build quando possível.
10. Criar commit seguro.

Não apagar builds, saves ou caches sem autorização.

Não executar push sem autorização.

Não declarar uma build validada apenas pelo resultado `Succeeded` do Unity.

## RELATÓRIO OBRIGATÓRIO

Ao encerrar a sessão, informar:

- Etapas concluídas.
- Arquivos e sistemas alterados.
- Testes executados e resultados.
- Bugs corrigidos.
- Pendências.
- Caminho da build, se gerada.
- Branch e hash do commit.
- Cota semanal restante, se consultável.
- Próxima tarefa recomendada.

---

# REGRAS FINAIS

**ORDEM OFICIAL INALTERÁVEL SEM AUTORIZAÇÃO:**

1. Combate 2.0 — Punhos, Katana e combos.
2. HUD 2.0, Menus 2.0 e diálogos.
3. Bossfight de Lucius 2.0.
4. Redesign dos inimigos e bosses.
5. Gothic Eclipse.
6. Cutscenes cinematográficas.
7. Navegação, áudio e polimento.
8. Validação, build e entrega.

Testes e checkpoints devem ocorrer ao longo de todas as fases, não somente no final.

Não iniciar todas as fases simultaneamente.

Não antecipar tarefas futuras enquanto a fase atual apresentar problemas críticos.

**A implementação começa somente após a confirmação do reset semanal e autorização do usuário.**

**OBJETIVO FINAL:** fazer DARKLIGHT: REQUIEM evoluir para um hack and slash 2D dark fantasy com combate profundo, interface refinada, bosses memoráveis, apresentação cinematográfica e excelente estabilidade — sem desperdiçar a cota semanal do Codex.
