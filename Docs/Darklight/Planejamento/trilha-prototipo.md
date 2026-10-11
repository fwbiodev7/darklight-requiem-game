# DARKLIGHT: REQUIEM — trilha fechada do protótipo pessoal

Atualização de 10/10/2026: usar somente os cinco MP3 fornecidos pelo usuário. Cópias sem conversão em `AudioSources/Darklight/DarkResonance/`, no projeto independente. Os originais em Downloads permanecem intactos. Right Here in My Arms e Sad But True foram retiradas da seleção; não há outras faixas previstas.

| Modo | Artista — faixa | Duração reportada pelo Windows |
|---|---|---|
| Love Metal | HIM — Join Me in Death | 3:35 |
| Love Metal | HIM — Wings of a Butterfly | 3:32 |
| Thrash Metal | Metallica — Seek & Destroy (Remastered) | 6:55 |
| Thrash Metal | Metallica — Creeping Death | 6:37 |
| Heavy Metal | Metallica — Enter Sandman | 5:32 |

Os tempos acima são propriedades dos arquivos, arredondadas ao segundo; não foram medidos pela reprodução no Unity. Todos reportam dois canais. Join Me in Death reporta 48 kHz; os demais, 44,1 kHz. As cópias mantêm formato, bitrate e conteúdo originais. O manifesto registra valores detalhados e hashes SHA-256 iguais entre origem e cópia.

## Estado do pacote

Recebimento, cópia e verificação de integridade concluídos. O pacote contém exatamente cinco MP3, totalizando 47.236.457 bytes. Ainda não houve importação em Assets, reprodução no Unity, cortes, conversão, análise de BPM ou integração ao gameplay. Dark Resonance permanece planejado.

IDs estáveis: `love_join_me`, `love_wings`, `thrash_seek`, `thrash_creeping`, `heavy_sandman`. Cada par do mesmo modo compartilha o modificador; trocar a faixa não prolonga duração nem remove cooldown. Heavy Metal tem apenas uma opção musical.

## Integração prevista

Importar as cinco cópias e validar reprodução e volume. Para cada gravação, escolher um trecho, registrar BPM/variações e marcadores de batida, definir transição de entrada/saída e conectar ao modo correspondente. Não inferir BPM exato pelo título da faixa.

O efeito dura 15 ou 20 segundos conforme o modo, independentemente da duração total da música. Sincronizar VFX com marcadores e relógio de áudio. Expiração encerra o buff e passa ao cooldown; a transição sonora deve respeitar essa duração. Falha de carregamento não pode travar o menu nem manter um modificador eterno. Manter controles de volume e intensidade visual.

Não baixar ou acrescentar músicas. Os arquivos fornecidos atendem ao protótipo individual solicitado; a preparação de uma versão pública continua sendo um marco separado.

Manifesto do pacote: `AudioSources/Darklight/DarkResonance/manifesto-audio.json`. Cópia de revisão: `outputs/audio/manifesto-audio.json` na pasta de trabalho.
