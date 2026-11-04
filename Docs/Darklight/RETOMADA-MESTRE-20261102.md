# DARKLIGHT: REQUIEM — RETOMADA MESTRE 02/11/2026
## Redesign completo, Gothic Eclipse, Lucius, transformações, IA 2.0 e build jogável

### 1. NOVA ORDEM SUPREMA DO CRIADOR

Retomar automaticamente DARKLIGHT: REQUIEM neste mesmo chat, em **02/11/2026 às 14:19:50 (America/Sao_Paulo / 17:19:50 UTC)**.

Workspace:
`C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main`

**ATUALIZAÇÃO CRÍTICA:** o usuário informou ter recebido uma redefinição semanal gratuita do Codex. Portanto, as restrições anteriores motivadas pelo esgotamento iminente dos tokens deixam de justificar a suspensão do escopo.

Não presumir uma porcentagem exata de saldo. Consultar o saldo real quando acessível.

### DECISÕES QUE SUBSTITUEM AS ANTERIORES

- BUILD NOVAMENTE AUTORIZADA.
- FABIUS NORMAL continua sendo a prioridade absoluta inicial.
- GOTHIC ECLIPSE passa a ser a segunda etapa.
- LUCIUS NORMAL está novamente autorizado, mas somente após a etapa gráfica.
- TRANSFORMAÇÕES de ambos estão novamente autorizadas.
- COMBATE IA 2.0 passa a ser a quinta etapa principal.
- Ao final, produzir uma build jogável validada, se as condições técnicas permitirem.

### ORDEM OBRIGATÓRIA

1. FINALIZAR FABIUS NORMAL.
2. IMPLEMENTAR ATMOSFERA DARK FANTASY E ATUALIZAÇÕES GRÁFICAS.
3. FINALIZAR LUCIUS NORMAL.
4. IMPLEMENTAR TRANSFORMAÇÕES DE FABIUS E LUCIUS.
5. FINALIZAR COMBATE IA 2.0.
6. VALIDAR, OTIMIZAR, GERAR BUILD JOGÁVEL E PUBLICAR.

**Não trocar a ordem por conveniência de geração.**

É permitido preparar dependências técnicas independentes, desde que isso não retire recursos da etapa prioritária nem antecipe integração que possa causar conflitos.

---

# 2. RETOMADA DO ESTADO REAL

Antes de trabalhar, ler incrementalmente:

- `Docs/Darklight/FECHAMENTO-20261102.md`
- `Docs/Darklight/PENDENCIAS-ANIMACOES-20261102.md`
- `RETOMADA-20261102.md`, se presente
- `outputs/retomada-20261102/continuation-state.json`
- `outputs/retomada-20261102/implementation-receipt.json`
- `outputs/retomada-20261102/art-integration.json`
- `outputs/retomada-20261102/final-git-receipt.json`
- `Docs/Darklight/GOTHIC-ECLIPSE-PLANO-20261101.md`, quando chegar à segunda etapa

Os recibos mais recentes prevalecem sobre checkpoints históricos.

Verificar Unity, processos Git, operações artísticas e subagentes já ativos. Não iniciar operações concorrentes ou repetir trabalhos em andamento.

Preservar alterações alheias.

Não repetir a auditoria das 176 figuras.

Não refazer artes aprovadas sem defeito demonstrável.

Unity canônica: **6000.6.0f1**.

Escala visual: **WorldVisualScale 1.325**.

Preservar anatomia, proporções, rosto, paleta e identidade dos personagens.

---

# 3. ETAPA 1 — FINALIZAR FABIUS RÉQUIEM NORMAL

## PRIORIDADE IMEDIATA: IMPLEMENTAR O MATERIAL JÁ CRIADO

Existem 15 clips, 60 figuras e sete folhas adicionados aos arquivos e catálogo, mas ainda não validados na Unity.

### 3.1 Importação obrigatória

