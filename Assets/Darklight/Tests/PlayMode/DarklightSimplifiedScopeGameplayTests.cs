using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightSimplifiedScopeGameplayTests
    {
        IDisposable storage;DarklightCampaign game;DarklightCombatLab lab;bool oldPause,oldSilent;float oldVolume;
        static void Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        [UnitySetUp] public IEnumerator Setup()
        {
            oldPause=AudioListener.pause;oldVolume=AudioListener.volume;oldSilent=DarklightAutomation.Silent;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            Directory.CreateDirectory("work/retomada-20261026/gameplay");
            storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/retomada-20261026/gameplay/save.json"));
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.EnterTraining();yield return null;yield return null;
            lab.SetPaused(true);lab.Player.enabled=false;lab.Player.ResetModel();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {storage?.Dispose();AudioListener.pause=oldPause;AudioListener.volume=oldVolume;DarklightAutomation.Silent=oldSilent;yield return null;}
        void ForceRetainedMotorForSystemRegression()
        {
            // The player selector must never grant weapon 3. Inject the retained
            // implementation only so these legacy mechanics tests remain meaningful.
            var legacy=game.weaponDefinitions[3];
            typeof(DarklightWeaponController).GetProperty("Equipped").SetValue(game.Weapons,legacy);
            lab.Player.loadout=legacy.loadout;lab.Player.ResetModel();
        }
        [UnityTest] public IEnumerator ActiveSelectorsKeepOnlyKatanaAndRuinFistsAndMigrateLegacyBike()
        {
            game.SelectSkin(1,false);Assert.That(game.SelectedSkin,Is.Zero);
            Assert.That(game.Weapons.Unlocked(0),Is.True);Assert.That(game.Weapons.Unlocked(1),Is.False);
            Assert.That(game.Weapons.Unlocked(2),Is.True);Assert.That(game.Weapons.Unlocked(3),Is.False);
            for(int chapter=0;chapter<DarklightProgress.ChapterCapacity;chapter++)
            {
                game.Progress.chapter=chapter;game.Progress.checkpoint=0;game.Progress.weaponUnlocks=1;game.Progress.equippedWeapon=3;
                game.Weapons.Restore();
                Assert.That(game.Progress.equippedWeapon,Is.Zero,"Restore must migrate the old bike at chapter "+chapter);
                Assert.That(game.Progress.legacyEquippedWeapon,Is.EqualTo(3));Assert.That(game.Weapons.Unlocked(3),Is.False);
                Assert.That(game.Progress.weaponUnlocks&8,Is.Zero,"Restore must not add the retired unlock bit at chapter "+chapter);
            }
            game.Progress.weaponUnlocks=15;game.Progress.equippedWeapon=3;game.Weapons.Restore();
            Assert.That(game.Progress.equippedWeapon,Is.Zero);Assert.That(game.Progress.legacyEquippedWeapon,Is.EqualTo(3));
            Assert.That(game.Progress.weaponUnlocks,Is.EqualTo(15),"Disabled legacy IDs remain in save history");
            game.Weapons.RequestNext();game.Weapons.BeforeTick(.2f,true);
            Assert.That(game.Weapons.Equipped.id,Is.EqualTo(DarklightWeaponId.Chain));
            game.Weapons.RequestNext();game.Weapons.BeforeTick(.2f,true);
            Assert.That(game.Weapons.Equipped.id,Is.EqualTo(DarklightWeaponId.Katana));
            game.Weapons.Request(3);game.Weapons.BeforeTick(.2f,true);
            Assert.That(game.Weapons.Equipped.id,Is.EqualTo(DarklightWeaponId.Katana));Assert.That(game.Progress.equippedWeapon,Is.Zero);
            Assert.That(game.AbyssRider.Active,Is.False);
            Call(game,"SetTrainingForm",3);Assert.That(game.Form,Is.EqualTo(2));
            Call(game,"TickFormState",2f);Assert.That(game.PresentedForm,Is.EqualTo(2));
            Assert.That(game.IncomingMultiplier,Is.LessThan(1));Assert.That(game.StrikeRangeBonus,Is.GreaterThan(0));
            yield return null;
        }
        [UnityTest] public IEnumerator RetainedMotorcycleBankProvidesEightLegacyViewsWithoutBorrowingRock()
        {
            ForceRetainedMotorForSystemRegression();Call(game,"SetTrainingForm",0);
            var fighter=lab.Player;var view=fighter.GetComponent<DarklightCharacterView>();var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var seen=new System.Collections.Generic.HashSet<Sprite>();DarklightMotorcycleRedesignBank.ClearCache();
            for(int d=0;d<8;d++)
            {
                float angle=d*Mathf.PI/4;var facing=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                fighter.Model.Tick(.01f,new CombatInput{aim=facing});view.Present(fighter.Model,Vector2.zero);
                var expected=DarklightMotorcycleRedesignBank.DualSawsPose(d,fighter.Model);
                Assert.That(expected,Is.Not.Null,"Missing on-foot Réquiem dual-saw view "+d);
                Assert.That(art.sprite,Is.SameAs(expected));Assert.That(seen.Add(expected),Is.True,"Real eight-view coverage");
                Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.True);
                Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
            }
            yield return null;
        }
        [UnityTest] public IEnumerator RetainedMountedIdleAnimatesEightLegacyDirectionsAndStopsWhenMoving()
        {
            ForceRetainedMotorForSystemRegression();Call(game,"SetTrainingForm",0);
            var fighter=lab.Player;var view=fighter.GetComponent<DarklightCharacterView>();
            var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            DarklightMotorcycleRedesignBank.ClearCache();
            for(int d=0;d<8;d++)
            {
                float angle=d*Mathf.PI/4;var facing=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                var seen=new System.Collections.Generic.HashSet<Sprite>();
                foreach(float dt in new[]{.001f,.36f,.61f,.80f})
                {
                    var expected=DarklightMotorcycleRedesignBank.Idle(0,d,dt);
                    Assert.That(expected,Is.Not.Null,"Missing mounted idle direction "+d);
                    Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(expected),Is.True);
                    Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(expected),Is.False);
                    seen.Add(expected);
                }
                Assert.That(seen.Count,Is.EqualTo(4),"Mounted source has four distinct idle figures per direction");
                fighter.ResetModel();fighter.Model.Tick(.01f,new CombatInput{aim=facing});view.Present(fighter.Model,Vector2.zero);
                Assert.That(art.sprite,Is.Not.Null);
            }
            Assert.That(DarklightMotorcycleRedesignBank.Idle(1,0,.1f),Is.Null,"Normal idle is not transformed art");
            yield return null;
        }
        [UnityTest] public IEnumerator RetainedRiderSpecialUsesLegacyRuntimeAndReturnsToDualSaws()
        {
            ForceRetainedMotorForSystemRegression();Call(game,"SetTrainingForm",0);
            var fighter=lab.Player;var view=fighter.GetComponent<DarklightCharacterView>();
            var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            DarklightMotorcycleRedesignBank.ClearCache();fighter.ResetModel();
            var dash=game.Weapons.Equipped.loadout.weaponSpecial;
            Assert.That(dash,Is.Not.Null,"The real Requiem Moto loadout supplies its weapon special");
            Assert.That(fighter.Model.TryAirFollowup(dash,new CombatInput{aim=Vector2.down}),Is.True,"Start the actual dash attack through the combat model");
            Assert.That(game.AbyssRider.MountedPresentation,Is.True,"Vehicle technique owns the mounted presentation");
            view.Present(fighter.Model,Vector2.zero);
            var assembly=DarklightMotorcycleRedesignBank.TransitionPose(0,true,0);
            Assert.That(assembly,Is.Not.Null,"The reviewed assembly now owns the dash preparation");
            Assert.That(art.sprite,Is.SameAs(assembly));
            fighter.Model.Tick(dash.startup+.01f,default);
            view.Present(fighter.Model,Vector2.zero);
            var mounted=DarklightMotorcycleRedesignBank.Riding(0,0,0);
            Assert.That(mounted,Is.Not.Null);Assert.That(art.sprite,Is.SameAs(mounted));
            Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(art.sprite),Is.True);
            Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.False);

            fighter.ResetModel();
            Assert.That(game.AbyssRider.MountedPresentation,Is.False,"Neutral state releases the vehicle presentation");
            view.Present(fighter.Model,Vector2.zero);
            Assert.That(art.sprite,Is.SameAs(DarklightMotorcycleRedesignBank.DualSawsPose(0,fighter.Model)));
            Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.True);
            yield return null;
        }
    }
}
