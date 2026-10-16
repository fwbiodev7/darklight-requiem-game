# Beta 1.0 — build1.0.8

Dez miniartes detalhadas novas, HUD contextual com vida à esquerda e mapa no canto superior direito. A tela do atlas usa as molduras e botões existentes do jogo, fundo opaco, painéis separados e nove ícones góticos novos. A primeira revisão de ícones rejeitada não é a versão apresentada nas capturas atuais.

Pontos de retorno ampliados de1,3 para2,3 unidades lógicas; portais de campanha2,5→4,2; saídas de região1,3→4,2, sujeitos à escala ambiental existente1,325. Âncoras, interações e colisões preservadas. Capturas reais mostram o personagem ao lado dos objetos, sem ocultá-los.

## Testes e evidências

-43 testes atuais de Play Mode aprovados/0 falhas, incluindo combate, serras sustentadas, montagem, investida, derrapagem, esmagamento, finalização III, engrenagens, exclusividade Rock, outras armas, remapeamento, persistência, HUD, Atlas e marcos ampliados. Saves preservados e preferências restauradas. Há também118 testes de Editor aprovados na rodada anterior desta entrega; total161 casos distintos, não somar repetições dos testes de mapa.
- Build1.0.8: sucesso,0 erros/1 aviso conhecido do pacote Pipeline sem RuntimePipelineConfig. Os19 avisos anteriores de Runtime foram corrigidos; não afirmar zero avisos em todo o projeto.
- Executável1.0.8:12 amostras automatizadas, quatro resoluções, exploração escura/clara e combate da moto com três IAs. Limites de vida/minimapa/mapa ampliado válidos, nenhuma exceção capturada, saves e preferências preservados. Capturas reais revisadas;1024×768 mantém a interface dentro da tela, com textos menores.
- Texturas do minimapa:1024×688, Point, sem mipmaps,4.998.048 bytes ativos cada medidos na Unity. Essa quantidade não é a memória total do jogo.

## Desempenho observado

RTX3050/Ryzen5 5500, preset3, VSync desativado, limite120FPS,5 segundos por amostra após6,3 de aquecimento; codificação de PNG fora da medição.

| Resolução | Intervalo das médias FPS | Pior p95 de frame |
| --- | --- | --- |
| 1280×720 | 118.83–120.20 | 9.28 ms |
| 1600×900 | 118.66–120.17 | 9.78 ms |
| 1920×1080 | 120.11–120.15 | 9.47 ms |
| 1024×768 | 117.72–120.06 | 9.99 ms |

Janela de QA em segundo plano; parte da execução coincidiu com testes no Editor. São medições curtas com carga concorrente, não ensaio isolado/prolongado nem avaliação humana. O viewport pixel perfect existente produz barras em resoluções maiores/mais altas; não esticar a pixel art para ocupá-las.

## Limitações reais

- Dispositivo XInput detectado e comandos simulados testados. Controle físico, conforto, diversão, áudio e balanceamento humano ainda precisam de avaliação manual. Nenhum áudio reproduzido nessa rodada.
- A correção anatômica aprovada do levantamento frontal normal está integrada, razão de altura corporal0,9947586 contra o personagem parado. A correção das pernas nas formas transformadas está em revisão e NÃO está concluída nesta build. Fontes novas com poses mecanicamente inadequadas não foram importadas.
- O catálogo autoral completo de ataques/pilotagem em oito direções e quatro formas continua pendente. Há reuso e espelhamento em parte das ações; sprites importados não comprovam a qualidade final das animações.
- Campanha, mapas7–10, Interface R2 e saves preservados. As miniartes interpretam visualmente os mapas; colisão vem da geometria real, sem afirmar correspondência artística de cada pixel pelos testes matemáticos.
- ZIP completo verificado por CRC/SHA256; não inclui saves, arquivos de proteção ou artes rejeitadas. Novos GIFs serão registrados após a publicação.
