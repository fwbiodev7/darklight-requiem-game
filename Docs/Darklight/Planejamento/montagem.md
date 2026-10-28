> **Escopo vigente em 26/10:** seguir [contrato canônico](../ESCOPO-CANONICO-20261026.md), [pendências](../PENDENCIAS-20261026.md) e [LDD](LDD.md). A ordem atual é Réquiem Normal → Ruptura → Eclipse Absoluto → Combat AI 2.0 → Sangue do Abismo → site. O roteiro abaixo é histórico: não restaurar Rock/Presas ou Eclipse/Absoluto separadas, não antecipar Lucius e não tratar fases antigas como novas entregas. Compilação, build, execução e campanha manual continuam estados distintos.

> Atualizacao histórica de entrega: o usuario autorizou a montagem autonoma de toda a campanha. As etapas de jogo deste roteiro foram integradas em DarklightCampaign; a versao possui seis mapas originais e ampliações posteriores, familias de inimigos, elites, chefes, RPG, formas, menus, save e Dark Resonance. O roteiro abaixo preserva o planejamento original. A verificacao daquela entrega foi limitada a compilacao, build e abertura. Guia histórico: outputs/campanha/LEIA-ME.md.

# DARKLIGHT: REQUIEM — plano de montagem do jogo

Revisão: 10/10/2026. Continua o roadmap auditado, incorpora o figurino com spikes e Dark Resonance. Este documento prepara as próximas implementações; não significa que os sistemas já estejam prontos. Manter o projeto original de Varginha intacto e trabalhar na cópia independente.

## Estado e sequência

| Etapa | Entrega concreta | Estado / condição de avanço |
|---|---|---|
| 00 — Fundação | Cópia independente, auditoria, arquitetura, importação e evidências | Concluída com pendências documentadas; 6/16 testes seletivos passaram |
| 01 — Identidade | Fonte aprovada, manifesto de inspeção, escala e contrato de animações | Visual aprovado e integrado; caminhada/golpe em oito direções. Demais animações e polimento pendentes |
| 02 — Combate | Laboratório com movimento, katana, combos, esquiva, parry e habilidade | Núcleo jogável integrado; 25 testes de regras/entrada e três de cena passaram. Acabamento visual pendente |
| 03 — Ecos e estilo | Rastejante, Atirador, Blindado, telegraphs, ondas e D→SSS | Depende do contrato de dano/defesa da etapa 02 |
| 04 — Ruptura | Energia, transformação, habilidades e reversão | Depende de modificadores e eventos de combate estáveis |
| 05 — Vespera | Rua, beco, arena, colisões, iluminação, portais e checkpoint | Depende da escala visual fechada e dos encontros funcionais |
| 06 — Kael | Duas fases, cinematografia, despertar, derrota e recompensa | Depende de arena, combate, inimigos e Ruptura |
| 07 — RPG | XP, níveis, árvore curta e inventário | Depende de recompensas e save isolado |
| 08 — Narrativa/UI/save | Menu próprio, HUD, diálogos, missão, morte/vitória e persistência | Fecha o fluxo completo, sem conteúdo antigo indevido |
| 09 — Polimento/controle | Porte final de Varginha, áudio, efeitos, medição e build Windows | Depende do fluxo jogável integrado; validar hardware quando disponível |
| 10 — Campanha | Seis capítulos, novos chefes, Eclipse e Absoluto | Um capítulo por marco; primeira experiência aprovada antes da expansão |
| 11 — Site | Site oficial com material real e link ao portfólio | Pode ocorrer antes do fim da campanha; publicação separada |

Dark Resonance acompanha 04/07/08/09 como extensão opcional: arquitetura de modificadores, desbloqueio, interface/missão e sincronização sonora. Sua implementação continua planejada, sem código nesta entrega. Usar exclusivamente os cinco MP3 fornecidos pelo usuário, já preservados em `AudioSources/Darklight/DarkResonance/` com manifesto; importação e reprodução ainda estão pendentes.

## Fechamento da arte e contrato de produção

Fonte atual: `ArtSources/Darklight/Etapa01/fabio-sprites-spikes-v4.png`. Preservar os PNGs aprovados. O atlas tem dimensões 1536×1024 e oito figuras estáticas. Os retângulos de inspeção não são recortes definitivos; há alpha intermediário e equipamento ultrapassando células regulares. Não importar como grade 4×2 automaticamente.

Antes da integração definitiva: revisar NW/NE, mão da katana e corrente; retirar apenas artefatos externos de fundo por edição assistida; conferir contornos e padding; alinhar pés e escala; atualizar variante com óculos; produzir caminhada e ações com quadros distintos. Não corrigir todo o alpha ou substituir uma cor globalmente, pois isso pode apagar detalhes do personagem.

