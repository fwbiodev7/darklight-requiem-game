# Configurações Funcionais e Parâmetros Canônicos — DARKLIGHT: REQUIEM
**Data:** 05/11/2026  
**Pipeline:** Universal Render Pipeline (URP 17.6)  

---

## 1. Parâmetros de Câmera e Renderização 2D

Para garantir nitidez absoluta, proporção gótica e 0 jitter em todas as resoluções:

| Parâmetro | Valor Canônico | Justificativa Técnica |
|---|---|---|
| **Projection** | `Orthographic` | Perspectiva 2D lateral/topo isenta de distorção de profundidade angular. |
| **PPU de Cenário (Art)** | `24` | 1 unidade de mundo Unity = 24 pixels de arte rasterizada. |
| **Tamanho da Grade (Grid)** | `0.041666f` ($1 / 24$) | Alinhamento do snap físico para cada pixel da composição. |
| **WorldVisualScale** | `1.325f` | Proporção imponente do caçador Fábio em relação ao ambiente de Vespera. |
| **Filter Mode (Sprites)** | `Point (no filter)` | Garante contornos afiados no estilo dark fantasy, sem interpolação turva. |
| **Mipmaps** | `Disabled` | Impede que a GPU troque para texturas reduzidas durante movimentação. |
| **MSAA / FXAA** | `Disabled` | Anti-aliasing borra os contornos estilizados do pixel art. |
| **Z-Sorting Precision** | `100f` (100 camadas/metro) | Base `3000` em `DarklightSorting.cs` para precisão milimétrica. |

---

## 2. Parâmetros da Linha do Tempo (Timeline & Cutscenes)

| Parâmetro | Valor Canônico | Descrição |
|---|---|---|
| **Taxa de Quadros (Frame Rate)** | `30.0 fps` | Alinhada ao timing de animação clássico e passos do pixel art. |
| **Modo de Encerramento** | `Hold` / `None` | Segura o último frame até que a transição de retorno do controle seja disparada. |
| **Extensão dos Arquivos** | `.playable` | Salvos no diretório canônico `Assets/Darklight/Cutscenes/`. |
| **Sinais Narrativos** | `SignalReceiver` | Mapeados no GameObject `CutsceneDirector` com chamadas atômicas. |

---

## 3. Parâmetros Canônicos do Caçador (Player Specs)

| Elemento | Especificação Visual | Código Hexadecimal / RGB |
|---|---|---|
| **Íris dos Olhos (Base)** | Vermelho Escuro Vivo | `#C40814` |
| **Íris dos Olhos (Sombra)** | Bordô Profundo | `#5A0000` |
| **Esclera (Branco dos Olhos)** | Branco Limpo com Brilho | `#FFFFFF` (Sem tingimento vermelho) |
| **Pele Padrão** | Pigmento Neutro Claro | RGB `[246, 174, 130]` |
| **Mechas Ruptura / Eclipse** | Púrpura Abissal | `#8A1C9E` (Mantendo o corte canônico) |
| **Lote de Geração de Sprites** | 8 a 12 figuras inteiras | Sprites completos com cabeça, corpo e arma unidos. |
