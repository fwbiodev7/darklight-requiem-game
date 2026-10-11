# DARKLIGHT: REQUIEM — arquitetura proposta

Estado: proposta da Etapa 00. Os módulos abaixo ainda não foram implementados.

## Estrutura

```text
Assets/Darklight/
  Runtime/
    Core/             inicialização, relógio de combate, eventos
    Input/            ações, dispositivos, remapeamento, vibração
    Character/        motor 2D, orientação, estados e apresentação
    Combat/           golpes, hitboxes, dano, defesa, cancelamentos
    Enemies/          percepção, estados e coordenação de ataques
    Style/            pontuação, repetição e queda de rank
    Transformations/  energia, duração e modificadores de Ruptura
    Progression/      atributos, XP, habilidades e inventário
    World/            interação, portais, checkpoints e encontros
    Narrative/        diálogos, eventos e cutscenes
    UI/               HUD, menus, prompts e configurações
    Audio/            efeitos e camadas musicais
    Persistence/      save próprio, migrações e recuperação
  Data/               definições de golpes, inimigos e transformações
  Art/                fontes aprovadas e sprites finais separados
  Prefabs/
  Scenes/
  Editor/             importação, auditorias e ferramentas
  Tests/EditMode/
  Tests/PlayMode/
Docs/Darklight/
Website/Darklight/     proposta para o site futuro, independente do Unity
```

Essa estrutura será criada progressivamente. Não gerar módulos vazios nem substituir o namespace `Game` em massa. Conservar os sistemas de Varginha na cópia até seus consumidores serem migrados e verificados. O site antigo de Varginha também não será divulgado como site de DARKLIGHT.

## Dependências e contratos

- `Input` fornece intenções; não acessa o personagem ou decide dano. Gameplay e UI terão ativação independente. Combate ficará bloqueado durante pausa, diálogo, remapeamento e cutscene.
- `Character` aplica movimento em `FixedUpdate` com Rigidbody2D; câmera acompanha em `LateUpdate`. Orientação visual e mira serão dados distintos. Caminhar diagonalmente não comprova animação de oito vistas.
- `Combat` recebe comandos e usa definições configuráveis de antecipação, contato, recuperação, alcance, dano, janela de buffer, cancelamento e invulnerabilidade. Hitboxes identificam alvos já atingidos pelo mesmo golpe.
- Hitstop não deve alterar o tempo global sem coordenar pausa, efeitos e UI. O ataque herdado usa `Time.timeScale`: adaptar esse ponto antes de somar novos sistemas.
- `Enemies` mantém telegraphs, cooldowns e limite de atacantes simultâneos; não conhece HUD ou save.
- `Transformations` aplica modificadores reversíveis sobre atributos e movimentos. Implementar Normal/Ruptura primeiro; Eclipse/Absoluto ficam em dados e planejamento futuros.
- `Style` recebe eventos de combate, variedade e dano. HUD e áudio observam mudanças de rank sem calcular pontuação.
- `Persistence` salva identificadores estáveis, não referências de cenas ou GameObjects. Usar caminho e chaves próprios de DARKLIGHT; preservar o padrão de escrita temporária e substituição existente.
- `Narrative`, `World` e `UI` comunicam-se por eventos ou interfaces pequenas. Adaptadores temporários podem consumir a base sem duplicar todo o código.
- Efeitos frequentes passam a usar pooling quando medição e perfil de uso justificarem. O ataque herdado instancia efeitos e faz consultas que alocam; ele não é uma solução de pooling já pronta.

## Reaproveitamento

Extrair apenas a parte geral do movimento de `EdelzioTopDownController`: ele também carrega sanidade, mochila, itens e bloqueios narrativos de Varginha. Reaproveitar limites/suavização de `CameraFollow2D`, separando nomes e áreas das cenas antigas. Aproveitar buffer, combo e feedback de `VarginhaPlayerAttack` como referência; seus golpes são socos cardeais, não a espada Réquiem.

Separar modelo de diálogo de `VarginhaGameHUD` e lógica de interação de `InteractableProp`. Os menus atuais usam IMGUI e adaptador próprio de controle; mantê-los durante a migração. Migrar UI somente com benefício concreto, sem introduzir navegação duplicada.

## Escala proposta

Recomendação para aprovação na Etapa 01: resolução interna **640×360**, saída inicial **1920×1080**, escala inteira 3×. A opção 512×288 exige barras ou escala fracionária para ocupar 1080p; não usar 3,75× como se fosse inteiro. Para 512×288, 3× ocupa 1536×864 e pode ser centralizado.

Proposta inicial de assets novos: tiles 32×32 e PPU 32, com dimensões do personagem definidas após comparar silhuetas e cobertura da arena. Isso ainda não é uma configuração aplicada. As artes herdadas têm contratos próprios e recortes em runtime; não converter tudo para PPU 32 nem reduzir fontes automaticamente.

Antes de configurar Pixel Perfect Camera, resolver a referência do pipeline de qualidade e executar a detecção real do pipeline. Pacote URP instalado não comprova URP ativo. Avaliar textos/HUD em overlay para manter legibilidade sem suavizar o mundo.
