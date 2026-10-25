using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed class DarklightWeaponRedesignPage
    {
        public string id,action,attackKey,resourcePath,imageSha256,sourcePath,sourceSha256,status;
        public int skin,weapon,form,direction;
        public int[] sourceSize,nativeSize,durationMs;
        public float pixelsPerUnit;
        public bool wholeFigures,loop;
        public string[] spriteNames,sourceSpriteNames;
        [NonSerialized] internal Sprite[] frames;
        [NonSerialized] internal bool attempted;
    }
    [Serializable] public sealed class DarklightWeaponRedesignCatalog
    {
        public int schemaVersion;
        public bool complete;
        public string fallbackPolicy;
        public DarklightWeaponRedesignPage[] pages;
    }

    // Opt-in whole figures. Missing contexts continue through the existing authored
    // consumer; they never borrow a redesigned direction, form or weapon.
    public static class DarklightWeaponRedesignBank
    {
        [Serializable] sealed class IconSheet{public string status;public bool wholeFigures;}
        public const string Root="Darklight/WeaponRedesign20261023/";
        public const string FallbackPolicy="approved-existing-same-context";
        static readonly Dictionary<int,List<DarklightWeaponRedesignPage>> contexts=new();
        static readonly Dictionary<string,Dictionary<string,Sprite>> sheets=new(StringComparer.Ordinal);
        static readonly HashSet<Sprite> authored=new();
        static readonly Dictionary<string,Sprite> icons=new(StringComparer.Ordinal);
        static readonly MaterialPropertyBlock block=new();
        static bool attempted,iconsAttempted;
        static int Context(int skin,int weapon,int form,int direction)=>((skin*3+weapon)*4+form)*8+direction;
        public static bool Sha256(string value)
        {
            if(value==null||value.Length!=64)return false;
            foreach(char c in value)if(!Uri.IsHexDigit(c))return false;
            return true;
        }
        static bool Size(int[] value)=>value?.Length==2&&value[0]>0&&value[1]>0;
        static bool TimedAction(string action)=>action=="Idle"||action=="Walk"||action=="Run"||
            action=="IdleAlert"||action=="IdleImpatient"||action=="IdleInspect"||
            action=="IdleFatigued"||action=="IdleWounded"||action=="StartMove"||action=="Brake"||action=="Turn";
        static bool KnownAction(string action)=>TimedAction(action)||action=="Attack"||action=="Charging"||
            action=="Dodge"||action=="Parry"||action=="Hurt"||action=="Dead"||action=="Transformation";
        public static bool ValidPage(DarklightWeaponRedesignPage page)
        {
            if(page==null||string.IsNullOrEmpty(page.id)||page.skin<0||page.skin>1||page.weapon<0||page.weapon>2||
                page.form<0||page.form>3||page.direction<0||page.direction>7||!KnownAction(page.action)||!page.wholeFigures||
                page.status!="visual-reviewed"&&page.status!="imported"&&page.status!="runtime-verified"||
                string.IsNullOrEmpty(page.resourcePath)||!page.resourcePath.StartsWith(Root,StringComparison.Ordinal)||
                page.resourcePath.Contains("..")||page.resourcePath.EndsWith(".png",StringComparison.OrdinalIgnoreCase)||
                !Sha256(page.imageSha256)||!Sha256(page.sourceSha256)||string.IsNullOrEmpty(page.sourcePath)||
                !page.sourcePath.StartsWith("ArtSources/",StringComparison.Ordinal)||page.sourcePath.Contains("..")||
                !Size(page.nativeSize)||!Size(page.sourceSize)||page.pixelsPerUnit<=0||float.IsNaN(page.pixelsPerUnit)||float.IsInfinity(page.pixelsPerUnit)||
                page.spriteNames==null||page.spriteNames.Length==0)return false;
            foreach(string name in page.spriteNames)if(string.IsNullOrEmpty(name)||!name.StartsWith("Redesign_",StringComparison.Ordinal))return false;
            if(!string.IsNullOrEmpty(page.attackKey)&&page.action!="Attack")return false;
            if(page.sourceSpriteNames?.Length>0)
            {
                if(page.sourceSpriteNames.Length!=page.spriteNames.Length)return false;
                var names=new HashSet<string>(StringComparer.Ordinal);
                foreach(string name in page.sourceSpriteNames)if(string.IsNullOrEmpty(name)||!names.Add(name))return false;
                return true;
            }
            if(!TimedAction(page.action)||page.durationMs?.Length!=page.spriteNames.Length)return false;
            foreach(int duration in page.durationMs)if(duration<=0)return false;
            return true;
        }
        public static bool ValidCatalog(DarklightWeaponRedesignCatalog data)
        {
            if(data==null||data.schemaVersion!=1||data.fallbackPolicy!=FallbackPolicy||data.pages==null)return false;
            var keys=new HashSet<string>(StringComparer.Ordinal);var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var page in data.pages)
                if(!ValidPage(page)||!ids.Add(page.id)||!keys.Add(Context(page.skin,page.weapon,page.form,page.direction)+":"+page.action+":"+(page.attackKey??"")))return false;
            // Completeness is a separate coverage audit, never inferred from a nonempty bank.
            return true;
        }
        static void LoadCatalog()
        {
            if(attempted)return;attempted=true;
            var text=Resources.Load<TextAsset>(Root+"catalog");if(text==null)return;
            var data=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(text.text);
            if(!ValidCatalog(data)){Debug.LogError("Weapon redesign catalog rejected: invalid descriptor or duplicate context");return;}
            foreach(var page in data.pages)
            {
                int key=Context(page.skin,page.weapon,page.form,page.direction);
                if(!contexts.TryGetValue(key,out var entries))contexts[key]=entries=new();
                entries.Add(page);
            }
        }
        static bool Load(DarklightWeaponRedesignPage page)
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
                if(!named.TryGetValue(page.spriteNames[i],out result[i]))return false;
                var sprite=result[i];
                if(sprite.texture.width!=page.nativeSize[0]||sprite.texture.height!=page.nativeSize[1]||
                    Mathf.Abs(sprite.pixelsPerUnit-page.pixelsPerUnit)>.001f)return false;
            }
            page.frames=result;foreach(var sprite in result)authored.Add(sprite);return true;
        }
        public static string Action(CombatModel model,Vector2 movement,string livingClip=null)
        {
            if(model.State==CombatState.Attack||model.ActiveThisTick)return "Attack";
            if(model.State!=CombatState.Neutral)return model.State.ToString();
            return !string.IsNullOrEmpty(livingClip)?livingClip:movement.sqrMagnitude>.01f?"Walk":"Idle";
        }
        public static int Frame(DarklightWeaponRedesignPage page,float seconds,string sourceSpriteName=null)
        {
            if(page?.spriteNames==null||page.spriteNames.Length==0)return -1;
            if(page.sourceSpriteNames?.Length>0)return Array.IndexOf(page.sourceSpriteNames,sourceSpriteName);
            if(page.durationMs?.Length!=page.spriteNames.Length)return -1;
            long duration=0;foreach(int ms in page.durationMs)duration+=ms;
            if(duration<=0)return -1;
            double position=Math.Max(0,double.IsNaN(seconds)||double.IsInfinity(seconds)?0:seconds)*1000d;
            position=page.loop?position%duration:Math.Min(position,duration-1);
            for(int i=0;i<page.durationMs.Length;i++){if(position<page.durationMs[i])return i;position-=page.durationMs[i];}
            return page.spriteNames.Length-1;
        }
        public static bool TryResolve(int skin,int weapon,int form,int direction,CombatModel model,Vector2 movement,
            Sprite existing,out Sprite sprite,string livingClip=null,float livingTime=-1)
        {
            sprite=null;if(model==null||skin<0||skin>1||weapon<0||weapon>2||form<0||form>3||direction<0||direction>7)return false;
            LoadCatalog();if(!contexts.TryGetValue(Context(skin,weapon,form,direction),out var entries))return false;
            string action=Action(model,movement,livingClip),attack=model.Attack?.displayName??"";
            // Named contacts first, then an explicit mapping of an existing phase.
            for(int pass=0;pass<2;pass++)foreach(var page in entries)
            {
                if(page.action!=action||(pass==0?string.IsNullOrEmpty(page.attackKey)||page.attackKey!=attack:!string.IsNullOrEmpty(page.attackKey)))continue;
                float time=model.State==CombatState.Neutral?(livingTime>=0?livingTime:model.Clock):model.StateTime;
                int index=Frame(page,time,existing?.name);
                if(index<0||!Load(page))continue;
                sprite=page.frames[index];return true;
            }
            return false;
        }
        public static bool IsAuthored(Sprite sprite)=>sprite!=null&&authored.Contains(sprite);
        public static Sprite Icon(int weapon,int form,Sprite fallback)
        {
            if(weapon<0||weapon>2||form<0||form>3)return fallback;
            if(!iconsAttempted)
            {
                iconsAttempted=true;
                var text=Resources.Load<TextAsset>(Root+"EquipmentIcons");
                var descriptor=text==null?null:JsonUtility.FromJson<IconSheet>(text.text);
                if(descriptor?.wholeFigures==true&&(descriptor.status=="visual-reviewed"||descriptor.status=="imported"||descriptor.status=="runtime-verified"))
                    foreach(var sprite in Resources.LoadAll<Sprite>(Root+"EquipmentIcons"))if(sprite!=null)icons[sprite.name]=sprite;
            }
            return icons.TryGetValue("Redesign_Icon_Weapon"+weapon+"_Form"+form,out var selected)?selected:fallback;
        }
        public static void ApplyAuthoredOwnership(SpriteRenderer renderer)
        {
            if(renderer==null||!IsAuthored(renderer.sprite))return;
            renderer.GetPropertyBlock(block);
            // This PNG already contains the selected form, skin, eyes and weapons.
            // Old owner IDs/palette selectors can otherwise erase the new blade.
            block.SetFloat("_WholeSprite",1);block.SetFloat("_WholeFrameId",0);block.SetFloat("_VampireForm",0);
            block.SetFloat("_DarkForm",0);block.SetFloat("_Physique",0);block.SetFloat("_LayerOcclusion",0);
            block.SetFloat("_EraseHead",0);block.SetFloat("_RuinFists",0);block.SetFloat("_AlphaClip",.1f);
            block.SetTexture("_VampireMask",Texture2D.blackTexture);block.SetVector("_CanonicalSkinShift",Vector4.zero);
            renderer.SetPropertyBlock(block);
        }
        public static void ClearCache(){attempted=iconsAttempted=false;contexts.Clear();sheets.Clear();authored.Clear();icons.Clear();}
    }
}
