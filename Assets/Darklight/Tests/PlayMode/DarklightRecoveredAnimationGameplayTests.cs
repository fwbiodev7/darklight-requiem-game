using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        [UnityTest] public IEnumerator RecoveredMovesVisitEveryAuthoredPhaseAndPreserveContactTiming()
        {
            int contexts=0,figures=0;
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&p.action=="Attack"&&
                (p.attackKey=="Giro Devastador"||p.attackKey=="Leve3"||p.attackKey=="Dentes Cruzados")).ToArray();
            foreach(var move in new[]{(weapon:0,key:"Leve3"),(weapon:2,key:"Giro Devastador"),(weapon:3,key:"Dentes Cruzados")})
                Assert.That(pages.Where(p=>p.weapon==move.weapon&&p.attackKey==move.key).Select(p=>p.direction),
                    Is.EquivalentTo(Enumerable.Range(0,8)),move.key+": require an exact authored clip in every direction, without fallback");
            foreach(var page in pages)
            {
                Context(0,page.weapon,0,page.direction);
                var attack=Attacks(fighter.loadout).Single(a=>a.displayName==page.attackKey);
                float startup=attack.startup,active=attack.active,recovery=attack.recovery;
                Assert.That(fighter.Model.TryAirFollowup(attack,new CombatInput{aim=Facing(page.direction)}),Is.True);
                var seen=new HashSet<string>();bool contacted=false;
                int contactIndex=page.startupFrames>0?page.startupFrames:page.spriteNames.Length-2;
                while(fighter.Model.State==CombatState.Attack)
                {
                    Present();AssertAuthored(page);
                    if(seen.Add(art.sprite.name))Capture("recovered-"+page.id+"-"+seen.Count);
                    float t=fighter.Model.StateTime;
                    if(t<startup)Assert.That(art.sprite.name,Is.Not.EqualTo(page.spriteNames[contactIndex]));
                    else if(t<startup+active)
                    {
                        Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames[contactIndex]));contacted=true;
                    }
                    Tick(Facing(page.direction));
                }
                Assert.That(contacted,Is.True,page.id);
                Assert.That(seen.Count,Is.EqualTo(page.spriteNames.Length),page.id+": every authored phase must reach the renderer");
                Assert.That(attack.startup,Is.EqualTo(startup));Assert.That(attack.active,Is.EqualTo(active));Assert.That(attack.recovery,Is.EqualTo(recovery));
                Present();Assert.That(art.sprite,Is.Not.Null);contexts++;figures+=seen.Count;
                yield return null;
            }
            Assert.That(contexts,Is.EqualTo(24));
            Report("recovered-20261101",contexts,figures,"Leve3, Giro Devastador and Dentes Cruzados each rendered in eight exact authored directions from real loadouts; every startup/contact/recovery figure observed; gameplay timings unchanged. Other animations/forms remain pending.");
        }

        [UnityTest] public IEnumerator RecoveredMotorcycleTransitionUsesWholeStartupAndRecoveryFigures()
        {
            Context(0,3,0,0);
            var attack=fighter.loadout.weaponSpecial;
            Assert.That(fighter.Model.TryAirFollowup(attack,new CombatInput{aim=Vector2.down}),Is.True);
            var seen=new HashSet<Sprite>();bool contact=false;
            while(fighter.Model.State==CombatState.Attack)
            {
                Present();float t=fighter.Model.StateTime;
                if(t<attack.startup||t>=attack.startup+attack.active)
                {
                    var expected=DarklightMotorcycleRedesignBank.TransitionPose(0,t<attack.startup,
                        t<attack.startup?t/attack.startup:(t-attack.startup-attack.active)/attack.recovery);
                    Assert.That(expected,Is.Not.Null);
                    Assert.That(art.sprite,Is.SameAs(expected));Assert.That(DarklightMotorcycleRedesignBank.IsTransition(art.sprite),Is.True);
                    Assert.That(art.flipX,Is.False);if(seen.Add(art.sprite))Capture("motorcycle-transition-"+seen.Count);
                }
                else
                {
                    Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(art.sprite),Is.True);
                    Assert.That(DarklightMotorcycleRedesignBank.IsTransition(art.sprite),Is.False);contact=true;
                }
                Tick(Vector2.down);
            }
            Assert.That(contact,Is.True);Assert.That(seen.Count,Is.EqualTo(12));
            Present();Assert.That(game.AbyssRider.MountedPresentation,Is.False);
            Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.True);
            Assert.That(game.AbyssRider.BlocksSwap,Is.False);
            Report("motorcycle-transition-20261031",1,seen.Count,"Whole-figure join/separate playback within existing dash startup/recovery; mounted contact and safe return to on-foot saws observed. South only.");
            yield return null;
        }
    }
}
