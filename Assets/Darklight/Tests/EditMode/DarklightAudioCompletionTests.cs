using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightAudioCompletionTests
    {
        [Test] public void HeadphoneEquipCompletionIsQuietButRemovalAndInterruptionNotifyOnce()
        {
            var audio=new TodayPowerAudioTransitions();
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
            Assert.That(audio.Tick(true,0,false,-1,true),Is.EqualTo(TodayPowerAudioEvent.None));
            Assert.That(audio.Tick(true,0,false,0,false),Is.EqualTo(TodayPowerAudioEvent.None),"Equip animation completed normally");
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.ResonanceEnded|TodayPowerAudioEvent.HeadphonesRemoved));
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
            audio.Tick(true,0,false,-1,true);
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.HeadphonesRemoved),"Interrupted before activation");
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
        }
        [Test] public void FormDissipationAndFrenzyExpiryHaveIndependentSingleEdges()
        {
            var audio=new TodayPowerAudioTransitions();audio.Tick(true,2,true,-1,false);
            Assert.That(audio.Tick(true,2,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.FrenzyEnded));
            Assert.That(audio.Tick(true,2,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.FormEnded));
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
        }
        [Test] public void PauseDeathAndMapBaselinesDoNotReplayOldPowerEndsOnResume()
        {
            var audio=new TodayPowerAudioTransitions();audio.Tick(true,3,true,1,false);
            Assert.That(audio.Tick(false,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None));
            audio.Tick(true,1,true,0,false);audio.Reset();
            Assert.That(audio.Tick(true,0,false,-1,false),Is.EqualTo(TodayPowerAudioEvent.None),"Map/model/death resets discard old powers");
        }
        [Test] public void RealBloodRecoveryWaitsForResourcesAndNotifiesOnlyOnce()
        {
            var audio=new TodayCooldownAudio();var blood=new DarklightBlood();
            var progress=new DarklightProgress{vampireMastery=3,vampireSkills=63};
            var resonance=new float[3];const int selected=(int)VampireAbility.Frenzy;
            Assert.That(audio.Tick(true,blood.Recovery,selected,true,resonance,0,false),Is.False);
            blood.Set(50);Assert.That(blood.Activate(selected,progress),Is.True);
            Assert.That(audio.Tick(true,blood.Recovery,selected,false,resonance,0,false),Is.False);
            blood.Tick(DarklightVampireSkills.Cooldowns[selected]+1);
            Assert.That(blood.Recovery[selected],Is.Zero);Assert.That(blood.Amount,Is.LessThan(DarklightVampireSkills.Costs[selected]));
            Assert.That(audio.Tick(true,blood.Recovery,selected,false,resonance,0,false),Is.False,"Recovery ended but insufficient blood");
            blood.Gain(100);
            Assert.That(audio.Tick(true,blood.Recovery,selected,true,resonance,0,false),Is.True);
            Assert.That(audio.Tick(true,blood.Recovery,selected,true,resonance,0,false),Is.False);
        }
        [Test] public void RealResonanceDurationDoesNotCountAsCooldownCompletion()
        {
            var audio=new TodayCooldownAudio();var resonance=new DarklightResonance();var blood=new float[6];
            audio.Tick(true,blood,0,false,resonance.Recovery,0,resonance.Ready);
            Assert.That(resonance.Activate(),Is.True);
            Assert.That(audio.Tick(true,blood,0,false,resonance.Recovery,0,resonance.Ready),Is.False);
            resonance.Tick(DarklightResonance.Durations[0]+.1f);
            Assert.That(resonance.Active,Is.EqualTo(-1));Assert.That(resonance.Recovery[0],Is.GreaterThan(0));
            Assert.That(audio.Tick(true,blood,0,false,resonance.Recovery,0,resonance.Ready),Is.False);
            resonance.Tick(DarklightResonance.Cooldowns[0]+.1f);
            Assert.That(resonance.Ready,Is.True);
            Assert.That(audio.Tick(true,blood,0,false,resonance.Recovery,0,resonance.Ready),Is.True);
            Assert.That(audio.Tick(true,blood,0,false,resonance.Recovery,0,resonance.Ready),Is.False);
        }
        [Test] public void SelectionAndLoadCannotManufactureCooldownNotifications()
        {
            var audio=new TodayCooldownAudio();var blood=new float[6];var resonance=new float[3];blood[1]=2;
            audio.Tick(true,blood,0,true,resonance,0,true);
            blood[1]=0;
            Assert.That(audio.Tick(true,blood,0,true,resonance,0,true),Is.False,"Unselected completion stays quiet");
            Assert.That(audio.Tick(true,blood,1,true,resonance,0,true),Is.False,"Selecting a ready ability is not a completion");
            blood[1]=1;audio.Tick(true,blood,1,false,resonance,0,true);blood[1]=0;
            Assert.That(audio.Tick(false,blood,1,true,resonance,0,true),Is.False,"Menu change sets baseline");
            Assert.That(audio.Tick(true,blood,1,true,resonance,0,true),Is.False);
            blood[1]=1;audio.Tick(true,blood,1,false,resonance,0,true);audio.Reset();blood[1]=0;
            Assert.That(audio.Tick(true,blood,1,true,resonance,0,true),Is.False,"Loaded zero is not a new edge");
        }
        [Test] public void SimultaneousCompletionsCoalesceWithoutRepeatedCue()
        {
            var audio=new TodayCooldownAudio();var blood=new float[6];var resonance=new float[3];blood[2]=1;resonance[1]=1;
            audio.Tick(true,blood,2,false,resonance,1,false);blood[2]=0;resonance[1]=0;
            Assert.That(audio.Tick(true,blood,2,true,resonance,1,true),Is.True);
            Assert.That(audio.Tick(true,blood,2,true,resonance,1,true),Is.False);
        }
        [Test] public void ExistingCompletionBanksExistAndRemainBlockedBySilentAutomation()
        {
            foreach(var prefix in new[]{DarklightSoundDirector.WeaponStowFallback,DarklightSoundDirector.HeadphonesRemoveFallback,
                DarklightSoundDirector.PowerEndFallback,DarklightSoundDirector.CooldownReadyFallback})
            {
                bool found=false;foreach(var clip in Resources.LoadAll<AudioClip>("Darklight/Audio"))if(clip.name.StartsWith(prefix,System.StringComparison.Ordinal))found=true;
                Assert.That(found,Is.True,prefix);
                Assert.That(prefix,Is.Not.EqualTo(DarklightSoundDirector.DeliveredPrefix("PW11")),"Ready cue must differ from denied action");
            }
            bool previous=DarklightAutomation.Silent;DarklightAutomation.Silent=true;var go=new GameObject("CompletionSilentGuard");
            try
            {
                var director=go.AddComponent<DarklightSoundDirector>();
                director.StowWeapon();director.RemoveHeadphones();director.PowerEnded();director.CooldownReady();
                Assert.That(go.GetComponentsInChildren<AudioSource>().Length,Is.Zero);Assert.That(director.ActiveVoices,Is.Zero);
            }
            finally{Object.DestroyImmediate(go);DarklightAutomation.Silent=previous;}
        }
    }
}
