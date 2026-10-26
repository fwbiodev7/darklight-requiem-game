# REDUÇÃO DE ESCOPO — PROPOSTA ANTIGRAVITY 26/10/2026

**Status: PROPOSTA — requer aprovação humana antes de implementação**

## Princípio

Desativar conteúdo obsoleto e simplificar sistemas, sem remoção física de arquivos.
Preservar legado acessível para restauração futura.
Não quebrar saves, controles ou conteúdo existente.

---

## 1. SKIN ROCK — Desativação Proposta

### Situação atual
- Rock (skin 1) está integrada em `DarklightCampaignSkins.cs`
- Requer arrays completos: rockIdle(8), rockMotion(24), rockActions(32), rockSlash(32)
- AbyssRider (weapon 3) depende exclusivamente de Rock
- Save persiste em `PlayerPrefs.Darklight.Skin`

### Proposta
- **NÃO remover** código nem assets de Rock
- Marcar Rock como **legado inativo** no contrato canônico
- Desabilitar o seletor de skin Rock no menu (preservando saves existentes)
- Se o save carrega skin=1, fallback para skin=0 (Réquiem) sem perda de progresso
- AbyssRider permanece como legado, inacessível sem Rock

### Benefício
- Reduz escopo artístico ativo pela metade (não precisa redesenhar Rock)
- Elimina necessidade de manter paridade Rock/Réquiem em novas artes
- AbyssRider fica como conteúdo legado preservado

### Risco
- Jogadores que salvaram em Rock precisam de fallback gracioso
- Cavaleiro do Abismo fica inacessível (já é legado)

---

## 2. ARMA PRESAS (Daggers) — Desativação Proposta

### Situação atual
- `DarklightWeaponId.Daggers` (index 1) no enum
- Unlock: `chapter > 0 || (chapter == 0 && checkpoint == 4)`
- Mensagem: `PRESAS: DERROTE KAEL`
- WeaponRedesign20261023 contém sprites Daggers (Idle, formas)
- 76 PNGs de WeaponRedesign incluem variantes Daggers

### Proposta
- **NÃO remover** do enum nem dos assets
- Ocultar do seletor de armas no ciclo de troca
- Preservar referências em saves/coverage
- Contrato canônico: armas ativas = Katana + Punhos + Moto

### Benefício
- Remove ~30% do escopo artístico de redesign por arma
- Simplifica testes de combinações (2 armas ativas vs 3+)
- Preserva sprites existentes como legado

---

## 3. ARMA CORRENTE (Chain) — Desativação Proposta

### Situação atual
- `DarklightWeaponId.Chain` (index 2) no enum
- `DarklightChainView.cs` implementa visual da corrente
- Unlock: `chapter > 2 || (chapter == 2 && checkpoint == 4)`

### Proposta
- Mesmo tratamento que Presas: ocultar do ciclo, preservar assets
- Contrato canônico não lista Corrente como arma ativa

### Benefício
- Remove mais um eixo do escopo artístico
- Simplifica o sistema de combate ativo

---

## 4. TRANSFORMAÇÕES ANTIGAS — Preservação como Legado

### Situação atual
- Sistema suporta Normal(0), Ruptura(1), Eclipse(2), Absoluto(3)
- Contrato canônico: Normal + Ruptura + Eclipse Absoluto (fusão violeta e branco-prateado)

### Proposta
- Manter o sistema de transformações exatamente como está
- Eclipse e Absoluto são formas distintas no código (2 vs 3)
- "Eclipse Absoluto" do contrato canônico pode ser mapeado para a forma 3 (Absoluto)
- Formas 1 (Ruptura) e 3 (Absoluto) ficam ativas
- Forma 2 (Eclipse) fica como legado preservado, acessível se desbloqueada

### Benefício
- Sem alteração de código necessária
- Preserva compatibilidade total de saves

---

## 5. CÓDIGO POTENCIALMENTE OBSOLETO — Investigação

### Candidatos a revisão (NÃO remoção)
- `DarklightCrystalNativeCapture.cs` — captura de diagnóstico
- `DarklightHeadFixCapture.cs` — captura de correção
- `DarklightUserSheetsCapture.cs` — captura de pranchas
- `DarklightDeliveryCapture.cs` — captura de entrega
- `DarklightHudPerformanceCapture.cs` — captura de performance

### Proposta
- Verificar se são usados em runtime ou apenas em Editor
- Se apenas Editor/diagnóstico, documentar mas NÃO remover
- Podem ser úteis para o Codex em testes futuros

---

## 6. SPRITES DUPLICADOS — Análise Pendente

### Necessário verificar
- Se existem sprites idênticos em múltiplos bancos
- Se RuinFists (154 PNGs) contém variantes rejeitadas junto com aprovadas
- Se WeaponRedesign20261023 contém candidatas superadas por versões em ArtSources/WeaponRedesign20261025

### Proposta
- Não remover sem verificação individual
- Mapear duplicatas objetivamente por hash SHA256
- Manter original e documentar duplicata

---

## 7. RESUMO DE IMPACTO

| Ação | Escopo Removido | Risco | Reversível |
|---|---|---|---|
| Rock desativada | ~50% skins | Baixo (fallback) | Sim |
| Presas desativada | ~25% armas | Baixo (enum preservado) | Sim |
| Corrente desativada | ~25% armas | Baixo (enum preservado) | Sim |
| Eclipse (form 2) legado | ~25% formas | Nenhum | Sim |
| Capturas diagnóstico | 0% (só documentação) | Nenhum | N/A |

### Escopo ativo após simplificação
- **Skin:** Réquiem (1 skin)
- **Armas:** Katana + Punhos da Ruína + Moto (3 armas, via redesign + banks)
- **Formas:** Normal + Ruptura + Eclipse Absoluto (3 formas)
- **Direções:** 8 reais
- **Contextos ativos:** 1 skin × 3 armas × 3 formas × 8 direções = 72 contextos base

---

## IMPLEMENTAÇÃO

**NENHUMA implementação será feita sem aprovação explícita.**
Este documento é apenas uma proposta analítica para revisão.
