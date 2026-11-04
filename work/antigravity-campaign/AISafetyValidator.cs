// DARKLIGHT: REQUIEM — Combat AI Safety & Deadlock Prevention
// Antigravity Support Engineering — 2026-11-04
// Audited against enemy state machines to prevent infinite attack loops, stuck chasing, or invalid damage reactions.

using System;
using UnityEngine;

namespace Darklight.Combat.AI
{
    public static class AISafetyValidator
    {
        public const float MaxAttackPrepTime = 3.0f;
        public const float MaxPursuitWithoutSight = 5.0f;

        /// <summary>
        /// Validates if an enemy state machine is stuck in windup or chasing endlessly.
        /// </summary>
        public static bool CheckAndRecoverStuckState(ref int currentState, ref float timer, float stateTime, bool hasLineOfSight)
        {
            // state 1 = windup / preparing
            if (currentState == 1 && stateTime > MaxAttackPrepTime)
            {
                // Force break out of preparation deadlock
                currentState = 0; // return to idle/vigilance
                timer = 0.5f;
                return true; // recovered
            }

            // state 0 = pursuit / moving
            if (currentState == 0 && !hasLineOfSight && stateTime > MaxPursuitWithoutSight)
            {
                // Reset pathing and idle momentarily
                timer = 1.0f;
                return true; // recovered
            }

            return false;
        }

        /// <summary>
        /// Ensures damage reactions do not result in NaN velocities or infinite knockback.
        /// </summary>
        public static Vector2 SanitizeKnockback(Vector2 rawForce, float maxForce = 5.0f)
        {
            if (float.IsNaN(rawForce.x) || float.IsNaN(rawForce.y))
                return Vector2.zero;

            return Vector2.ClampMagnitude(rawForce, maxForce);
        }
    }
}
