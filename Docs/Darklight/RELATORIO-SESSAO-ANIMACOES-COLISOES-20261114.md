# DARKLIGHT: REQUIEM — sessão de 14/11/2026

## Entrega

- Unity 6000.6.0f1, Windows x64.
- Build final: `outputs/build-windows-gothic-20261114-feedback/Windows/DarklightRequiem.exe`.
- Estado da entrega: **pasta empacotada para teste manual**, com os executáveis gerados pela Unity e os 16 arquivos de dados atuais copiados integralmente. A cópia nativa da Unity falhou por espaço em disco; o empacotamento foi reparado manualmente. O recibo original `build-result.json` com falha foi preservado, sem declarar um resultado Unity de zero erros. O recibo da distribuição reparada é `distribution-result.json`, resultado `Packaged`. As saídas `-consolidated`, `-final` e `-manual` desta rodada não são entregas válidas.
- **Sem testes automáticos EditMode/PlayMode e sem execução do jogo nesta rodada.** O usuário assumiu a validação manual. Não há resultado novo de 28/28 testes.
- **Sem commit e sem push**, conforme última orientação do usuário.

## Artes produzidas, revisadas e importadas

| Forma | Arma | Caminhada | Quadros |
|---|---|---|---:|
| Ruptura | Katana | oito direções | 24 |
| Ruptura | Punhos / Grevas Abissais | oito direções | 24 |
| Ruptura | Serras mecânicas circulares | oito direções | 24 |
| Serafim Carmesim | Katana | oito direções | 24 |
| Serafim Carmesim | Punhos / Grevas Abissais | oito direções | 24 |
| Serafim Carmesim | Serras mecânicas circulares | oito direções | 24 |

Também foi preparado `Session14SerafimFistsFront`: 12 quadros de antecipação, impacto e recuperação, cobrindo S, SE, E e NE. Os ataques existentes das demais armas foram preservados.

**Total importado real: 28 páginas, 156 quadros**, sendo 144 de caminhada e 12 de ataque. Recibo da Unity: `work/session20261114/walk-import-2026-11-14T232237-888917+0000/receipt.json`; exit code 0. O log contém as 28 mensagens de fatiamento, somando 156 sprites. Isso comprova importação, não teste de gameplay.

- A caminhada aprovada de Ruptura/Katana tornou-se referência de identidade e aura para Ruptura. Mantidas as mechas vermelhas destacadas e o cabelo compacto.
- Usadas as referências de Serafim fornecidas durante a sessão; a versão final foi aprovada após correções de idade, proporções e raízes das asas.
- Substituídos os quadros completos com terceiro braço, asa ausente/desconectada e proporções rejeitadas; quadros corretos foram preservados.
- Serras seguem a imagem enviada pelo usuário: dois discos vermelhos de dentes curvos, núcleo circular e estrutura mecânica metálica, um em cada mão.
- Alpha real preservado. Recortes de figuras inteiras, sem montagem anatômica, deformação ou redimensionamento dos pixels de entrega. Nas duas folhas cujos contornos se intercalavam, reutilizado o identificador de componentes inteiros existente, mantendo todos os pixels com alpha e acrescentando margem transparente.
- Pivôs e PPU registrados nos descritores; o PPU compatibiliza a escala de importação, sem ser usado para corrigir anatomia. Catálogo e importação usam `DarklightFabiusFormsImporter`, com consumo por `DarklightFabiusFormsBank` e `DarklightCharacterView`.
- Criadas as páginas ausentes de Punhos e Serras de Ruptura, incluindo Lateral/DiagS/DiagN com os identificadores históricos. A caminhada de Punhos de Serafim agora tem cobertura própria nas oito direções, evitando o fallback de Katana.
- As quatro candidatas originais não foram ativadas. Avaliação individual em `ArtSources/Candidates20261114/candidates-manifest.json`. Tentativas rejeitadas desta sessão foram excluídas da seleção de entrega.
- A conferência técnica de arquivos anterior à suspensão dos testes registrou **698 arquivos de Fabius Normal, zero diferenças**. Nenhum desses assets foi modificado pelas alterações desta rodada.

