using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightTransformedComposer
    {
        static string Work=>Path.GetFullPath("work/transformed-composition")+Path.DirectorySeparatorChar;
        [Serializable]sealed class Pending{public string[] missing;}
        static DarklightTransformedFrameReference Ref(DarklightTransformedPage page,int index)=>new(){resourcePath=page.resourcePath,palettePath=page.palettePath,spriteName=page.spriteNames[index],ownerId=page.frameIds[index]};
        public static string Compose()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            Directory.CreateDirectory(Work);
            var normal=AssetDatabase.LoadAssetAtPath<TodayCharacterCatalog>("Assets/Darklight/Resources/Darklight/TodayCharacterCatalog.asset");
            var raw=AssetDatabase.LoadAssetAtPath<DarklightTransformedCatalog>("Assets/Darklight/Editor/TransformedRawCatalog.asset");
            var catalog=AssetDatabase.LoadAssetAtPath<DarklightTransformedCatalog>("Assets/Darklight/Resources/Darklight/TransformedCatalog.asset");
            if(raw==null||normal==null||catalog==null)throw new InvalidOperationException("Whole figure catalogs required");
            var missing=new List<string>();var pages=new List<DarklightTransformedPage>();
            DarklightTransformedPage Find(string name,int form){var page=raw.pages.FirstOrDefault(p=>p.baseName==name&&p.form==form);if(page==null)missing.Add(name+":"+form);return page;}
            var authored=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText("Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json")).pages;
            foreach(var page in authored)for(int form=1;form<=3;form++)
            {
                if(page.kind!="Combat"&&page.kind!="GuardDodge"){var complete=Find(page.name,form);if(complete!=null)pages.Add(complete);continue;}
                string prefix=page.skin==0?"Requiem":"Rock",weapon=new[]{"Katana","Daggers","Chain"}[page.weapon];
                var refs=new DarklightTransformedFrameReference[page.sprites.Length];bool valid=true;
                for(int i=0;i<refs.Length;i++)
                {
                    int direction=page.directions[i/page.phases],phase=i%page.phases;DarklightTransformedPage source;int index;
                    if(page.kind=="Combat"&&phase>=1&&phase<=4)
                    {
                        var attack=authored.Single(p=>p.skin==page.skin&&p.weapon==page.weapon&&p.kind=="Attack"&&p.directions.Contains(direction));
                        source=Find(attack.name,form);index=Array.IndexOf(attack.directions,direction)*4+phase-1;
                    }
                    else
                    {
                        string role=page.kind=="Combat"?"CombatSupport":phase<4?"GuardSupport":"DodgeSupport";
                        source=Find(prefix+role+weapon+"Young",form);
                        index=direction*4+(page.kind=="Combat"?(phase==0?0:phase-4):phase%4);
                    }
                    if(source==null){valid=false;continue;}refs[i]=Ref(source,index);
                }
                if(valid)pages.Add(new DarklightTransformedPage{baseName=page.name,form=form,references=refs,spriteNames=refs.Select(r=>r.spriteName).ToArray(),frameIds=refs.Select(r=>r.ownerId).ToArray()});
            }
            File.WriteAllText(Work+"coverage-pending.json",JsonUtility.ToJson(new Pending{missing=missing.Distinct().ToArray()},true));
            if(missing.Count>0)throw new InvalidOperationException("Complete transformed animations still missing: "+missing.Distinct().Count());
            if(pages.Count!=165||pages.Sum(p=>p.spriteNames.Length)!=3552)throw new InvalidOperationException("Unexpected whole animation coverage");
            catalog.pages=pages.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();DarklightTransformedBank.ClearCache();
            DarklightLivingIdleImporter.ImportIfPresent();DarklightDetailedCombatImporter.ImportIfPresent();
            string result="{\"success\":true,\"pages\":165,\"wholeFrameReferences\":3552,\"bodyPartAssembly\":false}";File.WriteAllText(Work+"complete-catalog.json",result);return result;
        }
    }
}
