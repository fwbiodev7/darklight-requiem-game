# INTEGRATION PLAN & HANDOFF FOR CODEX
**Data**: 04/11/2026  
**Auditor**: Antigravity Technical Support Engineering  
**Destino**: `work/antigravity-handoff-20261104/`

## 1. Pacotes Prontos para Incorporação

1. **Cutscenes (`work/antigravity-cutscenes/`)**:
   - Mover os arquivos C# para `Assets/Darklight/Runtime/Cutscenes/` quando o Codex for estruturar as primeiras cenas cinemáticas (Tutorial Fabius/Lucius, Confronto Kael, etc.).
   - Utilizar a suíte de testes `CutsceneTests.cs` em `Assets/Darklight/Tests/EditMode/`.

2. **Transformações (`work/antigravity-transformations/`)**:
   - `TransformationSafetyController.cs` pode ser anexado como componente auxiliar ao `DarklightFighter` ou `DarklightCampaign`.

3. **Gothic Eclipse (`work/antigravity-gothic-eclipse/`)**:
   - `GothicEclipseOptimizer.cs` pode ser chamado em `DarklightRegionAtmosphere.cs` no método `Update()` para amortecer a carga de partículas.

4. **Campanha e Progressão (`work/antigravity-campaign/`)**:
   - `CampaignProgressionController.cs` pronto para orquestrar os objetivos dos 10 capítulos.

5. **Interface e UI (`work/antigravity-ui/`)**:
   - `UIPreferencesBridge.cs` pronto para centralizar a persistência de configurações no menu principal e de pausa.

6. **Diagnóstico QA (`work/antigravity-qa/`)**:
   - Utilizar o relatório `PUNHOS-WALK-DIAGNOSIS.md` para ajustar o pivô do frame `S01` no Sprite Editor.

