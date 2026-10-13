# Expansão de áudio — DARKLIGHT: REQUIEM — 13/10/2026

## Implementação

- Temas fornecidos pelo usuário: Rock = This Heavy Metal (MrClaps_); Réquiem = Motivation Epic Rock (AlexGrohl). As cópias no projeto têm o mesmo SHA-256 dos arquivos de Downloads. Não foram baixadas músicas.
- TodaySkinMusic controla as duas trilhas e mantém as fontes entre mudanças de mapa/checkpoint. Troca de skin em 0,85 s. Cada tema usa uma segunda fonte durante os últimos 0,7 s para suavizar o fechamento do loop. A sobreposição é limitada às transições controladas.
- Dark Resonance e primeiro despertar mantêm seus arquivos, custos, buffs, durações e recargas. A fonte existente de evento recebe fade-in/out; a trilha da skin é suspensa e retomada. Menus congelam as fontes e os pesos da transição. Música de menu existente permanece na fonte aprovada.
- O pool existente continua com 16 AudioSources de efeitos. Limite adicional de quatro vozes de inimigos e duas ocasionais de ambiente, com atenuação por distância e pan em estéreo. São quatro fontes de temas e quatro fontes de ambiente fixas, sem criar fontes por golpe ou inimigo.
- Os sete controles continuam sendo geral, música, efeitos, ambiente, interface, inimigos e personagem. Efeitos de inimigos agora respeitam a categoria Enemy; ambientes ocasionais respeitam Ambient.
- Passos: nove materiais, alternância de amostras, pequenas variações de pitch e volume. Contatos dos quadros 0/2 do ciclo atual de quatro quadros a 10 fps; exige estado Neutral, deslocamento real e gameplay ativo. Não emite passos de ataques, esquiva, transformação, conjuração, fones, troca de arma, parede ou teleporte.
- Materiais regionais e retângulos de pixel nas três pontes, piscina rasa e solo úmido do jardim. TodaySurfaceArea permite sobreposição explícita de materiais por Collider2D e prioridade, incluindo areia. Não foi inventado terreno de areia em mapas cuja arte não o mostra.
- Quinze categorias de inimigos têm duas texturas próprias. Dez chefes têm assinatura especial exclusiva, além de alerta/preparação, ataque, dano, morte, passos e respiração contextualizados. Golpes são disparados no contato real; observação do componente complementa respiração e fases.
- Reutilizados os 49 recursos existentes. Acrescentados 82 WAVs originais mono a 22.050 Hz: 18 passos, 30 identidades de criaturas, dez assinaturas de chefes, 12 transientes de poderes/combate, um batimento, seis camadas ambientais e cinco ambientes ocasionais. A expansão ocupa 3.755.178 bytes no disco; os curtos somam 3.269.540 bytes quando convertidos em PCM float.
- Ambientes em camadas: vento/Véu existentes, chuva, folhas, água, caverna, cidade, energia sobrenatural; ocasiões de trovão, goteira, criatura distante, sussurro e portal. O ambiente permanece audível durante Resonance com ajuste de ganho.
- Novos efeitos de garras, sombra, abraço, julgamento, Ruptura, Eclipse, Absoluto, parry, contra-ataque, crítico, finalização e esquiva. Batimento discreto abaixo de 28% da vida.

## Procedência e fontes

Os arquivos individuais da expansão estão registrados em `Assets/Darklight/Resources/Darklight/Audio/Expansion/EXPANSION-PROVENANCE.json`, com descrição, autor, duração, frequência e SHA-256. O gerador reproduzível está em `Tools/Audio/generate_audio_expansion.py`. Ele usa ruído filtrado, osciladores e envelopes; não usa gravações, falas, imitação de pessoas ou bibliotecas externas de samples. As 82 assinaturas foram conferidas e o pico real máximo foi 0,505295 (sem clipping).

Os efeitos antigos Kenney foram preservados, junto de seus registros e licenças. A consulta das páginas oficiais confirmou CC0 para [Impact Sounds](https://kenney.nl/assets/impact-sounds), [RPG Audio](https://kenney.nl/assets/rpg-audio) e [Interface Sounds](https://kenney.nl/assets/interface-sounds). O crédito a Kenney é voluntário.

Os dois novos temas são fornecidos pelo usuário, com páginas oficiais consultadas:

- [This Heavy Metal — MrClaps_](https://pixabay.com/music/hard-rock-this-heavy-metal-492569/).
- [Motivation Epic Rock — AlexGrohl](https://pixabay.com/music/rock-motivation-epic-rock-111444/).
- [Resumo da Pixabay Content License](https://pixabay.com/service/license-summary/): atribuição dispensada na licença publicada; não redistribuir o conteúdo isoladamente. As duas páginas indicam Content ID, portanto guardar os comprovantes de download do usuário para eventual contestação de vídeos.

`Assets/Darklight/Resources/Darklight/Music/SKIN-MUSIC-PROVENANCE.json` registra origem, autor, URLs, licença e SHA-256. As músicas anteriores do projeto não foram substituídas nem tiveram seus direitos reavaliados nesta expansão.

## Importação e validação silenciosa

Compilação confirmada no Editor Unity 6000.6.0f1. Importadores configurados pela API pública com SaveAndReimport; nenhum .meta foi escrito à mão. Readback em `audio-import-readback.json`: Rock 129,1755 s e Réquiem 119,11837 s; ambos estéreo a 44.100 Hz, Streaming, carregamento em background, sem pré-carregar o arquivo inteiro. As seis camadas novas também usam Streaming. Curtos mono usam PCM/DecompressOnLoad. O Editor tinha um AudioListener e nenhum mixer; as categorias do gerenciador existente foram mantidas.

Sete testes EditMode de DarklightAudioExpansionTests passaram com zero falhas: nove materiais, identidades de quinze inimigos/dez chefes, 82 recursos mono, streaming dos temas, contatos, prioridade de áreas e limites de vozes/predicados.

Quatro testes PlayMode de DarklightSkinMusicTests e a regressão silenciosa do primeiro despertar passaram: cinco testes, zero falhas. Junto dos sete EditMode, são 12 verificações de áudio aprovadas. DarklightAutomation.Silent permaneceu true; nenhuma reprodução, saves e preferências preservados. Evidências: audio-expansion-focused-tests.xml e audio-skin-focused-tests.xml, em finish-20261013. A contagem de AudioSources permaneceu constante durante mudanças de mapa e exploração; o pool ficou em 16 e as vozes ativas em zero durante o teste silencioso.

## Limites reais

- Nenhum som foi reproduzido nesta tarefa. Ainda precisa de escuta manual a qualidade musical do fechamento de loop dos MP3 e o equilíbrio final entre efeitos, ambientação e trilhas.
- Respirações, grunhidos e sussurros são texturas sintéticas estilizadas, sem atuação vocal humana ou falas. Não afirmar gravação realista ou dublagem.
- Testes silenciosos validam a mesma lógica de pesos, rotas, continuidade dos objetos, categorias, recursos e limites. Não medem a percepção de continuidade no dispositivo de saída nem DSP CPU em reprodução audível.
- A memória PCM acima é cálculo a partir dos samples importados/gerados, não uma captura de profiler em uma build. Não houve build, commit, push ou publicação por este agente.