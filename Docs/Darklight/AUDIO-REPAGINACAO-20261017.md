# Repaginação de áudio — 17/10/2026

## Atualização da retomada

- Fontes oficiais de Monster (mikeask) e Enemy sounds (AbNormalHumanBeing), CC0, verificadas e baixadas em ArtSources/AudioReview20261017 com hashes e URLs no PROVENANCE.json. Nenhuma música nova foi baixada.
- Monster preparado em mono 22.050 Hz, pico 0,5 e bordas com fade de 8 ms, preservando o original; conectado ao alerta/preparação do Crawler com ganho baixo e intervalo de 3,7 s para evitar sobreposição. Não reproduzido nesta sessão.
- Enemy sounds é um atlas de efeitos: preservado para seleção posterior, sem disparar o atlas inteiro no gameplay e sem presumir a semântica dos segmentos.
- Dez perfis ambientais distintos agora dosam vento, Véu e camada regional com transição gradual. Reaproveitam os recursos existentes; a fonte dungeon_ambient_1 foi reconfirmada na página de JaggedStone (CC0). Factory ambiance foi descartada desta seleção por ser publicada como música, preservando a restrição de trilhas.
- Nove testes nativos EditMode de áudio passaram: sete de infraestrutura e dois novos de perfis/pico/importação; nenhuma reprodução. XML: outputs/implementacao-fase2-20261017/testes-unity-audio-revisao.xml.
- Revisão auditiva, seleção das demais vocalizações e fechamento da repaginação completa continuam pendentes. Os passos, superfícies, impactos e poderes anteriores foram mantidos; não alegar que toda a repaginação está concluída.

O diagnóstico inicial abaixo é histórico.

Pedido do usuário: depois das animações, pesquisar na web e melhorar dano, ataques dos inimigos, passos, ambientação de cada fase e demais efeitos pertinentes. Incluído nas entregas de hoje. Testes sem reprodução automática.

## Situação atual

Pesquisa inicial realizada em 17/10. Nenhum novo arquivo externo foi baixado, importado ou ouvido nesta etapa. Integração e avaliação sonora permanecem pendentes.

A infraestrutura existente está documentada em Docs/AUDIO-EXPANSION-20261013.md: recursos Kenney, variações de passos, categorias de inimigos, camadas ambientais, controles de volume e limites de fontes. Reutilizar e conferir esses arquivos antes de acrescentar cópias dos mesmos pacotes.

## Fontes consultadas nas páginas de origem

| Fonte | Autor | Uso a avaliar | Licença publicada | Estado |
| --- | --- | --- | --- | --- |
| [Impact Sounds](https://kenney.nl/assets/impact-sounds) | Kenney | Impactos, foley e passos; conferir recursos já importados | CC0; 130 arquivos listados | Pacote já mencionado no projeto |
| [Monster](https://opengameart.org/content/monster-5) | mikeask | Vocalização de criatura | CC0 | Candidato sem download ou avaliação auditiva |
| [Enemy sounds](https://opengameart.org/content/enemy-sounds) | AbNormalHumanBeing | Alertas, dano e morte de inimigos | CC0; página informa fontes das adaptações | Candidato sem download ou avaliação auditiva |

As páginas estabelecem procedência e licença; não comprovam adequação artística no jogo. Pesquisar ainda ambientes das dez fases e armas/mecanismos, mantendo as músicas existentes.

## Execução seguinte

- Separar sons de carne, metal, defesa, crítico e ruptura de postura; sincronizar com contatos reais.
- Tornar preparação, ataque, dano e morte dos inimigos legíveis, limitando vocalizações simultâneas.
- Sincronizar passos com material, deslocamento e quadros de contato; impedir emissão parado ou contra parede.
- Diferenciar motor, engrenagens, sobrecarga, serras, montagem, derrapagem e impactos do Cavaleiro.
- Dar identidade ambiental às dez fases e aos ambientes internos pertinentes.
- Registrar origem, autor, licença, arquivo e hash; verificar duração, picos, cortes e custo de memória silenciosamente.
- Preservar os controles e limites de fontes existentes; evitar saturação e repetição.
- Avaliação audível somente quando o usuário solicitar reprodução.
