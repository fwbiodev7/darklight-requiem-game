# DARKLIGHT: REQUIEM — Gothic Eclipse + UI 2.0

Direção expressa do usuário recebida em 14/11/2026 (America/Sao_Paulo). Pendência para próxima sessão; NÃO interromper Lucius. Modelo solicitado: GPT-6.1 Sol, raciocínio Médio. Não alegar troca de modelo não verificada.

## Condição de início e referências

Começar somente DEPOIS de finalizar e registrar o checkpoint da integração de Lucius. Concluir artes não comprova integração. Este documento registra trabalho futuro, não implementação atual nem agendamento automático.

Quatro imagens enviadas na conversa, por função (a ordem dos anexos difere da numeração textual):
1. Gameplay/duelo sob chuva: direção visual atmosférica.
2. Menu principal com eclipse vermelho: direção visual do menu.
3. Composição Mapa/Arsenal/Configurações: menus internos.
4. Jornal gótico de Véspera: missões.

As referências são objetivos estéticos; não copiar literalmente layouts/artes/textos ou tratar dados ilustrados como dados reais do jogo. Preservar as referências originais da conversa para comparação, sem inventar IDs ou declarar que imagens foram importadas.

## A — Base Gothic Eclipse

- Inspecionar apenas o pipeline necessário e reutilizar shaders, materiais, sprites, partículas e luzes existentes.
- Iluminação 2D fria/quente com contraste e sombras coerentes; lanternas/tochas com luz dinâmica leve.
- Névoa volumétrica simulada por sprites/shaders leves; chuva animada em camadas com intensidade variável.
- Reflexos estilizados no piso molhado, sem reflexos reais caros; feixes de luz, parallax discreto e profundidade.
- Partículas de folhas, poeira, brasas e pequenas gotas; color grading e bloom moderados.
- Atmosfera gótica consistente em campanha, treinamento e bossfights.
- Primeira aplicação na arena de treinamento E numa área representativa da campanha; conferir iluminação/chuva/névoa/reflexos, legibilidade e desempenho antes de espalhar.

Regras obrigatórias:
- Preservar estrutura/jogabilidade dos mapas; NÃO reconstruir os dez mapas.
- Não alterar proporções de Fabius/Lucius, câmera Pixel Perfect ou escala aprovada; pixel art nítida.
- Não esconder leitura dos ataques nem modificar colisões funcionais para efeitos visuais.
- Evitar shaders complexos, alocações por frame e excesso de luzes; criar níveis configuráveis de qualidade/intensidade.

## B — Aplicação visual

Aplicar estrutura validada apenas em cenários compatíveis, ajustando câmera, qualidade e renderização. Evitar mudanças globais arriscadas e preservar os sistemas existentes.

## C — UI 2.0

Componentes reutilizáveis: botões, abas, molduras, seleção, painéis, ícones e transições. Pixelify Sans, pixel art consistente, legibilidade em 1080p e adaptação a outras resoluções. Teclado/mouse/controle em todos os menus; não alegar teste com controle físico sem realizá-lo.

### Menu principal
- Eclipse vermelho central no fundo, arquitetura gótica, névoa, profundidade e pequenos efeitos ambientais.
- Fabius com design atual à direita, logo oficial DARKLIGHT: REQUIEM.
- Botões com contornos vermelhos, brilho discreto, animações suaves e destaque de seleção; versão limpa e profissional.
- Opções funcionais: Continuar; Novo Jogo; Carregar Jogo; Arena de Treinamento; Configurações; Trocar Skin; Créditos; Sair.
- Preservar comportamentos/ligações; não criar opções decorativas sem função.

### Mapa
Atlas estilizado baseado no mapa REAL da campanha, com zoom, centralização, navegação, objetivos/checkpoints existentes, localização do player, ícones/legenda e painéis consistentes. NÃO substituir mapa funcional por imagem estática.

### Arsenal
Categorias: Armas; Técnicas; Formas; Skins.
- Armas de Fabius: Katana — Yamato; Punhos/Grevas — Beowulf; Moto — Cavalgada Infernal.
- Formas de Fabius: Normal; Ruptura; Serafim Carmesim.
- Lucius: somente Normal e Arconte nesta etapa.
- Retirar opções antigas do escopo ativo sem apagar recursos legados desnecessariamente.
- Descrições, estatísticas, requisitos e detalhes vêm dos dados REAIS do jogo.

### Configurações / Sistema
Geral; Áudio; Vídeo; Controles; Acessibilidade; Extras SOMENTE se houver funções reais. Incluir qualidade e intensidade dos efeitos Gothic Eclipse, preservando ajustes/preferências existentes.

### Missões
Diário/Jornal gótico de Véspera: principais, secundárias, concluídas, capítulos/objetivos, progresso real, recompensas e detalhes narrativos existentes. Usar GDD/sistema real sem inventar capítulos nem alterar história.

NÃO reintroduzir Artefatos ou Coleções, mesmo que apareçam na imagem ilustrativa.

### Pause e consistência
Atualizar pausa com mesma identidade/componentes. Menus internos SEM sprite do jogador decorativo; exceção apenas do menu principal, com Fabius à direita.

## D — Verificação e build

- Testar navegação/botões/ligações e ausência de textos cortados.
- Conferir resoluções, escala, qualidade e cenas com efeitos ativos.
- Testes direcionados; build Windows consolidada após implementação, NÃO após cada mudança.
- Respeitar suspensão de áudio existente, sem reprodução automática.
- Aprovação visual requer evidência de funcionamento no jogo; conceito gerado não basta.

## Economia e preservação

Trabalho sequencial, sem subagentes por padrão. Não repetir auditorias gerais; consultar só arquivos necessários, reutilizar recursos, evitar refatorações fora de escopo e novos sistemas de história/combate. Preservar alterações não commitadas e evitar operações destrutivas no Git. Preservar cota semanal; perto do limite concluir checkpoint e não abrir fase extensa. Usuário estabeleceu parada aos 10% restantes; não inventar saldo/reset se consulta falhar.

## Entrega por etapa

Checkpoint compacto contendo: implementado; arquivos modificados; comparação visual com referências quando houver captura; testes executados; impacto no desempenho SOMENTE quando mensurado; pendências; próximo passo.

Objetivo: Gothic Eclipse consistente com efeitos bonitos/otimizados e menus modernos FUNCIONAIS, preservando pixel art e jogabilidade. Prioridade é qualidade real em execução.
