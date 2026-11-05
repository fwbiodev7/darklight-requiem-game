---
name: darklight-cutscene-engineering
description: Author, coordinate, and validate 2D cinematic cutscenes, Timeline sequences, PlayableDirectors, animation/activation tracks, narrative signal events, safe skip gates, and gameplay transitions in DARKLIGHT: REQUIEM.
version: 1.0.0
---

# DARKLIGHT: REQUIEM — Cutscene Engineering & Timeline Architecture

Esta skill padroniza o fluxo de criação, edição, orquestração e validação de cutscenes cinematográficas em 2D para **DARKLIGHT: REQUIEM**, combinando o pacote oficial **Unity Timeline (`com.unity.timeline`)** com automação via C# / MCP e sistemas seguros de skip e transição.

---

## 1. Pré-Requisitos e Ambiente

* **Pacote do Projeto:** `com.unity.timeline` v6.6.0 (instalado e validado em `Packages/manifest.json`).
* **Módulo do Motor:** `com.unity.modules.director` v1.0.0.
* **Versão Unity:** 6000.6.0f1 (URP 17.6).
* **Modo de Execução:** Suporta tanto o bridge interativo `unity-open-mcp` (quando o servidor estiver conectado) quanto compilação direta via scripts C# em `Assets/Darklight/Editor/` e execução headless.

---

## 2. Tipos de Faixas (Track Types) e Vocabulário

Um **TimelineAsset** (`.playable`) é um ativo salvo no disco em `Assets/Darklight/Cutscenes/`. Um **PlayableDirector** é o componente de cena que executa esse ativo no runtime.

| Tipo de Faixa (`track_type`) | Classe Unity | Uso em DARKLIGHT: REQUIEM |
|---|---|---|
| `Animation` | `AnimationTrack` | Animações dos personagens (caminhada dramática, poses de ataque, quedas, empunhadura). |
| `Activation` | `ActivationTrack` | Ativação/desativação de atores, cartas de título, balões narrativos e adereços. |
| `Audio` | `AudioTrack` | Vozes de diálogo, efeitos sonoros de impacto de espada e música de cena. |
| `Signal` | `SignalTrack` | Disparo de eventos discretos: tremor de tela, flashes carmesim, início de combate. |
| `Control` | `ControlTrack` | Controle de partículas de Véu/Eclipse, fumaça ou sub-timelines. |
| `Group` | `GroupTrack` | Agrupamento hierárquico (ex: "Atores", "Áudio", "Câmera & Efeitos"). |

---

## 3. Protocolo de Skip Seguro e Rollback Atômico

Cutscenes em DARKLIGHT: REQUIEM devem garantir que o jogador possa pular a cena a qualquer momento sem travar o estado do jogo ou deixar o player invulnerável/preso:

### Arquitetura do Skip Gate:
1. **Input Gate Atômico:** Captura de tecla (`ESC` / `Enter` / Botão de Ação do controle) via Unity Input System.
2. **Fast-Forward & State Catch-up:**
   * Se o jogador pular, o `PlayableDirector.time` é imediatamente adiantado para `duration`.
   * Dispara todos os eventos de `SignalTrack` críticos que garantem a concessão de itens, atualização de objetivos ou destruição de obstáculos.
3. **Rollback de Câmera e Controles:**
   * Reativação imediata do `DarklightCombatant` do jogador.
   * Restauração da câmera para o alvo principal (`Player`) com interpolação suave (sem corte brusco de enquadramento).
   * Limpeza de qualquer ator temporário instanciado exclusivamente para a cinemática.

---

## 4. Receitas Canônicas

### Receita A: Cutscene de Infância (Treino de Fábio e Lucius com Espadas de Madeira)
* **Ativo:** `Assets/Darklight/Cutscenes/ChildhoodTraining.playable`
* **Duração:** 14.0 segundos (30 fps).
* **Faixas Requeridas:**
  1. `GroupTrack` ("Atores"):
     * `AnimationTrack` ("Fabius_Young"): Caminhada até o centro, pose de guarda com espada de madeira, golpes iniciais.
     * `AnimationTrack` ("Lucius_Young"): Defesa, contra-ataque simulado, riso/pausa.
  2. `GroupTrack` ("Áudio"):
     * `AudioTrack` ("SonsDeMadeira"): Clipes de impacto de madeira seca, passos na grama.
     * `AudioTrack` ("TrilhaNostalgia"): Fade-in suave de tema melancólico.
  3. `SignalTrack` ("Narrativa"):
     * Marcador em `1.5s`: Exibe legenda inicial de memória ancestral.
     * Marcador em `12.0s`: Dispara transição visual (fade para branco/sombra) que leva Fábio de volta ao presente diante de Lucien.

### Receita B: Apresentação de Chefe e Início de Combate
* **Ativo:** `Assets/Darklight/Cutscenes/BossEncounter_{NomeChefe}.playable`
* **Duração:** 4.5 segundos.
* **Faixas Requeridas:**
  1. `ActivationTrack` ("HUD"): Desativa minimal HUD durante a introdução.
  2. `AnimationTrack` ("Boss"): Animação de aparição / rugido / despertar da arma.
  3. `SignalTrack` ("Impacto"): Dispara tremor de tela via `DarklightCinematicCamera` e fala do chefe em `DarklightStory.BossLines`.
  4. Final da Timeline: Dispara `DarklightCampaign.StartEncounter(encounter)` e devolve o controle total do combate.

---

## 5. Implementação C# Sem Dependência de Servidor Externo

Caso o servidor MCP não esteja ativo no ambiente local, utilize a automação C# comprovada em `Assets/Darklight/Editor/`:

```csharp
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;

public static class DarklightTimelineBuilder
{
    public static TimelineAsset CreateCutscene(string assetPath, float fps = 30f)
    {
        var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        timeline.editorSettings.frameRate = fps;
        AssetDatabase.CreateAsset(timeline, assetPath);
        AssetDatabase.SaveAssets();
        return timeline;
    }
}
```
