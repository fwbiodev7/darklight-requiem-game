using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightAudioSettingsTests
    {
        static readonly string[] Keys={"Darklight.Master","Darklight.Music","Darklight.Effects","Darklight.Ambient","Darklight.UI","Darklight.Enemies","Darklight.Player"};
        readonly Dictionary<string,(bool exists,float value)> saved=new();
        DarklightCampaign game;DarklightCombatLab lab;float listener;
        InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(target);
        static void Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
        [UnitySetUp] public IEnumerator SetUp()
        {
            listener=AudioListener.volume;foreach(string k in Keys)saved[k]=(PlayerPrefs.HasKey(k),PlayerPrefs.GetFloat(k));
            DarklightAutomation.Silent=true;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewPanel("Options");Invoke(game,"Activate",1);yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Invoke(game,"FlushAudioPreferences");
            foreach(var entry in saved){if(entry.Value.exists)PlayerPrefs.SetFloat(entry.Key,entry.Value.value);else PlayerPrefs.DeleteKey(entry.Key);}PlayerPrefs.Save();
            AudioListener.volume=listener;InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;yield return null;
        }
        RectTransform Row(int index)=>lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel/Botao"+index) as RectTransform;
        UnityEngine.UI.Slider Slider(int index)=>Row(index).Find("ControleVolume").GetComponent<UnityEngine.UI.Slider>();
        Vector2 TrackPoint(int index,float amount,float y=-6)=>RectTransformUtility.WorldToScreenPoint(null,Slider(index).transform.TransformPoint(new Vector3(4+amount*(Slider(index).GetComponent<RectTransform>().rect.width-8),y,0)));
        [UnityTest] public IEnumerator DragKeepsItsRowClampsOutsideTrackAndPersistsExactPercent()
        {
            var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                Canvas.ForceUpdateCanvases();var original=Slider(7);
                InputSystem.QueueStateEvent(mouse,new MouseState{position=TrackPoint(7,.37f),buttons=1});yield return null;yield return null;
                Assert.That(game.MusicVolume,Is.EqualTo(.37f).Within(.001f));Assert.That(Slider(7),Is.SameAs(original),"Dragging rebuilt the control");
                Assert.That(Row(7).Find("Percentual").GetComponent<UnityEngine.UI.Text>().text,Is.EqualTo("37%"));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=TrackPoint(7,1.8f,-70),buttons=1});yield return null;yield return null;
                Assert.That(game.MusicVolume,Is.EqualTo(1));Assert.That(Field<int>(game,"selection"),Is.EqualTo(7));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=TrackPoint(7,-.5f),buttons=1});yield return null;yield return null;Assert.That(game.MusicVolume,Is.Zero);
                InputSystem.QueueStateEvent(mouse,new MouseState{position=TrackPoint(7,.37f),buttons=1});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=TrackPoint(7,.37f)});yield return null;yield return null;
                Assert.That(PlayerPrefs.GetFloat("Darklight.Music"),Is.EqualTo(.37f).Within(.001f));
                Invoke(game,"Open","Main");game.PreviewPanel("Options");Invoke(game,"Activate",1);yield return null;
                Assert.That(Slider(7).value,Is.EqualTo(37));Assert.That(lab.Paused,Is.True);
                string folder=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Darklight20261013");System.IO.Directory.CreateDirectory(folder);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"audio-settings.png"));yield return null;yield return null;
            }
            finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator GamepadChangesByOnePercentAndEveryCategoryHasVisibleControl()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                string[] labels={"VOLUME GERAL","MUSICA","EFEITOS","AMBIENTE","INTERFACE","INIMIGOS","PERSONAGEM"};
                for(int i=0;i<labels.Length;i++){Assert.That(Row(i+6).Find("Rotulo").GetComponent<UnityEngine.UI.Text>().text,Is.EqualTo(labels[i]));Assert.That(Slider(i+6).wholeNumbers,Is.True);Assert.That(Row(i+6).rect.width,Is.GreaterThan(300));}
                game.MusicVolume=.5f;game.PreviewPanel("Options",7);yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.left});yield return null;yield return null;Assert.That(game.MusicVolume,Is.EqualTo(.49f).Within(.001f));
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.right});yield return null;yield return null;Assert.That(game.MusicVolume,Is.EqualTo(.5f).Within(.001f));
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
            }
            finally{InputSystem.RemoveDevice(pad);}
        }
        [UnityTest] public IEnumerator MasterAndCategoryControlsRouteToActualSoundGainsAndPreferencesReload()
        {
            game.EffectsVolume=.5f;game.PlayerVolume=.2f;game.EnemyVolume=.8f;game.InterfaceVolume=.35f;
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Character),Is.EqualTo(.1f).Within(.001f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Enemy),Is.EqualTo(.4f).Within(.001f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Combat),Is.EqualTo(.5f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Interface),Is.EqualTo(.35f));
            foreach(string key in Keys)PlayerPrefs.SetFloat(key,.63f);
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;
            Assert.That(game.MasterVolume,Is.EqualTo(.63f));Assert.That(game.MusicVolume,Is.EqualTo(.63f));Assert.That(game.EnemyVolume,Is.EqualTo(.63f));Assert.That(game.PlayerVolume,Is.EqualTo(.63f));Assert.That(AudioListener.volume,Is.Zero);
            Assert.That(DarklightCampaign.VolumePercent(float.NaN),Is.Zero);Assert.That(DarklightCampaign.VolumePercent(1.4f),Is.EqualTo(100));yield return null;
        }
    }
}

