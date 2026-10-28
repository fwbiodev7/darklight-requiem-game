using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed class DarklightRuinFistsPage
    {
        public int skin,form,direction;
        public string bank,resourcePath,palettePath;
        public bool paletteVariant;
        public string[] spriteNames;
        [NonSerialized] internal Sprite[] frames;
        [NonSerialized] internal Texture2D mask;
        [NonSerialized] internal bool attempted;
    }
    [Serializable] public sealed class DarklightRuinFistsCatalog
    {
        public bool complete;
        public DarklightRuinFistsPage[] pages;
    }

    // Independent pages prevent a missing direction/form from borrowing another weapon.
    public static class DarklightRuinFistsBank
    {
        const string CatalogPath="Darklight/RuinFists/catalog";
        static DarklightRuinFistsCatalog catalog;
        static bool attempted,ready;
        static readonly Dictionary<int,DarklightRuinFistsPage> pages=new();
        static readonly Dictionary<Sprite,(DarklightRuinFistsPage page,int index)> frames=new();
        sealed class DeathClock{public float start;}
        static ConditionalWeakTable<CombatModel,DeathClock> deaths=new();
        static int Key(int skin,int form,int direction,bool motion)=>(((skin*4+form)*8+direction)*2)+(motion?1:0);
        public static bool Complete(DarklightRuinFistsCatalog data)
        {
            if(data?.complete!=true||data.pages?.Length!=128)return false;
            var seen=new HashSet<int>();
            var indexed=new Dictionary<int,DarklightRuinFistsPage>();
            var resources=new HashSet<string>(StringComparer.Ordinal);
            var allNames=new HashSet<string>(StringComparer.Ordinal);
            foreach(var page in data.pages)
            {
                if(page==null||page.skin<0||page.skin>1||page.form<0||page.form>3||page.direction<0||page.direction>7||
                   page.bank!="Combat"&&page.bank!="Motion"||page.spriteNames?.Length!=12||
                   string.IsNullOrEmpty(page.resourcePath)||string.IsNullOrEmpty(page.palettePath)||
                   !page.resourcePath.StartsWith("Darklight/RuinFists/",StringComparison.Ordinal)||
                   !seen.Add(Key(page.skin,page.form,page.direction,page.bank=="Motion")))return false;
                indexed.Add(Key(page.skin,page.form,page.direction,page.bank=="Motion"),page);
                if(page.paletteVariant&&(page.form!=1&&page.form!=2))return false;
                if(!page.paletteVariant&&!resources.Add(page.resourcePath))return false;
                var names=new HashSet<string>();
                foreach(string name in page.spriteNames)
                    if(string.IsNullOrEmpty(name)||!name.StartsWith("Ruin_",StringComparison.Ordinal)||!names.Add(name)||!page.paletteVariant&&!allNames.Add(name))return false;
            }
            foreach(var page in data.pages)if(page.paletteVariant)
            {
                if(!indexed.TryGetValue(Key(page.skin,0,page.direction,page.bank=="Motion"),out var source)||source.paletteVariant||
                   source.resourcePath!=page.resourcePath||source.palettePath!=page.palettePath)return false;
                for(int i=0;i<12;i++)if(source.spriteNames[i]!=page.spriteNames[i])return false;
            }
            return seen.Count==128;
        }
        public static bool Ready
        {
            get
            {
                if(attempted)return ready;attempted=true;
                var text=Resources.Load<TextAsset>(CatalogPath);
                if(text==null)return false;
                catalog=JsonUtility.FromJson<DarklightRuinFistsCatalog>(text.text);
                ready=Complete(catalog);
                if(ready)foreach(var page in catalog.pages)pages.Add(Key(page.skin,page.form,page.direction,page.bank=="Motion"),page);
                return ready;
            }
        }
        static bool Load(DarklightRuinFistsPage page)
        {
            if(page.attempted)return page.frames!=null;page.attempted=true;
            if(page.paletteVariant)
            {
                if(!pages.TryGetValue(Key(page.skin,0,page.direction,page.bank=="Motion"),out var source)||!Load(source))return false;
                page.frames=source.frames;page.mask=source.mask;return true;
            }
            var sprites=Resources.LoadAll<Sprite>(page.resourcePath);
            if(sprites.Length!=12)return false;
            var named=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in sprites)if(!named.TryAdd(sprite.name,sprite))return false;
            var ordered=new Sprite[12];
            for(int i=0;i<12;i++)if(!named.TryGetValue(page.spriteNames[i],out ordered[i]))return false;
            page.mask=Resources.Load<Texture2D>(page.palettePath);
            if(page.mask==null)return false;
            page.frames=ordered;
            for(int i=0;i<12;i++)frames[ordered[i]]=(page,i);
            return true;
        }
        static Sprite Frame(int skin,int form,int direction,bool motion,int index)
        {
            if(!Ready||!pages.TryGetValue(Key(skin,form,direction,motion),out var page)||!Load(page))return null;
            return page.frames[index];
        }
        public static int AttackFrame(AttackDefinition attack,float time,bool contact,float progress)
        {
            if(attack==null)return 0;
            bool startup=!contact&&time<attack.startup;
            bool active=contact||time<attack.startup+attack.active;
            if(!startup&&!active)
                return attack.ruinStrike==DarklightRuinStrike.AerialSlam&&time<attack.startup+attack.active+attack.recovery*.55f?10:11;
            return attack.ruinStrike switch
            {
                DarklightRuinStrike.Punch=>startup?2:3,
                DarklightRuinStrike.SideKick=>startup?4:5,
                DarklightRuinStrike.SpinKick=>startup?4:6,
                DarklightRuinStrike.Uppercut=>startup?7:8,
                DarklightRuinStrike.GroundSlam=>startup?7:10,
                DarklightRuinStrike.AerialSlam=>startup?8:9,
                DarklightRuinStrike.Cyclone=>startup?4:progress<.7f?6:10,
                _=>0
            };
        }
        // Four low steps over a calmer full cycle; other weapon cadences stay intact.
        public static int WalkFrame(float clock)=>Mathf.Min(3,Mathf.FloorToInt(Mathf.Repeat(clock,.6f)/.15f));
        public static Sprite Resolve(int skin,int direction,CombatModel model,Vector2 movement,float unscaledClock=-1)
        {
            bool motion=false;int frame;
            if(model.State==CombatState.Attack||model.ActiveThisTick)
                frame=AttackFrame(model.Attack,model.StateTime,model.ActiveThisTick,model.ContactProgress);
            else if(model.State==CombatState.Charging)frame=7;
            else if(model.State==CombatState.Dodge){motion=true;frame=model.StateTime<.045f?4:model.StateTime<.14f?5:6;}
            else if(model.State==CombatState.Parry){motion=true;frame=model.StateTime<.05f||model.StateTime>=.21f?7:8;}
            else if(model.State==CombatState.Hurt){motion=true;frame=9;}
            else if(model.State==CombatState.Dead)
            {
                // Defeat pauses the combat clock. The complete death pose must still finish and hold.
                float now=unscaledClock>=0?unscaledClock:Time.unscaledTime;
                if(!deaths.TryGetValue(model,out var death)){death=new DeathClock{start=now};deaths.Add(model,death);}
                motion=true;frame=now-death.start<.25f?10:11;
            }
            else if(movement.sqrMagnitude>.01f){motion=true;frame=WalkFrame(model.Clock);}
            else frame=Mathf.Repeat(model.Clock,1.2f)<.6f?0:1;
            return Frame(skin,0,direction,motion,frame);
        }
        public static Sprite Idle(int skin,int direction,float clock)=>Frame(skin,0,direction,false,Mathf.Repeat(clock,1.2f)<.6f?0:1);
        public static bool TryTransform(Sprite normal,int form,out Sprite sprite,out Texture2D mask,out int owner)
        {
            sprite=null;mask=null;owner=0;
            if(form<1||form>3||normal==null||!frames.TryGetValue(normal,out var source))return false;
            sprite=Frame(source.page.skin,form,source.page.direction,source.page.bank=="Motion",source.index);
            if(sprite==null||!frames.TryGetValue(sprite,out var transformed))return false;
            mask=transformed.page.mask;owner=transformed.index+1;return true;
        }
        public static Texture2D Mask(Sprite sprite)=>sprite!=null&&frames.TryGetValue(sprite,out var frame)?frame.page.mask:null;
        public static int FrameId(Sprite sprite)=>sprite!=null&&frames.TryGetValue(sprite,out var frame)?frame.index+1:0;
        public static bool IsRuin(Sprite sprite)=>sprite!=null&&sprite.name.StartsWith("Ruin_",StringComparison.Ordinal);
        public static bool UsesMaterialPalette(Sprite normal,int form)=>IsRuin(normal)&&frames.TryGetValue(normal,out var source)&&
            pages.TryGetValue(Key(source.page.skin,form,source.page.direction,source.page.bank=="Motion"),out var target)&&target.paletteVariant;
        public static void ClearCache(){catalog=null;attempted=ready=false;pages.Clear();frames.Clear();deaths=new();}
    }
}
