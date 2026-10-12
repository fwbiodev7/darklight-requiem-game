using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public enum DarklightAudioCategory { Combat, Character, Interface }
    public sealed class DarklightSoundDirector : MonoBehaviour
    {
        const int VoiceLimit=16;
        sealed class Voice {public AudioSource source;public DarklightAudioCategory category;public float gain,attenuation,began;}
        readonly Voice[] voices=new Voice[VoiceLimit];
        readonly Dictionary<string,AudioClip[]> clips=new();readonly Dictionary<string,int> variants=new();readonly Dictionary<string,float> ready=new();
        AudioClip[] allClips;
        DarklightCampaign game;AudioSource nature,dungeon;float ambientBlend,ambientTarget,duck;int swingSerial=-1;
        public int AllocatedVoices=>voices.Length;
        public int ActiveVoices {get{int n=0;foreach(var v in voices)if(v?.source!=null&&v.source.isPlaying)n++;return n;}}
        public void Initialize(DarklightCampaign owner)
        {
            if(game!=null)return;game=owner;allClips=Resources.LoadAll<AudioClip>("Darklight/Audio");
            for(int i=0;i<voices.Length;i++){var src=new GameObject("AudioVoice"+i).AddComponent<AudioSource>();src.transform.SetParent(transform,false);src.playOnAwake=false;src.spatialBlend=0;src.loop=false;src.priority=128;voices[i]=new Voice{source=src};}
            nature=Loop("Exterior",Resources.Load<AudioClip>("Darklight/Audio/park_ambience_wind"));dungeon=Loop("Veu",Resources.Load<AudioClip>("Darklight/Audio/dungeon_ambient_1"));
        }
        AudioSource Loop(string name,AudioClip clip){var src=new GameObject(name).AddComponent<AudioSource>();src.transform.SetParent(transform,false);src.clip=clip;src.playOnAwake=false;src.loop=true;src.spatialBlend=0;src.volume=0;return src;}
        AudioClip[] Samples(string prefix)
        {
            if(clips.TryGetValue(prefix,out var list))return list;
            var all=allClips;var selected=new List<AudioClip>();foreach(var c in all)if(c.name.StartsWith(prefix))selected.Add(c);selected.Sort((a,b)=>string.CompareOrdinal(a.name,b.name));return clips[prefix]=selected.ToArray();
        }
        public bool Play(string prefix,DarklightAudioCategory category,float gain=.35f,Vector2? position=null,float pitch=1,float interval=.04f)
        {
            if(game==null||DarklightAutomation.Silent||category!=DarklightAudioCategory.Interface&&!game.Active)return false;
            float now=Time.unscaledTime;string key=prefix+category;if(ready.TryGetValue(key,out float until)&&now<until)return false;
            var samples=Samples(prefix);if(samples.Length==0)return false;
            Voice voice=null;foreach(var v in voices)if(!v.source.isPlaying){voice=v;break;}
            if(voice==null)return false;ready[key]=now+interval;
            int index=variants.TryGetValue(key,out int last)?(last+1)%samples.Length:0;variants[key]=index;
            voice.category=category;voice.gain=Mathf.Clamp01(gain);voice.attenuation=1;voice.began=now;
            if(position.HasValue){Vector2 delta=position.Value-(Vector2)DarklightCombatLab.Instance.Player.transform.position;voice.source.panStereo=Mathf.Clamp(delta.x/12,-.65f,.65f);voice.attenuation=1/(1+delta.sqrMagnitude/36);}
            else voice.source.panStereo=0;
            voice.source.clip=samples[index];voice.source.pitch=Mathf.Clamp(pitch+Random.Range(-.035f,.035f),.7f,1.35f);voice.source.volume=Gain(voice);voice.source.Play();return true;
        }
        float Gain(Voice voice)=>Mathf.Min(.65f,voice.gain*voice.attenuation*(voice.category==DarklightAudioCategory.Interface?game.InterfaceVolume:game.EffectsVolume));
        public void Swing(AttackDefinition attack,int serial,int weapon)
        {
            if(swingSerial==serial)return;swingSerial=serial;
            Play(weapon==2?"rpg-metalLatch":weapon==1?"rpg-drawKnife":"rpg-knifeSlice",DarklightAudioCategory.Combat,attack.kind==StrikeKind.Light?.28f:.4f,null,weapon==1?1.16f:.96f);
        }
        public void Contact(DarklightCombatant target,AttackDefinition attack)
        {
            var enemy=target.GetComponent<DarklightEnemy>();bool metal=enemy!=null&&(enemy.kind==RequiemEnemyKind.Armored||enemy.kind==RequiemEnemyKind.Sentinel);
            Play(metal?attack.kind==StrikeKind.Light?"impact-impactMetal_light":"impact-impactMetal_heavy":attack.kind==StrikeKind.Light?"impact-impactPunch_medium":"impact-impactPunch_heavy",DarklightAudioCategory.Combat,.42f,target.transform.position);
        }
        public void Footstep(bool wood,bool grass,Vector2 position)=>Play(wood?"impact-footstep_wood":grass?"impact-footstep_grass":"impact-footstep_concrete",DarklightAudioCategory.Character,.16f,position,1,.16f);
        public void Region(int chapter){ambientTarget=chapter==2||chapter==3||chapter==6||chapter==8||chapter==9?1:0;swingSerial=-1;StopCombat();}
        public void StopCombat(){foreach(var voice in voices)if(voice?.source!=null&&voice.category!=DarklightAudioCategory.Interface)voice.source.Stop();}
        public void Awakening(){duck=2;Play("impact-impactPunch_heavy",DarklightAudioCategory.Character,.3f,null,.7f,.3f);}
        public void EnemyWarning(bool boss,Vector2 position)=>Play(boss?"rpg-metalLatch":"rpg-drawKnife",DarklightAudioCategory.Character,boss?.3f:.19f,position,boss?.8f:1,.16f);
        void Update()
        {
            if(game==null)return;if(DarklightAutomation.Silent){StopAll();return;}foreach(var voice in voices)if(voice?.source!=null&&voice.source.isPlaying){if(!game.Active&&voice.category!=DarklightAudioCategory.Interface)voice.source.Stop();else voice.source.volume=Gain(voice);}
            if(nature==null||dungeon==null)return;
            if(DarklightAutomation.Silent){nature.Stop();dungeon.Stop();return;}
            if(!game.Active){nature.Pause();dungeon.Pause();return;}
            if(!nature.isPlaying){nature.UnPause();if(!nature.isPlaying&&nature.clip!=null)nature.Play();}
            if(!dungeon.isPlaying){dungeon.UnPause();if(!dungeon.isPlaying&&dungeon.clip!=null)dungeon.Play();}
            float dt=DarklightCombatLab.Instance.CombatDelta;duck=Mathf.Max(0,duck-dt);ambientBlend=Mathf.MoveTowards(ambientBlend,ambientTarget,dt*.8f);
            float volume=game.AmbientVolume*.42f*(duck>0?.2f:game.Resonance.Active>=0?.35f:1);
            nature.volume=volume*(1-ambientBlend);dungeon.volume=volume*ambientBlend;
        }
        public void StopAll(){foreach(var v in voices)if(v?.source!=null)v.source.Stop();if(nature!=null)nature.Stop();if(dungeon!=null)dungeon.Stop();}
        void OnDisable()=>StopAll();
    }
}
