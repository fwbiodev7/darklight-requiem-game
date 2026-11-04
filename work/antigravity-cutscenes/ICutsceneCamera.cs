// DARKLIGHT: REQUIEM — Cutscene Camera Bridge
// Contract for Codex to connect Cinemachine or custom camera control.

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Abstraction over camera control during cutscenes.
    /// Codex provides the concrete implementation using Cinemachine or manual control.
    /// </summary>
    public interface ICutsceneCamera
    {
        /// <summary>Lock camera to a world position.</summary>
        void MoveTo(UnityEngine.Vector3 position, float duration = 0f);

        /// <summary>Follow a transform smoothly.</summary>
        void Follow(UnityEngine.Transform target, float smoothTime = 0.3f);

        /// <summary>Set orthographic size for zoom effects.</summary>
        void SetZoom(float size, float duration = 0f);

        /// <summary>Apply screen shake.</summary>
        void Shake(float intensity, float duration);

        /// <summary>Return camera control to gameplay.</summary>
        void ReleaseToGameplay();

        /// <summary>Whether camera is currently under cutscene control.</summary>
        bool IsActive { get; }
    }
}

