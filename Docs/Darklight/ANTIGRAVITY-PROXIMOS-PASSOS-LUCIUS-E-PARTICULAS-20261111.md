# Antigravity — integração de Lucius e ajuste geral das partículas

Data: 11/11/2026, America/Sao_Paulo. Base consultada: `antigravity/lucius-integration-delivery-20261110`, commit `673b6bf88e5278d35564820fdacae62f031d2fd6`. Produção artística separada em `codex/lucius-art-20261111`.

## Divisão expressa do trabalho

Pedido atual: Codex produz somente artes; Antigravity assume toda a implementação, importação, calibração no jogo, testes e builds. Primeiro completar as caminhadas, depois melhorar os ataques pela referência principal. O usuário aprovou a caminhada exibida e determinou não repetir gerações por diferenças mínimas imperceptíveis. Não reabrir esse polimento sem defeito relevante demonstrado.

Os corpos devem animar **no lugar**. Apenas o controlador de movimento do ator deve deslocar sua raiz; no caso de Lucius, a IA envia os comandos ao mesmo sistema físico. Uma torção, flexão ou movimento da espada não autoriza deslocamento adicional no renderer, em eventos de animação ou em root motion. Preservar movimentos mecânicos já intencionais e identificar sua origem antes de alterá-los.

Este documento é uma lista de implementação futura. Nenhuma correção técnica, importação Unity, Play Mode, teste de combate, avaliação sonora ou build é executada pela autoria deste pacote. Resultados verdes citados nos handoffs de 10/11 são históricos.

## 1. Receber o pacote e preservar o estado atual

1. Comparar a branch técnica atual com a base acima, preservar alterações próprias e integrar apenas os PNGs selecionados, seus prompts, manifesto e revisões. Não consumir pastas `candidates`, prompts não executados ou prévias como texturas de runtime.
2. Ler `ArtSources/EntregaLucius20261111/manifest.json` e o README da mesma pasta. Conferir SHA-256 de cada fonte selecionada antes de gerar `.meta` ou catálogo. O manifesto é de entrega artística, não um catálogo diretamente carregável pelo runtime.
3. Não substituir os pixels de S/SW/W/E já existentes de Walk com espada nas costas apenas por atualizar as quatro direções faltantes. Reutilizar as páginas antigas corretas. WalkArmed tem novas fontes em oito direções. Novas artes de ataque substituem uma família somente quando suas fases/direções correspondentes estiverem disponíveis e conferidas.
4. Preservar GUIDs e IDs dos assets que serão atualizados; para novas folhas, gerar IDs próprios e não reciclar os IDs de Fabius, Lucien ou Encapuzado. Não alterar enum/saves ou fazer renomeação global de Lucien.

## 2. Fatiar e calibrar as figuras completas

1. Medir o tamanho **real** de cada PNG: a resolução solicitada ao gerador pode diferir da resolução entregue. Usar os `rect` do manifesto e confirmar que incluem cabelo, mãos, botas, espada e efeitos completos. Não inferir células pelo nome do arquivo.
2. Não aplicar auto-trim por quadro que desloque o apoio. Preservar o canvas completo. Não apagar RGB oculto sob alpha zero: o visualizador pode exibir esse RGB como névoa, embora o PNG seja transparente. Conferir alpha sobre fundos claro/escuro antes de concluir que existe fundo residual.
3. Importar como Sprite Multiple, Point, sem mipmap e sem compressão que destrua os clusters; conferir configurações reais do projeto e material pixel. Não reduzir a folha pelo Max Size inadvertidamente.
4. Manter `WorldVisualScale = 1.325`. Comparar altura anatômica cabelo–sola com o Idle final aprovado. PPU calibra densidade da fonte; não deve corrigir cabeça, idade, corpo ou espada desproporcionais. A referência Idle atual usa PPU `214.97090106361628`; isso não significa que fontes novas de outra resolução devam herdar esse número cegamente.
5. Calibrar pivôs pelo eixo da pelve/apoio no chão, não pelo centro dos bounds da espada ou do arco. Converter coordenadas top-left da fonte para o sistema Unity bottom-left de forma explícita. Nas vistas de costas, a ponta da bainha e a bota elevada não definem a raiz. Registrar a coordenada escolhida por figura inteira.
6. Fazer preview alinhado pelo apoio antes da importação definitiva. Uma origem de célula fixa não prova apoio fixo quando o desenho está deslocado dentro da célula. Não mudar a escala por fase nem compensar drift de pivô com deslocamento físico do ator.

