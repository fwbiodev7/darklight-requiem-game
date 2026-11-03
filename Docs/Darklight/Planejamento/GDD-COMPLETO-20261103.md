# DARKLIGHT: REQUIEM — GDD completo consolidado

**Versão:** 03/11/2026. **Plataforma:** Windows. **Engine:** Unity 6000.6.0f1. **Gênero:** RPG de ação/hack and slash 2D em pixel art dark fantasy.

Este documento reúne o design principal e os planos completos aprovados de Gothic Eclipse, Lucius Normal e prólogo cinematográfico. Planejamento, implementação local, testes e publicação são estados diferentes. As regras desta abertura e as ordens diretas mais recentes prevalecem sobre datas e sequências históricas nos anexos.

## Visão e pilares

Exploração de Vespera e do Véu, combate expressivo com leitura clara, progressão de personagem e narrativa cinematográfica. Um eclipse rompe a fronteira entre a cidade e o Véu, trazendo criaturas conhecidas como Ecos. Fabius e Lucius são irmãos gêmeos; a campanha revela progressivamente a ruptura entre eles. O destino dos irmãos e o resultado do confronto futuro não são antecipados.

Pilares: identidade visual consistente; movimentação em oito direções; armas com funções próprias; defesa, esquiva e contra-ataque legíveis; continuidade entre narrativa e gameplay; progressão preservada; atmosfera gótica que permite enxergar personagens e ataques.

## Contrato canônico dos personagens

| Elemento | Fabius | Lucius |
|---|---|---|
| Identidade | Protagonista adulto, skin única Réquiem | Irmão gêmeo e antagonista com identidade própria |
| Visual | Cabelo castanho compacto, pele original, olhos vermelho-escuros, sobretudo preto/vermelho | Cabelo claro, olhos azuis, sobretudo preto/azul-escuro, espada grande nas costas |
| Proporções | Corpo compacto adulto, escala visual 1.325 | Mesma altura e proporções compatíveis, escala 1.325 |
| Arsenal | Katana, Punhos e Moto/serras | Arsenal próprio conforme plano abaixo |
| Formas | Normal, Ruptura, Eclipse Absoluto | Normal, Nexo Fenrir |

A forma Abissal não existe. Nomes antigos de arma/Moto ou arquivos históricos não criam outra transformação. Rock e skins substituídas ficam como legado, fora do escopo ativo. A apresentação principal de Lucius não tem capuz nem armadura aparente. O Encapuzado conserva seu mistério: apresentar os irmãos no prólogo não autoriza identificar antecipadamente essa figura em diálogos, HUD ou recompensas.

