using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightConsistencyImporter
    {
        public const string WalkManifest="Assets/Darklight/ArtSources/ConsistentWalk20261017/manifest.json";
        public const string AbyssManifest="Assets/Darklight/ArtSources/ConsistentAbyss20261017/manifest.json";
        public static void ImportWalkIfPresent(){if(File.Exists(WalkManifest))DarklightLivingIdleImporter.ImportManifest(WalkManifest);}
        public static void Import()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            DarklightLivingIdleImporter.Import();ImportWalkIfPresent();
            var manifest=JsonUtility.FromJson<TodayCharacterImporter.Manifest>(File.ReadAllText(AbyssManifest));
            if(manifest?.pages==null||manifest.pages.Length!=8)throw new InvalidOperationException("Eight corrected complete saw attack sheets required");
            foreach(var page in manifest.pages)
                TodayCharacterImporter.ImportTextureFrames("Assets/Darklight/Resources/Darklight/AbyssPrototype/"+page.name+".png",page.ppu,page.sprites,
                    "Assets/Darklight/Resources/Darklight/AbyssPrototype/"+page.name+"_Owner.png",true);
            DarklightAbyssDirectionalImporter.ImportIfPresent();
            DarklightAbyssSpriteBank.ClearCache();DarklightCanonicalComplexion.ClearCache();AssetDatabase.SaveAssets();
            Debug.Log("Consistency sources imported: six-phase walks and eight complete saw attack sheets.");
        }
    }
}
