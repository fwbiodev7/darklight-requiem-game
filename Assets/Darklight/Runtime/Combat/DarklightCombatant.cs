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
        public bool Receive(float damage, Vector2 force,bool unblockable=false)
        {
            if (Health.IsDead) return false;
            var enemy=player?null:GetComponent<DarklightEnemy>();
            if(enemy!=null&&(!enemy.AcceptDamage||enemy.EvadeStrike()))return false;
            if(enemy!=null)damage=enemy.ProtectPurification(damage);
            if (fighter != null && (fighter.Model.Invulnerable||(!unblockable&&fighter.Model.Defend(-force))))
            { DarklightCombatLab.Instance?.Feedback(transform.position + Vector3.up, true);DarklightCombatLab.Instance?.Coach?.Record(fighter.Model.Invulnerable?DarklightLesson.Dodge:DarklightLesson.Parry); if(fighter.Model.PerfectParry&&!unblockable)DarklightCampaign.Instance?.Defended(); return false; }
            if(player)damage *= DarklightCampaign.Instance?.IncomingMultiplier ?? 1;
            if(player && (Health.IsInvincible||DarklightCampaign.Instance?.ShadowInvulnerable==true))return false;
            if(player&&DarklightCampaign.Instance!=null)damage=DarklightCampaign.Instance.ProtectAwakening(damage);
            Health.TakeDamage(damage);
            if(player) { if(DarklightCampaign.Instance != null)Health.SetInvincibilityDuration(.28f); DarklightCampaign.Instance?.Damaged(); }
            fighter?.Model.Hurt(Health.IsDead,DarklightCampaign.Instance?.ResistsStagger==true?.12f:.18f);
            DarklightCombatLab.Instance?.Feedback(transform.position + Vector3.up, false);
            return true;
        }
    }
}
