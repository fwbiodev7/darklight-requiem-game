DARKLIGHT: REQUIEM — RETOMADA DE 01/11/2026

Objetivo principal

Retomar o desenvolvimento de DARKLIGHT: REQUIEM no mesmo workspace, dando continuidade ao trabalho realizado em 31/10/2026.

Prioridade absoluta: concluir os personagens, suas animações e o sistema de Combate 2.0.

Adicionar uma atmosfera Dark Fantasy cinematográfica aos mapas existentes, sem reconstruí-los, redesenhá-los ou substituir seus assets.

Trabalhar com qualidade, economia de tokens, subagentes especializados e commits seguros.

1. Auditoria e retomada

Antes de começar:

- Inspecionar o Git e identificar o checkpoint mais recente.
- Verificar o que foi realmente concluído em 31/10.
- Identificar todas as animações de Fabius produzidas hoje.
- Não refazer animações aprovadas.
- Verificar sprites ainda pendentes, problemas de importação e integração.
- Preservar a branch de publicação correta e os merges já realizados.
- Manter Unity 6000.6.0f1 e "WorldVisualScale = 1.325".

O checkpoint anterior conhecido era "86b93e8", mas deve ser substituído pelo estado real mais recente encontrado no repositório.

2. PRIORIDADE 1 — Finalização de Fabius

Como grande parte das animações pendentes foi produzida em 31/10, não reiniciar o redesign.

Verificar e concluir apenas o que realmente faltar:

- Katana e seus combos.
- Punhos e suas técnicas.
- Moto e suas animações de combate.
- Movimentação, esquivas e transições.
- Oito direções reais.
- Formas Normal, Ruptura e Eclipse Absoluto.
- Correção de sprites antigos misturados aos novos.
- Importação, timings, eventos e hitboxes.

Dar atenção às pendências anteriores da Katana Leve3 e do Giro Devastador, caso ainda estejam abertas.

Meta: deixar Fabius funcional, consistente e visualmente completo antes de produzir conteúdo novo desnecessário.

3. PRIORIDADE 2 — Implementação de Lucius

Após estabilizar Fabius, concentrar os subagentes artísticos na implementação de Lucius.

Design canônico

Lucius é o irmão gêmeo e antagonista de Fabius.

Preservar:

- Proporções e estilo de pixel art compatíveis com Fabius.
- Sobretudo preto e azul.
- Cabelos claros.
- Olhos azuis.
- Arsenal próprio, diferente do protagonista.

Formas

Implementar somente:

1. Normal.
2. Nexo Fenrir, fusão das transformações anteriormente planejadas.

Não reintroduzir Ascensão, Fenrir e Nexo como transformações jogáveis separadas.

Animações

Produzir e integrar o conjunto necessário para o combate:

- Idle e movimentação.
- Ataques e combos.
- Esquivas e cancelamentos.
- Reações a dano.
- Lançamentos e perseguições aéreas.
- Técnicas especiais.
- Transformação Nexo Fenrir.
- Animações de bossfight.

Priorizar o conjunto efetivamente necessário para a luta contra Lucius. Não produzir centenas de sprites redundantes.

A versão encapuzada deve continuar misteriosa e respeitar a narrativa, sem revelar antecipadamente sua identidade.

4. PRIORIDADE 3 — Combate 2.0 e IA

Evoluir o combate em tempo real inspirado em Devil May Cry 5.

IA avançada

Implementar ou aperfeiçoar:

- Leitura contextual das ações do jogador.
- Esquivas e contra-ataques.
- Comportamentos ofensivos e defensivos.
- Variação de combos.
- Reações às transformações.
- Perseguição aérea.
- Gerenciamento de distância.
- Diferentes níveis de agressividade.
- Janelas de vulnerabilidade claras.
- Resistência especial para chefes.

Evitar IA injusta que simplesmente leia os inputs e reaja instantaneamente a tudo.

Combate de Lucius

Lucius deve possuir uma identidade própria, com movimentação e técnicas diferentes das de Fabius.

A bossfight deve explorar velocidade, teletransportes quando coerentes com seu design, contra-ataques, combos aéreos e a transformação Nexo Fenrir.

