// DARKLIGHT: REQUIEM — Cutscene Skip Controller
// Ensures mandatory events complete even when skip is requested.

using System.Collections.Generic;

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Tracks mandatory events within a cutscene sequence and ensures
    /// they complete during skip.
    /// </summary>
    public class CutsceneSkipController
    {
        readonly List<MandatoryEvent> mandatoryEvents = new();
        readonly HashSet<string> completedEvents = new();

        public bool AllMandatoryComplete
        {
            get
            {
                foreach (var evt in mandatoryEvents)
                    if (!completedEvents.Contains(evt.Id)) return false;
                return true;
            }
        }

        public int PendingCount
        {
            get
            {
                int count = 0;
                foreach (var evt in mandatoryEvents)
                    if (!completedEvents.Contains(evt.Id)) count++;
                return count;
            }
        }

        /// <summary>Register an event that MUST complete even during skip.</summary>
        public void RegisterMandatory(string id, System.Action executeImmediate)
        {
            mandatoryEvents.Add(new MandatoryEvent { Id = id, Execute = executeImmediate });
        }

        /// <summary>Mark an event as completed (called during normal playback).</summary>
        public void Complete(string id) => completedEvents.Add(id);

        /// <summary>Execute all pending mandatory events immediately (called during skip).</summary>
        public void ExecuteAllPending()
        {
            foreach (var evt in mandatoryEvents)
            {
                if (completedEvents.Contains(evt.Id)) continue;
                evt.Execute?.Invoke();
                completedEvents.Add(evt.Id);
            }
        }

        /// <summary>Reset for new sequence.</summary>
        public void Reset()
        {
            mandatoryEvents.Clear();
            completedEvents.Clear();
        }

        struct MandatoryEvent
        {
            public string Id;
            public System.Action Execute;
        }
    }
}

