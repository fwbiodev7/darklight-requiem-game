# HANDOFF ANTIGRAVITY — 26/10/2026

**Documento de transferência para o Codex**

## 1. Repositório e Setup

| Item | Valor |
|---|---|
| Repositório | `https://github.com/fwbiodev7/darklight-requiem-game.git` |
| Clone local | `C:\Projetos\DARKLIGHT-ANTIGRAVITY` |
| Branch | `antigravity/apoio-redesign-20261026` |
| Base remota | `origin/codex/expansao-aerea-moto-20261023` |
| Commit inicial | `b719c527c736009e3ff560fd124ad07f67ae30d0` |
| Unity | `6000.6.0f1` |

## 2. Documentos Criados

- `Docs/Darklight/AUDITORIA-ANTIGRAVITY-20261026.md` — auditoria completa do clone
- `Docs/Darklight/HANDOFF-ANTIGRAVITY-20261026.md` — este documento
- `Docs/Darklight/REDUCAO-ESCOPO-ANTIGRAVITY-20261026.md` — proposta de simplificação

## 3. Commits Gerados

| Hash | Descrição | Arquivos |
|---|---|---|
| 594dbd6 | docs: auditoria inicial do clone, handoff e proposta de reducao de escopo | Docs/Darklight/ |

## 4. Estado do Clone vs Fechamento de 26/10

### Ausente no clone (depende do Codex)
- Simplificação de Fabius (36 figuras novas selecionadas em 26/10)
- Migração de saves implementada em 26/10
- Documentos FECHAMENTO/ESCOPO/PENDENCIAS/AUDITORIA-GIT de 26/10
- Gates finais e build de 26/10
- Testes PC04/PC06 aprovados em 26/10
- Qualquer WeaponRedesign20261026/

### Presente no clone
- Estrutura completa do projeto Unity 6000.6.0f1
- 144 scripts C# de runtime (Campaign/Character/Combat/Input/World)
- 112 scripts de teste (51 EditMode + 61 PlayMode)
- 76 PNGs em WeaponRedesign20261023 (Resources)
- 154 PNGs em RuinFists (Resources)
- ArtSources até 25/10 (incluindo retomadas 1350 e 1850)
- Outputs de redesign até 25/10
- coverage.json com 76 contextos / 275 figuras
- Sistema de armas: Katana (0), Daggers/Presas (1), Chain/Corrente (2), AbyssRider (3)
- Sistema de skins: Réquiem (0), Rock (1)
- Transformações: Normal (0), Ruptura (1), Eclipse (2), Absoluto (3)
- WorldVisualScale = 1.325f confirmado

## 5. Arquitetura de Código Identificada

### Enums e IDs
- `DarklightWeaponId`: Katana=0, Daggers=1, Chain=2, AbyssRider=3
- Skins: 0=Réquiem, 1=Rock
- Formas: 0=Normal, 1=Ruptura, 2=Eclipse, 3=Absoluto

### Sistemas Principais
- `DarklightWeaponController` — troca de armas, unlock por capítulo
- `DarklightWeaponRedesignBank` — banco de sprites redesign com catálogo JSON
- `DarklightCampaignSkins` — alternância Réquiem/Rock
- `DarklightCampaignTransformations` — sistema de transformação com timer 180s+
- `DarklightRuinFistsBank` — banco específico dos Punhos da Ruína
- `DarklightMotorcycleRedesignBank` — banco da moto
- `DarklightCharacterView` — renderização com WorldVisualScale=1.325f

### Dependências Rock/Skin 1
- `AbyssRider` (weapon 3) requer `SelectedSkin==1` (Rock)
- `SelectSkin(1)` requer rockIdle/rockMotion/rockActions/rockSlash completos
- Rock está integrado em saves (`PlayerPrefs.Skin`), armas e combate

## 6. Conflitos Identificados

- AGENTS.md mantém Presas/Rock como ativos; contrato canônico atual os desativa
- `DarklightWeaponId.Daggers` (Presas) continua no enum e no sistema de unlock
- `DarklightWeaponId.Chain` (Corrente) existe no enum mas não é arma canônica atual
- AbyssRider exclusivo de Rock — mantém dependência com skin legada

## 7. Sprites Novos Produzidos

Nenhum sprite foi gerado nesta sessão. A auditoria confirma que a geração depende de:
1. Verificação das referências canônicas disponíveis
2. Ferramentas adequadas de pixel art
3. Prioridade definida (Katana > Punhos > Moto)

## 8. Artes Rejeitadas

Nenhuma arte foi gerada ou rejeitada nesta sessão.

## 9. Testes Executados

Nenhum teste Unity foi executado (ausência de runtime Unity neste ambiente).

## 10. Recursos Desativados / Propostos para Desativação

Ver `REDUCAO-ESCOPO-ANTIGRAVITY-20261026.md` para proposta completa.

## 11. Instruções para o Codex

1. Examinar commits na branch `antigravity/apoio-redesign-20261026`
2. Comparar a auditoria com o workspace local atualizado
3. Os documentos de análise podem ser incorporados se ainda válidos
4. A proposta de redução de escopo requer aprovação antes de implementação
5. Nenhum merge automático deve ser feito — revisão manual necessária
6. Preservar todas as alterações locais mais recentes do Codex

## 12. Diferenças Conhecidas vs Fechamento 26/10

- Clone não contém nenhuma das alterações de 26/10
- Última referência temporal no clone é 25/10 retomada 18h50
- Coverage: 76 contextos com 275 figuras vs ~1460 contextos pendentes
- Build e gates finais não confirmados no clone
