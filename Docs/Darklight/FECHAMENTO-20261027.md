# DARKLIGHT: REQUIEM — concluídos, validação e pendências para 28/10/2026

Documento único de acompanhamento da rodada de 27/10. Atualização inicial: 21h30, Brasília. Os resultados finais e o recibo Git serão acrescentados neste mesmo arquivo. O redesign global continua parcial.

## Direção do usuário aplicada

- Somente Réquiem; Katana, Punhos da Ruína e Moto. Normal, Ruptura e Eclipse Absoluto. Legados preservados e sem seleção ativa.
- As animações aprovadas da Katana são a autoridade principal de identidade, rosto, maturidade, proporções e roupa, inclusive para o piloto da moto.
- Revisar e corrigir autonomamente, sem pedir aprovação humana a cada lote. Preservar quadros corretos e reparar figuras completas afetadas.
- Melhorar contatos de combate com animação pixel art sincronizada ao acerto real, mantendo os controles e as opções de conforto.
- Fazer checkpoint e commit/push por volta de 21h40 e registrar as pendências do dia seguinte. Não fazer merge definitivo na main.

## Concluído e integrado ao código

| Item | Estado e evidência |
|---|---|
| Escopo simplificado | Seletores e migração de saves preservam IDs e histórico, usando arsenal 0/2/3 e três formas ativas. |
| Antigravity | Dez commits preservados na branch isolada por fast-forward até `f26ecf490d1f35c48eb4f19656550fb4ab4c3144`; backup PRIVATE da branch conferido antes da integração. O diff original tem zero Runtime/arte. |
| Correções do apoio técnico | Oito declarações locais C# inválidas corrigidas; fixture de save isolada dentro de work; expectativas adaptadas à migração de Presas e ao escopo atual. |
| Contratos de save e caminhos | **11/11 aprovados na Unity**, compilação limpa. Evidência: `outputs/retomada-20261027/contract-tests.xml`. |
| Ferramentas | Runner rejeita XML ausente/antigo, zero casos, código Unity não zero, esquema e contadores incoerentes. Doze testes das ferramentas aprovados. Validadores receberam contadores/saída de erro corretos e cobertura derivada dos arquivos. |
| Configuração de subagentes | `.codex/config.toml` e três perfis TOML no projeto; até três agentes, modelos por função, contexto curto, permissões herdadas. Configuração global preservada. Foram usados artistas Sol/high e auditor Luna/high. |
| Limpeza segura da rodada anterior | Removidos **19.703.023.347 bytes** de cache da cópia antiga e oito builds superadas; fontes, saves, histórico e build de 25/10 preservados. Recibos em `work/retomada-20261026/cleanup/`. |

O handoff técnico para o Antigravity está em [DIVISAO-TRABALHO-ANTIGRAVITY-20261027.md](DIVISAO-TRABALHO-ANTIGRAVITY-20261027.md). Não foi enviada mensagem externa automaticamente.

## Arte produzida, revisada e selecionada

| Lote | Produzido/revisado | Integração |
|---|---|---|
| Katana Walk SW/W V1 | 12 figuras, seis fases por direção; aprovação humana expressa preservada. | Dois clips adicionados ao catálogo sem regenerar a fonte aprovada. |
| Katana Walk NW/N | 12 figuras, dois clips de 400 ms; nove figuras originais preservadas e três reparadas inteiras. | Adicionadas ao catálogo; validação de caminhada/transição no gate atual. |
| Punhos Chute Quebra-Osso | Mais 12 figuras N/NE/E/SE, completando fontes nos oito lados com as de 26/10. Contato Norte corrigido por figura inteira, 11 quadros preservados. | Quatro clips adicionados; oclusão do chute Norte registrada, sem inventar aprovação humana. |
| Moto base Normal V3 | 12 figuras completas: oito vistas montadas e quatro fases de Slam S, usando Katana como autoridade de maturidade. | Consumidor direcionado a `RequiemMotoForm0Ride8SlamS20261027V3`. |
| Moto idle MatureV1 | **32 figuras, oito direções**, quatro fases por lado. Dez figuras preservadas e dois retornos reparados inteiros; oito GIFs/timings conferidos. | Três fontes MatureV1 selecionadas pelo consumidor. Idles anteriores retidos por fidelidade permanecem fora da seleção atual. |
| Contatos pixel art | 12 efeitos completos, três clips Punch/Katana/Heavy de 165 ms; fonte final sem névoa ampla, clusters revisados em escala junto à Katana. | Banco próprio importável; renderização em pool de 16 instâncias, disparada após acerto real. |

O catálogo principal chegou a **166 clips** após as adições selecionadas. Moto e FX têm bancos separados. Esses números não representam conclusão global nem todos os golpes/direções completos.

