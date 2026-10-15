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
                var weapon=result[w]=Copy(originals[w]);var old=originals[w].loadout;var l=new CombatLoadout{weapon=w,attackBuffer=.18f,defenseBuffer=.18f};
                l.lights=new AttackDefinition[old.lights.Length];for(int i=0;i<l.lights.Length;i++)l.lights[i]=Attack(old.lights[i],w,i);
                l.heavy=Attack(old.heavy,w,10);l.charged=Attack(old.charged,w,11);l.counter=Attack(old.counter,w,12);l.skill=Attack(old.skill,w,13);l.aerial=Attack(old.aerial,w,14);l.weaponSpecial=Attack(old.weaponSpecial,w,15);
                l.pauseBranch=Attack(old.lights[old.lights.Length-1],w,20);l.pauseBranch.displayName=w==0?"Corte do Silencio":w==1?"Passo das Presas":"Orbita do Abismo";
                l.pauseBranch.hitPulses=w==1?3:1;l.pauseBranch.lungeSpeed=w==0?1.2f:0;l.pauseBranch.angle=w==2?280:125;l.pauseBranch.hitShape=w==2?HitShape.Sweep:HitShape.Sector;l.pauseBranch.damage*=1.12f;
                l.heavyBranch=Attack(old.heavy,w,21);l.heavyBranch.displayName=w==0?"Ruptura Precisa":w==1?"Dilaceracao":"Queda da Corrente";
                l.heavyBranch.postureDamage=3;l.heavyBranch.finisher=true;l.heavyBranch.pullTarget=w==2;l.heavyBranch.hitPulses=w==1?3:1;
                l.heavyBranch.damage*=w==1?.52f:1.15f;weapon.loadout=l;
            }
            return result;
        }
        public void Dispose(){foreach(var item in owned)if(item!=null){if(Application.isPlaying)Object.Destroy(item);else Object.DestroyImmediate(item);}owned.Clear();}
    }
}
