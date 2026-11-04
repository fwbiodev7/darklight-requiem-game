---
name: darklight-pixelart-animation
description: Produzir, corrigir e integrar sprites e animações de personagens do DARKLIGHT em oito direções, preservando identidade, quadros aprovados e contratos Unity. Use para trabalho artístico; auditoria sem produção pertence a darklight-animation-qa.
---

# Animação pixel art DARKLIGHT

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz. Use o processo existente em `Tools/Skills/pixel-art-sprite-sheet-master/SKILL.md` e somente seu workflow pertinente. Para raster novo, leia a skill `segredo-varginha-pixelart` disponível no catálogo e use a ferramenta integrada de imagens; seu nome não autoriza conteúdo de outro jogo.

## Contrato visual e técnico

Inspecione referência inteira e clip consumidor. Registre personagem, forma, arma, ação, direção, resolução nativa, duração, retângulos, pivôs, PPU, GUIDs e eventos. Consulte a matriz/recibo mais recente em `outputs/retomada-20261103/` antes de ampliar o escopo. N/NE/E/SE/S/SW/W/NW exigem desenho coerente, sem espelhamento automático de assimetrias.

Fabius adulto mantém Réquiem, corpo/cabelo compactos, pele original, olhos vermelho-escuros (#C40814, sombra #5A0000) e escala visual aprovada 1.325. Lucius mantém anatomia adulta compatível, rosto próprio, cabelo claro, olhos azuis, sobretudo preto/azul e espada grande; não use Fabius recolorido como arte final. Infância é identidade etária própria somente quando a cena exigir. Normal usa equipamento neutro.

## Produção seletiva

Preserve fontes e hashes. Gere folhas de 8–12 figuras inteiras quando houver esse volume real, sem inventar poses para completar lote. Cabeça, corpo e arma permanecem juntos inclusive nas referências; não monte partes anatômicas. Defeito localizado exige substituir somente os quadros completos afetados, preservando os corretos pixel a pixel.

Compare identidade, apoio dos pés, arco da arma, câmera, volume e continuidade no tamanho de jogo e em zoom nearest-neighbor. Alternância real de passada não se obtém deslocando/duplicando a mesma figura. PPU não corrige anatomia. Use prévia animada para antecipação, impacto, recuperação e loop; confira roupa/arma por direção.

Integre somente candidatos revisados pelo importador existente, preservando GUIDs, pivôs/retângulos variáveis, eventos e tempos de combate. Não imponha PPU único: confirme o contrato real. Confira filtro Point e nitidez na cena com pipeline ativa.

Registre fonte/hash → candidato → revisão visual → cópia Assets → importação → teste Unity → publicação. Prévia ou JSON não comprova gameplay. Para quadros deslocados, use `darklight-animation-qa` antes de regenerar; teste somente após correção justificada, sem repetir suítes encerradas.