**Katana Heavy**
- Oito direções.
- 32 figuras.
- N5 e NE9 corrigidos.
- Preservar os 22 quadros anteriores.
- Tempos 100/110/100/100 ms.
- Startup 1 / active 1 / recovery 2.

**Katana Walk**
- NE/E/SE.
- 12 figuras.
- E1 e NE4 corrigidos.
- Atlas compacto 1726×1799.
- Preservar resolução e densidades por quadro.

**Punhos Walk**
- N/NE/NW/SE.
- 16 figuras.
- Correções SE preservadas.
- S anteriormente aprovado e intocado.

Importar via sistema existente, sem duplicar registros, PNGs ou páginas.

Validar:
- Pivôs e proporções.
- Pixels por unidade.
- Empunhadura e alinhamento.
- Contato dos pés.
- Animação contínua.
- Transições.
- Retorno ao idle.
- Loop e timing.
- Funcionamento no Animator e na gameplay.

Os 144 testes EditMode e 30 PlayMode anteriores NÃO validam esses novos sprites.

Executar testes novos e registrar os resultados.

**GATE:** os 15 clips devem ser efetivamente integrados e testados antes de iniciar grandes lotes artísticos.

## 3.2 Finalizar Punhos Walk

Corrigir as três direções pendentes:

- E
- W
- SW

Problema anterior: repetição da mesma perna, contato lateral incorreto e caminhada pouco natural.

Usar a referência de `ArtSources/WeaponRedesign20261102`.

Manter:
- Passada baixa.
- Alternância real dos pés.
- Braços coerentes com a direção.
- Anatomia inteira.
- Identidade visual.
- Silhueta e proporções aprovadas.

Corrigir quadros inteiros defeituosos, não montar sprites a partir de partes incompatíveis.

Não espelhar assimetrias.

Não substituir as cinco direções aprovadas.

Integrar e testar as oito direções completas.

## 3.3 Completar ataques e técnicas

### Katana

Concluir as oito técnicas adicionais ainda sem animação própria completa.

Preservar:
- Leves 1/2/3.
- Heavy.
- Caminhadas.
- Esquivas aprovadas.

Cada técnica deve ter antecipação, ação, impacto, recuperação e transições apropriadas.

### Punhos

Concluir oito técnicas adicionais, preservando os quatro golpes existentes.

Priorizar:
- Peso corporal.
- Movimentação correta dos braços.
- Contato convincente.
- Continuidade entre combos.
- Recuperação sem teleporte de pose.

### Moto

Concluir as 19 técnicas pendentes de cobertura visual própria.

Preservar técnicas e movimentos já aprovados.

As animações devem respeitar:
- Geometria da moto.
- Posição de Fabius.
- Perspectiva.
- Direção.
- Movimento das rodas.
- Contato e impacto.
- Entrada e saída dos ataques.

Não usar uma pose estática repetida para simular movimento complexo.

## 3.4 Mobilidade

Concluir:

- Caminhada das serras em oito direções.
- Transições entre corrida/caminhada/idle.
- Partida da moto.
- Frenagem.
- Curvas.
- Aceleração.
- Mudanças de direção.
- Transições entre golpes e locomoção.

## 3.5 Sistemas adicionais

Completar nas três armas:

- Defesa.
- Parry.
- Carregamento.
- Dano.
- Morte.
- Recuperação.
- Reação a impacto.
- Transições entre estados.

Integrar os combos aéreos:

- Lançamento.
- Perseguição.
- Combinação de golpes.
- Arremesso.
- Finalização.
- Retorno ao solo.

Respeitar os limites anti-infinito já definidos.

## 3.6 Critério de conclusão do Fabius Normal

A etapa estará concluída somente quando:

1. As animações necessárias estiverem presentes.
2. Os recursos estiverem efetivamente integrados.
3. O personagem puder executar as ações na Unity.
4. As três armas funcionarem sem fallback visual indevido.
5. Os ataques possuírem transições coerentes.
6. As oito direções estiverem corretas onde exigidas.
7. Não houver erros críticos de Animator ou gameplay.
8. A identidade visual estiver preservada.

