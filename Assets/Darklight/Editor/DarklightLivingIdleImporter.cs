using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightLivingIdleImporter
    {
        const string ManifestPath="Assets/Darklight/ArtSources/LivingIdle20261017/manifest.json";
        [Serializable] public sealed class Manifest
        {
            public TodayCharacterImporter.Page[] pages;
            public FormPage[] forms;
        }
        [Serializable] public sealed class FormPage
        {
            public string name,baseName;public int form;public float ppu;
            public TodayCharacterImporter.Frame[] sprites;
        }
        public static void ImportIfPresent(){if(File.Exists(ManifestPath))Import();}
        [MenuItem("DARKLIGHT/Arte/Importar idles vivos aprovados")]
        public static void Import()
        {
            ImportManifest(ManifestPath);
        }
        public static void ImportManifest(string path)
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            if(manifest?.pages==null || manifest.forms==null)throw new InvalidOperationException("Living idle manifest missing");
            foreach(var page in manifest.pages)
                if(manifest.forms.Count(f=>f.baseName==page.name)!=3 ||
                    !manifest.forms.Where(f=>f.baseName==page.name).Select(f=>f.form).OrderBy(f=>f).SequenceEqual(new[]{1,2,3}))
                    throw new InvalidOperationException("Every new idle requires all three complete forms: "+page.name);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var incoming=manifest.pages.Select(p=>
            {
                var d=new TodayCharacterPage{name=p.name,kind=p.kind,skin=p.skin,age=0,weapon=p.weapon,
                    phases=p.phases,directions=p.directions,idleBlinkFrame=p.idleBlinkFrame,ownerMask=true,
                    spriteNames=p.sprites.Select(f=>f.name).ToArray(),resourcePath="Darklight/TodayCharacterAnimations/"+p.name,
                    paletteResourcePath="Darklight/TodayCharacterAnimations/"+p.name+"_Palette"};
                if(!TodayCharacterBank.ValidDescriptor(d))throw new InvalidOperationException("Invalid new idle: "+p.name);
                TodayCharacterImporter.ImportTextureFrames("Assets/Darklight/Resources/"+d.resourcePath+".png",p.ppu,p.sprites,
                    "Assets/Darklight/Resources/"+d.paletteResourcePath+".png",true);
                return d;
            }).ToArray();
            var formDescriptors=manifest.forms.Select(p=>
            {
                string resource="Darklight/TransformedYoung/"+p.name;
                TodayCharacterImporter.ImportTextureFrames("Assets/Darklight/Resources/"+resource+".png",p.ppu,p.sprites,
                    "Assets/Darklight/Resources/"+resource+"_Owner.png",true);
                return new DarklightTransformedPage{baseName=p.baseName,form=p.form,resourcePath=resource,
                    palettePath=resource+"_Owner",spriteNames=p.sprites.Select(f=>f.name).ToArray(),
                    frameIds=Enumerable.Range(1,p.sprites.Length).ToArray()};
            }).ToArray();
            var normal=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            var transformed=Resources.Load<DarklightTransformedCatalog>("Darklight/TransformedCatalog");
            if(normal==null || transformed==null)throw new InvalidOperationException("Existing production catalogs must be present");
            // Only replace the neutral katana idle descriptors. Combat/death pages remain intact.
            var names=manifest.pages.Select(p=>p.name).ToHashSet();
            var removed=normal.pages.Where(p=>incoming.Any(n=>n.skin==p.skin&&n.weapon==p.weapon&&n.kind==p.kind)).Select(p=>p.name).ToHashSet();
            // Re-composing the legacy form catalog must not leave orphaned old
            // idle forms after the production normal pages were already replaced.
            var legacy=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText("Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json"));
            foreach(var p in legacy.pages)
                if(incoming.Any(n=>n.skin==p.skin&&n.weapon==p.weapon&&n.kind==p.kind))removed.Add(p.name);
            normal.pages=normal.pages.Where(p=>!removed.Contains(p.name)&&!names.Contains(p.name)).Concat(incoming).ToArray();
            transformed.pages=transformed.pages.Where(p=>!removed.Contains(p.baseName)&&!names.Contains(p.baseName)).Concat(formDescriptors).ToArray();
            EditorUtility.SetDirty(normal);EditorUtility.SetDirty(transformed);AssetDatabase.SaveAssets();
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();
            DarklightCanonicalComplexion.ClearCache();
            Debug.Log("Living idle import: "+incoming.Length+" normal pages, "+formDescriptors.Length+" form pages; combat and death unchanged.");
        }
    }
}
