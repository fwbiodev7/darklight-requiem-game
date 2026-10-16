using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Darklight.Editor
{
    [InitializeOnLoad]public static class DarklightMapsDeliveryBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/mapas-reforma-20261016/";
        static DarklightMapsDeliveryBuilder(){EditorApplication.update+=Poll;}
        static void Poll(){if(EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer||!File.Exists(Work+"build.request"))return;File.Delete(Work+"build.request");Build();}
        static void Check(string name,int expected=0)
        {
            var c=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+name));
            if(c.passed<1||expected>0&&c.passed!=expected||c.failed!=0||!c.savesPreserved||!c.prefsRestored)throw new InvalidOperationException("Incomplete protected validation: "+name);
        }
        public static void Build()
        {
            Check("map8/map8-tests.xml.summary.json",6);Check("map9/final-validated-tests.xml.summary.json",6);Check("map10/map10-tests.xml.summary.json",6);Check("final-edit-tests.xml.summary.json");Check("combat-regression-tests.xml.summary.json");
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Stable Edit mode required");
            string version=PlayerSettings.bundleVersion;
            try
            {
                DarklightAutomation.Silent=true;PlayerSettings.bundleVersion="1.0.6";AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows");
                var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Work+"Windows/DarklightRequiem.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
                bool ok=r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
                File.WriteAllText(Work+"build-result.json",JsonUtility.ToJson(new Result{success=ok,errors=r.summary.totalErrors,warnings=r.summary.totalWarnings,seconds=r.summary.totalTime.TotalSeconds,version="1.0.6",label="Beta 1.0 — Cavaleiro e fases 7–10"},true));if(!ok)throw new InvalidOperationException("Windows delivery build failed");
            }
            finally{PlayerSettings.bundleVersion=version;AssetDatabase.SaveAssets();}
        }
        [Serializable]sealed class Checks{public int passed,failed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string version,label;}
    }
}
