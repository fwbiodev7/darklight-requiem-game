# DARKLIGHT: REQUIEM — RETOMADA AUTÔNOMA E FINALIZAÇÃO PROGRESSIVA
**Referência:** 01/11/2026 — 01h15 (America/Sao_Paulo)  
**Modo:** execução autônoma, incremental, rastreável e econômica  
**Objetivo:** concluir o máximo possível com qualidade, sem comprometer o estado já validado.

## 1. Retomada e segurança operacional

Retome o desenvolvimento de DARKLIGHT: REQUIEM **neste mesmo chat, workspace e repositório de publicação**, utilizando o estado real da última execução.

A renovação da janela foi indicada para 01/11/2026 às 01h07, com retomada planejada para 01h15. Caso o horário já tenha passado, execute na primeira oportunidade disponível, respeitando os limites efetivos.

Antes de modificar qualquer arquivo:

1. Verifique se existe outra rodada ativa de Unity, Git, geração de sprites ou integração.
2. Não execute processos concorrentes ou operações potencialmente conflitantes.
3. Confirme diretório de trabalho, branch, HEAD, status do Git e histórico recente.
4. Consulte os documentos de continuidade e recibos.
5. Identifique exclusivamente pendências reais e alterações ainda não publicadas.

**Documentos obrigatórios:**

- `Docs/Darklight/FECHAMENTO-20261031.md`
- `DARKLIGHT_RETOMADA_30-10-2026.md`
- `outputs/retomada-20261031/release-progress.json`
- `outputs/retomada-20261031/build-result.json`
- `outputs/retomada-20261031/final-git-receipt.json`
- `outputs/retomada-20261031/art-integration.json`

Leia também o `AGENTS.md` aplicável e os registros posteriores, se existirem.

Não reinicie atividades concluídas e comprovadas. Evidências posteriores válidas prevalecem sobre checkpoints anteriores, mas nunca substitua recibos por suposições.

## 2. Estado técnico preservado

A referência anterior registra **76 figuras e 20 clips integrados**, distribuídos entre:

- Giro Devastador dos Punhos: 8 direções.
- Katana Leve3: 5 direções.
- Dentes da Moto: 5 direções.
- Reunir/Separar: direção Sul.

Esses números são um checkpoint histórico, não uma autorização para considerar todos os assets testados em runtime.

Diferencie rigorosamente:

- Fonte artística identificada.
- Figura candidata.
- Figura aprovada.
- Figura importada.
- Clip integrado.
- Animação validada no Unity.
- Funcionalidade testada em runtime.
- Build concluída e publicada.

Somente recibos verificáveis podem certificar as últimas etapas.

Preservar as correções anteriores:

- Regressões de buffer de combate resolvidas.
- Eclipse antigo separado do fluxo novo.
- `form2` ativo somente quando `fusedEclipse=true`.
- Lacunas de transformações utilizam o Normal novo, sem filtros artificiais.
- Proveniência de 17 páginas corrigida.
- Acentuação de 29 `attackKeys` corrigida.
- Identificadores, serialização e compatibilidade existentes preservados.

Não reintroduza regressões para simplificar implementações.

## 3. Escopo canônico obrigatório

O redesign atual utiliza exclusivamente:

**Protagonista:** Fabius, skin Réquiem.

**Equipamentos:**
- Katana.
- Punhos.
- Moto.

**Transformações:**
- Normal.
- Ruptura.
- Eclipse Absoluto, fusão violeta/prata.

Manter fora do escopo ativo a skin Rock, as Presas do Caos e as transformações antigas separadas.

O design aprovado da Katana governa a identidade visual do personagem:

- Anatomia adulta e proporções constantes.
- Rosto, cabelo, pele e olhos canônicos.
- Oito direções genuínas.
- Sem crescimento artificial de cabelo.
- Sem cristais grandes, manoplas rejeitadas ou alterações não aprovadas.
- `WorldVisualScale = 1.325`.

**Não redesenhe o Fabius inteiro para corrigir uma animação.** Preserve o design aprovado e altere apenas o necessário.

## 4. Prioridade máxima — concluir Fabius Normal

### 4.1. Recuperação da Katana Leve3

As fontes NW/N/NE possuem 28 figuras com resíduos e exigem reparos completos.

Procedimento:

