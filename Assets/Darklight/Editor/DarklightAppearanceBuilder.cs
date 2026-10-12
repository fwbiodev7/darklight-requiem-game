using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightAppearanceBuilder
    {
        public static void Apply(DarklightCampaign game,DarklightFighter player,bool rebuildBindings=false)
        {
            const string path="Assets/Darklight/Data/Appearance.asset";
            var profile=AssetDatabase.LoadAssetAtPath<CharacterAppearanceProfile>(path);
            if(profile==null)
            {
                profile=ScriptableObject.CreateInstance<CharacterAppearanceProfile>();
                profile.stages=new[]{Stage("INICIAL",0,0,0,0),Stage("DESENVOLVIMENTO",60,1,1,0),Stage("INTERMEDIARIO",120,2,2,1),Stage("AVANCADO",240,3,3,1),Stage("FINAL",365,4,4,2)};
                AssetDatabase.CreateAsset(profile,path);
            }
            profile.hair=DarklightUserArtImporter.Slice("appearanceHair",true);
            profile.windHair=DarklightUserArtImporter.Slice("appearanceWind",true);
            profile.facialHair=DarklightUserArtImporter.Slice("appearanceBeard",true);
            profile.shoulders=DarklightUserArtImporter.Slice("appearanceShoulders",true);
            var view=player.GetComponent<DarklightCharacterView>();var all=new HashSet<Sprite>();
            void Add(Sprite[] sprites){if(sprites!=null)foreach(var s in sprites)if(s!=null)all.Add(s);}
            Add(view.idle);Add(view.walk);Add(view.motion);Add(view.actions);Add(view.slash);
            Add(game.rockIdle);Add(game.rockMotion);Add(game.rockActions);Add(game.rockSlash);Add(game.formFrames);Add(game.rockFormFrames);
            foreach(var id in new[]{"Requiem","PresasDoVeu","LacoDaRuptura"}){var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/"+id+".asset");if(w!=null){Add(w.requiemPoses);Add(w.rockPoses);}}
            var images=new Dictionary<string,Texture2D>();var colors=new Dictionary<string,Color32[]>();var attachments=new List<AppearanceAttachment>();
            foreach(var s in all)
            {
                string png=AssetDatabase.GetAssetPath(s);
                if(!images.TryGetValue(png,out var image)){image=new Texture2D(2,2,TextureFormat.RGBA32,false);image.LoadImage(File.ReadAllBytes(png));images[png]=image;colors[png]=image.GetPixels32();}
                attachments.Add(FindAttachment(s,colors[png],image.width));
            }
            foreach(var image in images.Values)UnityEngine.Object.DestroyImmediate(image);
            // Preserve manually corrected attachment points when rebuilding the delivery scene.
            var previous=rebuildBindings?null:profile.attachments?.Where(a=>a.source!=null).ToDictionary(a=>a.source);
            profile.attachments=attachments.Select(a=>previous!=null&&previous.TryGetValue(a.source,out var old)?old:a).ToArray();
            var system=player.GetComponent<CharacterAppearanceSystem>()??player.gameObject.AddComponent<CharacterAppearanceSystem>();system.profile=profile;
            EditorUtility.SetDirty(profile);Debug.Log("Appearance: "+all.Count+" frame attachments, 5 stages, 100 shared layer sprites; age 17 to 18 over 365 narrative days.");
        }
        static AppearanceStage Stage(string name,int days,int hair,int beard,int physique)=>new(){name=name,minimumNarrativeDays=days,hair=hair,facialHair=beard,physique=physique};
        static AppearanceAttachment FindAttachment(Sprite sprite,Color32[] pixels,int w)
        {
            var rect=sprite.rect;
            int x0=(int)rect.xMin,x1=(int)rect.xMax,y0=(int)rect.yMin,y1=(int)rect.yMax;
            int minX=x1,maxX=x0,top=y0,faceLeft=x1,faceRight=x0,faceTop=y0,faceBottom=y1;
            // Metadata only: examine opaque brown scalp pixels in the upper third of each frame.
            for(int y=y0+(y1-y0)*2/3;y<y1;y++)for(int x=x0;x<x1;x++)
            {
                var c=pixels[y*w+x];if(c.a<200)continue;
                bool brown=c.r>28&&c.r<165&&c.r>c.g*1.12f&&c.g>c.b*1.06f&&c.g>20;
                if(brown){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);top=Mathf.Max(top,y);}
                bool skin=c.r>150&&c.g>70&&c.g<205&&c.b>30&&c.b<180&&c.r>c.g+25&&c.g>c.b+10;
                if(skin){faceLeft=Mathf.Min(faceLeft,x);faceRight=Mathf.Max(faceRight,x);faceTop=Mathf.Max(faceTop,y);faceBottom=Mathf.Min(faceBottom,y);}
            }
            float ppu=sprite.pixelsPerUnit;
            float center=faceRight>=faceLeft?(faceRight+faceLeft)*.5f:maxX>=minX?(minX+maxX)*.5f:x0+sprite.pivot.x;
            float width=maxX>=minX?(maxX-minX+1)/ppu:sprite.bounds.size.y*.32f;
            width=Mathf.Clamp(width,sprite.bounds.size.y*.18f,sprite.bounds.size.y*.44f);
            float crown=top>y0?top:y1-1;
            float lip=faceTop>faceBottom?Mathf.Lerp(faceBottom,faceTop,.28f):crown-(y1-y0)*.19f;
            Vector2 Local(float x,float y)=>new((x-x0-sprite.pivot.x)/ppu,(y-y0-sprite.pivot.y)/ppu);
            return new AppearanceAttachment{source=sprite,crown=Local(center,crown),upperLip=Local(center,lip),headWidth=width,shoulders=Local(center,crown-(y1-y0)*.28f),shoulderWidth=width*1.12f};
        }
    }
}
