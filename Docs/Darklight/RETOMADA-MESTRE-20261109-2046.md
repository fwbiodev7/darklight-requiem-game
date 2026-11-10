# DARKLIGHT: REQUIEM — RETOMADA MESTRE 09/11/2026

## 1. RETOMADA AUTOMÁTICA

Retomar o desenvolvimento de DARKLIGHT: REQUIEM **neste mesmo chat**, após conferir a renovação real da janela de cinco horas.

**Horários informados:**
- Reset previsto: 09/11/2026 às 20h46min24s.
- Retomada agendada: 20h49.
- Fuso: America/Sao_Paulo.

Não presumir que os tokens foram renovados. Conferir o saldo disponível antes de iniciar trabalhos custosos.

Workspace principal:

`C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main`

Ler prioritariamente:

- `Docs/Darklight/CHECKPOINT-CODEX-20261109.md`
- `Docs/Darklight/OPERACAO-OPENBOT-20261109.md`

Verificar também os recibos de commit, artefatos recentes e checkpoints produzidos pelas seis IAs do OpenBot.

O commit remoto anteriormente identificado foi `e57d956`, às 16h07 de 09/11. Confirmar o HEAD remoto atual, pois ele pode ter mudado.

**Não refazer auditorias completas já concluídas. Trabalhar incrementalmente.**

## 2. SEGURANÇA E INTEGRAÇÃO

Antes de modificar qualquer arquivo:

1. Conferir branch, HEAD, status, commits locais e alterações remotas.
2. Verificar quais modificações pertencem ao Codex e quais foram produzidas pelo OpenBot.
3. Preservar alterações concorrentes, saves, configurações e referências originais.
4. Não sobrescrever trabalho de outros agentes.
5. Não executar reset destrutivo, limpeza indiscriminada ou force push.
6. Criar commits pequenos, rastreáveis e separados por finalidade.
7. Confirmar que o push realmente chegou ao remoto.
8. Evitar cópias redundantes, relatórios repetidos e reprocessamento de assets já aprovados.

Preservar especialmente:

`ArtSources/FabiusForms20261109/`

Os designs de Ruptura e Eclipse Absoluto estão registrados como referências para etapas futuras.

Se existir uma sessão Unity ou operação Git concorrente, coordenar antes de continuar. Não iniciar segunda instância da Unity.

## 3. ESTADO CONFIRMADO — FABIUS NORMAL

Último estado informado:

- 200 sequências ativas.
- 610 figuras.
- 74 folhas ativas.
- 72 sequências de defesa, dano e morte recuperadas.
- 144 figuras recuperadas.
- Katana, Punhos e Moto cobertos nessas 72 sequências nas oito direções.
- Correções de morte, prévia de personagem, aliases e defesa da Moto implementadas.
- Os 21 erros C# anteriores foram corrigidos na compilação.
- Testes EditMode interrompidos pelo mecanismo de proteção de RAM.
- Nenhum XML novo de testes.
- Gate 1 de Fabius Normal permanece ABERTO.

Esses números são o último checkpoint informado, não prova de validação completa.

Não tratar compilação bem-sucedida como gameplay aprovado.

## 4. PRIORIDADE ABSOLUTA — FINALIZAR FABIUS NORMAL

Esta é a única etapa de produção autorizada até o fechamento do Gate 1.

### Fase A — Recuperação e validação

Verificar as 72 sequências recuperadas:

- Defesa.
- Dano.
- Morte.
- Entrada e saída dos estados.
- Duração e continuidade.
- Travamento correto das ações após morte.
- Pose final de morte.
- Direções, pivôs e ordenação visual.

Executar testes EditMode afetados quando houver RAM suficiente.

Não reduzir nem desativar os guardas de memória.

Se a Unity continuar bloqueada, registrar o motivo, preservar evidências e continuar somente tarefas seguras que não dependam da execução.

Não iniciar Play Mode, música ou testes que exijam autorização adicional sem autorização vigente.

### Fase B — Serra de Entrada

As oito sequências revisadas de Serra de Entrada já existem fora do catálogo ativo.

Recuperar, integrar, associar aos golpes corretos e validar.

Não redesenhar essas sequências sem encontrar defeitos concretos.

### Fase C — Carregamento de golpes

Investigar o estado `Charging`.

Procurar sequências, poses e materiais reaproveitáveis.

Implementar animação de carregamento funcional, com início, manutenção, interrupção e transição para o ataque.

Produzir arte nova somente se o material existente não oferecer leitura visual adequada.

### Fase D — Combate e ataques

Conferir os ataques das três armas:

**Katana:** Leve 1, Leve 2, Leve 3, pesados, especiais e ramificações implementadas.

