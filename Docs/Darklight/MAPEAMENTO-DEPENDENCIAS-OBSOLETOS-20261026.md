# MAPEAMENTO DE DEPENDENCIAS E CODIGOS OBSOLETOS — 26/10/2026
**DARKLIGHT: REQUIEM — Etapa B (Antigravity)**

## Objetivo
Mapear de forma exaustiva as dependencias estruturais, acoplamentos de skins e armas, e codigos diagnosticos/potencialmente obsoletos no clone `DARKLIGHT-ANTIGRAVITY`, **sem excluir ou desativar nenhum arquivo ou linha de codigo**, fornecendo embasamento solido para as decisoes de consolidacao do Codex.

---

## 1. SISTEMA DE ARMAS E ENUMS

### 1.1 `DarklightWeaponId` e Alocacao de Slots
O enum central em `Assets/Darklight/Runtime/Combat/WeaponDefinition.cs`:
```csharp
public enum DarklightWeaponId { Katana, Daggers, Chain, AbyssRider }
```
- **Slot 0 (`Katana`)**:
  - Arma primaria padrao.
  - Disponivel desde o inicio do jogo (`unlockAfterChapter = 0`).
  - Totalmente integrada a `DarklightCombatProfiles` e `DarklightWeaponRedesignBank`.
- **Slot 1 (`Daggers` / Presas)**:
  - Desbloqueada apos Capitulo 0 (`chapter > 0 || (chapter == 0 && checkpoint == 4)`).
  - Referenciada em `DarklightCampaignMinimalHUD.cs`, `DarklightCombatProfiles.cs` e `DarklightCombatCoach.cs`.
  - Possui sprites autorais em `WeaponRedesign20261023` (Idle e Presas1).
- **Slot 2 (`Chain` / Corrente -> Punhos da Ruina)**:
  - Originalmente desbloqueada no Capitulo 2 (`chapter > 2`).
  - **Reproposta de runtime**: Em `DarklightCharacterView.cs` (linha 62):
    ```csharp
    bool ruinFists = weapon == 2 && DarklightRuinFistsBank.Ready;
    ```
    E em `DarklightRuinFistsProfile.cs`:
    ```csharp
    public const int WeaponIndex = 2;
    ```
    Quando `DarklightRuinFistsBank.Ready` esta ativo, o slot 2 assume o nome de exibicao `"PUNHOS DA RUINA"`, substitui o loadout de combate e troca todos os sprites da Corrente pelos Punhos da Ruina. Isso preserva total compatibilidade com os saves existentes e mascaras de bits de unlock (`1 << 2`).
- **Slot 3 (`AbyssRider` / Moto)**:
  - Arma associada a mobilidade veicular e combate aereo/impacto.
  - Possui forte acoplamento com a skin Rock (analisado na secao 2).

---

## 2. ACOPLAMENTOS DA SKIN ROCK (SKIN 1) E A BARREIRA DA MOTO

### 2.1 Dependencias Identificadas no Codigo
A Moto (`AbyssRider`) encontra-se estritamente restrita a `skin == 1` (Rock) em quatro camadas independentes:

1. **Desbloqueio e Equipamento (`DarklightWeaponController.cs`)**:
   - Linha 12:
     ```csharp
     public bool Unlocked(int index) => index >= 0 && index < game.weaponDefinitions.Length &&
         (index != 3 || game.SelectedSkin == 1) &&
         ((game.Progress.weaponUnlocks & (1 << index)) != 0 || index == 3 && game.Training);
     ```
   - Linha 45 (`EnforceSkin`):
     ```csharp
     public void EnforceSkin()
     {
         if (Equipped?.id != DarklightWeaponId.AbyssRider || Unlocked(3)) return;
         pending = -1;
         int index = Unlocked(lastCompatible) ? lastCompatible : 0;
         // Forca desequipar AbyssRider se a skin nao for Rock (1)
     }
     ```
   - Se o jogador estiver com Requiem (`SelectedSkin == 0`), a moto e considerada bloqueada e desequipada instantaneamente.

2. **Visualizacao do Personagem (`DarklightCharacterView.cs`)**:
   - Linhas 66 e 76:
     ```csharp
     var selected = !previewing && skin == 1 && game?.AbyssRider?.Active == true
         ? DarklightAbyssSpriteBank.Resolve(game.AbyssRider, model)
         : TodayCharacterBank.Resolve(skin, 0, weapon, direction, model, movement);
     
     if (!previewing && skin == 1 && game?.AbyssRider?.Active == true) {
         // Ativa DarklightAbyssLocomotion apenas se skin == 1
     }
     ```
   - Se `skin == 0` (Requiem), o sistema ignora o banco de sprites da moto e busca poses no banco humano padrao.