1. Verifique os originais, hashes e histórico de geração.
2. Identifique especificamente os quadros defeituosos.
3. Corrija resíduos, recortes, anatomia e sobreposições.
4. Preserve integralmente as figuras aprovadas.
5. Valide coerência temporal, pivôs, direção e continuidade.
6. Integre somente figuras aprovadas e testáveis.

Não recicle poses traseiras incorretas nem apresente espelhamento como direção genuína.

### 4.2. Moto — Dentes e demais pendências

Preserve como candidatas as 31 figuras das fontes Dentes S/E/SE.

Revise integridade, alinhamento, sequência, efeitos, direção e compatibilidade com a Moto Réquiem antes de integrar.

Complete as lacunas reais de animação e integração, sem duplicar as cinco direções previamente registradas.

### 4.3. Caminhada

As nove fontes de caminhada rejeitadas por repetição de apoio continuam rejeitadas.

Não as reintroduza, renomeie ou reutilize silenciosamente.

Caso seja necessário gerar novos quadros, exija alternância real dos pés, transferência de peso, estabilidade do tronco e ausência de deslizamento visual.

Preserve a Walk v5 aprovada onde aplicável.

### 4.4. Esquiva com partículas — nova tarefa

Foi criada nesta conversa uma referência visual de **Fabius Réquiem executando seis quadros de esquiva com katana, fumaça cinza-prateada, arcos de vento e pequenas partículas azuladas**.

Verifique se o asset está efetivamente disponível no workspace. Caso não esteja, registre a dependência e continue outras tarefas sem bloquear a rodada.

Se estiver disponível:

1. Compare a arte composta com os sprites canônicos.
2. Identifique possíveis diferenças de rosto, corpo, espada e vestuário.
3. Preserve a animação aprovada original.
4. Separe, preferencialmente, partículas e personagem em camadas independentes.
5. Sincronize os efeitos com preparação, deslocamento e recuperação.
6. Evite partículas permanentes, efeitos excessivos ou oclusão do personagem.
7. Teste transparência, pivôs, velocidade, direção e transições.
8. Confirme que a esquiva continua responsiva e com duração funcional correta.

Não considere a imagem ilustrativa um sprite runtime validado.

A integração deve ser não destrutiva, permitindo desativar os efeitos sem perder a animação original.

## 5. Combate — revisão técnica e IA 2.0

Após estabilizar as animações essenciais de Fabius Normal, prossiga para as pendências reais de combate.

Inspecione os controladores, máquinas de estados, animações, eventos, colisores e integração dos equipamentos.

Priorize:

- Transições entre idle, caminhada, ataque e esquiva.
- Buffer de comandos e cancelamentos permitidos.
- Janelas de invulnerabilidade coerentes.
- Recuperação após esquiva, dano e combos.
- Hitboxes e hurtboxes alinhadas à animação.
- Prevenção de ataques infinitos.
- Limites de perseguição e combos aéreos.
- Compatibilidade entre Katana, Punhos e Moto.
- Eventos de animação sem duplicação.
- Consistência de estados após save/load.

**IA 2.0:** avançar gradualmente na leitura contextual de ataques, perseguição, defesa, reação, contra-ataques e recuperação.

A IA deve desafiar o jogador de forma justa, com antecipações visuais e janelas compreensíveis, sem leitura injusta de inputs ou reações instantâneas artificiais.

Não reescreva sistemas estáveis apenas por preferência arquitetural.

Utilize testes focados, reproduzíveis e automatizados sempre que possível.

## 6. Transformações — depois do Normal

Somente após o funcionamento satisfatório do Fabius Normal, avance para Ruptura e Eclipse Absoluto.

Prefira herdar lógica e animações compartilháveis da forma Normal, alterando apenas os componentes necessários.

Evite multiplicar desnecessariamente sprites, clips e estados.

Mantenha:

- Ruptura com sua identidade vermelha.
- Eclipse Absoluto com identidade violeta/prata.
- Silhueta, proporções e legibilidade consistentes.
- `fusedEclipse=true` como condição de ativação da forma consolidada.

Não preencha lacunas com arte reprovada ou recoloração que altere indevidamente a identidade do personagem.

## 7. Lucius — preparação e implementação progressiva

Lucius é o antagonista gêmeo de Fabius, não Lucien. Não misture seus nomes, assets ou controladores.

