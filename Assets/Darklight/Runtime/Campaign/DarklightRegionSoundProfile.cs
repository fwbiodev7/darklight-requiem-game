using UnityEngine;
namespace Darklight
{
    public readonly struct DarklightRegionSoundProfile
    {
        public readonly float Wind,Veil,Regional;
        public DarklightRegionSoundProfile(float wind,float veil,float regional){Wind=wind;Veil=veil;Regional=regional;}
        static readonly DarklightRegionSoundProfile[] Profiles={
            new(.28f,.025f,.18f),new(.19f,.055f,.24f),new(.09f,.20f,.23f),new(.025f,.24f,.28f),new(.26f,.06f,.21f),
            new(.045f,.29f,.25f),new(.02f,.32f,.24f),new(.11f,.22f,.22f),new(.015f,.35f,.29f),new(.01f,.38f,.31f)};
        public static DarklightRegionSoundProfile ForChapter(int chapter)=>Profiles[Mathf.Clamp(chapter,0,Profiles.Length-1)];
    }
}
