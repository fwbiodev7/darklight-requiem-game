// DARKLIGHT: REQUIEM — Cutscene Input Gate
// Blocks gameplay input during cutscenes and restores it safely.

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Controls which input actions are available during cutscene playback.
    /// Integrates with DarklightInput without modifying it directly.
    /// </summary>
    public interface ICutsceneInputGate
    {
        /// <summary>Block all gameplay input. Only cutscene-specific controls remain.</summary>
        void BlockGameplayInput();

        /// <summary>Allow limited input (e.g., skip button only).</summary>
        void AllowSkipOnly();

        /// <summary>Allow dialogue advance input during cutscene dialogues.</summary>
        void AllowDialogueAdvance();

        /// <summary>Restore full gameplay input. Must be idempotent and safe after death/scene change.</summary>
        void RestoreGameplayInput();

        /// <summary>Whether gameplay input is currently blocked.</summary>
        bool IsBlocked { get; }
    }

    /// <summary>
    /// Default implementation that integrates with DarklightInput.
    /// Uses the existing Gameplay action map enable/disable.
    /// </summary>
    public class DarklightInputGate : ICutsceneInputGate
    {
        bool blocked;
        int blockDepth;

        public bool IsBlocked => blocked;

        public void BlockGameplayInput()
        {
            blockDepth++;
            blocked = true;
            var lab = DarklightCombatLab.Instance;
            if (lab != null)
            {
                lab.Input.SetGameplay(false);
                lab.Player?.Model?.SuspendInput();
            }
        }

        public void AllowSkipOnly()
        {
            // Keep gameplay blocked, UI map handles skip via Pause/Submit
            blocked = true;
        }

        public void AllowDialogueAdvance()
        {
            // UI map's DialogueAdvance action remains active by default
            blocked = true;
        }

        public void RestoreGameplayInput()
        {
            blockDepth = System.Math.Max(0, blockDepth - 1);
            if (blockDepth > 0) return;

            blocked = false;
            var lab = DarklightCombatLab.Instance;
            if (lab != null)
                lab.Input.SetGameplay(true);
        }

        /// <summary>Force-restore regardless of depth. Used on death/scene transitions.</summary>
        public void ForceRestore()
        {
            blockDepth = 0;
            blocked = false;
            var lab = DarklightCombatLab.Instance;
            if (lab != null)
                lab.Input.SetGameplay(true);
        }
    }
}

