using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightMenuTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);
        static void Invoke(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(obj,args);
        [UnitySetUp] public IEnumerator SetUp()
        {
            DarklightAutomation.Silent=true;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;
        }
        [UnityTearDown] public IEnumerator TearDown(){InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;yield return null;}
        void Silent(){foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);}
        UnityEngine.UI.Text Label(int i)=>lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel/Botao"+i+"/Rotulo").GetComponent<UnityEngine.UI.Text>();
        [UnityTest] public IEnumerator MainUsesApprovedOrderValidSaveAndSeparatedScenery()
        {
            string[] expected={"CONTINUAR","NOVO JOGO","CARREGAR JOGO","CONFIGURAÇÕES","TROCAR SKIN","CRÉDITOS","SAIR"};
            for(int i=0;i<expected.Length;i++)Assert.That(Label(i).text,Is.EqualTo(expected[i]));
            var hud=lab.PixelHUD.transform.Find("HUD640x360/Menus");
            Assert.That(hud.Find("Painel/Moldura").gameObject.activeSelf,Is.False);
            var city=hud.Find("VesperaCinematografica/CidadeDeVespera").GetComponent<UnityEngine.UI.Image>();
            Assert.That(city.sprite,Is.Not.Null);Assert.That(city.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(lab.PixelHUD.canvas.pixelPerfect,Is.True);
            var saved=DarklightProgress.Load();var summary=hud.Find("Painel/Botao0/ResumoDoSave").GetComponent<UnityEngine.UI.Text>();
            Assert.That(summary.text,Is.EqualTo(DarklightCampaign.MenuSaveSummary(saved)));
            if(saved==null)Assert.That(Field<int>(game,"selection"),Is.Not.Zero,"Continue received focus without valid save");
            Assert.That(DarklightCampaign.MenuProgressPercent(new DarklightProgress{chapter=2,checkpoint=2}),Is.EqualTo(25));
            Assert.That(DarklightCampaign.MenuProgressPercent(new DarklightProgress{completed=true}),Is.EqualTo(100));
            Silent();yield return null;
        }
        [UnityTest] public IEnumerator SkinSelectionByPadUpdatesSceneryAndKeyboardReturnsToSameButton()
        {
            string save=System.IO.File.Exists(DarklightProgress.Path)?System.IO.File.ReadAllText(DarklightProgress.Path):null;
            var pad=InputSystem.AddDevice<Gamepad>();var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                game.PreviewPanel("Main",4);InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("Skins"));InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                game.PreviewPanel("Skins",1);InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
                Assert.That(game.SelectedSkin,Is.EqualTo(1));
                var hero=lab.PixelHUD.transform.Find("HUD640x360/Menus/VesperaCinematografica/FabioSelecionado").GetComponent<UnityEngine.UI.Image>();
                Assert.That(hero.sprite.name,Does.StartWith("RockPose_"));
                InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("Main"));Assert.That(Field<int>(game,"selection"),Is.EqualTo(4));
                game.PreviewPanel("Skins",0);Invoke(game,"Activate",0);yield return null;
                Assert.That(game.SelectedSkin,Is.Zero);Assert.That(hero.sprite.name,Does.StartWith("RequiemPose_"));
                Assert.That(System.IO.File.Exists(DarklightProgress.Path)?System.IO.File.ReadAllText(DarklightProgress.Path):null,Is.EqualTo(save),"Skin selection overwrote campaign save");
                Silent();
            }
            finally {InputSystem.RemoveDevice(pad);InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator MouseOpensLoadPickerAndBackDoesNotEnterGameplay()
        {
            var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                game.PreviewPanel("Main");yield return null;Canvas.ForceUpdateCanvases();
                var button=Label(2).transform.parent as RectTransform;Vector2 point=RectTransformUtility.WorldToScreenPoint(null,button.TransformPoint(button.rect.center));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1});yield return null;yield return null;
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("LoadGames"));Assert.That(Label(0).text,Is.EqualTo("SAVE ATUAL"));Assert.That(Label(1).text,Is.EqualTo("COPIA ANTERIOR"));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;
                Invoke(game,"Activate",2);yield return null;
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("Main"));Assert.That(lab.Paused,Is.True);Silent();
            }
            finally {InputSystem.RemoveDevice(mouse);}
        }
    }
}