Escopo atual:

- Lucius Normal.
- Lucius Nexo Fenrir.

Preservar a identidade visual aprovada: sobretudo preto/azul, cabelos claros, olhos azuis, anatomia compatível com Fabius e arsenal próprio.

As referências existentes **não comprovam a implementação de um consumidor runtime independente**.

Antes de gerar novas artes:

1. Audite os sistemas de personagens e inimigos existentes.
2. Identifique componentes reutilizáveis.
3. Defina controlador, estados, dados e transições próprios de Lucius.
4. Planeje seu arsenal e comportamento distintos dos de Fabius.
5. Estabeleça um fluxo mínimo de bossfight verificável.
6. Implemente progressivamente, com validação por etapa.

Não declare Lucius jogável ou implementado apenas por existirem referências artísticas.

Caso o orçamento de execução não permita avançar, deixe uma especificação técnica objetiva e preserve a prioridade do Fabius.

## 8. Subagentes — divisão eficiente

Utilize subagentes curtos e especializados, conforme `AGENTS.md`, sem iniciar gerações redundantes.

Delegações recomendadas:

**Arte:** reparar exclusivamente sprites defeituosos, preservar fontes e registrar hashes.

**Combate:** investigar estados, buffers, cancelamentos, hitboxes e possíveis regressões.

**IA:** propor e implementar mudanças pequenas, isoladas e testáveis.

**QA:** verificar clips, integridade de assets, testes e recibos.

**Integração:** tarefa exclusiva do coordenador principal.

Cada subagente deve entregar resultado objetivo, arquivos alterados, evidências e pendências, sem repetir relatórios extensos.

Somente o coordenador executa Unity, integração final, builds e operações Git compartilhadas.

Use áudio silencioso, saves isolados e evite instâncias concorrentes.

Não aumente artificialmente o paralelismo quando houver risco de conflito ou desperdício de tokens.

## 9. Controle de qualidade e publicação

Repositório remoto autorizado:

`fwbiodev7/darklight-requiem-game`

Workspace de publicação:

`work/publicacao-20261021`

Branch autorizada:

`integracao-completa-20261026`

A raiz Git é legado e não deve ser utilizada para publicação.

**Proibições:**

- Não executar merge em `main`.
- Não publicar remoções herdadas da raiz legada.
- Não executar reset destrutivo sem necessidade comprovada.
- Não sobrescrever assets canônicos aprovados.
- Não publicar candidatos como se estivessem validados.

Antes de cada publicação:

1. Conferir arquivos modificados e diferenças.
2. Confirmar que não existem dados privados ou temporários incluídos.
3. Executar apenas os testes pertinentes às mudanças.
4. Registrar erros, warnings novos e resultados.
5. Realizar commit identificável.
6. Executar push na branch correta.
7. Confirmar branch, HEAD e commit remoto.
8. Atualizar os recibos de integração e publicação.

Não repetir gates verdes que não foram afetados pelas alterações.

### Exclusões e preservação

Não incluir em commits builds, ZIPs, saves pessoais, caches, fontes rejeitadas, referências privadas ou MP3 originais.

**Excluir do versionamento não significa apagar indiscriminadamente do disco.**

Preserve a última build funcional, preferências, configurações, saves importantes e fontes aprovadas.

Limpezas locais só podem remover materiais comprovadamente descartáveis, sem comprometer recuperação ou continuidade.

A limpeza de PC registrada em `outputs/pc-cleanup-20261031` é independente e não integra os commits do DARKLIGHT.

## 10. Administração do orçamento de tokens

Execute em ciclos curtos de diagnóstico, implementação, validação e publicação.

**Regra de segurança:** encerrar novas implementações quando restarem aproximadamente 10% da janela disponível.

Não espere chegar a esse limite para iniciar a publicação.

- Acima de 30%: desenvolvimento e reparos prioritários.
- Entre 30% e 20%: concluir integrações em andamento.
- Entre 20% e 10%: privilegiar testes, build, recibos e commits.
- Em 10% ou menos: não iniciar novas tarefas; preservar estado, publicar o que estiver validado e finalizar.

Os percentuais são critérios operacionais, não uma afirmação de que a ferramenta consegue consultar o saldo exato. Se o limite não estiver disponível, utilize sinais confiáveis e antecipe o fechamento.

