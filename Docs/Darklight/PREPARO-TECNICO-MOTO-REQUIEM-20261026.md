# PREPARO TECNICO PARA INTEGRACAO DE REQUIEM NA MOTO — 26/10/2026
**DARKLIGHT: REQUIEM — Etapa C (Antigravity)**

## Objetivo
Detalhar a arquitetura necessaria e as modificacoes cirurgicas para que a Moto (`DarklightWeaponId.AbyssRider`) funcione nativamente com a skin **Requiem (`SelectedSkin == 0`)**, eliminando a restricao legada que a limitava exclusivamente a Rock (`skin == 1`), preservando a compatibilidade retroativa de saves e preparando os hooks de assets.

---

## 1. DIAGNOSTICO DO ACOPLAMENTO ATUAL

Atualmente, o sistema possui 4 pontos de bloqueio rigido que impedem Requiem de pilotar a moto:

### Ponto 1: `DarklightWeaponController.cs` (Desbloqueio)
- **Codigo atual (linha 12)**:
  ```csharp
  public bool Unlocked(int index) => index >= 0 && index < game.weaponDefinitions.Length &&
      (index != 3 || game.SelectedSkin == 1) &&
      ((game.Progress.weaponUnlocks & (1 << index)) != 0 || index == 3 && game.Training);
  ```
- **Efeito**: A condicao `(index != 3 || game.SelectedSkin == 1)` bloqueia o slot 3 (AbyssRider) sempre que a skin selecionada for Requiem (`SelectedSkin == 0`).

### Ponto 2: `DarklightWeaponController.cs` (EnforceSkin)
- **Codigo atual (linha 45)**:
  ```csharp
  public void EnforceSkin()
  {
      if (Equipped?.id != DarklightWeaponId.AbyssRider || Unlocked(3)) return;
      pending = -1;
      int index = Unlocked(lastCompatible) ? lastCompatible : 0;
      game.Progress.equippedWeapon = index;
      Equipped = game.weaponDefinitions[index];
      fighter.loadout = Equipped.loadout;
      fighter.ResetModel();
      game.AbyssRider?.ResetMotion();
  }
  ```
- **Efeito**: Ao alternar para Requiem ou ao carregar um save em Requiem, o metodo forca o desequipamento imediato da AbyssRider para Katana (0).

### Ponto 3: `DarklightCharacterView.cs` (Apresentacao Visual)
- **Codigo atual (linhas 66 e 76)**:
  ```csharp
  var selected = !previewing && skin == 1 && game?.AbyssRider?.Active == true
      ? DarklightAbyssSpriteBank.Resolve(game.AbyssRider, model)
      : TodayCharacterBank.Resolve(skin, 0, weapon, direction, model, movement);

  if (!previewing && skin == 1 && game?.AbyssRider?.Active == true)
  {
      abyssLocomotion ??= GetComponent<DarklightAbyssLocomotion>() ?? gameObject.AddComponent<DarklightAbyssLocomotion>();
      var gait = abyssLocomotion.Present(model, movement, game.PresentedForm);
      if (gait != null) { selected = gait; locomotionSelected = true; }
  }
  ```
- **Efeito**: O resolvedor de sprites e o componente de locomocao da moto so sao ativados se `skin == 1`.

### Ponto 4: `DarklightMotorcycleRedesignBank.cs` (Carregamento do Banco)
- **Codigo atual (linhas 24 e 34)**:
  ```csharp
  public static string Resource(int form) =>
      DarklightWeaponRedesignBank.Root + "RockMotoForm" + form + "Ride8SlamS20261025V2";

  // Na validacao da folha:
  if (entry == null || entry.form != form || entry.skin != 1 || !entry.staticRideViews || ...) return null;
  ```
- **Efeito**: O banco assume o prefixo fixo `RockMotoForm` e valida explicitamente `entry.skin == 1`, retornando `null` se qualquer outro valor for informado.

---

## 2. ESPECIFICACAO DO DESACOPLAMENTO CIRURGICO

