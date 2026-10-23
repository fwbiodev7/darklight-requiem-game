using UnityEngine;

namespace Darklight
{
    public enum DarklightAirReaction { None, Launch, Suspend, Throw, Slam }

    // Uses the combat clock: pause/hitstop freeze gravity and all expiry windows.
    public sealed class DarklightAirBody
    {
        public float Height { get; private set; }
        public float Velocity { get; private set; }
        public bool Airborne => Height > .01f || Velocity > 0;
        public int Hits { get; private set; }
        public bool SlamPending { get; private set; }
        float age, hold, immunity;
        // A chase adds no hit/damage and cannot extend the global airborne cap.
        public void HoldForChase()
        {if(Airborne&&!SlamPending&&age<4.5f)hold=Mathf.Max(hold,.35f);}
        public bool React(DarklightAirReaction reaction, float strength = 4, bool resistant = false)
        {
            if (resistant || reaction == DarklightAirReaction.None) return false;
            if (reaction == DarklightAirReaction.Launch)
            {
                if (immunity > 0 || Airborne) return false;
                Height = .06f; Velocity = Mathf.Clamp(strength, 2.6f, 5.5f);
                age = hold = 0; Hits = 0; SlamPending = false; return true;
            }
            if (!Airborne || SlamPending) return false;
            if (reaction == DarklightAirReaction.Slam)
            { Velocity = -12; hold = 0; SlamPending = true; return true; }
            if (Hits >= 10 || age >= 4.5f) return false;
            Hits++;
            if (reaction == DarklightAirReaction.Throw)
            { Height = Mathf.Max(.7f, Height); Velocity = 1.2f; hold = .18f; }
            else { Velocity = Mathf.Max(0, Velocity); hold = .20f; }
            return true;
        }
        // True only on the landing edge, including a large frame crossing the floor.
        public bool Tick(float dt)
        {
            if (dt <= 0) return false;
            immunity = Mathf.Max(0, immunity - dt);
            if (!Airborne) return false;
            age += dt;
            float frozen = Mathf.Min(hold, dt); hold -= frozen;
            float falling = dt - frozen;
            if (age > 4.5f) { hold = 0; Velocity = Mathf.Min(Velocity, -4); falling = dt; }
            Height = Mathf.Max(0, Height + Velocity * falling - 4 * falling * falling);
            Velocity -= 8 * falling;
            if (Height > 0) return false;
            Velocity = hold = 0; immunity = .8f; Hits = 0; return true;
        }
        public void Reset() { Height = Velocity = age = hold = immunity = 0; Hits = 0; SlamPending = false; }
    }
}
