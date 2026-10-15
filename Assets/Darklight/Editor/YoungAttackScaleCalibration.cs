using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
namespace Darklight.Editor
{
    public static class YoungAttackScaleCalibration
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/young-only-20261014/";
        const string Manifest="Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json";
        [Serializable]class Data{public Page[] pages;}
        [Serializable]class Page{public string name;public float previousPPU,ppu,factor;}
        [Serializable]class Result{public bool success,sourcePixelsPreserved,spriteIdentitiesPreserved;public int pages;}
        static string Hash(string path)=>Convert.ToBase64String(SHA256.Create().ComputeHash(File.ReadAllBytes(path)));
        public static string Apply()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new Exception("Editor must be idle");
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Work+"attack-calibration.json"));
            if(data.pages.Length!=11||data.pages.Any(p=>!p.name.Contains("Attack")||!p.name.Contains("Young")||p.factor<.7f||p.factor>1.4f||Path.GetFileName(p.name)!=p.name))throw new Exception("Unexpected calibration input");
            var inputs=new List<(Page page,TextureImporter importer,string path,string hash,string[] ids)>();
            foreach(var p in data.pages)
            {
                string path="Assets/Darklight/Resources/Darklight/TodayCharacterAnimations/"+p.name+".png";
                var ti=(TextureImporter)AssetImporter.GetAtPath(path);var factories=new SpriteDataProviderFactories();factories.Init();var provider=factories.GetSpriteEditorDataProviderFromObject(ti);if(provider==null)throw new Exception("Sprite provider missing");provider.InitSpriteEditorDataProvider();
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect))throw new Exception("Sprite calibration capability unavailable");
                inputs.Add((p,ti,path,Hash(path),provider.GetSpriteRects().OrderBy(s=>s.name).Select(s=>s.name+":"+s.spriteID).ToArray()));
            }
            var result=new Result{sourcePixelsPreserved=true,spriteIdentitiesPreserved=true};
            foreach(var i in inputs)
            {
                i.importer.spritePixelsPerUnit=i.page.ppu;i.importer.SaveAndReimport();
                var factories=new SpriteDataProviderFactories();factories.Init();var provider=factories.GetSpriteEditorDataProviderFromObject(i.importer);provider.InitSpriteEditorDataProvider();
                if(Hash(i.path)!=i.hash)throw new Exception("Source image changed");if(!provider.GetSpriteRects().OrderBy(s=>s.name).Select(s=>s.name+":"+s.spriteID).SequenceEqual(i.ids))throw new Exception("Sprite identities changed");result.pages++;
            }
            var manifest=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText(Manifest));
            foreach(var p in data.pages){var page=manifest.pages.Single(x=>x.name==p.name);page.ppu=p.ppu;}
            File.WriteAllText(Manifest,JsonUtility.ToJson(manifest,true));AssetDatabase.ImportAsset(Manifest);result.success=true;File.WriteAllText(Work+"attack-calibration-applied.json",JsonUtility.ToJson(result,true));return "Calibrated11 complete attack sheets; source pixels and sprite identities preserved";
        }
    }
}
