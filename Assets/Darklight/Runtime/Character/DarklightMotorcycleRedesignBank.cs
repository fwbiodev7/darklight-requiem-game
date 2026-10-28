using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Authored whole rider/bike figures. A missing form/action never borrows one.
    public static class DarklightMotorcycleRedesignBank
    {
        [Serializable] sealed class Motorcycle
        {
            public int form,skin;
            public string[] ride,slam;
            public bool staticRideViews;
        }
        [Serializable] sealed class Sheet
        {
            public string status;
            public int[] nativeSize;
            public float pixelsPerUnit;
            public bool wholeFigures;
            public Motorcycle motorcycle;
            public MotorcycleIdle motorcycleIdle;
            public DualSaws motorcycleDualSaws;
        }
        [Serializable] sealed class DualSaws {public int skin,form;public bool onFoot,dualSaws,staticReadyViews;public string[] ready,attackSouth;}
        sealed class DualPage {public Sprite[] ready,attack;}
        static DualPage dualPage;static bool dualAttempted;
        static readonly HashSet<Sprite> dualSprites=new();
        public static string RequiemDualSawsResource=>DarklightWeaponRedesignBank.Root+"ReqMotoNormalDualSaws8AttackS20261027V1";
        static DualPage LoadDualSaws()
        {
            if(dualAttempted)return dualPage;dualAttempted=true;
            var text=Resources.Load<TextAsset>(RequiemDualSawsResource);if(text==null)return null;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);var entry=sheet?.motorcycleDualSaws;
            if(sheet==null||!sheet.wholeFigures||sheet.nativeSize?.Length!=2||sheet.pixelsPerUnit<=0||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified"||
                entry==null||entry.skin!=0||entry.form!=0||!entry.onFoot||!entry.dualSaws||entry.ready?.Length!=8||entry.attackSouth?.Length!=4)return null;
            var sprites=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(RequiemDualSawsResource))
                if(sprite.texture.width!=sheet.nativeSize[0]||sprite.texture.height!=sheet.nativeSize[1]||Mathf.Abs(sprite.pixelsPerUnit-sheet.pixelsPerUnit)>.001f||!sprites.TryAdd(sprite.name,sprite))return null;
            var page=new DualPage{ready=new Sprite[8],attack=new Sprite[4]};var names=new HashSet<string>();
            for(int i=0;i<12;i++)
            {
                string name=i<8?entry.ready[i]:entry.attackSouth[i-8];
                if(name==null||!name.StartsWith("Redesign_",StringComparison.Ordinal)||!names.Add(name)||!sprites.TryGetValue(name,out var sprite))return null;
                if(i<8)page.ready[i]=sprite;else page.attack[i-8]=sprite;
            }
            dualPage=page;foreach(var sprite in page.ready){authored.Add(sprite);dualSprites.Add(sprite);}
            foreach(var sprite in page.attack){authored.Add(sprite);dualSprites.Add(sprite);}return page;
        }
        public static Sprite DualSawsPose(int direction,CombatModel model=null)
        {
            var page=LoadDualSaws();if(page==null||direction<0||direction>7)return null;
            if(direction==0&&model?.State==CombatState.Attack&&model.Attack!=null)
            {
                var a=model.Attack;float t=model.StateTime;
                return page.attack[t<a.startup*.5f?0:t<a.startup?1:t<a.startup+a.active?2:3];
            }
            return page.ready[direction];
        }
        public static bool IsDualSaws(Sprite sprite)=>sprite!=null&&dualSprites.Contains(sprite);
        [Serializable] sealed class IdleClip {public int direction;public string[] frames;public int[] durationMs;}
        [Serializable] sealed class MotorcycleIdle {public int form,skin;public IdleClip[] clips;}
        sealed class IdlePage {public Sprite[] frames;public int[] durationMs;public int totalMs;}
        static readonly IdlePage[] idlePages=new IdlePage[8];
        static bool idleAttempted;
        sealed class Page {public Sprite[] ride,slam;}
        static readonly Page[] pages=new Page[8];
        static readonly bool[] attempted=new bool[8];
        static readonly HashSet<Sprite> authored=new();
        static readonly MaterialPropertyBlock block=new();
        public static string Resource(int form)=>DarklightWeaponRedesignBank.Root+"RockMotoForm"+form+"Ride8SlamS20261025V2";
        public static string RequiemResource(int form)=>DarklightWeaponRedesignBank.Root+"RequiemMotoForm"+DarklightPlayableScope.Form(form)+"Ride8SlamS"+(form==0?"20261027V3":"20261026V1");
        public static string RequiemIdleResource=>DarklightWeaponRedesignBank.Root+"RequiemMotoForm0IdleSSWW20261027MatureV1";
        public static string RequiemRearIdleResource=>DarklightWeaponRedesignBank.Root+"RequiemMotoForm0IdleNW_N_NE20261027MatureV1";
        public static string RequiemSideIdleResource=>DarklightWeaponRedesignBank.Root+"RequiemMotoForm0IdleE_SE20261027MatureV1";
        static void LoadIdle()
        {
            if(idleAttempted)return;idleAttempted=true;
            LoadIdleSheet(RequiemIdleResource);LoadIdleSheet(RequiemRearIdleResource);LoadIdleSheet(RequiemSideIdleResource);
        }
        static void LoadIdleSheet(string resource)
        {
            var text=Resources.Load<TextAsset>(resource);if(text==null)return;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);var entry=sheet?.motorcycleIdle;
            if(sheet==null||!sheet.wholeFigures||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified"||
                sheet.nativeSize?.Length!=2||sheet.pixelsPerUnit<=0||float.IsNaN(sheet.pixelsPerUnit)||float.IsInfinity(sheet.pixelsPerUnit)||
                entry==null||entry.form!=0||entry.skin!=0||entry.clips==null)return;
            var named=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(resource))
                if(sprite.texture.width!=sheet.nativeSize[0]||sprite.texture.height!=sheet.nativeSize[1]||
                    Mathf.Abs(sprite.pixelsPerUnit-sheet.pixelsPerUnit)>.001f||!named.TryAdd(sprite.name,sprite))return;
            var pending=new IdlePage[8];var used=new HashSet<string>(StringComparer.Ordinal);
            foreach(var clip in entry.clips)
            {
                if(clip==null||clip.direction<0||clip.direction>7||pending[clip.direction]!=null||idlePages[clip.direction]!=null||clip.frames?.Length!=4||clip.durationMs?.Length!=4)return;
                var page=new IdlePage{frames=new Sprite[4],durationMs=clip.durationMs};
                for(int i=0;i<4;i++)
                {
                    string name=clip.frames[i];
                    if(name==null||!name.StartsWith("Redesign_",StringComparison.Ordinal)||!used.Add(name)||clip.durationMs[i]<=0||
                        !named.TryGetValue(name,out page.frames[i]))return;
                    page.totalMs+=clip.durationMs[i];
                }
                pending[clip.direction]=page;
            }
            for(int d=0;d<8;d++)if(pending[d]!=null)
            {idlePages[d]=pending[d];foreach(var sprite in pending[d].frames)authored.Add(sprite);}
        }
        public static Sprite Idle(int form,int direction,float seconds)
        {
            if(form!=0||direction<0||direction>7||float.IsNaN(seconds)||float.IsInfinity(seconds))return null;
            LoadIdle();var page=idlePages[direction];if(page==null)return null;
            double position=Math.Max(0,seconds)*1000d%page.totalMs;
            for(int i=0;i<page.frames.Length;i++){if(position<page.durationMs[i])return page.frames[i];position-=page.durationMs[i];}
            return page.frames[page.frames.Length-1];
        }
        static Page Load(int form,int skin=1)
        {
            if(form<0||form>3||skin<0||skin>1)return null;
            int slot=skin*4+form;
            if(attempted[slot])return pages[slot];attempted[slot]=true;
            var text=Resources.Load<TextAsset>(skin==0?RequiemResource(form):Resource(form));if(text==null)return null;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);var entry=sheet?.motorcycle;
            if(sheet==null||!sheet.wholeFigures||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified"||
                sheet.nativeSize?.Length!=2||sheet.pixelsPerUnit<=0||float.IsNaN(sheet.pixelsPerUnit)||float.IsInfinity(sheet.pixelsPerUnit)||
                entry==null||entry.form!=form||entry.skin!=skin||!entry.staticRideViews||entry.ride?.Length!=8||entry.slam?.Length!=4)return null;
            var sprites=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(skin==0?RequiemResource(form):Resource(form)))
            {
                if(sprite.texture.width!=sheet.nativeSize[0]||sprite.texture.height!=sheet.nativeSize[1]||
                    Mathf.Abs(sprite.pixelsPerUnit-sheet.pixelsPerUnit)>.001f||!sprites.TryAdd(sprite.name,sprite))return null;
            }
            var names=new HashSet<string>(StringComparer.Ordinal);
            var page=new Page{ride=new Sprite[8],slam=new Sprite[4]};
            for(int i=0;i<12;i++)
            {
                string name=i<8?entry.ride[i]:entry.slam[i-8];
                if(name==null||!name.StartsWith("Redesign_",StringComparison.Ordinal)||!names.Add(name)||!sprites.TryGetValue(name,out var sprite))return null;
                if(i<8)page.ride[i]=sprite;else page.slam[i-8]=sprite;
            }
            pages[slot]=page;foreach(var sprite in page.ride)authored.Add(sprite);foreach(var sprite in page.slam)authored.Add(sprite);
            return page;
        }
        public static Sprite Riding(int form,int direction,int skin=1)=>direction<0||direction>7?null:Load(form,skin)?.ride[direction];
        public static Sprite GroundSlam(int form,int direction,float seconds,float startup,float active,float recovery,int skin=1)
        {
            if(direction!=0||seconds<0||startup<=0||active<=0||recovery<=0)return null;
            var page=Load(form,skin);if(page==null)return null;
            int phase=seconds<startup*.4f?0:seconds<startup?1:seconds<startup+active?2:3;
            return page.slam[phase];
        }
        public static Sprite Resolve(DarklightAbyssRider rider,CombatModel model,int form)
        {
            if(rider==null||model==null||!rider.Active||model.State!=CombatState.Attack||model.Attack==null)return null;
            int direction=DarklightCharacterView.DirectionIndex(model.Facing);var attack=model.Attack;
            if(rider.Slamming&&!rider.AerialExecution)
                return GroundSlam(form,direction,model.StateTime,attack.startup,attack.active,attack.recovery);
            if(rider.Dashing&&!rider.Drifting&&!rider.Finishing&&model.StateTime>=attack.startup&&model.StateTime<attack.startup+attack.active)
                return Riding(form,direction);
            return null;
        }
        public static Sprite ResolveRequiem(DarklightAbyssRider rider,CombatModel model,int form,out bool poseFallback,Vector2 movement=default)
        {
            poseFallback=true;
            if(rider==null||model==null||!rider.Active)return null;
            int direction=DarklightCharacterView.DirectionIndex(model.Facing);
            if(!rider.MountedPresentation)
            {
                var saws=DualSawsPose(direction,model);
                if(saws!=null){poseFallback=model.State!=CombatState.Attack||direction!=0;return saws;}
            }
            if(model.State==CombatState.Neutral&&movement.sqrMagnitude<=.01f)
            {
                var idle=Idle(form,direction,model.Clock);
                if(idle!=null){poseFallback=false;return idle;}
            }
            // Only the authored complete south slam is a new animation. The other
            // techniques keep their mechanics while a coherent mounted pose is used
            // until their own Réquiem sequences are drawn; Rock art is never borrowed.
            if(form==0&&rider.Slamming&&!rider.AerialExecution&&model.State==CombatState.Attack&&model.Attack!=null)
            {
                var a=model.Attack;
                var slam=GroundSlam(0,direction,model.StateTime,a.startup,a.active,a.recovery,0);
                if(slam!=null){poseFallback=false;return slam;}
            }
            return Riding(form,direction,0)??Riding(0,direction,0);
        }
        public static bool IsAuthored(Sprite sprite)=>sprite!=null&&authored.Contains(sprite);
        public static void ApplyOwnership(SpriteRenderer renderer)
        {
            if(renderer==null||!IsAuthored(renderer.sprite))return;
            renderer.GetPropertyBlock(block);
            block.SetFloat("_WholeSprite",1);block.SetFloat("_WholeFrameId",0);block.SetFloat("_VampireForm",0);
            block.SetFloat("_DarkForm",0);block.SetFloat("_Physique",0);block.SetFloat("_LayerOcclusion",0);
            block.SetFloat("_EraseHead",0);block.SetFloat("_RuinFists",0);block.SetFloat("_AlphaClip",.1f);
            block.SetTexture("_VampireMask",Texture2D.blackTexture);block.SetVector("_CanonicalSkinShift",Vector4.zero);
            renderer.SetPropertyBlock(block);
        }
        public static void ClearCache(){Array.Clear(pages,0,pages.Length);Array.Clear(attempted,0,attempted.Length);Array.Clear(idlePages,0,idlePages.Length);idleAttempted=dualAttempted=false;dualPage=null;dualSprites.Clear();authored.Clear();}
    }
}
