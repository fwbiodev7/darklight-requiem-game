# Relatório técnico de arte — 20/10/2026

## Resultado

Importadas 12 figuras completas: quatro ataques frontais de katana/Requiem/roxa e uma folha gerada de uma vez com oito poses de adagas/Requiem/roxa, frente/sudoeste. Adagas v2 limpa o avermelhado facial da candidata inicial. Três de 183 páginas transformadas trocaram referências; as outras 180 e os 260 PNGs anteriores permanecem iguais. É uma entrega parcial da padronização global.

Referências: captura dos olhos em `ArtSources/PlayerEyeReference20261020`, repouso Requiem da pele aprovada e sprite separado em `ArtSources/PlayerStyleReference20261020`. Geração integrada com figuras completas, sem montagem anatômica. Fontes finais/prompts/proveniência em `Assets/Darklight/ArtSources/PlayerVisualRevision20261020`; rejeitadas fora da entrega.

## Formatos e escala

| Folha | Fonte | Atlas importado | Consumo |
|---|---|---|---|
| Katana roxa | Quatro RGBA 1254×1254 | 1792×448, quatro células 448×448 | Fases 0–3 frontais, forma 2; outras 12 referências da página intactas |
| Adagas roxas v2 | Uma RGBA 1774×887, oito figuras | 2048×1024, oito células 512×512 | Quatro poses S e quatro SW, duas páginas CombatDaggers, forma 2 |

Katana: escala uniforme de cada figura inteira por nearest-neighbor, marcos registrados, corpo lógico de 220 px e PPU 110,55276. Adagas: figuras na escala original da fonte, retângulos/ownership reais e pivôs proporcionais aos originais; PPU pelo corpo. Ambas usam corpo de aproximadamente 1,99 unidades, compatível com o repouso atual do runtime (aprox. 2 unidades). A altura em pixels da prévia externa não foi confundida com unidades Unity. Nenhum membro foi esticado isoladamente.

Importação real: Point, sem mipmaps/compressão, tamanhos nativos, sRGB correto, ownership linear e IDs individuais. Tempos/eventos de combate preservados. Tempos HTML de inspeção: katana 100 ms; adagas 120 ms. JSONs registram retângulos, PPU, pivôs, clips, hashes e proveniência.

Histogramas reais extraídos: 45.282 cores RGBA na katana e 217.930 nas adagas, incluindo alpha parcial. São fontes geradas de cor contínua com aparência de pixel art, **não paletas indexadas restritas certificadas**. `palette.json` lista explicitamente as 256 cores mais frequentes; o histograma integral fica em `palette-full.json.gz`, sem perda e com SHA-256. Não houve quantização/recoloração global das figuras. O acabamento de paleta nativa permanece uma pendência de qualidade.

## Cor

`CanonicalComplexion.json`: 262 entradas convergem à base [246,174,130], via correção de pigmento quente já existente na renderização; fontes/sombras preservadas e ajuste desativado na morte. NPCs não afetados.

`PlayerIrisPalette.hlsl`: rampa #5A0000 → #C40814 nos três shaders, separada da transformação. Preserva pupila/branco, cabelo roxo, arma vermelha, pele e alpha. Conversão sRGB exata no PixelSprite evita perda nos canais escuros. Máscaras sem canal de íris **ainda não são corrigidas**; outras olheiras/íris autoradas estão pendentes. Nenhum filtro global de vermelho ou proposta automática facial foi aplicado.

## Verificação e limites

- Auditorias dos dois atlas: zero erros estruturais. Avisos de alpha parcial conferidos em fundo escuro/Point. Repetições nos retornos e menor altura de crouch/dive são intencionais; mais intermediários permanecem possíveis.
- Contatos/figuras completas e animações HTML vistos; seleção de quadros e zoom exercitada. Capturas QA-KATANA-NAVEGADOR.png e QA-ADAGAS8-NAVEGADOR.png.
- Unity 6000.6.0f1, EditMode: 11 casos distintos aprovados; depois da revisão v2, mais quatro verificações focadas de adagas/paleta aprovadas. XMLs anexos. Verificados importação, referências, pivôs, configuração de textura, compilação dos três shaders e renderização das cores nas quatro formas.
- Scripts: 23 testes de sprites e oito de saída compacta aprovados; skill validada pelo validador oficial. Compressão indevida e conversão escura inexata detectadas na primeira execução foram corrigidas e retestadas com sucesso.
- QA-ENTREGA.json confirma os 260 hashes PNG, escopo do catálogo e comparação do código protegido de morte/reversão com a base Git. Sem Play, áudio, teste auditivo, controle físico ou build nova.

Importação/revisão não certificam todas as transições no executável. A build 1.0.13 não foi substituída. Ver PENDENCIAS-20261020.md. Skill instalada com 12 fontes; economia de saída determinística em uso, sem Distill/modelo local ou porcentagem de tokens medida. Logs extensos permanecem em ../work; apenas relatórios e XMLs necessários integram a entrega.
