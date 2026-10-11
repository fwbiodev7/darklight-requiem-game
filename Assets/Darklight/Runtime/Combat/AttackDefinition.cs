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
        public float Duration => startup + active + recovery;
    }

    [System.Serializable]
    public sealed class CombatLoadout
    {
        public AttackDefinition[] lights;
        public AttackDefinition heavy, charged, counter, skill, aerial, weaponSpecial;
    }

    public struct CombatInput
    {
        public Vector2 move, aim;
        public bool lightPressed, heavyPressed, heavyHeld, heavyReleased, dodgePressed, parryPressed, skillPressed,aerialPressed,weaponSpecialPressed;
    }
}