## 3. Completar Walk e implementar WalkArmed de verdade

Arquivos principais: `Assets/Darklight/Runtime/Character/DarklightLuciusSpriteBank.cs` e `DarklightLuciusView.cs`.

1. Acrescentar Walk NW/N/NE/SE, mantendo o mapa `S=0, SW=1, W=2, NW=3, N=4, NE=5, E=6, SE=7`. Conferir vistas reais sem flip de assimetrias, especialmente hilt/bainha e acessórios.
2. Acrescentar WalkArmed S/SW/W/NW/N/NE/E/SE como páginas próprias. As quatro fases são contato A, passagem A, contato B, passagem B; referência temporal existente `[140,130,140,130]` ms, ciclo de 540 ms. Não alterar a velocidade física para compensar arte.
3. **Bloqueio comprovado:** `Action()` reconhece `WalkArmed`, mas `ValidPage()` rejeita `loop=true` para essa ação (`DarklightLuciusSpriteBank.cs:47–49,75`). Incluir WalkArmed nas ações que podem fazer loop e acrescentar verificação focada para uma página armada válida e para os casos inválidos.
4. **Ligação ausente:** `DarklightLuciusView.Present()` trata apenas `action == "Walk"` como locomoção (`:35–45`). A seleção atual vem de `DarklightWeaponRedesignBank.Action()` e não escolhe automaticamente WalkArmed. Definir estado explícito de arma sacada/guarda do próprio Lucius e selecionar WalkArmed quando se desloca armado; Walk quando a espada está embainhada. Não consultar skin/arma global de Fabius para decidir esse estado.
5. Fazer Walk e WalkArmed usar o relógio por distância percorrida, direção de deslocamento real e reinício coerente da passada. Evitar reiniciar o ciclo todo frame ao alternar flags; parado deve retornar à guarda/repouso coerente, sem correr a animação no lugar.
6. Preservar a postura de duas mãos e uma única espada: ao entrar em combate não podem coexistir lâmina empunhada e espada duplicada nas costas. A bainha vazia pode continuar visível. Draw ainda exige seleção artística própria; não apresentar um fallback como saque finalizado.
7. Corrida pode reutilizar a caminhada conforme escopo reduzido, se a velocidade visual for aceitável; esquiva e perseguição continuam controladas pela física. Não criar novas famílias cosméticas.

## 4. Ligar os três ataques à referência principal e ao relógio real

Autoridade: `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png`, SHA-256 `a8d17c24cebb4a1dadccddc54983f17068803fe10014dcfb1b0840cbda8dde17` (idêntica ao master em ArtSources).

