// DARKLIGHT: REQUIEM — Cutscene Infrastructure
// Antigravity Support Engineering — 2026-11-04
// Interfaces and contracts for Codex integration.
// Does NOT create Timelines, sprites, animations or scenes.

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Represents a single cutscene sequence that the CutsceneManager can execute.
    /// Codex implements concrete sequences; Antigravity provides the execution contract.
    /// </summary>
    public interface ICutsceneSequence
    {
        /// <summary>Unique identifier for save/load and event tracking.</summary>
        string Id { get; }

        /// <summary>Human-readable name for debug and logging.</summary>
        string DisplayName { get; }

        /// <summary>Whether this sequence can be skipped by the player.</summary>
        bool Skippable { get; }

        /// <summary>Whether skip still requires mandatory events to complete.</summary>
        bool HasMandatoryEvents { get; }

        /// <summary>Called once when the sequence begins. Setup cameras, actors, etc.</summary>
        void Begin(CutsceneContext context);

        /// <summary>Called every frame while the sequence is active.</summary>
        /// <returns>true while running; false when complete.</returns>
        bool Tick(float deltaTime, CutsceneContext context);

        /// <summary>Called when skip is requested. Must complete mandatory events immediately.</summary>
        void Skip(CutsceneContext context);

        /// <summary>Called when the sequence ends (normal or skip). Cleanup.</summary>
        void End(CutsceneContext context);

        /// <summary>Validates that all required references are present before execution.</summary>
        CutsceneValidationResult Validate();
    }

    /// <summary>
    /// Context object passed to sequences providing access to game systems
    /// without direct coupling.
    /// </summary>
    public class CutsceneContext
    {
        public ICutsceneCamera Camera { get; set; }
        public ICutsceneInputGate InputGate { get; set; }
        public ICutsceneEventBridge EventBridge { get; set; }
        public ICutsceneCheckpointBridge CheckpointBridge { get; set; }

        /// <summary>Current campaign progress reference (read-only during cutscene).</summary>
        public object Progress { get; set; }

        /// <summary>Set by the manager; sequences can read elapsed time.</summary>
        public float ElapsedTime { get; set; }
    }

    /// <summary>Result of sequence validation.</summary>
    public class CutsceneValidationResult
    {
        public bool Valid { get; set; }
        public string[] Errors { get; set; }
        public string[] Warnings { get; set; }

        public static CutsceneValidationResult Ok() =>
            new CutsceneValidationResult { Valid = true, Errors = System.Array.Empty<string>(), Warnings = System.Array.Empty<string>() };

        public static CutsceneValidationResult Fail(params string[] errors) =>
            new CutsceneValidationResult { Valid = false, Errors = errors, Warnings = System.Array.Empty<string>() };
    }
}

