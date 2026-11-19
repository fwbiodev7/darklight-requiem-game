# Retomada de 17/11/2026 — Menus 2.0

## Direção vigente

Ordem direta mais recente: **redesign de todos os menus → Combate 2.0/combos → bossfight Lucius 2.0 → demais frentes do Prompt Mestre**. O usuário autorizou começar agora e aprovou o loading. Esta ordem supera a precedência anterior de combate antes dos menus. Preservar validação e escopo de cada etapa; menus não significam conclusão automática de HUD/diálogos.

**Etapa atual: Menus 2.0 — em progresso.** Cota consultada no início desta etapa: ferramenta informou 100% semanal disponível e 98% da janela de cinco horas. Preservar 20% semanal e 10% da janela, sem perpetuar a exceção antiga de cota do loading.

### Ajustes diretos posteriores do usuário

- **Ordem final sobre build:** o usuário cancelou a build desta sessão e continuará nela amanhã. Não iniciar build aos25%; continuar nas artes e encerrar com commit local/relatório aos15% restantes. Esta ordem supera os itens históricos abaixo sobre build e sua inclusão no commit. Sem novos testes e sem push permanecem vigentes.

- **Última restrição: sem testes em tudo.** Não iniciar novas suítes EditMode/PlayMode, smoke tests ou percursos de validação desta rodada. Interromper execução pendente própria; preservar evidências anteriores sem transformá-las em validação das novas alterações. A compilação necessária à build explicitamente solicitada continua autorizada. Revisão de código não comprova runtime.

- **Não publicar nem executar testes dos menus:** o usuário fará a avaliação manual. Registrar menus implementados separadamente de validados; não alegar Console/QA aprovados para o redesign.
- Continuar o trabalho na ordem autorizada. Com **25% restantes da janela de cinco horas**, gerar também a build Windows. Com **15% restantes**, encerrar novos lotes, criar relatório de concluídos/pendentes e fazer **commit local conjunto** das mudanças prontas e da build, sem push. A reserva semanal de 20% permanece; se atingida antes, preservar checkpoint sem ultrapassá-la.
- A ordem específica de incluir a build no commit local supera a exclusão histórica de builds para esta entrega. Não incluir os MP3 privados, saves, caches, configurações alheias ou outros projetos. Conferir o método de armazenamento do artefato e seu espaço antes de versionar; não publicar grandes binários inadvertidamente.
- Música adicional fornecida para Lucius × Fabius: `4-10 It Has To Be This Way (Platinum Mix) [vs. Steven Armstrong].mp3`, instalada privadamente fora do checkout. Integração escrita por essential_recovery, aguardando revisão/testes focados de áudio, sem reprodução audível.

## Resultado encerrado: loading

- Duas animações de oito quadros, 50/50 por carregamento, independentes da lógica, pequenas no canto inferior esquerdo.
- Fundo gótico derivado das referências, texto corrigido dentro da moldura, fonte Point e barra carmesim ornamentada.
- Quatro testes únicos próprios aprovados; saves isolados, preferências restauradas, silêncio.
- GIFs reais fase 1 → fase 2 disponíveis em `outputs/loading-mascots-20261117/phase1-phase2-cat.gif` e `phase1-phase2-mini.gif` e mostrados ao usuário.
- GitHub main confirmado: loading `c56d829f0e14dd2d68f6252b45377e174633d0e5`; registro de Map Remaster `a4417789b387f52cfa915ca6be4dd5de93832e1c`.
- Publicação isolada em `work/loading-mascots-20261117/publish`, branch `codex/loading-mascots-publish-20261117`, sobre os commits concorrentes do site até `945a311`. Não resetar a raiz para alinhar hashes: os commits locais equivalentes são `89ed07c` e `41b5f99`.
- Relatório completo: `Docs/Darklight/LOADING-MASCOTES-RELATORIO-20261117.md`.

## Menus: base inspecionada

Runtime monta a interface em `DarklightCampaign.CreateUI`; `DarklightCampaignMenu` organiza principal/pausa e botões; `DarklightCampaignSettings` aplica polish/configurações; `DarklightAtlas` tem a tela de mapa independente. `DarklightMenuBuilder` importa os assets consumidos pelo runtime, sem prefab novo obrigatório.

Referências aprovadas acessíveis em Downloads: menu gótico principal, arena, arsenal, mapa, pausa e configurações, anexadas em 17/11. Reutilizar o fundo Vespera, logo oficial, fontes, sprites aprovados, ações e Input System; produzir somente elementos novos indispensáveis. Aparência: preto/violeta, molduras góticas finas, destaque carmesim, gemas e texto claro sem vazamento. Não colocar screenshots com textos/botões embutidos como interface funcional.

Auditoria auxiliar somente leitura de configurações/atlas/treinamento atribuída a `continuity_audit`; coordenação mantém única execução da Unity e Git.

## Preservação

Sem regenerar Fabius Normal/alterar pivôs, trocar combate/HUD durante a etapa de menus ou reintroduzir Moto/Serras. Skill antiga menciona Moto, mas Prompt Mestre novo restringe Arsenal a Katana e Punhos/Grevas. Manter compatibilidade de saves e dados ocultos.

Mudanças concorrentes da raiz preservadas fora da entrega: `.codex/config.toml` e dois caches Python. Fontes geradas do loading estão preservadas localmente em ArtSources, referências privadas fora da publicação.

## Map Remaster 2.0

Pedido salvo em `Docs/Darklight/MAP-REMASTER-2-PRIORIDADE-20261119.md`. Não iniciar antes de 19/11/2026 às 21:11 America/Sao_Paulo, nem ignorar as fases. Prioridade de planejamento da frente futura: estudo real das fontes → diagnóstico de dez mapas → piloto Rua das Cinzas → soluções sistêmicas → mapas 2–10. Nenhum estudo/auditoria de mapas foi feito para esse pedido nesta etapa.