**Punhos:** Direto, Chute, Ascensão, Giro, Martelo, Juízo, Resposta Brutal, Cataclismo, Queda, Espiral, Órbita e Sentença da Ruína.

**Moto:** Dentes Cruzados, Serra de Entrada, técnicas motorizadas, ataques especiais e transições existentes.

Verificar associações, timing, impactos, hitboxes, direção e continuidade.

Permitir reutilização inteligente de animações quando os movimentos forem compatíveis.

Evitar que golpes com funções visualmente diferentes pareçam idênticos, especialmente finalizadores e especiais.

Não gerar dezenas de animações extras desnecessárias.

### Fase E — Moto

Conferir montagem e desmontagem nas oito direções.

Existe uma sequência específica identificada para sul/frente.

Antes de produzir sete conjuntos adicionais, avaliar espelhamento, reutilização, interpolação ou transições existentes.

Escolher a implementação de menor custo que mantenha a qualidade da pixel art.

Verificar também:

- Faísca da defesa.
- Ponto de contato.
- Oclusão ao norte.
- Emissão única do efeito.
- Posicionamento da Moto em relação ao personagem.
- Transição correta entre andar, atacar, montar e desmontar.

### Fase F — Partículas e esquiva

Verificar partículas existentes de:

- Ataques.
- Defesa.
- Dano.
- Morte.
- Esquiva.
- Moto.

Corrigir profundidade, transparência, alinhamento e leitura visual.

Manter a estética dark fantasy e a identidade pixel art.

Não regenerar efeitos que já estejam corretos.

## 5. DEFINIÇÃO DE GATE 1 CONCLUÍDO

Fabius Normal será considerado concluído quando:

1. Todas as ações essenciais previstas estiverem implementadas e associadas corretamente.
2. Katana, Punhos e Moto estiverem utilizáveis.
3. Os ataques, especiais, esquivas, defesas, danos e mortes funcionarem.
4. As oito direções necessárias estiverem cobertas, inclusive por reutilização visualmente adequada.
5. Não existirem falhas críticas de pivô, transparência, escala, posicionamento ou continuidade.
6. A compilação estiver sem erros.
7. Os testes críticos afetados forem executados e aprovados.
8. Não houver bugs bloqueantes conhecidos nas funcionalidades verificadas.

Separar polimento opcional de defeitos obrigatórios.

**Não manter o Gate 1 aberto indefinidamente por detalhes cosméticos.**

Se houver testes bloqueados por falta de recursos, declarar o Gate 1 como pendente de validação, nunca como aprovado artificialmente.

## 6. REGRA DEFINITIVA DAS TRANSFORMAÇÕES

IMPORTANTE: esta seção define a implementação FUTURA. Não iniciar transformações antes do Gate 1.

**Ruptura e Eclipse Absoluto NÃO precisam de novas animações completas.**

Usar a totalidade das animações existentes de Fabius Normal como base.

Manter exatamente:

- Corpo e proporções.
- Rosto e pele.
- Roupas e acessórios.
- Armas e movimentos.
- Direções e poses.
- Quadros e timing.
- Hitboxes, pivôs e gameplay.

Modificar SOMENTE:

**Ruptura**
- Cabelo em tonalidade carmesim, baseado na referência aprovada.
- Aura vermelha/carmesim contida, com estética vampírica e referência de intensidade ao Super Saiyajin God.

**Eclipse Absoluto**
- Cabelo prateado/branco, baseado na referência aprovada.
- Aura violeta misturada com branco, inspirada na energia de Bills/Beerus, sutil e sem exageros.

Implementar preferencialmente com máscaras de cabelo, materiais, shaders ou processamento automatizado dos sprites existentes, conforme compatibilidade com a Unity e fidelidade visual.

Utilizar um sistema de aura reutilizável, associado às posições e animações existentes.

Não gerar centenas de sprites do zero nem alterar pele, olhos, roupas ou anatomia.

Preservar a mesma resolução, densidade, escala e estética dos pixels.

Reutilizar o mesmo sistema de combate e as animações de todas as armas nas duas transformações.

A troca de forma deverá alterar somente os aspectos visuais autorizados e os atributos/poderes já previstos pelo sistema.

Testar ativação, desativação, morte, troca de arma, esquiva e transições entre formas quando essa etapa for iniciada.

**Objetivo: duas formas novas com o mínimo possível de trabalho artístico redundante.**

## 7. ORDEM DE PRODUÇÃO APÓS GATE 1

Respeitar a esteira:

1. Fabius Normal — finalizar Gate 1.
2. Lucius Normal — finalizar personagem e animações.
3. Bosses — melhorar especialmente walk, ataques e fluidez, eliminando rigidez e quadros insuficientes.
4. Gothic Eclipse — evolução visual e atmosfera dark fantasy aplicada ao material existente, sem refazer mapas desnecessariamente.
5. Transformações — Ruptura e Eclipse Absoluto por reutilização de animações, cabelo e aura; formas de Lucius conforme escopo aprovado.
6. Combate IA 2.0 — comportamento avançado, cancelamentos, perseguição, respostas e equilíbrio.
7. Testes integrados, otimização, build Windows e publicação.

