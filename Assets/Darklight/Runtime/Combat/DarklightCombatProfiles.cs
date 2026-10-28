using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    // Session-only copies keep all original authored assets recoverable.
    public sealed class DarklightCombatProfiles
    {
        readonly List<Object> owned=new();
        T Copy<T>(T source)where T:Object{if(source==null)return null;var copy=Object.Instantiate(source);owned.Add(copy);return copy;}
        AttackDefinition Attack(AttackDefinition source,int weapon,int variant)
        {
            var a=Copy(source);if(a==null)return null;
            a.name=source.name+"_Technical_"+weapon+"_"+variant;
            a.windupMove=weapon==1?.72f:.58f;a.recoveryMove=weapon==1?.82f:.66f;
            a.hitstop=a.kind==StrikeKind.Light?.022f:a.kind==StrikeKind.Charged?.055f:.038f;
            a.postureDamage=a.kind==StrikeKind.Light?(weapon==1?.55f:1):a.kind==StrikeKind.Charged?3.8f:2.2f;
            a.whiffCancelFraction=a.kind==StrikeKind.Light?1:1.4f;
            a.hitCancelFraction=1;
            if(weapon==1){a.startup*=.85f;a.recovery*=.8f;a.chargeSeconds=.35f;a.hitShape=HitShape.Thrust;a.width=.9f;a.angle=Mathf.Min(a.angle,100);}
            else if(weapon==2){a.hitShape=HitShape.Sweep;a.angle=Mathf.Max(a.angle,150);a.recovery*=1.08f;a.chargeSeconds=.6f;}
            else {a.hitShape=HitShape.Sector;a.chargeSeconds=.45f;}
            if(a.kind==StrikeKind.Aerial){a.launch=true;a.postureDamage=2.4f;a.cooldown=Mathf.Max(1.2f,a.cooldown);}
            if(a.kind==StrikeKind.Counter){a.finisher=true;a.postureDamage=4;}
            return a;
        }
        public WeaponDefinition[] Create(WeaponDefinition[] originals)
        {
            var result=new WeaponDefinition[originals.Length];
            for(int w=0;w<result.Length;w++)
            {
                if(w==DarklightRuinFistsProfile.WeaponIndex&&DarklightRuinFistsBank.Ready)
                {
                    var fists=result[w]=Copy(originals[w]);fists.displayName=DarklightRuinFistsProfile.DisplayName;
                    fists.loadout=DarklightRuinFistsProfile.Create(owned.Add);
                    fists.requiemPoses=new[]{DarklightRuinFistsBank.Idle(0,0,0)};
                    fists.rockPoses=new[]{DarklightRuinFistsBank.Idle(1,0,0)};
                    fists.icon=fists.requiemPoses[0];continue;
                }
                var weapon=result[w]=Copy(originals[w]);var old=originals[w].loadout;var l=new CombatLoadout{weapon=w,attackBuffer=.18f,defenseBuffer=.18f};
                bool addDaggerFinisher=w==1&&old.lights.Length==3;
                l.lights=new AttackDefinition[old.lights.Length+(addDaggerFinisher?1:0)];
                for(int i=0;i<old.lights.Length;i++)l.lights[i]=Attack(old.lights[i],w,i);
                if(addDaggerFinisher)
                {
                    // The extra input commits two contacts; each pays half of the
                    // preceding light's damage/posture instead of doubling its total.
                    var previous=l.lights[old.lights.Length-1];
                    var finisher=l.lights[old.lights.Length]=Attack(old.lights[old.lights.Length-1],w,old.lights.Length);
                    finisher.displayName="Execucao Dupla";finisher.startup=.05f;finisher.active=.08f;finisher.recovery=.12f;
                    finisher.hitPulses=2;finisher.damage=previous.damage*.5f;finisher.postureDamage=previous.postureDamage*.5f;
                    finisher.finisher=true;finisher.hitstop=.028f;finisher.hitShape=HitShape.Sector;finisher.angle=110;
                }
                l.heavy=Attack(old.heavy,w,10);l.charged=Attack(old.charged,w,11);l.counter=Attack(old.counter,w,12);l.skill=Attack(old.skill,w,13);l.aerial=Attack(old.aerial,w,14);l.weaponSpecial=Attack(old.weaponSpecial,w,15);
                l.pauseBranch=Attack(old.lights[old.lights.Length-1],w,20);l.pauseBranch.displayName=w==0?"Corte do Silencio":w==1?"Passo das Presas":"Orbita do Abismo";
                l.pauseBranch.hitPulses=w==1?3:1;l.pauseBranch.lungeSpeed=w==0?1.2f:0;l.pauseBranch.angle=w==2?280:125;l.pauseBranch.hitShape=w==2?HitShape.Sweep:HitShape.Sector;l.pauseBranch.damage*=1.12f;
                l.heavyBranch=Attack(old.heavy,w,21);l.heavyBranch.displayName=w==0?"Ruptura Precisa":w==1?"Dilaceracao":"Queda da Corrente";
                l.heavyBranch.postureDamage=3;l.heavyBranch.finisher=true;l.heavyBranch.pullTarget=w==2;l.heavyBranch.hitPulses=w==1?3:1;
                l.heavyBranch.damage*=w==1?.52f:1.15f;weapon.loadout=l;
                if(w==0)
                {
                    var finisher=Attack(old.lights[old.lights.Length-1],w,30);finisher.displayName="Corte do Crepúsculo";
                    finisher.damage*=.6f;finisher.hitPulses=2;finisher.startup=.12f;finisher.active=.16f;finisher.recovery=.25f;
                    finisher.finisher=true;finisher.hitShape=HitShape.Sweep;finisher.angle=155;finisher.postureDamage=1.8f;
                    var extended=new AttackDefinition[l.lights.Length+1];System.Array.Copy(l.lights,extended,l.lights.Length);extended[extended.Length-1]=finisher;l.lights=extended;
                    var special=l.weaponSpecial=Attack(old.heavy,w,31);special.kind=StrikeKind.WeaponSpecial;special.displayName="Eclipse Partido";
                    special.damage=17;special.hitPulses=2;special.startup=.16f;special.active=.22f;special.recovery=.30f;special.range=2.15f;
                    special.hitShape=HitShape.Sweep;special.angle=190;special.cooldown=6.5f;special.lungeSpeed=.5f;special.finisher=true;special.pullTarget=special.blink=false;
                }
                else if(w==1&&l.weaponSpecial!=null)
                {
                    var special=l.weaponSpecial;special.displayName="Dança das Presas";special.damage=7;special.hitPulses=5;
                    special.startup=.055f;special.active=.32f;special.recovery=.18f;special.cooldown=8.5f;special.range=1.65f;
                    special.hitShape=HitShape.Sector;special.angle=250;special.finisher=true;
                }
            }
            return result;
        }
        public void Dispose(){foreach(var item in owned)if(item!=null){if(Application.isPlaying)Object.Destroy(item);else Object.DestroyImmediate(item);}owned.Clear();}
    }
}
