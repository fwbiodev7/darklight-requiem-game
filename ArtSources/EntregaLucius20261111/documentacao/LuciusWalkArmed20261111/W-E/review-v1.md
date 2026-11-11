# Lucius WalkArmed W/E — V1 — 11/11/2026

Estado: produzido e revisado visualmente com ressalvas; não importado, não testado em Unity. Fonte preservada integralmente; nenhuma edição de pixels nem montagem anatômica.

- Fonte: `LuciusWalkArmedWE20261111V1-source.png`, 1536×1024 RGBA, grade4×2 de384×512. SHA-256: `dbe17031db4485c28ff34b4bc28a6c3d25512e826eba9b8bcee34617f1f60763`.
- Oito figuras completas: linha superior W/esquerda; inferior E/direita. Lâmina, pés, cabelo e casaco cabem nas células; sem duplicata de espada nas costas. Espada única baixa em duas mãos. Roupa/olhos/cabelo compatíveis com referências inspecionadas inteiras.
- Escala real do desenho: cerca de378–380px de altura incluindo silhueta (inferior à meta aproximada412px); constante no ciclo. Fonte raster gerada nessa resolução; resolução artística anterior a ela não é conhecida.
- Eixo da cabeça W estável nas quatro fases; E varia1px. Limite inferior alpha>=128: W449/448/449/447, E448/446/448/446. Não calibrar pivô pelo centro do bounding box da espada. Corpo sem avanço deliberado entre células.
- Ressalva de movimento: contatos A/B e passagens A/B são muito semelhantes; a troca anatômica explícita das pernas é pouco legível no perfil e não foi certificada como alternância perfeita. Preservada a geração única conforme orientação de não refazer diferenças discretas. Não declarar gait biomecânico perfeito.
- Alpha real: 1.099.511pixels totalmente transparentes; alpha máximo254. Nenhum alpha/borda foi alterado.
- Plano: contato A140ms → passagem A130ms → contato B140ms → passagem B130ms;540ms. Timings derivados do Walk existente. `preview-W.gif` e `preview-E.gif` são cópias de inspeção quantizadas em fundo escuro; `preview.html` usa diretamente a fonte e oferece pausa, avanço, velocidade e fundo claro. Sequência estática foi inspecionada; execução no consumidor Unity não ocorreu.
- Pendência técnica do consumidor: `DarklightLuciusSpriteBank.ValidPage` reconhece a ação WalkArmed, mas a whitelist de loop ainda não a aceita. Coordenação deve resolver antes de cadastrar loop=true. Não houve alteração em Runtime, Assets, catálogo, Git ou Unity.
- Referências e hashes completos, retângulos top-left reais e bounds: `manifest-v1.json`. Prompt integral: `prompt-v1.txt`. Paleta medida e interpretação de materiais: `palette-v1.json`.