Não confundir quantidade de arquivos gerados com conclusão funcional.

---

# 4. ETAPA 2 — DARK FANTASY + ATUALIZAÇÕES GRÁFICAS

Após estabilizar Fabius Normal, implementar o plano **GOTHIC ECLIPSE**.

**ORDEM EXPRESSA DO CRIADOR: NÃO REFAZER MAPAS.**

A atmosfera deve ser aplicada sobre os cenários existentes.

## 4.1 Atmosfera

Implementar:

- Iluminação gótica.
- Sombras ambientais.
- Névoa atmosférica.
- Profundidade visual.
- Partículas suspensas.
- Brilhos sobrenaturais.
- Contraste cinematográfico.
- Variações de iluminação por região.
- Pós-processamento compatível com pixel art.

Evitar que o jogo fique excessivamente escuro ou que os inimigos deixem de ser visíveis.

Preservar leitura dos ataques, projéteis, obstáculos e elementos de interação.

## 4.2 Melhorias visuais gerais

Aprimorar:

- Harmonia das paletas.
- Integração dos personagens aos cenários.
- Efeitos de profundidade.
- Transições visuais.
- Impactos de combate.
- Feedback de dano.
- Partículas de movimento.
- Partículas de defesa/parry.
- Carregamentos de energia.
- Efeitos das transformações, apenas quando chegar à etapa 4.
- Acabamento gráfico das interfaces, HUD e indicadores, sem reconstrução desnecessária.

Utilizar shaders, luzes, partículas, overlays e pós-processamento quando forem adequados ao pipeline existente.

Não comprometer a fidelidade pixel art com filtros que borrem sprites.

## 4.3 Personagens e cenário

Revisar a integração visual do personagem ao ambiente.

Quando aplicável, corrigir posicionamentos inadequados de NPCs, incluindo a personagem anteriormente observada sobre um telhado, para que permaneça em posição válida próxima ao player, sem atravessar colisores ou interferir na gameplay.

Fazer isso apenas se a situação persistir no estado atual do projeto.

## 4.4 Performance

- Evitar excesso de luzes simultâneas.
- Controlar overdraw de partículas.
- Reutilizar materiais quando viável.
- Evitar alocações desnecessárias por frame.
- Monitorar FPS e memória.
- Verificar qualidade gráfica em cenas representativas.

**GATE:** alcançar uma atmosfera dark fantasy consistente e funcional, sem modificar desnecessariamente a estrutura dos dez mapas existentes.

---

# 5. ETAPA 3 — CRIAR E IMPLEMENTAR LUCIUS NORMAL

Agora está autorizado retomar o antagonista principal.

Lucius é o irmão gêmeo de Fabius.

A direção visual deve respeitar a última referência aprovada, incluindo eventuais revisões posteriores encontradas nos arquivos.

## 5.1 Identidade

- Mesmo padrão de escala, altura e proporções de Fabius.
- Silhueta compatível com o parentesco entre os dois.
- Identidade facial própria, mas reconhecivelmente semelhante.
- Cabelo claro/prateado, conforme referência final.
- Olhos azuis.
- Sobretudo predominantemente preto/azul.
- Visual dark fantasy.
- Armas próprias, visualmente distintas das de Fabius.

Não adicionar armaduras, capuzes ou acessórios rejeitados anteriormente.

Respeitar o redesign final mais recente em vez de fichas antigas.

## 5.2 Animações base

Implementar, conforme necessidades reais do personagem:

- Idle.
- Caminhada.
- Corrida/perseguição.
- Mudanças de direção.
- Esquiva.
- Defesa.
- Parry.
- Ataques.
- Combos.
- Carregamento.
- Reação a dano.
- Morte.
- Transições e recuperação.

Priorizar oito direções nos sistemas que exigirem cobertura direcional.

Criar clips, sprites e controladores reutilizando a infraestrutura funcional, mas sem simplesmente reaproveitar a aparência de Fabius.

## 5.3 Integração

