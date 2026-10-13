using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightSkinMusicTests
    {
        DarklightCampaign game;TodaySkinMusic music;
        static void Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
        bool hadSkin;int savedSkin;
        [UnitySetUp] public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            hadSkin=PlayerPrefs.HasKey("Darklight.Skin");savedSkin=PlayerPrefs.GetInt("Darklight.Skin",0);
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;game.PreviewChapter(0);music=game.SkinMusic;music.enabled=false;
        }
        [UnityTearDown] public IEnumerator Restore()
        {
            DarklightAutomation.Silent=true;if(hadSkin)PlayerPrefs.SetInt("Darklight.Skin",savedSkin);else PlayerPrefs.DeleteKey("Darklight.Skin");PlayerPrefs.Save();
            if(game!=null){game.AudioDirector.StopAll();game.MenuMusicSource.Stop();game.EventMusicSource.Stop();}
            yield return null;
        }
        static void AssertSilent()
        {
            Assert.That(DarklightAutomation.Silent,Is.True);
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False,source.name);
        }
        [UnityTest] public IEnumerator SkinSelectionCrossfadesProductionWeightsWithoutStartingAnyClip()
        {
            game.SelectSkin(0,false);music.EvaluateMix(1);Assert.That(music.RequiemWeight,Is.EqualTo(1));Assert.That(music.RockWeight,Is.Zero);
            Assert.That(music.RequiemSource.clip.name,Is.EqualTo("RequiemTheme"));Assert.That(music.RockSource.clip.name,Is.EqualTo("RockTheme"));
            Assert.That(music.RequiemSource.loop&&music.RockSource.loop,Is.True);
            game.SelectSkin(1,false);music.EvaluateMix(TodaySkinMusic.TransitionSeconds*.5f);
            Assert.That(music.RockWeight,Is.EqualTo(.5f).Within(.001f));Assert.That(music.RequiemWeight,Is.EqualTo(.5f).Within(.001f));
            music.EvaluateMix(TodaySkinMusic.TransitionSeconds*.5f);Assert.That(music.RockWeight,Is.EqualTo(1));Assert.That(music.RequiemWeight,Is.Zero);AssertSilent();yield return null;
        }
        [UnityTest] public IEnumerator ResonanceTakesPriorityAndCancellationRestoresTheSameSkinSource()
        {
            game.SelectSkin(1,false);music.EvaluateMix(1);var source=music.RockSource;
            Invoke(game,"BeginResonance");Assert.That(game.Resonance.Active,Is.GreaterThanOrEqualTo(0));music.EvaluateMix(1);
            Assert.That(music.EventWeight,Is.EqualTo(1));Assert.That(music.RockWeight,Is.Zero);Assert.That(music.RequiemWeight,Is.Zero);
            var track=game.EventMusicSource.clip;Assert.That(track,Is.Not.Null);Assert.That(track,Is.Not.SameAs(source.clip));
            Invoke(game,"StopPower");music.EvaluateMix(TodaySkinMusic.TransitionSeconds*.5f);
            Assert.That(music.EventWeight,Is.EqualTo(.5f).Within(.001f));Assert.That(music.RockWeight,Is.EqualTo(.5f).Within(.001f));
            music.EvaluateMix(TodaySkinMusic.TransitionSeconds*.5f);Assert.That(music.EventWeight,Is.Zero);Assert.That(music.RockWeight,Is.EqualTo(1));Assert.That(music.RockSource,Is.SameAs(source));AssertSilent();yield return null;
        }
        [UnityTest] public IEnumerator PauseMapsCheckpointsAndExplorationKeepTheMusicSourcesAndVoicePool()
        {
            game.SelectSkin(1,false);music.EvaluateMix(1);var rock=music.RockSource;var requiem=music.RequiemSource;
            int before=Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length;
            Invoke(game,"Open","Pause");music.EvaluateMix(5);Assert.That(music.RockWeight,Is.EqualTo(1),"Menu must freeze fade progress");
            Invoke(game,"Close");Invoke(game,"LoadMap",1);music.EvaluateMix(0);
            Assert.That(music.RockSource,Is.SameAs(rock));Assert.That(music.RequiemSource,Is.SameAs(requiem));
            game.ExploreRegion(6);yield return null;game.ExploreRegion(9);yield return null;
            Assert.That(music.RockSource,Is.SameAs(rock));Assert.That(game.AudioDirector.AllocatedVoices,Is.EqualTo(16));
            Assert.That(Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length,Is.EqualTo(before));Assert.That(game.AudioDirector.ActiveVoices,Is.Zero);AssertSilent();
        }
        [UnityTest] public IEnumerator SavedSkinReloadAndEveryVolumeCategoryPreserveSilentRouting()
        {
            PlayerPrefs.SetInt("Darklight.Skin",1);
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;game.PreviewChapter(0);music=game.SkinMusic;music.enabled=false;music.EvaluateMix(1);
            Assert.That(game.SelectedSkin,Is.EqualTo(1));Assert.That(music.RockWeight,Is.EqualTo(1));
            game.EffectsVolume=.5f;game.PlayerVolume=.2f;game.EnemyVolume=.8f;game.InterfaceVolume=.35f;game.AmbientVolume=.45f;game.MusicVolume=.37f;
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Character),Is.EqualTo(.1f).Within(.001f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Enemy),Is.EqualTo(.4f).Within(.001f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Combat),Is.EqualTo(.5f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Interface),Is.EqualTo(.35f));
            Assert.That(game.CategoryVolume(DarklightAudioCategory.Ambient),Is.EqualTo(.45f));
            foreach(TodayFootstepSurface surface in System.Enum.GetValues(typeof(TodayFootstepSurface)))Assert.That(game.AudioDirector.SampleCount(TodaySurfaces.Prefix(surface)),Is.GreaterThanOrEqualTo(3));
            Assert.That(game.AudioDirector.Play("dl-heartbeat",DarklightAudioCategory.Character),Is.False);music.EvaluateMix(1);Assert.That(music.RockSource.volume,Is.Zero);AssertSilent();
        }
    }
}