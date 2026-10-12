# DARKLIGHT: REQUIEM

RPG 2D em pixel art de Fábio Ronaldo. **Alpha em desenvolvimento.**

Um eclipse abre fendas em Vespera. Fábio, Lyra e Kael atravessam a cidade e o Véu, enfrentando as consequências do Projeto Eclipse.

## Versão pública e desenvolvimento atual

A [Alpha 0.1 para Windows](https://github.com/fwbiodev7/darklight-requiem-game/releases/tag/v0.1.0-alpha) é o lançamento anterior. O código deste repositório está mais avançado e **não representa um novo executável validado**.

O desenvolvimento atual inclui dez regiões e a base narrativa dos dez capítulos, três armas, duas skins, progressão, poderes vampíricos, minimapa com exploração e objetivos, diálogo gradual com retrato, HUD gótico e novos menus de pausa/opções. A campanha expandida, o catálogo envelhecido e o polimento continuam em produção. As novas posturas da katana ainda estão pendentes.

Leia [o feedback, as correções e as limitações de 11/10](Docs/Darklight/Feedback/FEEDBACK-2026-10-11.md). A dificuldade de orientação, colisões no Jardim/Observatório e a troca de idade entre animações foram registradas. Foram aplicados ajustes de piso e orientação e uma mitigação de aparência coerente; **não foram executados testes nesta etapa**, por solicitação do criador.

Enquanto o catálogo envelhecido estiver incompleto, a gameplay usa o conjunto inicial inteiro para evitar mudança de idade entre repouso, caminhada e combate. A idade narrativa permanece salva. As artes envelhecidas continuam disponíveis como prévias e serão habilitadas em gameplay quando seus conjuntos estiverem completos.

## Jogar

Baixe a Alpha 0.1, extraia o pacote completo e abra `Windows/DarklightRequiem.exe`. Consulte [o guia específico dessa versão](ALPHA-0.1.md). [Site do jogo](https://darklight-requiem.zfabiobrronaldo.chatgpt.site).

Os comandos do código atual podem ser remapeados: WASD/analógico esquerdo para mover; mouse esquerdo/X para ataque leve; mouse direito/Y para pesado; Espaço/B para esquiva; Q/LB para parry; E/A para interação; V/LS para trocar arma; H/analógico direito cima para poder vampírico; K/analógico direito baixo para escolher poder; T/LT para transformar; Tab/Select para mapa; Esc/Start para pausa. As teclas desta versão em desenvolvimento podem diferir da Alpha publicada. O personagem não segue o cursor do mouse.

## Abrir na Unity

Unity **6000.6.0f1**, dependências em `Packages`. Abra `Assets/Darklight/Scenes/DarklightCampaign.unity`. Importar e compilar não comprova gameplay, campanha completa ou controle físico.

`Darklight.Editor.DarklightPlaytestBuilder.Build` produz uma montagem local em `../outputs/teste-vampiro/Windows`. O argumento `--requiem-playtest` abre uma sessão temporária no Jardim dos Ecos com seis poderes e três armas, sem gravar campanha/exploração. Pausa → Ferramentas de teste permite reabastecer recursos e reiniciar o jardim. Não iniciar testes, gameplay ou áudio automaticamente enquanto a restrição do criador estiver vigente.

## Recursos e licenças

Arte, fontes e metadados de importação preservados. Press Start 2P acompanha sua licença OFL; novos efeitos e ambientes CC0 têm manifesto em `Assets/Darklight/Resources/DarklightAudio/AUDIO-LICENSES.json`. As músicas fornecidas pelo criador não recebem licença de redistribuição por estarem neste protótipo. Não incluir caches da Unity, builds, credenciais ou saves pessoais nos commits.