- Criar prefab e configuração próprios.
- Verificar colisores e hitboxes.
- Integrar o arsenal próprio.
- Validar pivôs e escala.
- Preservar organização de Assets.
- Preparar a infraestrutura de combate necessária sem antecipar a IA 2.0 completa.

**GATE:** Lucius Normal deve estar visualmente fiel, integrado e funcional antes de começar as transformações.

---

# 6. ETAPA 4 — TRANSFORMAÇÕES DOS DOIS PERSONAGENS

Retomar as transformações aprovadas no escopo reduzido mais recente.

Não restaurar automaticamente formas ou skins descartadas.

## 6.1 Fabius Réquiem

Formas:

1. Normal — já concluída na primeira etapa.
2. Ruptura.
3. Eclipse Absoluto — fusão violeta/prata, anteriormente definida.

No Eclipse Absoluto, respeitar a identidade visual do cabelo branco/prateado e da paleta da transformação.

Não transformar a pele ou olhos de maneira incompatível com a ficha aprovada.

## 6.2 Lucius

Formas:

1. Normal — concluída na terceira etapa.
2. Nexo Fenrir — forma resultante da fusão e redução de escopo anteriormente decididas.

Verificar a ficha visual canônica mais recente antes de gerar artes.

Não recriar separadamente Ascensão, Fenrir e Nexo antigos se permanecer válida a decisão de fusão.

## 6.3 Implementação das formas

Para cada forma:

- Preservar estrutura corporal.
- Preservar anatomia e proporções.
- Alterar os elementos visuais apropriados.
- Criar efeitos de transformação.
- Criar ou adaptar os clips necessários conforme diferenças reais de movimento.
- Integrar estados ao Animator.
- Integrar armas, ataques e efeitos.
- Garantir retorno seguro à forma normal quando permitido.
- Verificar mudanças de forma durante situações especiais.
- Impedir estados quebrados ou animações ausentes.

Não duplicar dezenas de sprites quando um sistema comprovadamente puder reutilizar animações sem prejudicar a fidelidade visual.

**GATE:** transformações acionáveis, estáveis, visualmente corretas e compatíveis com combate e movimentação.

---

# 7. ETAPA 5 — COMBATE IA 2.0

Após os dois personagens e suas transformações funcionarem, implementar a evolução do combate.

Objetivo: combate em tempo real mais inteligente, responsivo, cinematográfico e desafiador.

## 7.1 IA de combate

Implementar ou melhorar:

- Perseguição inteligente.
- Controle de distância.
- Aproximação e recuo.
- Escolha contextual de ataques.
- Reconhecimento de estados visíveis do player.
- Defesas e contra-ataques.
- Reações a ataques repetitivos.
- Variação de combos.
- Mudança de estratégia.
- Recuperação após erro.
- Reposicionamento.
- Esquiva contextual.
- Alternância entre pressão e defesa.

A IA não deve ler informações futuras nem responder instantaneamente a todos os inputs.

Evitar comportamento injusto, combos inevitáveis e vantagens artificiais sem telegraphing.

## 7.2 IA de Lucius

Dar a Lucius comportamento próprio.

Ele deve parecer um rival experiente, não apenas um inimigo comum com vida elevada.

Priorizar:

- Ataques variados.
- Alternância de distância.
- Contra-ataques.
- Combos aéreos.
- Reação a padrões repetidos.
- Cancelamentos permitidos.
- Janelas claras de vulnerabilidade.
- Comportamentos distintos entre Normal e Nexo Fenrir.

Não criar ataques sem possibilidade razoável de defesa ou reação.

## 7.3 Sistema de combate

Aprimorar:

- Hitboxes e hurtboxes.
- Hitstop.
- Knockback.
- Cancelamentos.
- Buffer de inputs.
- Prioridade de golpes.
- Transições entre solo e ar.
- Sistemas anti-infinito.
- Perseguição aérea.
- Finalizações.
- Feedback audiovisual.

Reutilizar a arquitetura existente quando possível, evitando reescrever todo o sistema.