Não copiar diretamente os movimentos do protagonista.

Preservação

Manter os sistemas já funcionais, incluindo as seis técnicas da moto e as limitações contra combos infinitos.

Testar todos os novos comportamentos em gameplay real.

5. PRIORIDADE 4 — Dark Fantasy Atmospheric Overhaul

ATENÇÃO: NÃO REDESENHAR, RECONSTRUIR OU REFAZER OS MAPAS.

Os dez mapas existentes devem permanecer intactos em sua geometria, tilesets, colisões, estruturas e posicionamento de objetos.

A proposta é adicionar uma camada atmosférica configurável sobre os ambientes atuais.

Efeitos desejados

- Chuva leve, moderada e intensa.
- Névoa em camadas.
- Ventos e partículas ambientais.
- Iluminação sombria cinematográfica.
- Relâmpagos ocasionais.
- Ajustes de cor por região.
- Sons ambientes de chuva, vento e tempestades.
- Iluminação temporária dos ataques e transformações.

Criar um sistema reutilizável, como "DarklightAtmosphereController", respeitando a arquitetura existente.

Regras técnicas

- Não substituir sprites dos mapas.
- Não reconstruir cenários com parallax novo.
- Não alterar colisões.
- Não modificar o layout das fases.
- Não converter o projeto inteiro para outro pipeline gráfico sem necessidade.
- Não adicionar sistemas pesados desnecessários.
- Permitir ativar e desativar efeitos.
- Criar presets atmosféricos por região.
- Garantir legibilidade do combate.

A atmosfera deve funcionar como uma melhoria visual não destrutiva.

Primeiro validar o sistema em um mapa existente e, se estiver estável, aplicar presets aos demais.

Limite de prioridade: não consumir o tempo destinado a Lucius ou ao Combate 2.0 com polimento atmosférico.

Se o tempo for insuficiente, deixar a atmosfera parcialmente implementada e documentada, sem prejudicar as prioridades principais.

6. Distribuição dos subagentes

Organizar o trabalho conforme as dependências:

Subagente artístico A: concluir e validar pendências de Fabius.

Subagente artístico B: produzir e integrar as animações canônicas de Lucius.

Subagente técnico C: desenvolver Combate 2.0, IA e bossfight.

Subagente técnico D: implementar a atmosfera Dark Fantasy não destrutiva, somente quando não competir com as prioridades principais.

QA: validar importações, animações, combate, saves, desempenho e build.

Evitar edição concorrente dos mesmos arquivos.

Utilizar Ultra nas tarefas de maior complexidade artística e técnica, economizando recursos em verificações mecânicas.

7. Testes e validação

Testar:

- Fabius nas três formas.
- Lucius nas duas formas.
- Combate terrestre e aéreo.
- IA e bossfight.
- Timings e transições de animação.
- Colisões e hitboxes.
- Integridade dos mapas originais.
- Desempenho com efeitos atmosféricos.
- Saves e carregamentos.
- Compilação e inicialização da campanha.

Não declarar sistemas concluídos sem testes correspondentes.

8. Entrega final

Ao encerrar a sessão:

1. Gerar uma build jogável para Windows, se o ambiente permitir.
2. Registrar quais animações foram concluídas.
3. Informar o progresso real de Fabius e Lucius.
4. Documentar o estado do Combate 2.0.
5. Mostrar capturas dos efeitos atmosféricos implementados, caso existam.
6. Executar os testes disponíveis.
7. Fazer commits e push seguros.
8. Atualizar o checkpoint de continuidade.

Antes do fim dos tokens, priorizar salvar o trabalho, consolidar os commits e documentar pendências.

Regra final

Não aumentar novamente o escopo do redesign.

Não refazer os dez mapas, não recriar animações aprovadas e não adicionar novas transformações ou armas.

A prioridade é transformar o progresso já produzido em um jogo cada vez mais completo e jogável.

A atmosfera Gothic Eclipse é uma melhoria complementar, não um novo projeto artístico.

Ordem obrigatória: Fabius → Lucius → Combate 2.0 → Atmosfera Dark Fantasy → Build e QA.