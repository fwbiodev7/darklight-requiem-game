# DARKLIGHT: REQUIEM — encerramento de 17/11/2026

## Direção final do usuário

Menus → Combate 2.0/combos → bossfight Lucius → demais frentes do Prompt Mestre. Posteriormente: **sem novos testes em tudo, sem publicação desta entrega, sem build nesta sessão**; continuar nas artes e fazer commit local/relatório perto de **15% restantes da janela de cinco horas**, preservando 20% semanal. A build antes pedida aos25% foi cancelada; o usuário continuará nela amanhã. Não iniciar build ou testes automaticamente na retomada.

## Concluído e aprovado: loading

- Dois mascotes de oito quadros: Gato Ruptura com lã e Mini Fabius caminhando. Sorteio50/50 uma vez por exibição; tempo não escalado, sem atrasar carregamento, sem objetos novos por quadro e parada ao esconder.
- Fundo gótico inspirado nas referências, textos contidos na moldura, fonte Point e barra carmesim com moldura/reflexo/cristal. Estrutura e lógica existentes preservadas.
- Quatro testes únicos próprios aprovados antes da restrição posterior; GIFs reais Rua das Cinzas → Distrito do Vinil enviados ao usuário, que aprovou o resultado.
- Publicação anterior confirmada: `c56d829f0e14dd2d68f6252b45377e174633d0e5`; documento de Map Remaster em `a4417789b387f52cfa915ca6be4dd5de93832e1c`. Ver `LOADING-MASCOTES-RELATORIO-20261117.md` para fontes, hashes e evidências.

## Menus: implementados, avaliação manual pendente

Reutilizados Canvas, botões/handlers, Input System, logo, cenário Vespera, fonte e molduras9-slice nativas. Não foram usados screenshots completos como UI.

- Principal com sete entradas: continuar, novo jogo, carregar, arena, configurações, créditos e sair.
- Pausa reorganizada com continuar, arsenal, mapa, personagem/registro, combate/treino, configurações e menu principal; funções adicionais preservadas no submenu de personagem, em vez de uma lista que ultrapassava a tela.
- Molduras violetas, foco carmesim, cabeçalhos/rodapé e áreas separadas para seleção e detalhes aplicados aos painéis existentes, incluindo configurações, controles, visual, diário e treinamento.
- Arsenal mostra Katana e Punhos/Grevas, com arte existente e atributos reais da campanha. Rotas visíveis de Moto/Serras removidas, preservando dados e arquivos antigos.
- Mapa/atlas recebeu layout e molduras góticas; viewport continua396×224, com projeção, geometria, fog, coordenadas, navegação e persistência preservados. Isto não é Map Remaster físico.

**Não foram executados testes dos menus.** A validação de layout, navegação e legibilidade fica com o usuário, conforme sua ordem; não declarar o redesign validado ou publicado.

## Combate 2.0: incremento técnico, etapa ainda parcial

- Seleção pública limitada aos IDs0(Katana) e2(Punhos); IDs1/3 legados migram para0 sem apagar unlocks/histórico. Restore não reativa a moto no capítulo9; ciclo de armas não a seleciona.
- Controles visíveis e atalhos de UI para sistemas retirados desativados. Contador obrigatório do capítulo9 atualizado para duas armas, coerente com o gate; nenhuma geometria/mapa/história foi reconstruída.
- Ramo leve→leve→pesado da Katana agora lança, reutilizando sua animação aprovada. Finalizador aéreo descendente próprio utiliza movimento completo existente, com reação Slam, respeitando a imunidade de relançamento e o limite de juggle existentes.
- Banco aéreo passa a ser por loadout: golpes de Punhos reutilizam soco/chute/giro completos correspondentes, sem usar a animação de uppercut como chute. A troca em recovery preserva modelo/alvo/altura/contatos; validação completa do híbrido ainda está pendente.
- Cadeias terrestres, ramos por pausa, defesa, hitboxes e perseguição existentes foram reaproveitados; não são todos sistemas criados nesta rodada. Não declarar Combate2.0 globalmente encerrado.

## Artes novas de Punhos

Piloto frontal S com quatro Idle e quatro Walk, preservando os arquivos/pivôs anteriores. Usuário aprovou expressamente a caminhada frontal com guarda alta; seus quatro frames foram preservados pixel a pixel na seleção final.

Seleção: `ArtSources/FistsCombatStance20261117/selected/FabiusFistsCombatS-selected-v1.png`.

