# DARKLIGHT: REQUIEM — REESTRUTURAÇÃO DA CAMPANHA E DESIGNS OFICIAIS

## OBJETIVO PRINCIPAL

Atualizar a estrutura narrativa e visual de DARKLIGHT: REQUIEM com base nas três referências anexadas pelo usuário.

**NOVA DECISÃO CANÔNICA: A campanha principal terá somente três chefes obrigatórios, nesta ordem:**

1. **Matriarca Rubra** — primeira bossfight.
2. **Kael Voss** — segunda bossfight, posteriormente se torna aliado.
3. **Lucius** — chefe final, com as formas Normal e Arconte.

**NOCTHAR ESTÁ REMOVIDO DO ESCOPO JOGÁVEL.**

No lugar de batalhas adicionais contra chefes, desenvolver encontros com inimigos distintos, exploração, puzzles e desafios de combate interessantes.

Preservar a identidade gótica, o ritmo cinematográfico e os sistemas já aprovados.

---

## 1. REFERÊNCIAS VISUAIS OFICIAIS

As três imagens anexadas são as referências principais dos personagens.

Não utilizar versões antigas caso entrem em conflito com os novos designs.

### LYRA — ALIADA OFICIAL

Usar a primeira imagem anexada.

Preservar:

- Cabelos castanho-avermelhados, volumosos e ondulados.
- Rosto e expressão da referência.
- Casaco preto com detalhes carmesins e dourados.
- Decote e silhueta feminina aprovados.
- Calças escuras, cintos e botas.
- Livro verde-azulado e acessórios.
- Visual gótico elegante, sem descaracterização.

**PROPORÇÃO OBRIGATÓRIA:** Lyra deverá possuir a mesma altura e proporções gerais de Fabius Normal em gameplay.

Não aumentar excessivamente a cabeça nem alongar o corpo.

Lyra é aliada desde o começo da campanha. Preservar seus diálogos, presença narrativa e personalidade.

A pistola presente em suas referências não autoriza a criação automática de um novo sistema de armas.

### MATRIARCA RUBRA — PRIMEIRA CHEFE

Usar a segunda imagem anexada.

Preservar:

- Cabelos longos prateados.
- Olhos vermelhos.
- Vestido gótico vermelho e preto.
- Capa ornamentada.
- Asas de morcego.
- Halo de espinhos com rosa e caveira.
- Detalhes de rosas, crânios e acessórios dourados.
- Espada vermelha sobrenatural.

A Matriarca deve parecer um pouco mais alta e imponente que Fabius, sem perder o estilo pixel art e as proporções gerais do jogo.

Sua batalha deve transmitir a presença de uma aristocrata vampírica perigosa, com golpes bem antecipados e movimentos elegantes.

### KAEL VOSS — SEGUNDO CHEFE / FUTURO ALIADO

Usar a terceira imagem anexada.

Preservar:

- Cabelos prateados.
- Olhos azuis.
- Roupa preta e azul.
- Armadura leve.
- Capa azul ornamentada.
- Espada luminosa azul.
- Cicatrizes, acessórios e identidade visual de anti-herói.

Kael deverá possuir proporções compatíveis com Fabius Normal.

Não confundir seu design com Lucius.

**IMPORTANTE:** Kael continua sendo um personagem que começa como inimigo e posteriormente se torna aliado.

Preservar sua rendição, purificação e a narrativa relacionada a Kael Fraturado, adaptando apenas o necessário à nova campanha.

---

## 2. REMOÇÃO SEGURA DE NOCTHAR

Nocthar e sua forma final devem deixar de fazer parte da campanha jogável.

Isso inclui desativar ou retirar com segurança:

- Bossfight e gatilhos de combate.
- Transições para a forma final.
- Aparições obrigatórias.
- Referências em objetivos e diálogos ativos.
- Portais e eventos que dependam de sua derrota.
- HUD e barras de chefe específicas.
- Entradas de seleção e treinamento que dependam dele.
- Desbloqueios que exijam sua derrota.
- Progressão narrativa associada à sua bossfight.

