using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightPlaytestBuilder
    {
        public static void Build()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/teste-vampiro/Windows"));Directory.CreateDirectory(folder);
            var paths=Array.ConvertAll(Array.FindAll(EditorBuildSettings.scenes,s=>s.enabled),s=>s.path);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=paths,locationPathName=Path.Combine(folder,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Path.Combine(folder,"resultado-build.txt"),$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nwarnings={report.summary.totalWarnings}\nbytes={report.summary.totalSize}\ntime={report.summary.totalTime}");
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Playtest build failed");
        }
    }
}
