# DARKLIGHT: REQUIEM — ACERVO DE ARTES, 11/11/2026

Este arquivo reúne as artes e referências recuperáveis desta conversa, inclusive personagens, transformações, quatro inimigos comuns, três bosses, arena e peças promocionais.

IMPORTANTE: NÃO É UM PATCH PRONTO E NÃO É UMA BUILD. Todas as imagens são fontes/arte conceitual e precisam de verificação de slicing, transparência, alinhamento, rig e estado real no repositório.

PRIORIDADE DE INTEGRAÇÃO:
1. Compare com Git/Assets/ArtSources; não importe nada duplicado.
2. Use 01_Fabius_Formas_10_Folhas como base aprovada; 06_Fabius_Artes_Extras_Variantes são alternativas/referências.
3. Para Lucius, verificar versões finais aprovadas e reutilizar sistema existente; não importar protótipos sem conferência.
4. Os quatro inimigos normais estão em 04_Inimigos_Comuns.
5. Os bosses estão em 05_Bosses; as montagens/candidatos exigem revisão visual, especialmente Matriarca e caminhada/morte de Kael.
6. Conteúdo de 98_RASCUNHOS_NAO_INTEGRAR não deve ir para o runtime.
7. 10_Conceitos_UI_VFX: NÃO redesenhar o HUD; está adiado.

DIREÇÕES: N, NE, E, SE, S, SW, W, NW. Uma folha frontal/lateral NÃO implica 8 direções completas.
Efeitos presentes nas imagens não devem empurrar o pivô do corpo. Evite regeneração/re-fatiação dos 522 pivôs previamente estabilizados de Fabius Normal.

ESTADO DESTE PACOTE: coleção de fontes, SEM commit nem teste Unity nesta sessão; validação de gameplay deve ser feita no Antigravity/PC.

Arquivos adicionais:
- INVENTARIO.csv: caminho, tamanho, SHA-256, tipo, status, observação.
- DUPLICATAS_IDENTICAS.csv: originais com bytes idênticos removidos do pacote para economizar espaço.
- PROMPT_ANTIGRAVITY_IMPLEMENTACAO.md: instruções de integração e handoff.
- PARTES: ZIPs independentes numerados com subset de assets. Cada parte contém inventário e prompt.