Pacote de animações: repouso em oito direções; caminhada com quatro quadros por direção; corrida; três leves; pesado/carregado; esquiva; defesa/parry/contra-ataque; habilidade; dano; queda/morte; ativação de Ruptura. Organizar ações em folhas próprias, com antecipação, contato e recuperação compatíveis com o código. Colocar fones pertence ao pacote futuro de Dark Resonance. FPS e número de quadros das ações devem acompanhar as durações aprovadas no laboratório.

Escala proposta para comparar: corpo de 64 ou 72 pixels de altura no mundo 640×360. Os 32 PPU dos tiles não impõem automaticamente PPU ao personagem. Ajustar escala física com base no piso, portas e alcance, mantendo proporção adulta. A página de comparação é uma inspeção em navegador, sem mudança no arquivo nem configuração aplicada ao Unity.

## Etapa 02 — plano executável

Estado em 10/10/2026: laboratório criado em `Assets/Darklight/Scenes/DarklightCombatLab.unity`, com três alvos, perfis de ataque, entrada teclado/controle e pausa/reinício. A câmera agora aproxima 50% e acompanha o protagonista, atendendo ao pedido de zoom. Mundo interno 640×360 com apresentação por pixels. Capturas e evidências estão em `outputs/etapa02/`; o relatório `LEIA-ME.md` distingue sistemas implementados, testes e pendências. A estrutura abaixo registra o contrato proposto; a implementação consolidou motor/ataque em `DarklightFighter.cs` e regras em `CombatModel.cs`, com `DarklightCombatant.cs` para dano. A entrega permanece um laboratório técnico, antes dos inimigos e da campanha.

Objetivo: laboratório independente no novo projeto para validar o controle da katana antes de montar Vespera. Um alvo de treino serve para medir dano e timing; não será apresentado como inimigo final. Arte de produção segue a referência aprovada.

Reutilização: extrair movimento/câmera sem sanidade, mochila ou eventos da campanha antiga; usar buffer, combo, stagger e knockback herdados como referência; adaptar Input System e prompts verificados. Preservar consumidores antigos até a migração estar testada.

Arquivos propostos, relativos à cópia:

| Área | Arquivos / contrato |
|---|---|
| Entrada | `Assets/Darklight/Runtime/Input/DarklightInput.cs`, mapas Gameplay/UI separados e configurações com chaves próprias |
| Movimento | `Runtime/Character/DarklightMotor2D.cs`, orientação/mira separadas e Rigidbody2D |
| Combate | `Runtime/Combat/AttackDefinition.cs`, `Combatant.cs`, `AttackRunner.cs`, `DefenseState.cs`; dano e modificadores separados de apresentação |
| Apresentação | `Runtime/Character/DarklightCharacterView.cs`; atlas com retângulos/pivôs explícitos, sem alterar consumidor cardeal antigo |
| Dados | `Assets/Darklight/Data/Attacks/`; leve, pesado, carregado e contra-ataque configuráveis |
| Editor/cena | `Assets/Darklight/Editor/DarklightCombatLabBuilder.cs`, `Scenes/DarklightCombatLab.unity`; criação pela API do Editor |
| Testes | `Assets/Darklight/Tests/`; timing, dano único, pausa, cancelamentos e navegação |

Estratégia: definir estados normal/ataque/esquiva/parry/dano/morte e relógio comum; implementar golpe leve e dano único; acrescentar combos e pesado; defesa e cancelamentos; habilidade e mira; conectar animações e feedback. Centralizar hitstop para não conflitar com pausa. Não renomear o namespace da base em massa.

Isolar identidade de produto, save e preferências antes de rodar testes que gravem configurações. O porte começa com intenções de entrada na etapa 02 e termina com menus/remapeamento/vibração na etapa 09.

Ferramentas previstas: Unity CLI já instalado para consultar a instância da cópia, importar e compilar, executar a ferramenta de criação do laboratório e os testes delimitados. Sempre indicar o caminho da cópia ao controlar o Editor. Exemplos de operações: `unity status --format json`, execução do método Editor via `unity run`, testes via `unity test`. Consultar ajuda local antes de montar a sintaxe da execução. Não instalar dependências sem necessidade verificada.

Conclusão: jogador move e mira; sete sequências de combo previstas podem ser exercitadas; ataque acerta somente na janela ativa; alvo não recebe dano duplicado; esquiva/parry têm estados observáveis; gameplay fica bloqueado em pausa/UI; teclado e controle simulado funcionam. O aparelho físico permanece uma validação separada.

Riscos: ataque herdado cardeal, relógios misturados, movimentos ligados à campanha antiga, sprites ainda sem acabamento e regressões na navegação. Os testes seletivos antigos com falhas de sprites não devem ser ocultados nem declarados resolvidos pelo laboratório.

