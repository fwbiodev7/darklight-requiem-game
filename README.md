# DARKLIGHT: REQUIEM

RPG 2D em pixel art de Fábio Ronaldo. **Alpha em desenvolvimento.**

Um eclipse abre fendas em Vespera. Fábio, Lyra e Kael atravessam a cidade e o Véu, enfrentando as consequências do Projeto Eclipse.

## Versão pública e desenvolvimento atual

A [Alpha 0.1 para Windows](https://github.com/fwbiodev7/darklight-requiem-game/releases/tag/v0.1.0-alpha) continua sendo o download público anterior. Em 13/10/2026 foi gerado e verificado um executável local atualizado; este commit contém seu código e recursos, sem substituir o pacote público da Alpha.

Correções verificadas: idade 0–4 preservada durante caminhada, combate e troca de armas; fallback da mesma skin/idade/arma; deslizamento em paredes com colliders originais; três transformações; sete volumes de áudio, temas por skin, nove superfícies e 82 efeitos originais. Animações inteiras aprovadas foram preservadas. Importadas 29 folhas/432 quadros completos de caminhada. Posturas da Katana foram canceladas.

Validação: 85 verificações EditMode, 87 PlayMode, reconferências físicas finais e 672 capturas reais do executável. Saves preservados, testes silenciosos. A cobertura inclui 4.800 combinações de identidade e rotas dos dez mapas; isso não é uma partida manual completa nem teste com controle físico.

Algumas ações envelhecidas ainda mantêm uma pose completa da idade e arma atuais quando falta uma folha específica. Essas animações, artes próprias de poderes e polimento continuam pendentes. Leia [o relatório de 13/10](Docs/CORRECOES-20261013.md) e [a expansão de áudio](Docs/AUDIO-EXPANSION-20261013.md).

## Jogar

Baixe a Alpha 0.1, extraia o pacote completo e abra `Windows/DarklightRequiem.exe`. Consulte [o guia específico dessa versão](ALPHA-0.1.md). [Site do jogo](https://darklight-requiem.zfabiobrronaldo.chatgpt.site).

Os comandos do código atual podem ser remapeados: WASD/analógico esquerdo para mover; mouse esquerdo/X para ataque leve; mouse direito/Y para pesado; Espaço/B para esquiva; Q/LB para parry; E/A para interação; V/LS para trocar arma; H/analógico direito cima para poder vampírico; K/analógico direito baixo para escolher poder; T/LT para transformar; Tab/Select para mapa; Esc/Start para pausa. As teclas desta versão em desenvolvimento podem diferir da Alpha publicada. O personagem não segue o cursor do mouse.

## Abrir na Unity

Unity **6000.6.0f1**, dependências em `Packages`. Abra `Assets/Darklight/Scenes/DarklightCampaign.unity`. Importar e compilar não comprova gameplay, campanha completa ou controle físico.

`Darklight.Editor.DarklightPlaytestBuilder.Build` produz uma montagem local em `../outputs/teste-vampiro/Windows`. O argumento `--requiem-playtest` abre uma sessão temporária no Jardim dos Ecos com seis poderes e três armas, sem gravar campanha/exploração. Pausa → Ferramentas de teste permite reabastecer recursos e reiniciar o jardim. Não iniciar testes, gameplay ou áudio automaticamente enquanto a restrição do criador estiver vigente.

## Recursos e licenças

Arte, fontes e metadados de importação preservados. Press Start 2P acompanha sua licença OFL; novos efeitos e ambientes CC0 têm manifesto em `Assets/Darklight/Resources/Darklight/Audio/AUDIO-LICENSES.json`. As músicas fornecidas pelo criador não recebem licença de redistribuição por estarem neste protótipo. Não incluir caches da Unity, builds, credenciais ou saves pessoais nos commits.