Prompts finais registrados em `ArtSources/WalkRepair20261114/generation-prompts.json`; geração pelo `image_gen` integrado.

Fontes e hashes: `ArtSources/WalkRepair20261114/manifest.json`. Mapa exato de direção/quadro: `integration-map.json`. Recibo: `integration-receipt.json`. Prévia e contatos: subpasta `review/`. Não houve agentes paralelos.

## Colisões: mecanismos corrigidos no código compartilhado

- Corrigido o sinal da separação de sobreposições: o deslocamento anterior podia aprofundar a penetração em paredes. A correção reconsulta os contatos e trata primeiro o mais profundo, com limite de quatro iterações.
- Corrigido o bloqueio da direção de saída quando existe sobreposição; mantido o bloqueio ao avançar contra a superfície.
- Passos rápidos, puxões e dash usam o colisor real e seu offset, com segunda varredura após o primeiro avanço, em vez de um círculo incompatível em torno da raiz do personagem.
- Colisões estáticas dos mapas unificadas em `CompositeCollider2D`, eliminando arestas internas entre blocos adjacentes. Material sem atrito e pequena suavização das quinas.
- Sincronizados os transforms da geometria e das consultas de spawn, evitando consultas à posição anterior dos colisores recém-criados.
- O gerador compartilhado atende às dez regiões e à arena de treinamento; o fallback antigo também recebe material sem atrito. Geometria de navegação e desenho dos mapas foram preservados.

**Limite de validação:** as causas identificadas foram corrigidas no código. Não foi percorrido cada canto de cada mapa nesta sessão; não se certifica ausência de todos os bugs antes do teste manual.

## Névoa sombria nas bordas

Estendido `DarklightAtmosphereController`, reutilizando sprite suave, material e opções existentes. São 12 elementos sem colisor, três por lado, posicionados nos limites do mapa em coordenadas de mundo. A parte mais densa fica fora do cenário; a franja interna suaviza a transição para o vazio. Movimento lento, congelado na pausa e reduzido com as opções de efeitos. Não foram adicionadas músicas, pipeline gráfica ou texturas novas para isso; o orçamento de 88 partículas de chuva foi preservado.

## Compilação e espaço em disco

1. A primeira tentativa de build somente das artes foi interrompida pelo limite de 180 segundos do executor; nenhuma saída foi declarada pronta.
2. A tentativa seguinte, já com colisões e névoa, registrou `Succeeded` com **2 erros**. O log contém `Failed to write compressed chunk to the archive 'Library/PlayerDataCache/Win642/Data/data.unity3d'! Error: 14`; essa saída é incompleta e não foi aceita como entrega.
3. O usuário autorizou excluir `outputs/build-windows-test-20261113/` e `outputs/build-windows-20261111/`. A exclusão automática foi rejeitada com **blocked by policy**. O usuário removeu as pastas e esvaziou a lixeira manualmente; espaço livre observado depois: aproximadamente 12,6 GiB.
4. A tentativa com `--clean-player-cache` também registrou `Succeeded` com dois erros internos do alocador da Unity (`Failed to free block ... size delta was 44`). O arquivo de dados tinha cabeçalho zerado e havia um `.tmp` de aproximadamente 2,1 GB; essa saída foi rejeitada como incompleta.
5. Removidos apenas arquivos de dados inválidos/temporários produzidos pelas tentativas desta sessão, preservando recibos e logs. Iniciada build padrão Windows sem o gravador LZ4, com `--clean-player-cache --uncompressed-player`, em pasta separada `build-windows-gothic-20261114-manual`. Esse modo usa os arquivos de dados normais do player, sem mudar gameplay ou importações das artes.

Build funcional anterior preservada: `outputs/build-windows-gothic-20261114-serafim-saws/Windows/`. A build de estabilidade de 10/11 e os saves também foram preservados.

