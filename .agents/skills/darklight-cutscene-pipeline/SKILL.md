---
name: darklight-cutscene-pipeline
description: Dirigir, construir e validar cutscenes 2D pixel art do DARKLIGHT na Unity, com Timeline, câmeras, skip, legendas e retorno seguro ao gameplay. Use na produção de uma cena; para selecionar acontecimentos da campanha, use darklight-campaign-cinematics.
---

# Cutscenes DARKLIGHT

Leia `Docs/Darklight/SKILLS-CONTEXTO-20261104.md` a partir da raiz do repositório; confirme a etapa autorizada. Consulte o trecho pertinente de `Docs/Darklight/Planejamento/PROLOGO-CINEMATOGRAFICO-20261103.md`, sem antecipar sua produção.

## Preparação

- Inspecione `Packages/manifest.json` e o lock atual, câmera, combate, diálogos e persistência. Pontos de entrada: `Assets/Darklight/Runtime/Campaign/DarklightCampaignNarrative.cs`, `DarklightCampaignDialogueUI.cs`, `DarklightProgress.cs` e `Assets/Darklight/Runtime/World/DarklightLabCamera.cs`. Eles são candidatos ao reuso; inspecione seus contratos antes de invocá-los.
- Produza um storyboard com planos, duração, ação, falas, efeitos, bindings, transição e estado final. Separe controles do tutorial jogável de controles bloqueados durante planos dirigidos.
- Timeline está declarada no manifesto de 04/11; isso não comprova resolução/importação. Cinemachine não está declarada. Use somente APIs da versão efetivamente instalada. Não instale um MCP para ter acesso a uma skill. Consulte `Docs/Darklight/SKILLS-FONTES-20261104.md` se precisar avaliar uma integração.

## Construção e validação

Reutilize o diretor/câmera existentes quando adequados. Ao usar Timeline, confira cada track, clip, Signal e binding do PlayableDirector; um arquivo `.playable` isolado não entrega uma cena. Cinemachine é opcional e condicionado à compatibilidade; preserve câmera ortográfica, enquadramento e pixel perfect. Não adote APIs 2.x em uma integração 3.x.

Capture o estado anterior de input, IA, câmera, timeScale, HUD e áudio. Faça conclusão, skip, cancelamento e troca de cena convergirem para uma restauração idempotente. Eventos de progressão devem ocorrer uma única vez; estabeleça explicitamente quais eventos serão confirmados ao pular. Use saves de teste e trate interrupção/load sem duplicar recompensas ou deixar controles presos.

Na infância, Fabius é controlável, com Lucius e espadas de madeira; use versões infantis completas e próprias. No duelo adulto, interrompa antes de confirmar um golpe fatal; o flashback inicia a campanha sem revelar o destino dos irmãos. Legendas devem ser legíveis e acompanhar skip/pausa.

Teste em uma cena representativa: término normal, skip no início/meio/fim, reentrada, pausa, load e transição interrompida. Registre capturas de enquadramento e continuidade. Respeite a autorização vigente de Play/áudio. Reporte storyboard, assets, bindings, alterações, evidência visual, execução real e pendências separadamente; preparação documental não é integração Unity.
