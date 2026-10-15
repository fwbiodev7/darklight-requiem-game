using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightPauseTests
    {
        InputSettings.BackgroundBehavior background;
        InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj);
        static void Invoke(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
        [SetUp] public void Prepare()
        {
            DarklightAutomation.Silent=true;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        }
        [TearDown] public void Restore(){InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;}
        static void Silent(){foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);}
        [UnityTest] public IEnumerator CampaignAndExplorationPauseKeepThinMenuStyleAndReturnFocus()
        {
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var g=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;g.PreviewChapter(0);
            var health=lab.Player.GetComponent<HealthSystem>();health.SetInvincibilityDuration(.4f);health.TakeDamage(1);g.PreviewPanel("Pause",8);yield return null;
            var menus=lab.PixelHUD.transform.Find("HUD640x360/Menus");
            Assert.That(menus.Find("VesperaCinematografica").gameObject.activeSelf,Is.False);
            Assert.That(menus.Find("Painel/Moldura").gameObject.activeSelf,Is.False);
            var first=menus.Find("Painel/Botao0") as RectTransform;Assert.That(first.sizeDelta,Is.EqualTo(new Vector2(214,20)));
            Assert.That(first.GetComponent<UnityEngine.UI.Image>().sprite,Is.Null);Assert.That(first.Find("Borda0"),Is.Not.Null);
            Assert.That(first.Find("Rotulo").GetComponent<UnityEngine.UI.Text>().font,Is.EqualTo(lab.PixelHUD.pixelFont));
            Assert.That(menus.Find("Painel/SubtituloRequiem").GetComponent<UnityEngine.UI.Text>().text,Is.EqualTo("DARKLIGHT: REQUIEM"));
            Vector3 player=lab.Player.transform.position,camera=lab.WorldCamera.transform.position;
            float clock=lab.Player.Model.Clock;yield return new WaitForSecondsRealtime(.55f);Assert.That(health.IsInvincible,Is.True,"Pause consumed combat invulnerability");
            Assert.That(lab.Player.transform.position,Is.EqualTo(player));Assert.That(lab.WorldCamera.transform.position,Is.EqualTo(camera));
            Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));Assert.That(lab.CombatDelta,Is.Zero);
            Invoke(g,"Activate",8);yield return null;Assert.That(Field<string>(g,"panel"),Is.EqualTo("Options"));
            Invoke(g,"Open","Pause");yield return null;Assert.That(Field<int>(g,"selection"),Is.EqualTo(8));
            Invoke(g,"Activate",0);yield return null;Assert.That(lab.Paused,Is.False);
            g.ExploreRegion(6);g.PreviewPanel("Pause");yield return null;
            Assert.That(menus.Find("Painel/Botao1/Rotulo").GetComponent<UnityEngine.UI.Text>().text,Is.EqualTo("ESCOLHER REGIÃO"));
            Assert.That(menus.Find("Painel/Moldura").gameObject.activeSelf,Is.False);Silent();
        }
        [UnityTest] public IEnumerator TrainingPauseSupportsPadMouseDefenseRestartAndMainMenu()
        {
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCombatLab.unity");yield return null;yield return null;
            var lab=DarklightCombatLab.Instance;var pad=InputSystem.AddDevice<Gamepad>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                lab.SetPaused(true);yield return null;
                var root=lab.PixelHUD.transform.Find("HUD640x360/PausaCinematografica");Assert.That(root.gameObject.activeSelf,Is.True);
                Assert.That(root.Find("Botao2/Rotulo").GetComponent<UnityEngine.UI.Text>().text,Is.EqualTo("ALTERNAR TREINO DE DEFESA"));
                Vector3 p=lab.Player.transform.position;float clock=lab.Player.Model.Clock;
                yield return new WaitForSecondsRealtime(.15f);Assert.That(lab.Player.transform.position,Is.EqualTo(p));Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));
                InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.down});yield return null;yield return null;
                Assert.That(lab.PauseSelection,Is.EqualTo(1));InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.down});yield return null;yield return null;
                Assert.That(lab.PauseSelection,Is.EqualTo(2));InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                bool defense=lab.Targets[0].DefensePractice;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
                Assert.That(lab.Targets[0].DefensePractice,Is.EqualTo(!defense));Assert.That(lab.Paused,Is.False);
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                lab.SetPaused(true);yield return null;Canvas.ForceUpdateCanvases();
                var button=root.Find("Botao0") as RectTransform;Vector2 point=RectTransformUtility.WorldToScreenPoint(null,button.TransformPoint(button.rect.center));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1});yield return null;yield return null;Assert.That(lab.Paused,Is.False);
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;
                lab.SetPaused(true);Invoke(lab,"SelectMenu",1);yield return null;yield return null;
                lab=DarklightCombatLab.Instance;Assert.That(lab.Paused,Is.False);Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("DarklightCombatLab"));
                lab.SetPaused(true);Invoke(lab,"SelectMenu",3);yield return null;yield return null;
                Assert.That(DarklightCampaign.Instance,Is.Not.Null);Assert.That(Field<string>(DarklightCampaign.Instance,"panel"),Is.EqualTo("Main"));Silent();
            }
            finally {InputSystem.RemoveDevice(pad);InputSystem.RemoveDevice(mouse);}
        }
    }
}

