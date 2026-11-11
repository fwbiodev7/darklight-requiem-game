# Guia das Partículas — DARKLIGHT: REQUIEM (v2.0 Estilo Final)
**Data:** 10/11/2026  
**Autoridade de Design:** Usuário / Direção de Arte  
**Status:** ESPECIFICAÇÃO OFICIAL APROVADA  
**Imagem Mestre:** `ArtSources/CombatParticles20261110/Darklight_Combat_Particles_v2_Master.jpg`

---

## Regra Geral Obrigatória
- **Tudo em pixel-art.**
- Sem blur realista, sem smoke “soft” demais, sem efeito vetorial liso.
- Bordas legíveis e recortadas.
- Brilho forte, mas com leitura de sprite.
- Transparência PNG.
- Preferência por escala base em múltiplos limpos.
- Aparência compatível com o jogo, sem virar VFX realista fora do estilo.

---

## 1) CONTATO / IMPACTO GERAL
Essas são as partículas de acerto básico, choque de golpe, impacto em inimigo ou colisão visual rápida.

### Tamanho recomendado
- **Pequeno:** 32x32
- **Médio:** 48x48
- **Grande:** 64x64

### Como usar na gameplay
- **32x32:** hit leve, contato rápido, golpe pequeno.
- **48x48:** impacto padrão de combo.
- **64x64:** impacto forte, hit confirmado importante, final de combo ou ataque pesado.

### Como deve aparecer
- Flash rápido.
- Surge no ponto exato do impacto.
- Duração curta (4 a 6 frames úteis / 8 frames no ciclo master).
- Começa com brilho concentrado e depois abre em faíscas.
- Para impacto neutro: branco/amarelo.
- Para impacto de espada/energia: branco-azulado.

### Visual pixel-art
- Centro brilhante.
- Pontas em estrela.
- Fragmentos pequenos saindo.
- Nada muito borrado.

---

## 2) ATAQUE DE KATANA
São os rastros e arcos de corte da espada do Fabius.

### Tamanho recomendado
- **Curto:** 64x64
- **Médio:** 96x96
- **Grande:** 128x128

### Como usar na gameplay
- **64x64:** cortes rápidos e leves.
- **96x96:** ataque normal mais visível.
- **128x128:** golpes mais dramáticos, combos, heavy slash, finalizador curto.

### Como deve aparecer
- Seguir o movimento da lâmina.
- Formar um arco limpo, elegante e rápido.
- Pode aparecer levemente inclinado conforme a direção do golpe.
- Deve nascer junto do swing e sumir logo após o follow-through.
- Duração curta: 3 a 5 frames por slash.

### Visual pixel-art
- Branco com azul-claro.
- Brilho forte no centro.
- Pontinhas ou faíscas ao redor.
- Curva fluida, mas ainda bem legível em sprite.
- Sem parecer pincelada digital suave demais.

---

## 3) ATAQUE DE PUNHOS
São os impactos dos socos/chutes/grevas do Fabius.

### Tamanho recomendado
- **Pequeno:** 40x40
- **Médio:** 56x56
- **Grande:** 72x72

### Como usar na gameplay
- **40x40:** socos rápidos.
- **56x56:** golpe padrão.
- **72x72:** golpe pesado, impacto mais brutal, hit de combo forte.

### Como deve aparecer
- Explosão curta e “seca”.
- Mais peso que elegância.
- Deve parecer um impacto bruto, com pressão e explosão.
- Pode combinar clarão + poeira + pequenos fragmentos.

### Visual pixel-art
- Branco forte no centro.
- Vermelho/carmesim discreto ou branco/amarelado dependendo do golpe.
- Formato de explosão irregular.
- Pode ter pedrinhas/poeira para dar peso.
- Deve vender sensação de porrada, não de magia.

---

## 4) ATAQUE DE MOTO
São efeitos de aceleração, derrapagem, ataque da moto e impacto pesado.

### Tamanho recomendado
- **Pequeno:** 96x64
- **Médio:** 128x96
- **Grande:** 160x128

### Como usar na gameplay
- **96x64:** arranque curto, mini derrapada, contato rápido.
- **128x96:** ataque padrão da moto.
- **160x128:** técnica forte, colisão, atropelamento, explosão de impulso.

### Como deve aparecer
- Mais horizontal.
- Fumaça, poeira, faíscas e atrito no chão.
- Precisa parecer pesado e veloz.
- Funciona bem atrás da moto e também em impacto frontal.
- Deve acompanhar o deslocamento, principalmente perto do solo.

