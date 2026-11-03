# DARKLIGHT: REQUIEM — PLANO MESTRE DO PRÓLOGO CINEMATOGRÁFICO

*Status:* Planejado, implementação futura

*Início autorizado:* Após conclusão e validação de Lucius Normal

*Prioridade:* Nova etapa narrativa, antes das transformações e do Combate IA 2.0

*Engine:* Unity 6, 2D Pixel Art

## 1. Objetivo narrativo

Implementar um prólogo cinematográfico jogável em DARKLIGHT: REQUIEM para apresentar a relação entre Fabius e Lucius, ensinar os fundamentos do combate e estabelecer o mistério que sustentará a campanha.

A estrutura será:

*INFÂNCIA → DUELO COM ESPADAS DE MADEIRA → TRANSIÇÃO À LUTA FINAL → GOLPE DECISIVO MISTERIOSO → FLASHBACK → CAMPANHA PRINCIPAL → RETORNO À LUTA FINAL.*

O primeiro adversário de Fabius também será o último.

*Regra:* a história deve despertar curiosidade sobre a ruptura entre os irmãos sem antecipar o destino de nenhum deles.

Não modificar retroativamente a campanha inteira sem necessidade. Integrar a narrativa por cenas adicionais, transições e acontecimentos já existentes.

---

## 2. Etapa A — Preparação e análise técnica

Antes de implementar:

1. Ler AGENTS.md e o estado mais recente do projeto.
2. Confirmar Fabius e Lucius Normal concluídos e utilizáveis.
3. Inspecionar os sistemas existentes de combate, movimentação, câmera, diálogos, cenas e saves.
4. Identificar componentes reutilizáveis.
5. Construir uma matriz de dependências e um storyboard técnico.
6. Registrar quais partes precisam de código novo e quais podem reutilizar a infraestrutura existente.

Não duplicar controladores funcionais.

Preservar os saves, GUIDs, metadados, configurações, progressão e assets aprovados.

Gerar um checkpoint inicial antes de qualquer integração.

---

## 3. Etapa B — Personagens infantis

### Fabius criança

Criar uma versão infantil do protagonista, mantendo características reconhecíveis do personagem.

Características:
- Cabelo semelhante ao de Fabius, adaptado à infância.
- Proporções infantis consistentes e próprias.
- Roupa simples de treinamento.
- Espada de madeira.
- Expressões faciais específicas.
- Animações originais em pixel art.

### Lucius criança

Criar versão infantil de Lucius que demonstre sua semelhança física com Fabius.

Diferenças discretas de cabelo, acessórios e roupas poderão facilitar a identificação.

A aparência deve transmitir que são irmãos próximos, não inimigos.

### Animações mínimas

Para cada criança:

- Idle.
- Movimentação nas oito direções.
- Ataque leve.
- Segundo ataque de combo.
- Defesa.
- Esquiva.
- Reação a impacto.
- Animação especial de duelo.
- Vitória e derrota amigável.

O treinamento terá um conjunto limitado de técnicas, sem exigir a quantidade de animações do combate adulto.

*Não criar sprites infantis simplesmente diminuindo os adultos.* A anatomia deve ser desenhada corretamente.

Artes devem ser produzidas e aprovadas em staging antes de entrar em Assets.

---

## 4. Etapa C — Cenário de treinamento

Criar uma área pequena e independente da campanha principal.

*Conceito:* pátio de treinamento durante a infância dos irmãos.

Elementos visuais:
- Piso de pedra ou madeira.
- Árvores e vegetação discreta.
- Iluminação quente de fim de tarde.
- Som ambiente tranquilo.
- Espadas de treinamento.
- Poucos elementos interativos.

O cenário deve contrastar com a atmosfera dark fantasy do presente.

Não criar um mapa extenso. Priorizar composição, animação e narrativa.

### Câmera

Implementar enquadramentos específicos para:
- Apresentação dos irmãos.
- Início do treinamento.
- Primeira colisão de espadas.
- Diálogo de encerramento.
- Transição cinematográfica.

Utilizar pixel-perfect e o sistema de câmera existente quando possível.

---

## 5. Etapa D — Tutorial jogável

*Duração-alvo:* 4–6 minutos.

O tutorial deve ocorrer naturalmente durante a luta com Lucius, sem parecer uma sequência de menus.

### Fase 1 — Movimentação

Lucius desafia Fabius a alcançá-lo.

Ensinar:
- Caminhada.
- Oito direções.
- Aproximação.
- Posicionamento.

### Fase 2 — Ataques

Lucius apresenta as espadas de madeira.

Ensinar:
- Ataque básico.
- Combo de dois golpes.
- Janela de recuperação.
- Alcance da arma.

