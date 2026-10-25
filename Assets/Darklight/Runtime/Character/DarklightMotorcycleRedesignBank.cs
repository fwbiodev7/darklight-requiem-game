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
        }
        sealed class Page {public Sprite[] ride,slam;}
        static readonly Page[] pages=new Page[4];
        static readonly bool[] attempted=new bool[4];
        static readonly HashSet<Sprite> authored=new();
        static readonly MaterialPropertyBlock block=new();
        public static string Resource(int form)=>DarklightWeaponRedesignBank.Root+"RockMotoForm"+form+"Ride8SlamS20261025V2";
        static Page Load(int form)
        {
            if(form<0||form>3)return null;
            if(attempted[form])return pages[form];attempted[form]=true;
            var text=Resources.Load<TextAsset>(Resource(form));if(text==null)return null;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);var entry=sheet?.motorcycle;
            if(sheet==null||!sheet.wholeFigures||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified"||
                sheet.nativeSize?.Length!=2||sheet.pixelsPerUnit<=0||float.IsNaN(sheet.pixelsPerUnit)||float.IsInfinity(sheet.pixelsPerUnit)||
                entry==null||entry.form!=form||entry.skin!=1||!entry.staticRideViews||entry.ride?.Length!=8||entry.slam?.Length!=4)return null;
            var sprites=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(Resource(form)))
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
            pages[form]=page;foreach(var sprite in page.ride)authored.Add(sprite);foreach(var sprite in page.slam)authored.Add(sprite);
            return page;
        }
        public static Sprite Riding(int form,int direction)=>direction<0||direction>7?null:Load(form)?.ride[direction];
        public static Sprite GroundSlam(int form,int direction,float seconds,float startup,float active,float recovery)
        {
            if(direction!=0||seconds<0||startup<=0||active<=0||recovery<=0)return null;
            var page=Load(form);if(page==null)return null;
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
        public static void ClearCache(){Array.Clear(pages,0,pages.Length);Array.Clear(attempted,0,attempted.Length);authored.Clear();}
    }
}
