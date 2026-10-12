using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightUserArtImporter
    {
        const string Root="Assets/Darklight/Art/User/";
        [Serializable] class Frame {public string name;public float[] rect,border,pivot;}
        [Serializable] class Sheet {public Frame[] frames;public float pixelsPerUnit=48;}
        public static void Apply(DarklightCampaign game)
        {
            for(int i=1;i<=6;i++)Single("Assets/Darklight/Resources/Darklight/Maps/map"+i+".png",24,false);
            Single(Root+"menuBg.png",48,false);
            game.menuBackground=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"menuBg.png");
            game.uiArt=Slice("ui",false);game.hudArt=Slice("hud",false);
            game.icaroFrames=Slice("icaro",true);game.lyraFrames=Slice("lyra",true);
            game.extras[0]=game.icaroFrames[0];game.extras[1]=game.lyraFrames[0];
            game.rockFormFrames=Slice("rockForms",true);game.rockIdle=Slice("skinIdle",true);game.rockMotion=Slice("skinMotion",true);game.rockActions=Slice("skinActions",true);game.rockSlash=Slice("skinSlash",true);
            // The source form atlas has seven distinct directions. Retain the correct
            // east-facing Rock body and apply form effects instead of turning it northeast.
            for(int form=0;form<3;form++)game.rockFormFrames[6*3+form]=game.rockIdle[6];
        }
        static TextureImporter Configure(string path,float ppu,bool transparent,SpriteImportMode mode)
        {
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing user art: "+path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=mode;
            importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=transparent;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;importer.isReadable=false;importer.SaveAndReimport();return importer;
        }
        static void Single(string path,float ppu,bool transparent)=>Configure(path,ppu,transparent,SpriteImportMode.Single);
        public static Sprite[] Slice(string name,bool actor)
        {
            string path=Root+name+".png";var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(Root+name+"Rects.json"));
            var importer=Configure(path,actor?sheet.pixelsPerUnit:48,true,SpriteImportMode.Multiple);
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite|EEditCapability.EditSpriteRect|EEditCapability.EditPivot|EEditCapability.EditSpriteName|EEditCapability.EditBorder))throw new InvalidOperationException("Sprite editing capability unavailable");
            var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(names==null)throw new InvalidOperationException("Sprite identity provider unavailable");
            var previous=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
            int height=AssetDatabase.LoadAssetAtPath<Texture2D>(path).height;
            var rects=sheet.frames.Select(f=>new SpriteRect{name=f.name,spriteID=previous.TryGetValue(f.name,out var id)?id:GUID.Generate(),rect=new Rect(f.rect[0],height-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]),alignment=SpriteAlignment.Custom,pivot=f.pivot?.Length==2?new Vector2(f.pivot[0],f.pivot[1]):actor?new Vector2(.5f,0):Vector2.one*.5f,border=new Vector4(f.border[0],f.border[1],f.border[2],f.border[3])}).ToArray();
            provider.SetSpriteRects(rects);names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
            return sheet.frames.Select(f=>sprites[f.name]).ToArray();
        }
    }
}
