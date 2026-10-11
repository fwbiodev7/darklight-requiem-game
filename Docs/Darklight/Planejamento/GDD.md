# DARKLIGHT: REQUIEM — GDD consolidado

Revisão: 10/10/2026. A autorização mais recente permite montar autonomamente a campanha completa. A versão atual contém os seis capítulos, cinco famílias de inimigos e elites, seis chefes, três formas, RPG, menus, save e Dark Resonance. O usuário fará os testes manuais; não confundir compilação/abertura com campanha ou balanceamento validados. Fábio e HUD preservam a referência aprovada com spikes pequenos.

## Visão do jogo

RPG de ação e hack and slash 2D topview, fantasia urbana sombria, exploração, combate expressivo e narrativa cinematográfica. Plataforma inicial: Windows. Teclado/mouse e controle devem permitir jogar e navegar por todos os menus. O desenvolvimento começa por uma experiência completa curta, seguida da campanha em seis capítulos.

Vespera sofre Rupturas após um eclipse incomum: a realidade mistura-se à dimensão chamada O Véu e surgem criaturas conhecidas como Ecos. Fábio sobrevive a um ataque e desperta Réquiem, espada que canaliza luz e escuridão. Lucien Veyr e o Projeto Eclipse pretendem romper a barreira entre os mundos.

Fábio é O Portador; Kael Voss é o rival de katana espectral azul; Lyra Moreau investiga as Rupturas; Lucien Veyr é O Arquiteto. Réquiem terá a forma de katana do concept aprovado. O protagonista preserva o rosto e cabelo das fotos, proporções adultas, sobretudo de couro preto, spikes discretos nos ombros e acessórios prateados. A versão sem óculos é a referência das vistas atuais; a alternativa com óculos ainda exige atualização ao figurino final.

## Direção visual e apresentação

Usar `$segredo-varginha-pixelart`: câmera de RPG topview, pixels quadrados, contorno escuro, sombras em grupos e identidade coerente em oito vistas. Preservar a mão dominante e a posição dos acessórios; não obter direções assimétricas apenas por espelhamento. Paleta do novo jogo: preto, carvão, vermelho profundo, violeta e luzes frias, com pele e cabelo naturais.

A folha aprovada é uma fonte visual de 1536×1024, com oito figuras estáticas. Sua resolução não representa a resolução lógica final. Recortes, direções, pivôs, alpha e escala precisam de acabamento antes do uso definitivo. A folha não contém animações.

Proposta de apresentação: mundo a 640×360, ampliação inteira 3× para 1920×1080; tiles novos de 32 pixels. Comparar corpos de 64 e 72 pixels de altura antes de fixar escala. Não converter automaticamente assets herdados com outros PPU. A auditoria detectou Built-in; instalação do pacote URP não comprova pipeline ativo. Escolha e correção do pipeline antecedem iluminação e configuração definitiva da câmera.

## Ciclo principal

Explorar → observar um encontro → combinar ataques e defesas → ganhar estilo e energia → transformar → vencer → recolher recompensa → conversar/investigar → abrir o próximo trecho. Checkpoints permitem repetir encontros sem atravessar diálogos já vistos. Missões secundárias acrescentam contexto e opções de combate.

## Combate

Movimento em oito direções, mira independente e leitura clara dos ataques. Réquiem oferece leve, pesado e carregado; combos iniciais: leve/leve/leve, leve/leve/pesado, leve/pesado, pesado carregado, esquiva/ataque, parry/contra-ataque e habilidade/ataque.

Definir cada golpe por antecipação, contato, recuperação, alcance, dano, stagger e cancelamentos. Input buffering permite antecipar a próxima ação sem disparar ações durante pausa ou diálogo. Cada golpe acerta cada alvo uma vez, salvo ataques explicitamente definidos como múltiplos. Esquiva, parry perfeito, invulnerabilidade, hitstop e super armor precisam de janelas legíveis e verificáveis. Finalizadores e repertório ampliado entram após o núcleo estar estável.

O rank D→SSS premia variedade e execução, perde valor com repetição e dano recebido e decai quando o combate fica inativo. Eventos de combate alimentam estilo, energia, áudio e HUD; a interface não calcula dano nem rank.

## Inimigos e chefes

Primeira experiência: Eco Rastejante, Eco Atirador e Eco Blindado. Depois: Eco Caçador, Eco Espectral e elites. Todos precisam de percepção, aproximação, preparação visível, ataque, recuperação, reação, morte e limites de coordenação para evitar ataques simultâneos injustos.

Kael Voss é o primeiro chefe: fase 1 com combos, parry, esquivas, investidas e cortes de energia; fase 2 acrescenta teletransporte curto, projéteis e sequências agressivas. O encontro provoca o despertar de Ruptura. Barra de vida, introdução, transição, derrota e recompensa compõem o encontro; pular a cinematografia deve preservar o estado correto da luta.

## Transformações

