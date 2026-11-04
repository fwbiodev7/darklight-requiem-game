using UnityEngine;

namespace Darklight
{
    /// <summary>
    /// AAA Combat Juice & Game Feel Engine for Darklight: Requiem.
    /// Provides zero-GC hitstop, traumatic camera recoil, impact frames, and visual punch.
    /// </summary>
    public static class DarklightCombatJuice
    {
        // Hitstop durations tuned for visceral response
        public const float HitstopLight = 0.045f;
        public const float HitstopHeavy = 0.095f;
        public const float HitstopParry = 0.160f;
        public const float HitstopLethal = 0.220f;

        public static void ApplyHitImpact(Vector3 impactPoint, bool heavy, bool lethal = false, bool isParry = false)
        {
            var lab = DarklightCombatLab.Instance;
            if (lab == null) return;

            // 1. Visceral Hit-Stop
            float stopDuration = isParry ? HitstopParry : lethal ? HitstopLethal : heavy ? HitstopHeavy : HitstopLight;
            lab.Hitstop(stopDuration);

            // 2. Traumatic Directional Camera Shake
            var cam = lab.WorldCamera?.GetComponent<DarklightLabCamera>();
            if (cam != null)
            {
                cam.ConfirmContact(impactPoint);
                cam.Impulse(heavy || isParry ? 2 : 1);
            }
        }

        public static void TriggerParryJuice(Vector3 position)
        {
            ApplyHitImpact(position, heavy: true, lethal: false, isParry: true);
            DarklightCombatFX.Impact(position, true);
        }
    }
}
