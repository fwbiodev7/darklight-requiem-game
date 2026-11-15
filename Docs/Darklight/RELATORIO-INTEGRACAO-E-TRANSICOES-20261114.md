# Integração e transições — 14/11/2026

Estado: fontes recuperadas e integração seletiva executada. Transições novas em revisão visual; **trabalho global, Gothic Eclipse/UI 2.0 e build final ainda não concluídos**. O usuário exige ver as transições antes da aprovação. Lucius e Serafim tiveram o desenho aprovado durante esta conversa; Ruptura foi novamente direcionada à folha `Planilha de Sprites_ Ascensão de Ruptura.png` e precisa de nova prévia.

## Fontes recebidas e preservação

- O ZIP `DARKLIGHT-28-ARTES-ORIGINAIS.zip.zip` continha 27 PNGs. O anexo posterior `Fabius Carmesim_ folha de sprites de combate.png` completou **28/28**, total de **54.968.253 bytes** de fontes.
- Conferidos nome (incluindo codificação do ZIP e substituição de `:` por `_` no Windows), tamanho do inventário, CRC do ZIP, assinatura PNG, Pillow `verify`, decodificação integral e dimensões/alpha. **Zero fontes superadas extraídas**. SHA-256 registrados como baseline dos arquivos recebidos; o inventário histórico não continha hashes originais para comparação criptográfica anterior.
- Originais preservados em `ArtSources/Session20261114/raw/libfile_*.png`; recibo `zip-recovery-validation.json`. Não alterado o inventário histórico para apagar o estado anterior de transferência.
- Fabius Normal: baseline de **698 arquivos**, zero alterações de bytes após importação/testes. Não alterados artes, pivôs ou GUIDs calibrados.

## Integração seletiva executada

- Fabius: **11 folhas / 132 quadros inteiros** preparados e importados, com âncoras no corpo/pés, direções visualmente mapeadas e ataques de três fases sincronizados ao startup/contato/recuperação do modelo de combate.
- Lucius: **15 folhas / 176 quadros ativos / 45 clipes** preparados e importados. Contextos únicos substituídos seletivamente; Idle existente e ataques Normal preservados. Os clipes novos foram resolvidos no Unity sem fallback para Idle pelos testes específicos.
- Total: **26 fontes ativas**. As 28 fontes permanecem preservadas. Não se confundiu quantidade de arquivos recebidos com quantidade de fontes seguras para ativar.
- Não ativadas: `Ficha de ataques do guerreiro alado carmesim.png` (duplica poses traseiras com equipamento divergente da folha corrigida recebida) e `Lucius em caminhada, espada nas costas.png` (repete E/W). Excluídos também três quadros com VFX sobreposto do ataque frontal Arconte e um quadro NW que muda a orientação. Recibos detalhados: `fabius-integration.json` e `lucius-integration.json`.
- Alteração delicada registrada pelo integrador Lucius: recuperação de contexto NE legado após substituição inicial, mantendo PPU local calibrado `220.214094`, em vez de restaurar indiscriminadamente o catálogo do HEAD. Conferir `baselineRecovery` no recibo.
- Importador novo `DarklightSession14Delivery.ImportSources` usa importação seletiva, preserva IDs existentes e não chama o importador de Fabius Normal. Fabius usa retângulos Unity bottom-left; Lucius usa o contrato top-left do `ImportStandalone`.
- Auditoria estrutural final: **180 páginas** (38 Forms, 70 Lucius Normal, 72 Arconte), **26 imagens novas referenciadas**, zero erros/avisos estruturais. Não certifica aprovação estética global nem desempenho.

## Testes e execução real

| Execução | Resultado | Evidência |
|---|---|---|
| Primeira importação com guard legado | Interrompida por reserva de RAM; incompleta | `work/session20261114/import-2026-11-14T125542-215699+0000` |
| Importação após autorização expressa de memória | Exit 0, concluída | `work/session20261114/import-2026-11-14T125853-189811+0000/receipt.json` |
| EditMode amplo inicial | 58 passaram, 9 falharam | `work/session20261114/EditMode-2026-11-14T130006-836062+0000/tests.xml` |
| EditMode focado após correções | **19/19 passaram** | `work/session20261114/EditMode-2026-11-14T130733-450325+0000/tests.xml` |
| PlayMode sem dispositivo gráfico | Crash `3221225477`, sem XML; não é teste aprovado | `work/session20261114/PlayMode-2026-11-14T130903-139836+0000` |
| PlayMode com D3D11 | **8/8 passaram** | `work/session20261114/PlayMode-2026-11-14T131016-476643+0000/tests.xml` |
| Validação/captura final do protótipo | **9/9 passaram**, sem erros de script/assert no log final | `outputs/session20261114/transitions/capture-results.xml` |

As nove falhas iniciais não foram apagadas: duas eram verificações de lifecycle de MonoBehaviour executadas inadequadamente fora de Play. Foram corrigidas e complementadas com testes PlayMode reais de desativação/limpeza. As **sete falhas restantes** cobram folhas inexistentes de caminhada Ruptura/Punhos e Serras (`FabiusRupturaFistsWalkDiagS20261113`, `...Lateral...`, `...DiagN...`, `FabiusRupturaSawsWalkLateral20261113`). Não foram reescritas como sucesso nem supridas por duplicação de sprites. A suíte ampla ainda não está totalmente verde.

