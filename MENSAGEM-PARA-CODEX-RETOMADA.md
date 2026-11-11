# DARKLIGHT: REQUIEM — Mensagem de Handoff Técnico para o CODEX
**Data da Passagem:** 10/11/2026 (Sessão Noturna)  
**Origem:** Antigravity  
**Destino:** Codex (ao reiniciar / renovar cota)  
**Status do Repositório:** Commit `b0dc1d8` enviado para as branches `codex/retomada-fabius-20261109` e `antigravity/lucius-integration-delivery-20261110`.  

---

### Caro Codex,

O trabalho técnico de estabilização visual, resolução de bugs de animação e validação de build foi concluído com sucesso total. Seguem os detalhes cruciais para que você continue o desenvolvimento sem refazer trabalho ou causar regressões:

---

### 1. O Que Foi Resolvido e Está 100% Pronto (NÃO REFAZER / NÃO REDESENHAR)

1. **Correção Definitiva do Tremor de Fabius:**
   - **Diagnóstico:** As matrizes de pixels estavam matematicamente corretas, mas os metadados de pivôs (`pivot.x` e `pivot.y`) nos arquivos `.json` e `.png.meta` continham heurísticas rascunhadas provisórias com variações de até 21px no Idle e até 144px no Walk.
   - **Solução Implementada:** Auditados todos os 962 quadros de Fabius Normal nas 108 folhas em `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`.
   - **522 quadros em 95 folhas** foram recalibrados com precisão subpixel:
     - **Idle:** Eixo $X$ e apoio $Y$ travados (**0.0px de oscilação**).
     - **Walk:** Pés ancorados na linha do chão e tronco alinhado à vertical física.
     - **Ataques:** Base ancorada no corpo, com VFX expandindo para fora sem puxar o personagem.
   - **ATENÇÃO:** Não regenere, não re-fatie e não altere os pivôs calibrados de Fabius Normal.

2. **Testes Automatizados de Estabilidade:**
   - Foi criado o teste `Assets/Darklight/Tests/EditMode/DarklightFabiusAnimationStabilityTests.cs`.
   - Total de 132/132 testes EditMode/PlayMode executados e aprovados.

3. **Build Windows Standalone x64 Funcional:**
   - A build está completa, sem erros, com arquivo de dados `data.unity3d` íntegro (1.85 GB):
     - Pasta: `outputs/build-windows-estabilidade-20261110/Windows/`
     - Executável: `outputs/build-windows-estabilidade-20261110/Windows/DarklightRequiem.exe`
     - Pacote ZIP: `outputs/build-windows-estabilidade-20261110/DarklightRequiem-Windows-Estabilidade-20261110.zip` (1.56 GB)
   - Adicionados os scripts de execução rápida no terminal:
     - `run-game.bat` (CMD)
     - `run-game.ps1` (PowerShell)

---

### 2. Referência Canônica Oficial para os Ataques de Lucius (DEFINIÇÃO DO USUÁRIO)

O usuário forneceu a referência canônica visual exata que deve guiar **todos os próximos ataques e combos de Lucius**:

- **ARQUIVO PRINCIPAL OBRIGATÓRIO:**
  `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png`
- **Referência Secundária:**
  `Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Secondary_20261110.jpg`

Esta imagem primária define:
- Postura de combate de Lucius com espada longa de lâmina prateada/aço escuro.
- Efeito luminoso azul nos arcos de corte e impacto da ponta da lâmina.
- Sequência de estocada frontal e cortes horizontais/diagonais.
- Anatomia, proporção dos membros e botas pesadas com fivelas.

---

### 3. Sistema de Lucius Atualizado no Código

Em `Assets/Darklight/Runtime/Character/DarklightLuciusSpriteBank.cs`:
1. Foram cadastradas as ações `"Draw"` (saque da espada) e `"WalkArmed"` (caminhada com espada na mão).
2. O método `TryPose` possui um fallback automático para que Lucius não congele em Idle se um ataque direcional (como `SwordThrust`) for solicitado para direções sem arte integrada.

---

### 4. Suas Próximas Tarefas Imediatas (Backlog Priorizado para o Codex)

1. **Ataques de Lucius (Estocada / Combos):**
   - Utilizando como gabarito `Lucius_Attack_Reference_Primary_20261110.png`, produzir as folhas de ataque restantes (especialmente `SwordThrust` nas direções prioritárias) e fatiar com pivôs consistentes no ponto de contato dos pés.
2. **Caminhadas de Lucius (8 direções):**
   - Concluir as duas variações: espada nas costas e espada empunhada (`WalkArmed`), mantendo passada baixa sem marcha militar.
3. **Animação de Saque (`Draw`):**
   - Lucius puxando a espada das costas e assumindo a postura de combate.
4. **Preservação:**
   - Manter os 132 testes passando e gerar o checkpoint ao se aproximar de 10% da cota.

Boa continuação! O projeto está estável, testado e jogável.
