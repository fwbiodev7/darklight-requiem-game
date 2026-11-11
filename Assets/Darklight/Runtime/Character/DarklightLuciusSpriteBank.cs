using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Appearance is independent of form: the hood does not add a transformation.
    public enum DarklightLuciusAppearance { Normal, Hooded }
    [Serializable] public sealed class DarklightLuciusSpritePage
    {
        public string id,action,attackKey,resourcePath,imageSha256,sourceSha256,status;
        public int direction,startupFrames,activeFrames;
        public int[] nativeSize,durationMs;
        public float pixelsPerUnit;
        public string[] spriteNames;
        public bool loop,wholeFigures;
        [NonSerialized] internal Sprite[] frames;
        [NonSerialized] internal bool attempted;
        [NonSerialized] internal DarklightWeaponRedesignPage clock;
    }
    [Serializable] public sealed class DarklightLuciusSpriteCatalog
    {
        public int schemaVersion;
        public string characterId,form,weapon,appearance;
        public bool complete;
        public DarklightLuciusSpritePage[] pages;
    }

    // A separate opt-in presentation bank. It never addresses Lucien's boss ID,
    // the player's skins, campaign registration or transformation state.
    public static class DarklightLuciusSpriteBank
    {
        public const string Root="Darklight/LuciusNormal20261109/";
        public const string HoodedRoot="Darklight/LuciusHooded20261110/";
        static readonly Dictionary<DarklightLuciusAppearance,DarklightLuciusSpriteCatalog> catalogs=new();
        static readonly HashSet<DarklightLuciusAppearance> attempted=new();
        static readonly Dictionary<string,Dictionary<string,Sprite>> sheets=new(StringComparer.Ordinal);
        static string ResourceRoot(DarklightLuciusAppearance appearance)=>appearance==DarklightLuciusAppearance.Normal?Root:
            appearance==DarklightLuciusAppearance.Hooded?HoodedRoot:null;
        static bool Name(string name)
        {
            if(string.IsNullOrEmpty(name))return false;
            foreach(char c in name)
                if(!(c>='A'&&c<='Z'||c>='a'&&c<='z'||c>='0'&&c<='9'||c=='_'||c=='-'))return false;
            return true;
        }
        static bool Action(string action)=>action=="Idle"||action=="Walk"||action=="Run"||
            action=="Attack"||action=="Charging"||action=="Dodge"||action=="Parry"||action=="Hurt"||action=="Dead"||
            action=="Draw"||action=="WalkArmed";
        static bool SwordKey(string key)=>key==DarklightLuciusCombatProfile.SwordHorizontal||
            key==DarklightLuciusCombatProfile.SwordVertical||key==DarklightLuciusCombatProfile.SwordThrust;
        public static bool ValidPage(DarklightLuciusSpritePage page)
            =>ValidPage(page,DarklightLuciusAppearance.Normal);
        public static bool ValidPage(DarklightLuciusSpritePage page,DarklightLuciusAppearance appearance)
        {
            string root=ResourceRoot(appearance);if(root==null)return false;
            if(page==null||!Name(page.id)||!page.id.StartsWith("Lucius",StringComparison.Ordinal)||!Action(page.action)||page.direction<0||page.direction>7||
                !page.wholeFigures||page.status!="visual-reviewed"&&page.status!="imported"&&page.status!="runtime-verified"||
                !DarklightWeaponRedesignBank.Sha256(page.imageSha256)||!DarklightWeaponRedesignBank.Sha256(page.sourceSha256)||
                page.nativeSize?.Length!=2||page.nativeSize[0]<=0||page.nativeSize[1]<=0||
                page.pixelsPerUnit<=0||float.IsNaN(page.pixelsPerUnit)||float.IsInfinity(page.pixelsPerUnit)||
                string.IsNullOrEmpty(page.resourcePath)||!page.resourcePath.StartsWith(root,StringComparison.Ordinal)||
                !Name(page.resourcePath.Substring(root.Length))||
                page.spriteNames==null||page.spriteNames.Length==0||page.durationMs?.Length!=page.spriteNames.Length)return false;
            // Attack identity is exact; other states cannot carry a technique key.
            if(page.action=="Attack"?!SwordKey(page.attackKey):!string.IsNullOrEmpty(page.attackKey))return false;
            if(page.startupFrames<0||page.activeFrames<0)return false;
            if(page.startupFrames!=0||page.activeFrames!=0)
                if(page.action!="Attack"||page.startupFrames<1||page.activeFrames<1||
                    page.startupFrames+page.activeFrames>=page.spriteNames.Length)return false;
            var names=new HashSet<string>(StringComparer.Ordinal);
            foreach(string name in page.spriteNames)
                if(!Name(name)||!name.StartsWith("Redesign_Lucius",StringComparison.Ordinal)||!names.Add(name))return false;
            foreach(int duration in page.durationMs)if(duration<=0)return false;
            if(page.loop&&page.action!="Idle"&&page.action!="Walk"&&page.action!="Run"&&page.action!="Charging"&&page.action!="WalkArmed")return false;
            return true;
        }
        public static bool ValidCatalog(DarklightLuciusSpriteCatalog data)
            =>ValidCatalog(data,DarklightLuciusAppearance.Normal);
        public static bool ValidCatalog(DarklightLuciusSpriteCatalog data,DarklightLuciusAppearance appearance)
        {
            if(ResourceRoot(appearance)==null)return false;
            if(data==null||data.schemaVersion!=1||data.characterId!="Lucius"||data.form!="Normal"||
                data.weapon!="Sword"||data.pages==null)return false;
            if(appearance==DarklightLuciusAppearance.Normal?!string.IsNullOrEmpty(data.appearance)&&data.appearance!="Normal":data.appearance!="Hooded")return false;
            var ids=new HashSet<string>(StringComparer.Ordinal);var contexts=new HashSet<string>(StringComparer.Ordinal);
            var idle=new HashSet<int>();
            foreach(var page in data.pages)
            {
                if(!ValidPage(page,appearance)||!ids.Add(page.id)||!contexts.Add(page.action+":"+page.direction+":"+(page.attackKey??"")))return false;
                if(page.action=="Idle")idle.Add(page.direction);
            }
            // complete is descriptive only: even a claimed complete kit must
            // satisfy every page contract and provide all eight native views.
            return idle.Count==8;
        }
        public static int Frame(DarklightLuciusSpritePage page,float seconds)
            =>Frame(page,seconds,DarklightLuciusAppearance.Normal);
        public static int Frame(DarklightLuciusSpritePage page,float seconds,DarklightLuciusAppearance appearance)
        {
            if(!ValidPage(page,appearance))return -1;
            page.clock??=new DarklightWeaponRedesignPage{
                spriteNames=page.spriteNames,durationMs=page.durationMs,loop=page.loop};
            return DarklightWeaponRedesignBank.Frame(page.clock,seconds);
        }
        static DarklightLuciusSpriteCatalog LoadCatalog(DarklightLuciusAppearance appearance)
        {
            string root=ResourceRoot(appearance);if(root==null)return null;
            if(!attempted.Add(appearance))return catalogs.TryGetValue(appearance,out var cached)?cached:null;
            var text=Resources.Load<TextAsset>(root+"catalog");if(text==null)return null;
            var loaded=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            if(!ValidCatalog(loaded,appearance)){Debug.LogError("Lucius "+appearance+" catalog rejected: invalid whole-figure descriptor or duplicate context");return null;}
            return catalogs[appearance]=loaded;
        }
        static bool Load(DarklightLuciusSpritePage page)
        {
            if(page.attempted)return page.frames!=null;page.attempted=true;
            if(!sheets.TryGetValue(page.resourcePath,out var named))
            {
                named=new(StringComparer.Ordinal);
                foreach(var sprite in Resources.LoadAll<Sprite>(page.resourcePath))
                    if(sprite!=null&&!named.TryAdd(sprite.name,sprite))return false;
                sheets[page.resourcePath]=named;
            }
            var result=new Sprite[page.spriteNames.Length];
            for(int i=0;i<result.Length;i++)
            {
                if(!named.TryGetValue(page.spriteNames[i],out var sprite)||
                    sprite.texture.width!=page.nativeSize[0]||sprite.texture.height!=page.nativeSize[1]||
                    Mathf.Abs(sprite.pixelsPerUnit-page.pixelsPerUnit)>.001f)return false;
                result[i]=sprite;
            }
            page.frames=result;return true;
        }
        public static bool TryPose(string action,int direction,float time,out Sprite sprite)
            =>TryPose(action,direction,time,out sprite,out _);
        public static bool TryPose(string action,int direction,float time,out Sprite sprite,out bool fallback,string attackKey=null)
            =>TryPose(DarklightLuciusAppearance.Normal,action,direction,time,out sprite,out fallback,attackKey);
        public static bool TryPose(DarklightLuciusAppearance appearance,string action,int direction,float time,out Sprite sprite,out bool fallback,string attackKey=null)
        {
            sprite=null;fallback=false;
            if(!Action(action)||direction<0||direction>7)return false;
            if(!TryActionPage(appearance,action,direction,attackKey,out var page))
            {
                if(action=="Attack"&&attackKey==DarklightLuciusCombatProfile.SwordThrust)
                {
                    if(TryActionPage(appearance,"Attack",direction,DarklightLuciusCombatProfile.SwordHorizontal,out page)||
                       TryActionPage(appearance,"Attack",direction,DarklightLuciusCombatProfile.SwordVertical,out page))
                    {
                        fallback=true;
                    }
                }
                else if(action=="Draw"||action=="Dodge"||action=="Hurt"||action=="Dead")
                {
                    // Fallback to nearest cardinal direction if exact diagonal not provided natively
                    int[] fallbacks=direction switch {
                        1 => new[]{0, 2}, // SW -> S, W
                        3 => new[]{4, 2}, // NW -> N, W
                        5 => new[]{4, 6}, // NE -> N, E
                        7 => new[]{0, 6}, // SE -> S, E
                        _ => new[]{0, 4, 2, 6}
                    };
                    foreach(int altDir in fallbacks)
                    {
                        if(TryActionPage(appearance,action,altDir,attackKey,out page))
                        {
                            fallback=true;
                            break;
                        }
                    }
                }
                if(page==null)
                {
                    if(action=="Idle"||!TryActionPage(appearance,"Idle",direction,null,out page))return false;
                    fallback=true;
                }
            }
            int frame=Frame(page,time,appearance);
            if(frame<0||!Load(page))return false;
            sprite=page.frames[frame];return true;
        }
        // Exact availability is separate from the safe Idle fallback, so a
        // missing action cannot be mistaken for an integrated animation.
        public static bool TryActionPage(DarklightLuciusAppearance appearance,string action,int direction,string attackKey,out DarklightLuciusSpritePage page)
        {
            page=null;if(!Action(action)||direction<0||direction>7)return false;
            var data=LoadCatalog(appearance);if(data==null)return false;
            foreach(var candidate in data.pages)
                if(candidate.action==action&&candidate.direction==direction&&
                    (action!="Attack"||candidate.attackKey==attackKey)){page=candidate;return true;}
            return false;
        }
        // The authored contact frame must coincide with gameplay contact even
        // when the equipped base changes startup, active or recovery duration.
        public static float AttackTime(DarklightLuciusSpritePage page,AttackDefinition attack,float stateTime)
        {
            if(page==null||attack==null||page.startupFrames<1||page.activeFrames<1)return stateTime;
            int start=0,end=page.startupFrames;float elapsed=stateTime,duration=attack.startup;
            if(stateTime>=attack.startup)
            {
                start=end;end+=page.activeFrames;elapsed-=attack.startup;duration=attack.active;
                if(stateTime>=attack.startup+attack.active)
                {start=end;end=page.durationMs.Length;elapsed-=attack.active;duration=attack.recovery;}
            }
            int offset=0,span=0;
            for(int i=0;i<end;i++){if(i<start)offset+=page.durationMs[i];else span+=page.durationMs[i];}
            return (offset+Mathf.Clamp(duration>0?elapsed/duration:1,0,.99999f)*span)/1000f;
        }
        public static void ClearCache(){catalogs.Clear();attempted.Clear();sheets.Clear();}
    }
}
