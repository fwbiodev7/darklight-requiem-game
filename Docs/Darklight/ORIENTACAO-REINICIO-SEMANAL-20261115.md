# Orientação de reinício semanal — DARKLIGHT: REQUIEM

Registrada em 15/11/2026 por solicitação do usuário. Não iniciar as fases nesta rodada de encerramento.

## Autoridade e sequência

Ler esta orientação, AGENTS.md, o último checkpoint técnico e `weekly_reset_prompt.md` antes de iniciar qualquer tarefa após o reset semanal. O Prompt Mestre existente contém as oito fases: seguir invariavelmente Fase 1 → Fase 2 → … → Fase 8. Esta sequência supera a ordem anterior de HUD primeiro. Não avançar até a fase atual estar concluída, validada e autorizada.

## Referências

Pasta indicada pelo usuário:
`C:/Users/Usuario/.gemini/antigravity/brain/0c83ce2f-f70c-4779-8479-2a03050e3bdb/referencias_pos_reset_codex/`

Consultar o README.md dessa pasta para identificar cada referência e o novo layout de HUD. Ao iniciar cada fase, inspecionar e importar os sprites, layouts e mockups pertinentes, garantindo sua aplicação nos assets correspondentes. Preservar os originais e registrar o que foi consultado, importado e aplicado; não confundir mockup com asset pronto para gameplay.

Verificação em 15/11: pasta existente, com oito arquivos JPG; README.md ausente. O arquivo `darklight_test_report.json` também não foi localizado no workspace nesta verificação. Rechecar na retomada; não inventar seu conteúdo. Evidência técnica disponível: `Docs/Darklight/RELATORIO-TESTES-FINAIS-20261115.md` (70 testes únicos aprovados, uma falha de percurso pendente no mapa 0).

## Checklist antes de modificar arquivos

1. Ler AGENTS.md e o último checkpoint técnico.
2. Consultar a cota real por ferramenta disponível; informar se não for possível.
3. Confirmar que ocorreu o reset semanal, distinguindo-o do reset de cinco horas.
4. Inspecionar branch, commits, alterações locais e backups, preservando trabalho alheio.
5. Checar espaço livre; recomendado pelo menos 2 GiB.
6. Abrir o Prompt Mestre e as referências antes de implementar.

## Economia e execução

- Manter pelo menos 20% da cota semanal; esta reserva substitui limites históricos conflitantes.
- Reutilizar assets existentes e evitar alterações massivas sem ganho perceptível.
- Atualizar o status da fase corrente no início real, por exemplo “Fase 1 — Em progresso”. Estado neste registro: aguardando reset semanal; nenhuma fase iniciada nesta rodada.
- Antes de alterações estruturais, executar os testes automatizados associados, consultando `darklight_test_report.json` quando disponível e os relatórios reais existentes.
- Preservar a build funcional e Fabius Normal/pivôs; manter as proteções de saves e as restrições de áudio vigentes.
- Encerrar com checkpoint antes de comprometer a reserva. Não iniciar fase seguinte sem conclusão, validação e autorização.

Este registro é uma instrução persistente para a retomada, não uma declaração de agendamento ou de execução automática após o reset.
