# Fabius — importação e QA focado de 03/11/2026

## Resultado atual

**167 clips / 492 figuras / 52 folhas ativas com recibos de importação Unity.** Não confundir com aprovação global de Fabius. Foram importados 78 clips / 168 figuras nesta retomada. Os 15 clips iniciais passaram novamente por 3 testes PlayMode isolados, sem campanha, áudio, saves ou preferências gravados. XML: `outputs/retomada-20261102/integration-play-184503.xml`.

Não houve build ou validação completa de gameplay. Defesa/dano/morte nas 8 direções de cada arma e faíscas Moto ainda precisam de validação funcional específica. Os três testes não comprovam toda a entrega.

## Memória

Uma tentativa diagnóstica com RAM inicial aproximadamente 0.67 GiB superior à tentativa interrompida conseguiu finalizar o refresh e importar uma folha. Observador: `work/codex-memory-diagnosis-20261103/profiled-sheet/process-memory.json` (24 amostras). Picos amostrados: dotnet 1387 MiB, Unity principal 1179 MiB, worker Unity 835 MiB. São máximos por processo observados, não necessariamente simultâneos. Guardas preservados. Não encerrar compiladores de outras sessões.

Recibo de importação inicial: `outputs/retomada-20261103/profiled-sheet-import.json`. A tentativa seguinte revelou defeito real de geometria dos atlas, sem parar pelo guarda; a importação final corrigida concluiu em `outputs/retomada-20261102/ready-packet-184342-import.json`. Cache aquecido preservado.

## Correção concreta dos atlas

O check-only antigo não validava interseções de retângulos e portanto seu `contractsValid: true` não provava ausência de sobreposição. Unity encontrou sobreposições de 1–3 linhas em três folhas Hurt N/NE/E/SE (Katana, Punhos e Moto). Esse achado corrige a declaração anterior de perfeição geométrica no relatório recebido.

Foram criadas versões derivadas `LayoutV2` com margens, sem regenerar arte. As 24 figuras completas mantêm RGBA byte a byte, dimensões locais, pivôs, PPU, timings e IDs de frames. Fontes e atlas anteriores permanecem intactos. Provas: `outputs/retomada-20261103/hurt-layout-repair/rgba-proofs.json`; páginas revisadas: `pages.json`; atlas: `staging/` no mesmo diretório.

O catálogo ativo usa as versões novas. As três entregas antigas têm `supersededBy` no histórico; importador e contador ignoram layouts superados. A checagem Python agora rejeita interseção e bounds antes de lançar Unity. Caso negativo original e caso positivo corrigido verificados em `preflight-receipt.json`.

**Antigravity:** o snapshot antigo `outputs/entrega-artes-fabius-20261103/` permanece preservado. Não reaplicar suas três folhas Hurt originais ao catálogo ativo. O overlay LayoutV2 e o catálogo/recibos atuais prevalecem; os estados já estão importados. Antes de trabalhar, conferir a trava de integração.

## Pendências exatas

- Punhos Walk SW: contato B e passagem B realmente alternados.
- Moto Walk W/E: alternância inequívoca com câmera constante.
- Revisão funcional de Parry/Hurt/Dead e faíscas Moto: contato, oclusão e uma emissão por entrada em defesa.
- Validação final de Fabius, antes de Gothic Eclipse e Lucius.

Os candidatos SW V19/V20 e Moto W/E desta retomada não foram aprovados. KatanaSW histórica é referência de identidade/escala, mas repete apoio; não usá-la como autoridade de passada. Não ampliar golpes, gerar transformações ou iniciar Gothic antecipadamente.

O prólogo está salvo no GDD como planejamento futuro após Lucius Normal, antes das transformações e Combate IA 2.0.
