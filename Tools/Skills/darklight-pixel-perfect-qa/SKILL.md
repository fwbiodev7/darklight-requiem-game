---
name: darklight-pixel-perfect-qa
description: Specialized quality assurance and audit guidelines for 2D pixel perfect rendering, URP 2D pipeline, texture filtering, PPU calibration, and jitter elimination in DARKLIGHT: REQUIEM.
version: 1.0.0
---

# DARKLIGHT: REQUIEM — Pixel Perfect QA & URP 2D Audit

Esta skill estabelece o guia oficial de garantia de qualidade visual e conformidade técnica para renderização **Pixel Perfect 2D** sob o **Universal Render Pipeline (URP 17.6 / Unity 6000.6.0f1)** em **DARKLIGHT: REQUIEM**.

---

## 1. Regra Fundamental de Pipeline (URP vs. Built-in)

> [!CRITICAL]
> **DARKLIGHT: REQUIEM utiliza URP 2D.**
> O componente correto é **`UnityEngine.Rendering.Universal.PixelPerfectCamera`** (integrado nativamente ao pacote `com.unity.render-pipelines.universal`).
> **NUNCA instale o pacote `com.unity.2d.pixel-perfect`** — ele pertence exclusivamente à Built-in Render Pipeline legada e causa conflito catastrófico de compilação e renderização no Unity 6.

---

## 2. Checklist de Auditoria Técnica de Texturas e Sprites

Antes de aprovar qualquer lote de spritesheets ou texturas de mapa, execute a auditoria ponto a ponto:

### 2.1 Configurações de Importação de Sprites (Inspector / TextureImporter)
* [ ] **Filter Mode:** Obrigatoriamente `Point (no filter)`. *Bilinear ou Trilinear causam borrões inaceitáveis na arte.*
* [ ] **Generate Mip Maps:** **Desmarcado**. *Mipmaps criam efeito turvo quando a câmera se move ou faz zoom.*
* [ ] **Compression:** `None` / `RGBA 32-bit`. *Compressões como DXT ou Crunch criam artefatos escuros ao redor das bordas dos sprites.*
* [ ] **Pixels Per Unit (PPU):**
  * Mapas e Terrenos Ilustrados: `PPU = 24` (com `DarklightWorldScale.Environment`).
  * Folhas de Sprites dos Personagens: Consistência estrita em toda a folha.
* [ ] **Sprite Pivots:** Definidos em pixels inteiros (`Custom / Pixels`), evitando pivôs com fração (`.5px`) que causam amostragem desalinhada no raster.

### 2.2 Configuração da Câmera URP
* [ ] Projeção: `Orthographic`.
* [ ] **Anti-Aliasing (MSAA / FXAA):** **Completamente Desativado** no URP Asset e na Câmera. O Anti-Aliasing suaviza as bordas propositais do pixel art.
* [ ] **Allow HDR:** Desativado na câmera base para evitar artefatos de precisão em cores de paleta limitada.
* [ ] **Grid Snapping:** Ativado no componente `PixelPerfectCamera` para impedir jitter sub-pixel em movimentos diagonais da moto e do caçador.

---

## 3. Interação com `WorldVisualScale` (1.325)

No runtime do jogo, o multiplicador de escala visual do caçador e inimigos utiliza o fator canônico:
$$\text{WorldVisualScale} = 1.325$$

### Diretrizes de Estabilidade:
* **Escala de Transform:** Os GameObjects dos atores aplicam a escala visual no `Transform.localScale`. O `PixelPerfectCamera` deve operar com a resolução de referência correta para que o snap continue coerente com a grade de mundo.
* **Sem Tremor em Repouso (Idle Jitter):** Garantir que a posição do corpo (`Rigidbody2D.position`) utilize valores arredondados para a menor unidade de pixel:
  $$\Delta x_{\min} = \frac{1}{\text{PPU}} \approx \frac{1}{24} \approx 0.04166\text{ unidades}$$

---

## 4. Compatibilidade com Pós-Processamento Dark Fantasy

O estilo visual gótico e sombrio de Vespera utiliza efeitos de volume do URP:
* **Bloom Carmesim / Roxo:** Permitido para emissão de energia das armas (Ruptura/Eclipse), desde que o limiar (`Threshold`) esteja configurado acima de `1.1` para afetar apenas os brilhos intensos e não esfumaçar a silhueta do personagem.
* **Vignette:** Totalmente compatível. Escurece as bordas da tela sem alterar a nitidez dos pixels centrais.
* **Color Adjustments (Contraste e Saturação):** Aplicado globalmente no render graph sem re-amostrar ou distorcer os contornos de pixel art.

---

## 5. Script de Verificação Automática (Editor QA)

O script abaixo pode ser executado para auditar todos os spritesheets da pasta `Assets/Darklight/`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DarklightPixelPerfectAudit
{
    [MenuItem("Darklight/QA/Auditar Sprites Pixel Perfect")]
    public static void AuditAllSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Darklight" });
        int issues = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite) continue;

            if (importer.filterMode != FilterMode.Point)
            {
                Debug.LogWarning($"[FilterMode != Point] {path}");
                issues++;
            }
            if (importer.mipmapEnabled)
            {
                Debug.LogWarning($"[MipMaps Ativos] {path}");
                issues++;
            }
        }
        Debug.Log($"[Pixel Perfect Audit Concluída] {guids.Length} texturas analisadas. {issues} inconformidades encontradas.");
    }
}
#endif
```
