using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightAbyssDirectionalImporter
    {
        public const string Manifest="Assets/Darklight/ArtSources/AbyssDirectional20261018/manifest.json";
        public static void ImportIfPresent()
        {
            if(!File.Exists(Manifest))return;
            var data=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText(Manifest));
            var expected=Enumerable.Range(1,3).SelectMany(f=>new[]{"W","NW","N"}.Select(v=>"ActionF"+f+v)).ToArray();
            if(data?.pages==null||data.pages.Length!=9||!data.pages.Select(p=>p.name).OrderBy(n=>n).SequenceEqual(expected.OrderBy(n=>n)))
                throw new InvalidOperationException("Nine directional transformed action sheets required");
            const string root="Assets/Darklight/Resources/Darklight/AbyssPrototype/";
            foreach(var page in data.pages)
            {
                if(page.sprites?.Length!=12)throw new InvalidOperationException("Twelve full directional poses required: "+page.name);
                TodayCharacterImporter.ImportTextureFrames(root+page.name+".png",page.ppu,page.sprites,root+page.name+"_Owner.png",true);
            }
            foreach(var view in new[]{"W","NW","N"})
            {
                var mask=(TextureImporter)AssetImporter.GetAtPath(root+"ActionF0"+view+"_Owner.png");
                mask.textureType=TextureImporterType.Default;mask.filterMode=FilterMode.Point;mask.mipmapEnabled=false;
                mask.sRGBTexture=false;mask.alphaIsTransparency=false;mask.textureCompression=TextureImporterCompression.Uncompressed;
                mask.npotScale=TextureImporterNPOTScale.None;mask.wrapMode=TextureWrapMode.Clamp;mask.maxTextureSize=4096;mask.SaveAndReimport();
            }
            DarklightAbyssSpriteBank.ClearCache();AssetDatabase.SaveAssets();
            for(int form=0;form<4;form++)if(!DarklightAbyssSpriteBank.HasCompleteDirections(form))
                throw new InvalidOperationException("Directional action coverage incomplete for form "+form);
        }
    }
}
