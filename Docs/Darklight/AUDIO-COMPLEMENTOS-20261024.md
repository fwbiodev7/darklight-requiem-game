# DARKLIGHT REQUIEM — complementos de áudio de 24/10/2026

Eventos adicionais ligados aos bancos existentes; **nenhum áudio reproduzido ou aprovado por audição**. Os 19 efeitos recebidos e os 23 MP3 originais permanecem byte a byte iguais. Nenhum recurso novo foi importado nesta etapa.

## Eventos implementados

| Evento real | Banco utilizado | Limite da entrega |
|---|---|---|
| Troca efetiva de arma: guardar a anterior e sacar/equipar a próxima | Guardar: `rpg-metalLatch`; sacar mantém WM01/WM03/WM08 | Feedback genérico provisório. Não foi selecionado um trecho dedicado de guardar do WM01. Troca bloqueada, carregamento e restauração não disparam guardar. |
| Frenesi Carmesim termina | `interface-close`, categoria personagem | Sinal genérico de encerramento; não é uma nova gravação de dissipação. |
| Forma volta visualmente ao Normal | `interface-close`, categoria personagem | Segue `PresentedForm`, inclusive o ponto de liberação da saída. Não repete por quadro. |
| Dark Resonance termina | `interface-close` e `rpg-cloth` | Encerramento e retirada dos fones; a conclusão normal de colocar fones não dispara retirada. |
| Colocar fones é interrompido antes da ativação | `rpg-cloth` | Evento de retirada/cancelamento, uma vez. |
| Recarga selecionada conclui e há recursos suficientes | `interface-confirmation`, categoria personagem | Banco distinto da recusa PW11. Aguarda sangue/energia e agrupa conclusões simultâneas. Bloqueios momentâneos por outras ações continuam seguindo as regras existentes. |

Os eventos usam o pool e os volumes existentes. Pausa, morte, mudança de mapa e troca do modelo estabelecem uma nova referência; não reproduzem encerramentos antigos ao retomar. Selecionar uma habilidade cuja recarga já havia terminado também não cria um falso evento de conclusão. Fim simultâneo de mais de um poder produz um único sinal genérico de encerramento.

A observação de estados também acontece durante a automação silenciosa. O bloqueio central de reprodução continua ativo: observar uma transição não autoriza áudio nem cria fontes extras. Não foram alterados custos, dano, tempos, saves ou músicas.

## Verificação executada

Comando sem reprodução: `python Tools/Audio/audit_audio_completion_20261024.py` — **exit code 0, sem problemas**.

- 23 originais conferidos por SHA-256; 19 WAV preparados conferidos por SHA-256 e limites de canais/PCM/duração/picos.
- PCM decodificado: **4.353.472 bytes**, abaixo do limite de 5 MB para a remessa.
- Quatro fontes longas preservadas fora do catálogo e dos recursos do runtime.
- Cinco músicas previamente fornecidas; nenhum MP3 das três músicas comerciais adiadas encontrado em `Assets`.
- Evidência: `work/session-20261024/audio-completion-audit.json`.

Classe nova `DarklightAudioCompletionTests`: **8 testes escritos**, cobrindo fones/retirada/interrupção, saída de forma/Frenesi, pausa/reset, recarga real de sangue com falta de recurso, duração versus recarga real de Resonance, mudança de seleção/carregamento, conclusões simultâneas e bancos existentes sob bloqueio silencioso. Compilação e execução Unity integram a rodada coordenada principal; resultados devem ser registrados após essa execução. Os oito testes não foram contabilizados como aprovados nesta preparação.

## O que continua dependendo de audição ou novos arquivos

Os quatro longos **WM02, WM06, PW02 e PW09 não receberam seleção semântica**. A auditoria fornece somente uma linha do tempo completa em janelas uniformes de dois segundos, com tempos, origem e hash. Não usa amplitude para escolher golpes, não extrai WAVs e não importa candidatos em `Assets`.

Falta audição manual autorizada para escolher os trechos corretos e avaliar volume, repetição, clareza e sincronismo dos efeitos novos e dos bancos provisórios. As variações específicas de guardar katana, adagas, impactos, dor/queda/recuperação, esforço e dissipação continuam sem gravação dedicada validada. A lista de 89 conjuntos ainda sem nova remessa permanece no status de 23/10; os novos hooks não mudam essa contagem. As três músicas de bossfight continuam adiadas e fora da entrega.

## Arquivos desta etapa

- `Assets/Darklight/Runtime/Campaign/DarklightDeliveredSfx.cs`: entradas de guardar/fones/encerramento/recarga com bancos existentes.
- `Assets/Darklight/Runtime/Campaign/TodayCampaignSoundDesign.cs`: ligação às transições reais.
- `Assets/Darklight/Runtime/Campaign/TodayPowerAudioTransitions.cs` e `.meta`: observadores de fim de poder e recargas.
- `Assets/Darklight/Tests/EditMode/DarklightAudioCompletionTests.cs` e `.meta`: oito cenários focados.
- `Tools/Audio/audit_audio_completion_20261024.py`: auditoria reproduzível e linha do tempo para escuta futura.

Não foram modificados `DarklightCampaign.cs`, `DarklightCampaignVampire.cs`, fontes MP3, WAVs recebidos ou músicas.