## Montagem dos encontros até a primeira versão jogável

02→03: os Ecos usam o mesmo contrato de dano e avisos visuais; introduzir um tipo por encontro antes de misturar três. O rank observa eventos reais de combate e penaliza repetição.

03→04: energia vem de golpes/parries e dados de combate. Ruptura aplica modificadores reversíveis e transita de volta sem reescrever atributos base. Reservar a mesma composição de modificadores para Dark Resonance, sem implementá-lo nesta fase por acidente.

04→05: rua de apresentação, tutorial curto, beco com encontro misto, checkpoint e arena. Separar Ground, Walls, Props, Collision, Lighting, Interaction, Spawns e Portals. Testar portas, limites, câmera e deslocamento antes de decorar.

05→06: Kael exercita os recursos já ensinados. A transição narrativa desperta Ruptura; checkpoint e reinício devem restaurar fase, inimigos, vida e energia de forma determinística. Música/iluminação mudam após a transição; cenas podem ser puladas.

06→08: recompensas, árvore curta e diálogos sustentam a progressão. HUD e menus fecham novo jogo→arena→Kael→encerramento→reinício. Save identifica entidades por IDs, sem referências de GameObjects. Uma partida antiga ou arquivo inválido precisa de tratamento explícito.

09: aplicar o porte completo do controle auditado, incluindo PixelButtonHover e mudanças locais de prompts/navegação. Revisar conflitos de botões antigos; desligar vibração ao perder foco ou dispositivo. Medir FPS e memória em build, verificar mixagem e efeitos, documentar pendências, preparar build Windows quando viável.

## Controles propostos

| Ação | Teclado/mouse | Controle |
|---|---|---|
| Mover / mirar | WASD / cursor | Analógicos esquerdo / direito |
| Leve / pesado-carregado | Mouse esquerdo / direito | X / Y |
| Esquiva / interação | Espaço / E | B / A |
| Parry / habilidade | Q / F | LB / RB |
| Modificador / transformação | Shift / R | LT / RT |
| Pausa | Esc | Start |
| MP3 opcional | M | Back/Select |
| Confirmar / voltar nos menus | Enter / Esc | A / B |

Remapear todas as ações; prompts acompanham o dispositivo. Back/Select abre o MP3 apenas fora de contextos que bloqueiem gameplay e depois do desbloqueio. Atualizar o plano de porte da Etapa 00 com essa ação no momento da implementação.

## Campanha, site e aceitação final

Detalhar e desenvolver os seis capítulos do GDD progressivamente. Para cada capítulo: mapa navegável, objetivos, conversas, encontros, conteúdo de arte/áudio, chefe, recompensa, save e transição; testar retomada antes de avançar. Eclipse e Absoluto precisam de repertório e situações próprios, não apenas outro multiplicador de dano.

Dark Resonance exige MP3 pixel art, NPC original, missão, fones, três modos, duração/cooldown e sincronização sonora. Manter conteúdo opcional desacoplado da progressão principal. A seleção fechada dos cinco arquivos está em `trilha-prototipo.md`. A integração usará essas cópias; BPM, trechos e marcadores serão definidos sobre as gravações recebidas.

Site usa capturas reais, galeria aprovada, devlog honesto e downloads existentes. Publicação, commits/pushes e alterações no portfólio continuam fora da autorização atual.

Marco final: campanha completa jogável, menu/controle integral, saves isolados e recuperáveis, arte consistente, trilha documentada, testes e medições registrados, build verificável e pendências declaradas. Datas e duração total só podem ser estimadas após o primeiro ciclo jogável.

## Revisão dos seis mapas — 10/10/2026

Os fundos ampliados foram substituídos por 24 peças de piso e 24 peças de arquitetura. Os seis capítulos agora usam pisos repetíveis, fachadas, portas e mobiliário próprios, com colisões na base e transparência de oclusão. A câmera continua em 640 × 360 e enquadra o corpo de Fábio. Arte do protagonista e HUD preservados. Capturas reais dos seis mapas e manifesto em outputs/mapas-refeitos/. Compilação e abertura verificadas; jogabilidade e controle físico permanecem para teste manual.

## Revisão aprovada de jogabilidade — 10/10/2026

Seis layouts modulares com exploração e encontros posicionais, combos rápidos com buffer no hitstop, navegação/cancelamentos e colisões, 68 quadros complementares, três poderes próprios e HUD/menu compactos. GUIA ATUAL: outputs/melhorias/LEIA-ME.md. 35 testes de lógica e 9 de cena distintos aprovados. Compilação sem erros; áudio e controle físico não testados. NPCs, formas e algumas ações ainda usam poses individuais. Música de menu e configurações/cinemática adicionais aguardam aprovação do novo plano.