Normal → Ruptura → Eclipse → Absoluto. Implementar primeiro Normal/Ruptura: energia ganha em combate, ativação, duração, novas habilidades e retorno seguro. Eclipse e Absoluto pertencem à campanha ampliada. Modificadores devem ser removidos ao expirar, morrer ou transitar de cena, sem multiplicar permanentemente os atributos.

## Dark Resonance — opcional, integrado à campanha

Um MP3 sobrenatural transforma música em poderes temporários. A campanha principal deve funcionar integralmente sem desbloquear ou usar o aparelho. Os valores abaixo são hipóteses iniciais para testes, não balanceamento aprovado em gameplay.

| Modo | Efeito inicial proposto | Duração | Linguagem visual |
|---|---|---|---|
| Love Metal | Cura 1,4% da vida máxima por segundo, até o limite | 18 s | Aura vermelha e ornamentos góticos discretos |
| Thrash de Foco | +40% velocidade de ataque, +20% movimento, +20% energia de forma obtida | 14 s | Pulsos rápidos e faíscas acompanhando o ritmo |
| Heavy de Força | +45% dano, impacto dobrado e recuperação menor ao sofrer dano | 12 s | Pulsos graves e contorno defensivo legível |

Um modo ativo por vez. Recargas independentes: Love 32s, Força 28s, Foco 30s; custos 35/40/40 da energia própria do MP3. Trocar a seleção não elimina a recarga. A cura não ressuscita nem excede a vida máxima. Transformações permanecem independentes.

O MP3 permite selecionar música ou estilo e mostra faixa, modo, efeito, duração e cooldown. Músicas do mesmo estilo compartilham o modificador; trocar de faixa é uma escolha musical. No single-player, abrir o painel pausa a ação; após confirmar e fechar, Fábio coloca os fones numa animação interrompível por dano, sem invulnerabilidade gratuita. O poder começa ao concluir a animação. Antes de concluir, a interrupção não concede buff nem consome a ativação.

Teclado proposto: M abre o MP3, setas selecionam, Enter confirma, Esc volta. Controle: Back/Select abre, D-Pad seleciona, A confirma, B volta. Remapeamento e prompts acompanham o dispositivo; menus bloqueiam golpes. Não ocupar LB/RB/LT/RT já reservados ao combate. A seleção também deve permitir mouse.

Integração: modificadores temporários separados dos atributos permanentes; recalcular timing de animação e hitboxes juntos ao acelerar ataques. Testar o acúmulo com Ruptura antes de definir limites. Morte encerra o efeito; salvar/carregar e trocar de cena não podem eliminar cooldown. O planejamento de save inclui tempo restante e estado do sistema, sem permitir duração infinita por pausa ou troca de música.

VFX usam BPM e marcadores fornecidos pela faixa, com relógio de áudio e transições curtas. Não dependem de detectar automaticamente batidas no arquivo. Pulsos acompanham o ritmo sem mudar hitboxes, parry ou avisos dos inimigos. Volume zero não remove os poderes. Oferecer intensidade reduzida de flashes e efeitos para preservar leitura e conforto.

Missão secundária proposta: **Frequência do Véu**. Na antiga loja de discos de Vespera, Fábio ajuda a recuperar componentes do aparelho e fragmentos sonoros, aprende a selecionar um modo e recebe o MP3. A missão tem versão curta de tutorial e pode ser concluída depois do primeiro encontro com Kael, sem bloquear a história.

NPC original proposto: **Ícaro Nox**, técnico de som e colecionador de equipamentos antigos, com estética de rock gótico dos anos 2000. Criar rosto, figurino, voz e história próprios, sem reproduzir a identidade de artistas reais sem autorização.

Planejamento de arte pela skill: MP3 fechado/aberto/ativo/em cooldown; botões de seleção/foco/confirmação; três ícones de modos; fones como acessório; sequência de colocar e retirar fones em oito direções; retrato e vistas do NPC; auras e pulsos em folhas separadas. Interface pode ocupar aproximadamente 224×128 pixels lógicos, aparelho 32×32 e ícones 16×16, como ponto de partida. Revisar a legibilidade antes de fixar tamanhos. Pivôs dos fones seguem a cabeça; katana e braços devem respeitar oclusão. Manter spikes, rosto e cabelo do protagonista aprovado.

Trilha do protótipo pessoal: usar exclusivamente os cinco MP3 fornecidos pelo usuário. Love Metal: **Join Me in Death** e **Wings of a Butterfly**, de HIM. Thrash Metal: **Seek & Destroy** e **Creeping Death**, de Metallica. Heavy Metal: **Enter Sandman**, de Metallica. Right Here in My Arms e Sad But True saem da seleção. Essa associação é uma decisão de design dos modos, não uma classificação musical definitiva.

Os cinco arquivos foram recebidos e copiados sem alteração para `AudioSources/Darklight/DarkResonance/`, fora de Assets. O manifesto registra IDs, caminhos, hashes, tamanho e propriedades reportadas pelo Windows. Os cinco arquivos também foram importados em Assets/Darklight/Audio/Music como estéreo com Streaming. Reprodução é implementada; sincronização fina e marcação de BPM ficam para avaliação manual. Não adicionar outras músicas nem baixar faixas. O protótipo é individual e sem publicação prevista no momento. O NPC permanece original, sem copiar artistas reais. Detalhes constam em `trilha-prototipo.md`.

