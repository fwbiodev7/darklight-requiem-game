using System;
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
        [UnityTest] public IEnumerator ActionParticlesEmitOnceForRealStatesAndRespectLimits()
        {
            Context(0,0,0,0);
            var fx=DarklightActionParticles.Current;Assert.That(fx,Is.Not.Null,"Campaign Present must initialize the shared particle pool");
            Call(fx,"Advance",2f);
            Assert.That(fx.ActiveCount,Is.Zero);
            Assert.That(Resources.LoadAll<Sprite>(DarklightActionParticles.Resource),Has.Length.EqualTo(12),"All four authored three-frame action effects must load");
            int originalSkin=game.SelectedSkin;bool reduced=game.ReducedFlashes;
            try
            {
                var cases=new (DarklightActionParticleKind kind,Action enter)[]
                {
                    (DarklightActionParticleKind.Dodge,()=>{fighter.ResetModel();Tick(Vector2.down,new CombatInput{dodgePressed=true,move=Vector2.down});}),
                    (DarklightActionParticleKind.Defense,()=>{fighter.ResetModel();Tick(Vector2.down,new CombatInput{parryPressed=true});}),
                    (DarklightActionParticleKind.Damage,()=>{fighter.ResetModel();fighter.Model.Hurt();Present();}),
                    (DarklightActionParticleKind.Death,()=>{fighter.ResetModel();fighter.Model.Hurt(true);Present();})
                };
                foreach(var item in cases)
                {
                    int before=fx.Count(item.kind);item.enter();
                    Assert.That(DarklightActionParticles.Current.Count(item.kind),Is.EqualTo(before+1),item.kind+" must emit exactly once on the real state entry");
                    Assert.That(DarklightActionParticles.Current.LastPresentedSprite,Is.Not.Null);
                    var available=Resources.LoadAll<Sprite>(DarklightActionParticles.Resource).Concat(Resources.LoadAll<Sprite>(DarklightActionParticles.WeaponResource));
                    Assert.That(available,Does.Contain(DarklightActionParticles.Current.LastPresentedSprite));
                    Assert.That(DarklightWeaponRedesignBank.IsAuthored(art.sprite),Is.True,item.kind+" must leave a complete authored character sprite visible");
                    Assert.That(art.color,Is.EqualTo(Color.white),"Action particles must not tint the authored player figure");
                    Assert.That(game.SelectedSkin,Is.EqualTo(originalSkin),"Action effects must not change the selected skin");
                    for(int repeat=0;repeat<4;repeat++)Present();
                    Assert.That(DarklightActionParticles.Current.Count(item.kind),Is.EqualTo(before+1),item.kind+" repeat Present calls cannot duplicate the event");
                    yield return null;
                }

                game.ReducedFlashes=true;
                DarklightActionParticles.Emit(DarklightActionParticleKind.Damage,Vector3.zero,Vector2.down);
                Assert.That(fx.LastOpacity,Is.LessThanOrEqualTo(.4f));
                Assert.That(fx.LastDiameter,Is.LessThanOrEqualTo(.9f));

                Assert.That(fx.Capacity,Is.EqualTo(16));
                for(int i=0;i<fx.Capacity+12;i++)
                    DarklightActionParticles.Emit(DarklightActionParticleKind.Dodge,Vector3.zero,Vector2.down);
                Assert.That(fx.ActiveCount,Is.LessThanOrEqualTo(fx.Capacity),"Manual emissions must respect the fixed pool bound");

                Call(fx,"Advance",2f);
                Assert.That(fx.ActiveCount,Is.Zero,"Every emitted action clip is shorter than the cleanup advance");
                int deathCount=fx.Count(DarklightActionParticleKind.Death);
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Dead));
                for(int repeat=0;repeat<4;repeat++)Present();
                Assert.That(fx.Count(DarklightActionParticleKind.Death),Is.EqualTo(deathCount),"A held Dead state cannot respawn the death effect after it expires");
                Assert.That(fx.ActiveCount,Is.Zero);
                AssertSilent();
            }
            finally{game.ReducedFlashes=reduced;game.SelectSkin(originalSkin,false);}
            yield return null;
        }

        [UnityTest] public IEnumerator CatalogDodgeClipsRunThroughRealCombatClockAndReturnToIdle()
        {
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&DarklightPlayableScope.WeaponActive(p.weapon)&&p.action=="Dodge").OrderBy(p=>p.weapon).ThenBy(p=>p.direction).ToArray();
            Assert.That(pages.Length,Is.EqualTo(24),"All three active weapons need eight authored Dodge directions");
            foreach(int weapon in new[]{0,2,3})
                Assert.That(pages.Where(p=>p.weapon==weapon).Select(p=>p.direction),Is.EquivalentTo(Enumerable.Range(0,8)),
                    "Every Dodge direction must resolve its own complete clip, without a fallback");
            var observedByWeapon=new Dictionary<int,int>();int contexts=0,figures=0;
            foreach(var page in pages)
            {
                Assert.That(page.spriteNames,Has.Length.EqualTo(3),page.id);
                Assert.That(page.durationMs,Is.EqualTo(new[]{40,150,40}),page.id+" must retain authored phase timing");
                Assert.That(page.durationMs.Sum(),Is.EqualTo(230));
                Context(0,page.weapon,0,page.direction);var facing=Facing(page.direction);fighter.ResetModel();fighter.Model.FormStyle=0;
                int before=DarklightActionParticles.Current.Count(DarklightActionParticleKind.Dodge);
                Tick(facing,new CombatInput{dodgePressed=true,move=facing});
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Dodge));
                Assert.That(DarklightActionParticles.Current.Count(DarklightActionParticleKind.Dodge),Is.EqualTo(before+1),page.id+" actual dodge input must notify action particles once");
                var seen=new HashSet<string>(StringComparer.Ordinal);bool sawProtectedStart=false,sawProtectedEnd=false;
                for(int tick=0;tick<28&&fighter.Model.State==CombatState.Dodge;tick++)
                {
                    if(fighter.Model.StateTime>=.02f&&fighter.Model.StateTime<=.18f)
                    {Assert.That(fighter.Model.Invulnerable,Is.True,page.id+" should preserve the normal 20–190ms invulnerability window");sawProtectedStart=true;}
                    if(fighter.Model.StateTime>=.20f)
                    {Assert.That(fighter.Model.Invulnerable,Is.False,page.id+" must end invulnerability by 200ms");sawProtectedEnd=true;}
                    if(page.spriteNames.Contains(art.sprite.name))seen.Add(art.sprite.name);
                    Tick(facing);
                }
                Assert.That(sawProtectedStart,Is.True,page.id);Assert.That(sawProtectedEnd,Is.True,page.id);
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral),page.id+" should return to idle after the 230ms clip");
                Assert.That(seen,Is.EquivalentTo(page.spriteNames),page.id+" every authored Dodge pose must be observed in the real CharacterView");
                Present();
                if(page.weapon==3){AssertCurrentFigure(page.id+" return to on-foot saws");Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.True);}
                else
                {
                    var idle=catalog.pages.Single(p=>p.skin==0&&p.weapon==page.weapon&&p.form==0&&p.direction==page.direction&&p.action=="Idle");
                    AssertAuthored(idle);
                }
                Assert.That(DarklightActionParticles.Current.LastPresentedSprite,Is.Not.Null);
                observedByWeapon[page.weapon]=observedByWeapon.GetValueOrDefault(page.weapon)+1;contexts++;figures+=seen.Count;
                AssertSilent();yield return null;
            }
            string coverage=string.Join(", ",observedByWeapon.OrderBy(p=>p.Key).Select(p=>$"weapon {p.Key}: {p.Value}/8 directions"));
            Report("dodge-action-pages",contexts,figures,"Real model dodge input rendered all 72 authored poses in 24 exact clips; one effect per state entry, normal invulnerability 20–190ms, then Idle. Coverage ("+coverage+"). This does not certify other animation actions.");
        }

        [UnityTest] public IEnumerator ReviewedFistsSouthWalkLoopsAllFramesAndSettlesToIdle()
        {
            var page=catalog.pages.SingleOrDefault(p=>p.skin==0&&p.weapon==2&&p.form==0&&p.direction==0&&p.action=="Walk");
            Assert.That(page,Is.Not.Null,"The reviewed four-frame Normal Punhos South Walk page must be integrated for this focused check");
            Assert.That(page.spriteNames,Has.Length.EqualTo(4));Assert.That(page.durationMs.Sum(),Is.EqualTo(400));
            Context(0,2,0,0);var facing=Vector2.down;var seen=new HashSet<string>(StringComparer.Ordinal);
            for(int tick=0;tick<100&&seen.Count<page.spriteNames.Length;tick++)
            {
                Tick(facing,new CombatInput{move=facing});
                if(view.UsesWeaponRedesignSprite&&page.spriteNames.Contains(art.sprite.name)){AssertAuthored(page);seen.Add(art.sprite.name);}
            }
            Assert.That(seen,Is.EquivalentTo(page.spriteNames),"All four selected complete South gait figures must reach the renderer");
            var idle=catalog.pages.Single(p=>p.skin==0&&p.weapon==2&&p.form==0&&p.direction==0&&p.action=="Idle");
            for(int tick=0;tick<60;tick++){Tick(facing);if(idle.spriteNames.Contains(art.sprite.name))break;}
            AssertAuthored(idle);Assert.That(idle.spriteNames,Does.Contain(art.sprite.name));AssertSilent();
            Report("fists-south-walk",1,seen.Count,"The reviewed Normal Punhos South four-frame Walk loop rendered through real Context(weapon 2), then settled into its authored Idle page. No other direction is implied.");
            yield return null;
        }
    }
}
