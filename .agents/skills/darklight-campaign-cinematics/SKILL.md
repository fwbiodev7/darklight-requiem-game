---
name: darklight-campaign-cinematics
description: Mapear o GDD do DARKLIGHT aos acontecimentos das dez fases e selecionar cutscenes justificadas, transições e estados narrativos. Use para integração narrativa da campanha; construção de uma cena específica pertence a darklight-cutscene-pipeline.
---

# Campanha e cinematografia

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz e só as seções pertinentes de `Docs/Darklight/Planejamento/GDD-COMPLETO-20261103.md`. Consulte o plano do prólogo quando necessário. A ordem atual está no contexto de 04/11, acima dos anexos históricos.

## Acontecimentos reais

Confronte GDD com `Assets/Darklight/Runtime/Campaign/DarklightCampaignNarrative.cs`, `DarklightCampaign.cs`, `DarklightProgress.cs`, `DarklightProgressExpansion.cs`, diálogos e mapas existentes. Confirme como chapter/region/encounter representam dez fases antes de criar IDs ou alterar saves. Documento não equivale a implementação.

Produza matriz por fase: acontecimento/fonte, condição de entrada, cena/mapa, participantes, objetivo jogável, legenda, transição, flag persistente, conclusão, replay/skip e estado implementado. Se faltar conteúdo canônico, marque lacuna sem inventar revelações.

Selecione cutscene somente se comunicar acontecimento que gameplay/diálogo curto não transmitem adequadamente. Reutilize cenas/diálogos funcionais. Não refaça mapas para cinematografia nem crie uma cena por fase apenas para completar contagem.

## Continuidade e persistência

Preserve infância/tutorial controlando Fabius → confronto adulto interrompido antes de confirmar golpe fatal → flashback/campanha → reencontro futuro. Mantenha o mistério dos irmãos e de O Encapuzado; não revele essa identidade por HUD, fala, recompensa ou nome técnico exposto ao jogador.

Defina flags idempotentes e saves antes/depois das transições. Skip preserva o estado acordado sem duplicar recompensas; load/reentrada não repetem eventos obrigatórios indevidamente. Diferencie progressão, replay voluntário e tutorial que devolve controle.

Use `darklight-cutscene-pipeline` ao produzir cenas selecionadas. Revise contratos Antigravity sem duplicar engenharia de suporte; arte, edição de Timeline e montagem final ficam sob coordenação Codex.

Valide passagem representativa e persistência antes de propagar: evento normal, skip, save/load e mudança de fase. Relate cobertura das dez fases, cenas justificadas, fontes, lacunas e execução real. Matriz/storyboard são planejamento, sem alegar campanha integrada ou build concluída.