Reserve margem suficiente para commit, push, validação e registro final.

Caso a build falhe, registre o motivo real e preserve a última build funcional.

Nunca declare uma build jogável sem evidência da sua geração e validação.

## 11. Entregáveis da rodada

Ao finalizar, produza um fechamento curto e verificável contendo:

1. **Concluído:** alterações efetivamente implementadas e testadas.
2. **Parcial:** recursos integrados com validação pendente.
3. **Bloqueado:** motivo técnico e dependência objetiva.
4. **Não iniciado:** pendências que permaneceram fora da rodada.
5. **Arte:** quantidades de figuras aprovadas, importadas e rejeitadas.
6. **Combate e IA:** melhorias verificadas e regressões identificadas.
7. **Build:** caminho, estado e resultado dos testes.
8. **Git:** branch, HEAD, commit e confirmação do push.
9. **Continuidade:** prioridade exata da próxima execução.

Atualize os arquivos de checkpoint e recibos correspondentes.

Não confunda leitura de código, testes isolados, importação e aprovação completa.

## 12. Condições de encerramento e comunicação

Trabalhe autonomamente dentro do escopo autorizado, sem interromper a rodada para escolhas técnicas triviais.

Solicite intervenção somente quando houver bloqueio real, risco de perda de dados ou decisão que altere substancialmente o escopo.

Notifique apenas:

- Conclusão verificável.
- Falha que impeça continuidade.
- Mudança material no projeto.
- Ação necessária do usuário.

Não envie atualizações repetitivas sem progresso.

Se o objetivo integral estiver comprovadamente concluído, registre o fechamento definitivo e desative a automação específica de retomada, caso tenha permissão para gerenciá-la.

Caso contrário, mantenha as pendências reais documentadas para a próxima execução.

**Diretriz final:**

Priorize qualidade, continuidade, rastreabilidade e uma build progressivamente mais completa.

Termine primeiro o Fabius Normal, estabilize combate e IA, depois avance para transformações e Lucius.

Não refaça o que está correto. Não publique o que não foi validado. Não sacrifique estabilidade apenas para aumentar números de sprites ou commits.

O sucesso desta rodada será medido pelo avanço real do jogo, não pelo volume de arquivos gerados.# DARKLIGHT: REQUIEM — RETOMADA AUTÔNOMA, COMBATE E VFX 2.0
**Referência:** 01/11/2026  
**Objetivo:** concluir o máximo possível do redesign, integrar efeitos visuais exclusivos, estabilizar o combate e avançar na implementação de Lucius.

## 1. Retomada segura do projeto

Retome DARKLIGHT: REQUIEM neste mesmo chat e workspace, respeitando a disponibilidade real da janela de execução.

Não iniciar Unity, Git, integrações ou gerações concorrentes caso uma rodada anterior ainda esteja ativa.

Consultar obrigatoriamente:

- `Docs/Darklight/FECHAMENTO-20261031.md`
- `DARKLIGHT_RETOMADA_30-10-2026.md`
- `outputs/retomada-20261031/release-progress.json`
- `outputs/retomada-20261031/build-result.json`
- `outputs/retomada-20261031/final-git-receipt.json`
- `outputs/retomada-20261031/art-integration.json`
- `AGENTS.md` e registros posteriores.

Preservar as 76 figuras e 20 clips integrados conforme checkpoint anterior, atualizando os números somente mediante recibos mais recentes.

Manter todas as correções de buffers, proveniência, attackKeys e transformação Eclipse já registradas.

Não confundir figuras candidatas, integradas, aprovadas visualmente e validadas no Unity.

## 2. Escopo canônico

O projeto utiliza:

- Fabius Réquiem.
- Katana, Punhos e Moto.
- Normal, Ruptura e Eclipse Absoluto.
- Oito direções genuínas.
- `WorldVisualScale = 1.325`.

Preservar corpo adulto, rosto, cabelo, pele, olhos e proporções canônicas.

Não reintroduzir Rock, Presas, formas antigas separadas ou artes rejeitadas.

Continuar as pendências reais de Katana Leve3, Dentes Moto, Giro Punhos, caminhadas, esquivas, transições e combate.