Entrega futura: protótipo dos modificadores após combate/Ruptura estáveis; interface e missão nas etapas de RPG/narrativa; sincronização e mixagem no polimento. Dark Resonance é uma extensão opcional e não bloqueia a primeira experiência jogável. Testes futuros: regeneração, dano/velocidade, redução defensiva, interrupção, duração/cooldown, morte, persistência, acumulação com Ruptura, áudio silenciado, sincronização e navegação sem mouse. O sistema foi implementado após a autorização posterior de montagem autônoma do jogo completo.

## RPG, narrativa e persistência

XP, níveis, atributos, inventário e pequena árvore de habilidades com escolhas úteis ao combate. Expandir equipamentos somente após a experiência inicial. Diálogos, objetivos e recompensas usam identificadores estáveis. Save próprio de DARKLIGHT, escrita temporária e substituição segura; isolar também chaves de configuração e remapeamento da campanha de Varginha.

Menus: novo jogo, continuar, opções, pausa, morte, vitória, créditos e retorno. HUD: vida, energia, habilidade/cooldown, rank, objetivo, chefe e indicador opcional de Dark Resonance. Não deixar nomes, textos ou progresso da campanha antiga aparecerem como conteúdo novo.

## Campanha de seis capítulos — integrada

As funções de cada capítulo abaixo são propostas de montagem sobre os títulos originais. Novos chefes, mapas e diálogos serão detalhados em seus próprios marcos.

| Capítulo | Função narrativa e jogável proposta | Dependências |
|---|---|---|
| 1. A Noite da Ruptura | Ataque em Vespera, Réquiem, investigação inicial, Kael e despertar | Núcleo completo da primeira experiência |
| 2. Vozes do Abismo | Lyra conduz a investigação, exploração de novos distritos e pistas do Véu; missão do MP3 disponível | Missões, persistência, inimigos ampliados |
| 3. Sangue Desperto | Revelar ligação de Fábio com o Véu e aprofundar Ruptura | Progressão e encontros que testem domínio do combate |
| 4. O Projeto Eclipse | Investigar Lucien e a organização, novos conflitos e despertar de Eclipse | Segunda forma, ambientes e chefes próprios |
| 5. Além do Véu | Explorar a dimensão e consequências das escolhas | Regras de exploração e progressão entre regiões |
| 6. O Último Eclipse | Confronto final, Absoluto e desfechos | Campanha integrada, estados narrativos e save verificados |

Cada capítulo precisa de começo, objetivo, exploração, encontros, chefe/momento central, recompensa, transição e possibilidade de retomar o save. Não preencher a duração com repetição obrigatória. Nenhuma duração da campanha foi medida.

## Controle, áudio e site

Portar o suporte verificado de Varginha com prompts, remapeamento, navegação, sliders, recuperação após desconexão e vibração. O adaptador local depende de PixelButtonHover; copiar somente o arquivo de navegação deixa dependência faltante. Completar validação final no G808 quando disponível; teste simulado não comprova o aparelho físico.

Separar música, efeitos, ambiente e interface. Música dinâmica acompanha fases de Kael e transformação. Planejar SFX para katana, defesa, impacto, passos e interface; medir custos antes de introduzir pooling indiscriminado.

Site oficial após material real aprovado: apresentação, história, personagens, gameplay, galeria, trailer, devlog, download quando existir, FAQ e desenvolvedor. Link ao portfólio https://devfabio.vercel.app e rodapé “Desenvolvido por Fábio — devfabio.vercel.app”. SEO, acessibilidade, teclado, movimento reduzido e responsividade. Publicação e alterações no portfólio continuam exigindo autorização específica.

## Critérios de produto

A primeira entrega jogável deve conter arena noturna navegável, protagonista aprovado, oito direções, katana, combos, esquiva, parry, habilidade, três inimigos, rank, Ruptura, Kael em duas fases, HUD, áudio/efeitos, controle, menu/pausa, checkpoint e reinício. Meta de 10–15 minutos e 60 FPS depende de medição.

A campanha completa exige, além disso, seis capítulos integrados, progressão e saves de longo prazo, transformações posteriores, chefes e narrativa próprios, conteúdo opcional e créditos. A preparação para publicação terá revisão própria da trilha. Não confundir a cópia auditada ou uma folha de arte com jogo completo.



## Revisão aprovada de jogabilidade — 10/10/2026

Seis layouts modulares com exploração e encontros posicionais, combos rápidos com buffer no hitstop, navegação/cancelamentos e colisões, 68 quadros complementares, três poderes próprios e HUD/menu compactos. GUIA ATUAL: outputs/melhorias/LEIA-ME.md. 35 testes de lógica e 9 de cena distintos aprovados. Compilação sem erros; áudio e controle físico não testados. NPCs, formas e algumas ações ainda usam poses individuais. Música de menu e configurações/cinemática adicionais aguardam aprovação do novo plano.
