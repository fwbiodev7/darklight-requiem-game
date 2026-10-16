using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightDialogueDeliveryBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/dialogo-textura-20261015/";
        public static void Build()
        {
            var tests=JsonUtility.FromJson<Tests>(File.ReadAllText(Work+"dialogue-tests.xml.summary.json"));
            if(tests.failed!=0||tests.passed!=4||!tests.savesPreserved||!tests.prefsRestored)throw new InvalidOperationException("Dialogue validation incomplete");
            DarklightAutomation.Silent=true;PlayerSettings.bundleVersion="1.0.2";AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Work+"Windows/DarklightRequiem.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
            bool success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
            File.WriteAllText(Work+"build-result.json",JsonUtility.ToJson(new Result{success=success,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,seconds=report.summary.totalTime.TotalSeconds},true));
            if(!success)throw new InvalidOperationException("Dialogue build failed");
        }
        [Serializable]sealed class Tests{public int failed,passed;public bool savesPreserved,prefsRestored;}
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;}
    }
}
