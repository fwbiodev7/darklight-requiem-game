using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Approved complete eight-frame figures. Their grounded pivots and baked FX
    // own the ascent: never add body displacement, body tint or a second aura.
    public static class DarklightOfficialTransitionBank
    {
        public const string Root="Darklight/OfficialTransitions20261114/";
        static readonly string[] Names={null,"FabiusRupturaOfficial20261114","FabiusSerafimOfficial20261114","LuciusArconteOfficial20261114"};
        static readonly int[][] Durations={null,new[]{100,150,150,200,200,150,150,150},new[]{350,450,550,1050,850,600,450,300},new[]{350,400,550,900,800,600,350,250}};
        static readonly Sprite[][] Frames=new Sprite[4][];
        static readonly HashSet<Sprite> Authored=new();
        static MaterialPropertyBlock ownership;
        public static void ClearCache(){Array.Clear(Frames,0,Frames.Length);Authored.Clear();}
        public static int FrameIndex(int kind,float progress)
        {
            if(kind<1||kind>3||float.IsNaN(progress)||float.IsInfinity(progress))return -1;
            int total=0;foreach(int duration in Durations[kind])total+=duration;
            double elapsed=Mathf.Clamp01(progress)*total;
            for(int i=0;i<7;i++){if(elapsed<Durations[kind][i])return i;elapsed-=Durations[kind][i];}
            return 7;
        }
        public static bool TryPose(int kind,float progress,out Sprite sprite)
        {
            sprite=null;int index=FrameIndex(kind,progress);if(index<0)return false;
            if(Frames[kind]==null)
            {
                var loaded=Resources.LoadAll<Sprite>(Root+Names[kind]);
                if(loaded==null||loaded.Length!=8)return false;
                var named=new Dictionary<string,Sprite>(StringComparer.Ordinal);
                foreach(var frame in loaded)if(frame==null||!named.TryAdd(frame.name,frame))return false;
                var ordered=new Sprite[8];
                for(int i=0;i<8;i++)if(!named.TryGetValue("Redesign_"+Names[kind]+"_"+i.ToString("00"),out ordered[i]))return false;
                Frames[kind]=ordered;foreach(var frame in ordered)Authored.Add(frame);
            }
            sprite=Frames[kind][index];return sprite!=null;
        }
        public static bool IsAuthored(Sprite sprite)=>sprite!=null&&Authored.Contains(sprite);
        public static void ApplyOwnership(SpriteRenderer renderer)
        {
            if(renderer==null||!IsAuthored(renderer.sprite))return;
            ownership??=new MaterialPropertyBlock();renderer.GetPropertyBlock(ownership);
            ownership.SetFloat("_WholeSprite",1);ownership.SetFloat("_WholeFrameId",0);
            ownership.SetFloat("_VampireForm",0);ownership.SetFloat("_DarkForm",0);
            ownership.SetFloat("_Physique",0);ownership.SetFloat("_LayerOcclusion",0);
            ownership.SetFloat("_EraseHead",0);ownership.SetFloat("_RuinFists",0);
            ownership.SetFloat("_FormIntensity",0);ownership.SetFloat("_ResonanceTint",0);
            ownership.SetFloat("_AlphaClip",.1f);ownership.SetVector("_CanonicalSkinShift",Vector4.zero);
            ownership.SetTexture("_VampireMask",Texture2D.blackTexture);renderer.SetPropertyBlock(ownership);
            renderer.color=Color.white;renderer.flipX=renderer.flipY=false;
        }
    }
}
