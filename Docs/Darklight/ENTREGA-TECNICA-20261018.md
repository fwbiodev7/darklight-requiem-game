# Revisão técnica — 18/10/2026

## Alterações

- Caminhadas Requiem: seis fases por ciclo, com duração de 0,4 s preservada, três armas e oito direções. Escala uniforme por folha calibrada contra o repouso, sem redimensionar membros ou alterar o tamanho do renderer entre poses.
- Ataques da moto: oito folhas frontais/diagonais revistas e nove folhas direcionais transformadas novas (108 poses completas), usando os movimentos e identidade aprovados em transformacoes-animacoes-20261017. Laterais direitas reutilizam espelhamento do banco já existente; não são oito vistas únicas desenhadas.
- Leitor direcional compatível com os nomes lineares legados e a nova organização linha/coluna. Verificação de cobertura inclui todas as doze poses em cada direção e forma. Máscaras identificam personagens inteiros e impedem o aparecimento de figuras vizinhas.
- Repouso/movimento: revisões de folhas existentes e paleta baseada exclusivamente na Requiem original, mediana RGB 246/174/130. Cabelo branco conserva a referência aprovada; não houve nova alteração na sequência protegida de morte/reversão.
- Áudio: efeito metálico nos ataques da moto; contato de pouso único por aterrissagem, preservado durante pausa e descartado na morte/troca de mapa. Recursos existentes mantidos; nenhuma música adicional. Auditoria silenciosa de arquivos, importação, eventos e controles.
- Captura do executável com limites de espera para transformação e métricas de carregamento, memória e combate.

## Validação

67 testes focados de EditMode passaram, sem falhas ou testes ignorados. Auditoria das 53 fontes de produção confirmou cópia inalterada dos PNGs selecionados e integridade dos oito arquivos protegidos de morte/reversão. Auditoria dos 84 WAV existentes passou; formatos restantes e vínculos são verificados pela Unity, que carrega 132 efeitos.

24 testes jogáveis atuais passaram, sem falhas ou ignorados, com saves preservados e preferências restauradas. Incluem ataques/montagem/desmontagem/golpe e defesa da moto nas quatro formas, cobertura direcional real, seis fases de caminhada, 576 amostras de transição de caminhada, regressão de combate/poderes/troca de identidade, sete equipamentos em combate sustentado, controles de volume, ponte e quatro fluxos de Game Over. O teste direcional aguarda explicitamente o término da transformação antes de comparar o sprite: sua primeira execução não fazia isso e foi corrigida.

Total desta revisão: 91 testes focados aprovados (67 EditMode + 24 PlayMode). Build Windows 1.0.12 concluída com zero erros e três avisos; executável validado duas vezes com saves preservados, seis mapas, oito combinações de skin/forma e retorno ao normal após a morte.

Na última execução fora da tela: 909 quadros, 15 ataques, mediana 5.61 ms e p95 7.15 ms em RTX3050. Pico residente externo 574.7 MiB, memória privada comprometida 1.78 GiB. Medições não garantem FPS interativo. Tempos de preparação de mapas com recursos carregados e um quadro: 53.4–186.6 ms; não representam abertura a frio. A API interna de pico de RAM retornou indisponível; a medição externa foi registrada separadamente.

Na saída do player a Unity registrou aviso de buffer gráfico de 4096 bytes; não houve erro de gameplay registrado. Nenhuma alocação própria desse tipo foi localizada no Runtime nesta revisão.

Os testes são silenciosos. Não constituem audição dos efeitos nem teste de controle físico. Medições em câmera fora da tela não garantem FPS em exibição interativa. Estudos adicionais de adagas que ficaram somente em outputs não fazem parte desta revisão.
