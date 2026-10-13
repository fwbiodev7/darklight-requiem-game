using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightAwakeningMusicTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        static void Invoke(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        static T Get<T>(object target,string field)=>(T)target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        [UnitySetUp]public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;
            game.PreviewChapter(0);Invoke(game,"Spawn",RequiemEnemyKind.Kael,game.Map.SafeSpawn(game.Map.Arenas[3]),false);yield return null;
        }
        [UnityTest]public IEnumerator FirstAwakeningMusicSelectsProvidedTrackOnceWithoutPlayingDuringSilentTests()
        {
            var source=Get<AudioSource>(game,"music");var menuTrack=game.MenuMusicSource.clip;
            Assert.That(source.isPlaying,Is.False);
            game.ProtectAwakening(90);
            Assert.That(game.Progress.awakeningState,Is.EqualTo(1));
            Assert.That(source.clip.name,Is.EqualTo("FirstVampireAwakening"));
            Assert.That(source.clip.loadType,Is.EqualTo(AudioClipLoadType.Streaming));
            Assert.That(source.loop,Is.False);Assert.That(source.spatialBlend,Is.Zero);
            Assert.That(source.volume,Is.EqualTo(game.MusicVolume));Assert.That(source.isPlaying,Is.False);
            Assert.That(game.MenuMusicSource.clip,Is.SameAs(menuTrack));
            source.clip=null;game.ProtectAwakening(99);
            Assert.That(source.clip,Is.Null,"A completed awakening was triggered a second time");
            Invoke(game,"LoadMap",0);
            Assert.That(game.Progress.awakeningState,Is.EqualTo(1));Assert.That(source.clip,Is.Null);
            yield return null;
        }
        [UnityTest]public IEnumerator AwakeningMusicPausesAndResumesPositionAndDoesNotOverlapMainMenu()
        {
            game.ProtectAwakening(90);var source=Get<AudioSource>(game,"music");float previousListenerVolume=AudioListener.volume;
            try
            {
                // Exercise the same source and menu lifecycle without emitting sound or saving the campaign.
                AudioListener.volume=0;source.mute=true;DarklightAutomation.Silent=false;source.Play();
                float until=Time.realtimeSinceStartup+3;
                while(source.time<=.03f&&Time.realtimeSinceStartup<until)yield return null;
                Assert.That(source.isPlaying,Is.True);Assert.That(source.time,Is.GreaterThan(.03f));
                Invoke(game,"Open","Pause");float paused=source.time;
                yield return new WaitForSecondsRealtime(.1f);
                Assert.That(source.isPlaying,Is.False);Assert.That(source.time,Is.EqualTo(paused).Within(.02f));
                Invoke(game,"Close");yield return new WaitForSecondsRealtime(.1f);
                Assert.That(source.isPlaying,Is.True);Assert.That(source.time,Is.GreaterThan(paused));
                Invoke(game,"Open","Main");yield return null;
                Assert.That(source.isPlaying,Is.False);Assert.That(game.MenuMusicSource.clip,Is.Not.SameAs(source.clip));
            }
            finally
            {
                DarklightAutomation.Silent=true;source.Stop();game.AudioDirector?.StopAll();game.MenuMusicSource.Stop();AudioListener.volume=previousListenerVolume;
            }
        }
    }
}