3. **Banco de Redesign da Moto (`DarklightMotorcycleRedesignBank.cs`)**:
   - Linhas 24 e 34:
     ```csharp
     public static string Resource(int form) =>
         DarklightWeaponRedesignBank.Root + "RockMotoForm" + form + "Ride8SlamS20261025V2";
     
     // Validacao estrita de carregamento do JSON:
     if (entry == null || entry.form != form || entry.skin != 1 || ...) return null;
     ```
   - O banco rejeita qualquer recurso onde `entry.skin != 1` e carrega exclusivamente os arquivos nomeados `RockMotoForm*`.

### 2.2 Requisitos Estruturais para Moto Funcionar com Requiem
Para que a Moto funcione com a skin Requiem (conforme diretriz canonica), sera necessario:
1. Em `DarklightWeaponController`: permitir `index == 3` quando `SelectedSkin == 0`.
2. Em `DarklightCharacterView`: verificar `game?.AbyssRider?.Active == true` independentemente de `skin == 1`.
3. Em `DarklightMotorcycleRedesignBank`: parametrizar o metodo `Resource(int form)` ou criar `Resource(int skin, int form)` que aponte para recursos de Requiem quando disponiveis.
4. **Dependencia artistica**: Criar ou derivar sprites de Requiem na moto (atualmente inexistentes no clone).

---

## 3. TRANSFORMACOES: FORMA 3 ANTIGA VS ECLIPSE ABSOLUTO

### 3.1 Arquitetura Atual no Clone
Em `DarklightCampaignTransformations.cs`:
- Linha 24:
  ```csharp
  Notice(new[] { "", "RUPTURA", "ECLIPSE", "ABSOLUTO" }[Form]);
  ```
- As formas ativas no codigo sao:
  - `Form 0`: Normal
  - `Form 1`: Ruptura
  - `Form 2`: Eclipse
  - `Form 3`: Absoluto

### 3.2 Advertencia Critica: Forma 3 Antiga != Eclipse Absoluto
- A forma 3 presente no clone (`Absoluto`) corresponde a implementacao original de cabelos prateados/brancos e aura estatica de brilho intenso.
- O contrato canônico atual define o **"Eclipse Absoluto"** como uma fusao visual e conceitual de violeta profundo (Eclipse) com branco-prateado estelar (Absoluto).
- **Regra de Seguranca**: O valor numerico antigo `Form == 3` **nao deve ser tratado como implementacao concluida de Eclipse Absoluto**.
- A consolidacao do Eclipse Absoluto foi realizada pelo Codex em 26/10 e sua logica mais recente reside fora do clone atual. Nenhuma fusao ou sobreposicao de codigo deve ser forcada nesta branch.

---

## 4. MAPEAMENTO DE CODIGOS DIAGNOSTICOS E DE CAPTURA

Foram inspecionadas as classes com sufixo `Capture`, `Check` e `Review` em `Assets/Darklight/Runtime/`:

1. **`DarklightDeliveryCapture.cs` (com partials `CrystalNativeCapture` e `HeadFixCapture`)**:
   - `[ExecuteInEditMode]` / acionamento estrito por flags de QA.
   - Nao intercepta o fluxo normal de gameplay de campanha.
2. **`DarklightHudPerformanceCapture.cs`**:
   - Monitoramento passivo de taxa de quadros e draw calls para revisao visual.
3. **`DarklightNativeCapture.cs`**:
   - Utilitario de leitura de render targets para geracao de evidencias sem tela de swapchain.
4. **`DarklightUserSheetsCapture.cs`**:
   - Partial de `DarklightCampaign` projetada para testes de pranchas congeladas.
5. **`DarklightAbyssBuildCheck.cs` e `DarklightMapBuildCheck.cs`**:
   - Assercoes estaticas de integridade de cenas e mapas no ciclo de build.

### Veredito de Manutencao
Nenhum desses arquivos deve ser desativado ou deletado. Eles compoem o ferramental de garantia de qualidade e automacao utilizado pelo Codex e pelo Antigravity para registrar evidencias confiaveis.
