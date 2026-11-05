# Fundamentos de Pixel Art & Guia Canônico (DARKLIGHT: REQUIEM)

Este documento integra os princípios técnicos universais de pixel art profissional ao cânone visual estrito de **DARKLIGHT: REQUIEM**.

---

## 1. Regra de Supremacia do Cânone Visual de DARKLIGHT

> [!IMPORTANT]
> O cânone visual aprovado de DARKLIGHT: REQUIEM é **soberano** sobre recomendações artísticas genéricas:
> 1. **Maturidade e Idade:** Fábio é um caçador adulto, calejado e maduro. **Proibido traços infantis, rostos fofos ou proporções chibi.**
> 2. **Olhos Canônicos:** Íris em **vermelho escuro claramente visível** em TODAS as formas (Normal, Ruptura e Eclipse Absoluto). Base: `#C40814`, Sombra: `#5A0000`. Jamais tingir a esclera (branco dos olhos) ou as pálpebras de vermelho.
> 3. **Pele Canônica:** Base padrão de pigmento `[246, 174, 130]`. Manter luz e sombra coerentes com a iluminação gótica de Vespera.
> 4. **Cabelo Canônico:** Nas formas Ruptura e Eclipse, mechas e cristais roxos são aplicados **sem alterar o volume, comprimento ou a silhueta do corte de cabelo**.
> 5. **Quadros Inteiros:** Geração sempre de personagens completos (cabeça, corpo, arma e vestimenta juntos). Nunca recortar ou colar partes anatômicas.
> 6. **Densidade de Geração:** Folhas organizadas em lotes de **8 a 12 figuras inteiras por folha**.

---

## 2. Fundamentos Técnicos & Prevenção de Defeitos

### 2.1 Silhueta e Legibilidade (The Silhouette Test)
* A silhueta de Fábio, dos chefes e dos inimigos deve ser imediatamente identificável em preto sólido (100% preenchimento).
* A pose da arma (Katana empunhada, Punhos em guarda de impacto, Moto em tração) deve sobressair da massa do tronco para que a ação seja legível em 1x.

### 2.2 Controle de Linhas e Contornos
* **Jaggies (Escadões Irregulares):** Evite sequências de pixels desarmônicas como `1-1-3-1-2`. Use progressões limpas (`1-1-2-2-3-3` ou `1-2-3`).
* **Doubles (Pixels Duplos Acidentais):** Remova cantos grossos de 90° onde dois pixels adjacentes criam nós não intencionais no contorno.
* **Outlines Estilizados:** Em DARKLIGHT, o contorno externo utiliza tons escuros de vinho/sombra gótica (`#1A0B10` a `#2A1218`), integrando a figura ao cenário noturno sem parecer colada.

### 2.3 Sombras, Volume e Luz (Shading & Form)
* **Direção de Luz Canônica:** Luz vinda do topo/ângulo superior frontal (iluminação ambiente do Eclipse e tochas de rua).
* **Pillow Shading (PROIBIDO):** Nunca sombreie uniformemente de fora para dentro contornando a borda como se fosse uma almofada. As sombras devem seguir a geometria cilíndrica e planar dos músculos, capas e armaduras.
* **Banding:** Evite que as linhas de transição de sombra repliquem exatamente a linha de contorno externa em degraus paralelos.

### 2.4 Clusters e Dithering
* **Clusters Intencionais:** Agrupe pixels da mesma tonalidade em formas limpas e legíveis.
* **Orphan Pixels (Pixels Órfãos):** Elimine pixels isolados sem propósito de textura que geram ruído ou sujeira visual.
* **Dithering:** Restrito a texturas pesadas (pedra bruta, ferrugem de lâminas ou asfalto destruído). **PROIBIDO dithering no rosto de Fábio ou na pele dos personagens.**

---

## 3. Checklist de Crítica e Auditoria Rápida (1x e Zoom)

Ao inspecionar qualquer animação (Idle, Walk, Attack, Dash, Hurt):
1. **Teste do Piscar (Squint Test):** Com os olhos semicerrados ou em miniatura (1x), a arma e o movimento se destacam?
2. **Teste de Anatomia:** As proporções da cabeça, tronco e pernas respeitam a altura de Fábio nas referências aprovadas da Katana?
3. **Teste de Paleta:** A cor dos olhos é `#C40814`? A pele está baseada em `[246, 174, 130]`?
4. **Teste de Contato:** Os pés tocam o solo na mesma linha de base horizontal em cada direção cardinal?
