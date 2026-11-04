# DARKLIGHT: REQUIEM — preparação do Combate 2.0 para o Antigravity

## Missão

Preparar melhorias incrementais de combate e IA aproveitando os sistemas atuais. Entregar um pacote isolado, pequeno e rastreável para integração posterior pelo Codex.

**Preparação não significa ativação agora.** A prioridade continua: Fabius Normal → Gothic Eclipse → Lucius Normal → transformações → Combate 2.0. Não atrasar as etapas anteriores, reabrir redesign ou multiplicar golpes/animações.

**Projeto:** `C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main`

Leia o AGENTS.md. Você não está sozinho no workspace: não reverter alterações alheias. Caso tenha os três handoffs, manter pacotes separados e concluir uma entrega pequena antes de iniciar outra.

## Área exclusiva de escrita

`C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/work/antigravity-combat-2/`

Pode ler o projeto e preparar arquivos candidatos apenas nessa pasta. Não editar diretamente Assets, cenas, prefabs, ProjectSettings, Packages ou a cópia de publicação. Não executar Git, Unity, PlayMode, builds, limpeza ou automações. Não duplicar projeto/Library nem instalar dependências.

## Estado e contratos vigentes

- [Escopo reduzido de Fabius](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/ESCOPO-ENXUTO-FABIUS-20261102.md).
- [Escopo de Lucius Normal](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/LUCIUS-NORMAL-ESCOPO-ENXUTO-20261102.md).
- [Estado real da produção](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/outputs/retomada-20261102/continuation-state.json).

O usuário reduziu a produção artística: variantes de combos compartilham golpes-base, aéreos reaproveitam animações e somente defesa/dano/morte exigem estados essenciais novos. Combate 2.0 deve melhorar decisões, regras e fluidez com esse arsenal, sem criar outra fila de técnicas.

Preservar por padrão a esquiva Normal de 230 ms e a invulnerabilidade de 20 até antes de 190 ms. Não alterar silenciosamente dano, custos, buffers, cooldowns, alcance ou timings já existentes. Mudanças propostas de balanceamento ficam documentadas separadamente, como propostas não validadas.

## Recuperar antes de inventar

Ler apenas os consumidores relevantes e localizar seus testes existentes:

- [CombatModel](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Combat/CombatModel.cs): estados, fases, buffers, cancelamentos e invulnerabilidade.
- [AttackDefinition e contratos de ataques](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Combat/AttackDefinition.cs).
- [StrikeResolver](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Combat/DarklightStrikeResolver.cs): contato, alvos, pulsos e resolução de dano.
- [Enemy](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Campaign/DarklightEnemy.cs) e [CombatOverhaul](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Campaign/DarklightCombatOverhaul.cs): decisões e permissão de ataques; localizar também `DarklightThreatDirector`.
- [AirCombo](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Character/DarklightAirCombo.cs): lançamento, perseguição, contatos e finalização; localizar a implementação de `AirBody` e suas limitações atuais.
- [Navigation](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/World/DarklightNavigation.cs): movimento seguro, obstáculos e linha de visão.
- [AbyssRider](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Campaign/DarklightAbyssRider.cs): regras próprias da Moto que não devem ser quebradas por um tratamento genérico.

Não presumir que as melhorias abaixo estejam todas ausentes. Primeiro distinguir recurso existente, defeito comprovado e lacuna real. Não substituir o sistema inteiro de IA/combate.

## Pacotes candidatos, em ordem

### 1. Perseguição e posicionamento

Melhorar aproximação, manutenção de distância e recuperação de caminhos bloqueados usando a navegação atual. Evitar inimigos empilhados, oscilação e travamento contra paredes. Respeitar linha de visão e alcance. Não reconstruir mapas, alterar colisores ou adicionar outra infraestrutura de navegação sem necessidade comprovada.

Reutilizar a permissão de ataques e o diretor de ameaças existentes. Não criar um segundo árbitro concorrente de ataques. Preservar limites de pressão sobre o jogador e tratamento de ameaças fora da câmera.

### 2. Decisões contextuais justas

