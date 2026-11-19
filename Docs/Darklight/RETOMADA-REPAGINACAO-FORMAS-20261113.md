# Repaginação das formas — direção vigente em 13/11/2026

## Ordem expressa do usuário

1. Completar **Fabius Serafim Carmesim** primeiro: Katana, Punhos/Grevas e Serras/Moto, com desenho fiel à referência principal já aprovada, caminhadas e estados coerentes.
2. Depois completar **Fabius Ruptura**: outras armas e correções seletivas de caminhadas divergentes. Escopo visual menor que o Serafim. Referência fornecida novamente nesta conversa; correspondente local inspecionada em `ArtSources/FabiusForms20261109/references/fabius-ruptura-carmesim-original.png`.
3. Depois **Lucius Arconte**: usuário rejeitou os ataques atuais e informou vistas de caminhada faltantes. Refazer ataques e completar locomoção pela imagem anexada nesta conversa: cabelo prateado com mechas verdes, olhos verdes, sobretudo preto com acabamento dourado, botas com fivelas douradas, faixa azul, espada prateada com energia verde e aura verde escura. Não usar o antigo design azul ou roxo como autoridade desta forma.

Não antecipar produção de Ruptura/Arconte enquanto Serafim estiver incompleto. Não alterar sprites, proporções nem pivôs calibrados de Fabius Normal. Manter as mudanças locais anteriores. Esta sessão não autoriza chamar candidatas de integração validada.

## Auditoria do Serafim no checkout

Consumidor: `Assets/Darklight/Runtime/Character/DarklightFabiusFormsBank.cs`.
Catálogo: `Assets/Darklight/Resources/Darklight/FabiusForms20261111/catalog.json`.

| Ação | Conteúdo catalogado | Problema observado no consumidor |
| --- | --- | --- |
| KatanaAttack | 9 quadros, marcados E | Outras direções não resolvem nessa API; poses desenhadas precisam de revisão antes de aceitar os rótulos E. |
| FistAttack | 9 quadros, marcados E | Outras direções não resolvem. |
| MotoToSaws | 12 quadros, marcados E | Outras direções não resolvem. |
| MotorcycleRide | 8 quadros, marcados E | Não selecionado por TryResolveFormPose; CharacterView ignora esse banco durante riding. |
| TransformationUltimate | 8 quadros, marcados E | Sem ramo de seleção nessa API. |
| Caminhada | N/S, W/E e SE | SW usa S; NW/NE usam N. Seleção de caminhada não distingue arma: pode exibir Katana com Punhos/Serras. |
| Guard | 6 quadros SE | Só selecionada para Katana em Parry e SE. |
| Hurt/Dead | HurtDead E + Hurt SE/6 e Dead SE/6 | SE específico só para Katana; demais direções não estão cobertas por esses clipes. |
| Dodge | FlyGlide E/6; Fly8D E/24 | Fly8D não é selecionada; o nome 8D não comprova oito direções. |

Fontes existentes fora do mapeamento: `ArtSources/NovasArtesComplementares20261111/23_Fabius_Serafim_Dodge_Norte_Sul.jpg` e `24_Fabius_Serafim_Dodge_Lateral.jpg`. Variantes recuperadas 008 e 077 também existem, mas não são automaticamente aprovadas.

Uma sequência de ataque com nove fases não representa nove direções. Não duplicar pixels E ou simplesmente alterar direction para anunciar cobertura. Confirmar cada figura inteira e sua perspectiva antes de mapear.

## Produzido nesta sessão, sem integração

Fontes preservadas em `ArtSources/FormsRevision20261113/`:

- `RupturaPunhosWalkES-candidate-v1.png`: 12 figuras geradas antes da mudança de prioridade. Não aprovada para integração: cabelo da primeira fileira toca o limite superior; alternância das pernas precisa de revisão. Produção de Ruptura pausada pela nova ordem.
- `SerafimKatanaWalkEN-candidate-v1.png`: 12 figuras. Não aprovada para integração: os seis primeiros quadros pedidos como E saíram em três quartos; alternância de contatos precisa de revisão. Nome do arquivo registra o pedido, não certifica direção real.

Nenhum desses PNGs foi colocado em Assets, nenhum catálogo/importador/runtime foi alterado nesta sessão, nenhum teste Unity ou nova build foi executado. Não reaproveitar essas candidatas como se estivessem aprovadas.

## Referência necessária e próximo passo

A referência individual principal de Serafim está identificada como `libfile_19e02b6c4f90819186b7d320d316baef` (Captura de tela 2026-11-11 202235(1).png), também reapresentada no histórico como `libfile_11838abe878c8191a4ec203818009c43`. A leitura retornou identificação da imagem, mas não os pixels para inspeção local; a transferência dos bytes falhou. Não tratar uma folha antiga, justamente questionada pelo usuário, como substituta dessa autoridade visual.

Retomar com a imagem principal reanexada nesta conversa ou materializada com sucesso. Comparar referência inteira com fontes existentes; produzir apenas figuras completas necessárias, revisar identidade/armas/direções/alternância de passos/margens, conferir recortes reais e apoio, integrar e testar antes de considerar Serafim completo. Só então seguir para Ruptura e Arconte. Manter a solicitação anterior de build como etapa posterior à revisão e integração, sem entregar a build antiga como resultado desta rodada.

Consulta de cota falhou nesta sessão; saldo e reset permanecem desconhecidos. Nenhum commit/push executado.
