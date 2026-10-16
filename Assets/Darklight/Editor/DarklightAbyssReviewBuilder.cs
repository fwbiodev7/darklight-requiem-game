using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Darklight.Editor
{
    [InitializeOnLoad]public static class DarklightAbyssReviewBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/cavaleiro-sprites-20261015/";
        static DarklightAbyssReviewBuilder(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer||!File.Exists(Work+"build-20261016.request"))return;
            File.Delete(Work+"build-20261016.request");Build();
        }
        public static void Build()
        {
            var tests=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+"stability-final.xml.summary.json"));
            if(tests.failed!=0||tests.passed!=30||!tests.savesPreserved||!tests.prefsRestored)throw new InvalidOperationException("Cavaleiro review tests incomplete");
            var flow=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+"flow-gameplay-fixed.xml.summary.json"));if(flow.failed!=0||flow.passed!=1||!flow.savesPreserved||!flow.prefsRestored)throw new InvalidOperationException("Four-form real-input check incomplete");
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be stable in Edit mode");
            string previous=PlayerSettings.bundleVersion;
            try
            {
                DarklightAutomation.Silent=true;PlayerSettings.bundleVersion="1.0.4";AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows-20261016");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Work+"Windows-20261016/DarklightRequiem.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
                bool success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
                File.WriteAllText(Work+"build-result-20261016.json",JsonUtility.ToJson(new Result{success=success,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,seconds=report.summary.totalTime.TotalSeconds,version="1.0.4",label="Beta 1.0 — Cavaleiro — combos responsivos"},true));
                if(!success)throw new InvalidOperationException("Windows prototype build failed");
            }
            finally{PlayerSettings.bundleVersion=previous;AssetDatabase.SaveAssets();}
        }
        [Serializable]sealed class Checks{public int passed,failed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string version,label;}
    }
}
