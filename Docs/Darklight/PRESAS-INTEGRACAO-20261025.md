# Presas do Caos — preparo para integração em25/10/2026

Escopo real: Réquiem, Normal, primeiro golpe `Presas1`. Duas folhas/24 figuras completas em seis direções. Catálogo compartilhado e Unity ficam sob a coordenação principal; os fragmentos abaixo não significam importação ou validação em gameplay.

| Lote | Direções | Figuras | Fragmento do catálogo | PPU |
| --- | --- | ---: | --- | ---: |
| v2 revisada de24/10 | S,SW,W | 12 | `outputs/redesign-armas-20261025/presas-pages.json` | 143.93939394 |
| nova folha integrada de25/10 | NW,N,NE | 12 | `outputs/redesign-armas-20261025/presas-missing-views-pages.json` | 139.89898990 |

Atlas/manifestos: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqDaggersNormalNeutralPresas1S_SW_W20261025V2.{png,json}` e `ReqDaggersNormalNeutralPresas1NW_N_NE20261025V1.{png,json}`. Fontes originais preservadas em `ArtSources/WeaponRedesign20261024/candidates/` e cópias íntegras rastreadas pelo pipeline em `ArtSources/WeaponRedesign20261023/`. Os PNGs são raster nativo gerado de1536×1024 antes do empacotamento; resolução lógica indexada não certificada.

Preparo reproduzível: `Tools/AnimationPreview/prepare_presas_resume_20261025.py` e `prepare_presas_missing_views_20261025.py`. Jobs específicos na pasta de candidatas. A nova folha usou a ferramenta integrada `image_gen`; prompt final completo em `ReqDaggersNormalPresas1NW_N_NE20261025v1-prompt.txt`. Referência íntegra: v2 revisada, SHA256 `b918d0e89a14e1e6bf45597a075e7cddce2d8c96890c6ffe393033b72b4d83ff`.

Os24 quadros mantêm os pixels RGBA selecionados, sem recomposição anatômica, escala raster, espelhamento ou recoloração. A v2 mantém o reparo anterior restrito ao quadro inteiro SW02. O novo lote desenha costas/diagonais traseiras e mantém cabelo castanho, roupa preta/forro vinho e duas adagas neutras. No norte, as costas da cabeça substituem o rosto; rotação dos ombros acompanha o golpe. Cada mão segura sua própria adaga.

Pivôs seguem apoios das botas inspecionados na fonte; PPU é por folha, medido pelo pipeline e ainda requer comparação em câmera Unity. O valor174.242 do manifesto antigo era uma proposta anterior, não uma escala validada. Nenhum tempo, evento, colisão ou save foi alterado. Páginas mapeiam os nomes legados exatos; a prévia usa30/45/38/38ms e arredonda o startup real29.75ms somente para a visualização.

Auditoria estrutural: zero erros em ambos os lotes. Lote S/SW/W:13 avisos; NW/N/NE:14 avisos. São12 avisos de quantidade de cores por folha, alpha parcial em ambas e altura aparente no NW novo pela adaga erguida. A altura com arma não foi usada para redimensionar quadros individuais. Contatos da fonte e dos dois atlas empacotados inspecionados; figuras e pontas completas, sem halos aparentes no contato escuro. As prévias HTML/GIF foram produzidas; movimento não foi inspecionado nesta subtarefa. Não certificar paleta limitada, aprovação do usuário, importação Unity ou gameplay a partir dessa auditoria.

Pendentes: Presas1 E/SE para Réquiem Normal; todos os outros golpes/ações/movimentos da mesma identidade; Rock Normal e as três transformações nas duas skins. A coordenação deve mesclar os fragmentos, importar pelo importador existente, conferir câmera/PPU/transições/contato e atualizar a cobertura somente com arte realmente selecionada pelo runtime. Nenhum áudio ou Play iniciado nesta subtarefa.
