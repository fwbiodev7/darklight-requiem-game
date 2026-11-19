# Lucius Normal — Hurt diagonal — 19/11/2026

Produzidas e revisadas 12 figuras completas: SW/NW/NE/SE, 3 fases por direção, tempos originais **85/120/110 ms**, sem loop. Tool integrada image_gen, quatro chamadas (12 figuras iniciais, correção de 9, correção NE de 3, correção NW de 3). Prompts em prompts.txt. Somente as figuras selecionadas estão nesta pasta; candidatas rejeitadas permanecem em ../work/lucius-continuidade-20261119/hurt/.

Identidade comparada com LuciusNormalHurt4V20261119 e repouso direcional completo. Mantidos cabelo prateado compacto, olhos azuis, pele, roupa preto/azul, botas e espada prateada na mão direita. Fontes e atlas finais preservam integralmente os pixels RGBA das figuras selecionadas; sem montagem anatômica, rotação plana, espelhamento ou resampling dos PNGs autoritativos. Prévia em tamanho de jogo usa apenas nearest-neighbor.

| Direção | PNG / corpo neutro | PPU | SHA-256 PNG e fonte selecionada |
|---|---|---|---|
| SW | 1086×440 / 345 px | 180.8901484 | dba55c459a79213c75c4fabddd8ac293421e59fb0239e4e852084e071e133fe4 |
| NW | 2172×724 / 616 px | 322.980670766 | 1abfe06a216c2b6f4f8d21b19bac8f195f64b3fc84a83bfb6d295e8a9e4edf51 |
| NE | 2172×724 / 650 px | 340.80752597 | bfd4311c063020a0eb6fc41b17786f1a5a060718c34b301ffe5d4563248fabd2 |
| SE | 1086×568 / 370 px | 193.998130168 | 22d04f96d452e039494b43b157e4617adc295a025138e96b5bd3d63951381297 |

Cada arquivo tem três retângulos horizontais completos. SW/SE: [0,0,362,H], [362,0,362,H], [724,0,362,H]. NW/NE: [0,0,724,724], [724,0,724,724], [1448,0,724,724]. Coordenadas, pivôs normalizados bottom-left e âncoras manuais estão nos JSONs entregues. O apoio exclui a ponta da espada. A escala usa altura neutra do corpo e 304/159.3930583 unidades como gabarito cardeal; quatro texturas evitam misturar densidades nativas.

Rejeições: v1 SW frontal demais, NE próxima de N e SE próxima de E; NW v1 rejeitada como sequência porque a cabeça invertia a direção no recoil. v2 NE recuperação virava à esquerda, portanto foi substituída pela sequência NE v3 coerente. NW v4 mantém a orientação esquerda com reação de flexão do tronco. Retidos SW/SE v2, NE v3, NW v4.

QA: inspeção das fontes completas e contato alinhado em tamanho de jogo e 2× nearest-neighbor; figuras, arma e botas completas, reação distinta e retorno de guarda coerente. Quatro audits estruturais da skill retornaram exit 0 e zero erros em prepared/audit-delivery-*/report.json. Os avisos reais são alpha parcial >10% e ausência de validação Unity; alpha nativo foi preservado após inspeção em fundo cinza. As fontes seguem a renderização das referências aprovadas, com muitas cores e alpha parcial; não se afirma paleta limitada. GIFs/HTML foram gerados; revisão temporal em execução e avaliação na Unity permanecem pendentes.

PNG+JSON+metas novos copiados para Assets/Darklight/Resources/Darklight/LuciusNormal20261109/. Integrados ao disco, **ainda sem catálogo/importação Unity/runtime**. catalog-snippet.json contém só as quatro páginas novas para a coordenação substituir os fallbacks Hurt de direções 1/3/5/7; não alterar páginas cardeais. Manter complete=false. Nenhum catálogo, Runtime, teste, Git, Play, build ou publicação alterado/executado por este trabalho.

Paletas completas em work/prepared/*-palette-delivery.json, hashes e localização em palette-files.json. Uma tentativa repetida de palette foi recusada por arquivo existente (exit 2); a geração em destinos novos passou nos quatro arquivos. Auditorias PPU iniciais alertavam precisão de serialização; JSON e meta foram alinhados a 12 algarismos significativos e os quatro audits finais não têm esse aviso.

Incidente informado à coordenação: audit_unity_metadata.py não aceita --help, interpretou-o como raiz e escreveu outputs/validation/metadata_audit.json vazio (exit 1, zero assets). Esse recibo não valida nada e deve ser restaurado ou excluído da entrega pela coordenação; nenhum Git foi usado para fazê-lo.

