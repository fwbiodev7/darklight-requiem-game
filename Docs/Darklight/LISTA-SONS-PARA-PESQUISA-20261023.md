# DARKLIGHT REQUIEM — lista de sons para pesquisar

Data: 23/10/2026. **Lista de aquisição e implementação futura; nenhum som foi importado, baixado ou reproduzido nesta tarefa.** O usuário pesquisará e enviará os arquivos.

## Como usar

- **P1:** primeira remessa; identidade da moto, combate aéreo e inimigos novos.
- **P2:** completar e melhorar armas, personagem, poderes e chefes.
- **P3:** polimento de ambientes, interações e interface.
- **N:** não foi identificado um arquivo dedicado a esse evento no banco inspecionado. Pode haver reaproveitamento genérico; não significa silêncio absoluto no jogo.
- **R:** já há base genérica, sintética ou reaproveitada. Procurar uma melhoria é opcional até a revisão auditiva; não precisa comprar ou baixar tudo novamente.
- Um ID representa um **conjunto sonoro**, não obrigatoriamente um MP3 por ação. Sons compatíveis podem ser reutilizados. A escolha definitiva depende da escuta e da sincronização em gameplay.

O projeto já contém **132 arquivos em Resources/Darklight/Audio**: 49 na raiz, 82 na expansão e um na revisão de criaturas, além de **cinco MP3 em Resources**. Há passos, impactos, interface, ambientes, poderes e identidades de inimigos. Os 82 recursos da expansão são sintéticos conforme o registro existente. A presença desses arquivos não certifica qualidade auditiva nem cobertura específica de cada evento.

## 1. Motocicleta — P1

Preferir motor pesado e agressivo, metal, corrente, pneu e serra mecânica. Enviar motor e serra separados quando possível, sem música de fundo.

| ID | Som procurado / uso | Estado | Pesquisa sugerida |
|---|---|---|---|
| MT01 | Partida e ignição do motor | N | heavy motorcycle engine start |
| MT02 | Motor em marcha lenta, loop estável | N | motorcycle idle engine loop |
| MT03 | Aceleração, arrancada e desaceleração | N | motorcycle rev acceleration deceleration |
| MT04 | Motor em movimento, loop de pilotagem | N | motorcycle driving engine loop |
| MT05 | Desligamento do motor | N | motorcycle engine shutdown |
| MT06 | Montagem e separação das duas metades; travas mecânicas | R | mechanical weapon assemble metal latch |
| MT07 | Troca de marcha, engrenagem e sobrecarga | R | gear shift mechanical overload |
| MT08 | Serra ligando, corte contínuo em loop e parada | N | chainsaw start cutting loop stop |
| MT09 | Contato de serra em corpo e em armadura, separados | R | chainsaw impact flesh metal armor |
| MT10 | Pneu derrapando e retomando aderência | N | motorcycle tire skid drift |
| MT11 | Atropelamento e colisão em cenário | R | heavy vehicle collision impact |
| MT12 | Salto, suspensão da moto e aterrissagem pesada | R | motorcycle suspension jump landing |
| MT13 | Serra Infernal: aceleração da lâmina e corte final | R | spinning blade attack impact |
| MT14 | Giro Devastador: rotação de metal e impacto circular | R | spinning heavy weapon whoosh |
| MT15 | Esmagamento Vertical: preparação e impacto no solo | R | heavy slam ground crack debris |
| MT16 | Roda Destruidora: roda acelerando e lançamento do alvo | R | tire spin heavy launch impact |
| MT17 | Cavalgada Brutal: arrancada curta e contatos sucessivos | R | motorcycle charge ram impact |
| MT18 | Execução Aérea Motorizada: queda, esmagamento e detritos | R | cinematic descending slam debris |

MT13–MT18 podem ser construídos com MT01–MT12 e os impactos abaixo. Não é necessário encontrar seis arquivos longos com a sequência inteira pronta.

## 2. Combate aéreo e impactos — P1

