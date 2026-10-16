using UnityEngine;

namespace Darklight
{
    public enum CombatState { Neutral, Attack, Charging, Dodge, Parry, Hurt, Dead }
    public enum StrikeKind { Light, Heavy, Charged, Counter, Skill, Aerial, WeaponSpecial }

    public sealed class AttackDefinition : ScriptableObject
    {
        public string displayName;
        public StrikeKind kind;
        public float startup = .12f, active = .1f, recovery = .22f;
        public float damage = 15f, range = 1.6f, angle = 110f, knockback = .2f;
        [Min(1)] public int hitPulses=1;
        public float lungeSpeed, cooldown;
        public bool pullTarget,blink;
        // Optional authored phases; old profiles retain their original sector behaviour.
        public HitShape hitShape;
        public float innerRange, width=.55f, sweepDegrees, windupMove=.42f,recoveryMove=.42f;
        public float postureDamage=1,chargeSeconds=.45f,hitstop=.035f;
        public bool launch,finisher;
        public float hitCancelFraction=1,whiffCancelFraction=1;
        [Range(0,1)] public float recoveryChainFraction=.35f;
        public float Duration => startup + active + recovery;
    }
    public enum HitShape { Sector, Thrust, Sweep }

    [System.Serializable]
    public sealed class CombatLoadout
    {
        public AttackDefinition[] lights;
        public AttackDefinition heavy, charged, counter, skill, aerial, weaponSpecial;
        public AttackDefinition pauseBranch,heavyBranch,swapEntry;
        public float attackBuffer,defenseBuffer;
        public int weapon;
    }

    public struct CombatInput
    {
        public Vector2 move, aim;
        public bool lightHeld,lightReleased;
        public bool lightPressed, heavyPressed, heavyHeld, heavyReleased, dodgePressed, parryPressed, skillPressed,aerialPressed,weaponSpecialPressed;
    }
}
