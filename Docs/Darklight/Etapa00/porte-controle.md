# DARKLIGHT: REQUIEM — porte do controle de Varginha

## O que já veio na cópia da main

- Unity Input System 1.20.0, ações criadas em runtime e mapas `Gameplay`/`UI`.
- Movimento por analógico esquerdo/D-Pad e mira por analógico direito.
- Detecção do último dispositivo, prompts de botões e teclado/mouse remapeáveis.
- Remapeamento do gamepad salvo em PlayerPrefs; conflitos trocam o botão das ações.
- Captura que aguarda soltar o botão, cancelamento por Start/Esc e recuperação ao desconectar.
- Navegação IMGUI por posição dos elementos, confirmação A, retorno B, sliders e rolagem.
- Vibração com intensidade configurável e parada em pausa do aplicativo, perda de foco, transição e desconexão.

Evidências: `Assets/Scripts/Game/Varginha/VarginhaInputRuntime.cs`, `VarginhaInputBindings.cs`, `VarginhaGamepadBindings.cs`, `VarginhaGamepadUI.cs`, `Experiment/CampaignControllerSettings.cs`, `Experiment/VarginhaGameSettings.cs` e `Assets/Tests/PlayMode/CampaignControllerRemapTests.cs`.

## Melhorias encontradas no projeto local

O checkout local está em `codex/campanha-15-fases-e-site`, commit observado `df2b0911fb0fceee70bdbd8cc7d0333acee5f382`. Ele não é a base `main` clonada. A comparação normalizada mostra três dos seis arquivos examinados com diferenças de conteúdo; os outros três diferem somente na representação de bytes/fins de linha.

- `VarginhaInputRuntime.cs`: rótulos adicionais de ação/movimento/navegação/confirmação e correções na substituição dos prompts.
- `VarginhaGamepadUI.cs`: seleção inicial, navegação por linhas, preservação de atributos dos elementos, limpeza do estado, transição entre mouse/controle e tratamento mais preciso de sliders.
- `CampaignControllerRemapTests.cs`: categoria adicional `HudPuzzles` no teste.

O adaptador local depende de `PixelButtonHover.cs`, ausente na main, e da integração do tema/botões. Portanto, copiar somente o arquivo de navegação produziria dependências faltantes. `PixelButtonHoverUGUI.cs` existe no projeto local, mas só deve acompanhar o porte se o novo jogo realmente usar uGUI. O diff auditado está em `diferencas-controle.diff`.

## Mapeamento final proposto

| Ação DARKLIGHT | Controle XInput | Teclado/mouse proposto |
|---|---|---|
| Movimento | Analógico esquerdo | WASD / setas |
| Direção do ataque | Analógico direito | Cursor no mundo |
| Ataque leve | X | Botão esquerdo do mouse |
| Pesado / carregar | Y | Botão direito, segurar para carregar |
| Esquiva | B | Espaço |
| Interagir | A | E |
| Parry | LB | Q |
| Habilidade | RB | F |
| Modificador | LT | Shift |
| Transformação | RT | R |
| Pausa | Start | Esc |
| Navegar menus | D-Pad / analógico esquerdo | Setas / mouse |
| Confirmar / voltar | A / B | Enter / Esc |

O teclado é uma proposta, não uma preferência já aprovada. A base associa Y ao inventário, LT à corrida e RT à lanterna: esses vínculos precisam ser substituídos no novo mapa, sem manter as ações antigas disparando junto. A enumeração existente usa índices para atribuir botões; evitar inserir ações novas nessa lista sem revisar todos os consumidores.

## Ordem de implementação futura

1. Separar as ações de DARKLIGHT, chaves de remapeamento/configuração e caminho de save dos usados por Varginha; mudar a identidade do produto antes dos testes que gravam preferências.
2. Portar as melhorias de prompts/navegação com as dependências necessárias, preservando a implementação original para comparação.
3. Integrar movimento, mira independente, ataques e menus. Não usar apenas a existência da ação `Aim` como prova de direção independente: o ataque herdado lê a orientação do personagem e cardinaliza o golpe.
4. Ajustar deadzone por ação/dispositivo. A base usa mínimo 0,20 e máximo 0,95 globalmente; avaliar se isso mantém controle fino e evita drift no Harrow.
5. Validar gatilhos como botão/valor, transformação com um disparo por acionamento e pesado com duração de pressão. Suspender gameplay durante remapeamento e menus.
6. Fazer a validação final do porte junto ao polimento da vertical slice.

## Critérios de aceitação

Teste simulado: oito direções sem vantagem diagonal; mira sem movimento; buffer, ataques e modificadores; troca de dispositivo sem prompts errados; captura/cancelamento/conflitos/restauração; desconexão durante combate/captura; pausa e menus sem ataques involuntários; sliders e rolagem acessíveis.

Teste físico no **Redragon Harrow G808 em XInput**: identificação pelo Input System, calibração dos sticks, botões/gatilhos, reconexão USB/sem fio conforme o hardware disponível, vibração e parada dos motores. Esses testes físicos ainda não foram executados. Suporte XInput genérico não comprova certificação do aparelho específico.

Estado desta etapa: suporte herdado preservado e porte planejado. O mapeamento DARKLIGHT e as melhorias locais ainda não foram integrados ao novo gameplay.
