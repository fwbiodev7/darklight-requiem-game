using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightDetailedCombatImporter
    {
        const string Path="Assets/Darklight/ArtSources/DetailedCombat20261017/manifest.json";
        [Serializable] public sealed class Manifest
        {
            public TodayCharacterImporter.Page[] pages;
            public DarklightLivingIdleImporter.FormPage[] forms;
        }
        public static void ImportIfPresent(){if(File.Exists(Path))Import();}
        [MenuItem("DARKLIGHT/Arte/Importar golpes detalhados completos")]
        public static void Import()
        {
            var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path));
            if(data?.pages==null||data.forms==null)throw new InvalidOperationException("Missing detailed combat manifest");
            foreach(var page in data.pages)
                if(string.IsNullOrEmpty(page.attackKey)||page.kind!="Attack"||
                    !data.forms.Where(f=>f.baseName==page.name).Select(f=>f.form).OrderBy(f=>f).SequenceEqual(new[]{1,2,3}))
                    throw new InvalidOperationException("A specific individual strike requires all three complete forms: "+page.name);
            foreach(var group in data.pages.GroupBy(p=>(p.skin,p.weapon,p.attackKey)))
                if(!group.SelectMany(p=>p.directions).OrderBy(d=>d).SequenceEqual(Enumerable.Range(0,8)))
                    throw new InvalidOperationException("A detailed strike must cover all eight directions without duplicates");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var incoming=data.pages.Select(p=>
            {
                var page=new TodayCharacterPage{name=p.name,kind=p.kind,attackKey=p.attackKey,skin=p.skin,age=0,weapon=p.weapon,
                    phases=p.phases,directions=p.directions,ownerMask=true,startupFrameEnd=p.startupFrameEnd,activeFrameEnd=p.activeFrameEnd,
                    spriteNames=p.sprites.Select(s=>s.name).ToArray(),resourcePath="Darklight/TodayCharacterAnimations/"+p.name,
                    paletteResourcePath="Darklight/TodayCharacterAnimations/"+p.name+"_Palette"};
                if(!TodayCharacterBank.ValidDescriptor(page))throw new InvalidOperationException("Invalid detailed strike: "+p.name);
                TodayCharacterImporter.ImportTextureFrames("Assets/Darklight/Resources/"+page.resourcePath+".png",p.ppu,p.sprites,
                    "Assets/Darklight/Resources/"+page.paletteResourcePath+".png",true);return page;
            }).ToArray();
            var forms=data.forms.Select(p=>
            {
                string resource="Darklight/TransformedYoung/"+p.name;
                TodayCharacterImporter.ImportTextureFrames("Assets/Darklight/Resources/"+resource+".png",p.ppu,p.sprites,
                    "Assets/Darklight/Resources/"+resource+"_Owner.png",true);
                return new DarklightTransformedPage{baseName=p.baseName,form=p.form,resourcePath=resource,palettePath=resource+"_Owner",
                    spriteNames=p.sprites.Select(s=>s.name).ToArray(),frameIds=Enumerable.Range(1,p.sprites.Length).ToArray()};
            }).ToArray();
            var normal=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");var transformed=Resources.Load<DarklightTransformedCatalog>("Darklight/TransformedCatalog");
            if(normal==null||transformed==null)throw new InvalidOperationException("Production catalogs missing");
            var removed=normal.pages.Where(p=>incoming.Any(n=>n.skin==p.skin&&n.weapon==p.weapon&&n.kind==p.kind&&n.attackKey==p.attackKey)).Select(p=>p.name).ToHashSet();
            normal.pages=normal.pages.Where(p=>!removed.Contains(p.name)).Concat(incoming).ToArray();
            var names=incoming.Select(p=>p.name).ToHashSet();
            transformed.pages=transformed.pages.Where(p=>!removed.Contains(p.baseName)&&!names.Contains(p.baseName)).Concat(forms).ToArray();
            EditorUtility.SetDirty(normal);EditorUtility.SetDirty(transformed);AssetDatabase.SaveAssets();
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();DarklightCanonicalComplexion.ClearCache();
            Debug.Log("Detailed combat imported: "+incoming.Length+" pages / "+forms.Length+" form pages. Generic attacks preserved.");
        }
    }
}
