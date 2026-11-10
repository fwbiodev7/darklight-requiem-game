using UnityEngine;

namespace Darklight
{
    [RequireComponent(typeof(HealthSystem))]
    public sealed class DarklightCombatant : MonoBehaviour
    {
        public bool player;
        HealthSystem _health;
        public HealthSystem Health => _health != null ? _health : (_health = GetComponent<HealthSystem>());
        DarklightFighter _fighter;
        DarklightFighter fighter => _fighter != null ? _fighter : (_fighter = GetComponent<DarklightFighter>());
        DarklightEnemy _enemy;
        DarklightEnemy enemy => _enemy != null ? _enemy : (_enemy = GetComponent<DarklightEnemy>());
        void Awake()
        {
            _health = GetComponent<HealthSystem>();
            if (_health != null) _health.SetInvincibilityDuration(player && DarklightCampaign.Instance!=null?.28f:0);
            _fighter = GetComponent<DarklightFighter>();
            _enemy = GetComponent<DarklightEnemy>();
        }
        public bool Receive(float damage, Vector2 force,bool unblockable=false)
        {
            if (Health.IsDead) return false;
            var enemy=player?null:GetComponent<DarklightEnemy>();
            if(enemy!=null&&(!enemy.AcceptDamage||enemy.TryGuardHit(force)||enemy.EvadeStrike()))return false;
            if(enemy!=null)damage=enemy.ProtectPurification(damage);
            var combatModel=fighter?.Model??enemy?.CombatModel;
            if (combatModel != null && (combatModel.Invulnerable||(!unblockable&&combatModel.Defend(-force))))
            { DarklightCombatLab.Instance?.Feedback(transform.position + Vector3.up, true);DarklightCombatLab.Instance?.Coach?.Record(combatModel.Invulnerable?DarklightLesson.Dodge:DarklightLesson.Parry); if(player&&combatModel.Invulnerable)DarklightCampaign.Instance?.Evaded(combatModel.Clock-combatModel.StateTime);else if(player&&combatModel.PerfectParry&&!unblockable)DarklightCampaign.Instance?.Defended(); return false; }
            if(player)damage *= DarklightCampaign.Instance?.IncomingMultiplier ?? 1;
            if(player && (Health.IsInvincible||DarklightCampaign.Instance?.ShadowInvulnerable==true))return false;
            if(player&&DarklightCampaign.Instance?.Training==true&&!DarklightCampaign.Instance.TrainingDamage)return false;
            if(player&&DarklightCampaign.Instance!=null)damage=DarklightCampaign.Instance.ProtectAwakening(damage);
            float before=Health.CurrentHealth;Health.TakeDamage(damage);float lost=before-Health.CurrentHealth;
            if(lost<=0)return false;
            if(player) { if(DarklightCampaign.Instance != null)Health.SetInvincibilityDuration(.28f); DarklightCampaign.Instance?.Damaged(lost); }
            combatModel?.Hurt(Health.IsDead,DarklightCampaign.Instance?.ResistsStagger==true?.12f:.18f);
            DarklightCombatLab.Instance?.Feedback(transform.position + Vector3.up, false);
            return true;
        }
    }
}
