---
name: darklight-cinematic-camera
description: Architectural guidelines and implementations for 2D orthographic cinematic cameras, framing, smooth blends, impact shakes, boss intro shots, and safe transitions in DARKLIGHT: REQUIEM.
version: 1.0.0
---

# DARKLIGHT: REQUIEM — Cinematic Camera & Cinemachine Architecture

Esta skill define as práticas de engenharia de câmera 2D ortográfica para **DARKLIGHT: REQUIEM**, cobrindo o controle de enquadramento, movimentação suave, tremores de impacto (screen shake) e a rota de integração segura com o **Cinemachine 3.x** sem interromper o controlador canônico existente.

---

## 1. Diagnóstico do Sistema de Câmera Existente

* **Controlador Atual:** Câmera ortográfica 2D personalizada operada por `DarklightCombatLab` e rotinas de interpolação suave (`Vector3.SmoothDamp` / `Lerp`).
* **Configuração Canônica:**
  * Projeção: `Orthographic`
  * Tamanho Ortográfico (`orthographicSize`): Calibrado para o campo de visão de combate e bounds de fase.
  * Escala Visual: `DarklightWorldScale.Environment` e escala de ator `1.325`.
* **Regra de Segurança Estrita:** **Não remover nem quebrar o controlador atual.** Qualquer integração com Cinemachine 3.x deve operar como uma camada complementar de sobreposição para cutscenes e apresentações de chefe, preservando a estabilidade da câmera de gameplay principal.

---

## 2. Padrões de Cinemachine 3.x Adaptados para 2D Ortográfico

No Cinemachine 3.x (`Unity.Cinemachine`), a arquitetura é baseada em componentes acoplados ao GameObject da `CinemachineCamera`:

### Estrutura dos Slots em 2D:
1. **Body (`CinemachinePositionComposer` ou `CinemachineFollow`):**
   * Em 2D ortográfico, o eixo Z é fixado em `-10f`.
   * `Damping` em X e Y: `0.15s` para movimento ágil do caçador, evitando atraso excessivo durante dashes de moto.
   * `Dead Zone`: Pequena (0.05) para manter Fábio centrado sem micro-tremores em repouso.
2. **Aim:**
   * Em 2D ortográfico puro, o slot Aim é dispensado ou mantido em `HardLookAtTarget` no plano XY para evitar rotação indevida da câmera nos eixos X/Y.
3. **Noise (Perlin Multi-Canal 2D):**
   * Componente: `CinemachineBasicMultiChannelPerlin`.
   * Perfil de ruído ortográfico em X e Y apenas (sem perturbação no eixo Z para evitar inconsistência de clipping).
4. **Lens:**
   * Modo: `Orthographic`.
   * Near Clip: `0.1f`, Far Clip: `100f`.

---

## 3. Catálogo de Tremores de Impacto (Screen Shake Matrix)

| Evento de Combate | Amplitude | Frequência | Duração | Descrição |
|---|---|---|---|---|
| **Ataque Básico (Katana / Punhos)** | 0.08 | 25 Hz | 0.08s | Pulso seco e imediato na direção oposta ao golpe. |
| **Parry Perfeito (Q)** | 0.18 | 35 Hz | 0.14s | Impacto metálico estático com leve desaceleração temporal (`slowTime`). |
| **Golpe Pesado de Ruptura / Eclipse** | 0.28 | 20 Hz | 0.22s | Vibração densa refletindo a energia abissal liberada. |
| **Passada da Moto Abissal (Drift/Dash)** | 0.12 | 15 Hz | Contínuo | Tremor contínuo de esteira no solo de paralelepípedos. |
| **Entrada / Rugido de Chefe** | 0.38 | 18 Hz | 0.60s | Tremor profundo no ambiente durante a animação de apresentação. |

---

## 4. Apresentação Cinemática de Chefe e Retorno Seguro

Durante encontros como Kael Voss, Valtherion ou Nocthar, a câmera executa a sequência de 3 etapas:

```
[Gameplay Normal] ──(Blend Suave 0.8s)──> [Foco no Chefe / Intro]
                                                │
[Gameplay Retomado] <──(Blend Retorno 0.6s)─────┘
```

### Protocolo de Retorno Seguro:
1. A câmera de cinemática eleva temporariamente a prioridade (`Priority = 20`).
2. O `CinemachineBrain` executa um blend suave (`EaseInOut`, 0.8s) para enquadrar o chefe.
3. A fala do chefe e a legenda de apresentação são exibidas.
4. Ao término (ou após o jogador apertar `Enter`/`A` para dispensar):
   * A prioridade da câmera cinemática retorna para 0.
   * O blend reconduz a visão para Fábio em 0.6s.
   * Os comandos de combate e movimento são liberados sem descontinuidade física.

---

## 5. Implementação C# Standalone para Uso Imediato

Enquanto o pacote `com.unity.cinemachine` não estiver instalado no `manifest.json`, o seguinte componente desacoplado garante todos os efeitos com 0 dependências externas:

```csharp
using UnityEngine;

namespace Darklight
{
    public sealed class DarklightCinematicCamera : MonoBehaviour
    {
        public static DarklightCinematicCamera Instance { get; private set; }
        Vector3 shakeOffset;
        float shakeDuration, shakeMagnitude;

        void Awake() => Instance = this;

        public void Shake(float duration, float magnitude)
        {
            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
        }

        void LateUpdate()
        {
            if (shakeDuration > 0)
            {
                shakeOffset = (Vector3)(Random.insideUnitCircle * shakeMagnitude);
                shakeDuration -= Time.unscaledDeltaTime;
                shakeMagnitude = Mathf.MoveTowards(shakeMagnitude, 0, Time.unscaledDeltaTime * 2f);
            }
            else shakeOffset = Vector3.zero;

            transform.position += shakeOffset;
        }
    }
}
```
