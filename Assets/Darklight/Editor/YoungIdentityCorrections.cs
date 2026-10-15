using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class YoungIdentityCorrections
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/young-identity-corrections-20261014/";
        const string Root="Assets/Darklight/Resources/Darklight/TodayCharacterAnimations/";
        [Serializable]sealed class Manifest{public Page[] pages;}
        [Serializable]sealed class Page{public string name,baseName;public float ppu;public Frame[] sprites;}
        [Serializable]sealed class Frame{public string name;public int x,y,w,h,normalIndex,ownerId;public float pivotX,pivotY;}
        public static string Apply()
        {
            if(EditorApplication.isCompiling||EditorApplication.isPlaying||BuildPipeline.isBuildingPlayer)throw new InvalidOperationException("Editor must be idle");
            var prepared=JsonUtility.FromJson<Manifest>(File.ReadAllText(Work+"prepared/manifest.json"));
            const string path="Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json";
            if(!File.Exists(Work+"metadata-before.json"))File.Copy(path,Work+"metadata-before.json");
            var source=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText(path));
            Directory.CreateDirectory(Work+"originals");
            foreach(var page in prepared.pages)
            {
                var target=source.pages.Single(p=>p.name==page.baseName);if(page.sprites.Length!=target.sprites.Length||page.sprites.Where((s,i)=>s.ownerId!=i+1).Any())throw new InvalidOperationException("Original frame order must be retained");
                foreach(string suffix in new[]{".png","_Palette.png"})if(!File.Exists(Work+"originals/"+page.baseName+suffix))File.Copy(Root+page.baseName+suffix,Work+"originals/"+page.baseName+suffix);
                File.Copy(Work+"prepared/"+page.name+".png",Root+page.baseName+".png",true);
                File.Copy(Work+"prepared/"+page.name+"_Owner.png",Root+page.baseName+"_Palette.png",true);
                target.ppu=page.ppu;target.ownerMask=true;
                target.sprites=page.sprites.Select((s,i)=>new TodayCharacterImporter.Frame{name="Today_"+page.baseName+"_"+i,x=s.x,y=s.y,w=s.w,h=s.h,pivotX=s.pivotX,pivotY=s.pivotY}).ToArray();
            }
            File.WriteAllText(path,JsonUtility.ToJson(source,true));AssetDatabase.Refresh();
            string result=TodayCharacterImporter.Import();File.WriteAllText(Work+"import-result.json",JsonUtility.ToJson(source,true));return result;
        }
    }
}
