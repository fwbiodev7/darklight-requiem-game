# Auditoria de proporções — Moto Normal — 01/11/2026

Revisadas **68 figuras** (Dodge24, Dentes32 e reunir/separarS12), em fonte nativa inteira e escala equivalente ao jogo, contra **24idles** Katana/Punhos/Moto nas oito direções. **Nenhum alongamento anatômico inequívoco foi identificado nas versões vigentes. Nenhuma arte foi alterada.** Isso é conclusão visual qualitativa das partes visíveis, não certificação esquelética ou teste Unity.

- **Dodge S/SW/W e NW/N/NE,18fig:** cabeça, tórax, coxa, canela e botas visíveis mantêm coerência com o repouso; altura reduzida e sobreposição são compatíveis com flexão. Exemplos baixos revistos nativamente: `Selected_07` e `NW_N_NE_V1_07` (IDs completos no JSON).
- **Dodge E/SE V5,6fig humanamente aprovadas:** agachamentos `_01/_04` compactos, pernas/botas coerentes. Advisory de câmera nos quadros **E `_00/_02`**: rosto/tronco mais frontais que o idleE de perfil. Não é prova de defeito de proporção; aprovação e pixels preservados.
- **Dentes,32fig:** cabeça/cabelo/tórax consistentes; avanço e rotação explicam a extensão traseira. Em SW/W `_02/_06`, perna dianteira dobrada e traseira estendida; não usar largura das serras/abas para medir membros.
- **Reunir/separarS,12fig:** cabeça/cabelo/tórax coerentes. Montados `_03.._05` apresentam encurtamento e abertura de pernas naturais à pose; moto oculta quadril/coxa. Pivô de roda e caixa do veículo não medem altura humana. Segmentos ocultos permanecem limite explícito de avaliação.
- **Idles cruzados,24fig:** porte e identidade compatíveis por direção; garras/armaduras dos pés de Punhos e discos das serras aumentam equipamento, sem estabelecer aumento de ossos ou pés corporais.

**Método:** `WorldVisualScale=1.325`,80px/unidade de prévia e PPU real de cada descriptor, alinhamento pelo pivô; figuras completas sem deformação anatômica. O helper da coordenação foi ampliado para conter retângulos inteiros: cortes antigos de extremos de armas eram apenas do painel QA; assets preservados. PPU não aprova anatomia.

**Evidência:** inventário em [moto.json](moto.json), com IDs completos, PNG/JSON/fontes e SHA-256 reais; seis contatos `moto/*-worldscale.png`, [idles cruzados](moto/cross-canonical-idles-worldscale.png), [transição nativa](moto/transition-S-native1x.png) e [transição em escala](moto/transition-S-worldscale.png). Todos os hashes de imagem/fonte conferem com os metadados de entrada. Dados de timing/pivô foram preservados.

**Estados:** revisão estática concluída; nenhuma integração ou execução Unity por esta auditoria.18Dodge anteriores importados/testados conforme relato da coordenação; V5E/SE permanece com receipt `imported=false/runtimeVerified=false` no instante lido. Integração posterior pertence à coordenação. Não inferir teste de movimento ou aprovação humana global.

**Pendência real:** continuidade de câmera E V5 registrada como advisory; nenhuma correção anatômica específica justificada. Preservar todos os quadros vigentes, especialmente V5 aprovado. Articulações encobertas não receberam medidas inventadas. Novos Parry seguem pausados para a auditoria solicitada.
