# DARKLIGHT: REQUIEM — correções verificadas em 13/10/2026

## Resultado
- Idade visual 0–4, skin e arma equipada preservadas na seleção final dos sprites e nas trocas sequenciais. Removidos fallback para folha jovem e fallback de Adagas/Corrente para Katana. Animações completas aprovadas têm prioridade.
- Importadas 29 folhas/432 quadros completos de caminhada, com fonte PNG preservada, filtro Point, sem mipmaps e pivôs de apoio. Vistas rejeitadas não são selecionadas nem publicadas. Cabeça, corpo e arma permanecem juntos.
- Ações ainda sem folha própria mantêm uma pose inteira da mesma idade, skin e arma. Isso evita rejuvenescimento, mas não constitui uma animação nova de ataque, defesa ou esquiva. A cobertura artística completa continua pendente.
- Deslizamento tangencial conectado à movimentação do jogador, segundo cast para deslocamento residual, controle de origem sobreposta a parede e material sem atrito reutilizável. Tamanhos/offsets dos colliders e limites dos mapas preservados.
- Material de física recriado quando o Unity descarrega seu objeto nativo. Caminhadas existentes não são substituídas por poses paradas.
- Menu de áudio com sete volumes 0–100%, temas Rock/Réquiem com transições, nove superfícies e 82 efeitos originais integrados. Música do primeiro despertar preservada. Testes silenciosos.

## Arquivos principais
`DarklightCharacterView.cs`, `DarklightUserSpriteBank.cs`, `TodayCharacterCatalog.cs`, `TodayCharacterImporter.cs`, `DarklightNavigation.cs`, `DarklightFighter.cs`, `DarklightPaintedMap.cs`, `DarklightCombatLabBuilder.cs` e testes de idade/armas, colisões, proporções e catálogo. Fontes de áudio e documentação constam em AUDIO-EXPANSION-20261013.md.

## Evidência
- 85 verificações EditMode aprovadas: 84 na suíte completa e uma reconferência do catálogo; verificações finais de contato/deslizamento 3/3.
- Suíte PlayMode final: 87/87 aprovados, incluindo 4.800 combinações de identidade, trocas reais das três armas, combate, transformações, checkpoints, finais e rotas físicas dos dez mapas. Após o último ajuste de contato/material: rotas e parede sólida reconferidas 2/2.
- Executável Windows gerado com Unity 6000.6.0f1. Build Succeeded, 11 avisos de APIs antigas/Pipeline desativado no Player. O único erro contado no relatório é o timeout da chamada de controle do Editor após 5 segundos; nenhuma falha de compilação do jogo.
- 672 PNGs capturados do executável em ensaio controlado. Zero cabeças separadas/erasure; saves preservados. 272 capturas usam uma pose de fallback da idade/arma correta, explicitamente registrada no manifesto de QA.
- Projeto já registrado no Unity Hub como darklight-requiem-main, Unity 6000.6.0f1, Built-in. Integridade verificada sem erros/avisos; DarklightCampaign.unity é a primeira cena habilitada.

## Limites reais
Não foi realizada uma partida manual completa, teste com controle físico ou avaliação auditiva. Capturas e controles de poses são ensaios automatizados. Algumas ações envelhecidas/especiais/poderes ainda precisam de novas folhas completas; não gerar personagens por partes. O retrato original do HUD foi preservado. A build local corrigida não deve ser confundida com o download público anterior da Alpha 0.1.

Nenhum som de teste ou aviso de conclusão foi reproduzido. Fontes, saves, histórico e projetos alheios preservados.
