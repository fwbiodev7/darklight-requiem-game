using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        const string FirstAwakeningTrackPath="Darklight/Music/FirstVampireAwakening";
        AudioClip firstAwakeningTrack;
        bool firstAwakeningMusicPending;
        bool HasFirstAwakeningMusic=>firstAwakeningTrack!=null&&music.clip==firstAwakeningTrack;
        void ScheduleFirstAwakeningMusic()
        {
            firstAwakeningTrack??=Resources.Load<AudioClip>(FirstAwakeningTrackPath);
            firstAwakeningMusicPending=firstAwakeningTrack!=null;
            TryStartFirstAwakeningMusic();
        }
        void TryStartFirstAwakeningMusic()
        {
            if(!firstAwakeningMusicPending||!Active||Resonance.Active>=0)return;
            firstAwakeningMusicPending=false;
            StopMenuMusic();music.Stop();music.clip=firstAwakeningTrack;
            music.loop=false;music.volume=MusicVolume;
            if(!DarklightAutomation.Silent)music.Play();
        }
    }
}
