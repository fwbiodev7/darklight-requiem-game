using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public float MasterVolume=1,EnemyVolume=1,PlayerVolume=1;
        public static int VolumePercent(float value)=>float.IsNaN(value)||float.IsInfinity(value)?0:Mathf.RoundToInt(Mathf.Clamp01(value)*100);
        void InitializeAudioPreferences()
        {
            MasterVolume=VolumePercent(PlayerPrefs.GetFloat("Darklight.Master",1))/100f;
            EnemyVolume=VolumePercent(PlayerPrefs.GetFloat("Darklight.Enemies",1))/100f;
            PlayerVolume=VolumePercent(PlayerPrefs.GetFloat("Darklight.Player",1))/100f;
            MusicVolume=VolumePercent(MusicVolume)/100f;EffectsVolume=VolumePercent(EffectsVolume)/100f;
            AmbientVolume=VolumePercent(AmbientVolume)/100f;InterfaceVolume=VolumePercent(InterfaceVolume)/100f;
            AudioListener.volume=DarklightAutomation.Silent?0:MasterVolume;
        }
        public float CategoryVolume(DarklightAudioCategory category)=>category==DarklightAudioCategory.Interface?InterfaceVolume:category==DarklightAudioCategory.Ambient?AmbientVolume:
            EffectsVolume*(category==DarklightAudioCategory.Character?PlayerVolume:category==DarklightAudioCategory.Enemy?EnemyVolume:1);
    }
}
