# DARKLIGHT: REQUIEM — ESCOPO ENXUTO DE LUCIUS NORMAL

**Aplicação:** após a conclusão e validação de Fabius Réquiem Normal e a implementação do Gothic Eclipse.

**Objetivo:** implementar Lucius Normal como antagonista jogável pela IA, com combate funcional, visual canônico e o mínimo necessário de animações exclusivas.

## 1. Regras de escopo

Não reproduzir o processo extenso de redesign de Fabius.

Lucius terá um conjunto reduzido de animações, com reutilização inteligente de clips, lógica e sistemas existentes.

Priorizar:

- Identidade visual.
- Silhueta reconhecível.
- Movimentos fluidos.
- Combate responsivo.
- Bossfight desafiadora.
- Integração rápida e segura.

Não criar uma animação individual para cada nome de técnica.

Não produzir transformações nesta etapa.

## 2. Visual canônico

Lucius é o irmão gêmeo de Fabius.

Preservar:

- Mesma altura e proporções adultas de Fabius.
- Estrutura corporal e estilo pixel art compatíveis.
- Cabelo claro, com estilo inspirado em Mirai Trunks.
- Rosto inspirado na referência aprovada de Ville Valo.
- Olhos azuis.
- Sobretudo preto e azul-escuro.
- Espada grande nas costas.
- Silhueta elegante e ameaçadora.
- Sem armadura aparente.
- Sem capuz na versão principal.

Não copiar o rosto de Fabius integralmente nem modificar sua identidade visual aprovada.

Utilizar `WorldVisualScale = 1.325`.

Aproveitar o esqueleto lógico, os sistemas de direção e a infraestrutura de animação do Fabius quando tecnicamente compatíveis.

Não reutilizar sprites visuais de Fabius como resultado final de Lucius.

## 3. Conjunto mínimo de animações

### Locomoção

- Idle em oito direções.
- Walk em oito direções.
- Corrida reutilizando Walk com ajustes de velocidade quando visualmente aceitável.
- Esquiva reutilizando deslocamento e transições coerentes.

Não criar variações cosméticas independentes.

### Ataques

**Família 1 — Combo básico**

Um combo de três golpes de espada grande.

Reutilizar suas fases para variações de ataques leves.

**Família 2 — Ataque pesado**

Um golpe forte com antecipação clara, impacto marcante e recuperação perceptível.

**Família 3 — Técnica especial**

Uma técnica exclusiva de Lucius, com energia azul-escura e efeitos próprios.

Priorizar partículas para diferenciação visual, evitando multiplicar sprites corporais.

Cada família deverá possuir cobertura direcional coerente, sem espelhamentos anatômicos incorretos.

### Combate aéreo

Reutilizar golpes-base existentes quando compatíveis.

Perseguição aérea deve funcionar como deslocamento, sem impacto artificial.

Não criar cinco sequências aéreas independentes.

### Estados essenciais

Produzir somente:

- Defesa.
- Dano.
- Morte.

Carregamento reutiliza antecipação existente.

Não produzir estados cosméticos redundantes.

## 4. IA inicial de Lucius

Criar uma bossfight funcional utilizando a infraestrutura de combate existente.

Comportamentos mínimos:

- Aproximação e perseguição.
- Controle de distância.
- Combo básico.
- Ataque pesado.
- Técnica especial.
- Defesa contextual.
- Reação ao dano.
- Recuperação após ataques.
- Morte e encerramento correto da batalha.

Manter janelas de reação justas.

Evitar leitura perfeita de inputs, ataques inevitáveis e combos infinitos.

Não implementar a IA 2.0 completa nesta etapa; ela pertence à fase posterior do plano mestre.

## 5. Reutilização técnica

Reaproveitar, quando adequado:

- Sistema de oito direções.
- Controladores de animação.
- Máquina de estados.
- Eventos de ataque.
- Hitboxes e hurtboxes.
- Sistema de partículas.
- Movimentação e colisão.
- Temporizadores e cooldowns.
- Infraestrutura de combate aéreo.
- Sistemas de dano, defesa e morte.

Não duplicar scripts inteiros de Fabius para criar Lucius.

Preferir componentes reutilizáveis e dados específicos de cada personagem.

Separar dados de Lucius dos dados de Fabius para evitar alterações involuntárias no protagonista.

## 6. Gothic Eclipse

Lucius deve ser visualmente compatível com a atmosfera Gothic Eclipse já implementada.

Usar:

- Contraste azul e preto.
- Partículas de energia azulada.
- Sombras e iluminação atmosférica.
- Efeitos discretos de espada.
- Feedback visual forte nos ataques especiais.

Não reconstruir mapas ou sistemas de iluminação apenas para Lucius.

## 7. Produção e integração

Ordem:

1. Recuperar referências canônicas de Lucius.
2. Implementar visual Normal.
3. Integrar Idle e Walk.
4. Integrar três famílias ofensivas.
5. Integrar defesa, dano e morte.
6. Configurar IA básica.
7. Integrar efeitos e partículas.
8. Executar testes finais.
9. Corrigir defeitos comprovados.

Usar até três subagentes com arquivos exclusivos.

Somente o coordenador controla Unity, Git, Assets e publicação.

Não criar novos lotes sem necessidade comprovada.

## 8. Critérios de conclusão

Lucius Normal estará concluído quando:

- O personagem estiver visualmente fiel à referência aprovada.
- As oito direções funcionarem.
- As três famílias ofensivas estiverem integradas.
- Defesa, dano e morte funcionarem.
- A IA conseguir combater Fabius.
- Hitboxes e eventos estiverem sincronizados.
- Não houver erros críticos de animação.
- A bossfight puder começar, acontecer e terminar corretamente.
- Os testes finais forem aprovados.

Não declarar conclusão apenas por importar sprites.

## 9. Próximas etapas

Após concluir Lucius Normal:

1. Transformações de Fabius.
2. Transformação Nexo Fenrir de Lucius.
3. Combate IA 2.0.
4. QA, otimização e build Windows final.

Não antecipar essas etapas.

**DIRETRIZ FINAL:** Lucius deve parecer um antagonista completo, não um Fabius recolorido. Priorizar personalidade, qualidade e funcionalidade, com produção gráfica mínima e reutilização técnica máxima.
