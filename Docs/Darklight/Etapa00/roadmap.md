# DARKLIGHT: REQUIEM — roadmap de implementação

Cada linha é um plano de etapa. A aprovação da Etapa 00 não autoriza as etapas seguintes. A duração de 10–15 minutos da vertical slice e o alvo de 60 FPS dependem de testes.

Os caminhos novos são propostas relativas à raiz da cópia. `DL/` abaixo representa `Assets/Darklight/`. Nenhum desses módulos foi criado nesta auditoria.

| Etapa | Objetivo e reutilização | Arquivos/arte previstos | Estratégia e critério de conclusão | Testes e risco principal |
|---|---|---|---|---|
| 00 Fundação/auditoria | Cópia da main; conferir sistemas, escala e controle | `AGENTS.md`, `Docs/Darklight/Etapa00/`; nenhuma arte nova | Preservar base e GUIDs, remover remoto, levantar evidências e limitações | Integridade Git, importação e referências; base pode conter problemas prévios |
| 01 Identidade visual | Fábio pelas quatro fotos; usar a skill e consumidores de atlas como referência | `DL/Art/Concepts/`, `DL/Art/Characters/Fabio/`, manifestos; concept normal/com óculos e folha de oito vistas | Comparar com objetos da base; aprovar resolução, proporção, figurino e identidade antes da animação definitiva | Alpha, recortes, direções, alinhamento e leitura em 1×; geração não garante grade exata |
| 02 Combate/entrada | Movimento, câmera, buffer e impacto existentes como base | `DL/Runtime/Character/`, `Combat/`, `Input/`, `Data/Attacks/`, cena de laboratório; espada e ataques | Leve/pesado/carregado, combos, esquiva, parry, habilidade e mira independente; iniciar adaptação dos controles aqui | Timing, um acerto por alvo/golpe, pausas e cancelamentos; ataque herdado é cardeal e mistura relógios |
| 03 Inimigos/estilo | Aproveitar percepção, stagger e knockback existentes | `DL/Runtime/Enemies/`, `Style/`, `Data/Enemies/`; Rastejante, Atirador e Blindado | Estados/telegraphs e coordenação de três tipos; variedade/repetição/dano alteram D→SSS | Ataques justos, transições, morte e cálculo do rank; evitar múltiplos atacantes sem aviso |
| 04 Ruptura | Reutilizar efeitos e eventos; forma é nova | `DL/Runtime/Transformations/`, `Data/Transformations/`; sprites, aura vermelha e despertar | Energia de combate, ativação/duração, novos modificadores e reversão; preparar extensões sem implementar Eclipse/Absoluto | Expiração, morte, troca de cena e retorno dos atributos; evitar buffs acumulados |
| 05 Arena/mapa | Planos, colisões, portais e iluminação existentes | `DL/Scenes/VesperaSlice.unity`, `DL/Art/Tilesets/Vespera/`, prefabs de portas/props | Rua/beco/arena noturna; separar Ground/Walls/Props/Collision/Lighting/Interação/Spawns/Portais | Colisão, caminhos, esquiva, limites da câmera e checkpoints; imagem do mapa não comprova navegação |
| 06 Kael Voss | Usar eventos de cutscene, shake e combate como referência | `DL/Runtime/Enemies/Bosses/`, `Data/Bosses/Kael/`, arena; oito vistas/ataques/telegraphs | Duas fases, introdução, despertar de Ruptura, derrota e recompensa; integrar começo/meio/fim da slice | Fases, gatilhos, derrota/reinício e pulo de cena; priorizar leitura dos ataques |
| 07 RPG | Inventário/vida/save existentes como referência, sem copiar itens antigos | `DL/Runtime/Progression/`, `Data/Skills/`, inventário; ícones | Atributos, XP, níveis e pequena árvore útil ao combate; equipamentos complexos ficam para depois | Compra/desbloqueio, gastos, valores limites e persistência; não sobrecarregar a slice |
| 08 Narrativa/UI/save | Diálogos, retratos, menus, configurações e escrita de save existentes | `DL/Runtime/Narrative/`, `UI/`, `Persistence/`; HUD, retratos, menus, créditos | Menu próprio, novo/continuar, pausa, morte/vitória, missões e checkpoint; save e identidade de produto isolados | Navegação só com controle/teclado, retomada, migração e arquivo inválido; nenhum texto/save de Varginha deve vazar |
| 09 Polimento e porte final do controle | Integrar prompts, remapeamento, navegação, sliders e vibração do checkout local | `DL/Runtime/Input/`, `UI/`, `Audio/`, efeitos e documentos de QA | Completar e validar porte no G808; ajustar iluminação/áudio/efeitos; entregar build Windows da slice quando viável | Hardware físico, desconexão/foco, FPS medido, memória e regressão; teste simulado não comprova funcionamento do aparelho |
| 10 Campanha | Expandir a slice aprovada por capítulo | Novas cenas/dados/arte; Eclipse/Absoluto e chefes posteriores | Planejar seis capítulos e desenvolver cada um em ciclo próprio; validar cada marco antes de avançar | Progressão, narrativa, balanceamento e saves de longo prazo; escopo/custo de arte |
| 11 Site oficial | Usar somente materiais aprovados e progresso real | `Website/Darklight/`; Next.js/TypeScript, SEO, favicon, OG e galeria | Home, história, personagens, gameplay, galeria, trailer, devlog, download, FAQ e desenvolvedor; link ao portfólio fornecido | Responsividade, teclado, movimento reduzido, links e SEO; trailer/demo sem links falsos, domínio público e publicação só após autorização |

## Vertical slice

Fluxo proposto: apresentação de Vespera → encontro de treinamento → rua com três arquétipos de Eco → checkpoint → duelo com Kael → despertar de Ruptura → segunda fase → encerramento e reinício. Distribuir tempo só após playtests; não preencher duração com corredores ou diálogos obrigatórios.

Condição para entregar a slice: personagem aprovado, combate completo, três inimigos, rank, Ruptura, Kael, arena navegável, HUD, áudio/efeitos, menu/pausa, checkpoint/reinício e suporte a controle. A implementação termina nas etapas 02–09; a campanha inteira não entra no primeiro ciclo.

## Site e portfólio

O site pode começar quando houver artes aprovadas e captura real do jogo. Não reutilizar capturas de Varginha como se fossem DARKLIGHT. Preparar apresentação de Fábio sem inventar credenciais, botão para `https://devfabio.vercel.app` e rodapé “Desenvolvido por Fábio — devfabio.vercel.app”. Não modificar o portfólio existente. Sitemap/URLs canônicas dependem do domínio público aprovado; download e trailer só serão habilitados quando existir material real.

## Próxima etapa proposta, ainda não autorizada

Etapa 01: revisar as referências de arte, produzir concept de Fábio normal e com óculos e folha inicial de oito vistas; apresentar comparativo de escala e aprovar 640×360/32 PPU ou a alternativa escolhida. Animações de combate definitivas só depois da aprovação visual. Nenhuma arte dessa etapa foi gerada na fundação.