Manter as fontes NW/N/NE defeituosas de Leve3 como pendentes de reparo. Não reutilizar as nove fontes rejeitadas de caminhada.

## 3. NOVA PRIORIDADE — SISTEMA VFX 2.0

Implementar um sistema modular e reutilizável de efeitos visuais em pixel art para todo o combate.

O objetivo não é simplesmente adicionar partículas decorativas. Cada efeito deve comunicar claramente velocidade, força, direção, colisão, defesa ou energia.

**Inspecionar primeiramente os efeitos de ataque já existentes**, incluindo os apresentados na imagem de referência:

- Explosões e estrelas amarelo-alaranjadas.
- Cortes e faíscas branco-prateados.
- Fragmentos de impacto brancos e cinzentos.

Esses efeitos são uma base, mas precisam de melhorias.

Problemas a investigar:

- Explosões genéricas, sem relação clara com a arma.
- Formatos muito semelhantes entre ataques.
- Poucas etapas perceptíveis de expansão e dissipação.
- Falta de direção nos fragmentos.
- Impactos sem sensação de peso.
- Ausência de variação entre golpes leves, fortes e finalizadores.
- Possíveis inconsistências de pivô, recorte, transparência e velocidade.

Não eliminar efeitos válidos indiscriminadamente. Recuperar, melhorar e especializar os arquivos de origem.

Imagens enviadas no chat são referências visuais, não comprovação de assets disponíveis no workspace. Localizar as fontes reais antes de qualquer alteração.

## 4. Identidade exclusiva por arma

### 4.1. Katana — precisão e velocidade

Criar e integrar:

- Arcos de vento brancos e prateados.
- Rastros finos de lâmina.
- Pequenas partículas azuladas.
- Faíscas metálicas em colisões.
- Cortes diagonais, horizontais e verticais com direção coerente.
- Rastros crescentes para ataques fortes.
- Partículas de deslocamento nas esquivas.
- Explosão concentrada no final de combos.

Os golpes leves precisam de efeitos rápidos e discretos.

Os golpes fortes podem gerar um arco maior, mas sem esconder o personagem ou o inimigo.

Evitar que o efeito pareça um ataque mágico constante: a Katana Normal deve transmitir técnica, precisão e velocidade.

### 4.2. Punhos — impacto e destruição

Criar e integrar:

- Ondas de choque curtas.
- Explosões de poeira.
- Fragmentos de pedra.
- Faíscas quentes nos impactos intensos.
- Círculos de pressão no solo.
- Pequenas rachaduras temporárias em superfícies compatíveis.
- Rastros de deslocamento pesado.
- Partículas direcionais nos socos e chutes.

Quanto maior a força do golpe, maior a reação visual, respeitando limites de legibilidade.

Nas esquivas, o personagem deve deslocar poeira e pequenos detritos com os pés, em vez de produzir os mesmos arcos mágicos da Katana.

Nos ataques aéreos e esmagamentos, concentrar as partículas no local da colisão.

### 4.3. Moto — atrito, metal e brutalidade

Criar e integrar:

- Faíscas metálicas laranjas.
- Rastros circulares de rotação.
- Fumaça cinza de atrito.
- Poeira de derrapagem.
- Marcas temporárias de pneus ou rodas.
- Fragmentos metálicos nos impactos adequados.
- Trilhas de velocidade.
- Efeitos de frenagem e arrancada.

As rodas devem parecer pesadas e perigosas.

Aplicar efeitos específicos às seis técnicas existentes:

1. Serra Infernal — partículas de atrito e faíscas.
2. Giro Devastador — rastros circulares e força centrífuga.
3. Esmagamento Vertical — poeira, detritos e onda de impacto.
4. Roda Destruidora — rotação agressiva e partículas projetadas.
5. Cavalgada Brutal — velocidade, derrapagem e atrito.
6. Execução Aérea Motorizada — rastro de queda e explosão de aterrissagem.

Preservar a identidade das técnicas e seu funcionamento original.

Não transformar esses efeitos em novas armas ou alterar a lógica dos golpes sem necessidade.

## 5. Eventos universais de combate

### 5.1. Acerto de golpes

Todo golpe que realmente conecta deve poder gerar partículas no ponto de impacto.

Variar conforme:

- Arma utilizada.
- Intensidade do golpe.
- Direção da colisão.
- Alvo atingido.
- Forma ativa.

