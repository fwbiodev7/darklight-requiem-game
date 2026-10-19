# Estado e pendências — 19/10/2026

Este registro substitui a lista de 18/10. A prévia e a build são entregas distintas.

## Concluído nesta sessão

- Prévia local recuperada em http://127.0.0.1:8551/previa.html, com servidor persistente restrito ao computador. Página principal atualizada para a revisão v6.
- Pele das duas skins e quatro formas baseada exclusivamente na Requiem original, mediana RGB 246/174/130. Correção de renderização por fonte, sem pintar PNGs, cabelos, roupas ou armas. O usuário aprovou essa correção.
- Nove folhas brancas com cristais físicos recuperadas e conferidas; fontes anteriores preservadas. Comprimento aprovado do cabelo branco mantido como referência.
- Revisão de identidade facial das 24 folhas de katana/adagas/corrente, nas duas skins e quatro formas, usando as caminhadas e os ataques da Cavaleiro como referência. Correção adicional dos olhos nas três transformações conforme seus repousos, incluindo a Cavaleiro; as fontes aprovadas anteriores permanecem preservadas. Esta revisão externa não foi importada no Unity.
- Altura original fixa por skin: Requiem 266 px e Rock 278 px, com uma escala uniforme por folha medida pela guarda completa. Sem redução automática pelo tamanho da arma, janela ou ataque. Isso não certifica anatomia idêntica em todos os desenhos.
- Combos e efeitos distintos por arma, 28 páginas de ataque; caminhar com arma sem ciclo próprio agora fica indisponível com aviso, sem trocar silenciosamente para katana.
- Correções no jogo: quarto golpe das adagas; quadro e rastro de contato preservados quando um tick atravessa toda a fase ativa; cores dos rastros por forma; aura/transformação ancoradas no corpo neutro; remoção da corrente procedural duplicada.
- Preservada a correção de layout de mapa já existente no projeto. Um teste atual verificou molduras do HUD, atalho [SELECT] e limites dos objetivos.
- Unity: 10 casos EditMode aprovados, após corrigir uma montagem incorreta do objeto de teste da paleta; 7 casos PlayMode aprovados, incluindo morte/checkpoint, backup de save, pontes, combate sustentado e HUD. Armazenamento de teste isolado.
- Nova build Windows x64 1.0.13 gerada sem erros. O executável passou pela conferência silenciosa das quatro formas da Cavaleiro, derrapagem, impacto, retorno de serras, movimento e pausa. Saves e preferências preservados. A arte integrada anteriormente continua nesta build.
- Oito arquivos protegidos de morte/reversão permanecem iguais aos hashes originais. Sem reprodução de música, teste auditivo ou teste com controle físico.

## Prioridades para amanhã — apontadas pelo usuário

1. **"remover olheiras vermelhas inexistentes criadas por erros seus"** — há manchas vermelhas indevidas sob os olhos nas candidatas de ataque, inclusive na forma roxa mostrada pelo usuário. A tentativa de conter a saturação dos olhos não resolveu a região abaixo deles. Remover essas manchas nas duas skins e conferir as três transformações contra a pose parada correspondente, preservando o tom de pele aprovado.
2. **"melhorar qualidade das pixel art baseando-se na pose parada"** — usar o repouso aprovado como autoridade de rosto, contorno, acabamento e proporções; comparar também caminhada e Cavaleiro. As novas folhas continuam candidatas, sem aprovação artística final. Não gerar novas variações desconectadas nem corrigir identidade apenas por escala.

Referência do problema: captura enviada em 19/10, `codex-clipboard-c82068c6-0668-4c95-a22d-d8bbaf7c4115.png`. Nesta etapa fica documentado para amanhã; não iniciar outra rodada de geração hoje.

## Demais pendências

1. Ciclos reais das outras sete direções de caminhada, nas duas skins e formas, e caminhada das demais armas. As tentativas SW/SE que repetiram a mesma perna continuam excluídas.
2. Ataques nas outras direções e continuidade completa de caminhar/parar/atacar com cada arma e transformação.
3. Conferência artística final da nova revisão de rostos, largura das cabeças, relação torso/pernas e consistência de cabelo/cristais em todos os estados. Conferência técnica e escala uniforme não equivalem a aprovação visual nem garantem desenhos idênticos à referência.
4. Integração das novas folhas coerentes da prévia no Unity, seguida de revisão dos eventos de dano, colisões, efeitos e transições afetados. Gerar outra build depois dessa integração; a 1.0.13 é a revisão técnica com a arte já integrada.
5. Áudio: completar os efeitos dedicados restantes da Cavaleiro/moto e revisar sincronização, volumes e ambientes/inimigos. Audição continua suspensa conforme a instrução de não iniciar som automaticamente.
6. Verificar o percurso completo das três pontes do mapa 5. O teste atual verificou a ativação dos três corredores e a diferença visual de profundidade na primeira ponte.
7. Avaliação de desempenho mais longa no executável e dos seis capítulos após a integração visual. O combate no Editor teve p95 de 6,9–8,0 ms, com picos de 55,7–112,3 ms; isso não certifica FPS no executável ou em outros computadores.

## Evidências e entrega

Consulte `QA-RUNTIME.json`, os quatro XMLs Unity de 19/10, `QA-FONTES-20261019.json`, `QA-CONTROLES-20261019.json`, `QA-NAVEGADOR-20261019.json` e `FONTES-COERENCIA-ATAQUES-20261019.json`.

Pacote técnico: `outputs/revisao-runtime-20261019/`, com resumo, hashes e arquivo Windows 1.0.13. A publicação autorizada usa somente uma lista explícita no clone de entrega; exclui saves, fontes rejeitadas e exclusões alheias do checkout principal. O site público e o portfólio não fazem parte desta etapa.

Build publicada: [release 1.0.13](https://github.com/fwbiodev7/darklight-requiem-game/releases/tag/v1.0.13). Commit técnico: `7ec2eccc86feedcb0b04e9639ddadb313723463e`. ZIP confirmado no GitHub por tamanho e SHA-256.
