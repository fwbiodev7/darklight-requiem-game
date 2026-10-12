using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Darklight.Tests
{
    public sealed class DarklightCampaignTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        static void Invoke(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(obj,args);
        static T Get<T>(object obj,string name)=>(T)obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);
        [UnitySetUp] public IEnumerator LoadCampaign()
        {
            DarklightAutomation.Silent=true;
            oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;
        }
        [UnityTearDown] public IEnumerator RestoreInputSettings(){InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;yield return null;}
        void AssertSilent(){foreach(var a in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(a.isPlaying,Is.False,"Automation started audio");}
        [UnityTest] public IEnumerator TenMapsHavePhysicalRoutesClearSpawnsAndBoundedKnockback()
        {
            var path=new List<Vector2>();
            for(int i=0;i<DarklightStory.Chapters.Length;i++)
            {
                game.PreviewChapter(i);yield return null;yield return null;Physics2D.SyncTransforms();
                foreach(var point in game.Map.Arenas)Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,point,path),Is.True,"Arena route: chapter "+i);
                foreach(var point in new[]{game.Map.Ally,game.Map.Relic,game.Map.Portal})
                {
                    var approach=game.Map.SafeSpawn(point);
                    Assert.That(Vector2.Distance(approach,point),Is.LessThan(1.7f),"Interaction out of reach: chapter "+i);
                    Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,approach,path),Is.True,"Exploration route: chapter "+i+" target "+point+" approach "+approach);
                }
                var body=lab.Player.GetComponent<Rigidbody2D>();DarklightNavigation.SafeStep(body,Vector2.left*100);Physics2D.SyncTransforms();
                Assert.That(game.Map.Bounds.Contains(body.position),Is.True,"Knockback escaped map");
                var camera=lab.WorldCamera.GetComponent<DarklightLabCamera>();Assert.That(camera.bounds,Is.EqualTo(game.Map.Bounds));
                Assert.That(lab.WorldCamera.orthographicSize,Is.EqualTo(DarklightLabCamera.GameplayHalfHeight).Within(.01f));AssertSilent();
            }
        }
        [UnityTest] public IEnumerator MovementPauseAndDodgeUseActualPhysics()
        {
            game.PreviewChapter(0);yield return null;yield return null;
            var keyboard=InputSystem.AddDevice<Keyboard>();Vector2 start=lab.Player.transform.position;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));yield return new WaitForSeconds(.18f);
            Assert.That(lab.Player.transform.position.x-start.x,Is.GreaterThan(.45f));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D,Key.Space));yield return new WaitForSeconds(.1f);
            Assert.That(lab.Player.Model.State,Is.EqualTo(CombatState.Dodge));
            Invoke(game,"Open","Pause");Vector2 paused=lab.Player.transform.position;
            var body=lab.Player.GetComponent<Rigidbody2D>();Vector2 pausedPhysics=body.position;
            Vector3 pausedCamera=lab.WorldCamera.transform.position;yield return new WaitForSeconds(.1f);
            Assert.That(Vector2.Distance(paused,lab.Player.transform.position),Is.LessThan(.03f));Assert.That(lab.Input.Gameplay.enabled,Is.False);
            Assert.That(Vector2.Distance(pausedPhysics,body.position),Is.LessThan(.001f));
            Assert.That(Vector3.Distance(pausedCamera,lab.WorldCamera.transform.position),Is.LessThan(.001f));
            Invoke(game,"Close");Assert.That(body.interpolation,Is.EqualTo(RigidbodyInterpolation2D.Interpolate));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.RemoveDevice(keyboard);AssertSilent();
        }
        [UnityTest] public IEnumerator AllPowersApplyGameplayAndKeepFormsIndependent()
        {
            for(int mode=0;mode<3;mode++)
            {
                game.PreviewChapter(0);yield return null;yield return null;game.Progress.mp3Unlocked=true;game.Resonance.Select(mode);
                var health=lab.Player.GetComponent<HealthSystem>();health.TakeDamage(30);float oldHealth=health.CurrentHealth;
                Assert.That(game.TryActivatePower(),Is.True);yield return new WaitForSeconds(.38f);
                Assert.That(game.Resonance.Active,Is.EqualTo(mode));
                if(mode==0){yield return new WaitForSeconds(.3f);Assert.That(health.CurrentHealth,Is.GreaterThan(oldHealth));}
                if(mode==1){Assert.That(game.DamageMultiplier,Is.EqualTo(1.45f).Within(.01f));Assert.That(game.Resonance.Impact,Is.EqualTo(2));}
                if(mode==2){Assert.That(game.AttackSpeed,Is.EqualTo(1.4f));Assert.That(game.MovementSpeed,Is.EqualTo(1.2f));}
                Assert.That(game.Form,Is.Zero);Assert.That(game.Resonance.Energy,Is.LessThan(70));
                game.Resonance.Tick(DarklightResonance.Durations[mode]+1);yield return null;Assert.That(game.Resonance.Active,Is.EqualTo(-1));AssertSilent();
            }
        }
        [UnityTest] public IEnumerator MenuSkipsLockedPowersAndSupportsPadMouseAndReturnFocus()
        {
            Invoke(game,"Open","MP3");yield return null;
            Assert.That(Get<int>(game,"selection"),Is.EqualTo(5),"Locked song buttons received focus");
            var pad=InputSystem.AddDevice<Gamepad>();
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
            Assert.That(Get<string>(game,"panel"),Is.EqualTo("Pause"));Assert.That(lab.Input.Action("Interact").WasPressedThisFrame(),Is.False);
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
            Invoke(game,"Open","Main");yield return null;Canvas.ForceUpdateCanvases();
            var options=game.transform.root;var hud=lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel/Botao3") as RectTransform;
            var mouse=InputSystem.AddDevice<Mouse>();Vector2 point=RectTransformUtility.WorldToScreenPoint(null,hud.TransformPoint(hud.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1});yield return null;yield return null;
            Assert.That(Get<string>(game,"panel"),Is.EqualTo("Options"));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;
            Invoke(game,"Open","Main");Assert.That(Get<int>(game,"selection"),Is.EqualTo(3));
            InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(pad);AssertSilent();
        }
        [UnityTest] public IEnumerator SupplementaryFramesAreDistinctAndPixelFiltered()
        {
            var view=lab.Player.GetComponent<DarklightCharacterView>();Assert.That(view.motion.Length,Is.EqualTo(24));
            var body=lab.Player.GetComponent<Rigidbody2D>();body.position=game.Map.Arenas[0]+Vector2.left*3;Physics2D.SyncTransforms();
            Invoke(game,"Spawn",RequiemEnemyKind.Crawler,game.Map.Arenas[0],false);
            var enemy=Object.FindAnyObjectByType<DarklightEnemy>();yield return null;
            Assert.That(enemy.attackFrames.Length,Is.EqualTo(4));Assert.That(enemy.attackFrames[1],Is.Not.EqualTo(enemy.attackFrames[2]));
            foreach(var sprite in enemy.attackFrames)Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            lab.Player.Model.Tick(.01f,new CombatInput{dodgePressed=true,move=Vector2.right});view.Present(lab.Player.Model,Vector2.right);
            var first=lab.Player.GetComponentInChildren<SpriteRenderer>().sprite;lab.Player.Model.Tick(.10f,default);view.Present(lab.Player.Model,Vector2.right);
            Assert.That(lab.Player.GetComponentInChildren<SpriteRenderer>().sprite,Is.Not.EqualTo(first));AssertSilent();
        }
    }
}


