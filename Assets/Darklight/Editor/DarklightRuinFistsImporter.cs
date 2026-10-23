using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    // Existing data-provider importer preserves native Unity sprite IDs on reimport.
    public static class DarklightRuinFistsImporter
    {
        const string ManifestPath="Assets/Darklight/ArtSources/RuinFists20261021/manifest.json";
        const string CatalogPath="Assets/Darklight/Resources/Darklight/RuinFists/catalog.json";
        [Serializable]public sealed class Page
        {
            public string name,sourceName,imagePath,maskPath,imageSha256,maskSha256;
            public bool paletteVariant;
            public float ppu;public int skin,form,direction;public string bank;
            public TodayCharacterImporter.Frame[] sprites;
        }
        [Serializable]public sealed class Manifest{public bool complete;public Page[] pages;}
        static string Hash(string path)
        {
            using var sha=SHA256.Create();using var stream=File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
        static void Configure(string path)
        {
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);
            if(ti==null)throw new InvalidOperationException("Texture importer missing: "+path);
            var defaults=ti.GetDefaultPlatformTextureSettings();defaults.textureCompression=TextureImporterCompression.Uncompressed;
            defaults.format=TextureImporterFormat.RGBA32;defaults.maxTextureSize=4096;ti.SetPlatformTextureSettings(defaults);
            foreach(string platform in new[]{"Standalone","Android","iPhone"})
            {
                var settings=ti.GetPlatformTextureSettings(platform);settings.textureCompression=TextureImporterCompression.Uncompressed;
                if(settings.overridden){settings.format=TextureImporterFormat.RGBA32;settings.maxTextureSize=4096;ti.SetPlatformTextureSettings(settings);}
            }
            ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.npotScale=TextureImporterNPOTScale.None;
            ti.SaveAndReimport();
        }
        [MenuItem("DARKLIGHT/Arte/Importar Punhos da Ruína completos")]
        public static void Import()
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if(manifest?.complete!=true||manifest.pages?.Length!=128)throw new InvalidOperationException("Punhos da Ruína require 128 complete skin/form/direction/action banks");
            var catalog=new DarklightRuinFistsCatalog{complete=true,pages=manifest.pages.Select(p=>new DarklightRuinFistsPage{
                skin=p.skin,form=p.form,direction=p.direction,bank=p.bank,paletteVariant=p.paletteVariant,
                resourcePath="Darklight/RuinFists/"+(p.paletteVariant?p.sourceName:p.name),
                palettePath="Darklight/RuinFists/"+(p.paletteVariant?p.sourceName:p.name)+"_Owner",spriteNames=p.sprites.Select(f=>f.name).ToArray()}).ToArray()};
            if(!DarklightRuinFistsBank.Complete(catalog))throw new InvalidOperationException("Invalid/duplicate/missing Punhos da Ruína coverage");
            foreach(var page in manifest.pages)
            {
                string expected="Assets/Darklight/Resources/Darklight/RuinFists/"+(page.paletteVariant?page.sourceName:page.name);
                if(page.imagePath!=expected+".png"||page.maskPath!=expected+"_Owner.png"||
                   !page.paletteVariant&&(Hash(page.imagePath)!=page.imageSha256||Hash(page.maskPath)!=page.maskSha256))
                    throw new InvalidOperationException("Missing or changed complete-figure source: "+page.name);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            const string sigil="Assets/Darklight/Resources/Darklight/HUD/StyleSigil.png";
            var emblem=(TextureImporter)AssetImporter.GetAtPath(sigil);
            if(emblem!=null){emblem.textureType=TextureImporterType.Sprite;emblem.spriteImportMode=SpriteImportMode.Single;emblem.alphaIsTransparency=true;emblem.spritePixelsPerUnit=64;emblem.SaveAndReimport();Configure(sigil);}
            foreach(var page in manifest.pages.Where(p=>!p.paletteVariant))
            {
                TodayCharacterImporter.ImportTextureFrames(page.imagePath,page.ppu,page.sprites,page.maskPath,true);
                Configure(page.imagePath);Configure(page.maskPath);
            }
            // Publish the activation descriptor only after every page imported successfully.
            File.WriteAllText(CatalogPath,JsonUtility.ToJson(catalog,true)+"\n");
            AssetDatabase.ImportAsset(CatalogPath,ImportAssetOptions.ForceSynchronousImport);
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();DarklightCanonicalComplexion.ClearCache();
            if(!DarklightRuinFistsBank.Ready)throw new InvalidOperationException("Imported catalog did not activate");
            var weapon=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/LacoDaRuptura.asset");
            if(weapon!=null)
            {
                weapon.displayName=DarklightRuinFistsProfile.DisplayName;weapon.icon=DarklightRuinFistsBank.Idle(0,0,0);
                weapon.requiemPoses=new[]{DarklightRuinFistsBank.Idle(0,0,0)};weapon.rockPoses=new[]{DarklightRuinFistsBank.Idle(1,0,0)};
                EditorUtility.SetDirty(weapon);AssetDatabase.SaveAssets();
            }
            Debug.Log("Punhos da Ruína imported: 128 skin/form/direction/bank configurations; normal/crimson/violet share authored whole-frame geometry with semantic palettes, white has separate hair artwork. Other weapons/catalogs preserved.");
        }
    }
}
