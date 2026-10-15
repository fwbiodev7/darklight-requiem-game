# DARKLIGHT: REQUIEM

RPG 2D em pixel art de Fábio Ronaldo. **Prévia do remaster em desenvolvimento.**

Um eclipse abre fendas em Vespera. Fábio, Lyra e Kael atravessam a cidade e o Véu, enfrentando as consequências do Projeto Eclipse.

## Atualização de 14/10/2026

Protagonistas exclusivamente jovens, secundários no mesmo traço, três formas com figuras próprias, auras contínuas e entrada de transformação mais forte. Transformações liberadas desde o primeiro capítulo, com pelo menos três minutos de ação. Iluminação e sombras na campanha, novos efeitos de combate, correção de saltos laterais das animações, remapeamento de teclado/controle e telas de passagem com pequenas ilustrações do próximo destino.

Leia [as mudanças, evidências e pendências](Docs/REMASTER-20261014.md). O criador pediu para encerrar os testes e concluir o commit; não houve nova rodada após a integração final. Esta prévia não é uma edição definitiva certificada.

## Jogar

O download anterior é a [Alpha 0.1 para Windows](https://github.com/fwbiodev7/darklight-requiem-game/releases/tag/v0.1.0-alpha), com [guia próprio](ALPHA-0.1.md). Novos pacotes, quando disponíveis, aparecem em [Releases](https://github.com/fwbiodev7/darklight-requiem-game/releases). Extraia o pacote completo e abra `Windows/DarklightRequiem.exe`.

[Site do jogo](https://darklight-requiem.zfabiobrronaldo.chatgpt.site).

## Comandos da nova prévia

WASD / analógico esquerdo: mover; mouse esquerdo / X: leve; mouse direito / Y: pesado; Espaço / B: esquiva; Q / LB: parry; E / A: interagir; V / LS: trocar arma; T / LT / L2: transformar; R / RT / R2: elixir; Tab / Select: mapa; Esc / Start: pausa. O personagem acompanha a direção de movimento.

Em Pausa → Transformações, escolha a forma. Em Pausa → Controles, ajuste atalhos por dispositivo, resolva conflitos ou restaure os padrões. Os comandos podem diferir da Alpha 0.1 publicada.

## Abrir na Unity

Unity **6000.6.0f1**. Dependências em `Packages`. Cena: `Assets/Darklight/Scenes/DarklightCampaign.unity`. O projeto usa URP 2D. Importar e compilar não comprova gameplay ou desempenho.

O modo temporário `--requiem-playtest` preserva a campanha. Ferramentas de captura e diagnóstico só são iniciadas explicitamente; não reproduzir áudio ou executar testes sem autorização do criador.

## Recursos e licenças

Figuras completas e metadados de importação preservados. Press Start 2P acompanha sua licença OFL; efeitos e ambientes CC0 têm manifesto em `Assets/Darklight/Resources/Darklight/Audio/AUDIO-LICENSES.json`. Músicas fornecidas pelo criador não recebem licença de redistribuição por estarem no protótipo. Não incluir caches, credenciais, builds ou saves pessoais nos commits.
