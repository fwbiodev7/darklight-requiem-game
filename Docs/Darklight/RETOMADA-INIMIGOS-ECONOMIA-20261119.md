# Prompt de retomada — Redesign dos inimigos com economia máxima de tokens

Registrado em 18/11/2026 para a próxima sessão, em 19/11/2026. Este registro não inicia trabalho nem agenda execução. Conferir HEAD, alterações locais e checkpoints antes de retomar; as informações de estado abaixo são as fornecidas no pedido.

**DARKLIGHT: REQUIEM — continuação após Lucius 2.0**

Estamos atualmente na etapa **Lucius 2.0**, ainda em implementação e sem testes autorizados.
A próxima frente, **após concluir a revisão lógica de Lucius 2.0**, será:

## Fase seguinte autorizada

**Redesign dos inimigos e demais bosses**, com foco em **economia máxima de tokens**.

## Diretriz principal desta fase

**NÃO criar novas artes extensas do zero se puder reutilizar o que já existe.**
A regra agora é:

- basear novas artes dos inimigos **nos assets já existentes e aprovados**
- reaproveitar sprites, silhuetas, proporções, poses e animações sempre que possível
- fazer **variações controladas** por cima do material atual
- priorizar **edições pontuais** em vez de folhas totalmente novas
- evitar qualquer expansão cara semelhante ao que ocorreria com o Combate 2.0 artístico de Fabius

## Objetivo visual

Melhorar os inimigos e bosses em:

- identidade visual
- legibilidade dos ataques
- coerência com o clima de DARKLIGHT: REQUIEM
- distinção entre classes de inimigos
- leitura rápida em gameplay

Sem descaracterizar o que já funciona.

## Estratégia obrigatória de economia

Ao trabalhar nos inimigos, seguir esta ordem:

### 1. Auditoria do material existente

Levantar os inimigos e bosses já existentes e classificar:

- quais já estão aceitáveis
- quais precisam apenas de pequenos ajustes
- quais precisam de redesign parcial
- quais realmente exigiriam arte nova no futuro

### 2. Redesign por reaproveitamento

Antes de qualquer geração nova, tentar resolver com:

- recoloração
- troca de detalhes de roupa/acessórios
- ajustes de cabelo/rosto/máscara/olhos/aura
- alterações de silhueta leve
- pequenas mudanças em armas já existentes
- reaproveitamento de poses e tempos de animação
- reedição localizada em sprites já aprovados

### 3. Evitar folhas completas desnecessárias

Não abrir produção de sprite sheets extensas sem necessidade comprovada.

Se houver necessidade de arte nova, priorizar:

- poucas direções primeiro
- poucos frames por teste
- um piloto representativo, com expansão condicionada à aprovação explícita do usuário
- um inimigo por vez
- uma classe prioritária por vez

### 4. Preservar compatibilidade

Toda proposta artística deve, sempre que possível:

- manter pivôs consistentes
- manter proporções compatíveis com colisão atual
- evitar quebrar animações/aproveitamentos já existentes
- minimizar retrabalho na Unity

### Controle de produção artística e aprovação

Durante o redesign dos inimigos e bosses:

- Priorizar reutilização de sprites e animações existentes.
- Permitir novas artes pontuais quando o reaproveitamento comprometer a identidade visual.
- Produzir inicialmente apenas um piloto representativo por inimigo ou classe.
- Não expandir o piloto para outras direções, ataques ou animações sem aprovação explícita do usuário.
- Preservar os sprites originais e manter possibilidade de rollback.
- Evitar alterações no visual canônico de Fabius e Lucius.
- Reutilizar efeitos, materiais e componentes existentes sempre que possível.
- Apresentar o custo estimado de produção antes de iniciar qualquer lote artístico extenso.
- Não interpretar a aprovação de um piloto como autorização automática para produzir todas as animações.

Esta regra prevalece sobre qualquer orientação histórica de expansão artística autônoma nesta frente.

## Restrições importantes

- **Não retomar** o pacote artístico do Combate 2.0 de Fabius.
- Não criar novas animações extensas de Fabius Normal, Ruptura ou Serafim Carmesim.
- Essa decisão foi suspensa por custo/tempo e deve permanecer congelada.
- **Testes continuam suspensos** até nova autorização.
- Não gerar build, não pushar e não rodar validação runtime sem ordem explícita.
- **Map Remaster 2.0 continua proibido antes de 19/11 às 21:11 Brasília** e somente depois das fases anteriores.

## Ordem atual do projeto

1. **Concluir Lucius 2.0**
2. **Redesign dos inimigos e demais bosses** com reaproveitamento máximo
3. **Evolução gráfica / Gothic Eclipse / mapas**
4. **Cutscenes cinematográficas**
5. **Polimento, navegação e áudio**
6. **Fechamento e build**, somente quando autorizado

## Situação já existente que deve ser preservada

- Loading concluído e aprovado
- Menus implementados em código e aguardando avaliação manual
- Combate 2.0 parcial mantido
- Artes dos Punhos suspensas e inativas
- Último commit local: `dcd3119`
- Sem build e sem push

## Entrega esperada nesta próxima fase

Ao iniciar o redesign dos inimigos, produzir primeiro um **plano técnico enxuto**, contendo:

1. lista dos inimigos/bosses atuais
2. avaliação do que pode ser reaproveitado
3. proposta de redesign de baixo custo para cada grupo
4. prioridade de execução do menor custo para o maior custo
5. indicação clara do que pode ser resolvido **sem novas gerações**
6. indicação do que realmente exigiria novas artes, se houver

## Regra final

O foco agora é **maximizar resultado visual com mínimo consumo de tokens**.
Preferir **reutilização inteligente, edição localizada e consistência** em vez de refações completas.

---

## Resumo do que esse prompt manda fazer

Ele basicamente diz pro Codex:

- terminar primeiro o **Lucius 2.0**
- depois mexer nos inimigos
- **sem sair gerando arte nova à toa**
- reaproveitando o máximo do que já existe
- fazendo redesign leve e inteligente
- deixando geração pesada só como último caso

---

## Em uma frase

**A nova regra é: redesign de inimigos por reaproveitamento, não por reconstrução completa.**

> Trecho final do texto fornecido, preservado como citação e sem ação solicitada: “Se você quiser, eu também posso fazer agora uma **versão mais curta e mais direta desse prompt**, estilo ‘copiar e colar no Codex sem enrolação’.”

## Referências para retomar

- `weekly_reset_prompt.md`
- `Docs/Darklight/RELATORIO-FINAL-20261118.md`
- `Docs/Darklight/CHECKPOINT-RETOMADA-20261118.md`
- `Docs/Darklight/MAP-REMASTER-2-PRIORIDADE-20261119.md`

Preservar a reserva mínima de 20% semanal e conferir a cota real antes de trabalho sustentado. Revisão lógica não equivale a teste executado: registrar Lucius como não validado enquanto os testes permanecerem suspensos.