| ID | Som procurado / uso | Estado | Pesquisa sugerida |
|---|---|---|---|
| AR01 | Uppercut/lançamento: golpe seco e impulso ascendente | R | uppercut launch impact whoosh |
| AR02 | Perseguição aérea: impulso curto de vento/energia | N | air dash rush whoosh |
| AR03 | Leves e chutes no ar, com variações | R | aerial punch kick impact |
| AR04 | Arremesso do inimigo e deslocamento pelo ar | N | body throw air whoosh |
| AR05 | Segunda perseguição e mergulho descendente | N | downward dive rush whoosh |
| AR06 | Inimigo batendo no solo e corpo caindo | R | body fall ground impact |
| AR07 | Onda de impacto, rachadura e pedras caindo | R | shockwave ground crack stone debris |
| CB01 | Corte em alvo orgânico: leve e pesado | R | sword flesh hit light heavy |
| CB02 | Impacto em armadura: leve e pesado | R | sword armor metal impact |
| CB03 | Defesa comum e ataque bloqueado | R | sword block shield impact |
| CB04 | Parry perfeito e contra-ataque | R | perfect parry metal ring |
| CB05 | Quebra de postura/guarda | N | guard break stagger impact |
| CB06 | Crítico e finalização, sem trilha embutida | R | critical hit finisher stinger |
| CB07 | Golpe errando/cortando o ar | R | weapon swing miss whoosh |

Não procurar “som de hitstop”: a pausa e o controle da mixagem são funções do sistema, não exigem outro MP3.

## 3. Armas do player — P2

O terceiro slot atual usa **Punhos da Ruína**. Não solicitar um pacote de corrente como se ainda fosse a arma principal desse slot.

| ID | Som procurado / uso | Estado | Pesquisa sugerida |
|---|---|---|---|
| WM01 | Requiem/katana: sacar e guardar | R | katana unsheathe sheathe |
| WM02 | Katana: corte leve, pesado e carregado | R | katana slash heavy charged swing |
| WM03 | Presas/adagas: sacar e cortes rápidos alternados | R | dual dagger draw slash |
| WM04 | Punhos: soco curto, soco pesado e impacto com manopla | R | armored gauntlet punch impact |
| WM05 | Punhos: chute lateral e chute giratório | R | martial arts kick spin whoosh |
| WM06 | Punhos: uppercut, martelo e esmagamento | R | uppercut hammer fist ground slam |
| WM07 | Punhos: Cataclismo, ciclone e energia dos golpes | R | dark energy fist shockwave cyclone |
| WM08 | Troca de arma e prontidão após equipar | R | weapon equip swap foley |

Os movimentos podem reutilizar CB/AR. Não precisa duplicar o mesmo impacto para cada arma, skin ou direção.

## 4. Personagem, poderes e transformações — P2

| ID | Som procurado / uso | Estado | Pesquisa sugerida |
|---|---|---|---|
| PC01 | Esquiva, roupa e deslocamento rápido | R | dodge cloth movement whoosh |
| PC02 | Receber dano leve/pesado; esforço curto sem fala | R | male pain grunt exertion |
| PC03 | Queda, morte e recuperação do personagem | R | body fall death effort foley |
| PC04 | Respiração de esforço e vida baixa, discretas | R | exhausted breathing heartbeat |
| PC05 | Elixir: frasco, beber e recuperação | R | potion bottle drink heal |
| PC06 | Roupa/couro/equipamento em movimento | R | leather armor cloth movement |
| PW01 | Garras Carmesins: três cortes e contato | R | dark claw slash impact |
| PW02 | Passo Sombrio: sumiço, deslocamento e reaparição | R | shadow teleport dash |
| PW03 | Sede de Sangue: ativação e absorção | R | vampire blood drain dark magic |
| PW04 | Abraço do Véu: sucção/atração de inimigos | R | dark magic pull vortex |
| PW05 | Frenesi Carmesim: ativação, energia e término | R | blood rage power activation |
| PW06 | Julgamento de Sangue: preparação e corte final | R | dark magic sword judgment impact |
| PW07 | Ruptura: transformação e dissipação | R | violent dark transformation burst |
| PW08 | Eclipse: transformação e dissipação | R | void shadow transformation |
| PW09 | Absoluto: transformação e dissipação | R | radiant energy transformation |
| PW10 | Dark Resonance: colocar/tirar fones e ativar/encerrar poder | R | headphones equip power activation |
| PW11 | Recarga concluída, recurso insuficiente e habilidade bloqueada | R | ability ready cooldown error cue |

