using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Darklight.Editor
{
    [InitializeOnLoad]public static class DarklightImpactDeliveryBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/cavaleiro-final-20261016/";
        static DarklightImpactDeliveryBuilder(){EditorApplication.update+=Poll;}
        static void Poll(){if(EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer||!File.Exists(Work+"build.request"))return;File.Delete(Work+"build.request");Build();}
        static void Check(string name,int expected=0)
        {
            var c=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+name));
            if(c.passed<1||expected>0&&c.passed!=expected||c.failed!=0||!c.savesPreserved||!c.prefsRestored)throw new InvalidOperationException("Incomplete protected validation: "+name);
        }
        public static void Build()
        {
            Check("final-edit-tests.xml.summary.json",118);Check("impact-final-tests.xml.summary.json",29);Check("impact-capture-tests.xml.summary.json",1);
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Stable Edit mode required");
            string version=PlayerSettings.bundleVersion;
            try
            {
                DarklightAutomation.Silent=true;PlayerSettings.bundleVersion="1.0.7";AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows");
                var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Work+"Windows/DarklightRequiem.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
                bool ok=r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
                File.WriteAllText(Work+"build-result.json",JsonUtility.ToJson(new Result{success=ok,errors=r.summary.totalErrors,warnings=r.summary.totalWarnings,seconds=r.summary.totalTime.TotalSeconds,version="1.0.7",label="Beta 1.0 — impactos do Cavaleiro e revisão de avisos"},true));if(!ok)throw new InvalidOperationException("Windows delivery build failed");
            }
            finally{PlayerSettings.bundleVersion=version;AssetDatabase.SaveAssets();}
        }
        [Serializable]sealed class Checks{public int passed,failed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string version,label;}
    }
}
