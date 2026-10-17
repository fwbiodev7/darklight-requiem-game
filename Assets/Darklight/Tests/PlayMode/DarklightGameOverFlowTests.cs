using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed class DarklightGameOverFlowTests
    {
        DarklightCampaign game;DarklightCombatLab lab;IDisposable storage;string path;
        Keyboard keyboard;InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static readonly BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,Hidden).GetValue(target);
        [UnitySetUp] public IEnumerator Load()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            path=System.IO.Path.GetFullPath("work/game-over-flow/"+Guid.NewGuid().ToString("N")+".json");storage=DarklightProgress.IsolateStorageForTests(path);
            background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard=InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewCampaign();yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
            storage?.Dispose();foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);
            AudioListener.pause=true;AudioListener.volume=0;yield return null;
        }
        [UnityTest] public IEnumerator SavedCoordinatesSurviveDeathAndReturnWithEquipment()=>VerifyReturn("primary");
        [UnityTest] public IEnumerator OldSaveWithoutCoordinatesUsesCheckpoint()=>VerifyReturn("old");
        [UnityTest] public IEnumerator CorruptPrimaryUsesBackupWithoutOverwritingIt()=>VerifyReturn("backup");
        [UnityTest] public IEnumerator NoFilesUseCurrentCheckpointFallback()=>VerifyReturn("missing");
        IEnumerator VerifyReturn(string mode)
        {
            var saved=new DarklightProgress{chapter=0,checkpoint=1,level=3,xp=17,potions=2,blood=75,weaponUnlocks=7,equippedWeapon=1};
            var map=DarklightMapDefinition.ForChapter(0);Vector2 expected=map.SafeSpawn(map.Checkpoints[1]);
            if(mode=="primary"||mode=="backup")
            {expected=map.SafeSpawn(expected+Vector2.right*.4f);saved.RecordReturnPoint(0,1,expected);}
            if(mode=="backup"){File.WriteAllText(path+".bak",JsonUtility.ToJson(saved));File.WriteAllText(path,"{corrupt");}
            else if(mode!="missing")saved.Save();
            if(mode=="missing")typeof(DarklightCampaign).GetProperty("Progress").SetValue(game,saved);
            var primary=File.Exists(path)?File.ReadAllBytes(path):null;var backup=File.Exists(path+".bak")?File.ReadAllBytes(path+".bak"):null;
            game.Progress.preferredForm=2;game.Blood.Set(100);yield return null;
            Assert.That(game.TryTransformPlayer(),Is.True);
            float entryDeadline=Time.realtimeSinceStartup+5;
            while(game.Transforming&&Time.realtimeSinceStartup<entryDeadline)yield return null;
            Assert.That(game.Form,Is.EqualTo(2));Assert.That(game.Transforming,Is.False);
            var health=lab.Player.GetComponent<HealthSystem>();health.SetInvincibilityDuration(0);health.TakeDamage(100000);
            yield return null;yield return null;
            Assert.That(lab.Player.Model.State,Is.EqualTo(CombatState.Dead));Assert.That(lab.Paused,Is.True);
            Assert.That(Field<string>(game,"panel"),Is.Not.EqualTo("Death"),"Game Over must wait for the protected reversion");
            yield return new WaitForSecondsRealtime(DarklightDeathReversion.Duration+.1f);
            Assert.That(Field<string>(game,"panel"),Is.EqualTo("Death"));Assert.That(game.Form,Is.Zero);
            Assert.That(Field<System.Collections.IList>(game,"buttons").Count,Is.EqualTo(3));
            Assert.That(lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel").gameObject.activeInHierarchy,Is.True);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            float deadline=Time.realtimeSinceStartup+5;
            while(!game.Active&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(game.Active,Is.True,"Resume button must finish its reveal and release gameplay");Assert.That(lab.Paused,Is.False);
            Assert.That(health.CurrentHealth,Is.EqualTo(health.MaxHealth));Assert.That(game.Form,Is.Zero);
            Assert.That(lab.Player.Model.State,Is.EqualTo(CombatState.Neutral));
            Assert.That(game.Progress.xp,Is.EqualTo(17));Assert.That(game.Progress.potions,Is.EqualTo(2));Assert.That(game.Progress.equippedWeapon,Is.EqualTo(1));
            Assert.That(Vector2.Distance(lab.Player.transform.position,expected),Is.LessThan(.03f));
            if(primary==null)Assert.That(File.Exists(path),Is.False);else CollectionAssert.AreEqual(primary,File.ReadAllBytes(path));
            if(backup==null)Assert.That(File.Exists(path+".bak"),Is.False);else CollectionAssert.AreEqual(backup,File.ReadAllBytes(path+".bak"));
            Assert.That(File.Exists(path+".tmp"),Is.False);
            foreach(var audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(audio.isPlaying,Is.False,"Test must not start any audio source");
        }
    }
}
