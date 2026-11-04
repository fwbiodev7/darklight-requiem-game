using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
namespace Darklight.Tests
{
    public sealed partial class DarklightNovemberIsolatedGameplayTests
    {
        HashSet<DarklightActionParticles> essentialPriorParticles;
        HashSet<DarklightCombatImpacts> essentialPriorImpacts;
        WeaponDefinition essentialMotoWeapon;

        void EssentialContext(int weaponIndex,int direction)
        {
            if(weaponIndex!=3){Context(0,weaponIndex,0,direction);return;}
            // The base fixture contains the three conventional profiles. The
            // motorcycle uses its real runtime factory, not array index 3.
            if(essentialMotoWeapon==null)
                essentialMotoWeapon=actor.AddComponent<DarklightAbyssRider>().Initialize(null);
            weapon=essentialMotoWeapon;model=new CombatModel(weapon.loadout);
            model.Tick(.001f,new CombatInput{aim=Facing(direction)});Present();
            Assert.That(DarklightCharacterView.DirectionIndex(model.Facing),Is.EqualTo(direction));
        }

        [SetUp] public void CaptureEssentialEffectOwners()
        {
            essentialPriorParticles=new HashSet<DarklightActionParticles>(Object.FindObjectsByType<DarklightActionParticles>(FindObjectsSortMode.None));
            essentialPriorImpacts=new HashSet<DarklightCombatImpacts>(Object.FindObjectsByType<DarklightCombatImpacts>(FindObjectsSortMode.None));
        }

        [TearDown] public void RestoreEssentialEffectOwners()
        {
            foreach(var fx in Object.FindObjectsByType<DarklightActionParticles>(FindObjectsSortMode.None))
                if(!essentialPriorParticles.Contains(fx))Object.DestroyImmediate(fx.gameObject);
            foreach(var fx in Object.FindObjectsByType<DarklightCombatImpacts>(FindObjectsSortMode.None))
                if(!essentialPriorImpacts.Contains(fx))Object.DestroyImmediate(fx.gameObject);
        }

        DarklightWeaponRedesignPage[] EssentialPages(string action)
        {
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&(p.weapon==0||p.weapon==2||p.weapon==3)&&
                p.action==action&&p.presentation!="Mounted").ToArray();
            Assert.That(pages,Has.Length.EqualTo(24),action);
            foreach(int id in new[]{0,2,3})
                Assert.That(pages.Where(p=>p.weapon==id).Select(p=>p.direction),Is.EquivalentTo(Enumerable.Range(0,8)),action);
            return pages;
        }

