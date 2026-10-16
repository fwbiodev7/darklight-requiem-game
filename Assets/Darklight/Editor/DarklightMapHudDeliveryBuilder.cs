using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Darklight.Editor
{
    [InitializeOnLoad]public static class DarklightMapHudDeliveryBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/minimapa-reforma-20261015/authorial/";
        static DarklightMapHudDeliveryBuilder(){EditorApplication.update+=Poll;}
        static void Poll(){if(EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer||!File.Exists(Work+"build.request"))return;File.Delete(Work+"build.request");Build();}
        static void Check(string name,int count)
        {
            var c=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+name));
            if(c.passed!=count||c.failed!=0||!c.savesPreserved||!c.prefsRestored)throw new InvalidOperationException("Incomplete current validation: "+name);
        }
        public static void Build()
        {
            Check("final-validated-tests.xml.summary.json",41);Check("final-edit-tests.xml.summary.json",118);Check("map-screen-r2/validated-tests-r2.xml.summary.json",10);Check("map-screen-r2/landmarks-final.xml.summary.json",1);
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Stable Edit mode required");
            string version=PlayerSettings.bundleVersion;
            try
            {
                if(PlayerSettings.productName!="DARKLIGHT REQUIEM")throw new InvalidOperationException("Original product/save path required");
                DarklightAutomation.Silent=true;PlayerSettings.bundleVersion="1.0.8";AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows-icons");
                var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Work+"Windows-icons/DarklightRequiem.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
                bool ok=r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
                File.WriteAllText(Work+"build-result-icons.json",JsonUtility.ToJson(new Result{success=ok,errors=r.summary.totalErrors,warnings=r.summary.totalWarnings,seconds=r.summary.totalTime.TotalSeconds,version="1.0.8",label="Beta 1.0 — atlas texturizado, novos ícones, checkpoints e portais maiores"},true));
                if(!ok)throw new InvalidOperationException("Windows build failed");
            }
            finally{PlayerSettings.bundleVersion=version;AssetDatabase.SaveAssets();}
        }
        [Serializable]sealed class Checks{public int passed,failed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string version,label;}
    }
}
