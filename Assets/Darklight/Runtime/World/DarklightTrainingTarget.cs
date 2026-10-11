using UnityEngine;

namespace Darklight
{
    /// <summary>Stationary lab fixture for timing defense, not an enemy AI.</summary>
    [RequireComponent(typeof(HealthSystem), typeof(DarklightCombatant))]
    public sealed class DarklightTrainingTarget : MonoBehaviour
    {
        public bool DefensePractice;
        public HealthSystem Health { get; private set; }
        float clock;
        bool fired;
        SpriteRenderer display;
        void Awake() { Health = GetComponent<HealthSystem>(); Health.SetMaxHealth(150, false); display = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            var lab = DarklightCombatLab.Instance;
            if (lab == null || !lab.Ready || lab.CombatDelta <= 0) return;
            if (Health.IsDead) { display.color = new Color(.3f, .25f, .25f); return; }
            if (!DefensePractice) { display.color = Color.white; return; }
            clock += lab.CombatDelta;
            if (clock >= 3f) { clock %= 3f; fired = false; }
            bool warning = clock >= 2.1f && clock < 2.7f;
            display.color = warning ? new Color(.5f, .7f, 1) : Color.white;
            if (clock < 2.7f || fired) return;
            fired = true;
            Vector2 difference = lab.Player.transform.position - transform.position;
            if (difference.magnitude <= 2f) lab.Player.GetComponent<DarklightCombatant>().Receive(12, difference.normalized);
        }
    }
}
