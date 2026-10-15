using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class YoungActionImporter
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/young-only-20261014/";
        const string Target="Assets/Darklight/Resources/Darklight/TodayCharacterAnimations/";
        const string Manifest="Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json";
        public static string Import()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            try
            {
                var original=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText(Work+"young-manifest.json"));
                var actions=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText(Work+"prepared-actions/manifest.json"));
                if(actions.pages.Any(p=>p.age!=0||!p.name.Contains("Young")))throw new InvalidOperationException("Young-only action input required");
                var pages=original.pages.Concat(actions.pages).ToArray();
                for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int d=0;d<8;d++)
                foreach(string kind in new[]{"Walk","Attack","Combat","GuardDodge"})
                    if(pages.Count(p=>p.skin==skin&&p.weapon==weapon&&p.kind==kind&&p.directions.Contains(d))!=1)throw new InvalidOperationException($"Incomplete or duplicate {skin}/{weapon}/{d}/{kind}");
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach(var page in actions.pages)foreach(string suffix in new[]{".png","_Palette.png"})
                    {
                        string name=page.name+suffix;
                        if(Path.GetFileName(name)!=name)throw new InvalidOperationException("Unsafe asset name");
                        File.Copy(Work+"prepared-actions/"+name,Target+name,true);
                    }
                    File.WriteAllText(Manifest,JsonUtility.ToJson(new TodayCharacterImporter.Manifest{pages=pages},true));
                }
                finally{AssetDatabase.StopAssetEditing();}
                string imported=TodayCharacterImporter.Import();
                File.WriteAllText(Work+"actions-imported.json",JsonUtility.ToJson(new Result{success=true,message=imported},true));return imported;
            }
            catch(Exception ex){File.WriteAllText(Work+"actions-imported.json",JsonUtility.ToJson(new Result{success=false,message=ex.ToString()},true));throw;}
        }
        [Serializable] class Result{public bool success;public string message;}
    }
}