Golpes leves recebem flashes menores.

Golpes pesados recebem explosões e fragmentos adicionais.

Finalizadores recebem um efeito de maior destaque, sem excesso de duração.

Não gerar impactos visuais quando o ataque erra ou é cancelado antes de acertar.

### 5.2. Defesa comum

Revisar as partículas prateadas existentes.

Adicionar:

- Faíscas na colisão.
- Pequeno flash de contato.
- Fragmentos luminosos.
- Dissipação rápida.

A defesa comum não deve parecer visualmente mais poderosa que um parry perfeito.

### 5.3. Parry perfeito

Criar uma resposta visual exclusiva:

- Estrela metálica intensa.
- Flash branco-dourado.
- Onda de choque circular.
- Faíscas projetadas na direção da colisão.

O efeito deve ser imediatamente reconhecível, diferente da defesa comum.

Sincronizar exatamente com a confirmação de parry pelo sistema de combate.

### 5.4. Dano recebido

Aprimorar os efeitos existentes de dano:

- Pequeno flash vermelho ou branco.
- Fragmentos de impacto.
- Poeira em colisões físicas.
- Reação proporcional à força do ataque.

Não cobrir o rosto do personagem constantemente.

Evitar efeitos repetidos excessivamente em sequências rápidas de dano.

### 5.5. Morte

Preservar as animações de morte originais.

Integrar, conforme o tipo de inimigo:

- Fragmentos escuros.
- Fumaça residual.
- Energia se dissipando.
- Pequeno impacto no solo.
- Desaparecimento progressivo dos efeitos.

Chefes podem receber sequências maiores, respeitando a direção artística e a narrativa.

Não substituir a animação de morte por uma simples explosão.

### 5.6. Finalização de combos

Adicionar partículas especiais no último golpe de uma sequência.

Exemplos:

- Katana: corte luminoso de curta duração.
- Punhos: onda de choque e detritos.
- Moto: faíscas, fumaça e impacto giratório.

A intensidade deve aumentar sem prejudicar a leitura do combate.

## 6. Movimentação e esquivas

Integrar partículas independentes para:

- Arrancada.
- Corrida.
- Frenagem.
- Mudança brusca de direção.
- Esquiva.
- Salto.
- Aterrissagem.
- Queda de grande altura.

### Esquivas por equipamento

**Katana:** vento prateado, pequenos rastros e brilho azulado.

**Punhos:** poeira pesada, detritos e ondas de pressão junto ao solo.

**Moto:** fumaça de roda, faíscas e trilhas curtas de derrapagem.

As spritesheets de esquiva produzidas anteriormente servem como referência, desde que disponíveis e aprovadas.

Priorizar partículas em camadas independentes do corpo de Fabius.

Não alterar a anatomia ou substituir sprites canônicos apenas para adicionar efeitos.

Manter as oito direções de esquiva funcionalmente corretas.

## 7. Combate aéreo

Revisar o sistema existente de lançamento, perseguição, suspensão, arremesso e finalização aérea.

Integrar partículas em:

- Lançamento do inimigo.
- Início de perseguição.
- Aceleração aérea.
- Conexão de golpes no ar.
- Arremesso.
- Impacto contra o solo.
- Finalização.

Usar rastros de vento para velocidade, impactos para colisões e poeira para contato com o cenário.

Preservar todos os limites anti-infinito e regras de resistência de chefes.

Não reintroduzir regressões de buffer ou posicionamento.

## 8. Carregamento de golpes e colisões

Criar efeitos progressivos de carregamento quando o golpe realmente possuir essa mecânica.

Sequência visual:

1. Partículas discretas começam a surgir.
2. Energia se concentra no equipamento.
3. O brilho aumenta conforme o carregamento válido.
4. O efeito termina no disparo, cancelamento ou interrupção.

Não inventar ataques carregados para golpes que não possuem carregamento.

Nas colisões entre armas, utilizar flashes metálicos e faíscas direcionais.

Em ataques bloqueados, utilizar efeitos de contato distintos dos acertos diretos.

## 9. Transformações — partículas exclusivas

### Normal

Manter partículas discretas e naturais.

Não adicionar aura permanente desnecessária.

### Ruptura

Criar:

- Chamas e brasas vermelhas.
- Pequenos fragmentos de energia.
- Ondas de pressão avermelhadas.
- Trilhas especiais nos golpes fortes.
- Explosão curta na ativação.

Preservar a aparência física canônica.

### Eclipse Absoluto

Criar:

- Energia violeta e prateada.
- Fragmentos luminosos.
- Pequenas descargas elétricas estilizadas.
- Ondas de pressão.
- Distorções visuais discretas.
- Partículas orbitais durante a ativação.

Não usar filtros que modifiquem indevidamente a pele, o rosto ou outros elementos canônicos.

Preservar a regra `fusedEclipse=true` para a forma consolidada.

Não gerar todas as variações de uma vez: concluir primeiro os efeitos essenciais do Normal.

## 10. Atmosfera dark fantasy sem reconstrução dos mapas

Melhorar os cenários existentes por meio de camadas visuais adicionais.

Aplicar conforme o contexto de cada ambiente:

- Névoa próxima ao chão.
- Cinzas flutuantes.
- Poeira em áreas abandonadas.
- Brasas em áreas destruídas.
- Partículas próximas a fontes de luz.
- Pequenos detritos durante confrontos.
- Efeitos ambientais reagindo a ataques muito fortes.

Preservar integralmente geometria, navegação, colisores e identidade dos mapas existentes.

Não refazer mapas apenas para incorporar atmosfera.

Evitar partículas constantes que sobrecarreguem o cenário ou prejudiquem a visibilidade de inimigos e objetos.

## 11. Bossfights e Lucius

Após estabilizar as prioridades anteriores, preparar efeitos próprios de chefes.

Lucius possui identidade e arsenal distintos dos de Fabius.

Escopo:

- Normal.
- Nexo Fenrir.

Planejar uma linguagem visual própria para seus poderes, respeitando suas referências canônicas.

Não utilizar simplesmente as mesmas partículas de Fabius com cores diferentes.

Criar respostas ambientais para golpes de grande intensidade, especialmente nas bossfights principais.

Não confundir Lucius com Lucien.

Não declarar Lucius implementado caso existam somente referências visuais ou código preparatório.

## 12. Arquitetura técnica dos VFX

Utilizar uma solução de VFX 2D integrada à arquitetura real do projeto.

Preferir sprites animados e sistemas leves compatíveis com a estética pixel art.

Diretrizes:

- Sprites PNG com transparência verdadeira.
- Filtragem Point.
- Sem mipmaps em sprites pixel art.
- Compressão desativada quando necessária para preservar pixels.
- Pivôs e alinhamentos consistentes.
- Sorting Layers e Order in Layer corretos.
- Sincronização com os eventos reais dos ataques.
- Sem dependência de FPS fixo para a lógica de animação.
- Reutilização por object pooling quando aplicável.
- Sem instanciar e destruir grandes quantidades de objetos por golpe.
- Sem alocações desnecessárias em trechos críticos do combate.
- Sem flashes de tela excessivos ou efeitos estroboscópicos.

Organizar os efeitos por categorias:

- Weapon/Katana.
- Weapon/Fists.
- Weapon/Moto.
- Combat/Hit.
- Combat/Defense.
- Combat/Parry.
- Combat/Death.
- Movement/Dodge.
- Movement/Landing.
- Transformations.
- Environment.
- Bosses.

Esses são agrupamentos lógicos sugeridos, não caminhos obrigatórios. Respeitar a estrutura já existente antes de criar novas pastas e classes.

### Controle de intensidade

Implementar ou aproveitar configurações para controlar:

- Quantidade de partículas.
- Duração.
- Escala.
- Opacidade.
- Frequência.
- Intensidade de efeitos especiais.

Os efeitos devem comunicar o gameplay mesmo em configurações reduzidas.

Evitar que vários impactos simultâneos cubram completamente os personagens.

## 13. Plano de execução

Executar por etapas, priorizando qualidade e reaproveitamento.

**Etapa A — Auditoria**

Identificar todos os efeitos já existentes, sua origem, integração, problemas e possibilidades de reaproveitamento.

**Etapa B — Combate essencial**

Melhorar impactos, defesa, dano, morte e parry.

**Etapa C — Identidade das armas**

Criar e integrar efeitos distintos para Katana, Punhos e Moto, começando pelos ataques que já estão funcionais.