1. Manter exatamente `SwordHorizontal`, `SwordVertical`, `SwordThrust` no perfil e nas páginas. Horizontal precisa ler como varredura transversal, Vertical como descida, Thrust como avanço axial da espada. Não usar o antigo active horizontal semelhante a estocada como prova de que o golpe novo está integrado.
2. Registrar uma sequência completa de antecipação, contato e recuperação por direção. Os ataques existentes usam tempos nominais `[140,120,220]` ms, mas a janela real continua pertencendo ao `AttackDefinition` de cada instância.
3. Preencher `startupFrames`/`activeFrames` e conservar o remapeamento por `DarklightLuciusSpriteBank.AttackTime()` (`:176+`), sincronizando o contato desenhado com `attack.startup`, `attack.active` e `attack.recovery`. Não usar o clock de Idle nem um timer independente para decidir dano.
4. Quando as páginas nativas de Thrust estiverem disponíveis, verificar `TryActionPage` por chave exata antes de chamar a apresentação. O fallback em `TryPose()` (`:145–155`) continua útil para ausência real; não contabilizá-lo como cobertura nativa da estocada.
5. Travar facing no momento do commit do ataque; perseguição/strafe não podem girar uma estocada já iniciada. Cancelamento, parry, stagger e morte devem interromper o golpe e seus efeitos coerentemente.
6. Manter `RegisterHit` para dano único por alvo/serial mesmo com vários ticks ativos. Não criar dano duplicado em Animation Event quando o modelo já resolve a janela ativa.
7. Para separar arte e movimento, monitorar raiz física, `LuciusVisual.localPosition`, escala e rotação do renderer. `LateUpdate()` atualmente faz snap da posição visual para a raiz (`DarklightLuciusView.cs:19–22`); manter o snap coerente sem um segundo sistema de follow somando offsets ou movimento à figura.
8. As artes podem conter os arcos canônicos. Decidir por golpe se o rastro é embutido ou emitido em VFX separado; não somar dois arcos equivalentes. Metais e brilho de ponta discretos não substituem confirmação de hit.

## 5. Corrigir posição e escala de TODAS as partículas V2

Queixa atual do usuário: efeitos fora do lugar, alguns minúsculos, outros grandes. Não considerar a aprovação da imagem mestre como aprovação desses parâmetros no jogo.

### Achados confirmados na base

- `DarklightCharacterView.cs:139–143` emite ataque em `display.position + Facing * .45`, uma âncora genérica que não acompanha mão/lâmina/roda ou contato desenhado.
- `Runtime/World/DarklightActionParticles.cs:103–104` usa alturas fixas `.12/.4/.85` para esquiva/morte/dano; `:134` escala o caminho antigo pela largura dos bounds.
- `Runtime/World/DarklightPixelContactBurst.cs:60–61` soma `Vector3.up * .65` ao ponto e emite tanto o efeito antigo quanto V2. `DarklightCombatFX.cs:11–12` também solicita o impacto geométrico. Há múltiplos caminhos no mesmo evento; sua sobreposição é causa provável de tamanho/aparência incorretos.
- `DarklightPixelContactBurst.cs:71` fixa sorting 901; V2 usa `OrderForY(groundY)+2` (`DarklightCombatParticlesV2.cs:305`). A mistura muda a leitura de profundidade.
- V2 lê `recommendedScale` (`:152`) e o aplica à `localScale` (`:331`) sem normalizar pela dimensão visível alfa por família. O atlas possui fatias de cerca de 101 px de largura, apesar de o guia pedir amplitudes diferentes para contato, katana, punhos e moto.
- V2 gira katana pelo facing, mas as demais famílias em geral só fazem `flipX` (`:308–319`). Uma diagonal não fica necessariamente alinhada à trajetória.
- Os pivôs importados continuam afetando SpriteRenderer, mas os campos de pivô/retângulo do manifesto não resolvem sozinhos a âncora semântica de emissão. Não concluir que o pivô foi ignorado integralmente pelo Unity.

### Implementação solicitada

