# DARKLIGHT: REQUIEM — Beta 1.0, revisão de combate

Build de avaliação. O balanceamento exige sessões manuais e ajustes a partir do feedback.

## O que mudou

- Três perfis de arma: katana com cortes direcionais, adagas com estocadas rápidas e múltiplos contatos, corrente com varredura e controle. Os dados originais permanecem como fonte; os perfis são cópias de sessão.
- Sequência de leves, variação com pausa, final pesado após dois leves, carregados, lançamento curto, contra-ataque e troca durante recuperação de acerto confirmado. Ataques bloqueados não confirmam acerto.
- Resolução de contato por forma e fase do golpe, bloqueio por paredes, deduplicação por alvo e pulso, postura, quebra de guarda e limites de hitstop.
- Inimigos com aproximação lateral, defesa e reação com atraso conforme papel/dificuldade. Coordenação limita ataques simultâneos e ameaças fora da câmera. Elites e chefes prolongam sequências.
- Estilo reduz crédito por repetição. Trocar arma sem acertar não concede estilo.
- Vermelha: maior dano de postura, compromisso de recuperação e risco. Roxa: mobilidade e conservação da sequência na esquiva. Branca: extensões após acerto e troca mais cedo.
- Normal, Difícil e Extrema; Extrema desbloqueia após conclusão da campanha, incluindo saves antigos com final registrado. Mudam reação, pressão, defesa e encadeamento, sem depender de aumento de vida.
- Arena pelo menu inicial ou pausa: alvo passivo, caçador, blindado, atirador, grupo misto com elite e Kael; seleção de armas/formas/dificuldade, dano opcional e visualização de hitboxes. Usa cópia do progresso, sem XP ou gravação da campanha.

## Como avaliar

1. Extraia o pacote inteiro e abra `DarklightRequiem.exe`. Entre em **Arena de treinamento**.
2. Pratique leves consecutivos; leve, pausa curta, leve; dois leves e pesado; carregado; troca de arma após acerto. Use o menu de controles para consultar os atalhos atuais/remapeados.
3. Ative hitboxes. Confira frente, costas e diagonais, limite de alcance e obstáculos da campanha.
4. Compare as três armas e as formas normal/vermelha/roxa/branca nas duas aparências. Confira legibilidade e sensação de peso.
5. Selecione blindado e depois grupo misto. Compare Normal e Difícil, ative receber dano e observe janelas de defesa, pressão dos atiradores e recuperação após erro.
6. Saia da arena e confira retorno ao capítulo, equipamento e progresso. Teste chefes e transições na campanha. A build de referência anterior continua disponível separadamente.
7. Registre local, dificuldade, arma/forma, ação e comportamento esperado/observado; imagens ou vídeo ajudam a reproduzir problemas.

## Verificação executada

- 57 testes de edição: modelo de combate, ramificações, buffers, cancelamentos, formas, estilo, dificuldade, bindings e deslizamento.
- 13 testes finais em execução com Direct3D: arena, preservação do progresso, trilhas selecionadas sem reprodução, guarda/quebra, suspensão visual, papéis ativos, diferenças reais de cadência, contato de três armas em oito direções e bloqueio por parede/alcance, menus, captura da interface e recuperação após morte no treino.
- A regressão física percorreu os objetivos dos dez mapas em ambos os sentidos com o raio do jogador.
- Executor confirmou preservação dos saves e restauração das preferências em ambas as rodadas.
- Medição isolada do modelo: 10.000 ticks após aquecimento, 0 bytes alocados e 3 ms no total. Isso não mede o FPS do jogo completo.
- Build Windows concluída com zero erros e 16 avisos; inicialização silenciosa executada sem erros registrados no log. Pacote com 197 arquivos e CRC verificado.

## Limitações

- Não houve avaliação humana completa da campanha, teste com controle físico ou audição das músicas nesta sessão. Áudio não foi iniciado automaticamente.
- A meta de 60 FPS em encontros completos e a comparação de desempenho antes/depois ainda exigem medição representativa; o microbenchmark não comprova essa meta.
- A tentativa de comparação gráfica com o verificador legado não avançou no modo de execução sem janela e foi encerrada. Não foram publicados números de FPS dessa tentativa.
- O equivalente aéreo é suspensão visual curta de inimigos leves; a colisão permanece no chão. Blindados e chefes resistem. Não é um sistema de plataforma 3D.
- Foram reutilizadas as animações e identidades aprovadas. Os chefes mantêm suas fases existentes, agora com ajustes de coordenação e sequência; não foram produzidos novos conjuntos completos de animação.
- Balanceamento de dano, janelas, pressão e pontuação é inicial e deve ser validado manualmente.