Fontes/hashes/PPU/QA/proveniência: `outputs/retomada-20261027/{katana-normal,punhos-normal,moto-maturity,moto-idle-mature,contact-fx}`. A fonte final dos FX tem SHA-256 `594945fddd58227a70cd7684272d14416f71ad101c7eef68844c624a19b1cf6e`. Avisos de paleta/alpha parcial foram documentados; não houve limpeza global de cores ou montagem anatômica.

## Contato cinematográfico implementado

- O resolver confirma colisão, linha livre e dano recebido antes de criar a ênfase; ataque no ar não deve dispará-la.
- A figura completa recebe um pequeno avanço/recuo visual, sem deformação anatômica nem alteração de escala/collider.
- A câmera faz uma ênfase breve e limitada no contato, respeitando redução de efeitos e a preferência de tremor.
- Os efeitos autorais percorrem quatro fases durante o hitstop existente; não adicionam bloqueio de entrada nem dano extra.
- Um teste focado usa o resolver real para distinguir erro/acerto e conferir as quatro fases no consumidor. Sua aprovação depende do gate final abaixo.

## Validação e build

| Etapa | Estado às 21h30 |
|---|---|
| Importação final | Concluída, saída zero. |
| EditMode final | **82/82 aprovados**, sem falhas/ignorados. |
| PlayMode final | Em execução; não presumir resultado antes do XML. |
| Build Windows | Depende dos gates; saída planejada `outputs/retomada-20261027/Windows/`. |
| Executável fora do Editor | Pendente para esta compilação. |
| ZIP | Gerado somente depois da build aprovada; caminho e SHA serão registrados. |

Progresso verificável: `outputs/retomada-20261027/release-progress.json`; logs em `work/retomada-20261027/release/`. Testes automáticos não equivalem a campanha humana completa nem a teste com controle físico. PC04/PC06 permanecem 2/2 aprovados pela evidência de 26/10; não foram reexecutados sem mudança sonora relevante.

## Pendências reais para 28/10

1. Conferir o resultado final da build/ZIP e os fluxos realmente observados no standalone; não repetir gates verdes sem mudança relevante.
2. Completar a caminhada Katana NE/E com alternância coerente dos passos e desenhar SE. As versões NE/E mantidas como candidatas não são clips ativos completos.
3. Completar demais ações Normais: corrida, combos restantes, pesados/carregados/aéreos, defesa/parry, esquiva, dano/morte e transições. Punhos ainda precisa de locomoção e outras técnicas.
4. Moto: ciclos reais de locomoção, montar/desmontar, transições e demais direções/fases das seis técnicas. Vistas montadas estáticas não são ciclos de movimento.
5. Depois do Normal, completar Ruptura e o redesign da aparência fundida Eclipse Absoluto; preservar cabelo canônico e anatomia adulta. Fusão de poderes implementada não significa todas as artes prontas.
6. Concluir a auditoria sem merge cego das 220 diferenças das branches antigas; Antigravity pode analisar no clone independente e trazer recomendações/hashes. Não restaurar Rock/Presas ou artes superadas.
7. Validar campanha humana completa, áudio subjetivo/transições musicais e controle físico quando disponível; separar esses estados dos testes automáticos.
8. Após o redesign: Combat AI 2.0, Sangue do Abismo com Lucius Normal/Nexo Fenrir e O Encapuzado, depois site/material promocional. Minecraft permanece pausado.

## Git e continuidade

Publicar somente pela cópia `work/publicacao-20261021`, branch `integracao-completa-20261026`, remoto `fwbiodev7/darklight-requiem-game`. O `.git` da raiz é legado de outro projeto e não deve ser publicado. Backup privado validado preserva as referências anteriores e a branch Antigravity. Builds, saves, caches, MP3 originais, referências privadas e versões rejeitadas ficam fora da allowlist.

O recibo de commit/push, o resultado do standalone e as atualizações posteriores ao checkpoint serão registrados abaixo. Até existirem evidências, esses estados continuam pendentes.

## Atualizações posteriores ao checkpoint

- 21h33: primeira execução PlayMode final terminou **20/21**. A única falha estava no novo teste de caminhada, que exigia o loop antes das transições legítimas StartMove/Brake. A medição foi corrigida para aguardar essas transições com limite de tempo; nenhuma transição/artes aprovada foi removida. Gates frescos em repetição após essa mudança de teste; XML anterior preservado.
- Configurações de subagentes efetivamente usadas nesta rodada: dois artistas `gpt-6.1-sol/high` com contexto novo e auditor `gpt-6-luna/high`, conforme os perfis locais. Nenhuma mudança de permissões ou configuração global.

- 21h37: gates frescos aprovados: **82/82 EditMode e 21/21 PlayMode**, sem falhas/ignorados. Incluem caminhada com transições, contatos reais com quatro fases de FX e idles maduros nos oito lados. Build Windows em execução; standalone ainda não iniciado.
