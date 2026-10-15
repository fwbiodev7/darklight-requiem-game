using UnityEngine;
namespace Darklight
{
    // Every form uses its own creator-supplied theme on every activation.
    // Story awakening and Dark Resonance retain priority over this looping theme.
    public sealed class TodayTransformationMusic : MonoBehaviour
    {
        DarklightCampaign game;
        AudioSource source;
        AudioClip[] themes;
        float weight;
        public bool HasTheme => game != null && game.Form >= 1 && game.Form <= 3 && themes != null && themes[game.Form-1] != null;
        public static string ResourceForForm(int form)=>form==1?"FirstVampireAwakening":form==2?"VioletTransformation":form==3?"WhiteTransformation":null;
        public AudioSource Source => source;
        public float Weight => weight;
        void Awake()
        {
            game=GetComponent<DarklightCampaign>();
            source=new GameObject("TrilhaTransformacao").AddComponent<AudioSource>();
            source.transform.SetParent(transform,false);
            themes=new AudioClip[3];
            for(int form=1;form<=3;form++)themes[form-1]=Resources.Load<AudioClip>("Darklight/Music/"+ResourceForForm(form));
            source.clip=themes[0];
            source.playOnAwake=false;source.loop=true;source.spatialBlend=0;
            source.priority=40;source.volume=0;
        }
        void LateUpdate()=>EvaluateMix(Time.unscaledDeltaTime);
        public void EvaluateMix(float dt)
        {
            if(game==null||source==null)return;
            if(!game.Active){source.Pause();return;}
            bool selected=HasTheme;
            if(selected&&source.clip!=themes[game.Form-1])
            {source.Stop();source.clip=themes[game.Form-1];weight=0;}
            bool available=selected&&game.Resonance.Active<0&&!game.EventMusicHasPriority&&!game.WearingHeadphones;
            weight=TodaySkinMusic.Fade(weight,available?1:0,dt);
            source.volume=DarklightAutomation.Silent?0:game.MusicVolume*weight;
            if(DarklightAutomation.Silent){source.Pause();return;}
            if(weight>0)
            {
                source.UnPause();if(!source.isPlaying)source.Play();
            }
            else if(selected)source.Pause();
            else source.Stop();
        }
        void OnDisable(){if(source!=null)source.Stop();weight=0;}
    }
}
