using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed class DarklightLuciusSpritePage
    {
        public string id,action,resourcePath,imageSha256,sourceSha256,status;
        public int direction;
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
        public string characterId,form,weapon;
        public bool complete;
        public DarklightLuciusSpritePage[] pages;
    }

    // A separate opt-in presentation bank. It never addresses Lucien's boss ID,
    // the player's skins, campaign registration or transformation state.
    public static class DarklightLuciusSpriteBank
    {
        public const string Root="Darklight/LuciusNormal20261109/";
        static DarklightLuciusSpriteCatalog catalog;
        static bool attempted;
        static readonly Dictionary<string,Dictionary<string,Sprite>> sheets=new(StringComparer.Ordinal);
        static bool Name(string name)
        {
            if(string.IsNullOrEmpty(name))return false;
            foreach(char c in name)
                if(!(c>='A'&&c<='Z'||c>='a'&&c<='z'||c>='0'&&c<='9'||c=='_'||c=='-'))return false;
            return true;
        }
        static bool Action(string action)=>action=="Idle"||action=="Walk"||action=="Run"||
            action=="Attack"||action=="Charging"||action=="Dodge"||action=="Parry"||action=="Hurt"||action=="Dead";
        public static bool ValidPage(DarklightLuciusSpritePage page)
        {
            if(page==null||!Name(page.id)||!page.id.StartsWith("Lucius",StringComparison.Ordinal)||!Action(page.action)||page.direction<0||page.direction>7||
                !page.wholeFigures||page.status!="visual-reviewed"&&page.status!="imported"&&page.status!="runtime-verified"||
                !DarklightWeaponRedesignBank.Sha256(page.imageSha256)||!DarklightWeaponRedesignBank.Sha256(page.sourceSha256)||
                page.nativeSize?.Length!=2||page.nativeSize[0]<=0||page.nativeSize[1]<=0||
                page.pixelsPerUnit<=0||float.IsNaN(page.pixelsPerUnit)||float.IsInfinity(page.pixelsPerUnit)||
                string.IsNullOrEmpty(page.resourcePath)||!page.resourcePath.StartsWith(Root,StringComparison.Ordinal)||
                !Name(page.resourcePath.Substring(Root.Length))||
                page.spriteNames==null||page.spriteNames.Length==0||page.durationMs?.Length!=page.spriteNames.Length)return false;
            var names=new HashSet<string>(StringComparer.Ordinal);
            foreach(string name in page.spriteNames)
                if(!Name(name)||!name.StartsWith("Redesign_Lucius",StringComparison.Ordinal)||!names.Add(name))return false;
            foreach(int duration in page.durationMs)if(duration<=0)return false;
            if(page.loop&&page.action!="Idle"&&page.action!="Walk"&&page.action!="Run"&&page.action!="Charging")return false;
            return true;
        }
        public static bool ValidCatalog(DarklightLuciusSpriteCatalog data)
        {
            if(data==null||data.schemaVersion!=1||data.characterId!="Lucius"||data.form!="Normal"||
                data.weapon!="Sword"||data.pages==null)return false;
            var ids=new HashSet<string>(StringComparer.Ordinal);var contexts=new HashSet<string>(StringComparer.Ordinal);
            var idle=new HashSet<int>();
            foreach(var page in data.pages)
            {
                if(!ValidPage(page)||!ids.Add(page.id)||!contexts.Add(page.action+":"+page.direction))return false;
                if(page.action=="Idle")idle.Add(page.direction);
            }
            // complete is descriptive only: even a claimed complete kit must
            // satisfy every page contract and provide all eight native views.
            return idle.Count==8;
        }
        public static int Frame(DarklightLuciusSpritePage page,float seconds)
        {
            if(!ValidPage(page))return -1;
            page.clock??=new DarklightWeaponRedesignPage{
                spriteNames=page.spriteNames,durationMs=page.durationMs,loop=page.loop};
            return DarklightWeaponRedesignBank.Frame(page.clock,seconds);
        }
        static void LoadCatalog()
        {
            if(attempted)return;attempted=true;
            var text=Resources.Load<TextAsset>(Root+"catalog");if(text==null)return;
            var loaded=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            if(!ValidCatalog(loaded)){Debug.LogError("Lucius Normal catalog rejected: invalid whole-figure descriptor or duplicate context");return;}
            catalog=loaded;
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
        public static bool TryPose(string action,int direction,float time,out Sprite sprite,out bool fallback)
        {
            sprite=null;fallback=false;
            if(!Action(action)||direction<0||direction>7)return false;
            LoadCatalog();if(catalog==null)return false;
            for(int pass=0;pass<(action=="Idle"?1:2);pass++)foreach(var page in catalog.pages)
            {
                if(page.direction!=direction||page.action!=(pass==0?action:"Idle"))continue;
                int frame=Frame(page,time);
                if(frame<0||!Load(page))return false;
                sprite=page.frames[frame];fallback=pass!=0;return true;
            }
            return false;
        }
        public static void ClearCache(){catalog=null;attempted=false;sheets.Clear();}
    }
}