Corrigidos no protótipo: Serafim/Arconte com levitação apenas no renderizador, sem mover root/colisor/escala; Ruptura curta; aura verde Arconte; saída limpa ao desabilitar/morrer; referência Unity destruída no encerramento; efeitos atrás do personagem para preservar leitura. Durações candidatas: Ruptura 1,25 s, Serafim 4,6 s, Arconte 4,2 s. Testes de custo, pausa, duração, cancelamento e limpeza passaram. Esses resultados são do **protótipo com artes preexistentes**, não certificam as novas folhas desenhadas depois.

Execuções silenciosas. As capturas finais usam armazenamento de progresso isolado em `work`; também foi acrescentado isolamento explícito à fixture de transformação. Saves reais observados conservaram timestamps de 13/11. Não alegado teste de áudio audível, controle físico ou desempenho/FPS medido.

## Transições artísticas novas — decisões do usuário

- Referências novas preservadas em `ArtSources/TransformationTransitions20261114/references`; referências complementares e prompts também nas pastas de cada candidata. Ferramenta integrada `image_gen` usada, sem fallback CLI/API.
- **Lucius:** fonte `candidates/lucius/LuciusNormalToArconte-v3-approved-design-source.png`; usuário: “boa com lucius pode manter essa curti”. Aura verde escura de chamas finas/partículas conforme ficha, sem o casulo exagerado da versão intermediária. Desenho congelado. Oito poses completas; GIF e HTML em `previews/lucius`. A prévia usa deslocamento de apresentação explícito; não é gravação dessas artes no Unity.
- **Serafim:** usuário: “serafim tambem curti pode manter essa versão”. Fonte selecionada `candidates/serafim/serafim-transition-candidate-approved-design.png`; estados transformados da v2 congelados; célula Normal inteira da v1 preservada para evitar aura indevida inicial. A edição v3 em voo foi preservada separada e não eleita. Oito poses, 4,6 s, GIF/HTML e recibo final em `previews/serafim` e `candidates/serafim`.
- **Ruptura:** v5 deixou de ser candidata ativa após nova direção. Referência prioritária agora é o anexo `Planilha de Sprites_ Ascensão de Ruptura.png`, com oito figuras completas, mais `Herói Vampírico em Chamas Carmesins.png` como aparência final. Reutilizar figuras inteiras da nova folha para fidelidade literal; não montar anatomia nem reinterpretar como simples Normal com aura. Preservar versões anteriores somente como histórico, excluídas da futura entrega Git.
- As novas folhas de transição **ainda não foram copiadas para Assets nem importadas**. PPU/tempo/apoio e continuidade com o Normal real devem ser calibrados na integração, após a aprovação necessária. Não substituir Normal por figuras geradas, não tingir a pele/íris por código e não somar aura duplicada excessiva às auras já desenhadas.

## Pendências e sequência

1. Mostrar a nova prévia fiel de Ruptura; obter sua aprovação conforme pedido explícito. Desenhos Lucius/Serafim já aprovados, sem pedir aprovação desses desenhos novamente.
2. Integrar as três transições por fontes/hashes e quadros inteiros, com continuidade ao Normal, hover intencional, escala constante e restauração ao gameplay. Capturar novamente o consumidor real com essas artes; os GIFs artísticos não substituem essa prova.
3. Resolver caminhadas transformadas ausentes e as sete falhas relacionadas sem adulterar expectativas de testes. Normal Lucius NE/NW conserva cobertura anterior, sem fontes novas desta sessão. Confirmar lacunas reais de saque/esquiva/Idle e direções antes de declarar Lucius globalmente concluído.
4. Após checkpoint da integração das transições/Lucius, executar Gothic Eclipse e UI 2.0 conforme plano de 14/11. Reutilizar controlador/URP2D/88 partículas, arena e região representativa, mapa funcional, dados reais do Arsenal/Diário e preferências. Os quatro anexos específicos de 14/11 anterior ainda não têm caminhos locais identificados; três referências históricas atmosféricas existem. Não alegar aprovação visual com base só no texto do plano.
5. Executar regressões EditMode/PlayMode necessárias, validação visual e build Windows consolidada somente após a validação e aprovação da etapa. **Nenhuma build nova gerada nesta rodada.**
6. Revisar o conjunto pertinente antes de qualquer staging/commit. Não usar `git add .`. Não incluir fontes rejeitadas/superadas, logs privados, saves, caches, cenas temporárias ou builds. Não realizar escrita manual em `.git` nem contornar políticas. Se Git normal bloquear, deixar entrega e checkpoint para Antigravity.

Git nesta rodada: `status --short` e leituras normais de diff/show executados; alterações alheias preservadas. Nenhum staging/commit/push tentado enquanto as transições novas aguardam aprovação e o escopo final permanece incompleto. Não houve teste manual adicional de escrita em `.git`.