As três músicas/modos de Dark Resonance e os temas existentes não precisam ser reenviados apenas por causa desta lista. Falas/dublagem completa são opcionais e ficam fora do pacote obrigatório de efeitos.

## 5. Inimigos novos — P1

| ID | Som procurado / uso | Estado | Pesquisa sugerida |
|---|---|---|---|
| VN01 | Detonador Musgoso: criatura vegetal, movimento e alerta | N | plant monster growl rustle |
| VN02 | Detonador: carga crescente, explosão e recuperação | N | organic charge explosion debris |
| VN03 | Detonador: dano e derrota | N | plant creature hurt death |
| VN04 | Espectro Neon: ruído eletrônico, passos e alerta | N | cybernetic ghost glitch movement |
| VN05 | Espectro: investida, rajada/projétil e contato | N | cybernetic dash energy projectile |
| VN06 | Espectro: dano, falha e desligamento/derrota | N | robot glitch damage shutdown |
| VN07 | Ronin das Cinzas: roupa, passo e preparo de duelo | N | samurai cloth sandal movement |
| VN08 | Ronin: sacar, corte rápido e defesa de katana | R | iaido katana draw slash parry |
| VN09 | Ronin: dano, esforço e derrota | N | warrior hurt death grunt |

Atualmente os visitantes reutilizam identidades de tipos de inimigos existentes. Estes IDs pedem identidade própria; não imitar sons reconhecíveis extraídos dos jogos citados nos easter eggs.

## 6. Inimigos existentes e chefes — P2

Os inimigos existentes têm texturas/efeitos de base; muitos eventos reutilizam o mesmo arquivo com mudança de pitch. Para cada família abaixo, procurar **alerta/preparação, ataque, dano e derrota** em arquivos separados ou facilmente identificáveis. Respiração e passos são complementares. **Todos R**: escutar a base atual antes de substituir.

| ID | Família / identidade | Pesquisa sugerida |
|---|---|---|
| EN01 | Crawler: criatura baixa, rastejo e rosnado | crawling monster growl |
| EN02 | Shooter: preparação e disparo sobrenatural | dark magic ranged projectile |
| EN03 | Armored: armadura pesada, ataque e defesa | heavy armored enemy foley |
| EN04 | Hunter: perseguição, investida e ataque veloz | agile monster dash attack |
| EN05 | Spectral: presença etérea, ataque e dissipação | spectral ghost attack death |

Para os chefes abaixo, procurar um conjunto de **presença/entrada, preparação legível, ataque especial, dano, transição de fase e derrota**. Os dez já têm assinaturas sintéticas; o pedido é melhorar/diferenciar, não presumir ausência total.

| ID | Chefe atualmente listado na campanha | Direção sonora |
|---|---|---|
| BS01 | Kael Voss | Guerreiro, lâmina, força controlada |
| BS02 | Cantor do Abismo | Vocal sobrenatural, ressonância, coro sem música embutida |
| BS03 | Matriarca Rubra | Criatura vampírica, sangue e impacto pesado |
| BS04 | Sentinela Eclipse | Servo/mecanismo, carga energética e metal |
| BS05 | Guardião do Véu | Presença grave, magia e impacto de guardião |
| BS06 | Lucien Veyr | Conjuração, energia e ameaça controlada |
| BS07 | Valtherion — Primeiro Guardião | Armadura ancestral e golpes de grande peso |
| BS08 | Seraphine — Rainha da Noite | Presença espectral/vampírica e magia |
| BS09 | Kael — Cavaleiro Fraturado | Guerreiro fraturado, energia instável e lâmina |
| BS10 | Nocthar — Rei do Vazio | Vazio profundo, pressão e ruptura |

Os rótulos da última coluna são orientação artística para pesquisa, não uma afirmação de ataques específicos que ainda não foram conferidos individualmente. Não comprar um pacote por chefe antes da seleção. Um conjunto de variações pode servir a mais de um.

## 7. Passos e ambientes — P3

