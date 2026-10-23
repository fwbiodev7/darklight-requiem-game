using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    [Serializable]public sealed class DarklightTransformedFrameReference
    {
        public string resourcePath,palettePath,spriteName;public int ownerId;
    }
    [Serializable]public sealed class DarklightTransformedPage
    {
        public string baseName,resourcePath,palettePath;public int form;public string[] spriteNames;public int[] frameIds;
        public DarklightTransformedFrameReference[] references;
        [NonSerialized]public Sprite[] frames;[NonSerialized]public Texture2D mask;[NonSerialized]public bool attempted;
    }
    public sealed class DarklightTransformedCatalog:ScriptableObject {public DarklightTransformedPage[] pages;}
    public static class DarklightTransformedBank
    {
        static DarklightTransformedCatalog catalog;
        sealed class Sheet{public Dictionary<string,Sprite> frames=new();public Texture2D mask;}
        static readonly Dictionary<string,Sheet> sheets=new();
        static bool ResolveWhole(string path,string palette,string name,int id,out Sprite frame,out Texture2D mask,out int owner)
        {
            if(!sheets.TryGetValue(path,out var sheet))
            {
                sheet=new Sheet();foreach(var sprite in Resources.LoadAll<Sprite>(path))sheet.frames[sprite.name]=sprite;
                sheet.mask=Resources.Load<Texture2D>(palette);sheets.Add(path,sheet);
            }
            owner=id;mask=sheet.mask;return sheet.frames.TryGetValue(name,out frame)&&frame!=null&&mask!=null;
        }
        public static void ClearCache(){catalog=null;sheets.Clear();}
        public static bool TryResolve(Sprite normal,int form,out Sprite frame,out Texture2D mask,out int owner)
        {
            if(DarklightRuinFistsBank.TryTransform(normal,form,out frame,out mask,out owner))return true;
            frame=null;mask=null;owner=0;if(normal==null||form<=0||!normal.name.StartsWith("Today_"))return false;
            int split=normal.name.LastIndexOf('_');if(!int.TryParse(normal.name.Substring(split+1),out int index))return false;string name=normal.name.Substring(6,split-6);
            catalog??=Resources.Load<DarklightTransformedCatalog>("Darklight/TransformedCatalog");if(catalog?.pages==null)return false;
            foreach(var page in catalog.pages)
            {
                if(page.baseName!=name||page.form!=form)continue;
                if(page.references!=null&&page.references.Length>0)
                {
                    if(index<0||index>=page.references.Length)return false;var reference=page.references[index];
                    return reference!=null&&ResolveWhole(reference.resourcePath,reference.palettePath,reference.spriteName,reference.ownerId,out frame,out mask,out owner);
                }
                if(!page.attempted)
                {
                    page.attempted=true;var loaded=Resources.LoadAll<Sprite>(page.resourcePath);var lookup=new Dictionary<string,Sprite>();foreach(var sprite in loaded)lookup[sprite.name]=sprite;
                    page.frames=new Sprite[page.spriteNames.Length];for(int i=0;i<page.frames.Length;i++)if(lookup.TryGetValue(page.spriteNames[i],out var sprite))page.frames[i]=sprite;page.mask=Resources.Load<Texture2D>(page.palettePath);
                }
                if(index>=page.frames.Length||page.frames[index]==null||page.mask==null)return false;frame=page.frames[index];mask=page.mask;owner=page.frameIds[index];return true;
            }
            return false;
        }
    }
}