### Fase 3 — Defesa e esquiva

Lucius passa a atacar lentamente.

Ensinar:
- Defesa.
- Esquiva.
- Timing.
- Contra-ataque simples.

### Fase 4 — Primeiro parry

Lucius utiliza um golpe claramente anunciado.

O jogador aprende a defender no momento correto.

Esse golpe será reutilizado de maneira significativa na bossfight final.

### Fase 5 — Duelo livre

Liberar um confronto curto com os comandos já ensinados.

A luta termina quando Fabius ou Lucius vence.

Ambos os resultados devem permitir continuar a história. As diferenças serão apenas pequenos diálogos e reações.

### Regras funcionais

- Sem dano letal.
- Sem game over convencional.
- Feedback visual claro dos impactos.
- Hitboxes coerentes.
- Sem softlocks.
- Tutorial acessível por controle e teclado.
- Possibilidade de repetir instruções.
- Opção de pular tutoriais após a primeira conclusão.

---

## 6. Etapa E — Cutscene da luta final

*Duração-alvo:* 60–120 segundos.

O último golpe da luta infantil deve se conectar visualmente a um ataque entre Fabius e Lucius adultos.

### Sequência cinematográfica

*Cena 1 — Espadas de madeira*

Os irmãos avançam em direção um ao outro.

A câmera aproxima as lâminas antes do impacto.

*Cena 2 — Transição por correspondência visual*

No momento do choque, o cenário muda.

A espada de madeira se torna a katana de Fabius, enquanto Lucius surge com sua arma adulta.

O ambiente tranquilo desaparece, revelando uma arena destruída sob a atmosfera Gothic Eclipse.

*Cena 3 — Confronto do futuro*

Apresentar uma troca curta de golpes extremamente cinematográfica:

- Ataques rápidos.
- Bloqueios e parries.
- Esquivas.
- Breves deslocamentos aéreos.
- Impactos com partículas.
- Close-ups dos rostos.
- Destruição ambiental limitada.
- Efeitos de câmera e iluminação.

Utilizar animações existentes quando forem adequadas. Criar exclusivamente os quadros especiais necessários para a coreografia.

Não exigir transformações ainda não implementadas.

*Cena 4 — O golpe decisivo*

Lucius executa uma versão mortal do golpe ensinado na infância.

Fabius tenta responder.

A câmera acompanha as armas.

Ocorre um impacto forte.

*CORTAR PARA PRETO ANTES DE REVELAR O RESULTADO.*

Não mostrar morte confirmada, ferimento fatal explícito ou vencedor.

A conclusão desse confronto será revelada apenas na etapa final da campanha.

---

## 7. Etapa F — Início da campanha como flashback

Após a tela preta:

1. Reproduzir um breve elemento sonoro ou visual de transição.
2. Apresentar o texto discreto: *ANOS ANTES...*
3. Mostrar o título DARKLIGHT: REQUIEM.
4. Iniciar a primeira fase da campanha principal.
5. Entregar o controle ao jogador.

A narrativa se desenvolverá cronologicamente até alcançar o confronto visto no prólogo.

### Regras importantes

- Preservar os dez mapas planejados.
- Não reiniciar a progressão ao voltar ao passado.
- Integrar os diálogos novos sem destruir a narrativa existente.
- Evitar revelações antecipadas sobre Lucius.
- Manter a continuidade entre cutscenes e gameplay.
- Garantir que saves e carregamentos retomem o capítulo correto.

---

## 8. Etapa G — Retorno ao confronto final

Esta etapa será integrada à bossfight final quando o restante da campanha estiver pronto.

Ao chegar ao último confronto:

1. Reproduzir a coreografia inicial vista no prólogo.
2. Reutilizar os mesmos enquadramentos e eventos.
3. Recuperar o momento do golpe misterioso.
4. Continuar a cena além do ponto anteriormente ocultado.
5. Devolver o controle ao jogador na transição apropriada.

### Mecânica narrativa especial

Lucius executará o ataque que ensinou a Fabius durante a infância.

O jogador reconhecerá sua preparação e terá a oportunidade de responder usando o conhecimento adquirido no tutorial.

A execução do parry deverá ser justa, claramente sinalizada e compatível com o combate atual.

O sucesso poderá proporcionar uma vantagem, mas não deve exigir precisão injusta nem provocar bloqueio irreversível da progressão.

*Objetivo:* transformar a primeira lição dos irmãos no momento decisivo do confronto.

Não depender de Quick Time Events obrigatórios ou instant kills.

---

## 9. Etapa H — Arquitetura Unity

Priorizar a reutilização dos sistemas existentes.

Criar componentes novos apenas quando não houver equivalentes adequados.

