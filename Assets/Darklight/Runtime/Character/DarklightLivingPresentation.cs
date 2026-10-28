using UnityEngine;

namespace Darklight
{
    // Presentation only. Never changes CombatModel, damage, movement or input.
    public sealed class DarklightLivingPresentation
    {
        public string Clip { get; private set; } = "Idle";
        public float BreathClock { get; private set; }
        public float ClipTime { get; private set; }
        public bool Critical { get; private set; }
        float lastClock, idleTime, nextVariation = 8, variationRemaining;
        int variation;
        bool initialized, wasMoving;
        Vector2 lastMovement;

        public void Observe(float clock, CombatState state, Vector2 movement, float health, int form, bool threat, bool transforming)
        {
            float dt = initialized && clock >= lastClock ? Mathf.Min(.25f, clock - lastClock) : 0;
            if (initialized && clock < lastClock) Reset(); // respawn resets the combat clock
            initialized = true; lastClock = clock;
            ClipTime += dt;
            Critical = health <= .25f || Critical && health < .32f;
            if (state == CombatState.Dead) { SetClip("Death"); return; }
            if (state != CombatState.Neutral || transforming)
            {
                idleTime = variationRemaining = 0; nextVariation = 8; wasMoving = false;
                SetClip(transforming ? "Transformation" : state == CombatState.Attack ? "Attack" :
                    state == CombatState.Dodge ? "Dodge" : state == CombatState.Parry ? "Parry" :
                    state == CombatState.Hurt ? "Hurt" : "Charging"); return;
            }
            bool moving = movement.sqrMagnitude > .01f;
            if (moving)
            {
                idleTime = variationRemaining = 0; nextVariation = 8;
                bool turning = wasMoving && Vector2.Dot(lastMovement.normalized, movement.normalized) < -.4f;
                SetClip(turning || Clip == "Turn" && ClipTime < .14f ? "Turn" : !wasMoving || Clip == "StartMove" && ClipTime < .12f ? "StartMove" : "Walk");
                wasMoving = true; lastMovement = movement; return;
            }
            if (wasMoving) { SetClip("Brake"); wasMoving = false; }
            if (Clip == "Brake" && ClipTime < .14f) return;
            idleTime += dt;
            // Different cadence uses existing whole idle frames; no body deformation.
            BreathClock += dt * (Critical ? 1.35f : health < .55f ? 1.15f : form == 1 ? 1.08f : form == 2 ? .92f : 1);
            if (Critical) { SetClip("IdleWounded"); return; }
            if (threat) { SetClip("IdleAlert"); variationRemaining = 0; return; }
            if (variationRemaining > 0) { variationRemaining -= dt; return; }
            if (idleTime >= nextVariation)
            {
                variation = (variation + 1) % 3;
                SetClip(variation == 0 ? "IdleInspect" : variation == 1 ? "IdleImpatient" : "IdleFatigued");
                variationRemaining = 1.1f; nextVariation = idleTime + 8 + variation * 2; return;
            }
            SetClip("Idle");
        }

        void SetClip(string value) { if (Clip == value) return; Clip = value; ClipTime = 0; }
        public void Reset()
        {
            initialized = wasMoving = Critical = false; Clip = "Idle";
            BreathClock = ClipTime = idleTime = variationRemaining = lastClock = 0;
            nextVariation = 8; variation = 0; lastMovement = Vector2.zero;
        }
    }
}
