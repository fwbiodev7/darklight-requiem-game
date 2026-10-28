# Pendência — músicas das bossfights — 23/10/2026

**Estado: não iniciado; adiado por pedido explícito do usuário.**

A instrução atual é deixar este trabalho nas pendências. As ordens de implementar imediatamente contidas no texto fornecido descrevem a execução futura; não autorizam iniciá-la nesta rodada. Não importar, reproduzir, substituir trilhas ou gerar outra build agora.

## Ordem obrigatória

| Bossfight principal na progressão real | Música |
|---|---|
| Primeira | Going Under — Evanescence |
| Segunda | Afterlife — Evanescence |
| Terceira e última | See U in Hell — Papa Roach & Hanumankind |

Identificar os chefes correspondentes pela progressão efetiva da campanha quando a tarefa for retomada. Não presumir nomes nem confundir encontros opcionais com as três bossfights principais.

## Arquivos fornecidos

- `C:\Users\Usuario\Downloads\Evanescence_-_Going_under_(mp3.pm).mp3`
- `C:\Users\Usuario\Downloads\Evanescence - Afterlife (SPOTISAVER).mp3`
- `C:\Users\Usuario\Downloads\Papa_Roach_Hanumankind_-_See_U_in_Hell_from_the_Netflix_Series_Devil_May_Cry_Explicit_(mp3.pm).mp3`

Os caminhos foram indicados pelo usuário nesta conversa. Preservar os arquivos originais; não tratá-los como cache na limpeza. Ainda não foram importados nem validados como AudioClips. Não substituir por músicas genéricas nem declarar integração concluída sem os MP3 reais.

## Implementação futura

1. Inspecionar os controladores de campanha, batalhas e áudio, identificar os três chefes e reutilizar o sistema existente, sem duplicá-lo.
2. Importar os arquivos reais na estrutura Unity apropriada; configurar AudioClips, referências, carregamento eficiente, volume e loop conforme o sistema existente.
3. Ao iniciar a bossfight, fazer fade-out do ambiente anterior e fade-in curto da música correta. Evitar reprodução simultânea e vazamentos entre cenas.
4. Manter a faixa durante toda a batalha, inclusive mudanças de fase e ataques especiais, sem reinício indevido.
5. Na vitória, encerrar suavemente e retomar a trilha apropriada de vitória, cutscene ou ambiente.
6. Tratar morte do jogador, reinício, abandono e troca de cena, interrompendo/reiniciando corretamente.
7. Respeitar controles de volume e configurações existentes. Preservar efeitos de golpes, habilidades e diálogos, músicas usadas em outras partes, desempenho e salvamento. Não alterar mecânicas dos chefes, animações, sprites ou cutscenes sem necessidade.

## Validação e conclusão

- Testar as três bossfights: início, fases, ataques especiais, loop, vitória, morte do jogador, reinício, abandono e troca de cena.
- Conferir ordem correta, transições suaves, ausência de duplicação de música, ausência de vazamentos, volumes e preservação dos efeitos existentes.
- Distinguir testes automatizados silenciosos de audição real; observar as autorizações de áudio aplicáveis ao retomar a tarefa.
- Informar objetivamente implementação concluída, MP3 efetivamente integrados, testes realizados e limitações restantes.

**Distribuição:** usar as faixas comerciais somente em contextos com os direitos/licenças necessários. Não publicar builds contendo essas músicas sem autorização de distribuição. A existência dos arquivos locais não comprova essa autorização. A autorização anterior de publicação do jogo não se estende automaticamente à distribuição destas três faixas.