## Fechamento do empacotamento

A tentativa sem LZ4 gerou os dados completos no cache, mas a etapa nativa de cópia de `resources.assets.resS` falhou com `Copying the file failed: Espaço insuficiente no disco.`. As tentativas seguintes reutilizaram o cache. A tentativa de link físico evitou duplicação, mas o copiador recusou origem/destino com a mesma identidade; o link foi removido.

A distribuição foi completada por gravação sequencial para **arquivos independentes**, sem vínculo de hardlink com o cache: **16 arquivos, 7.898.603.111 bytes**, 98,9 segundos, cópia concluída com exit code 0. SHA-256 e tamanho de cada arquivo foram registrados em `Windows/distribution-result.json`. Os executáveis e bibliotecas são os que a Unity produziu nesta rodada. Foram conferidos apenas os arquivos necessários ao empacotamento; o jogo não foi aberto. Não se apresenta a falha original do BuildPipeline como sucesso.

Aplicada compressão transparente NTFS para reduzir o espaço físico sem alterar conteúdo. O recurso maior tem 7.427.279.216 bytes lógicos, armazenados em 2.626.154.496 bytes. Depois de confirmar a cópia independente, removido apenas esse grande arquivo do cache intermediário de player, regenerável; importações, fontes, assets e saves foram preservados. Espaço livre observado no fechamento: aproximadamente **2,8 GiB**.

Os atalhos `run-game.bat` e `run-game.ps1` apontam agora para a pasta `-feedback`. Não foi gerado ZIP adicional. Manter a pasta Windows inteira, incluindo `DarklightRequiem_Data`, `UnityPlayer.dll` e `MonoBleedingEdge`.

## Validação manual pendente

- Trocar Katana/Punhos/Serras e caminhar nas oito direções em Ruptura e Serafim; observar mãos, asas, mechas, escala e apoio dos pés.
- Conferir ataques frontais de Punhos de Serafim e retorno à caminhada.
- Encostar e deslizar em paredes, cantos e passagens estreitas; testar dash/puxões junto aos obstáculos, spawn e troca de região.
- Conferir névoa nas quatro bordas, entrada/saída de regiões, pausa e opções de efeitos.

## Git e cota

Nenhum commit/push foi realizado. Não incluir outputs, work, fontes `_partial.png` ou candidatas reprovadas no commit futuro. Antes da orientação de deixar o commit para o usuário, foi preparada uma worktree sem checkout em `../work/delivery-final-20261114`, branch `codex/finalizacao-animacoes-colisoes-20261114`; as alterações de entrega não foram copiadas nem staged nela. A raiz permanece em `main`, com as alterações locais preservadas.

Última consulta de cota durante o fechamento: **8% semanal restante**. O usuário autorizou continuar e pediu entrega jogável antes dos 5% restantes. Não há medição de consumo por arquivo ou por imagem.

## Referências consultadas

- [Pedro Medeiros — Wings / Flying](https://www.patreon.com/saint11/posts/wings-flying-8869136): leitura da silhueta e das formas principais na animação pixel art.
- [Monika Zagrobelna — How to Draw Wings](https://monikazagrobelna.com/2019/08/23/sketchbook-original-how-to-draw-wings/): estrutura articulada, raízes e membranas. O design fornecido pelo usuário permaneceu a autoridade visual.
- [Unity — ColliderDistance2D](https://docs.unity.cn/6000.6/Documentation/ScriptReference/ColliderDistance2D.html): distância assinada e separação dos colisores.


## Atualiza��o ap�s o teste manual � 14/11/2026

O usu�rio testou a distribui��o, relatou jogo �timo e divertido, morreu tr�s vezes e informou que as colis�es est�o arrumadas e o gr�fico est� bom. Autorizou em seguida o commit e envio ao GitHub, substituindo a decis�o anterior de deixar o commit para depois. Dire��o futura salva em `DIRECAO-E-FEEDBACK-20261114.md`; nenhuma dessas melhorias futuras foi iniciada neste fechamento.
