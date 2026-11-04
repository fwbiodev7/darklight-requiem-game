# CUTSCENE ENGINEERING — DARKLIGHT: REQUIEM
**Data**: 04/11/2026  
**Auditor**: Antigravity Technical Support Engineering  
**Destino**: `work/antigravity-cutscenes/`

## 1. Princípios da Infraestrutura

O sistema técnico de cutscenes foi projetado para permitir que o Codex monte cinematics sem acoplar a lógica à Unity Scene principal ou gerar dependências pesadas de Timeline antes da hora.

### Componentes Entregues:

1. **`ICutsceneSequence`**: Contrato base para qualquer sequência cinemática, definindo `Begin()`, `Tick()`, `Skip()`, `End()` e `Validate()`.
2. **`CutsceneManager`**: Gerenciador do ciclo de vida cinemático (`Idle`, `Starting`, `Playing`, `Paused`, `Skipping`, `Ending`, `Error`), com proteção de reentrância e tratamento seguro de exceções.
3. **`CutsceneInputGate` / `DarklightInputGate`**: Bloqueia os inputs de gameplay de forma atômica e restaura o controle ao finalizar, inclusive com `ForceRestore()` em caso de morte do jogador ou transição de cena.
4. **`CutsceneEventBridge`**: Sistema pub/sub desacoplado que registra eventos (`CutsceneEventType`) e preserva o histórico de disparo.
5. **`CutsceneSkipController`**: Garante que eventos obrigatórios (concessão de poderes, checkpoints, atualizações de missão) sejam disparados instantaneamente mesmo se o jogador pular a cutscene.
6. **`CutsceneCheckpointBridge`**: Interface para vincular o progresso narrativo e cutscenes já assistidas ao save do jogo.
7. **`ICutsceneCamera`**: Contrato para posicionamento de câmera, foco em atores, zoom e screen shake.
8. **`CutsceneSequenceValidator`**: Validador preventivo que checa dependências e referências nulas antes do início da execução.
9. **`CutsceneTests`**: Suíte de 18 testes unitários que validam todo o ciclo de vida, skip, cancelamento por morte, restauração de input e idempotência sem necessitar da Unity aberta.

