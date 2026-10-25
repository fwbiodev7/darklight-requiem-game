# Prévia parcial das armas — 24/10/2026

**O usuário não aprovou a entrega visual.** O feedback final exige tratar como urgentes na próxima sessão: punhos novos somente de frente, Normal visualmente próxima da Ruptura e demais direções/animações/combos ainda baseados em artes anteriores com filtros. A prévia apresenta essa limitação no início da página. Não há declaração de remasterização completa.

## Abrir novamente

Duplo clique em `outputs/previa-armas-20261024/ABRIR-PREVIA.cmd`. O atalho usa o Python já fornecido pelo Codex e inicia, quando necessário, um servidor vinculado somente a `127.0.0.1`, em janela oculta. Reaproveita um servidor local que já contenha a prévia. O arquivo de apoio `abrir-previa.ps1` mantém o procedimento legível. Não altera política de execução, assets ou preferências; não inicia áudio.

O endereço da rodada é `http://127.0.0.1:8766/outputs/previa-armas-20261024/index.html`. O atalho continua útil quando esse servidor temporário estiver encerrado; encontra outra porta local e abre o endereço adequado.

## Conteúdo e limites

A página mostra uma arma por vez: katana, Presas do Caos, Punhos da Ruína e o banco existente da moto, restrito a Rock. Permite selecionar skin, forma, direção, animação/golpe ou sequência; reproduzir/pausar; avançar/voltar quadros; mudar velocidade, ampliação, fundo e repetição. A ajuda explica os controles sem exigir conhecimento técnico.

`Resultado atual` aplica as 48 páginas do catálogo novo somente ao contexto e nome de fase correspondentes. `Comparar arte anterior` usa os bancos anteriores. O rótulo de cada quadro distingue arte própria nova, arte própria anterior e arte anterior com filtro. Nos punhos, o filtro vem das capturas reais do shader de **21/10/2026**, não de um tingimento global criado para a página. Os atlas novos são os preparados em 24/10. Transformações anteriores de katana/adagas seguem os sprites e máscaras declarados no catálogo.

O manifesto contém 5460 clips contextualizados, 5527 descritores de quadro e 224 combinações de skin/arma/forma/direção. Esses números incluem reutilização de figuras entre ações e comparações; **não significam 5460 animações inéditas**. As 158 figuras do redesign efetivamente referenciadas pelo catálogo aparecem em seus contextos exatos. Os demais quadros permanecem anteriores.

Combos concatenam as apresentações dos ataques efetivamente definidos em `DarklightCombatProfiles` e `DarklightRuinFistsProfile`, incluindo ramificações com pausa e final pesado. As fases de ataque seguem as janelas base de antecipação/contato/recuperação. Isso não reproduz entradas humanas, colisões, cancelamentos, modificadores de velocidade da campanha ou efeitos procedurais do jogo. Os tempos de inspeção dos movimentos e repousos anteriores não certificam toda a modulação do relógio vivo. A morte exibe a pose selecionada, sem simular sua cutscene/reversão. O banco da moto permanece explicitamente anterior.

Nenhum sprite foi redesenhado, recolorido no arquivo ou montado por partes para essa prévia. A exibição identifica quadros inteiros e respeita os pivôs/PPUs dos assets. A ampliação aumenta a área completa desenhada e permite rolagem, sem cortar o arquivo de origem.

## Verificações executadas

- `Tools/AnimationPreview/check_weapon_preview_manifest.py`: **zero erros e zero avisos**; 501 PNGs incluindo máscaras, referências, tamanhos, retângulos, pivôs, PPUs e durações verificados. Relatório `outputs/previa-armas-20261024/data-qa.json`.
- `Tools/AnimationPreview/review_weapon_viewer_20261024.cjs`: **16 seleções de arma/forma, zero erros JavaScript e zero solicitações com falha** em Edge headless. Verificados seleção, comparação anterior, reprodução, passos, sequências, velocidade, fundo, zoom e restrição da moto a Rock. Capturas completas e resultados em `browser-qa/`.
- Foi corrigida uma corrida da captura contra a pintura assíncrona: a troca de forma agora marca o carregamento e só conclui quando o quadro novo foi desenhado. A captura completa de Presas/Réquiem/Absoluto mostra o cabelo branco correspondente, com os seletores visíveis.
- A captura completa do combo de katana em 1× conserva personagem, botas e lâmina dentro da área. Ampliações maiores oferecem rolagem da imagem inteira.
- A sintaxe do apoio PowerShell foi validada. O atalho foi executado em modo de verificação sem abrir janela, retornando corretamente o endereço do servidor existente.

Essas verificações confirmam funcionamento e vínculos da prévia; não substituem aprovação visual do usuário, fidelidade artística ou conclusão do redesign. A crítica final do usuário foi preservada como pendência urgente para a próxima sessão.