Para viabilizar a moto com Requiem sem quebrar saves existentes ou a skin Rock, propoe-se a seguinte implementacao cirurgica a ser realizada em conjunto com a integracao de assets pelo Codex:

### 2.1 Ajuste em `DarklightWeaponController.cs`
- **Substituicao na linha 12**:
  ```csharp
  // Permitir AbyssRider tanto para Requiem (0) quanto para Rock (1)
  public bool Unlocked(int index) => index >= 0 && index < game.weaponDefinitions.Length &&
      ((game.Progress.weaponUnlocks & (1 << index)) != 0 || index == 3 && game.Training);
  ```
- **Efeito**: Remove a trava exclusiva `(index != 3 || game.SelectedSkin == 1)`. O metodo `EnforceSkin()` passa a validar o desbloqueio universalmente, mantendo o jogador montado ao alternar skins.

### 2.2 Ajuste em `DarklightCharacterView.cs`
- **Substituicao nas linhas 66 e 76**:
  ```csharp
  var selected = !previewing && game?.AbyssRider?.Active == true
      ? DarklightAbyssSpriteBank.Resolve(game.AbyssRider, model, skin)
      : TodayCharacterBank.Resolve(skin, 0, weapon, direction, model, movement);

  if (!previewing && game?.AbyssRider?.Active == true)
  {
      abyssLocomotion ??= GetComponent<DarklightAbyssLocomotion>() ?? gameObject.AddComponent<DarklightAbyssLocomotion>();
      var gait = abyssLocomotion.Present(model, movement, game.PresentedForm);
      if (gait != null) { selected = gait; locomotionSelected = true; }
  }
  ```
- **Efeito**: O visual veicular e ativado sempre que a AbyssRider estiver ativa, delegando a selecao de skin para os bancos autorais.

### 2.3 Ajuste em `DarklightMotorcycleRedesignBank.cs`
- **Suporte bivalente de skin**:
  ```csharp
  public static string Resource(int skin, int form)
  {
      string prefix = skin == 1 ? "RockMotoForm" : "ReqMotoForm";
      string path = DarklightWeaponRedesignBank.Root + prefix + form + "Ride8SlamS20261025V2";
      // Fallback gracioso: se a versao Requiem nao estiver importada, usar a base visual Rock com remap de paleta
      if (skin == 0 && Resources.Load<TextAsset>(path) == null)
          return DarklightWeaponRedesignBank.Root + "RockMotoForm" + form + "Ride8SlamS20261025V2";
      return path;
  }
  ```
- **Validacao flexivel**:
  ```csharp
  if (entry == null || entry.form != form || (entry.skin != 0 && entry.skin != 1) || ...) return null;
  ```

---

## 3. ESPECIFICACAO DOS ASSETS DE REQUIEM NA MOTO

### 3.1 Requisitos das Pranchas
1. **Resolucao e Grid**:
   - Folha de 1536x1024 RGBA32 Point-Filtered, PPU = 110.5527f (ou 128f conforme calibração de escala 1.325f).
   - 8 direcoes de pilotagem estatica (`staticRideViews: true`) cobrindo Sul (0), Sudoeste (1), Oeste (2), Noroeste (3), Norte (4), Nordeste (5), Leste (6), Sudeste (7).
   - 4 fases de impacto frontal (`GroundSlam` Sul): Antecipacao (0), Descida (1), Impacto no Solo (2), Recuperacao (3).
2. **Identidade Visual de Requiem**:
   - Cabelo longo escuro com mecha frontal caracteristica (em vez do cabelo espetado volumoso de Rock).
   - Casaco/manto com tecido fluido reagindo a velocidade do deslocamento.
   - Oculos/visor tatico aprovado em `ArtSources/WeaponRedesign20261023/references/approved-player-eyes.png`.

---

## 4. STATUS DE PRONTIDAO
- **Arquitetura de codigo**: Mapeada e pronta para aplicacao sem risco de regressao.
- **Assets**: Aguarda definicao de pranchas canonicas pelo Codex ou autorizacao de geracao de paleta.
