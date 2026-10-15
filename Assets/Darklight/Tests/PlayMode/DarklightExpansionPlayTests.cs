using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Darklight.Tests
{
    public sealed class DarklightExpansionPlayTests
    {
        DarklightCampaign game;DarklightCombatLab lab;Gamepad pad;
        InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldEditor;
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(target);
        static void Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
        [UnitySetUp] public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;oldBackground=InputSystem.settings.backgroundBehavior;oldEditor=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewChapter(4);
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(pad!=null)InputSystem.RemoveDevice(pad);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditor;yield return null;}
        [UnityTest] public IEnumerator PauseUsesCurrentSceneAndFreezesCombatCameraAndResources()
        {
            game.Progress.vampireMastery=3;game.Progress.vampireSkills=63;game.Blood.Set(100);game.Blood.Activate(4,game.Progress);
            game.PreviewPanel("Pause");yield return null;yield return null;
            float clock=lab.Player.Model.Clock,blood=game.Energy,remaining=game.Blood.FrenzyRemaining;Vector3 camera=lab.WorldCamera.transform.position;
            var blur=Object.FindFirstObjectByType<DarklightPauseBlur>();Assert.That(blur,Is.Not.Null);Assert.That(blur.Visible,Is.True);Assert.That(blur.SceneTexture,Is.Not.Null);Assert.That(blur.SceneTexture,Is.Not.EqualTo(lab.WorldCamera.targetTexture));
            yield return new WaitForSeconds(.15f);Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));Assert.That(game.Energy,Is.EqualTo(blood));Assert.That(game.Blood.FrenzyRemaining,Is.EqualTo(remaining));Assert.That(lab.WorldCamera.transform.position,Is.EqualTo(camera));
        }
        [UnityTest] public IEnumerator VisualPickerUsesYoungPortraitAndReturnsToPauseWithGamepad()
        {
            game.Appearance.PreviewStage(4);game.PreviewPanel("Pause");game.PreviewPanel("Skins");yield return null;
            var preview=Field<UnityEngine.UI.Image>(game,"skinPreview");Assert.That(preview.sprite.name,Does.StartWith("Today_RequiemIdleKatanaYoung"));
            pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));yield return null;yield return null;
            Assert.That(Field<string>(game,"panel"),Is.EqualTo("Pause"));Assert.That(lab.Paused,Is.True);Assert.That(game.Appearance.Stage,Is.Zero);
        }
        [UnityTest] public IEnumerator BloodAbilityHasCostPauseRecoveryAndDamageInterruption()
        {
            game.Progress.vampireMastery=3;game.Progress.vampireSkills=63;game.Progress.vampireSelected=5;game.Blood.Set(100);yield return null;
            Assert.That(game.TryVampireSkill(),Is.True);Assert.That(game.Energy,Is.EqualTo(35));Assert.That(game.TryVampireSkill(),Is.False);
            game.PreviewPanel("Pause");float recovery=game.Blood.Recovery[5];yield return new WaitForSeconds(.1f);Assert.That(game.Blood.Recovery[5],Is.EqualTo(recovery));
            Invoke(game,"Close");lab.Player.Model.Hurt();game.Damaged();yield return null;Assert.That(game.VampireCasting,Is.False);Assert.That(game.Energy,Is.EqualTo(35));Assert.That(game.Blood.Recovery[5],Is.GreaterThan(23));
        }
        [UnityTest] public IEnumerator ShadowStepCannotCrossScenery()
        {
            game.Progress.vampireMastery=3;game.Progress.vampireSkills=63;game.Progress.vampireSelected=1;game.Blood.Set(100);
            var body=lab.Player.GetComponent<Rigidbody2D>();body.position=game.Map.SafeSpawn(game.Map.Spawn);Physics2D.SyncTransforms();
            lab.Player.Model.Tick(.01f,new CombatInput{aim=Vector2.right});var wall=new GameObject("VampireCollisionFixture");wall.transform.position=(Vector3)body.position+Vector3.right;wall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,3);Physics2D.SyncTransforms();Vector2 before=body.position;
            Assert.That(game.TryVampireSkill(),Is.True);yield return new WaitForSeconds(.28f);Assert.That(body.position.x-before.x,Is.InRange(0,.61f));Assert.That(game.Energy,Is.EqualTo(82));Object.Destroy(wall);
        }
        [UnityTest] public IEnumerator FirstAwakeningIsPlayableOnceWithoutResettingAppearance()
        {
            game.PreviewChapter(0);game.Appearance.PreviewStage(2);Invoke(game,"Spawn",RequiemEnemyKind.Kael,game.Map.SafeSpawn(game.Map.Arenas[3]),false);yield return null;
            var health=lab.Player.GetComponent<HealthSystem>();float damage=game.ProtectAwakening(90);
            Assert.That(game.Progress.awakeningState,Is.EqualTo(1));Assert.That(game.Progress.vampireMastery,Is.EqualTo(1));Assert.That(game.Form,Is.EqualTo(1));Assert.That(game.Active,Is.True);Assert.That(game.Appearance.Stage,Is.Zero);Assert.That(damage,Is.LessThan(90));
            game.Blood.Set(10);game.ProtectAwakening(99);Assert.That(game.Energy,Is.EqualTo(10));Assert.That(health.HealthPercent,Is.GreaterThan(.2f));
        }
        [UnityTest] public IEnumerator SelectStillOpensAtlasWhileBloodControlsDoNotRotateTowardMouse()
        {
            pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Select));yield return null;yield return null;Assert.That(game.Atlas.IsOpen,Is.True);
        }
    }
}


