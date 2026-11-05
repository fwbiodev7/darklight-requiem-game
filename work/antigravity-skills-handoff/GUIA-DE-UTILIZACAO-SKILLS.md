# Guia de Utilização das Novas Skills — DARKLIGHT: REQUIEM
**Data:** 05/11/2026  
**Público-Alvo:** Desenvolvedores, Agentes e Automação Codex  

---

## 1. Como Usar `darklight-cutscene-engineering`

### Objetivo:
Criar uma cutscene com múltiplos atores e sincronização com áudio e gameplay.

### Exemplo Prático: Criando a Cutscene de Infância (Fábio e Lucius)
1. **Via Script C# (Modo Editor / Headless):**
   ```csharp
   using UnityEngine;
   using UnityEngine.Timeline;
   using UnityEditor;

   public static class SetupChildhoodCutscene
   {
       [MenuItem("Darklight/Cutscenes/Gerar Infancia")]
       public static void Generate()
       {
           var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
           timeline.editorSettings.frameRate = 30f;

           // Faixa de Atores
           var fabioTrack = timeline.CreateTrack<AnimationTrack>(null, "Fabius_Young");
           var luciusTrack = timeline.CreateTrack<AnimationTrack>(null, "Lucius_Young");

           // Faixa de Áudio
           var audioTrack = timeline.CreateTrack<AudioTrack>(null, "Audio_Madeira");

           // Faixa de Sinais (Transição)
           var signalTrack = timeline.CreateTrack<SignalTrack>(null, "Eventos");

           AssetDatabase.CreateAsset(timeline, "Assets/Darklight/Cutscenes/ChildhoodTraining.playable");
           AssetDatabase.SaveAssets();
           Debug.Log("Cutscene de Infancia gerada com sucesso!");
       }
   }
   ```
2. **Via Ponte MCP (Quando o servidor estiver ativo):**
   * Ativar o grupo: `unity_open_mcp_manage_tools(action="activate", group="timeline")`.
   * Criar o ativo: `unity_open_mcp_timeline_create(asset_path="Assets/Darklight/Cutscenes/ChildhoodTraining.playable", frame_rate="30")`.
   * Adicionar tracks e clipes com `unity_open_mcp_timeline_track_add` e `unity_open_mcp_timeline_clip_add`.

---

## 2. Como Usar `darklight-cinematic-camera`

### Objetivo:
Disparar tremores dinâmicos em golpes pesados e transições de enquadramento suave para introdução de chefes.

### Exemplo Prático: Tremor no Impacto da Ruptura
```csharp
// Em qualquer método de ataque ou colisão (ex: DarklightCombatRules / DarklightStrikeResolver)
DarklightCinematicCamera.Instance?.Shake(duration: 0.22f, magnitude: 0.28f);
```

### Exemplo Prático: Apresentação de Chefe
```csharp
// Transição suave para o Boss na Arena 4
public void FocusOnBoss(Transform bossTransform)
{
    // Aumenta a prioridade da câmera do Boss para 20
    // O CinemachineBrain interpola suavemente para o enquadramento
    bossCamera.Priority = 20;
    
    // Dispara a fala do chefe
    SpeakBossIntro(() => {
        // Ao finalizar ou skipar, devolve para o player
        bossCamera.Priority = 0;
    });
}
```

---

## 3. Como Usar `darklight-pixel-perfect-qa`

### Objetivo:
Auditar automaticamente sprites e câmeras para garantir que nenhuma textura esteja borrada ou com jitter.

### Exemplo Prático:
1. No menu do Unity Editor, selecione:  
   `Darklight > QA > Auditar Sprites Pixel Perfect`.
2. O validador percorre todas as texturas em `Assets/Darklight/` e aponta:
   * Texturas sem `Filter Mode = Point`;
   * Texturas com Mipmaps ativos;
   * Sprites com pivôs não-inteiros.
3. Corrige automaticamente as flags chamando `TextureImporter.SaveAndReimport()`.

---

## 4. Como Usar `pixel-art-sprite-sheet-master` com os Novos Fundamentos

### Objetivo:
Revisar novas animações geradas (ex: Katana, Punhos, Moto) garantindo maturidade, proporções e paleta canônica.

### Procedimento de Auditoria:
1. Abra `Tools/Skills/pixel-art-sprite-sheet-master/references/pixel-art-fundamentals.md`.
2. Execute o **Teste de Silhueta**: confira se o caçador possui contornos reconhecíveis e proporção adulta.
3. Execute o **Teste de Paleta**:
   * Olhos: `#C40814` (base) e `#5A0000` (sombra).
   * Pele: `[246, 174, 130]`.
   * Ausência de Pillow Shading ou dithering no rosto.
4. Execute o script de prévia em lote:  
   `python Tools/Skills/pixel-art-sprite-sheet-master/scripts/sprite_pipeline.py audit --sheet {caminho_da_folha}`.
