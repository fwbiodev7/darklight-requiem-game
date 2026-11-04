// DARKLIGHT: REQUIEM — Cutscene Event Bridge
// Bidirectional communication between cutscenes, gameplay, and narrative.

using System;
using System.Collections.Generic;

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Event types that cutscenes can emit or listen to.
    /// </summary>
    public enum CutsceneEventType
    {
        // Lifecycle
        SequenceStarted,
        SequenceCompleted,
        SequenceSkipped,
        SequenceCancelled,

        // Narrative
        DialogueStarted,
        DialogueCompleted,
        ObjectiveUpdated,
        MilestoneReached,

        // Combat integration
        BossIntroStarted,
        BossIntroCompleted,
        TransformationTriggered,
        TransformationCompleted,

        // Scene
        SceneTransitionRequested,
        FlashbackStarted,
        FlashbackEnded,

        // Player state
        PlayerDied,
        PlayerDamaged,
        CheckpointReached
    }

    /// <summary>
    /// Data accompanying a cutscene event.
    /// </summary>
    public struct CutsceneEvent
    {
        public CutsceneEventType Type;
        public string SequenceId;
        public string Data;
        public float Timestamp;

        public CutsceneEvent(CutsceneEventType type, string sequenceId, string data = null)
        {
            Type = type;
            SequenceId = sequenceId;
            Data = data;
            Timestamp = UnityEngine.Time.time;
        }
    }

    /// <summary>
    /// Contract for event communication between cutscenes and game systems.
    /// </summary>
    public interface ICutsceneEventBridge
    {
        /// <summary>Emit an event from cutscene to game systems.</summary>
        void Emit(CutsceneEvent evt);

        /// <summary>Subscribe to events from game systems during cutscene.</summary>
        void Subscribe(CutsceneEventType type, Action<CutsceneEvent> handler);

        /// <summary>Unsubscribe handler.</summary>
        void Unsubscribe(CutsceneEventType type, Action<CutsceneEvent> handler);

        /// <summary>Clear all subscriptions. Called on cutscene end.</summary>
        void ClearAll();
    }

    /// <summary>
    /// Default implementation with simple delegate-based pub/sub.
    /// </summary>
    public class CutsceneEventBridge : ICutsceneEventBridge
    {
        readonly Dictionary<CutsceneEventType, List<Action<CutsceneEvent>>> handlers = new();
        readonly List<CutsceneEvent> history = new();

        public IReadOnlyList<CutsceneEvent> History => history;

        public void Emit(CutsceneEvent evt)
        {
            history.Add(evt);
            if (handlers.TryGetValue(evt.Type, out var list))
            {
                // Iterate copy to allow modification during callbacks
                var snapshot = list.ToArray();
                foreach (var handler in snapshot)
                    handler?.Invoke(evt);
            }
        }

        public void Subscribe(CutsceneEventType type, Action<CutsceneEvent> handler)
        {
            if (!handlers.TryGetValue(type, out var list))
            {
                list = new List<Action<CutsceneEvent>>();
                handlers[type] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe(CutsceneEventType type, Action<CutsceneEvent> handler)
        {
            if (handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public void ClearAll()
        {
            handlers.Clear();
            // History is preserved for debugging
        }

        public bool HasEvent(string sequenceId, CutsceneEventType type)
        {
            foreach (var evt in history)
                if (evt.SequenceId == sequenceId && evt.Type == type) return true;
            return false;
        }

        public void ClearHistory() => history.Clear();
    }
}

