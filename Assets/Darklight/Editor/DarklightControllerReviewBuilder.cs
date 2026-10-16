using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Darklight.Editor
{
    [InitializeOnLoad]public static class DarklightControllerReviewBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/avaliacao-video-20261016/";
        static DarklightControllerReviewBuilder(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer||!File.Exists(Work+"build.request"))return;
            File.Delete(Work+"build.request");Build();
        }
        public static void Build()
        {
            var checks=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+"controller-tests.xml.summary.json"));
            if(checks.passed!=2||checks.failed!=0||!checks.savesPreserved||!checks.prefsRestored)throw new InvalidOperationException("Controller review requires two protected passing checks");
            var originals=JsonUtility.FromJson<Checks>(File.ReadAllText(Work+"controller-original-tests.xml.summary.json"));
            if(originals.passed!=2||originals.failed!=0||!originals.savesPreserved||!originals.prefsRestored)throw new InvalidOperationException("Original commands and guide regression incomplete");
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be stable in Edit mode");
            string version=PlayerSettings.bundleVersion;
            try
            {
                DarklightAutomation.Silent=true;PlayerSettings.bundleVersion="1.0.5";AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Work+"Windows/DarklightRequiem.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
                bool success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
                File.WriteAllText(Work+"build-result.json",JsonUtility.ToJson(new Result{success=success,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,seconds=report.summary.totalTime.TotalSeconds,version="1.0.5",label="Beta 1.0 — correção do controle"},true));
                if(!success)throw new InvalidOperationException("Controller review Windows build failed");
            }
            finally{PlayerSettings.bundleVersion=version;AssetDatabase.SaveAssets();}
        }
        [Serializable]sealed class Checks{public int passed,failed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string version,label;}
    }
}
