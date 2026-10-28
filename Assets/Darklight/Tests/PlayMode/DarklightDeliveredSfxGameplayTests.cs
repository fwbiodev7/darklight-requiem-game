using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    // These checks load the real campaign and exercise its audio entry points.
    // Silent is never disabled: they do not certify audible content or mix quality.
    public sealed class DarklightDeliveredSfxGameplayTests
    {
        static readonly string[] FloatKeys={"Master","Music","Effects","Ambient","UI","Enemies","Player"};
        static readonly string[] IntKeys={"VisualQuality","Flashes","FrameRate","Fullscreen","Height","Lessons","Shake","Skin","Tips","VSync","Width"};
        readonly Dictionary<string,(bool exists,float value)> floats=new();
        readonly Dictionary<string,(bool exists,int value)> integers=new();
        IDisposable storage;DarklightCampaign game;DarklightCombatLab lab;Scene loaded;
        bool previousSilent,previousPause,previousBackground,bindingsExist;float previousVolume;
        string bindings,preferencesBefore,savesBefore,isolatedPath,isolatedBefore;
        AudioSource[] sourcePool;
        static object Call(object target,string name,params object[] args)
            =>target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        static T Field<T>(object target,string name)
            =>(T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        static string HashFile(string path)
        {
            using var hash=SHA256.Create();return Convert.ToBase64String(hash.ComputeHash(File.ReadAllBytes(path)));
        }
        static string Saves()
            =>Directory.Exists(Application.persistentDataPath)?string.Join("|",Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories)
                .OrderBy(p=>p,StringComparer.Ordinal).Select(p=>p+":"+HashFile(p))):"";
        static string Preferences()
        {
            var rows=new List<string>();
            foreach(var key in FloatKeys)rows.Add(key+":"+PlayerPrefs.HasKey("Darklight."+key)+":"+PlayerPrefs.GetFloat("Darklight."+key).ToString("R",System.Globalization.CultureInfo.InvariantCulture));
            foreach(var key in IntKeys)rows.Add(key+":"+PlayerPrefs.HasKey("Darklight."+key)+":"+PlayerPrefs.GetInt("Darklight."+key));
            rows.Add("Bindings:"+PlayerPrefs.HasKey("Darklight.Bindings")+":"+PlayerPrefs.GetString("Darklight.Bindings"));
            return string.Join("|",rows);
        }
        [UnitySetUp] public IEnumerator SetUp()
        {
            previousSilent=DarklightAutomation.Silent;previousVolume=AudioListener.volume;previousPause=AudioListener.pause;
            previousBackground=Application.runInBackground;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;Application.runInBackground=true;
            foreach(var key in FloatKeys)floats[key]=(PlayerPrefs.HasKey("Darklight."+key),PlayerPrefs.GetFloat("Darklight."+key));
            foreach(var key in IntKeys)integers[key]=(PlayerPrefs.HasKey("Darklight."+key),PlayerPrefs.GetInt("Darklight."+key));
            bindingsExist=PlayerPrefs.HasKey("Darklight.Bindings");bindings=PlayerPrefs.GetString("Darklight.Bindings");
            preferencesBefore=Preferences();savesBefore=Saves();
            isolatedPath=Path.GetFullPath("work/audio-delivery-play-20261023/"+Guid.NewGuid().ToString("N")+".json");
            storage=DarklightProgress.IsolateStorageForTests(isolatedPath);
            new DarklightProgress().Save();isolatedBefore=HashFile(isolatedPath);
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            loaded=SceneManager.GetSceneByPath("Assets/Darklight/Scenes/DarklightCampaign.unity");
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;
            Assert.That(game,Is.Not.Null);Assert.That(lab?.Ready,Is.True);
            Assert.That(DarklightProgress.Path,Is.EqualTo(isolatedPath));Assert.That(DarklightProgress.HasIsolatedTestStorage,Is.True);
            Assert.That(Resources.LoadAll<AudioClip>("Darklight/Audio").Length,Is.EqualTo(151));
            Assert.That(Resources.LoadAll<AudioClip>("Darklight/Audio/Delivery20261023").Length,Is.EqualTo(19));
            sourcePool=game.GetComponentsInChildren<AudioSource>(true);
            Assert.That(game.AudioDirector.AllocatedVoices,Is.EqualTo(16));
            Assert.That(game.GetComponentsInChildren<AudioSource>(true).Count(s=>s.name.StartsWith("AudioVoice",StringComparison.Ordinal)),Is.EqualTo(16));
            AssertSilentAndPreserved();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            try
            {
                // Unload the live campaign before restoring the caller's listener
                // and Silent settings, even if the test failed mid-battle.
                if(game!=null){game.AudioDirector?.StopAll();game.gameObject.SetActive(false);}
                foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))source.Stop();
                if(loaded.IsValid()&&loaded.isLoaded)
                {
                    var empty=SceneManager.CreateScene("DeliveredSfxCleanup_"+Guid.NewGuid().ToString("N"));SceneManager.SetActiveScene(empty);
                    yield return SceneManager.UnloadSceneAsync(loaded);
                }
                Assert.That(Preferences(),Is.EqualTo(preferencesBefore),"Audio events changed stored preferences");
                Assert.That(Saves(),Is.EqualTo(savesBefore),"Real player saves changed despite isolated storage");
                if(isolatedBefore!=null)Assert.That(HashFile(isolatedPath),Is.EqualTo(isolatedBefore),"Silent audio events wrote the seeded test checkpoint");
            }
            finally
            {
                // Restore only if a regression changed a preference, keeping the
                // real user's values while preserving the failed assertion above.
                if(preferencesBefore!=null&&Preferences()!=preferencesBefore)
                {
                    foreach(var entry in floats){string key="Darklight."+entry.Key;if(entry.Value.exists)PlayerPrefs.SetFloat(key,entry.Value.value);else PlayerPrefs.DeleteKey(key);}
                    foreach(var entry in integers){string key="Darklight."+entry.Key;if(entry.Value.exists)PlayerPrefs.SetInt(key,entry.Value.value);else PlayerPrefs.DeleteKey(key);}
                    if(bindingsExist)PlayerPrefs.SetString("Darklight.Bindings",bindings);else PlayerPrefs.DeleteKey("Darklight.Bindings");PlayerPrefs.Save();
                }
                storage?.Dispose();Application.runInBackground=previousBackground;
                AudioListener.pause=previousPause;AudioListener.volume=previousVolume;DarklightAutomation.Silent=previousSilent;
            }
        }
        void AssertSilentAndPreserved()
        {
            Assert.That(DarklightAutomation.Silent,Is.True);Assert.That(AudioListener.pause,Is.True);Assert.That(AudioListener.volume,Is.Zero);
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                Assert.That(source.isPlaying,Is.False,source.name);
            Assert.That(game.AudioDirector.ActiveVoices,Is.Zero);
            CollectionAssert.AreEquivalent(sourcePool,game.GetComponentsInChildren<AudioSource>(true),"Audio source count/identities changed");
            Assert.That(Preferences(),Is.EqualTo(preferencesBefore));Assert.That(Saves(),Is.EqualTo(savesBefore));
            Assert.That(HashFile(isolatedPath),Is.EqualTo(isolatedBefore));
        }
        [UnityTest] public IEnumerator DeliveredEffectsLoadAndRealPotionPainPowerHooksStaySilentWithoutExtraSources()
        {
            game.PreviewChapter(0);yield return null;yield return null;
            foreach(var id in DarklightSoundDirector.DeliveredIds)
            {
                Assert.That(game.AudioDirector.SampleCount(DarklightSoundDirector.DeliveredPrefix(id)),Is.EqualTo(1),id);
                Assert.That(game.AudioDirector.Delivered(id,DarklightAudioCategory.Character,.3f,.1f,true),Is.False,id);
            }
            var fighter=lab.Player;var health=fighter.GetComponent<HealthSystem>();
            health.SetInvincibilityDuration(0);
            Assert.That(fighter.GetComponent<DarklightCombatant>().Receive(20,Vector2.left,true),Is.True,"Real damage must invoke the campaign pain hook");
            float damaged=health.CurrentHealth;int potions=game.Progress.potions;
            Call(game,"UsePotion");Assert.That(game.Progress.potions,Is.EqualTo(potions-1));Assert.That(health.CurrentHealth,Is.GreaterThan(damaged));
            game.Progress.potions=0;Call(game,"UsePotion");Assert.That(game.Progress.potions,Is.Zero,"Denied action must not consume a resource");
            fighter.ResetModel();game.Progress.vampireMastery=3;game.Progress.vampireSkills=63;game.Progress.vampireSelected=2;game.Blood.Set(0);
            Assert.That(game.TryVampireSkill(),Is.False,"Real refusal hook");
            foreach(int ability in new[]{2,3,4,5})
            {
                fighter.ResetModel();game.Progress.vampireSelected=ability;game.Blood.Set(100);game.Blood.Recovery[ability]=0;
                Assert.That(game.TryVampireSkill(),Is.True,DarklightVampireSkills.Names[ability]);
                Assert.That(game.Blood.Recovery[ability],Is.GreaterThan(0));
                // Advance the real campaign cast timer across its release point;
                // the loaded observer and director remain active in PlayMode.
                Call(game,"TickVampire",.2f);yield return null;yield return null;
                Call(game,"TickVampire",1f);Assert.That(game.VampireCasting,Is.False);
                AssertSilentAndPreserved();
            }
            fighter.ResetModel();game.Progress.mp3Unlocked=true;game.Resonance.Restore(game.Progress);
            Assert.That(game.TryActivatePower(),Is.True);Assert.That(game.WearingHeadphones,Is.True,"Actual headphones-start hook");
            float until=Time.realtimeSinceStartup+2;
            while(game.Resonance.Active<0&&Time.realtimeSinceStartup<until)yield return null;
            Assert.That(game.Resonance.Active,Is.GreaterThanOrEqualTo(0));Assert.That(game.EventMusicSource.clip,Is.Not.Null);
            Assert.That(game.TryActivatePower(),Is.False,"An already running power must be denied without another voice");
            Call(game,"StopPower");yield return null;Assert.That(game.Resonance.Active,Is.EqualTo(-1));
            AssertSilentAndPreserved();
        }
        [UnityTest] public IEnumerator WeaponFormMapDeathAndReturnKeepTheSameSilentPoolAndPreserveStorage()
        {
            game.EnterTraining();yield return null;yield return null;
            var fighter=lab.Player;var target=game.MapEnemies[0].GetComponent<DarklightCombatant>();
            foreach(int weapon in new[]{0,2,3})
            {
                fighter.ResetModel();game.Weapons.Request(weapon);game.Weapons.BeforeTick(.2f,true);
                Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(weapon));
                var attack=game.Weapons.Equipped.loadout.lights[0];
                game.WeaponSwing(attack,100+weapon);game.WeaponHit(target,attack,100+weapon);
                yield return null;yield return null;AssertSilentAndPreserved();
            }
            foreach(int form in new[]{1,2,0})
            {
                Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",1f);
                Assert.That(game.PresentedForm,Is.EqualTo(form));
                game.AudioDirector.Transform(form);game.TransformationMusic.EvaluateMix(.2f);
                yield return null;Assert.That(game.TransformationMusic.HasTheme,Is.EqualTo(form>0));AssertSilentAndPreserved();
            }
            game.ExitTraining();
            foreach(int chapter in new[]{4,9,1})
            {
                game.PreviewChapter(chapter);yield return null;yield return null;
                Assert.That(game.MapRegion,Is.EqualTo(chapter));AssertSilentAndPreserved();
            }
            var health=fighter.GetComponent<HealthSystem>();health.SetInvincibilityDuration(0);
            Assert.That(fighter.GetComponent<DarklightCombatant>().Receive(health.MaxHealth*3,Vector2.right,true),Is.True);
            yield return null;yield return null;
            Assert.That(health.IsDead,Is.True);Assert.That(game.Active,Is.False);Assert.That(lab.Paused,Is.True);
            game.AudioDirector.PlayerDeath();AssertSilentAndPreserved();
            yield return new WaitForSecondsRealtime(DarklightDeathReversion.Duration+.2f);
            Assert.That(Field<string>(game,"panel"),Is.EqualTo("Death"));AssertSilentAndPreserved();
            game.ResumeLastReturnPoint();float until=Time.realtimeSinceStartup+4;
            while((health.IsDead||!game.Active)&&Time.realtimeSinceStartup<until)yield return null;
            Assert.That(health.IsDead,Is.False);Assert.That(game.Active,Is.True);Assert.That(game.MapRegion,Is.Zero,"Seeded isolated checkpoint should be loaded");
            AssertSilentAndPreserved();
        }
    }
}
