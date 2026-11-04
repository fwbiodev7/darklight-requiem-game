# DARKLIGHT: REQUIEM — preparação de Lucius Normal para o Antigravity

## Missão e limite desta tarefa

Preparar a integração técnica de Lucius Normal aproveitando os sistemas já existentes de Fabius, sem abrir outro redesign extenso. Entregar um pacote separado que o coordenador possa integrar após finalizar Fabius Normal e Gothic Eclipse.

Esta tarefa autoriza **preparação independente**, não ativar Lucius na campanha agora. Não produzir transformações, IA 2.0 completa, novas histórias ou dezenas de técnicas.

**Projeto:** `C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main`

Leia o AGENTS.md. Você não está sozinho no projeto: preserve alterações de outros agentes e não reverta arquivos. Se também recebeu a tarefa de Gothic Eclipse, mantenha pacotes separados; priorize terminar um pacote pequeno antes de abrir outro.

## Área exclusiva de escrita

`C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/work/antigravity-lucius-normal/`

Leia o projeto, mas escreva somente nessa pasta. Não editar diretamente Assets, cenas, prefabs, ProjectSettings, Packages ou a cópia de publicação. Não fazer Git, abrir Unity, executar PlayMode, gerar builds, limpar disco ou alterar automações. Não duplicar o projeto inteiro nem criar Library/caches.

## Autoridade de escopo

- [Prompt oficial de Lucius](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/LUCIUS-NORMAL-ESCOPO-ENXUTO-20261102.md).
- [Redução vigente de Fabius](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/ESCOPO-ENXUTO-FABIUS-20261102.md).
- [Estado atual](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/outputs/retomada-20261102/continuation-state.json).
- [Famílias e cobertura atuais](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/outputs/retomada-20261102/animation-progress-current.json).

Não reiniciar filas históricas canceladas. Código e dados reais mais recentes prevalecem sobre afirmações antigas de integração ou testes.

## Identidade que deve permanecer separada

Lucius é irmão gêmeo de Fabius: mesma altura e proporções adultas, mas identidade própria. Cabelo claro compacto inspirado em Mirai Trunks, rosto conforme a referência aprovada de Ville Valo, olhos azuis, sobretudo preto/azul-escuro e espada grande nas costas. Sem capuz ou armadura aparente na versão principal. `WorldVisualScale = 1.325`.

Localizar as referências canônicas existentes e registrar seus caminhos/hashes; se não encontradas, marcar a produção visual correspondente como bloqueada. Não inventar aprovação nem solicitar uma geração baseada em referências que não viu.

**Reutilizar infraestrutura de Fabius, não seus sprites como arte final.** Não recolorir Fabius para chamar de Lucius, montar anatomia por partes, espelhar assimetrias ou usar PPU para corrigir proporções.

## Investigar primeiro: o que já pode ser reaproveitado

Leia apenas seções pertinentes destes consumidores:

- [CombatModel](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Combat/CombatModel.cs): estados, comandos, cooldowns, ataque e defesa.
- [CharacterView](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Character/DarklightCharacterView.cs): direção, escala, pivôs e apresentação.
- [WeaponRedesignBank](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs): seleção temporal, figuras completas, densidade por quadro e famílias compartilhadas.
- [CombatProfiles](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Combat/DarklightCombatProfiles.cs): dados de ataques e propriedade das cópias em runtime.
- [Fighter](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Character/DarklightFighter.cs) e [Enemy](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Campaign/DarklightEnemy.cs): identificar dependências de jogador e oportunidades de reaproveitamento para IA.

Armadilhas a evitar:

- Não colocar o controlador do jogador no chefe sem separar a origem dos comandos. A IA de Lucius não pode consumir teclado/mouse de Fabius.
- Não deixar a apresentação de Lucius ler arma, transformação ou skin do protagonista por referências globais da campanha.
- Não adicionar um índice de skin ao catálogo de Fabius sem analisar limites hardcoded, serialização e compatibilidade. Lucius precisa de dados e identidade de ator próprios.
- Não compartilhar instâncias mutáveis de loadout/ataques que alterem Fabius quando o chefe mudar de fase ou equipamento.
- Existem identificadores legados `Lucien`, inclusive enum, diálogos e recursos. O nome canônico é **Lucius**, mas não fazer substituição global nem reordenar enums/IDs que possam afetar saves, GUIDs ou narrativa. Registrar uma estratégia de compatibilidade; arte legada não equivale a referência aprovada do novo personagem.

## Entregas úteis agora

1. **Mapa de reutilização:** tabela curta com sistema existente, uso em Lucius, dependência a isolar e arquivo/método de referência. Não duplicar scripts completos de Fabius.
2. **Contrato de dados candidato:** identidade do ator, referências próprias de animações, loadout e parâmetros básicos de chefe. Preparar somente os adaptadores/componentes mínimos necessários, sem refatoração ampla do protagonista.
3. **Matriz mínima de assets:** Idle e Walk em oito direções; combo básico de três golpes, um pesado e um especial; defesa, dano e morte. Três famílias ofensivas, não uma por nome de técnica. Corrida/esquiva/aéreos aproveitam sequências compatíveis; carregamento usa antecipação existente; perseguição aérea não inventa contato. Não preencher lacunas com cópias visuais de Fabius.
4. **Plano de IA básica:** aproximação, distância, combo, pesado, especial, defesa contextual e recuperação. Usar informação observável e temporizadores; sem ler inputs futuros, reação perfeita, loops infinitos ou ataques inevitáveis.
5. **Plano de integração da bossfight:** identificar os pontos existentes de criação, início, morte e encerramento, preservando colisões, objetivos, progressão e saves. Propor a ligação de Lucius nesses pontos, sem ativá-la nesta tarefa.
6. **Plano de efeitos:** energia azul-escura e efeitos discretos da espada reutilizando o sistema de partículas. Sem nova biblioteca extensa, alterações de áudio ou mudanças no Gothic Eclipse.

Não gerar sprites nesta preparação. Se uma candidata técnica exigir placeholders, usar referências vazias ou fixtures isoladas, claramente identificadas; não inserir figuras provisórias na campanha nem apresentá-las como Lucius final.

## Formato do pacote

Dentro da pasta exclusiva, entregar:

- `REPORT.md`: resumo, achados concretos, limites e próxima integração exata.
- `REUSE-MAP.md`: o que compartilhar e o que isolar por ator.
- `ANIMATION-MATRIX.json`: famílias/direções mínimas, referências encontradas e lacunas reais; não inventar estados de aprovação.
- `candidate/`: apenas código/contratos novos ou modificados propostos, organizados por destino.
- `PATCH.diff` e `manifest.json`: caminhos e SHA-256 das bases/candidatas. Se o coordenador alterar a base, avisar; não sobrescrever a nova versão.
- `VALIDATION.md`: verificações futuras de independência de input/loadout/skin, oito direções, janelas justas, sincronização de contatos e início/fim da luta. Separar preparado de executado.

## Aceite e economia

O pacote deve permitir integrar Lucius depois das etapas prioritárias, com poucas alterações e dados independentes. Deve ficar claro como o chefe combate Fabius sem controlar ou modificar o protagonista. Preservar a identidade visual própria e o mistério do Encapuzado.

Não alegar compilação, testes, bossfight funcional ou aprovação visual sem execução e evidência. Somente o coordenador integra Assets, executa Unity e publica. Use logs compactos, não releia documentos completos e não inicie subagentes ou novas dependências por conta própria.