Separar percepção, escolha e execução. Usar posição observável, distância, linha de visão, recuperação e ataques já sinalizados. Incluir reação e comprometimento configuráveis, não esquiva/parry perfeito no mesmo frame.

Não ler teclado, mouse, controle, comandos privados enfileirados ou inputs futuros de Fabius. Não dar ao chefe conhecimento perfeito através de paredes. Lucius pode reutilizar a infraestrutura, com parâmetros próprios; isso não substitui sua bossfight básica da etapa anterior.

### 3. Cancelamentos, buffers e contatos

Revisar apenas falhas concretas nas janelas atuais: confirmar acerto versus errar, consumir comandos uma vez, expirar buffers e impedir cancelamento que apague contatos ativos ou permita dano gratuito. Preservar startup/ativo/recuperação e os eventos existentes.

Não confundir uma animação compartilhada com um evento de dano adicional. Cada contato deve seguir os pulsos/serial do ataque real; efeitos e sprites não podem causar duplicação de hits.

### 4. Combate aéreo e limites anti-infinito

Aproveitar lançamento → perseguição → golpes-base → finalização já existentes. A perseguição é movimento sem contato: não inserir dano nem impacto fictício. Não criar cinco novas animações aéreas.

Mapear os limites atuais e propor apenas o que faltar: controle por alvo de relançamento/suspensão, orçamento de contatos e saída segura da sequência. Definir reset em morte, pouso, perda de alvo, troca de encontro e reutilização de inimigos do pool. Evitar estado preso ou contadores persistindo em inimigos novos.

Preservar resistências de chefes/blindados e imunidades temporárias existentes; não tornar todo inimigo imune nem permitir loops infinitos. Pausa, hitstop e diferenças de delta não devem alterar indevidamente as janelas ou conceder contatos extras.

## Regras de implementação

- Preparar alterações pequenas e independentes, com ponto explícito de ativação futura e possibilidade de desativar sem remover o comportamento atual.
- Reutilizar componentes e dados. Não duplicar scripts completos para cada inimigo, arma ou personagem.
- Separar parâmetros por ator; não compartilhar estado mutável que contamine Fabius, Lucius ou outros inimigos.
- Preservar Katana, Punhos e Moto. Não alterar arte, escala, skin, olhos, cabelo, câmera, áudio, saves, preferências ou IDs serializados.
- Evitar alocações, criação de objetos, buscas globais e novas coleções a cada frame. Não introduzir materiais/partículas para resolver um problema de lógica.
- Não adicionar novas técnicas, armas, transformações ou bibliotecas de efeitos. Não misturar este pacote com Gothic Eclipse ou com a produção visual de Lucius.

## Entrega

Na pasta exclusiva:

1. `REPORT.md`: o que já existe, defeitos comprovados e mudanças propostas, com arquivos/métodos.
2. `REUSE-MAP.md`: pontos de reaproveitamento e dependências a isolar.
3. `candidate/`: somente arquivos novos/modificados necessários; cada subpacote identificável separadamente.
4. `PATCH.diff` e `manifest.json`: origem/destino e SHA-256 das bases/candidatas. Se a base mudar, informar e adaptar; não sobrescrever a versão nova.
5. `VALIDATION.md`: cenários direcionados e resultados esperados. Priorizar testes existentes; preparar novos somente para comportamentos realmente novos ou defeitos comprovados. Não executar Unity nesta tarefa.

Cobrir no plano de validação: perseguição com obstáculo; dois inimigos disputando espaço; pressão de ataques simultâneos; reação com atraso; acerto/erro/cancelamento; comando expirado; contato sem duplicação; perseguição aérea sem dano; término do combo; boss/blindado; morte/pausa/hitstop; reutilização de inimigo; Katana/Punhos/Moto.

Não alegar testes aprovados, ganho de performance, balanceamento validado ou combate pronto sem evidências de execução. O coordenador integra, testa e publica depois das etapas prioritárias. Entregar trabalho preparado, não uma aprovação fictícia.

**Economia:** contexto curto, saídas filtradas, sem auditoria geral repetida, novos subagentes ou dependências. Fechar um pacote pequeno antes de expandir o seguinte.