**Etapa D — Movimentação**

Integrar esquivas, aterrissagens, arrancadas e frenagens.

**Etapa E — Combate avançado**

Aplicar efeitos em combos, técnicas especiais, carregamentos e combate aéreo.

**Etapa F — Transformações e ambientes**

Criar efeitos das formas, atmosfera dark fantasy e elementos ambientais.

**Etapa G — Bossfights**

Preparar efeitos exclusivos de Lucius e demais chefes quando os respectivos sistemas estiverem prontos.

Não deixar a criação de VFX bloquear indefinidamente as animações essenciais, os testes e a entrega de uma build.

## 14. Subagentes e uso de tokens

Delegar tarefas independentes e curtas:

- Auditoria de partículas existentes.
- Revisão visual e geração de efeitos novos.
- Integração e eventos de animação.
- Otimização de renderização e pooling.
- Testes de regressão.

Somente o coordenador principal executa Unity, builds e operações Git compartilhadas.

Não executar processos concorrentes que possam corromper assets, importar arquivos simultaneamente ou gerar conflitos.

Não gerar novamente partículas aprovadas.

Cada subagente deve trabalhar apenas nas lacunas identificadas e entregar arquivos, hashes, evidências e estado de conclusão.

Interromper novas implementações quando restarem aproximadamente 10% da janela, reservando a margem final para validação, recibos, commit e push.

## 15. Testes e critérios de aprovação

Validar os novos VFX no Unity.

Verificar:

- O evento acontece na hora correta?
- O efeito corresponde ao equipamento?
- O efeito acompanha a direção do ataque?
- O ponto de impacto está correto?
- As partículas respeitam a câmera e os planos?
- Existe fundo preto indevido em sprites transparentes?
- Há cortes, desalinhamentos ou resíduos?
- O efeito desaparece corretamente?
- Ataques cancelados deixam partículas incorretas?
- O efeito sobrevive indevidamente após morte ou troca de cena?
- Existem perdas perceptíveis de desempenho?
- Ataques e defesa continuam funcionais após a integração?

Testar os oito direcionais nos recursos que exigem oito direções.

Realizar testes focados e screenshots ou registros visuais quando possível.

Marcar cada efeito como candidato, revisado, integrado ou validado, conforme evidência disponível.

Não considerar uma spritesheet bonita como funcionalidade concluída.

## 16. Publicação e preservação

Utilizar exclusivamente:

**Workspace:** `work/publicacao-20261021`

**Branch:** `integracao-completa-20261026`

**Remoto:** `fwbiodev7/darklight-requiem-game`

Não publicar pela raiz Git legada.

Não executar merge com `main`.

Não publicar builds, ZIPs, caches, saves pessoais, fontes rejeitadas, referências privadas ou MP3 originais.

Preservar a última build funcional, preferências, fontes canônicas e arquivos necessários à recuperação.

A limpeza de PC permanece independente dos commits DARKLIGHT.

Após validação:

1. Revisar as diferenças.
2. Executar testes relevantes.
3. Registrar resultados reais.
4. Criar commits identificáveis.
5. Fazer push na branch autorizada.
6. Confirmar HEAD e recibo remoto.
7. Atualizar checkpoints de integração e build.

Não repetir testes verdes sem alterações relevantes.

## 17. Encerramento

Ao concluir a rodada, registrar:

- Animações recuperadas.
- Partículas reaproveitadas.
- Partículas novas.
- Efeitos integrados e validados.
- Melhorias de combate.
- Regressões corrigidas.
- Pendências de IA 2.0.
- Progresso de transformações e Lucius.
- Resultado da build.
- Commit, branch e confirmação de push.
- Próxima prioridade real.

Manter como prioridade geral finalizar Fabius Normal, estabilizar combate e depois avançar para as formas e Lucius.

Os VFX 2.0 devem ser implementados progressivamente, aproveitando os sistemas funcionais.

Notificar somente conclusão, falha relevante, alteração material ou necessidade de decisão do usuário.

**Diretriz final:** DARKLIGHT deve transmitir velocidade na Katana, brutalidade nos Punhos, destruição na Moto e poder nas transformações.

Cada partícula precisa ter um propósito. O resultado desejado é um combate mais satisfatório, legível, responsivo e imersivo — não simplesmente uma tela cheia de explosões.