1. Inventariar as sete famílias, todos os call sites e os caminhos legados. Definir explicitamente a camada responsável por contato, rastro de ataque, esquiva, moto e acerto pesado. Emitir apenas as camadas pretendidas por evento; fallback legado somente quando o novo não estiver disponível, se essa for a política escolhida.
2. Adotar dimensões-alvo visíveis por família/intensidade, preservando proporção e orientação. Usar o guia aprovado de 10/11: contato 32/48/64, katana 64/96/128, punhos 40/56/72 e os tamanhos de moto/esquiva lá definidos. Esses números são referência de pixels de desenho, **não unidades Unity**; medir como aparecem na câmera de gameplay e em relação à altura do personagem.
3. Para um sprite com largura visível alfa `w`, densidade `PPU` e escala efetiva de pais `p`, calcular largura no mundo como `(w / PPU) * localScale * p`. Derivar `localScale` do alvo no mundo, em vez de multiplicar toda família por 1.0 e supor tamanho uniforme. Medir também altura e envelope máximo do ciclo, sem normalizar cada frame separadamente e causar pulsação.
4. Definir âncoras de emissão por semântica: hit no ponto realmente resolvido entre atacante/alvo; katana junto ao movimento da lâmina; punhos no membro atacante; moto junto ao pneu/solo; esquiva atrás do ator conforme deslocamento real. Não passar o índice aleatório de clip como ID de arma a `TryEmitContact`; verificar esse mapeamento atual em `DarklightPixelContactBurst.cs:61`.
5. Para efeitos que seguem a arma, usar âncora local da pose ou dados por fase/direção. Para impacto no mundo, congelar a posição de contato no instante do acerto. Para rastro de esquiva, desacoplar sua origem do facing quando facing e velocidade forem diferentes. Não arrastar um impacto já emitido junto do personagem.
6. Conferir oito direções e as assimetrias da arte. Definir rotação/flip por família e intenção; não girar automaticamente fumaça vertical, chão ou roda sem conferir a arte. Preservar pixels nítidos e material/aparência compatíveis.
7. Unificar a referência de profundidade por `groundY`, separada da elevação visual; efeitos diante/atrás do personagem podem precisar de offsets diferentes. Eliminar dependência acidental de sorting 901 quando a cena usa ordenação por Y.
8. Verificar duração, avanço dos 8 frames, pool, limpeza ao morrer/reiniciar treino, reduced flashes e ausência de emissão duplicada. Não tocar em trilha, MP3 ou reprodução sonora como parte desse ajuste visual.

## 6. Verificação focada a executar pelo Antigravity

| Verificação | Evidência de aceite |
|---|---|
| Walk + WalkArmed, oito direções | Páginas nativas localizadas; deslocamento físico avança passada; parado interrompe; arma única; sem fallback contado como arte nativa |
| Estabilidade com ator parado | Capturar raiz/renderer/pivô entre fases; raiz não se move por desenho; variação visual não vem de escala/posição arbitrária por sprite |
| Movimento simultâneo | Controlador desloca a raiz uma vez; renderer acompanha; sem drift, double movement ou slide artificial introduzido pela integração |
| Horizontal/Vertical/Thrust | Chave, direção e startup/contact/recovery conferidos; contato coincide com janela de dano; interrupção limpa e dano único |
| Partículas | Cada família em 8 direções, leve/pesado, hit/miss, treino reiniciado, fundos claros/escuros; capturas antes/depois com dimensões e âncoras medidas |
| Não regressão | Fabius Katana/Punhos/Moto; Lucius independente de Lucien e do input/skin do player; saves/preferências preservados |
| Build final | Relatório e hash da nova build, execução standalone registrada separadamente do sucesso de compilação |

Rodar apenas os testes ligados às mudanças efetivas e as regressões justificadas. Ampliar a suíte se surgir falha nova. A suíte V2 antiga verifica carregamento/nome/contagem/emissão básica; não certifica posição correta, tamanho percebido, duplicação, orientação ou sorting. Acrescentar cobertura para esses contratos, com tolerâncias geométricas explicitadas e inspeção visual em câmera real.

Unity deve ter um único responsável: Antigravity nesta divisão. Conferir a autorização vigente para Play Mode na sessão dele; não herdar a exceção sonora de uma rodada antiga como liberação geral. É possível preparar testes visuais silenciosos respeitando a suspensão de áudio.

## 7. Retorno exigido do Antigravity

Entregar commit/branch, fontes e hashes consumidos, lista de páginas por ação/direção, alterações de código/pivô/PPU/âncoras, capturas de partículas antes/depois e logs novos. Separar **produzido, revisado, importado, testado automatizado, verificado visualmente no runtime e build executada**. Enumerar faltantes reais, inclusive Draw/estados essenciais que ainda não possuem fonte final; não declarar Lucius inteiro concluído apenas por finalizar caminhada e três golpes.
