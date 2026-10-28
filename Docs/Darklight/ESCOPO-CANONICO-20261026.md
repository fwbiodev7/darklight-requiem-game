# Escopo canônico — 26/10/2026

Este contrato substitui os escopos de 23–25/10 e as referências históricas abaixo dele. Origem: os dois prompts anexados pelo usuário em 26/10, “RETOMADA 26/10/2026” e “Backup privado e unificação de todas as branches”. Definir o contrato não equivale a implementar ou validar suas partes.

## Personagem e equipamentos ativos

| Item | Escopo vigente | Preservação do legado |
|---|---|---|
| Fabius | Somente skin Réquiem | Rock não aparece como opção jogável; seus arquivos permanecem preservados |
| Arsenal | Katana, Punhos da Ruína e moto disponível para Réquiem | Presas do Caos sai da seleção ativa; nenhum arquivo excluído |
| Formas | Normal, Ruptura, Eclipse Absoluto | Eclipse e Absoluto deixam de existir separadamente; seus materiais permanecem candidatos à fusão |
| Lucius | Somente Normal e Nexo Fenrir | Ascensão, Fenrir e Nexo separadas deixam de ser transformações ativas |
| O Encapuzado | Manto azul-marinho quase preto, capuz grande, rosto oculto e ponta azul do sobretudo | Identidade revelada somente em Sangue do Abismo |

A frase genérica do prompt de backup sobre Lucius ter “normal e duas transformações” conflita com a especificação explícita “somente duas formas” da retomada. Prevalece a definição detalhada: **Normal e Nexo Fenrir**. Não antecipar a implementação de Lucius/O Encapuzado enquanto o redesign de Fabius estiver incompleto.

Normal: cabelo castanho natural, sem traços de transformação. Ruptura: mechas vermelhas intensas e poderes próprios. Eclipse Absoluto: energia violeta com branco-prateado, combinando elementos das duas formas anteriores; preservar os poderes úteis e remover somente redundâncias. Reutilizar um ID ou aplicar efeitos não certifica a aparência fundida como concluída.

Lucius Normal mantém o design gótico selecionado, cabelo prateado inspirado em Mirai Trunks, rosto inspirado em Ville Valo, olhos azuis, roupa preto/azul e espada grande nas costas. Nexo Fenrir combina verde-claro e violeta. Manter altura, anatomia adulta, proporções e semelhança facial com Fabius, preservando sua identidade individual.

## Contrato visual e técnico

- Oito direções reais: S, SW, W, NW, N, NE, E, SE. Não completar vistas com espelhamento de assimetrias ou rotação plana.
- Personagem, cabeça, corpo e equipamento permanecem juntos em quadros inteiros. Preservar pixel a pixel os quadros corretos; reparar somente figuras completas afetadas.
- Preservar corte canônico, volume e comprimento do cabelo, idade, proporções e escala. Mechas intensas não autorizam cabelo maior.
- Manter olhos vermelho-escuros de Fabius em todas as formas, conforme a referência principal de 20/10; não tingir pele/olhos pela transformação. Base de pele aprovada [246,174,130], com luz e sombra preservadas.
- Manter `WorldVisualScale = 1.325`, PPU calibrado pelo corpo, pivôs, timings, eventos, hitboxes e controles. Não normalizar pela arma ou pelo efeito erguido.
- Reutilizar Réquiem Normal/Katana/Punhos antes de gerar novas figuras. Moto mecânica liberada para Réquiem não transforma automaticamente as fontes antigas do piloto Rock em arte Réquiem aprovada.
- Uma sequência deve usar um conjunto visual coerente. Substituir progressivamente somente ações com cobertura integral das fases usadas; clips parciais não autorizam misturar estilos no mesmo golpe.
- Fontes de Eclipse/Absoluto antigas são candidatas para a fusão; não contar como nova arte de Eclipse Absoluto sem revisão completa.

O arquivo de legado é lógico, com manifesto em `outputs/retomada-20261026/legacy-manifest.json`: manter os assets nos caminhos atuais preserva GUIDs e rastreabilidade. Remoção da seleção ativa não exige apagar ou mover arquivos. Não reintroduzir materiais rejeitados ou obsoletos nas seleções.

## Cobertura e ordem

A auditoria reproduzível é `python Tools/AnimationPreview/audit_reduced_scope_20261026.py`. Sua saída `outputs/retomada-20261026/coverage.json` separa fontes aproveitáveis, candidatas à fusão, legado, matriz principal e banco da moto. Ela valida estrutura/hashes, mas não executa Unity, não certifica anatomia nem aprovação humana.

Ordem: **Réquiem Normal → Ruptura → Eclipse Absoluto → Combat AI 2.0 → Sangue do Abismo/Lucius/O Encapuzado → site/material promocional**. Minecraft continua pausado. A build estável tem prioridade sobre arte defeituosa ou recursos instáveis.

## Entrega e preservação

Gerar build Windows em pasta nova, preservar a anterior, conferir executável/dados, executá-la fora do Editor com armazenamento de teste isolado, testar inicialização/partida/controles/combate/moto/transições e preparar ZIP. Registrar compilação, testes automatizados, execução standalone e campanha humana como estados diferentes. Corrigir bloqueios e registrar limitações reais, incluindo PC04 e PC06.

Antes de qualquer merge, criar e validar `darklight-requiem-backup` como **PRIVATE**, com histórico, branches, tags e LFS quando houver, sem credenciais ou materiais privados proibidos. Falha nessa etapa interrompe a integração de branches. Branch de integração solicitada: `integracao-completa-20261026`. Não excluir branches, não usar operações destrutivas e não fazer merge na main sem nova autorização. Publicar somente escopo permitido; excluir builds, saves, caches, MP3 originais, referências privadas e fontes rejeitadas.

Perto de 15% restantes, interromper novas gerações e reservar recursos para QA, build, checkpoint e Git. Atualizar a automação existente somente quando necessário; não criar duplicatas. A conclusão global só existe quando as pendências reais forem resolvidas e verificadas.