## 7.4 Testes da IA

Validar:

- Ataques individuais.
- Combos.
- Defesas.
- Esquivas.
- Transformações.
- Mudança de armas.
- Dano e morte.
- Interrupções.
- Recuperação.
- Combate em diferentes direções.
- Batalhas longas.
- Comportamentos extremos.
- Situações que anteriormente causavam loops ou travamentos.

**GATE:** IA 2.0 responsiva, desafiadora e consistente, com comportamento reconhecivelmente superior ao sistema anterior.

---

# 8. BUILD JOGÁVEL — NOVAMENTE AUTORIZADA

**A ORDEM ANTERIOR DE CANCELAMENTO DA BUILD ESTÁ REVOGADA PELO CRIADOR.**

Executar testes progressivos durante o desenvolvimento e preparar uma build jogável ao final das cinco etapas principais.

Também são permitidas builds intermediárias de validação quando forem úteis e seguras, mas não devem interromper o desenvolvimento prioritário.

## 8.1 Preparação

Antes de BuildPlayer:

1. Verificar erros de compilação.
2. Executar testes EditMode/PlayMode relevantes.
3. Validar referências e Assets.
4. Verificar Animator Controllers e prefabs.
5. Conferir cenas necessárias no build profile.
6. Monitorar uso de memória.
7. Conferir espaço em disco.
8. Revisar dependências e recursos excessivos.
9. Verificar se nenhuma arte rejeitada foi incluída.
10. Preservar a build anterior válida.

O histórico aponta aproximadamente 986 PNGs em Resources, ocupando cerca de 6,4 GiB. Esse é um diagnóstico anterior, não uma medição atual.

Como a build foi reautorizada, investigar otimizações de memória e carregamento somente quando necessárias, preservando os assets e a integridade visual.

## 8.2 Proteções

Respeitar os guards existentes:

- Mínimo de 2,5 GiB de RAM livre.
- Mínimo de 3 GiB de commit disponível.
- Mínimo de 1 GiB de disco, além do espaço efetivamente necessário para a operação.

Não reduzir os limites para forçar uma build.

Caso haja risco, liberar apenas recursos regeneráveis comprovadamente seguros, otimizar a estratégia de carregamento e tentar novamente quando os requisitos forem atendidos.

## 8.3 Entrega

Objetivo: gerar uma build Windows executável com:

- Fabius Normal completo.
- Atmosfera Gothic Eclipse.
- Lucius Normal.
- Transformações de ambos.
- Combate IA 2.0.
- Animações integradas.
- Controles funcionais.
- Ausência de falhas críticas conhecidas.
- Registro da versão, data e commit correspondente.

Testar inicialização, carregamento de cenários, movimentação, ataques, transformações e confronto com Lucius.

Se algum componente não estiver validado, declarar explicitamente a limitação. Não rotular uma build parcial como completa.

**Priorizar uma entrega jogável e estável sobre uma build aparentemente completa, mas quebrada.**

---

# 9. GERENCIAMENTO DE DISCO, MEMÓRIA E TOKENS

O disco foi recuperado para aproximadamente 30 GB livres.

Preservar, quando viável, a faixa de 30–40 GB livres.

Não apagar:

- Artes originais.
- Sprites aprovados.
- Saves.
- Histórico Git.
- Última build válida.
- Recuperações importantes.
- Os 59 arquivos únicos preservados por SHA-256.

Usar até três subagentes quando vantajoso.

Distribuição sugerida:

1. Subagente artístico — animações, sprites e efeitos da etapa ativa.
2. Subagente técnico — integração e sistemas independentes.
3. Subagente de validação — inspeção de contratos, problemas de animação e preparação de testes.

**Somente o coordenador principal controla Unity, Git e publicação.**

Dar arquivos exclusivos aos subagentes para evitar conflitos.

Economizar tokens sem sacrificar qualidade:

