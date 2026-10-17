using UnityEngine;

namespace Darklight
{
    // Presentation only: gameplay powers are already removed by StopPower on death.
    // Uses an unscaled clock so the death menu cannot freeze the visual reversion.
    public sealed class DarklightDeathReversion
    {
        public const float Duration = .8f;
        public const float ReleaseFraction = .45f;
        int livingForm;
        bool dead;
        float startedAt, elapsed;

        public int SourceForm { get; private set; }
        public float Progress => Mathf.Clamp01(elapsed / Duration);
        public bool Active => dead && SourceForm > 0 && Progress < 1;

        public int Observe(int form, bool isDead, float unscaledNow)
        {
            form = Mathf.Clamp(form, 0, 3);
            if (!isDead)
            {
                livingForm = form;
                dead = false;
                SourceForm = 0;
                elapsed = 0;
                return form;
            }

            if (!dead)
            {
                dead = true;
                SourceForm = livingForm > 0 ? livingForm : form;
                startedAt = unscaledNow;
                elapsed = 0;
            }
            // Multiple presentations in one frame never restart or rewind it.
            elapsed = Mathf.Max(elapsed, unscaledNow - startedAt);
            return Active && Progress < ReleaseFraction ? SourceForm : 0;
        }
    }
}
