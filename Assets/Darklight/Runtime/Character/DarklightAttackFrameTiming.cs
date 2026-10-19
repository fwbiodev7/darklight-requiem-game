using UnityEngine;

namespace Darklight
{
    public static class DarklightAttackFrameTiming
    {
        public static bool Valid(int count, int startupEnd, int activeEnd) => count >= 4 && count <= 32 &&
            startupEnd >= 1 && activeEnd > startupEnd && activeEnd < count;

        // A long simulation tick can cross contact and finish recovery at once.
        // Keep the confirmed contact visible even when StateTime has reset to zero.
        public static int Frame(int count,int startupEnd,int activeEnd,CombatModel model)
        {
            var attack=model?.Attack;if(attack==null)return 0;
            float time=model.ActiveThisTick?attack.startup+model.ContactProgress*Mathf.Max(0,attack.active-.00001f):model.StateTime;
            return Frame(count,startupEnd,activeEnd,time,attack.startup,attack.active,attack.recovery);
        }

        // Frame boundaries follow existing hitbox windows, never a second combat clock.
        public static int Frame(int count, int startupEnd, int activeEnd, float time,
            float startup, float active, float recovery)
        {
            if (!Valid(count, startupEnd, activeEnd)) return 0;
            if (time < startup) return Sample(0, startupEnd, time, startup);
            if (time < startup + active) return Sample(startupEnd, activeEnd, time - startup, active);
            return Sample(activeEnd, count, time - startup - active, recovery);
        }
        static int Sample(int first, int end, float time, float duration) =>
            Mathf.Min(end - 1, first + Mathf.FloorToInt(Mathf.Clamp01(time / Mathf.Max(.0001f, duration)) * (end - first)));
    }
}
