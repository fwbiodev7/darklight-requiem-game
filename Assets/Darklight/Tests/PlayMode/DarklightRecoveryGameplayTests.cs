using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
namespace Darklight.Tests
{
    // Staged outside Assets while the coordinator owns the only Unity process.
    // Uses the existing muted, isolated CharacterView/CombatModel fixture.
    public sealed partial class DarklightNovemberIsolatedGameplayTests
    {
        DarklightWeaponRedesignPage RecoveryAttackPage(int weaponIndex,int direction,string key)
        {
            return catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==weaponIndex&&
                p.direction==direction&&p.action=="Attack"&&p.attackKey==key&&p.presentation!="Mounted");
        }

        void AssertRecoveryFigure(DarklightWeaponRedesignPage page)
        {
            AssertAuthored(page);AssertNativeFigure(page,art.sprite);
            int index=Array.IndexOf(page.spriteNames,art.sprite.name);
            float density=page.framePixelsPerUnit?.Length==page.spriteNames.Length?
                page.framePixelsPerUnit[index]:page.pixelsPerUnit;
            Assert.That(art.sprite.pixelsPerUnit,Is.EqualTo(density).Within(.001f),page.id);
            Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f),page.id);
            Assert.That(art.color,Is.EqualTo(Color.white),page.id);
        }

        void AssertRecoveryWindup(DarklightWeaponRedesignPage page,int serial)
        {
            Assert.That(model.State,Is.EqualTo(CombatState.Charging),page.id);
            Assert.That(model.ActiveThisTick,Is.False,page.id+": holding cannot cause contact");
            Assert.That(model.ContactPulses,Is.Zero,page.id);
            Assert.That(model.AttackSerial,Is.EqualTo(serial),page.id+": holding cannot begin an attack");
            AssertRecoveryFigure(page);
            int index=Array.IndexOf(page.spriteNames,art.sprite.name);
            Assert.That(index,Is.InRange(0,page.startupFrames-1),page.id+": charging must exclude contact/recovery poses");
        }

        [UnityTest] public IEnumerator NovemberSerraEntryEightDirectionsKeepsPhasesAndNativeFigures()
        {
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&p.weapon==3&&p.action=="Attack"&&
                p.attackKey=="Serra de Entrada"&&p.presentation!="Mounted").OrderBy(p=>p.direction).ToArray();
            Assert.That(pages,Has.Length.EqualTo(8));
            Assert.That(pages.Select(p=>p.direction),Is.EquivalentTo(Enumerable.Range(0,8)));
            int figures=0;
            foreach(var page in pages)
            {
                EssentialContext(3,page.direction);var facing=Facing(page.direction);
                // EssentialContext constructs the genuine Initialize(null) Moto
                // loadout. This test never substitutes or edits AttackDefinition.
                var attack=weapon.loadout.lights[0];
                string original=JsonUtility.ToJson(attack);
                Assert.That(attack.displayName,Is.EqualTo("Serra de Entrada"));
                Assert.That(page.spriteNames,Has.Length.EqualTo(4));
                Assert.That(page.durationMs,Is.EqualTo(new[]{125,125,240,370}));
                Assert.That(page.startupFrames,Is.EqualTo(2));Assert.That(page.activeFrames,Is.EqualTo(1));
                Assert.That(attack.startup,Is.EqualTo(.25f).Within(.00001f));
                Assert.That(attack.active,Is.EqualTo(.24f).Within(.00001f));
                Assert.That(attack.recovery,Is.EqualTo(.37f).Within(.00001f));
                Tick(facing,new CombatInput{lightPressed=true,lightHeld=true});
                Assert.That(model.State,Is.EqualTo(CombatState.Attack),page.id);
                Assert.That(model.Attack,Is.SameAs(attack),page.id);
                int serial=model.AttackSerial,ticks=0;bool contact=false;
                var seen=new HashSet<string>();
                while(model.State==CombatState.Attack&&ticks++<150)
                {
                    Present();AssertRecoveryFigure(page);seen.Add(art.sprite.name);
                    float t=model.StateTime;
                    int expected=t<attack.startup?Mathf.Min(1,Mathf.FloorToInt(t/attack.startup*2)):
                        t<attack.startup+attack.active?2:3;
                    Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames[expected]),page.id+": phase at "+t);
                    Assert.That(model.Attack,Is.SameAs(attack));Assert.That(model.AttackSerial,Is.EqualTo(serial));
                    Tick(facing);
                    bool crossing=t<attack.startup+attack.active&&model.StateTime>=attack.startup;
                    Assert.That(model.ActiveThisTick,Is.EqualTo(crossing),page.id+": real contact interval at "+t);
                    contact|=model.ActiveThisTick;
                }
                Assert.That(model.State,Is.EqualTo(CombatState.Neutral),page.id);
                Assert.That(contact,Is.True,page.id);Assert.That(seen,Is.EquivalentTo(page.spriteNames),page.id);
                Assert.That(JsonUtility.ToJson(attack),Is.EqualTo(original),page.id+": runtime attack remains unchanged");
                Tick(facing);
                var idle=DarklightMotorcycleRedesignBank.DualSawsPose(page.direction,model);
                Assert.That(idle,Is.Not.Null);Assert.That(art.sprite,Is.SameAs(idle));
                Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(art.sprite),Is.True);
                figures+=seen.Count;AssertSilent();yield return null;
            }
            Report("SerraEntryRecovery",8,figures,"real Moto light input, four phases, contact interval, native rect/pivot/density and unchanged attack; no campaign or graphical certification");
        }

        [UnityTest] public IEnumerator NovemberChargingAllWeaponsUsesWindupReleaseAndDodge()
        {
            int contexts=0,windupFigures=0;
            foreach(int weaponIndex in new[]{0,2,3})for(int direction=0;direction<8;direction++)
            {
                string key=weaponIndex==0?"Carregado":weaponIndex==2?"Juízo da Ruína":"Serra de Entrada";
                var page=RecoveryAttackPage(weaponIndex,direction,key);var facing=Facing(direction);
                Assert.That(page.startupFrames,Is.GreaterThan(0));
                Assert.That(page.startupFrames,Is.LessThan(page.spriteNames.Length));
                // Independent real models prevent earlier attacks, buffers or
                // dodge cooldowns from manufacturing the next scenario.
                for(int scenario=0;scenario<3;scenario++)
                {
                    EssentialContext(weaponIndex,direction);
                    var heavy=weapon.loadout.heavy;var charged=weapon.loadout.charged;
                    Assert.That(heavy,Is.Not.Null);Assert.That(charged,Is.Not.Null);
                    string heavyOriginal=JsonUtility.ToJson(heavy),chargedOriginal=JsonUtility.ToJson(charged);
                    Assert.That(charged.chargeSeconds,Is.GreaterThan(.08f).And.LessThan(.8f),page.id);
                    int serial=model.AttackSerial;
                    Tick(facing,new CombatInput{heavyPressed=true,heavyHeld=true});
                    AssertRecoveryWindup(page,serial);
                    var seen=new HashSet<string>{art.sprite.name};
                    int holdTicks=scenario==0?8:80;
                    for(int tick=0;tick<holdTicks;tick++)
                    {
                        Tick(facing,new CombatInput{heavyHeld=true});
                        AssertRecoveryWindup(page,serial);seen.Add(art.sprite.name);
                        if(model.StateTime>=page.durationMs.Take(page.startupFrames).Sum()*.001f)
                            Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames[page.startupFrames-1]),page.id+": hold final anticipation without looping");
                    }
                    if(scenario!=0)
                    {
                        Assert.That(model.StateTime,Is.GreaterThanOrEqualTo(.79f),page.id+": sustained real charge");
                        Assert.That(seen,Is.EquivalentTo(page.spriteNames.Take(page.startupFrames)),page.id+": every native anticipation plays");
                    }
                    if(scenario<2)
                    {
                        Tick(facing,new CombatInput{heavyReleased=true});
                        Assert.That(model.State,Is.EqualTo(CombatState.Attack),page.id);
                        Assert.That(model.Attack,Is.SameAs(scenario==0?heavy:charged),page.id+": short/long release selects real loadout entry");
                        Assert.That(model.AttackSerial,Is.EqualTo(serial+1),page.id);
                        Assert.That(model.ActiveThisTick,Is.False,page.id+": release starts anticipation, not contact");
                    }
                    else
                    {
                        Tick(facing,new CombatInput{heavyHeld=true,dodgePressed=true,move=facing});
                        Assert.That(model.State,Is.EqualTo(CombatState.Dodge),page.id+": dodge cancels held charge");
                        Assert.That(model.AttackSerial,Is.EqualTo(serial),page.id+": cancellation never commits charged attack");
                        Assert.That(model.ActiveThisTick,Is.False,page.id);Assert.That(model.ContactPulses,Is.Zero,page.id);
                        Tick(facing,new CombatInput{heavyReleased=true});
                        Assert.That(model.State,Is.EqualTo(CombatState.Dodge),page.id+": release cannot resurrect cancelled charge");
                        for(int tick=0;tick<30;tick++)Tick(facing);
                        Assert.That(model.State,Is.EqualTo(CombatState.Neutral),page.id);
                        Assert.That(model.AttackSerial,Is.EqualTo(serial),page.id);
                    }
                    Assert.That(JsonUtility.ToJson(heavy),Is.EqualTo(heavyOriginal),page.id);
                    Assert.That(JsonUtility.ToJson(charged),Is.EqualTo(chargedOriginal),page.id);
                }
                contexts++;windupFigures+=page.startupFrames;AssertSilent();yield return null;
            }
            Report("ChargingRecovery",contexts,windupFigures,"24 weapon/direction contexts; short and .8s held charge, native anticipation only, final-pose hold, real release selection and dodge cancellation; isolated muted runtime only");
        }
        [UnityTest] public IEnumerator NovemberActionParticlesTrackGroundDepthAndPreserveCharacterMaterial()
        {
            var fx=actor.AddComponent<DarklightActionParticles>();
            typeof(DarklightActionParticles).GetField("instance",System.Reflection.BindingFlags.Static|
                System.Reflection.BindingFlags.NonPublic).SetValue(null,fx);
            foreach(float groundY in new[]{8f,-3f})
            {
                var point=new Vector3(1,groundY+.85f,0);
                DarklightActionParticles.Emit(DarklightActionParticleKind.Defense,point,Vector2.right,groundY);
                var burst=actor.GetComponentsInChildren<SpriteRenderer>()
                    .Single(r=>r.gameObject.name=="ParticulaAcao"&&r.transform.position==point);
                Assert.That(burst.sortingOrder,Is.EqualTo(DarklightSorting.OrderForY(groundY)+1));
                var properties=new MaterialPropertyBlock();burst.GetPropertyBlock(properties);
                Assert.That(properties.GetFloat("_AlphaClip"),Is.EqualTo(.05f).Within(.0001f));
                Assert.That(properties.GetFloat("_WholeSprite"),Is.EqualTo(1));
                Assert.That(burst.sprite,Is.Not.Null);Assert.That(burst.enabled,Is.True);
                Assert.That(fx.ActiveCount,Is.LessThanOrEqualTo(fx.Capacity));
            }
            Assert.That(fx.Count(DarklightActionParticleKind.Defense),Is.EqualTo(2));
            AssertSilent();yield return null;
            Report("ParticleDepth",2,2,"ground depth independent of effect height and local alpha property; no campaign composition certification");
        }
    }
}
#endif
