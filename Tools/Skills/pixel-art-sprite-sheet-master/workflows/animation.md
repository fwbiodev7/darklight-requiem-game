# Desenho e teste de animação

Planeje somente ações/direções necessárias ao consumidor. Não confunda seis fases de um ataque com seis direções. Para oito vistas, registre S, SW, W, NW, N, NE, E, SE ou o mapeamento existente. Personagens assimétricos exigem desenho coerente nas vistas opostas.

| Ação | Desenho e verificação |
|---|---|
| Idle | Respiração/peso sutis e piscada quando apropriado; arma e pés estáveis. Movimento do canvas não substitui desenho. |
| Walk/run | Contato, descida, passagem e subida; alternância real das pernas, apoio coerente, balanço de braços e velocidade proporcional ao deslocamento. |
| Ataque/combos | Antecipação → aceleração → contato → seguimento → recuperação. Alcance, mãos e arma legíveis; cada golpe deve conectar à próxima guarda. |
| Esquiva | Impulso, trajetória e recuperação. Separar deslocamento do personagem e animação; registrar janela de invulnerabilidade no sistema existente. |
| Defesa/dano | Contato e reação reconhecíveis; preservar centro de massa, equipamento e recuperação. |
| Morte | Progressão sem loop indevido; último quadro sustentado. Preservar sequências protegidas e reversão existentes. |
| Objetos/armas | Eixo mecânico, estados e material constantes; efeitos não mudam o volume físico. |

De um sprite existente, gere os **desenhos completos** faltantes mantendo identidade e articulação. Não chamar duplicação, interpolação, bobbing ou deformação de ciclo real. Reaproveitar um hold idêntico é válido quando intencional e documentado (`allowStatic`); não apresentá-lo como ciclo de locomoção.

Pivô de personagem em chão: apoio definido pela pose e referência, não centro automático da bounding box que varia com a arma. Use âncoras anotadas e offsets inteiros. Poses aéreas, morte e mergulho requerem âncora intencional, não forçar pés à mesma linha. `pack` calcula canvas comum que comporta os quadros completos em torno dos pivôs; não estima anatomia nem corrige desenho.

Prévia obrigatória: 1× e zoom inteiro, fundo claro/escuro, play/pause, passo de frame, velocidade e loop. Verifique início/fim, deslizamento de pés, jitter, pop de silhueta, troca de mão, legibilidade do contato e transição com idle. O helper oferece contato PNG, HTML sem áudio e GIF de inspeção (GIF quantiza cores; PNG/JSON continuam a entrega autoritativa). Eventos de dano e root motion devem ser testados no consumidor Unity, não inferidos do GIF.
