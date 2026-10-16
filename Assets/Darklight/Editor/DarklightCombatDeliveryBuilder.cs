using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightCombatDeliveryBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/combat-overhaul-20261015/";
        public static void Build()
        {
            DarklightAutomation.Silent=true;
            PlayerSettings.bundleVersion="1.0.1";
            foreach(var kind in new[]{"edit","final-play"})
            {
                var result=JsonUtility.FromJson<Tests>(File.ReadAllText(Work+kind+"-tests.xml.summary.json"));
                if(result.failed!=0||result.passed==0||!result.savesPreserved||!result.prefsRestored)
                    throw new InvalidOperationException("Protected validation failed: "+kind);
            }
            AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows");
            File.WriteAllText(Work+"map4-validation.txt",Beta10MapValidation.Validate());
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},
                locationPathName=Work+"Windows/DarklightRequiem.exe",
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
            bool success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
            File.WriteAllText(Work+"build-result.json",JsonUtility.ToJson(new Result{
                success=success,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,
                seconds=report.summary.totalTime.TotalSeconds,executable=Work+"Windows/DarklightRequiem.exe"},true));
            if(!success)throw new InvalidOperationException("Combat evaluation build failed");
        }
        [Serializable]sealed class Tests{public int passed,failed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string executable;}
    }
}
