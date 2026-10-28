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
                var expected=DarklightMotorcycleRedesignBank.Riding(0,d,0);
                Assert.That(expected,Is.Not.Null,"Missing full Réquiem rider view "+d);
                Assert.That(art.sprite,Is.SameAs(expected));Assert.That(seen.Add(expected),Is.True,"Real eight-view coverage");
                Assert.That(view.UsesMotorcyclePoseFallback,Is.True,"Static pose is not a walking or idle animation");
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
                fighter.ResetModel();float angle=d*Mathf.PI/4;var facing=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                var seen=new System.Collections.Generic.HashSet<Sprite>();
                foreach(float dt in new[]{.001f,.35f,.25f,.18f})
                {
                    fighter.Model.Tick(dt,new CombatInput{aim=facing});view.Present(fighter.Model,Vector2.zero);
                    var expected=DarklightMotorcycleRedesignBank.Idle(0,d,fighter.Model.Clock);
                    Assert.That(expected,Is.Not.Null,"Missing mounted idle direction "+d);
                    Assert.That(art.sprite,Is.SameAs(expected));Assert.That(seen.Add(expected),Is.True,"Four distinct full idle figures");
                    Assert.That(view.UsesMotorcyclePoseFallback,Is.False);
                    Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                }
                view.Present(fighter.Model,facing);
                Assert.That(art.sprite,Is.SameAs(DarklightMotorcycleRedesignBank.Riding(0,d,0)),"Moving must leave stationary idle");
                Assert.That(view.UsesMotorcyclePoseFallback,Is.True);
            }
            Assert.That(DarklightMotorcycleRedesignBank.Idle(1,0,.1f),Is.Null,"Normal idle is not transformed art");
            yield return null;
        }
    }
}
