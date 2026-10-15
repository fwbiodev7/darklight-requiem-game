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
    public sealed class DarklightPlayerControlsTests
    {
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        static void Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        [UnityTest] public IEnumerator FirstChapterTriggerAndRemapMenuRemainFunctionalAfterCancelAndConflict()
        {
            DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var input=DarklightCombatLab.Instance.Input;
            var pad=InputSystem.AddDevice<Gamepad>();var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                input.RestoreBindings();game.PreviewChapter(0);game.Progress.formsUnlocked=0;game.Progress.preferredForm=3;game.Blood.Set(100);yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftTrigger=1});yield return null;yield return null;
                Assert.That(game.Form,Is.EqualTo(3));Assert.That(game.Transforming,Is.True);Assert.That(game.Energy,Is.EqualTo(40));Assert.That(game.Progress.chapter,Is.Zero);
                InputSystem.QueueStateEvent(pad,new GamepadState());game.PreviewPanel("Pause");game.PreviewPanel("Controls");
                var slot=input.Slots("Transform",false)[0];typeof(DarklightCampaign).GetField("bindingSlot",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game,slot);
                game.PreviewPanel("BindingSlot");Invoke(game,"StartBindingCapture");yield return new WaitForSecondsRealtime(.3f);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("BindingSlot"));Assert.That(input.UI.enabled,Is.True);Assert.That(input.Gameplay.enabled,Is.False);Assert.That(slot.binding.effectivePath,Is.EqualTo("<Keyboard>/t"));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.2f);
                Invoke(game,"StartBindingCapture");yield return new WaitForSecondsRealtime(.3f);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Q));yield return new WaitForSecondsRealtime(.2f);
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("BindingConflict"));Assert.That(slot.binding.effectivePath,Is.EqualTo("<Keyboard>/t"));Assert.That(input.UI.enabled,Is.True);
                Assert.That(input.Swap(slot,"<Keyboard>/q"),Is.True);game.PreviewPanel("Controls");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.2f);game.PreviewPanel("Pause");Invoke(game,"Close");yield return null;
                Assert.That(input.Gameplay.enabled,Is.True);Assert.That(input.Action("Transform").bindings[0].effectivePath,Is.EqualTo("<Keyboard>/q"));
            }
            finally {InputSystem.RemoveDevice(pad);InputSystem.RemoveDevice(keyboard);}
        }
    }
}