        IEnumerator VerifyEssentialState(string action,CombatState state)
        {
            int figures=0;
            foreach(var page in EssentialPages(action))
            {
                EssentialContext(page.weapon,page.direction);
                if(state==CombatState.Parry)Tick(Facing(page.direction),new CombatInput{parryPressed=true});
                else {model.Hurt(state==CombatState.Dead);Present();}
                Assert.That(model.State,Is.EqualTo(state),page.id);
                float deathClock=model.Clock;
                var seen=new HashSet<string>();
                for(int tick=0;tick<200&&model.State==state;tick++)
                {
                    AssertAuthored(page);AssertNativeFigure(page,art.sprite);
                    Assert.That(art.color,Is.EqualTo(Color.white),page.id+": skin must remain untinted");
                    Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f),page.id);
                    seen.Add(art.sprite.name);
                    if(state==CombatState.Dead&&model.StateTime>(page.durationMs.Sum()*.001f+.2f))break;
                    Tick(Facing(page.direction));
                }
                Assert.That(seen,Is.EquivalentTo(page.spriteNames),page.id+": every authored phase must play");
                Assert.That(page.loop,Is.False,page.id);
                if(state==CombatState.Dead)
                {
                    Assert.That(model.Clock,Is.EqualTo(deathClock),page.id+": death must not advance combat cooldown clock");
                    Assert.That(model.State,Is.EqualTo(CombatState.Dead),page.id);
                    Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames.Last()),page.id+": hold final death frame");
                    for(int i=0;i<30;i++)Tick(Facing(page.direction));
                    Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames.Last()),page.id);
                }
                else
                {
                    Assert.That(model.State,Is.EqualTo(CombatState.Neutral),page.id);
                    Assert.That(page.spriteNames,Does.Not.Contain(art.sprite.name),page.id+": leave reaction sprite");
                }
                figures+=seen.Count;AssertSilent();yield return null;
            }
            Report("Essential"+action,24,figures,"real state entry, all authored poses, native rect/pivot, untinted skin, fixed world scale and recovery/hold");
        }

        [UnityTest] public IEnumerator NovemberEssentialParryAllWeaponsEightDirections()=>VerifyEssentialState("Parry",CombatState.Parry);
        [UnityTest] public IEnumerator NovemberEssentialHurtAllWeaponsEightDirections()=>VerifyEssentialState("Hurt",CombatState.Hurt);
        [UnityTest] public IEnumerator NovemberEssentialDeathAllWeaponsEightDirections()=>VerifyEssentialState("Dead",CombatState.Dead);

        [UnityTest] public IEnumerator NovemberMotorcycleGuardSparkDispatchContactOcclusionAndOncePerEntry()
        {
            // Preview rendering intentionally has no VFX side effects. Exercise
            // the real dispatcher separately; mesh rendering needs campaign QA.
            var fx=actor.AddComponent<DarklightActionParticles>();
            typeof(DarklightActionParticles).GetField("instance",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,fx);
            foreach(var page in EssentialPages("Parry").Where(p=>p.weapon==3))
            {
                EssentialContext(3,page.direction);actor.transform.position=new Vector3(4,-3,0);
                DarklightActionParticles.Observe(view,model,art,3);
                int before=fx.MotorcycleGuardBursts,count=fx.Count(DarklightActionParticleKind.Defense);
                Tick(Facing(page.direction),new CombatInput{parryPressed=true});AssertAuthored(page);
                DarklightActionParticles.Observe(view,model,art,3);
                Assert.That(page.guardContact,Has.Length.EqualTo(2));
                bool visible=page.direction!=4; // North contact is behind the body, not a spark on the coat.
                Assert.That(DarklightWeaponRedesignBank.TryMotorcycleGuardContact(0,page.direction,art.sprite,out var local,out bool reportedVisible),Is.True);
                Assert.That(reportedVisible,Is.EqualTo(visible),page.id);
                var expectedLocal=new Vector3((page.guardContact[0]*art.sprite.rect.width-art.sprite.pivot.x)/art.sprite.pixelsPerUnit,
                    (page.guardContact[1]*art.sprite.rect.height-art.sprite.pivot.y)/art.sprite.pixelsPerUnit,0);
                Assert.That(Vector2.Distance(local,expectedLocal),Is.LessThan(.0001f),page.id);
                Assert.That(Vector3.Distance(fx.LastMotorcycleGuardPoint,art.transform.TransformPoint(expectedLocal)),Is.LessThan(.0001f),page.id);
                Assert.That(fx.MotorcycleGuardBursts-before,Is.EqualTo(visible?1:0),page.id);
                Assert.That(fx.Count(DarklightActionParticleKind.Defense)-count,Is.EqualTo(1),page.id);
                for(int i=0;i<10;i++){Present();DarklightActionParticles.Observe(view,model,art,3);Tick(Facing(page.direction));DarklightActionParticles.Observe(view,model,art,3);}
                Assert.That(fx.MotorcycleGuardBursts-before,Is.EqualTo(visible?1:0),page.id+": no per-frame respawn");
                Assert.That(fx.Count(DarklightActionParticleKind.Defense)-count,Is.EqualTo(1),page.id);
                while(model.State==CombatState.Parry){Tick(Facing(page.direction));DarklightActionParticles.Observe(view,model,art,3);}
                Tick(Facing(page.direction),new CombatInput{parryPressed=true});
                DarklightActionParticles.Observe(view,model,art,3);
                Assert.That(fx.MotorcycleGuardBursts-before,Is.EqualTo(visible?2:0),page.id+": next defense emits again");
                AssertSilent();yield return null;
            }
            Report("MotorcycleGuardDispatch",8,8,"contact transform, north occlusion and one event per entry; campaign mesh rendering remains unverified");
        }
    }
}
#endif
