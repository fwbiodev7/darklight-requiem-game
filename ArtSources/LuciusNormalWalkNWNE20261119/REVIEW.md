# Lucius Normal — Walk NW/NE — 19/11/2026

Escopo: somente as duas direções ausentes da caminhada desarmada, espada embainhada nas costas. Referências inteiras inspecionadas: `LuciusNormalIdle8V1.png` e `LuciusNormalWalkNSSession20261114.png`. Consumidor consultado: `DarklightLuciusSpriteBank.cs`; descritor/importador existente: `DarklightWeaponRedesignImporter.cs`. Não usar WalkArmed estático ou a fonte21 repetida como caminhada diagonal.

Produzidas oito figuras completas em 4×2: linha1 NW(3), linha2 NE(5), contatoA → passagemA → contatoB → passagemB. Geração integrada ImageGen, cinco chamadas: candidata inicial; correção dos contatos opostos; tentativa localizada da passagem NW; figura inteira isolada de pose; reaplicação dessa pose na folha em densidade coerente. Arquivos raw preservados, incluindo tentativas superadas. A figura isolada maior serviu apenas de referência e não foi redimensionada nem incorporada.

Seleção final preserva pixel a pixel os quadros0/1/4/5 da primeira candidata, substitui2/6/7 por figuras completas da segunda e3 pela figura completa da última. `whole-frame-provenance.json` contém retângulos, hashes da fonte e RGBA de cada figura. Sem montagem anatômica, espelhamento, deformação, recoloração ou resampling da arte final. Assets antigos preservados.

Revisão estática: costas diagonais constantes, dois contatos diferentes por direção, passagem oposta NW corrigida, cabelo compacto prateado, sobretudo preto/azul e espada no mesmo ombro. Corpo completo e equipamento dentro das células. Os oito quadros são únicos. Fonte/atlas nativos1774×887. Alturas corpo cabelo→bota:406,399,409,407,412,401,409,406px. Mediana406.5px / estatura canônica1.9072348768 = PPU213.135784. Âncoras seguem o método sessão14: eixo do torso projetado sobre baseline medido das botas; não centralizar pela espada/caudas. Não corrigir proporção por PPU. `bodyAnchorPx`, `bootBaselinePx`, `bootSupportSpanPx`, bounds e pivôs reais estão no descritor.

Timings: quatro fases×142ms=568ms versus caminhada existente seis×95ms=570ms. Preserva aproximadamente a cadência integral; não inventa dois desenhos duplicados para preencher seis frames. Point, mipmaps off, compressão0 e oito spriteIDs novos preparados em `.meta`; importação real não executada.

PNG/JSON/.meta copiados para `Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalWalkNWNE20261119.*`. Catálogo compartilhado intocado: snippet em `../work/retomada-20261119/lucius/normal-walk-pages.json`, para integração única pela coordenação. Caminhada passou de6 para8 direções somente depois dessa integração; este relatório não declara o catálogo já alterado.

Auditoria técnica: oito frames, dois clips, zero erros, dois avisos: alpha parcial nativo (>10% dos samples visíveis) e ausência de importação/runtime. Transparência nativa preservada, incluindo specks subthreshold de alpha; não houve limpeza destrutiva. Contato opaco confirma ausência de grandes halos coloridos. Relatório completo fora do repo: `../work/retomada-20261119/lucius/normal-walk-audit/REPORT.md`. Contato de âncoras: `normal-walk-NWNE-anchors-opaque.png`. Prévia estática/HTML e dois GIFs: `normal-walk-preview/`.

Estado: produzido e revisado estaticamente; copiado Assets e metadados preparados. Não importado nem testado na Unity; catálogo, runtime, build e Git não executados. Tentativa de abrir a prévia HTML local foi bloqueada pelo navegador (protocolo file não permitido); nenhum contorno foi tentado. Revisão animada observada permanece pendente, embora GIF e HTML estejam gerados.

PNG SHA256: `9f82a3659a1cb2b9bb9f1db9edaf4775e1c1c8c93049f36432827586faf1e378`.