**Passos existentes — R:** pedra/concreto, madeira, terra, grama, areia, água rasa, metal, piso interno e lama. Buscar 3–5 variações por material apenas onde os sons atuais não atenderem. Aterrissagem pode reutilizar o mesmo material com impacto apropriado. Não há necessidade de um som por direção ou por skin.

**Camadas existentes — R:** vento externo, ambiente de masmorra/Véu, cidade, chuva, folhas, água e caverna; ocasionais de trovão, goteira, criatura distante, sussurro e portal. Melhorar as fontes após escuta. Evitar enviar dez cópias do mesmo vento.

| ID | Local atual | Sons complementares a procurar | Estado |
|---|---|---|---|
| AM01 | Rua das Cinzas | Cidade vazia, vento por frestas e detritos | R |
| AM02 | Distrito do Vinil | Loja/interior, ruído de vinil e aparelhos, sem canção de fundo | N |
| AM03 | Catedral do Sangue | Interior reverberante, correntes e sinos ocasionais | N |
| AM04 | Laboratório Eclipse | Ventilação, zumbido elétrico, terminais e máquinas | N |
| AM05 | Jardim dos Ecos | Folhagem, água e fauna distante discreta | R |
| AM06 | Observatório Vespera | Vento em altura, mecanismo e energia | N |
| AM07 | Cripta dos Primeiros | Pedra, gotejamento, presença subterrânea e ecos | R |
| AM08 | Catedral do Crepúsculo | Interior amplo, reverberação e ressonância sobrenatural | R |
| AM09 | Véu Profundo | Vazio, sussurros sem fala inteligível e distorção | R |
| AM10 | Coração do Eclipse | Pulsação, energia instável e ruptura ambiental | R |

Os ambientes de AM02/03/04/06 já podem ter camadas gerais; N se refere aos detalhes dedicados listados. Sinos, máquinas e objetos só devem tocar onde a cena/interação justificar.

## 8. Exploração, progressão e interface — P3

| ID | Som procurado / uso | Estado | Pesquisa sugerida |
|---|---|---|---|
| IN01 | Portal: presença em loop, ativação e travessia | R | magic portal hum open travel |
| IN02 | Selo: carregar, romper e liberar passagem | N | magic seal break stone energy |
| IN03 | Relíquia/memória: descoberta e coleta | R | relic memory pickup chime |
| IN04 | Checkpoint: ativação e recuperação | R | checkpoint activation heal |
| IN05 | Porta/grade/mecanismo, quando a cena usar | R | stone door metal gate mechanism |
| IN06 | Terminal, servidor e aparelho interativo | N | computer terminal server activation |
| IN07 | Fragmento, moeda e recompensa | R | shard coin reward pickup |
| IN08 | Subir de nível e desbloquear arma/forma/habilidade | R | level up ability unlock cue |
| IN09 | Objetivo, missão e desafio concluídos | R | quest objective complete stinger |
| IN10 | Descoberta de easter egg e encontro secreto | N | secret discovery dark fantasy cue |
| UI01 | Navegar, confirmar, voltar, abrir e fechar menu | R | dark fantasy interface click confirm back |
| UI02 | Erro, opção bloqueada e ação sem recurso | R | interface denied error sound |
| UI03 | Diário, inventário e página/livro | R | inventory journal book page |
| UI04 | Mapa: abrir, marcar, desmarcar e objetivo | R | map marker interface ping |
| UI05 | Ajuste de opção/volume e equipar item | R | interface slider equip click |

## 9. Momentos narrativos e resultado — P2/P3

| ID | Som procurado / uso | Estado |
|---|---|---|
| EV01 | Entrada de chefe: sinal curto de ameaça | R |
| EV02 | Mudança de fase: assinatura curta, sem reiniciar a música | R |
| EV03 | Chefe derrotado/vitória: sinal de conclusão | R |
| EV04 | Morte do player/game over: encerramento curto | R |
| EV05 | Primeiro despertar e revelação sobrenatural: preparação/impacto | R |
| EV06 | Transição narrativa: entrada, tensão e encerramento de cena | R |

Primeiro reutilizar o que já existe e conferir a cena. Não é necessário buscar diálogos ou músicas completas para cada transição. EV01–EV06 são efeitos/assinaturas curtas; trilhas musicais são tratadas separadamente.

