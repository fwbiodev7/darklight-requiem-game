using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightTransformationTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        static object Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
        [UnitySetUp]public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewChapter(4);yield return null;
            game.Progress.formsUnlocked=3;game.Progress.weaponUnlocks=7;
        }
        [UnityTest]public IEnumerator TransitionPreservesCostPauseAndEndingWithoutRetrigger()
        {
            game.Progress.preferredForm=2;game.Blood.Set(100);
            Assert.That(game.TryTransformPlayer(),Is.True);Assert.That(game.Energy,Is.EqualTo(40));
            Assert.That(game.Transforming,Is.True);Assert.That(game.PresentedForm,Is.Zero);
            Assert.That(game.TryTransformPlayer(),Is.False);Assert.That(game.TryVampireSkill(),Is.False);Assert.That(game.TryActivatePower(),Is.False);
            game.PreviewPanel("Pause");float progress=game.TransformationProgress,time=game.FormTime;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(game.TransformationProgress,Is.EqualTo(progress));Assert.That(game.FormTime,Is.EqualTo(time));
            Invoke(game,"Close");yield return new WaitForSeconds(DarklightCampaign.TransformationEnterDuration+.15f);
            Assert.That(game.Transforming,Is.False);Assert.That(game.PresentedForm,Is.EqualTo(2));
            Assert.That(game.TryTransformPlayer(),Is.True);Assert.That(game.Form,Is.Zero);Assert.That(game.PresentedForm,Is.EqualTo(2));
            yield return new WaitForSeconds(DarklightCampaign.TransformationExitDuration+.15f);Assert.That(game.Transforming,Is.False);Assert.That(game.PresentedForm,Is.Zero);
            Assert.That(game.Energy,Is.EqualTo(40));
        }
        [UnityTest]public IEnumerator EveryFormHasThreeActiveMinutesAndMasteryStillAddsTime()
        {
            game.Progress.mastery=0;
            for(int form=1;form<=3;form++)
            {
                lab.Player.ResetModel();game.Progress.preferredForm=form;game.Blood.Set(100);
                Assert.That(game.TryTransformPlayer(),Is.True);Assert.That(game.FormTime,Is.EqualTo(180f));
                Invoke(game,"TickFormState",DarklightCampaign.TransformationEnterDuration);
                Assert.That(game.FormTime,Is.EqualTo(180f).Within(.001f));Assert.That(game.PresentedForm,Is.EqualTo(form));
                Invoke(game,"TickFormState",179.9f);Assert.That(game.Form,Is.EqualTo(form));
                Assert.That(game.FormTime,Is.GreaterThan(0));Invoke(game,"TickFormState",.11f);
                Assert.That(game.Form,Is.Zero);Assert.That(game.FormTime,Is.Zero);
                Invoke(game,"TickFormState",DarklightCampaign.TransformationExitDuration);
            }
            Assert.That(DarklightCampaign.TransformationDuration(4),Is.EqualTo(188f));
            Assert.That(DarklightCampaign.TransformationDuration(-2),Is.EqualTo(180f));
            yield return null;
        }
        [UnityTest]public IEnumerator BothSkinsThreeWeaponsAndEightDirectionsKeepScaleAndOneWingLayer()
        {
            var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int form=1;form<=3;form++)
            {
                lab.Player.ResetModel();game.SelectSkin(skin,false);game.Progress.equippedWeapon=weapon;game.Weapons.Restore();game.Progress.preferredForm=form;game.Blood.Set(100);
                Assert.That(game.TryTransformPlayer(),Is.True);Invoke(game,"TickFormPresentation",DarklightCampaign.TransformationEnterDuration+.01f);
                for(int d=0;d<8;d++)
                {
                    var model=new CombatModel(game.Weapons.Equipped.loadout);model.Tick(.01f,new CombatInput{aim=Quaternion.Euler(0,0,-d*45)*Vector2.down});
                    view.Present(model,Vector2.zero);Assert.That(art.sprite,Is.Not.Null);Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                    var wings=art.transform.parent.Find("AsasEspectrais");
                    Assert.That(wings,Is.Null);
                }
                Assert.That(game.TryTransformPlayer(),Is.True);Invoke(game,"TickFormPresentation",DarklightCampaign.TransformationExitDuration+.01f);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator DamageDeathAndReloadCannotLeaveTransformationInputLocked()
        {
            game.Blood.Set(100);lab.Player.Model.Hurt();Assert.That(game.TryTransformPlayer(),Is.False);Assert.That(game.Energy,Is.EqualTo(100));
            lab.Player.ResetModel();Assert.That(game.TryTransformPlayer(),Is.True);
            lab.Player.GetComponent<HealthSystem>().TakeDamage(9999);yield return null;yield return null;
            Assert.That(game.Transforming,Is.False);Assert.That(game.Form,Is.Zero);
            game.PreviewChapter(4);yield return null;Assert.That(game.Transforming,Is.False);Assert.That(game.PresentedForm,Is.Zero);
        }
        [UnityTest]public IEnumerator NoctharFinalPhaseUsesOwnIdleAndAllAttackPosesAndRestoresCorrectly()
        {
            game.PreviewChapter(9);Invoke(game,"Spawn",RequiemEnemyKind.Nocthar,game.Map.SafeSpawn(game.Map.Arenas[3]),false);yield return null;
            var boss=game.CurrentBoss;Assert.That(boss,Is.Not.Null);var art=boss.GetComponentInChildren<SpriteRenderer>();
            Invoke(boss,"Show",0);Assert.That(art.sprite.name,Does.StartWith("ExpansionBosses"));
            boss.RestorePhase(3);Invoke(boss,"Show",0);Assert.That(art.sprite.name,Is.EqualTo("NoctharFinal_0"));
            Vector3 scale=art.transform.localScale;
            for(int i=0;i<4;i++){Invoke(boss,"ShowAttack",i);Assert.That(art.sprite.name,Is.EqualTo("NoctharFinal_"+(i+4)));Assert.That(art.transform.localScale,Is.EqualTo(scale));}
            boss.RestorePhase(2);Invoke(boss,"Show",0);Assert.That(art.sprite.name,Does.StartWith("ExpansionBosses"));
            yield return null;
        }
    }
}