**NÃO excluir destrutivamente scripts, sprites ou assets históricos.**

Preservar os recursos em backups ou arquivos de legado organizados, mantendo possibilidade de restauração.

Antes de editar, auditar todas as dependências de Nocthar para evitar referências ausentes, saves incompatíveis e problemas de carregamento.

Não quebrar saves antigos. Criar migração ou fallback seguro para estados de campanha que referenciem Nocthar.

Os mapas existentes devem ser preservados e reaproveitados.

A remoção de Nocthar não significa apagar automaticamente o mapa ou cenário onde ele aparecia.

Registrar a mudança nos documentos oficiais, GDD e checkpoints.

---

## 3. NOVA ESTRUTURA DA CAMPANHA

A campanha principal deve possuir somente duas bossfights antes de Lucius.

**Sequência narrativa obrigatória:**

Matriarca Rubra → evolução da campanha e desafios → Kael Voss → aliança/purificação → desafios finais → Lucius.

Os dez mapas existentes devem continuar sendo a base da campanha.

Reorganizar os encontros e objetivos sem regenerar cenários inteiros.

Os outros chefes atualmente associados à campanha precisam ser identificados antes das alterações. Não apagar seus arquivos; retirar seus encontros obrigatórios da progressão principal e preservar material reutilizável.

Os espaços anteriormente ocupados por chefes extras devem ganhar atividades mais variadas.

## 4. NOVAS IDEIAS PARA SUBSTITUIR BOSSFIGHTS

### A. O Ritual dos Três Selos

Puzzle gótico envolvendo três selos mágicos espalhados pelo cenário.

O jogador deve:

1. Encontrar os selos.
2. Interpretar símbolos e pistas visuais.
3. Ativá-los na sequência correta.
4. Combater inimigos que surgem durante partes do ritual.
5. Abrir a passagem para o próximo mapa.

Evitar depender de tentativa e erro sem pistas.

### B. Espelhos do Abismo

Puzzle envolvendo espelhos sobrenaturais que redirecionam feixes de energia vermelha.

Fabius deve alinhar os espelhos para alcançar um receptor e abrir uma passagem.

Possibilidades:

- Espelhos giratórios.
- Reflexos falsos de Fabius.
- Passagens ocultas.
- Inimigos espectrais que interferem na solução.

Priorizar mecânica simples e legível, adequada ao jogo 2D.

### C. A Caçada das Fendas

Encontro de combate dinâmico sem um chefe tradicional.

Fendas sobrenaturais surgem no cenário, liberando pequenos grupos de inimigos existentes.

Utilizar combinações interessantes de:

- Crawler.
- Hunter.
- Armored.
- Shooter.
- Spectral.

O objetivo é fechar os portais enquanto enfrenta os inimigos.

Evitar ondas excessivamente longas ou repetitivas.

### D. O Corredor dos Ecos

Desafio de exploração com ilusões e armadilhas.

Fabius atravessa um corredor onde surgem versões ilusórias de elementos do cenário.

Utilizar:

- Pistas visuais.
- Armadilhas temporizadas.
- Portas falsas.
- Caminhos alternativos.
- Pequenos encontros de combate.

A solução deve recompensar observação e exploração, não apenas velocidade.

### E. Prova de Kael

Após Kael se tornar aliado, criar uma sequência cooperativa simples em que ele ajuda Fabius a atravessar uma área perigosa.

Exemplos:

- Kael ativa um mecanismo enquanto Fabius enfrenta inimigos.
- Os dois precisam acessar pontos diferentes do cenário.
- Kael auxilia na abertura de um portal.
- Uma breve sequência narrativa demonstra a confiança crescente entre ambos.

Reutilizar comportamento, animações e sistemas existentes antes de criar uma IA complexa de companheiro.

---

