# Presas Normal — retomada13h50 de25/10/2026

Réquiem Normal:40 novas figuras completas preparadas em cinco atlas, dez páginas. Produzidas e revisadas pelo agente; **ainda não importadas nem verificadas em gameplay nesta subtarefa**. Catálogo compartilhado, Unity, Play e áudio não foram alterados/iniciados.

Fragmento: `outputs/redesign-armas-20261025-retomada1350/presas-normal/presas-normal-pages.json`. Fontes versionadas e prompts: `ArtSources/WeaponRedesign20261025Retomada1350/presas-normal/`. Preparo reproduzível: `Tools/AnimationPreview/prepare_presas_normal_1350_20261025.py`; especificações/manuais de PPU e apoio em `preparation-specs.json`.

| Escopo Réquiem/Normal | Figuras inéditas | Estado desta entrega |
| --- | ---: | --- |
| Presas1 E/SE, quatro fases/direção | 8 | Revisadas/preparadas;32 figuras do golpe nas oito direções ao combinar com24 anteriores já integradas |
| Idle S/SW/W | 12 | Revisado/preparado |
| Idle NW/N | 8 | Revisado/preparado; nuca/costas coerentes, sem rosto frontal sobre tronco invertido |
| Idle NE | 4 | Novos quadros completos V3; corrente naturalmente oculta pelo painel distante |
| Idle E/SE | 8 | V2 corrige somente quadro SE03 inteiro, preservando sete bons |
| Parada, oito direções | 0 adicionais | Reutiliza fase neutra0 dos oito ciclos idle; não somar oito figuras inéditas |

São32 figuras de idle e oito de ataque novas, não72: as24 anteriores permanecem preservadas. Rock Normal é um contexto separado e continua pendente, seguido na próxima produção. Outras ações/golpes Normal e transformações permanecem na fila; o redesign integral não está concluído.

Fontes e atlas foram inspecionados visualmente: cabelo castanho curto, mesma identidade adulta/corpo, armas neutras, mãos com duas adagas, botas completas, olhos escuros vermelhos quando visíveis, pele limpa. S/SW/W e E/SE mostram frente/perfis próprios. NW/N/NE articulam cabeça, nuca, pescoço, ombros, tronco e base de forma coerente; N não apresenta olhos/rosto. A orientação dos golpes permite giro de ombros, sem substituir uma direção por rosto contrário. Nos idles frontais, as fases neutra/inalar/piscar/exalar apresentam alterações desenhadas de tronco, ombros, mãos e tecido, mantendo apoio das botas. Em costas, a fase2 acomoda a respiração/tecido; não inventa piscada visível.

Correções seletivas: o NE da fonte inicial tinha corrente no quadril oposto; seus quatro quadros completos foram excluídos e substituídos por V3, com a corrente coberta pelo painel distante. V2 não corrigia a corrente e foi rejeitada. Os oito NW/N corretos foram identificados como quadros inteiros e preservados byte a byte em uma fonte técnica de1536×674; nenhum componente anatômico foi recortado/transplantado. No idle E/SE, apenas a figura SE03 inteira foi substituída para manter a ponta da adaga dentro da fonte; sete figuras e todos os pixels fora de seu retângulo ficaram idênticos. Prova em `whole-frame-repair-proof.json`; script `preserve_presas_whole_frame_repairs_1350.py`.

PPUs finais por atlas: ataque E/SE173.232323; idle S/SW/W164.141414; idle NW/N158.080808; idle NE312.626263; idle E/SE229.292929. São fontes com resoluções distintas, sem redimensionar os pixels dos personagens. A calibração usa medidas manuais cabeça compacta/sola de postura comparável, excluindo adagas erguidas. Um PPU por folha. Medida projetada e posição de apoio têm incerteza; conferir na câmera Unity e nas transições.

O painel `world-scale-review.png` foi visto com16 figuras inteiras (recuperação do golpe e parada nas oito direções), PPUs reais e `WorldVisualScale=1.325`, lido do jogo. Todas usam112 pixels de exibição por unidade de mundo, sem autoajuste de miniatura por célula. O painel é uma comparação visual de escala, não um teste de gameplay.

QA fresco: cinco atlas,40 figuras, dez clips, **zero erros/47 avisos**. Quarenta avisos são de quantidade de cores, cinco de alpha parcial e dois da altura aparente dos golpes com adaga erguida. A auditoria não certifica paleta indexada nativa. Contatos empacotados inspecionados sem corte/halo aparente. Fontes, hashes, prova de RGBA e resultados estão nos manifestos e em `preparation-summary.json`. Relatórios finais em `qa-reviewed/`; cada modo mantém o SHA256 do input em seu arquivo `*-input.sha256`. As prévias HTML/GIF foram geradas; reprodução animada não foi inspecionada nesta subtarefa.

Para publicação, usar a seleção explícita `publication-selection.json`: excluir fontes iniciais parcialmente rejeitadas, V2 NE rejeitada, imagem intermediária de reparo E/SE e QA antigo `qa/`. As fontes finais conservam os pixels bons; hashes dos originais ficam na prova local. Próximos passos: coordenação revisar e mesclar os dez contextos, importar pelo consumidor existente, verificar escala/transições/contato sem áudio e atualizar cobertura real. Depois, Rock Normal separado, mantendo rosto/corpo/cabelo do mesmo Fabius e mudando apenas roupas/acessórios Rock.
