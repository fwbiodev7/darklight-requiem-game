using UnityEngine;

namespace Darklight
{
    /// <summary>
    /// AAA Permissive Input Buffer (120ms) for Darklight: Requiem.
    /// Eliminates dropped inputs by buffering attack and dodge intents during recovery frames.
    /// </summary>
    public sealed class DarklightCombatBuffer : MonoBehaviour
    {
        public const float BufferWindow = 0.120f; // 120ms industry standard buffer window

        float lightBufferTimer;
        float heavyBufferTimer;
        float dodgeBufferTimer;
        float parryBufferTimer;
        float specialBufferTimer;

        public void FeedInput(ref CombatInput input, float dt)
        {
            // Register fresh button presses
            if (input.lightPressed) lightBufferTimer = BufferWindow;
            if (input.heavyPressed) heavyBufferTimer = BufferWindow;
            if (input.dodgePressed) dodgeBufferTimer = BufferWindow;
            if (input.parryPressed) parryBufferTimer = BufferWindow;
            if (input.weaponSpecialPressed) specialBufferTimer = BufferWindow;

            // Decay buffer timers
            if (dt > 0)
            {
                lightBufferTimer = Mathf.Max(0, lightBufferTimer - dt);
                heavyBufferTimer = Mathf.Max(0, heavyBufferTimer - dt);
                dodgeBufferTimer = Mathf.Max(0, dodgeBufferTimer - dt);
                parryBufferTimer = Mathf.Max(0, parryBufferTimer - dt);
                specialBufferTimer = Mathf.Max(0, specialBufferTimer - dt);
            }
        }

        public void ApplyBufferedInputs(ref CombatInput input, bool isReadyForAction)
        {
            if (!isReadyForAction) return;

            if (dodgeBufferTimer > 0)
            {
                input.dodgePressed = true;
                dodgeBufferTimer = 0;
                return;
            }

            if (parryBufferTimer > 0)
            {
                input.parryPressed = true;
                parryBufferTimer = 0;
                return;
            }

            if (specialBufferTimer > 0)
            {
                input.weaponSpecialPressed = true;
                specialBufferTimer = 0;
                return;
            }

            if (heavyBufferTimer > 0)
            {
                input.heavyPressed = true;
                heavyBufferTimer = 0;
                return;
            }

            if (lightBufferTimer > 0)
            {
                input.lightPressed = true;
                lightBufferTimer = 0;
                return;
            }
        }

        public void Clear()
        {
            lightBufferTimer = 0;
            heavyBufferTimer = 0;
            dodgeBufferTimer = 0;
            parryBufferTimer = 0;
            specialBufferTimer = 0;
        }
    }
}