### Responsabilidades sugeridas

*PrologueSequenceDirector*
- Controlar a sequência do prólogo.
- Gerenciar checkpoints cinematográficos.
- Acionar transições.

*TrainingDuelController*
- Controlar o combate amigável.
- Aplicar regras não letais.
- Gerenciar objetivos do tutorial.

*TutorialObjectiveManager*
- Controlar o progresso das lições.
- Exibir instruções contextuais.
- Validar comandos executados.

*FinalDuelTeaserController*
- Reproduzir a coreografia inicial.
- Coordenar câmeras e efeitos.
- Encerrar no golpe misterioso.

*NarrativeTimelineBridge*
- Integrar infância, futuro e campanha.
- Preservar flags de progressão.
- Evitar repetir cenas involuntariamente.

Esses nomes são sugestões. Antes de criar scripts, verificar se o projeto já possui classes equivalentes.

### Implementação cinematográfica

Preferir Unity Timeline, Animation Events e ferramentas de câmera já instaladas quando adequadas.

Usar eventos de gameplay para coordenar o momento exato da devolução do controle.

Não incorporar lógica de narrativa diretamente ao controlador principal do personagem.

---

## 10. Etapa I — Qualidade e efeitos

Utilizar os sistemas de partículas, câmera e pós-processamento já existentes.

*Infância:* impactos leves de madeira, partículas discretas, iluminação quente e movimento de câmera suave.

*Futuro:* faíscas vermelhas, iluminação dramática, névoa, efeitos de impacto e transições visuais associadas ao Gothic Eclipse.

Priorizar contraste e legibilidade dos sprites.

Não permitir que partículas escondam informações relevantes de defesa ou esquiva.

Não iniciar produção musical adicional nesta etapa sem autorização. Utilizar efeitos e recursos sonoros licenciados já disponíveis ou placeholders.

---

## 11. Etapa J — Testes obrigatórios

### Tutorial
- Todos os objetivos podem ser concluídos.
- Vitória e derrota amigáveis avançam corretamente.
- Espadas de madeira não causam dano letal.
- Controle e teclado funcionam.
- Não existem loops ou bloqueios.
- Animações e hitboxes correspondem aos impactos.

### Cutscenes
- Sincronização entre animação e câmera.
- Transição infância/futuro sem inconsistências.
- Não há revelação acidental do resultado final.
- Pause, skip e retomada funcionam.
- O controle retorna corretamente ao jogador.

### Save e progressão
- Iniciar novo jogo.
- Concluir prólogo.
- Salvar e carregar.
- Pular tutorial já concluído.
- Continuar campanha sem regressões.
- Repetir prólogo quando houver opção apropriada.

### Performance e integridade
- Testar resoluções previstas.
- Evitar alocações excessivas.
- Não duplicar controladores.
- Preservar referências, metadados e GUIDs.
- Validar as animações no Unity e em runtime real.
- Conferir os efeitos de Gothic Eclipse nas cutscenes.

---

## 12. Critérios de entrega

O prólogo será considerado funcionalmente concluído quando:

1. Os irmãos infantis estiverem aprovados e integrados.
2. O tutorial for jogável do início ao fim.
3. A cutscene do futuro funcionar corretamente.
4. A campanha iniciar após o flashback.
5. Save, load, skip e controles estiverem validados.
6. Não existirem regressões críticas.
7. Houver evidências reais de testes no Unity.

A ligação definitiva com a bossfight final poderá permanecer como etapa futura até a campanha e a IA final estarem prontas.

Distinguir claramente *prólogo concluído* de *revelação final integrada*.

---

## 13. Execução segura e consumo de recursos

- Não antecipar a implementação antes do fechamento de Lucius Normal.
- Trabalhar com checkpoints incrementais.
- Preservar os assets aprovados de Fabius e Lucius.
- Usar staging para novas artes.
- Evitar múltiplos processos Unity.
- Manter os guardas de RAM, commit e disco definidos pelo projeto.
- Usar subagentes de arte apenas quando houver benefício real.
- Evitar gerações repetidas sem revisão dos resultados.
- Não gastar o limite semanal com experimentos desnecessários.
- Não executar builds canceladas sem nova autorização.
- Não criar automaticamente novas retomadas.
- Commit e publicação somente após conferência de escopo, diff e permissões.

*REGRA FINAL:*

A implementação deverá transformar o prólogo em uma experiência jogável, cinematográfica e emocionalmente significativa, sem comprometer os sistemas existentes nem atrasar desnecessariamente o restante do desenvolvimento.

*META:* fazer o jogador começar a aventura lutando contra Lucius por diversão e terminá-la enfrentando o mesmo irmão por razões que descobrirá ao longo da campanha.