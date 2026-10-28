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
        [UnityTest] public IEnumerator ActiveSelectorsMigrateRockAndSkipRemovedWeapon()
        {
            game.SelectSkin(1,false);Assert.That(game.SelectedSkin,Is.Zero);
            Assert.That(game.Weapons.Unlocked(0),Is.True);Assert.That(game.Weapons.Unlocked(1),Is.False);
            Assert.That(game.Weapons.Unlocked(2),Is.True);Assert.That(game.Weapons.Unlocked(3),Is.True);
            game.Progress.equippedWeapon=0;game.Weapons.Restore();game.Weapons.RequestNext();game.Weapons.BeforeTick(.2f,true);
            Assert.That(game.Weapons.Equipped.id,Is.EqualTo(DarklightWeaponId.Chain));
            game.Weapons.RequestNext();game.Weapons.BeforeTick(.2f,true);
            Assert.That(game.Weapons.Equipped.id,Is.EqualTo(DarklightWeaponId.AbyssRider));
            Assert.That(game.AbyssRider.Active,Is.True);Assert.That(game.AbyssRider.ExpandedMoves.Length,Is.EqualTo(6));
            Call(game,"SetTrainingForm",3);Assert.That(game.Form,Is.EqualTo(2));
            Call(game,"TickFormState",2f);Assert.That(game.PresentedForm,Is.EqualTo(2));
            Assert.That(game.IncomingMultiplier,Is.LessThan(1));Assert.That(game.StrikeRangeBonus,Is.GreaterThan(0));
            yield return null;
        }
        [UnityTest] public IEnumerator RequiemMotorcycleUsesEightAuthoredViewsWithoutBorrowingRock()
        {
            game.Weapons.Request(3);game.Weapons.BeforeTick(.2f,true);Call(game,"SetTrainingForm",0);
            var fighter=lab.Player;var view=fighter.GetComponent<DarklightCharacterView>();var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var seen=new System.Collections.Generic.HashSet<Sprite>();DarklightMotorcycleRedesignBank.ClearCache();
            for(int d=0;d<8;d++)
            {
                float angle=d*Mathf.PI/4;var facing=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                fighter.Model.Tick(.01f,new CombatInput{aim=facing});view.Present(fighter.Model,facing);
                var expected=DarklightMotorcycleRedesignBank.DualSawsPose(d,fighter.Model);
                Assert.That(expected,Is.Not.Null,"Missing on-foot Réquiem dual-saw view "+d);
                Assert.That(art.sprite,Is.SameAs(expected));Assert.That(seen.Add(expected),Is.True,"Real eight-view coverage");
                Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.True);
                Assert.That(view.UsesMotorcyclePoseFallback,Is.True,"Static ready pose is not a walking or idle animation");
                Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
            }
            yield return null;
        }
        [UnityTest] public IEnumerator RequiemMountedIdleAnimatesEightAuthoredDirectionsAndStopsWhenMoving()
        {
            game.Weapons.Request(3);game.Weapons.BeforeTick(.2f,true);Call(game,"SetTrainingForm",0);
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
                Assert.That(art.sprite,Is.SameAs(DarklightMotorcycleRedesignBank.DualSawsPose(d,fighter.Model)),"On-foot consumer must not use mounted idle frames");
                Assert.That(DarklightMotorcycleRedesignBank.IsDualSaws(art.sprite),Is.True);
                Assert.That(view.UsesMotorcyclePoseFallback,Is.True);
                view.Present(fighter.Model,facing);
                Assert.That(art.sprite,Is.SameAs(DarklightMotorcycleRedesignBank.DualSawsPose(d,fighter.Model)),"Moving on foot still uses the dual-saw presentation, not a mounted idle loop");
            }
            Assert.That(DarklightMotorcycleRedesignBank.Idle(1,0,.1f),Is.Null,"Normal idle is not transformed art");
            yield return null;
        }
        [UnityTest] public IEnumerator RealWeaponSpecialMountsDuringDashThenReturnsToDualSaws()
        {
            game.Weapons.Request(3);game.Weapons.BeforeTick(.2f,true);Call(game,"SetTrainingForm",0);
            var fighter=lab.Player;var view=fighter.GetComponent<DarklightCharacterView>();
            var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            DarklightMotorcycleRedesignBank.ClearCache();fighter.ResetModel();
            var dash=game.Weapons.Equipped.loadout.weaponSpecial;
            Assert.That(dash,Is.Not.Null,"The real Requiem Moto loadout supplies its weapon special");
            Assert.That(fighter.Model.TryAirFollowup(dash,new CombatInput{aim=Vector2.down}),Is.True,"Start the actual dash attack through the combat model");
            Assert.That(game.AbyssRider.MountedPresentation,Is.True,"Vehicle technique owns the mounted presentation");
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
