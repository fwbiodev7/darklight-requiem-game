using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    [Serializable] public sealed class TodayCharacterPage
    {
        public string name, kind, resourcePath, paletteResourcePath;
        public int skin, age, weapon;
        public int[] directions;
        public int phases;
        public int startupFrameEnd, activeFrameEnd;
        public int idleBlinkFrame=-1;
        public bool ownerMask;
        public string[] spriteNames;
        [NonSerialized] public Sprite[] frames;
        [NonSerialized] public Texture2D palette;
        [NonSerialized] internal bool framesAttempted, paletteAttempted;
        [NonSerialized] internal bool descriptorChecked,descriptorValid;
    }
    public sealed class TodayCharacterCatalog : ScriptableObject
    {
        // Only descriptors are serialized: loading the catalog never loads every texture.
        public TodayCharacterPage[] pages = Array.Empty<TodayCharacterPage>();
    }
    public static class TodayCharacterBank
    {
        const string Root = "Darklight/TodayCharacterAnimations/";
        static TodayCharacterCatalog catalog;
        static bool catalogAttempted;
        static readonly Dictionary<Sprite,int> frameOwners=new();
        static readonly Dictionary<Sprite,Texture2D> frameMasks=new();
        static bool CachedValid(TodayCharacterPage page)
        {
            if(!page.descriptorChecked){page.descriptorValid=ValidDescriptor(page);page.descriptorChecked=true;}
            return page.descriptorValid;
        }
        static TodayCharacterCatalog Catalog()
        {
            if (!catalogAttempted) { catalog = Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog"); catalogAttempted = true; }
            return catalog;
        }
        public static bool ValidDescriptor(TodayCharacterPage page)
        {
            if (page == null || string.IsNullOrEmpty(page.name) || page.skin < 0 || page.skin > 1 || page.age != 0 || page.weapon < 0 || page.weapon > 2 ||
                page.directions == null || page.directions.Length == 0 || page.spriteNames == null || page.spriteNames.Length != page.directions.Length * page.phases ||
                page.resourcePath != Root + page.name || page.paletteResourcePath != Root + page.name + "_Palette") return false;
            int expected = page.kind == "Motion" ? 5 : page.kind == "Idle" ? 2 : page.kind == "Walk" || page.kind == "Attack" ? 4 : page.kind == "Combat" || page.kind == "GuardDodge" ? 8 : 0;
            bool living = LivingKind(page.kind) && page.phases >= 2 && page.phases <= 32;
            bool detailedAttack = page.kind == "Attack" && page.phases > 4 && DarklightAttackFrameTiming.Valid(page.phases,page.startupFrameEnd,page.activeFrameEnd);
            bool detailedIdle = page.kind == "Idle" && page.phases == 6 && page.idleBlinkFrame == 5;
            if (!living && !detailedAttack && !detailedIdle && (page.phases != expected || expected == 0)) return false;
            int seen = 0;
            foreach (int direction in page.directions)
            {
                if (direction < 0 || direction > 7 || (seen & (1 << direction)) != 0) return false;
                seen |= 1 << direction;
            }
            for (int i = 0; i < page.spriteNames.Length; i++)
                if (page.spriteNames[i] != "Today_" + page.name + "_" + i) return false;
            return true;
        }
        public static TodayCharacterPage Page(int skin, int age, int weapon, int direction, string kind)
        {
            var data = Catalog();
            if (data?.pages == null) return null;
            foreach (var page in data.pages)
                if (page != null && page.skin == skin && page.age == 0 && page.weapon == weapon && page.kind == kind &&
                    page.directions != null && Array.IndexOf(page.directions, direction) >= 0 && CachedValid(page)) return page;
            return null;
        }
        static bool LoadFrames(TodayCharacterPage page)
        {
            if (page.framesAttempted) return page.frames != null;
            page.framesAttempted = true;
            var loaded = Resources.LoadAll<Sprite>(page.resourcePath);
            if (loaded.Length != page.spriteNames.Length) return false;
            var byName = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (var sprite in loaded)
            {
                if (sprite == null || byName.ContainsKey(sprite.name)) return false;
                byName.Add(sprite.name, sprite);
            }
            var ordered = new Sprite[page.spriteNames.Length];
            for (int i = 0; i < ordered.Length; i++)
                if (!byName.TryGetValue(page.spriteNames[i], out ordered[i])) return false;
            page.frames = ordered;
            for(int i=0;i<ordered.Length;i++)frameOwners[ordered[i]]=page.ownerMask?i+1:0;
            return true;
        }
        public static Sprite Resolve(int skin, int age, int weapon, int direction, CombatModel model, Vector2 movement)
        {
            string kind = model.State == CombatState.Parry || model.State == CombatState.Dodge ? "GuardDodge" :
                model.State == CombatState.Neutral && !model.ActiveThisTick && movement.sqrMagnitude > .01f ? "Walk" : model.State == CombatState.Neutral ? "Idle" : "Combat";
            if (model.State == CombatState.Attack || model.ActiveThisTick) kind = "Attack";
            var page = Page(skin, age, weapon, direction, kind);
            if (page == null && kind == "Idle") { kind = "Combat"; page = Page(skin, 0, weapon, direction, kind); }
            if (page == null && kind == "Attack") { kind = "Combat"; page = Page(skin, age, weapon, direction, kind); }
            if (page == null || !LoadFrames(page)) return null;
            int phase = 0;
            if (kind == "Idle") phase = DarklightIdleFrameTiming.Frame(page.phases,model.Clock,page.idleBlinkFrame);
            else if (kind == "Walk") phase = (int)(model.Clock * 10f) % 4;
            else if (kind == "Attack")
                phase = page.phases > 4 ? DarklightAttackFrameTiming.Frame(page.phases,page.startupFrameEnd,page.activeFrameEnd,
                    model.StateTime,model.Attack.startup,model.Attack.active,model.Attack.recovery) : model.ActiveThisTick ? 1 : model.StateTime < model.Attack.startup ? 0 :
                    model.StateTime < model.Attack.startup + model.Attack.active ? 1 :
                    model.StateTime < model.Attack.startup + model.Attack.active + model.Attack.recovery * .5f ? 2 : 3;
            else if (kind == "GuardDodge")
            {
                if (model.State == CombatState.Parry)
                    phase = model.StateTime < .04f ? 0 : model.StateTime < .14f ? 1 : model.StateTime < .21f ? 2 : 3;
                else phase = 4 + (model.StateTime < .045f ? 0 : model.StateTime < .12f ? 1 : model.StateTime < .19f ? 2 : 3);
            }
            else if (model.State == CombatState.Neutral && !model.ActiveThisTick) phase = (int)(model.Clock * 2f) % 2 == 0 ? 0 : 5;
            else if (model.State == CombatState.Hurt) phase = 6;
            else if (model.State == CombatState.Dead) phase = 7;
            else if (model.State == CombatState.Charging) phase = 1;
            else if (model.State == CombatState.Attack || model.ActiveThisTick)
                phase = model.ActiveThisTick || model.StateTime < model.Attack.startup + model.Attack.active ?
                    (model.ActiveThisTick || model.StateTime >= model.Attack.startup ? 2 : 1) :
                    model.StateTime < model.Attack.Duration - model.Attack.recovery * .3f ? 3 : 0;
            int row = Array.IndexOf(page.directions, direction);
            return page.frames[row * page.phases + phase];
        }
        public static Sprite YoungIdle(int skin, int weapon, int direction, float clock)
        {
            var page = Page(skin, 0, weapon, direction, "Idle");
            if (page == null || !LoadFrames(page)) return null;
            return page.frames[Array.IndexOf(page.directions, direction) * page.phases + DarklightIdleFrameTiming.Frame(page.phases,clock,page.idleBlinkFrame)];
        }
        static bool LivingKind(string kind) => kind=="IdleAlert" || kind=="IdleImpatient" || kind=="IdleInspect" ||
            kind=="IdleFatigued" || kind=="IdleWounded" || kind=="StartMove" || kind=="Brake" || kind=="Turn";
        public static Sprite LivingPose(int skin,int weapon,int direction,DarklightLivingPresentation living)
        {
            if(living.Clip=="Walk")return null; // keep authored locomotion cadence and foot contacts
            var page=Page(skin,0,weapon,direction,living.Clip);
            if(page==null && DarklightMotionFrameTiming.Supports(living.Clip))page=Page(skin,0,weapon,direction,"Motion");
            if(page==null && living.Clip.StartsWith("Idle",StringComparison.Ordinal))page=Page(skin,0,weapon,direction,"Idle");
            if(page==null || !LoadFrames(page))return null;
            float time=page.kind=="Idle"?living.BreathClock:living.ClipTime;
            int frame=page.kind=="Motion" ? DarklightMotionFrameTiming.Frame(living.Clip,time) : page.kind=="Idle" ? DarklightIdleFrameTiming.Frame(page.phases,time,page.idleBlinkFrame) : living.Clip=="StartMove" || living.Clip=="Brake" || living.Clip=="Turn" ?
                Mathf.Min(page.phases-1,(int)(time*page.phases/.14f)) : (int)(time*(page.kind=="Idle"?2f:8f))%page.phases;
            if(page.kind=="Motion" && frame==5)return YoungIdle(skin,weapon,direction,0);
            return page.frames[Array.IndexOf(page.directions,direction)*page.phases+frame];
        }
        public static Sprite StablePose(int skin, int age, int weapon, int direction)
        {
            var page = Page(skin, 0, weapon, direction, "Idle") ?? Page(skin, 0, weapon, direction, "Combat") ?? Page(skin, age, weapon, direction, "Attack") ?? Page(skin, age, weapon, direction, "Walk");
            if (page == null || !LoadFrames(page)) return null;
            int phase = page.kind == "Attack" ? 3 : 0;
            return page.frames[Array.IndexOf(page.directions, direction) * page.phases + phase];
        }
        public static Texture2D Mask(Sprite sprite)
        {
            if(sprite!=null && frameMasks.TryGetValue(sprite,out var cached))return cached;
            if (sprite == null || !sprite.name.StartsWith("Today_", StringComparison.Ordinal)) return null;
            int suffix = sprite.name.LastIndexOf('_');
            if (suffix <= 6) return null;
            string pageName = sprite.name.Substring(6, suffix - 6);
            var data = Catalog();
            if (data?.pages == null) return null;
            foreach (var page in data.pages)
            {
                if (page == null || page.name != pageName || !CachedValid(page) || !LoadFrames(page) || Array.IndexOf(page.frames, sprite) < 0) continue;
                if (!page.paletteAttempted) { page.palette = Resources.Load<Texture2D>(page.paletteResourcePath); page.paletteAttempted = true; }
                foreach(var frame in page.frames)frameMasks[frame]=page.palette;
                return page.palette;
            }
            return null;
        }
        public static int LoadedPageCount
        {
            get { int count = 0; if (catalog?.pages != null) foreach (var page in catalog.pages) if (page?.frames != null) count++; return count; }
        }
        public static int WholeFrameId(Sprite sprite)
        {
            if (sprite == null || Catalog()?.pages == null) return 0;
            if(frameOwners.TryGetValue(sprite,out int cached))return cached;
            string name=sprite.name;
            foreach (var page in Catalog().pages)
            {
                if (page == null || !page.ownerMask || !CachedValid(page)) continue;
                int index = Array.IndexOf(page.spriteNames, name);
                if (index >= 0) {frameOwners[sprite]=index+1;return index+1;}
            }
            return 0;
        }
        public static void ClearCache()
        {
            if (catalog?.pages != null) foreach (var page in catalog.pages)
                if (page != null) { page.frames = null; page.palette = null; page.framesAttempted = page.paletteAttempted = page.descriptorChecked = false; }
            frameOwners.Clear();frameMasks.Clear();
            catalog = null; catalogAttempted = false;
        }
    }
}
