# AUDITORIA DE TESTES, LACUNAS E REGRESSOES — 26/10/2026
**DARKLIGHT: REQUIEM — Etapa B (Antigravity)**

## Objetivo
Realizar um exame minucioso da suite de testes automatizados do clone `DARKLIGHT-ANTIGRAVITY`, investigando integridade de execucao, dependencias de ambiente, caminhos rigidos, quebras de referencias e lacunas de cobertura conceitual, com evidencias tecnicas verificaveis.

---

## 1. INVENTARIO E PREMISSA METODOLOGICA

### 1.1 Quantitativo de Testes no Clone
- **EditMode Tests**: 52 arquivos C# em `Assets/Darklight/Tests/EditMode/`
- **PlayMode Tests**: 73 arquivos C# em `Assets/Darklight/Tests/PlayMode/`
- **Total**: 125 arquivos de teste dedicados.

### 1.2 Principio: "Presenca de Script != Teste Aprovado"
Neste clone isolado, **nao ha processo do Unity Engine em execucao**, o que significa que o runner NUnit integrado do Unity nao foi disparado neste ciclo. A existencia dos arquivos de teste comprova a arquitetura desenhada, mas **nao autoriza presumir que todos os 125 testes estejam passando** no estado atual do repositorio.

---

## 2. EVIDENCIA VERIFICAVEL 1: CAMINHOS ABSOLUTOS HARDCODED

Foram detectados **29 pontos com caminhos absolutos locais** em scripts de teste, apontando diretamente para pastas de workspace especifico do Codex:
`C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/...`

### 2.1 Amostra de Testes Afetados
| Arquivo de Teste | Linha com Hardcode | Destino Fixo |
|---|---|---|
| `DarklightSoloTransformationTests.cs` | Linha 15 | `.../remaster-prototype-20261014/solo/` |
| `DarklightDialogueQARunner.cs` | Linha 12 | `.../dialogo-textura-20261015/` |
| `DarklightOverhaulQARunner.cs` | Linha 14 | `.../combat-overhaul-20261015/` |
| `DarklightRegionReformQARunner.cs` | Linha 13 | `.../mapas-reforma-20261015/` |
| `DarklightAbyssControllerReviewTests.cs` | Linha 17 | `.../avaliacao-video-20261016/` |
| `DarklightAbyssDeliveryMediaTests.cs` | Linha 16 | `.../minimapa-reforma-20261015/authorial/media-current/` |
| `DarklightAbyssFlowGameplayTests.cs` | Linha 15 | `.../cavaleiro-sprites-20261015/capturas/flow/` |
| `DarklightAbyssPrototypeTests.cs` | Linhas 14, 28, 45 | `.../cavaleiro-sprites-20261015/capturas/` |
| `DarklightAbyssRunOverGameplayTests.cs` | Linha 18 | `.../cavaleiro-sprites-20261015/capturas/runover/` |
| `DarklightAbyssUrgentTests.cs` | Linha 14 | `.../cavaleiro-sprites-20261015/` |
| `DarklightAcceptedScaleTests.cs` | Linha 13 | `.../cavaleiro-final-20261016/anatomy-correction/gameplay/` |
| `DarklightArtisticAtlasTests.cs` | Linha 15 | `.../revision-bugs-20261016-1957/captures/atlas/` |
| `DarklightCombatCaptureTests.cs` | Linha 13 | `.../combat-overhaul-20261015/captures/` |
| `DarklightCombatImpactTests.cs` | Linha 16 | `.../remaster-production-20261014/captures/` |
| `DarklightFocusedHUDTests.cs` | Linha 14 | `.../revision-bugs-20261016-1957/captures/hud/` |
| `DarklightLoadingTests.cs` | Linha 14 | `.../remaster-production-20261014/captures/` |
| `DarklightMap10ReviewTests.cs` | Linha 15 | `.../mapas-reforma-20261016/map10/` |
| `DarklightMap8ReviewTests.cs` | Linha 15 | `.../mapas-reforma-20261016/map8/` |
| `DarklightMap9ReviewTests.cs` | Linha 15 | `.../mapas-reforma-20261016/map9/` |

### 2.2 Impacto e Risco
- Esses testes tentam salvar capturas de tela e arquivos JSON em caminhos rigidos. Se o usuario, maquina ou ambiente de CI nao possuir essa estrutura exata de diretorios criada, o teste pode abortar com `DirectoryNotFoundException` ou falhar ao tentar gravar evidencias.

