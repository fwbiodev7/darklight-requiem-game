using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    [InitializeOnLoad]public static class DarklightRegionReformQARunner
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/mapas-reforma-20261015/";
        static DarklightRegionReformQARunner(){EditorApplication.update+=Watch;}
        static void Watch()
        {
            string output=SessionState.GetString("Darklight.Region.QA","");
            if(output==""||SessionState.GetString("Darklight.Tests.Output","")!=""||!File.Exists(output+".summary.json"))return;
            var r=JsonUtility.FromJson<Result>(File.ReadAllText(output+".summary.json"));SessionState.EraseString("Darklight.Region.QA");EditorApplication.Exit(r.failed==0&&r.savesPreserved?0:1);
        }
        [System.Serializable]sealed class Result{public int failed;public bool savesPreserved;}
        static void Run(string stage,string pattern)
        {
            string output=Work+stage+"-tests.xml";
            if(File.Exists(output+".summary.json")){File.Copy(output+".summary.json",output+".summary.json.previous",true);File.Delete(output+".summary.json");}
            SessionState.SetString("Darklight.Region.QA",output);DarklightAutomation.Silent=true;AudioListener.volume=0;
            DarklightDeliveryTestRunner.Focused(output,pattern);
        }
        public static void Before()=>Run("before","^Darklight.Tests.DarklightReformedRegionTests.CaptureFourOriginalMaps$");
        public static void Map7()=>Run("map7","^Darklight.Tests.(DarklightReformedRegionTests.Map7.*|DarklightNarrativePlayTests.InscriptionAndAncestorMemoryAdvanceDistinctCheckpoints)$");
        public static void Map7Extra()=>Run("map7-extra","^Darklight.Tests.DarklightReformedRegionTests.Map7(AtmosphereIsOptionalBoundedSilentAndPauses|OriginalWeaponsContactEightDirectionsInNewArena)$");
        public static void ImportMap7()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(var asset in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Darklight/Resources/Darklight/Maps"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(asset);if(!Path.GetFileName(path).StartsWith("map7"))continue;
                var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.filterMode=FilterMode.Point;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=4096;t.SaveAndReimport();
            }
            Map7();
        }
    }
}