## 5. IMPLEMENTAÇÃO INCREMENTAL

Não implementar todos os puzzles simultaneamente.

Ordem recomendada:

1. Auditar a progressão atual da campanha.
2. Documentar quais eventos dependem de Nocthar e dos outros chefes extras.
3. Preparar um plano de remoção e migração sem perda de dados.
4. Ajustar a sequência obrigatória Matriarca → Kael → Lucius.
5. Implementar um único puzzle piloto usando recursos existentes.
6. Validar objetivos, colisões, checkpoints, saves e conclusão.
7. Expandir para outros desafios somente após aprovação do piloto.

**Não adicionar todos os puzzles apenas porque foram sugeridos.** Avaliar quais se encaixam melhor nos mapas e na narrativa atual.

## 6. PRIORIDADE ARTÍSTICA

Respeitar a ordem:

1. Finalizar e validar Lucius Normal e Arconte, se ainda houver pendências críticas.
2. Integrar o novo design de Lyra.
3. Integrar o novo design da Matriarca.
4. Integrar o novo design de Kael.
5. Adaptar a campanha e implementar os desafios aprovados.

Produzir primeiro um sprite piloto por personagem.

Comparar Lyra e Kael diretamente com Fabius Normal, e a Matriarca na escala ligeiramente superior de chefe.

Não produzir todas as oito direções antes de conferir a qualidade do primeiro piloto.

As imagens anexadas são referências visuais, não atlas automaticamente prontos para o Unity.

Se as referências originais não estiverem disponíveis na execução, solicitar os arquivos corretos em vez de reconstruir designs a partir de miniaturas de baixa resolução.

## 7. CONTROLE DE COTAS

Preservar no mínimo **20% da cota semanal do Codex**.

Verificar o saldo real antes de iniciar novos lotes.

Priorizar:

- Reaproveitamento de código.
- Reutilização de sprites aprovados.
- Testes direcionados.
- Mudanças pequenas e reversíveis.
- Implementações que melhorem a experiência do jogador sem exigir grandes lotes artísticos.

Evitar redesenhar inimigos comuns ou criar sistemas complexos desnecessários.

## 8. TESTES E SEGURANÇA

Usar apenas uma instância da Unity.

Executar testes EditMode e PlayMode proporcionais às alterações, em modo silencioso e com saves isolados.

Validar especialmente:

- Progressão pelos dez mapas.
- Bossfight da Matriarca.
- Bossfight de Kael e sua posterior aliança.
- Acesso à bossfight de Lucius.
- Compatibilidade com saves antigos.
- Objetivos, checkpoints, portais e desbloqueios.
- Ausência de referências quebradas a Nocthar.
- Funcionamento do puzzle piloto.

Não gerar build, executar push ou publicar sem autorização explícita.

Não modificar áudio, site ou sistemas não relacionados.

Preservar alterações locais e recursos históricos.

## 9. ENTREGA ESPERADA

Apresentar um checkpoint com:

1. Nova sequência de chefes documentada.
2. Dependências de Nocthar identificadas.
3. Alterações realizadas e recursos desativados.
4. Estado dos designs de Lyra, Matriarca e Kael.
5. Puzzle piloto implementado, se autorizado pelo orçamento e pela etapa vigente.
6. Resultados reais dos testes.
7. Estado dos saves e progressão.
8. Arquivos alterados e commits locais.
9. Cota restante, quando verificável.
10. Próximas tarefas recomendadas.

**DIRETRIZ FINAL**

DARKLIGHT: REQUIEM deve priorizar qualidade e variedade, não quantidade de chefes.

A campanha principal terá Matriarca Rubra, Kael Voss e Lucius como seus três grandes confrontos.

Os demais espaços deverão oferecer exploração, mistério, puzzles e combates interessantes.

Preservar a identidade dos personagens, a progressão existente e o trabalho já aprovado.

Não avançar automaticamente para a próxima fase sem validar a atual.