SHA256: `202f33f42d937ab55cc5cc2162b41c8d1881ea3ea329e8d18e04c895de70740e`.

Idle revisado estaticamente: joelhos/apoios articulados, guarda alta e pulinho de6px (~1,55% do corpo), chão/pivô de referência constante. Transparência RGBA confirmada: o marrom aparente fica no RGB oculto, sem fundo amplo no alpha; nenhum apagamento/recoloração foi aplicado à caminhada aprovada. Fontes, prompts, hashes, aprovação e GIFs preservados.

**Ainda não integrado em Assets/runtime.** Reprodução contínua não foi inspecionada; manifesto mantém esse estado explícito. Sete direções restantes, calibragem de importação e conexão ao personagem continuam pendentes. Não substituir o conjunto funcional por oito direções incompletas. Máxima fidelidade ao rosto, corpo, cabelo, pele, sobretudo e escala do Fabius aprovado continua obrigatória.

## Música do duelo Lucius × Fabius

Integrada a faixa fornecida **It Has To Be This Way (Platinum Mix)** à fonte de evento/mixer existentes, sem AudioSource paralelo. Prioridade específica de Lucius (distinto de Lucien), loop, pausa/retomada, persistência entre fases do mesmo duelo, reinício em novo duelo e cleanup. Temas de skin/transformação cedem à prioridade; modo Silent não reproduz.

MP3 instalado privadamente em `C:/Users/Usuario/AppData/LocalLow/Fabio/DARKLIGHT REQUIEM/PrivateMusic/LuciusBoss.mp3`; não pertence a Assets/Git/build. SHA256: `f276067f7d821f4d71fd45f335f714605fd1f5a1e38784666c17bf9301f7d2ae`. Manifesto privado em `work/retomada-20261117/lucius-music.private.json`.

## Evidência anterior à suspensão de testes

A rodada EditMode já havia encerrado quando o usuário mandou parar todos os testes; nenhuma execução nova foi iniciada depois. XML: `outputs/retomada-20261117/combat20-edit-20261118T010555.xml` (dataUTC; sessão local17/11).

**38 aprovações e4 falhas**, sem skips:17 expansão aérea,10 perfis/combos,4 música (incluindo decodificação privada sem tocar),4 migração/escopo e3 técnicas de arma passaram. As4 falhas são da fixture `DarklightAirControllerTests` em SetUp: reflexão buscava campo `body`, mas o campo real é `_body`. O nome foi corrigido por leitura de código, **sem reexecutar**. Portanto o híbrido aéreo e os quatro casos dessa fixture não estão certificados. Saves preservados e preferências restauradas segundo o receipt.

Alterações posteriores, inclusive contador/controles e esta correção da fixture, não receberam testes. Não houve audição, PlayMode novo de duelo, smoke test ou build desta entrega. Compilação anterior não comprova runtime final.

## Pendências e retomada exata

1. Avaliação manual dos menus pelo usuário; corrigir apenas problemas concretos relatados.
2. Conferir loop frontal, importar/calibrar o piloto sem alterar pivôs antigos e completar sete direções de Idle/Walk com a identidade aprovada. Não refazer WalkS aprovado.
3. Concluir restante de Combate2.0, respostas aéreas de bosses vulneráveis e integração dos híbridos/transformações. Os testes suspensos só retomam mediante nova autorização.
4. Bossfight Lucius2.0 (IA/dificuldade justa/respostas aos combos) ainda não iniciada; música é uma entrega separada, não prova de bossfight concluída.
5. Build Windows adiada pelo usuário; não existe build nova a versionar nesta rodada.
6. Map Remaster2.0 salvo em `MAP-REMASTER-2-PRIORIDADE-20261119.md`: não antes de19/11/2026 às21:11 Brasília, dentro da fase autorizada. Estudo das fontes e auditoria física dos dez mapas não iniciados; falha histórica de percurso no capítulo0 continua pendente.

Entrega final somente local na branch `codex/menus-combat-20261117`, sem push. Recibo final de commit/saldo em `work/retomada-20261117/closing-receipt.json`. Fora do commit: MP3 privado, saves, caches, `.codex/config.toml`, candidatas artísticas rejeitadas e originais privados. Nenhuma geração permanece em voo. Consultar também `CHECKPOINT-MENUS-20261117.md` e os banners mais recentes do Prompt Mestre.
