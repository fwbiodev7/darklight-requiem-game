# DARKLIGHT: REQUIEM — campanha e desenhos oficiais

## Elenco e referências

As referências principais são [Lady](References/CampanhaOficial20261119/Lady.png), [Matriarca Rubra](References/CampanhaOficial20261119/Matriarca-Rubra.png) e [Nero](References/CampanhaOficial20261119/Nero.png). As três foram copiadas byte a byte. O [plano recebido](References/CampanhaOficial20261119/Plano-enviado.md) conserva os nomes do rascunho. Os nomes públicos vigentes são **Lady**, **Nero** e **Nero Fraturado**.

Lady investiga as rupturas desde o primeiro mapa e pode desenvolver uma relação com Fabius. O piloto frontal fica 4% abaixo de Fabius Normal, com o pivô dos pés preservado. Grimório e pistola são parte da arte. Nero tem a mesma altura corporal. A Matriarca mede cerca de 8% a mais que Fabius, sem contar o halo. Os três pilotos estão importados para apresentação. A Lady usa seu piloto como NPC, Nero como aliado e a Matriarca durante a apresentação em arena. Os retratos usam os desenhos oficiais. As duas lutas ganharam um conjunto de movimento, guarda, preparação, contato e recuperação. Os quadros preservam a escala da apresentação e usam os avisos e tempos de combate existentes.

## Campanha jogável

| Mapa | Fechamento |
| --- | --- |
| 1 — Rua das Cinzas | Ritual dos Três Selos e luta breve com Crawler e Hunter |
| 2 — Distrito do Vinil | Patrulha com atirador e caçadores |
| 3 — Catedral do Sangue | Matriarca Rubra, primeira luta de chefe |
| 4 — Laboratório Eclipse | Vigília com blindado e atiradores |
| 5 — Jardim dos Ecos | Emboscada com espectro e caçadores |
| 6 — Observatório Vespera | Guardiões do observatório |
| 7 — Cripta dos Primeiros | Guardiões e passagem após as memórias |
| 8 — Catedral do Crepúsculo | Última defesa do abrigo |
| 9 — Véu Profundo | Nero Fraturado, segunda luta de chefe. A purificação o torna aliado |
| 10 — Coração do Eclipse | Caçada das Fendas em dois grupos. O portal leva a Lucius Normal e Arconte, chefe final |

O Ritual dos Três Selos tem uma inscrição no cenário que informa a ordem. Uma combinação incorreta reinicia a sequência. Os inimigos surgem depois da solução. O checkpoint de conclusão e o portal usam o sistema existente da campanha. O despertar de Fabius continua possível no último combate do mapa inicial. Espelhos, Corredor dos Ecos e a prova cooperativa de Nero ficam para depois da avaliação do piloto.

## Nocthar e saves

O último encontro por mapa usava uma soma entre o índice do mapa e o enum dos chefes. A rota agora enumera explicitamente Matriarca e Nero. Os recursos históricos de Nocthar e dos demais chefes continuam disponíveis para referência. Não há gatilho, objetivo, diálogo, desbloqueio ou treino ativo de Nocthar. Seu antigo checkpoint de combate retoma a Caçada das Fendas. Um checkpoint com portal já liberado continua permitindo o duelo com Lucius. A leitura normaliza apenas a fase de chefe em mapas de patrulha e o índice de onda final. Capítulos, mapas, relíquias, escolhas, retorno e os identificadores técnicos de Kael foram mantidos.

## Verificação

- **EditMode:** 11 aprovados. A nova rota foi conferida nos dez mapas e os pontos antigos de Nocthar foram exercitados com o parser real de saves.
- **PlayMode:** 9 aprovados. O ritual rejeitou uma ordem errada, ativou os guardiões após a solução e salvou o portal. A Matriarca entrou na arena, Nero percorreu suas três fases e virou aliado após a purificação, as duas ondas finais liberaram o portal e o duelo de Lucius foi concluído nas duas posturas.
- Os testes usaram uma instância da Unity, modo silencioso e armazenamento isolado. Os recibos confirmam a preservação dos saves reais e a restauração das preferências.

Arquivos principais: `DarklightCampaignRoute.cs` para rota e ritual, `DarklightCampaign.cs` para disparo dos confrontos e colocação dos atores, `DarklightCampaignNarrative.cs` e `DarklightProgress.cs` para textos, e `DarklightProgressExpansion.cs` para a compatibilidade de saves. Os três sprites de apresentação estão em `Resources/Darklight/OfficialCast/`. Os logs e XMLs completos estão em `../work/campanha-oficial-20261119/`. Nenhuma build, commit ou push foi executado nesta rodada.

A repetição focada passou ao executar a entrada dos oito portões intermediários, dos quais apenas o de Nero gera chefe. Somada aos testes de ritual e Matriarca, ela cobre o despacho dos dez mapas. Na medição final ficaram **35% da janela de cinco horas** e **30% da semanal**.
