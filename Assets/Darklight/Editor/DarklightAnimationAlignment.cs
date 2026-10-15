using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightAnimationAlignment
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-production-20261014/alignment/";
        [Serializable]sealed class Manifest{public Page[] pages;}
        [Serializable]sealed class Page{public string name,path;public Frame[] sprites;}
        [Serializable]sealed class Frame{public string name;public float pivotX,pivotY;}
        [Serializable]sealed class Snapshot{public List<Identity> frames=new();public int pages;}
        [Serializable]sealed class Identity{public string path,name,id,guid,hash;public Vector2 pivot;public Rect rect;}
        static string Hash(string path){using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        public static string Apply()=>ApplyFile("pivot-patches.json","native");
        public static string ApplySecondary()=>ApplyFile("secondary-pivot-patches.json","native-secondary");
        static string ApplyFile(string proposalFile,string prefix)
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            var proposal=JsonUtility.FromJson<Manifest>(File.ReadAllText(Work+proposalFile));
            var factory=new SpriteDataProviderFactories();factory.Init();var before=new Snapshot();
            // Validate every provider and sprite before applying any edit.
            var operations=new List<Action>();
            foreach(var page in proposal.pages)
            {
                var importer=AssetImporter.GetAtPath(page.path) as TextureImporter;if(importer==null)continue;
                var provider=factory.GetSpriteEditorDataProviderFromObject(importer);if(provider==null)throw new InvalidOperationException("No sprite provider: "+page.path);
                provider.InitSpriteEditorDataProvider();var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
                if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.EditPivot))throw new InvalidOperationException("Pivot editing unavailable: "+page.path);
                var rects=provider.GetSpriteRects();var lookup=page.sprites.ToDictionary(f=>f.name);
                if(rects.Length!=lookup.Count||rects.Any(r=>!lookup.ContainsKey(r.name)))throw new InvalidOperationException("Sprite identity mismatch: "+page.path);
                string hash=Hash(page.path);string guid=AssetDatabase.AssetPathToGUID(page.path);
                foreach(var rect in rects)before.frames.Add(new Identity{path=page.path,name=rect.name,id=rect.spriteID.ToString(),guid=guid,hash=hash,pivot=rect.pivot,rect=rect.rect});
                before.pages++;
                operations.Add(()=>{foreach(var rect in rects){var f=lookup[rect.name];rect.pivot=new Vector2(f.pivotX,f.pivotY);rect.alignment=SpriteAlignment.Custom;}provider.SetSpriteRects(rects);provider.Apply();importer.SaveAndReimport();});
            }
            File.WriteAllText(Work+prefix+"-before.json",JsonUtility.ToJson(before,true));
            foreach(var operation in operations)operation();
            foreach(var group in before.frames.GroupBy(f=>f.path))
            {
                var importer=AssetImporter.GetAtPath(group.Key);var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();var rects=provider.GetSpriteRects().ToDictionary(r=>r.name);
                string hash=Hash(group.Key);
                foreach(var saved in group)if(!rects.TryGetValue(saved.name,out var current)||current.spriteID.ToString()!=saved.id||current.rect!=saved.rect||AssetDatabase.AssetPathToGUID(group.Key)!=saved.guid||hash!=saved.hash)throw new InvalidOperationException("Whole-figure identity changed: "+saved.path);
                foreach(var f in proposal.pages.First(p=>p.path==group.Key).sprites)if(Vector2.Distance(rects[f.name].pivot,new Vector2(f.pivotX,f.pivotY))>.00001f)throw new InvalidOperationException("Pivot did not persist: "+f.name);
            }
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();DarklightSecondaryStyleBank.Clear();
            string result="{\"success\":true,\"pages\":"+before.pages+",\"frames\":"+before.frames.Count+",\"pngUnchanged\":true,\"spriteIdsPreserved\":true,\"rectsPreserved\":true}";File.WriteAllText(Work+prefix+"-result.json",result);return result;
        }
    }
}
