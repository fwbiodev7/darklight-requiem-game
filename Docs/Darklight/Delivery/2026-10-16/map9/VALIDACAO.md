# Fase 9 — Véu Profundo / campo de batalha devastado

Integrada e validada individualmente antes de avançar para a fase 10. Quatro peças originais, sobreposição 384×256, costura dura sem suavização; base1536×1024 e detalhe3072×2048 em quatro partes. Fontes e hashes em chunks/; originais em backup/. GUIDs preservados.

Arquivo queimado, sepulturas, acampamento de cinzas, destroços de cerco e cratera de Kael têm marcos próprios. Piso dessaturado conserva a leitura dos ataques; nuvens rubras ocupam o abismo. Há retorno pelo oeste e conexão oriental direta. Geometria traçada sobre a arte final, com junções corrigidas para a cápsula.

`final-validated-tests.xml.summary.json`: **6 aprovados, 0 falhos/ignorados; saves preservados e preferências restauradas**.

- 48 percursos físicos nos dois sentidos, 21.773 passos de cápsula, raio0,3; cinco estados de checkpoint carregados em pontos seguros. 25 âncoras/junções também verificadas offline com margem8px.
- Três selos avançam os checkpoints0→1→2→3, com inimigos em chão válido.
- Encontro real único de Kael, três fases e pausa; interação de purificação mantém Kael vivo e registra checkpoint4/flag original.
- Três armas originais atingem em oito direções (24 contatos).
- Quatro detalhes Point/sem mipmaps, oito capturas reais da câmera/HUD em after/, inspecionadas. Comparação por função da missão em comparacao-map9.png.
- Evento ambiental opcional, repetível, limitado e pausável; cama/cue existentes distintos, nenhuma música baixada. Testes silenciosos confirmam nenhuma reprodução.

Primeira rodada:5 aprovados/1 falho no teste ambiental porque o combate afastava o jogador da interação. O teste agora isola o evento da IA e retorna à posição da interação; nenhum comportamento de combate foi alterado para satisfazê-lo. Evidência original preservada em map9-tests.xml.

Limites: não é aprovação humana de dificuldade/diversão, nem medição de FPS. A nova build será gerada após a fase10; a build1.0.5 ainda não contém estas reformas. Não houve mudança de formato dos saves ou história.