### Visual pixel-art
- Cinza escuro + cinza claro + laranja/faísca.
- Fumaça mais “blocada” e legível.
- Partículas de chão/pedra.
- Rastro de velocidade agressivo.
- Não deixar suave demais; manter leitura pixelada.

---

## 5) ESQUIVA DE KATANA
Esquiva rápida, leve, ágil, quase como um corte de vento.

### Tamanho recomendado
- **Pequena:** 64x32
- **Média:** 96x48
- **Grande:** 128x64

### Como usar na gameplay
- Para dash/esquiva curta com espada.
- Funciona como rastro de vento/energia.
- Pode surgir atrás do personagem ou acompanhando o corpo.

### Como deve aparecer
- Rastro fino e rápido.
- Sensação de agilidade e precisão.
- Duração bem curta (3 a 4 frames).

### Visual pixel-art
- Branco-azulado.
- Linhas de velocidade.
- Pequenos brilhos nas pontas.
- Aparência elegante, limpa e afiada.

---

## 6) ESQUIVA DE PUNHOS
Esquiva mais física, agressiva e seca.

### Tamanho recomendado
- **Pequena:** 64x32
- **Média:** 96x48
- **Grande:** 128x64

### Como usar na gameplay
- Dash evasivo com sensação de pressão no chão.
- Melhor quando combinado com poeira ou turbulência curta.

### Como deve aparecer
- Mais bruto que o da katana.
- Pequena explosão de ar/poeira no início.
- Rastro curto, turbulento.
- Pode parecer deslocamento com impulso de corpo.

### Visual pixel-art
- Cinza, bege, branco sujo.
- Pedrinhas ou poeira curta.
- Menos brilho mágico, mais força física.

---

## 7) ESQUIVA DE MOTO
A mais impactante das esquivas.

### Tamanho recomendado
- **Pequena:** 96x48
- **Média:** 128x64
- **Grande:** 160x80

### Como usar na gameplay
- Derrapagem, evasão lateral, mudança brusca de direção.
- Deve deixar rastro forte.
- Pode ser usada também em impulso técnico da moto.

### Como deve aparecer
- Rastro pesado e veloz.
- Faíscas no chão.
- Fumaça escura e fricção laranja.
- Precisa parecer a esquiva mais “violenta” visualmente.

### Visual pixel-art
- Fumaça escura.
- Faíscas laranja/vermelhas.
- Linhas fortes de deslocamento.
- Massa visual maior que as outras esquivas.

---

## Organização da Folha Mestre (7 Linhas x 8 Quadros)

| Linha | Efeito | Resolução Base | Frames | Identidade Visual | Sistema Unity |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Linha 1** | Contato / Impacto Geral | 32x32 / 48x48 / 64x64 | 01 a 08 | Flash estelar branco/amarelo com faíscas radiais | `DarklightPixelContactBurst.cs` |
| **Linha 2** | Ataque (Katana) | 64x64 / 96x96 / 128x128 | 01 a 08 | Arco de corte crescente branco-azulado e pontas afiadas | `DarklightCombatTrail.cs` / `FabioSlash` |
| **Linha 3** | Ataque (Punhos) | 40x40 / 56x56 / 72x72 | 01 a 08 | Explosão seca e bruta com fragmentos | `DarklightRuinFistsBank.cs` |
| **Linha 4** | Ataque (Moto) | 96x64 / 128x96 / 160x128 | 01 a 08 | Atrito no chão, fumaça blocada e faíscas | `DarklightAbyssRider.cs` |
| **Linha 5** | Esquiva (Katana) | 64x32 / 96x48 / 128x64 | 01 a 08 | Linhas de velocidade afiadas e rastro de vento azulado | `DarklightActionParticles.cs` |
| **Linha 6** | Esquiva (Punhos) | 64x32 / 96x48 / 128x64 | 01 a 08 | Turbulência física, poeira e pedregulhos no chão | `DarklightActionParticles.cs` |
| **Linha 7** | Esquiva (Moto) | 96x48 / 128x64 / 160x80 | 01 a 08 | Derrapagem pesada, queima de asfalto e rastro largo | `DarklightActionParticles.cs` |

---

## Escala Visual na Gameplay
- **Contato:** Pequeno e rápido, centrado no ponto exato do hit.
- **Katana:** Acompanha a lâmina, sem encobrir o personagem inteiro.
- **Punhos:** Concentrado próximo ao membro atacante (punho ou bota).
- **Moto:** Maior e mais horizontal, ocupando espaço rente ao chão.
- **Esquivas:** Sempre rastro curto e ágil, nunca mais chamativas que o golpe principal.
- **Esquiva da Moto:** A maior entre as esquivas, destacando a massa mecânica do veículo.