Fabius preserva íris vermelho-escura (guia #C40814, sombra #5A0000), pele canônica e cabelo sem crescimento artificial nas transformações. Lucius mantém olhos azuis. Personagens são desenhados por inteiro, com oito direções coerentes e assimetrias preservadas; armas e membros não são montados por peças. PPU organiza a densidade do raster, não corrige anatomia.

## Direção visual

Pixel art 2D topview com clusters e contornos nítidos, filtro Point, cores escuras e luzes localizadas. Personagens, HUD e cenários aprovados são preservados. Atlas usam retângulos e pivôs próprios, eventualmente densidade por quadro; o desenho não é distorcido para caber no importador.

O fundo combina azul-marinho, petróleo e sombras frias; efeitos prateados, carmesim e violetas mantêm leitura. Luz lunar suave, névoa translúcida e fontes âmbar existentes compõem Gothic Eclipse. Chuva e efeitos não escondem golpes ou inimigos. A camada atmosférica é configurável e reversível nos dez mapas existentes, sem reconstrução de geometria ou colisões.

A referência técnica existente usa renderização de mundo a 1280×720, com ampliação e enquadramento preservando pixel art. Tamanho corporal, câmera e colisores são responsabilidades distintas. Metas de desempenho precisam de medição real; não há certificação geral de 60 FPS neste GDD.

## Ordem de desenvolvimento vigente

1. Fabius Réquiem Normal: escopo gráfico enxuto, integração e validação essencial.
2. Gothic Eclipse nos mapas existentes.
3. Lucius Normal.
4. Prólogo cinematográfico jogável.
5. Transformações: Ruptura/Eclipse Absoluto e Nexo Fenrir.
6. Combate IA 2.0.
7. QA geral, otimização, build Windows e publicação validada.

O prólogo permanece planejado até Lucius Normal estar concluído e validado. Não iniciar novas transformações durante as etapas anteriores.

## Ciclo principal

Explorar → observar um encontro → combinar ataques e defesas → ganhar estilo e energia → transformar → vencer → recolher recompensa → conversar/investigar → abrir o próximo trecho. Checkpoints permitem repetir encontros sem atravessar diálogos já vistos. Missões secundárias acrescentam contexto e opções de combate.

## Combate

Movimento em oito direções, mira independente e leitura clara dos ataques. Arsenal ativo: katana, Punhos da Ruína e moto para Réquiem, preservando combate aéreo e suas finalizações. Katana oferece leve, pesado e carregado; combos iniciais: leve/leve/leve, leve/leve/pesado, leve/pesado, pesado carregado, esquiva/ataque, parry/contra-ataque e habilidade/ataque. Moto mantém Serra Infernal, Giro Devastador, Esmagamento Vertical, Roda Destruidora, Cavalgada Brutal e Execução Aérea Motorizada. A redução do seletor preserva timings, controles e mecânicas existentes; não renumerar IDs persistidos de forma destrutiva.

Definir cada golpe por antecipação, contato, recuperação, alcance, dano, stagger e cancelamentos. Input buffering permite antecipar a próxima ação sem disparar ações durante pausa ou diálogo. Cada golpe acerta cada alvo uma vez, salvo ataques explicitamente definidos como múltiplos. Esquiva, parry perfeito, invulnerabilidade, hitstop e super armor precisam de janelas legíveis e verificáveis. Finalizadores e repertório ampliado entram após o núcleo estar estável.

O rank D→SSS premia variedade e execução, perde valor com repetição e dano recebido e decai quando o combate fica inativo. Eventos de combate alimentam estilo, energia, áudio e HUD; a interface não calcula dano nem rank.

## Inimigos e chefes

Primeira experiência: Eco Rastejante, Eco Atirador e Eco Blindado. Depois: Eco Caçador, Eco Espectral e elites. Todos precisam de percepção, aproximação, preparação visível, ataque, recuperação, reação, morte e limites de coordenação para evitar ataques simultâneos injustos.

Kael Voss é o primeiro chefe: fase 1 com combos, parry, esquivas, investidas e cortes de energia; fase 2 acrescenta teletransporte curto, projéteis e sequências agressivas. O encontro provoca o despertar de Ruptura. Barra de vida, introdução, transição, derrota e recompensa compõem o encontro; pular a cinematografia deve preservar o estado correto da luta.

## Transformações

Normal → Ruptura → Eclipse Absoluto. Normal mantém cabelo castanho natural. Ruptura tem mechas vermelhas intensas; Eclipse Absoluto funde a energia violeta de Eclipse com branco-prateado e elementos de Absoluto, preservando os poderes úteis e removendo redundâncias. As antigas Eclipse e Absoluto não são opções independentes. Energia ganha em combate, ativação, duração e retorno seguro continuam; modificadores devem ser removidos ao expirar, morrer ou transitar de cena, sem multiplicar permanentemente os atributos. Migrar progressão antiga sem apagar saves.

Prioridade artística: concluir Normal antes de Ruptura e Eclipse Absoluto. Manter anatomia adulta, corte canônico e oito direções reais, `WorldVisualScale = 1.325`, PPU pelo corpo e olhos/pele preservados. Clips antigos de Eclipse/Absoluto são candidatos para revisão da fusão, não arte combinada aprovada. Substituir ações progressivamente com sequências completas; não misturar fases antigas e novas no mesmo golpe.

## Dark Resonance — opcional, integrado à campanha

Um MP3 sobrenatural transforma música em poderes temporários. A campanha principal deve funcionar integralmente sem desbloquear ou usar o aparelho. Os valores abaixo são hipóteses iniciais para testes, não balanceamento aprovado em gameplay.

| Modo | Efeito inicial proposto | Duração | Linguagem visual |
|---|---|---|---|
| Love Metal | Cura 1,4% da vida máxima por segundo, até o limite | 18 s | Aura vermelha e ornamentos góticos discretos |
| Thrash de Foco | +40% velocidade de ataque, +20% movimento, +20% energia de forma obtida | 14 s | Pulsos rápidos e faíscas acompanhando o ritmo |
| Heavy de Força | +45% dano, impacto dobrado e recuperação menor ao sofrer dano | 12 s | Pulsos graves e contorno defensivo legível |

Um modo ativo por vez. Recargas independentes: Love 32s, Força 28s, Foco 30s; custos 35/40/40 da energia própria do MP3. Trocar a seleção não elimina a recarga. A cura não ressuscita nem excede a vida máxima. Transformações permanecem independentes.

O MP3 permite selecionar música ou estilo e mostra faixa, modo, efeito, duração e cooldown. Músicas do mesmo estilo compartilham o modificador; trocar de faixa é uma escolha musical. No single-player, abrir o painel pausa a ação; após confirmar e fechar, Fabius coloca os fones numa animação interrompível por dano, sem invulnerabilidade gratuita. O poder começa ao concluir a animação. Antes de concluir, a interrupção não concede buff nem consome a ativação.

Teclado proposto: M abre o MP3, setas selecionam, Enter confirma, Esc volta. Controle: Back/Select abre, D-Pad seleciona, A confirma, B volta. Remapeamento e prompts acompanham o dispositivo; menus bloqueiam golpes. Não ocupar LB/RB/LT/RT já reservados ao combate. A seleção também deve permitir mouse.

Integração: modificadores temporários separados dos atributos permanentes; recalcular timing de animação e hitboxes juntos ao acelerar ataques. Testar o acúmulo com Ruptura antes de definir limites. Morte encerra o efeito; salvar/carregar e trocar de cena não podem eliminar cooldown. O planejamento de save inclui tempo restante e estado do sistema, sem permitir duração infinita por pausa ou troca de música.

VFX usam BPM e marcadores fornecidos pela faixa, com relógio de áudio e transições curtas. Não dependem de detectar automaticamente batidas no arquivo. Pulsos acompanham o ritmo sem mudar hitboxes, parry ou avisos dos inimigos. Volume zero não remove os poderes. Oferecer intensidade reduzida de flashes e efeitos para preservar leitura e conforto.

Missão secundária proposta: **Frequência do Véu**. Na antiga loja de discos de Vespera, Fabius ajuda a recuperar componentes do aparelho e fragmentos sonoros, aprende a selecionar um modo e recebe o MP3. A missão tem versão curta de tutorial e pode ser concluída depois do primeiro encontro com Kael, sem bloquear a história.

NPC original proposto: **Ícaro Nox**, técnico de som e colecionador de equipamentos antigos, com estética de rock gótico dos anos 2000. Criar rosto, figurino, voz e história próprios, sem reproduzir a identidade de artistas reais sem autorização.

Planejamento de arte pela skill: MP3 fechado/aberto/ativo/em cooldown; botões de seleção/foco/confirmação; três ícones de modos; fones como acessório; sequência de colocar e retirar fones em oito direções; retrato e vistas do NPC; auras e pulsos em folhas separadas. Interface pode ocupar aproximadamente 224×128 pixels lógicos, aparelho 32×32 e ícones 16×16, como ponto de partida. Revisar a legibilidade antes de fixar tamanhos. Pivôs dos fones seguem a cabeça; katana e braços devem respeitar oclusão. Manter spikes, rosto e cabelo do protagonista aprovado.

Trilha do protótipo pessoal: usar exclusivamente os cinco MP3 fornecidos pelo usuário. Love Metal: **Join Me in Death** e **Wings of a Butterfly**, de HIM. Thrash Metal: **Seek & Destroy** e **Creeping Death**, de Metallica. Heavy Metal: **Enter Sandman**, de Metallica. Right Here in My Arms e Sad But True saem da seleção. Essa associação é uma decisão de design dos modos, não uma classificação musical definitiva.

Os cinco arquivos foram recebidos e copiados sem alteração para `AudioSources/Darklight/DarkResonance/`, fora de Assets. O manifesto registra IDs, caminhos, hashes, tamanho e propriedades reportadas pelo Windows. Os cinco arquivos também foram importados em Assets/Darklight/Audio/Music como estéreo com Streaming. Reprodução é implementada; sincronização fina e marcação de BPM ficam para avaliação manual. Não adicionar outras músicas nem baixar faixas. Os arquivos musicais do protótipo pessoal ficam fora da publicação e dos pacotes públicos. O NPC permanece original, sem copiar artistas reais. Detalhes constam em `trilha-prototipo.md`.

Entrega futura: protótipo dos modificadores após combate/Ruptura estáveis; interface e missão nas etapas de RPG/narrativa; sincronização e mixagem no polimento. Dark Resonance é uma extensão opcional e não bloqueia a primeira experiência jogável. Testes futuros: regeneração, dano/velocidade, redução defensiva, interrupção, duração/cooldown, morte, persistência, acumulação com Ruptura, áudio silenciado, sincronização e navegação sem mouse. Existe infraestrutura anterior; o balanceamento e a validação geral permanecem etapas próprias.

## RPG, narrativa e persistência

XP, níveis, atributos, inventário e pequena árvore de habilidades com escolhas úteis ao combate. Expandir equipamentos somente após a experiência inicial. Diálogos, objetivos e recompensas usam identificadores estáveis. Save próprio de DARKLIGHT, escrita temporária e substituição segura; isolar também chaves de configuração e remapeamento da outros projetos.

Menus: novo jogo, continuar, opções, pausa, morte, vitória, créditos e retorno. HUD: vida, energia, habilidade/cooldown, rank, objetivo, chefe e indicador opcional de Dark Resonance. Não deixar nomes, textos ou progresso da campanha antiga aparecerem como conteúdo novo.


## Campanha, mapas e narrativa

Preservar os dez mapas/capítulos existentes, os seis capítulos originais e suas ampliações. Existência dos ambientes não equivale a narrativa, chefes e finais concluídos. Os títulos são:

1. A Noite da Ruptura.
2. Vozes do Abismo.
3. Sangue Desperto.
4. O Projeto Eclipse.
5. Além do Véu.
6. O Último Eclipse.
7. Herança Carmesim.
8. A Cidade sem Amanhecer.
9. O Trono do Vazio.
10. Darklight: Requiem.

Cada capítulo precisa de apresentação, objetivo, exploração, encontros, momento central, recompensa, transição e retomada de save. Não preencher duração com repetição obrigatória. A duração total da campanha ainda não foi medida. O confronto mostrado no prólogo é retomado no final da campanha; títulos históricos de capítulo não criam outra conclusão obrigatória nem transformações separadas.

Preservar pontes, navegação, câmera, NPCs, checkpoints, saídas, segredos, flags e recompensas. Mapas novos não são exigidos para Gothic Eclipse. A área infantil do prólogo é uma pequena cena independente, prevista no plano completo abaixo.

## Interface, controles e acessibilidade

Teclado/mouse e controle em gameplay e menus, com remapeamento e prompts coerentes. A orientação acompanha o movimento conforme controle aprovado; cursor ou alavanca direita não substituem essa regra. O controle físico requer teste próprio, além da simulação automatizada.

Menu principal: continuar, novo jogo, carregar, configurações, créditos e sair; continuar depende de save válido e mostra progresso real. Uma skin única ativa não exige reabrir o escopo de skins históricas. Pausa, mapa e diálogo bloqueiam ações de combate e congelam seus temporizadores.

HUD: vida, energia, cooldowns, rank, objetivo, barra de chefe e indicador opcional de Dark Resonance. Minimapa mostra exploração, objetivos, inimigos detectados e rotas conhecidas; mapa completo conserva descoberta e destinos. Interação por E/A ou botão contextual em NPCs, itens, checkpoints e saídas.

Diálogos usam caixa inferior gótica, retratos, texto gradual, pausas de pontuação e paginação. O primeiro avanço completa a frase; o próximo muda a página. Fechar diálogo não dispara golpe ou nova interação no mesmo frame. Checkpoint e portal mantêm suas artes distintas. Oferecer intensidade reduzida de flashes e clareza de avisos sem depender apenas de cor.

## Partículas e feedback

Katana: arcos prateados, rastros finos, vento azulado e impactos precisos. Punhos: poeira terrosa, detritos, fragmentos e ondas de choque pesadas. Moto: fumaça escura, rotação metálica, faíscas laranja/prateadas e impactos mecânicos. A defesa da Moto produz faísca curta no encontro das serras, com oclusão coerente.

Efeitos para ataques, esquiva, defesa, dano, morte, aterrissagem, colisão e finalização usam sistemas reutilizáveis com limites, pooling e limpeza. Não tingir a skin nem encobrir a animação. Hit-stop, vibração e movimento de câmera precisam preservar legibilidade e alinhamento da pixel art.

## Combate IA 2.0 — etapa posterior

Perseguição e posicionamento, controle de distância, separação dos inimigos, decisões contextuais, janelas de reação, cancelamentos, perseguição aérea e limites anti-infinito. Não prever inputs de modo injusto. Reutilizar o combate existente; validar filtros de colisão, buffer, esquiva, recuperação e reset de combos aéreos. Pacotes técnicos e código parcial não equivalem à aprovação da etapa completa.

## Áudio, distribuição e publicação

Preservar os recursos sonoros autorizados; não iniciar música automaticamente durante testes. Separar música, efeitos, ambiente e interface. Arquivos de músicas comerciais fornecidos para o protótipo pessoal ficam fora de GitHub, ZIPs públicos e redistribuição não autorizada. Novas músicas não fazem parte desta etapa.

A entrega final exige Windows jogável, início/continuação de partida, combate, transições, saves e preferências preservados. Fontes gráficas, importação, testes, abertura de executável e validação humana são evidências diferentes. Não apagar builds anteriores válidas ou arquivos únicos de recuperação.

Publicação autorizada do projeto somente pela cópia isolada e branch de integração já definidas. O site do jogo pode reunir apresentação, história, personagens, gameplay, galeria, trailer, devlog e download quando existir versão válida; não modificar outros projetos ou portfólio por consequência.

## Estado verificável em 03/11/2026

- Fabius Normal: lista curta de artes/animações encerrada, incluindo Punhos Walk SW e Moto Walk W/E; oito direções presentes em caminhada, esquiva, defesa, dano e morte das três armas.
- 170 clips / 504 figuras / 55 folhas ativas desta produção importados. Essa contagem não representa todos os arquivos históricos do jogo.
- 9 testes PlayMode isolados aprovados na execução final: 15 clips iniciais, 72 clips de estados essenciais, três caminhadas finais e provas de despacho/malha de faíscas. A malha foi renderizada numa câmera isolada; a composição na campanha não é certificada por esse teste.
- Gothic Eclipse: base técnica e atmosférica anterior existe; o overhaul completo e os gates dos dez mapas continuam como próxima etapa.
- Lucius Normal e prólogo: planos preservados; esta rodada não concluiu sua produção artística ou bossfight.
- Transformações e Combate IA 2.0: posteriores; nenhuma aprovação global por existência de scripts.
- QA geral da campanha e build Windows atualizada: pendentes.

A meta de conclusão do jogo permanece integral. O fechamento das animações Normal não declara toda a campanha concluída.

## Fabius — famílias gráficas e escopo fechado

## Corte aplicado

| Fila antiga | Decisão atual |
| --- | --- |
| Mais quatro técnicas gráficas da Katana | Dispensadas. Silêncio e Crepúsculo usam Leve3; Ruptura Precisa usa Pesado; Eclipse Partido usa Carregado. |
| Mais quatro técnicas gráficas dos Punhos | Dispensadas. Queda usa Martelo, Espiral/Órbita usam Giro, Sentença usa Juízo. |
| Cinco sequências aéreas novas por arma | Dispensadas como lotes próprios. Golpes aéreos usam figuras inteiras já prontas; perseguição é deslocamento sem contato, sem golpe inventado. |
| Dezoito técnicas restantes da Moto, cada uma com oito clips próprios | Fila cancelada. Golpes das serras compartilham Serra de Entrada/Dentes Cruzados. Pilotagem reaproveita a apresentação montada existente; impacto no chão é uma família, não uma sequência nova para cada nome. |
| Doze estados/variações próprios por arma | Reduzidos a defesa, dano e morte. Carregamento segura a antecipação já desenhada. Corrida e pequenas transições reaproveitam caminhada/idle quando coerentes. Sem novos lotes cosméticos de idles. |

A reutilização de ataques foi implementada explicitamente em `DarklightWeaponRedesignBank.AttackAnimationKey`, mantendo os tempos e eventos de cada comando. Não é remoção de combos nem mudança de dano. A tela pode continuar apresentando os nomes de variação existentes; eles não geram mais pendências gráficas individuais.

Código e artes desta produção estão implementados localmente. Publicação de runtime/artes continua condicionada aos gates finais; um commit de checkpoint documental não certifica nem publica essa implementação.

Carregamento preserva um clip próprio, caso exista; caso contrário usa a preparação completa revisada de Carregado/Juízo/Dentes. Não há montagem de membros, alteração de rosto, espelhamento nem recoloração da skin.


A lista curta desse contrato foi encerrada nesta revisão; não reabrir as filas canceladas. Continuam valendo os comandos, danos, tempos e eventos das técnicas compartilhadas.


---

# Gothic Eclipse — plano integral aprovado

Plano preservado integralmente. Em referências históricas à ordem de produção, prevalece a ordem vigente na abertura deste GDD.

# DARKLIGHT: REQUIEM — GOTHIC ECLIPSE
## Escopo gráfico e atmosférico enxuto — 02/11/2026

**PRIORIDADE:** executar somente após a conclusão e validação de Fabius Réquiem Normal.

**OBJETIVO:** transformar a atmosfera visual dos mapas existentes de DARKLIGHT: REQUIEM em uma experiência dark fantasy de alta qualidade, próxima da imagem de referência "Duelo Sob a Chuva Sombria", preservando integralmente a estrutura dos cenários, sprites, colisores e gameplay.

**DIRETRIZ ABSOLUTA: NÃO RECONSTRUIR OS DEZ MAPAS.**

A implementação deve aproveitar o pacote atmosférico preparado em 01/11, adicionando somente os efeitos necessários para atingir a referência.

Não transformar esta etapa em outro redesign extenso.

---

## 1. Recuperar o trabalho existente

Localizar e revisar os arquivos preparados pelo agente de atmosfera em 01/11:

- `DarklightAtmosphereController.cs`
- `AtmosphereOverlay.shader`
- `AtmosphereTextures.png`
- `AtmosphereTextures.json`

Consultar:

`Docs/Darklight/GOTHIC-ECLIPSE-PLANO-20261101.md`

Verificar os recibos e fontes da produção anterior.

**IMPORTANTE:** o pacote anterior foi produzido e revisado em código, mas não foi importado, compilado, executado ou aprovado visualmente.

Não declarar sua implementação concluída antes de executar os testes.

Preservar integralmente as fontes aprovadas.

## 2. Integrar a infraestrutura atmosférica

Copiar o controlador para:

`Assets/Darklight/Runtime/Campaign/DarklightAtmosphereController.cs`

Copiar o shader para:

`Assets/Darklight/Resources/Darklight/Rendering/AtmosphereOverlay.shader`

Copiar as texturas e o JSON para:

`Assets/Darklight/Resources/Darklight/AtmosphereTextures20261101/`

Configurar as texturas conforme o contrato existente: Default, alpha preservado, sRGB, Clamp, Bilinear, sem mipmaps e sem compressão com perdas.

A filtragem Bilinear deve ser aplicada somente aos overlays atmosféricos suaves. Preservar Point Filter nos sprites de personagens, mapas e elementos de pixel art que dependam de pixels nítidos.

Integrar em:

`DarklightRegionAtmosphere.cs`

No método:

`DarklightCampaign.InitializeRegionAtmosphere(int chapter)`

Imediatamente após `regionAtmosphere=null;` e antes do retorno antecipado, inserir:

`DarklightAtmosphereController.Attach(this,levelRoot,lab.WorldCamera,chapter);`

Verificar que a assinatura e o ciclo de vida continuam compatíveis com o código atual.

Não substituir o sistema de iluminação existente.

## 3. Aparência visual desejada

### A. Color grading dark fantasy

Aplicar aos pisos e fundos elegíveis:

- Sombras azul-marinho e azul-esverdeadas.
- Realces frios prateados.
- Dessaturação moderada.
- Contraste cinematográfico controlado.
- Preservação dos detalhes originais.

Não recolorir personagens, NPCs, inimigos, HUD ou elementos interativos.

Usar o material de filtro de chão reversível já preparado.

Não modificar permanentemente texturas ou materiais originais.

### B. Iluminação lunar

Aproveitar os três motivos `moon_0..2` existentes.

Criar feixes suaves de luz azul-prateada, com direção consistente e distribuição localizada.

A luz deve atravessar visualmente o ambiente sem criar uma camada branca opaca.

Evitar feixes exagerados, iluminação uniforme ou efeitos que dificultem enxergar os personagens.

Não adicionar sombras dinâmicas pesadas.

### C. Névoa em camadas

Aproveitar `fog_0..2`.

Manter:

- Névoa baixa.
- Profundidade entre camadas.
- Transparência suave.
- Movimento discreto, se compatível com as configurações de acessibilidade.

Não cobrir rostos, HUD, ataques ou objetos importantes.

### D. Chuva cinematográfica

Utilizar o pool existente de partículas.

Priorizar:

- Gotas finas e diagonais.
- Distribuição natural.
- Densidade moderada.
- Velocidade visual coerente.
- Variações discretas de comprimento e transparência.

A chuva não deve comprometer a leitura das animações de combate.

Não criar milhares de GameObjects individuais.

Respeitar o limite atual de 88 renderizadores de chuva/partículas.

### E. Piso molhado — módulo opcional

Este é um dos principais diferenciais da referência.

Adicionar uma camada leve de aparência úmida sobre pisos elegíveis, sem modificar os tiles originais.

Efeitos desejados:

- Brilhos azulados discretos.
- Reflexos âmbar próximos de fontes quentes.
- Pequenos círculos de gotas.
- Manchas úmidas com transparência variável.
- Destaques de luz em áreas selecionadas.

Não implementar reflexos completos em tempo real, espelhamento integral do personagem ou renderização duplicada de toda a cena.

Preferir shader simples ou sprites de destaque, com orçamento limitado.

**Regra:** se o piso não for adequado, desativar o efeito.

### F. Iluminação quente localizada — módulo opcional

A referência possui contraste entre luz lunar fria e iluminação âmbar.

Reaproveitar tochas, janelas, fogueiras e lanternas que já existirem nos mapas.

Adicionar apenas halos suaves ou brilhos locais.

Não criar novas construções, postes ou lanternas automaticamente.

Não modificar posições de objetos.

Não adicionar iluminação dinâmica custosa quando um overlay simples for suficiente.

Manter os efeitos de brasas e iluminação já existentes, evitando duplicação visual.

---

## 4. Presets por região

Os dez presets existentes devem ser preservados e ajustados individualmente.

Não aplicar chuva e névoa idênticas a todos os mapas.

Direção geral:

**Florestas:** névoa azulada, luar filtrado, sombras frias e chuva onde fizer sentido.

**Ruínas:** pedras escuras, tons verde-acinzentados, luz fria e brilhos discretos.

**Áreas externas construídas:** contraste entre luar e iluminação quente existente, com pisos molhados quando apropriado.

**Áreas internas:** atmosfera mais contida, sem chuva atravessando tetos ou paredes.

**Áreas de combate:** preservar visibilidade de hitboxes, personagens e efeitos ofensivos.

Utilizar os presets já criados em vez de reconstruir o sistema.

## 5. Integração gradual

### Gate 1 — Primeira região

Manter inicialmente:

`AllRegionsEnabled == false`

Ativar somente a região 0.

Realizar capturas antes/depois com câmera e personagem na mesma posição.

Verificar:

- Identidade do mapa preservada.
- Fabius com rosto, cabelo, pele e olhos inalterados.
- HUD legível.
- Nenhuma geometria alterada.
- Iluminação e névoa equilibradas.
- Piso molhado coerente, caso habilitado.
- Ausência de efeitos duplicados.

Desligar o controlador e confirmar restauração exata das cores originais do chão.

### Gate 2 — Funcionalidade e limites

Confirmar:

- `ApplyRegion(1)` não ativa efeitos quando todas as regiões estão bloqueadas.
- `ApplyRegion(0)` respeita o limite de partículas.
- Qualidade 0 reduz corretamente os efeitos.
- Intensidade 0 restaura a aparência original.
- Pausas e menus ocultam os efeitos conforme o contrato existente.
- Destruição e reinicialização não deixam renderizadores órfãos.
- Troca de mapa restaura o cenário anterior.

Não utilizar `ApplyRegion` como mecanismo de troca real de mapas.

### Gate 3 — Dez regiões

Após a aprovação visual da primeira região, habilitar as demais.

Usar:

`Current.SetAllRegionsEnabled(true)`

Validar a alternância dos presets 0–9 e a restauração das cores originais.

Não executar uma suíte gigantesca desnecessária; utilizar testes direcionados.

## 6. Performance e segurança

Preservar o sistema de pool e os limites de alocação existentes.

Não criar objetos ou materiais a cada frame.

Evitar:

- Sombras dinâmicas complexas.
- Reflexos reais de toda a cena.
- Pós-processamento excessivo.
- Ruído visual agressivo.
- Flashes e flicker.
- Alterações de câmera.
- Modificações de colisores.
- Recriação de mapas.

Respeitar os guardas de RAM e disco definidos no plano mestre.

Se um módulo opcional prejudicar desempenho, estabilidade ou fidelidade visual, desativá-lo sem comprometer a base atmosférica.

## 7. Preservação e reversibilidade

Toda alteração atmosférica deve ser reversível.

Não modificar permanentemente:

- Sprites originais.
- Mapas.
- Posições de personagens.
- Colisores.
- Saves.
- Materiais originais.
- Sistemas de interação.

O Gothic Eclipse deve funcionar como uma camada visual sobre o jogo existente.

Desabilitar os efeitos deve restaurar a aparência anterior sem resíduos.

## 8. Critérios de aprovação visual

Comparar o resultado com a referência enviada.

Buscar:

- Profundidade atmosférica.
- Luz lunar reconhecível.
- Sombras frias.
- Contraste quente/frio.
- Chuva natural.
- Névoa equilibrada.
- Piso úmido convincente.
- Pixel art original preservada.
- Personagens claramente visíveis.

Não tentar reproduzir literalmente a arquitetura ou os personagens da imagem.

A referência serve para iluminação, atmosfera, materiais aparentes e composição cromática, não para substituir os mapas.

Se a primeira versão parecer apenas um filtro azul escuro, ajustar a distribuição de luz e profundidade antes de adicionar novos sistemas.

## 9. Critério de encerramento

Gothic Eclipse estará concluído quando:

1. O controlador e shader estiverem integrados.
2. A primeira região estiver aprovada visualmente.
3. Os dez presets estiverem habilitados e verificados.
4. Os efeitos forem reversíveis.
5. Não houver regressões de gameplay.
6. A performance permanecer aceitável.
7. Os personagens e a HUD estiverem preservados.
8. Os resultados e capturas estiverem documentados.

Não prolongar esta etapa para criar efeitos cosméticos desnecessários.

Não declarar conclusão com base apenas em revisão de código.

## 10. Próxima prioridade

Após aprovação do Gothic Eclipse, iniciar imediatamente o escopo reduzido de **Lucius Normal**.

Não começar transformações ou IA 2.0 antes de concluir Lucius.

**OBJETIVO FINAL:** entregar uma atmosfera dark fantasy próxima da referência, sem reconstruir mapas, sem alterar a identidade dos personagens e sem transformar o Gothic Eclipse em outro ciclo extenso de produção artística.

Priorizar integração, qualidade visual, reversibilidade e desempenho.


---

# Lucius Normal — plano integral aprovado

Plano preservado integralmente. Em referências históricas à ordem de produção, prevalece a ordem vigente na abertura deste GDD.

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


---

# Prólogo cinematográfico — plano integral aprovado

Plano preservado integralmente. Em referências históricas à ordem de produção, prevalece a ordem vigente na abertura deste GDD.

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

---

# Referências e proveniência

Base de design: GDD histórico preservado em `GDD.md`; contrato `ESCOPO-CANONICO-20261026.md`; LDD vigente; escopos enxutos de 02/11; plano de prólogo de 03/11; recibos atuais de importação e testes. Fontes antigas permanecem preservadas, sem transformar skins, nomes ou filas superadas em tarefas atuais.
