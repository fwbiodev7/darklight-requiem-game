# Gothic Eclipse | implementação visual

A referência é «Duelo Sob a Chuva Sombria». A primeira cena de comparação é a Rua das Cinzas. O enquadramento deve revelar os rostos no centro, os sulcos das pedras sob os pés, brilhos azulados onde a chuva bate e reflexos âmbar perto das lamparinas.

1. Usar o filtro existente do chão para enfatizar as sombras frias e preservar áreas quentes da pintura. Aplicar pequenos brilhos horizontais descontínuos de pavimento em regiões chuvosas. Fixar o padrão em coordenadas do mundo para que a câmera não arraste as poças.
2. Ancorar o reflexo âmbar nas posições das lamparinas dos checkpoints que o remaster já utiliza. Reusar o material de atmosfera e os sprites suaves do controlador. Limitar a quantidade, às regiões úmidas e aos níveis de qualidade médio e alto.
3. Regular a transparência e direção da chuva existente, os feixes frios e a névoa baixa. Manter o orçamento existente de partículas e congelar a animação de brilho ao pausar. Em qualidade baixa, usar apenas a coloração do chão, chuva reduzida e névoa existente.
4. Na Unity, capturar o mesmo enquadramento com atmosfera ligada/desligada nos três níveis, em pausa e movimento. Conferir ordenação dos reflexos, legibilidade dos atores, troca de mapa e restauração ao desativar. Comparar tempo de CPU, GPU e chamadas de desenho no Player Windows antes de ajustar o orçamento.
