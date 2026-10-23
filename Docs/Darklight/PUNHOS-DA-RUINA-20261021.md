# Punhos da Ruína — 21/10/2026

Nome escolhido pelo usuário: **Punhos da Ruína**, para o conjunto de manoplas e grevas que substituirá corrente/foice no terceiro estilo. Eclipse Fangs / Presas do Eclipse é somente o nome de trabalho anterior. Não confundir com as adagas Presas do Véu.

O usuário aprovou expressamente o visual gerado: “caralho ficou muito foda as armas novas aprovadissimo bro”. Depois escolheu o nome sugerido Punhos da Ruína: “punhos da ruinha pode ser”. Não solicitar novamente essa aprovação visual ou de nome.

## Entrega desta etapa

Prévia local: `outputs/eclipse-fangs-20261021/candidate-v1/index.html`. Fontes integrais, prompts e referências com SHA-256: `ArtSources/EclipseFangs20261021/candidate-v1/`.

- 12 poses completas de Requiem normal e 12 de Rock normal: guarda, soco, chute lateral, giro, uppercut aéreo, chute aéreo, finalização e recuperação.
- Oito repousos estáticos comparando duas skins × quatro formas. Normal carmesim, Ruptura escarlate, Eclipse violeta, Absoluto branco azulado. Preservar olhos vermelhos, pele e identidades das skins.
- Oito vistas estáticas de repouso da Requiem normal, na ordem S, SW, W, NW, N, NE, E, SE. A SW incorreta foi substituída por uma figura inteira nova. Sete figuras originais mantêm seus pixels; a figura SW nova recebeu redução uniforme por nearest-neighbor, documentada no manifesto.
- PNG RGBA, JSON, contrato Unity ainda não consumido, histogramas completos compactados, contatos, HTML silencioso e GIFs de estudo.

Auditoria executada: quatro atlas, 40 figuras, zero erros estruturais. Avisos por atlas: 17, 17, 9, 9; incluem paleta contínua/alpha parcial e retornos intencionais de poses. A geração não certifica uma paleta indexada nem resolução lógica nativa de pixel art. A fonte de cada folha é 1536×1024; a correção SW tem 1223×1286. Os atlas entregues possuem dimensões, pivôs e PPU provisório registrados em cada `sheet.json`.

Antes de qualquer integração, 607 arquivos de produção pertinentes foram verificados sem alteração. Nenhum arquivo Unity foi modificado nesta etapa. Não houve Play, música, teste sonoro, teste de controle físico, build nova, commit ou push. A aprovação visual do usuário não equivale a importação ou verificação de runtime. Contatos/animações ainda exigem revisão visual na interface; os quadros-fonte foram inspecionados.

Log completo do empacotamento: `../work/compact/20261021T215005Z-cb4dfa15-eclipse-fangs-candidate.log`; execução 0, sem timeout.

## Pendências autorizadas

Continuação e estado mais recente: [ANDAMENTO-PUNHOS-20261021.md](ANDAMENTO-PUNHOS-20261021.md). A declaração acima de Unity não modificado pertence à entrega inicial da prévia; o perfil/importador foram preparados depois, ainda sem importação da matriz completa.

1. Integrar a substituição no índice 2, preservando GUID do asset da arma, desbloqueio após Matriarca e compatibilidade dos saves. Preservar integralmente katana, adagas e o quarto estilo.
2. Implementar perfil próprio de artes marciais: contatos curtos de soco/chute, giro, launcher aéreo, chute descendente, carregado e finalizações; remover puxão/alcance e efeitos de corrente. Atualizar nome, ícone, HUD e instruções para Punhos da Ruína.
3. Completar intermediários, ataques em oito direções e a matriz de duas skins × quatro formas. Oito vistas de repouso da Rock e locomoção/defesa/dano pertinentes ainda faltam. Não preencher direções ou ações ausentes com outra arma, sprite espelhado ou pose incorreta.
4. Vincular aura de punhos/antebraços/pernas/pés e todos os rastros/impactos à forma ativa. Distinguir melhor a intensidade carmesim normal da escarlate Ruptura. Aumentar energia em combos/carregados sem tingir pele/íris, roupa ou cristais indevidamente.
5. Conferir anatomia, olhos, pele, escala e paleta em 1×/nearest-neighbor; revisar clips em movimento e corrigir defeitos localizados por quadros completos.
6. Importar usando ferramentas existentes, Point, sem mipmaps/compressão; calibrar PPU/pivôs/fases e eventos; executar verificações silenciosas pertinentes. Não iniciar Play ou áudio sem nova instrução explícita.
7. Após implementação e testes, documentar pendências datadas e fazer commit/push somente das mudanças pertinentes ao DARKLIGHT, preservando alterações alheias e excluindo candidatas rejeitadas, conforme autorização local existente.