Não iniciar mapas novos, site, narrativa ou expansões paralelas sem autorização.

Após o fechamento de cada etapa, registrar o resultado antes de avançar.

## 8. OPENBOT, SUBAGENTES E ECONOMIA DE TOKENS

Utilizar os seis agentes do OpenBot quando disponíveis e úteis.

As tarefas devem ser independentes, bem delimitadas e sem conflitos de escrita.

Sugestão de responsabilidades, conforme as capacidades disponíveis:

- JARVIS: coordenação, checkpoints e integração.
- FRIDAY: inspeção de animações e qualidade visual.
- EDITH: validações, testes e identificação de regressões.
- ULTRON: engenharia, scripts e otimização.
- Agentes restantes: apoio especializado em arte, partículas e integração quando necessário.

Não abrir subagentes apenas para repetir uma análise já concluída.

Preferir verificações pontuais, diffs, scripts automatizados e reutilização de materiais.

Usar o Codex como responsável pela integração final e validação Unity, respeitando a divisão de trabalho definida nos documentos.

Economia obrigatória:

- Não repetir leitura integral de arquivos sem necessidade.
- Não realizar novas auditorias gerais.
- Não regenerar sprites aprovados.
- Não criar documentação excessiva.
- Não duplicar trabalho entre agentes.
- Não executar builds pesadas desnecessariamente.
- Priorizar entregas concretas sobre relatórios extensos.

Quando o saldo atingir 10%, interromper novos lotes de arte e tarefas custosas.

Reservar os tokens restantes para:

1. Registrar alterações e resultados.
2. Atualizar o checkpoint de continuidade.
3. Fazer commit somente do escopo autorizado.
4. Confirmar push remoto.
5. Registrar pendências e próximos passos.

## 9. FECHAMENTO E CONTINUIDADE

Antes de encerrar qualquer janela:

- Registrar o commit local e remoto.
- Identificar arquivos modificados.
- Registrar animações integradas e pendentes.
- Registrar testes executados, aprovados, falhos e bloqueados.
- Atualizar o estado real do Gate 1.
- Preservar referências, saves e trabalho concorrente.
- Registrar a próxima ação exata, sem criar uma nova auditoria desnecessária.

Se houver bloqueio técnico, documentar a causa e continuar apenas tarefas independentes seguras.

Não declarar conclusão sem evidências.

## 10. COMANDO FINAL

**EXECUTAR A RETOMADA A PARTIR DO ÚLTIMO CHECKPOINT VERIFICADO.**

Prioridade absoluta: concluir Fabius Réquiem Normal com qualidade, reaproveitando ao máximo os recursos existentes.

Não ampliar o escopo antes do Gate 1.

Preservar a fidelidade da pixel art e a qualidade dos golpes, mas evitar perfeccionismo que impeça o avanço do projeto.

As transformações devem ser implementadas futuramente por troca de cabelo e adição de aura, reaproveitando todas as animações normais.

Não solicitar confirmação para tarefas já autorizadas e seguras. Respeitar permissões vigentes para operações sensíveis.

**Trabalhar, validar, salvar e continuar.**ATUALIZAÇÃO OBRIGATÓRIA — ESCOPO REDUZIDO DE LUCIUS
Após finalizar Fabius Normal, implementar Lucius com o máximo de reutilização possível.
Lucius terá somente:
- Forma Normal.
- Uma única transformação: Nexo Fenrir.
- Uma única arma: sua espada característica.
Usar Fabius como base técnica para movimento, direções, animações compatíveis, estados, transições, dano, defesa, esquiva e morte.
Adaptar as animações de espada já existentes, respeitando o tamanho, empunhadura e identidade visual da arma de Lucius.
Criar somente os ataques exclusivos indispensáveis para caracterizar Lucius como boss principal.
Para Nexo Fenrir, reutilizar a estrutura e os movimentos do Normal, modificando apenas os elementos visuais e efeitos necessários.
Preservar o design aprovado de Lucius. Reutilizar a base técnica não significa copiar sua aparência de Fabius.
PROIBIDO ampliar o escopo com novas armas, várias transformações ou centenas de animações redundantes.
Objetivo: Lucius completo, funcional, visualmente distinto e com uma bossfight marcante, no menor tempo possível, sem sacrificar a qualidade essencial.
Após concluir e validar Lucius e as melhorias necessárias dos bosses, continuar Gothic Eclipse, transformações e Combate IA 2.0 conforme a esteira estabelecida.