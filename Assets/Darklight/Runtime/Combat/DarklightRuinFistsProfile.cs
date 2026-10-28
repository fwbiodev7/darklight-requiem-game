using System;
using UnityEngine;

namespace Darklight
{
    public enum DarklightRuinStrike { None, Punch, SideKick, SpinKick, Uppercut, GroundSlam, AerialSlam, Cyclone }

    // The third slot remains 2 so existing unlocks and save selections are valid.
    public static class DarklightRuinFistsProfile
    {
        public const string DisplayName="PUNHOS DA RUÍNA";
        public const int WeaponIndex=2;
        public static Color EnergyColor(int form)=>form==3?new Color(.88f,.95f,1):
            form==2?new Color(.66f,.12f,1):form==1?new Color(1,.035f,.085f):new Color(.54f,.065f,.16f);

        public static CombatLoadout Create(Action<UnityEngine.Object> own)
        {
            AttackDefinition Make(string name,StrikeKind kind,DarklightRuinStrike visual,float damage,
                float startup,float active,float recovery,float range,float angle,float posture)
            {
                var a=ScriptableObject.CreateInstance<AttackDefinition>();own(a);
                a.name="RuinFists_"+name;a.displayName=name;a.kind=kind;a.ruinStrike=visual;
                a.damage=damage;a.startup=startup;a.active=active;a.recovery=recovery;
                a.range=range;a.angle=angle;a.postureDamage=posture;a.hitPulses=1;
                a.hitShape=visual==DarklightRuinStrike.Punch?HitShape.Thrust:
                    visual==DarklightRuinStrike.SpinKick||visual==DarklightRuinStrike.Cyclone?HitShape.Sweep:HitShape.Sector;
                a.width=.75f;a.knockback=kind==StrikeKind.Light?.18f:.45f;
                a.windupMove=.45f;a.recoveryMove=.62f;a.hitstop=kind==StrikeKind.Light?.03f:.05f;
                a.hitCancelFraction=1;a.whiffCancelFraction=1.25f;a.recoveryChainFraction=.35f;
                a.chargeSeconds=.65f;return a;
            }
            var l=new CombatLoadout{weapon=WeaponIndex,attackBuffer=.22f,defenseBuffer=.18f};
            l.lights=new[]{
                Make("Direto da Ruína",StrikeKind.Light,DarklightRuinStrike.Punch,12,.075f,.065f,.13f,1.25f,75,.7f),
                Make("Chute Quebra-Osso",StrikeKind.Light,DarklightRuinStrike.SideKick,14,.075f,.085f,.16f,1.55f,100,.9f),
                Make("Giro Devastador",StrikeKind.Light,DarklightRuinStrike.SpinKick,16,.11f,.12f,.2f,1.8f,220,1.3f),
                Make("Martelo da Ruína",StrikeKind.Light,DarklightRuinStrike.GroundSlam,20,.16f,.10f,.26f,1.6f,130,2.3f)};
            l.lights[3].finisher=true;l.lights[3].knockback=.4f;l.lights[3].hitstop=.045f;
            l.heavy=Make("Ascensão da Ruína",StrikeKind.Heavy,DarklightRuinStrike.Uppercut,24,.14f,.12f,.28f,1.4f,105,2.5f);
            l.heavy.launch=true;l.heavy.lungeSpeed=.45f;
            l.charged=Make("Juízo da Ruína",StrikeKind.Charged,DarklightRuinStrike.GroundSlam,40,.22f,.14f,.4f,1.8f,150,4.2f);
            l.charged.finisher=true;l.charged.knockback=.75f;l.charged.hitstop=.065f;l.charged.windupMove=.2f;
            l.counter=Make("Resposta Brutal",StrikeKind.Counter,DarklightRuinStrike.Punch,32,.065f,.10f,.24f,1.35f,95,3.8f);
            l.counter.finisher=true;
            l.skill=Make("Cataclismo da Ruína",StrikeKind.Skill,DarklightRuinStrike.GroundSlam,36,.28f,.16f,.44f,2.15f,360,4);
            l.skill.finisher=true;l.skill.cooldown=3;l.skill.knockback=.7f;
            l.aerial=Make("Queda da Ruína",StrikeKind.Aerial,DarklightRuinStrike.AerialSlam,28,.21f,.12f,.3f,1.85f,150,3.2f);
            l.aerial.cooldown=1.25f;l.aerial.finisher=true;l.aerial.knockback=.55f;
            l.aerial.airReaction=DarklightAirReaction.Slam;l.airFinisher=l.aerial;
            l.weaponSpecial=Make("Espiral da Ruína",StrikeKind.WeaponSpecial,DarklightRuinStrike.Cyclone,11,.18f,.4f,.38f,1.9f,300,1.2f);
            l.weaponSpecial.hitPulses=3;l.weaponSpecial.cooldown=7;l.weaponSpecial.finisher=true;
            l.pauseBranch=Make("Órbita Devastadora",StrikeKind.Light,DarklightRuinStrike.SpinKick,21,.16f,.14f,.25f,1.85f,280,1.7f);
            l.heavyBranch=Make("Sentença da Ruína",StrikeKind.Heavy,DarklightRuinStrike.GroundSlam,34,.22f,.12f,.32f,1.8f,150,3.7f);
            l.heavyBranch.finisher=true;l.heavyBranch.knockback=.65f;
            return l;
        }
    }
}
