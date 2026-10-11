using UnityEngine;

namespace Darklight
{
    [RequireComponent(typeof(HealthSystem))]
    public sealed class DarklightCombatant : MonoBehaviour
    {
        public bool player;
        public HealthSystem Health { get; private set; }
        DarklightFighter fighter;
        void Awake() { Health = GetComponent<HealthSystem>(); Health.SetInvincibilityDuration(player && DarklightCampaign.Instance!=null?.28f:0); fighter = GetComponent<DarklightFighter>(); }
        public bool Receive(float damage, Vector2 force)
        {
            if (Health.IsDead) return false;
            if(!player&&GetComponent<DarklightEnemy>()?.EvadeStrike()==true)return false;
            if (fighter != null && fighter.Model.Defend(-force))
            { DarklightCombatLab.Instance?.Feedback(transform.position + Vector3.up, true); if(fighter.Model.PerfectParry)DarklightCampaign.Instance?.Defended(); return false; }
            if(player)damage *= DarklightCampaign.Instance?.IncomingMultiplier ?? 1;
            if(player && Health.IsInvincible)return false;
            Health.TakeDamage(damage);
            if(player) { if(DarklightCampaign.Instance != null)Health.SetInvincibilityDuration(.28f); DarklightCampaign.Instance?.Damaged(); }
            fighter?.Model.Hurt(Health.IsDead,DarklightCampaign.Instance?.ResistsStagger==true?.12f:.18f);
            DarklightCombatLab.Instance?.Feedback(transform.position + Vector3.up, false);
            return true;
        }
    }
}
