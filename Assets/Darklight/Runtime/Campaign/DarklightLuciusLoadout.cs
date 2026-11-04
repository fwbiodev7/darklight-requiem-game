// DarklightLuciusLoadout.cs
// Candidata - NAO INTEGRAR antes de Fabius Normal + Gothic Eclipse + sprites aprovados.
// Destino proposto: Assets/Darklight/Runtime/Campaign/DarklightLuciusLoadout.cs

using UnityEngine;

namespace Darklight
{
    /// <summary>
    /// Loadout de combate exclusivo de Lucius Veyr. Construido em runtime.
    /// Nao compartilha instancias com as armas de Fabius.
    /// Ativar apenas quando sprites de Lucius estiverem integrados.
    /// </summary>
    public static class DarklightLuciusLoadout
    {
        public static float ComboStartup      = 0.14f;
        public static float ComboActive       = 0.12f;
        public static float ComboRecovery     = 0.26f;
        public static float ComboRecoveryChain= 0.45f;
        public static float ComboDamage       = 8f;
        public static float ComboRange        = 2.2f;
        public static float HeavyStartup      = 0.28f;
        public static float HeavyActive       = 0.18f;
        public static float HeavyRecovery     = 0.55f;
        public static float HeavyDamage       = 16f;
        public static float HeavyRange        = 2.6f;
        public static float HeavyCooldown     = 1.8f;
        public static float SpecialStartup    = 0.32f;
        public static float SpecialActive     = 0.22f;
        public static float SpecialRecovery   = 0.70f;
        public static float SpecialDamage     = 22f;
        public static float SpecialRange      = 3.4f;
        public static float SpecialCooldown   = 6.0f;
        public static float PostureDamageLight  = 0.9f;
        public static float PostureDamageHeavy  = 2.8f;
        public static float PostureDamageSpecial= 4.0f;

        static CombatLoadout _instance;

        public static CombatLoadout Get()
        {
            if (_instance != null) return _instance;
            _instance = Build();
            return _instance;
        }

        public static void Reset() => _instance = null;

        static CombatLoadout Build()
        {
            var l = new CombatLoadout
            {
                weapon = -1,
                attackBuffer  = 0.20f,
                defenseBuffer = 0.20f
            };
            l.lights = new[]
            {
                MakeAttack("Lucius_Slash1", StrikeKind.Light, ComboDamage, ComboRange,
                    ComboStartup, ComboActive, ComboRecovery, PostureDamageLight, ComboRecoveryChain, false),
                MakeAttack("Lucius_Slash2", StrikeKind.Light, ComboDamage*1.1f, ComboRange,
                    ComboStartup*0.9f, ComboActive, ComboRecovery, PostureDamageLight, ComboRecoveryChain, false),
                MakeAttack("Lucius_Slash3", StrikeKind.Light, ComboDamage*1.3f, ComboRange*1.1f,
                    ComboStartup*0.85f, ComboActive*1.2f, ComboRecovery*1.1f, PostureDamageLight*1.4f, 1f, false)
            };
            l.heavy = MakeAttack("Lucius_Heavy", StrikeKind.Heavy, HeavyDamage, HeavyRange,
                HeavyStartup, HeavyActive, HeavyRecovery, PostureDamageHeavy, 1f, false,
                cooldown:HeavyCooldown, angle:140f, shape:HitShape.Sector);
            l.skill = MakeAttack("Lucius_Special", StrikeKind.Skill, SpecialDamage, SpecialRange,
                SpecialStartup, SpecialActive, SpecialRecovery, PostureDamageSpecial, 1f, true,
                cooldown:SpecialCooldown, angle:220f, shape:HitShape.Sweep, hitPulses:2);
            l.charged = MakeAttack("Lucius_Charge", StrikeKind.Charged, HeavyDamage*1.5f, HeavyRange*1.2f,
                0.55f, 0.20f, 0.65f, PostureDamageHeavy*1.8f, 1f, true,
                cooldown:2.5f, angle:160f, shape:HitShape.Sector);
            l.aerial = MakeAttack("Lucius_Aerial", StrikeKind.Aerial, HeavyDamage*0.8f, ComboRange,
                0.10f, 0.16f, 0.30f, PostureDamageHeavy, 1f, false,
                cooldown:1.2f, angle:100f, shape:HitShape.Sector);
            l.aerial.launch = true;
            return l;
        }

        static AttackDefinition MakeAttack(
            string id, StrikeKind kind, float damage, float range,
            float startup, float active, float recovery, float posture,
            float recoveryChainFrac, bool finisher,
            float cooldown=0f, float angle=120f,
            HitShape shape=HitShape.Sector, int hitPulses=1)
        {
            var a = ScriptableObject.CreateInstance<AttackDefinition>();
            a.name=id; a.kind=kind; a.damage=damage; a.range=range;
            a.startup=startup; a.active=active; a.recovery=recovery;
            a.recoveryChainFraction=recoveryChainFrac;
            a.whiffCancelFraction=kind==StrikeKind.Light?1f:1.4f;
            a.hitCancelFraction=1f; a.postureDamage=posture;
            a.finisher=finisher; a.hitPulses=hitPulses;
            a.cooldown=cooldown; a.angle=angle; a.hitShape=shape;
            a.hitstop=kind==StrikeKind.Light?.022f:kind==StrikeKind.Charged?.055f:.038f;
            a.windupMove=0.50f; a.recoveryMove=0.60f;
            return a;
        }
    }
}