- Contexto curto e específico.
- Reutilização de resultados aprovados.
- Checkpoints frequentes.
- Pequenos lotes verificáveis.
- Sem auditorias globais redundantes.
- Sem novas gerações de artes já aceitas.
- Sem revisões cosméticas irrelevantes antes de cumprir os gates.

A redefinição gratuita permite retomar o escopo completo, mas não autoriza desperdício de contexto, RAM, armazenamento ou tokens.

---

# 10. GIT E SEGURANÇA DE PUBLICAÇÃO

Publicar exclusivamente no repositório autorizado:

Diretório:
`work/publicacao-20261021`

Branch:
`integracao-completa-20261026`

Remoto:
`fwbiodev7/darklight-requiem-game`

NÃO publicar pela raiz Git legada:
`codex/fundacao-darklight`

Último push informado no checkpoint:
`bb4cb6935db62939763c781b174fc9317597508e`

Consultar recibos recentes antes de tratar esse hash como HEAD atual.

Preservar allowlist de publicação e os gates do integrador.

Não publicar candidatos rejeitados, referências privadas, saves, caches, builds temporárias, arquivos desnecessários ou áudio não autorizado.

A build executável pode ser preservada como artefato de entrega separado, sem forçar arquivos binários grandes para dentro do histórico Git.

Registrar checkpoints entre etapas e realizar commits seguros conforme validação.

---

# 11. CONTROLE DE PROGRESSO

Atualizar os recibos de desenvolvimento com estados explícitos:

- Não iniciado.
- Em desenvolvimento.
- Arte produzida.
- Integrado.
- Validado.
- Publicado.
- Bloqueado.

Registrar progresso real por arma, direção, personagem, transformação e sistema.

Não afirmar porcentagens globais sem denominador verificado.

Ao concluir cada etapa principal, registrar:

1. O que foi produzido.
2. O que foi integrado.
3. O que realmente funciona na Unity.
4. O que foi testado.
5. Quais erros permanecem.
6. Qual checkpoint foi preservado.
7. Qual etapa será executada a seguir.

Priorizar continuidade automática e evitar pedir confirmações para decisões já autorizadas neste prompt.

Caso haja bloqueio real, registrar causa, evidências e próximo passo, avançando apenas para trabalho independente e seguro.

---

# 12. ORDEM DE EXECUÇÃO FINAL

**ETAPA 1 — FABIUS NORMAL**
Importar e testar os 15 clips → finalizar Punhos Walk → completar Katana/Punhos/Moto → combos aéreos → defesa, dano, morte e transições → validar.

**ETAPA 2 — DARK FANTASY**
Aplicar Gothic Eclipse aos mapas existentes → iluminação, névoa, partículas, HUD e acabamento gráfico → otimizar e validar.

**ETAPA 3 — LUCIUS NORMAL**
Consolidar referência visual → criar sprites e animações → integrar arsenal, prefab e controladores → testar gameplay.

**ETAPA 4 — TRANSFORMAÇÕES**
Fabius Ruptura/Eclipse Absoluto → Lucius Nexo Fenrir → efeitos, animações, estados e transições → testes.

**ETAPA 5 — COMBATE IA 2.0**
IA avançada → comportamento de Lucius → combos, defesa, cancelamentos e anti-infinito → balanceamento e testes.

**ETAPA 6 — ENTREGA**
Otimização → testes gerais → build Windows jogável → validação real → checkpoint e publicação autorizada.

### INSTRUÇÃO FINAL

O criador reautorizou oficialmente a expansão do escopo graças à redefinição gratuita de limites.

Não manter as suspensões antigas de Lucius, transformações ou build.

**A prioridade é concluir progressivamente o redesign completo, sem pular etapas e sem confundir geração de artes com implementação funcional.**

Começar imediatamente pela integração e validação dos 15 clips que já existem.

Trabalhar com autonomia, preservar tudo que estiver correto, registrar checkpoints e avançar conforme os gates.

**META FINAL: DARKLIGHT: REQUIEM com Fabius e Lucius renovados, atmosfera Gothic Eclipse, transformações funcionais, combate IA 2.0 e uma build realmente jogável.**