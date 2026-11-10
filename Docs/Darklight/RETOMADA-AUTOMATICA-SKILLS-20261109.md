# Retomada automática das skills — 09/11/2026

O saldo semanal foi renovado: 75% disponível no início; janela de cinco horas com 44% disponível. As sete skills criadas em 04/11 aparecem agora no catálogo desta sessão. Checkpoint anterior, fontes, backup e commit `6cd5902` continuam preservados.

## Trabalho desta retomada

Aplicada `darklight-animation-qa`, com revisão independente somente leitura por `skills_forward_qa`.

- Partículas: `DarklightActionParticles` usa ordem fixa 900, enquanto `DarklightSorting.OrderForY` usa base3000 menos Y×100. Esse desacoplamento pode ocultar efeitos por atores/foreground conforme a posição. Foi preparado candidato ancorado ao chão, recalculado também na reutilização do pool.
- Transparência: PNG/descriptor FastWeaponDodge conferem por SHA. Material de personagem usa corte alpha0.65. Na fase inicial de Punhos, sobrevivem14940 pixels nesse corte, contra20463 no corte0.05; isso mede cobertura de alpha, não percepção em gameplay. Candidato limita o override0.05 aos efeitos, sem alterar materiais/personagens/arte/tempos/opção de flashes reduzidos.
- Foram preparados testes de pool/profundidade e resolução das caminhadas de Punhos nas oito direções, com capturas previstas, mas NÃO EXECUTADOS.
- Caminhada SW V24: retângulos590×376, PPU164.89899 e pivôs consistentes entre fonte e metadata. A cadência antiga de0.6s é caminho-base; o resolver ativo usa durationMs do descriptor. Não há defeito de desenho comprovado para justificar regeneração. Recibos históricos não substituem captura do caso relatado na build correta.

## Concorrência identificada e preservação

O chat ativo **Atualize do commit e salve designs**, id`01a1c20c-7dd2-78b3-bf7b-bc3a1a5bd601`, recebeu instrução direta mais recente do usuário e já integra recuperação de Fabius no mesmo workspace. Sua Unity PID16256 iniciou às17h49, em batch silencioso, com testes EditMode. Nosso preflight detectou o lock e NÃO iniciou outra Unity.

A execução observada desse outro coordenador terminou com erros de API na recuperação dos testes essenciais (`presentation`, `guardContact`, `Observe` com4 argumentos, `MotorcycleGuardBursts`, `TryMotorcycleGuardContact`). Isso não é resultado dos testes novos desta retomada, que nem começaram. A investigação/correção da recuperação fica com o coordenador já ativo.

Para evitar integração e Git simultâneos, o candidato foi preservado em `outputs/retomada-20261109/heartbeat-skills/candidate/` e `particle-depth-candidate.patch`, com estado e hashes no recibo. SOMENTE alterações próprias não validadas foram retiradas dos arquivos ativos quando o hash ainda coincidia exatamente; qualquer alteração concorrente foi preservada. Consulte `heartbeat-skills/continuation-state.json` para o resultado real. Não aplicar os arquivos completos sobre trabalho novo: revisar/adaptar somente o diff necessário.

Sem novos sprites, importação própria, execução PlayMode, screenshots de runtime, build ou aprovação visual. Nenhum Git/commit/push desta retomada, pois há outro coordenador usando a mesma árvore; este documento e candidatos ficam salvos localmente para a integração única. Não confundir esse estado com o push confirmado de04/11.

## Continuidade

Após a recuperação/compilação sob o coordenador ativo, revisar o candidato de partículas contra o código mais recente e executar quatro casos focados: `ActionParticlesFollowGroundDepthWhenPoolSlotsAreReused`, `ReviewedFistsWalkFramesResolveInEightDirections`, `ActionParticlesEmitOnceForRealStatesAndRespectLimits` e `WeaponDodgeParticlesAreDistinctAndExpireQuickly`, da classe `DarklightWeaponRedesignGameplayTests`. Manter áudio desativado, saves isolados, guardas e uma única Unity. Inspecionar capturas antes de concluir visibilidade/alinhamento.

A retomada automática única de04/11 foi consumida. Sua automação deve ser removida para não duplicar o trabalho que o usuário já retomou no outro chat. O desenvolvimento do jogo continua pendente; esta remoção não significa conclusão global.