---

## 3. EVIDENCIA VERIFICAVEL 2: ACOPLAMENTO PLAYMODE COM EDITOR VIA REFLECTION

Em `Assets/Darklight/Tests/PlayMode/DarklightSoloTransformationTests.cs`:
- Linha 18:
  ```csharp
  static object Call(string method, params object[] args) =>
      Type.GetType("Darklight.Editor.RemasterPrototype, Darklight.Editor")
          .GetMethod(method, BindingFlags.Public | BindingFlags.Static)
          .Invoke(null, args);
  ```
- **Risco**: Em testes de PlayMode executados dentro de um build Standalone Player (como ocorre em pipelines de validacao limpa), assemblies com sufixo `.Editor` nao sao inclusos no pacote executavel. O metodo `Type.GetType(...)` retorna `null`, gerando `NullReferenceException` imediata ao invocar `GetMethod`.

---

## 4. EVIDENCIA VERIFICAVEL 3: LACUNAS CRITICAS DE COBERTURA

### 4.1 Moto com Requiem (Cobertura Zero)
- `DarklightMotorcycleRedesignTests.cs`: testa exclusivamente `DarklightMotorcycleRedesignBank.Riding(form, dir)` que internamente carrega `RockMotoForm*` (`skin == 1`).
- Os testes de gameplay veicular (`DarklightAbyssFlowGameplayTests`, `DarklightAbyssRunOverGameplayTests`) configuram o cenario para Rock ou utilizam o prototipo generico.
- **Nao ha um unico teste unitario ou de integracao validando a pilotagem da moto com a skin Requiem (`skin == 0`)**.

### 4.2 Eclipse Absoluto (Cobertura Zero da Fusao Canonica)
- Os testes de transformacao (`DarklightTransformationTests.cs`, `DarklightSoloTransformationTests.cs`) cobrem:
  - Form 1 (Ruptura)
  - Form 2 (Eclipse)
  - Form 3 (Absoluto — versao antiga)
- A fusao canônica "Eclipse Absoluto" desenvolvida em 26/10 pelo Codex **nao possui cobertura neste clone**, pois o codigo correspondente ainda nao foi mesclado a este repositorio.

### 4.3 Katana Transformaçoes Multidirecionais (Lacuna de Assets)
- O `catalog.json` de armas possui apenas 1 direcao (Sul) para Katana Ruptura, Katana Eclipse e Katana Absoluto.
- Os testes de redesign (`DarklightWeaponRedesignTests`) testam a validade das paginas presentes, mas nao cobrem a ausencia das outras 7 direcoes transformadas da Katana, as quais dependem do fallback estavel.

---

## 5. AUDITORIA DE RECURSOS DINAMICOS E STATICS (`Resources.Load`)

Foram varridas todas as 107 chamadas estaticas e dinamicas a `Resources.Load` no projeto:
- **`Resources/Darklight/AbyssPrototype/`**: Todos os 24 PNGs das acoes de Join, AttackSix, Dismount, Slam, Finisher das formas 0..3 existem e estao acessiveis.
- **`Resources/Darklight/RuinFists/`**: Todas as 128 paginas mapeadas em `catalog.json` com seus respectivos sprites e mascaras de paleta existem.
- **`Resources/Darklight/WeaponRedesign20261023/`**: Todos os 76 PNGs e manifestos JSON mapeados existem.
- **Recursos deliberadamente nulos**: Em `DarklightCompleteAgeTests.cs`, o teste confere que `Darklight/CompleteAgeCatalog` e `Darklight/AppearanceBodies/*` retornam `null` / vazio para comprovar que o sistema legado de envelhecimento foi removido com sucesso.

---

## 6. RECOMENDACOES PARA O CODEX
1. **Substituicao de Caminhos Rigidos**: Trocar `C:/Users/Usuario/.codex/...` por `Path.Combine(Application.temporaryCachePath, ...)` ou `Path.Combine(Application.dataPath, "../outputs/...")` para tornar a suite 100% portavel.
2. **Desacoplamento de Editor em PlayMode**: Proteger chamadas de reflection em `DarklightSoloTransformationTests` com `#if UNITY_EDITOR` ou mover para assembly apropriado.
3. **Novos Testes de Moto com Requiem**: Assim que o Codex aprovar a liberacao de `Unlocked(3)` para Requiem, introduzir suite de teste de integracao `RequiemMotorcycleTests`.
