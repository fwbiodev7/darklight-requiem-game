// DARKLIGHT: REQUIEM — Cutscene Checkpoint Bridge
// Integrates cutscene state with the existing DarklightProgress save/load.

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Contract for save/load integration during cutscenes.
    /// </summary>
    public interface ICutsceneCheckpointBridge
    {
        /// <summary>Save current cutscene state to progress.</summary>
        void SaveCutsceneState(string sequenceId, float elapsed, string customData);

        /// <summary>Load saved cutscene state. Returns null if no saved state.</summary>
        CutsceneSaveData LoadCutsceneState();

        /// <summary>Clear saved cutscene state (after completion).</summary>
        void ClearCutsceneState();

        /// <summary>Mark a cutscene as watched (for skip-on-replay).</summary>
        void MarkWatched(string sequenceId);

        /// <summary>Check if a cutscene has been watched before.</summary>
        bool HasWatched(string sequenceId);
    }

    /// <summary>
    /// Serializable cutscene save data that can be stored in DarklightProgress.
    /// </summary>
    [System.Serializable]
    public class CutsceneSaveData
    {
        public string sequenceId;
        public float elapsed;
        public string customData;
        public bool interrupted;

        public bool IsValid => !string.IsNullOrEmpty(sequenceId);
    }

    /// <summary>
    /// Default implementation that uses DarklightProgress fields.
    /// Codex extends DarklightProgress with the necessary serialized fields.
    /// </summary>
    public class DarklightCheckpointBridge : ICutsceneCheckpointBridge
    {
        CutsceneSaveData pending;
        readonly System.Collections.Generic.HashSet<string> watched = new();

        public void SaveCutsceneState(string sequenceId, float elapsed, string customData)
        {
            pending = new CutsceneSaveData
            {
                sequenceId = sequenceId,
                elapsed = elapsed,
                customData = customData,
                interrupted = false
            };
            // Codex integration: write to Progress.cutsceneState
        }

        public CutsceneSaveData LoadCutsceneState()
        {
            // Codex integration: read from Progress.cutsceneState
            return pending?.IsValid == true ? pending : null;
        }

        public void ClearCutsceneState()
        {
            pending = null;
            // Codex integration: clear Progress.cutsceneState
        }

        public void MarkWatched(string sequenceId) => watched.Add(sequenceId);
        public bool HasWatched(string sequenceId) => watched.Contains(sequenceId);
    }
}