## 10. Músicas — já fornecidas ou existentes

- **Já fornecidas, integração adiada:** Going Under → primeira bossfight principal; Afterlife → segunda; See U in Hell → terceira e última. Arquivos confirmados em Downloads. Não precisa pesquisá-los novamente. Requisitos em `PENDENCIA-MUSICAS-BOSSFIGHTS-20261023.md`.
- O código atual lista dez chefes. A seleção das três bossfights **principais** para essas músicas será conferida pela progressão real ao retomar a implementação; não associar automaticamente as faixas aos três primeiros nomes desta lista.
- **Já existentes:** temas de Rock/Réquiem, modos de Dark Resonance, primeiro despertar e trilha de menu. Conferir as referências atuais antes de pedir arquivos adicionais. Não substituir nem remover estas trilhas como parte da aquisição de efeitos.
- **Opcional, depois da revisão:** vinhetas musicais próprias de vitória/game over/capítulo/cutscene, somente onde uma trilha apropriada ainda fizer falta. Não são obrigatórias para começar pelos sons de P1.

## Como enviar os arquivos

1. **Comece por MT01–MT12, AR01–AR07 e VN01–VN09.** Depois envie melhorias dos conjuntos R que você considerar fracos no teste manual.
2. Pode enviar MP3. Se o mesmo efeito estiver disponível em WAV, esse original também serve e é útil para efeitos curtos; não precisa converter os arquivos antes de enviar.
3. Nome sugerido: `MT02_motor_parado_loop_01.mp3`, `AR04_arremesso_01.mp3`, `VN05_neon_investida_01.mp3`. O ID permite associar o arquivo mesmo que o título original seja diferente.
4. Para impactos/passos/menus: som isolado curto, aproximadamente 0,1–2 s; poderes/transições geralmente 0,5–5 s. São guias de seleção, não limites rígidos.
5. Para motor/serra/ambientes: trecho limpo, idealmente 5–30 s ou mais, sem voz de apresentador, música, marca d'água ou efeitos estranhos. Indique se já forma loop; a continuidade ainda será validada na implementação.
6. Quando possível, envie 3–5 variações de passos/impactos e 2–3 vocalizações por tipo. Não precisa reunir todas as variações na primeira remessa nem enviar duplicatas apenas com volumes diferentes.
7. Junto dos arquivos, mande nome/ID, link de origem, autor e licença/permissão disponível. Não extrair sons protegidos de outros jogos para redistribuição sem autorização. A futura publicação deve respeitar a licença de cada arquivo; as três músicas comerciais não entram em builds distribuídas sem autorização específica.

## Trabalho técnico posterior — não requer novos MP3

- Associar os arquivos a eventos e contatos reais, incluir identidade dedicada dos visitantes e diferenciação de armas; preservar os sons aprovados e os originais enviados.
- Integrar a altura real de `DarklightAirCombo` à detecção sonora de aterrissagem: o observador atual calcula altura por animação do ataque e moto, ainda sem consultar essa altura física nova.
- Sincronizar motor/serra com partida, uso, parada, pausa, morte, abandono e mudança de cena; evitar loops órfãos.
- Tratar fades, volumes, variedade, prioridade, limites de vozes, distância/pan, ducking, importação, carregamento e memória no sistema existente.
- Validar as três músicas de chefes sem reinício por fase, sem competição com trilhas de skin/transformação e sem vazamentos entre cenas.
- Fazer testes de integração silenciosos e depois escuta autorizada em gameplay: equilíbrio, repetição, clipping, loops, fadiga e legibilidade dos ataques. Esta lista foi feita por inspeção de código/arquivos; nenhum áudio foi ouvido nesta tarefa.

Referências locais da inspeção: `DarklightSoundDirector`, `TodayCampaignSoundDesign`, `TodayAudioSurfaces`, `DarklightRegionSoundProfile`, `DarklightEnemyVisitors`, perfis dos Punhos/moto, `DarklightStory`, `Docs/AUDIO-EXPANSION-20261013.md` e `Docs/Darklight/AUDIO-REPAGINACAO-20261017.md`.
