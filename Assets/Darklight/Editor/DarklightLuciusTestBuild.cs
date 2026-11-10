using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightLuciusTestBuild
    {
        [Serializable] sealed class Receipt
        {
            public string result,path,unityVersion,createdUtc;
            public int errors,warnings;
            public ulong bytes;
            public double seconds;
        }
        public static void Build()
        {
            DarklightAutomation.Silent = true;
            AudioListener.pause = true;
            AudioListener.volume = 0;

            string destination="outputs/lucius-test-build-20261110/Windows";
            string[] args=Environment.GetCommandLineArgs();
            int outputArg=Array.IndexOf(args,"-darklightLuciusOutput");
            if(outputArg>=0&&outputArg+1<args.Length)destination=args[outputArg+1];
            string outputDir = Path.GetFullPath(destination);
            Directory.CreateDirectory(outputDir);
            string exePath = Path.Combine(outputDir, "DarklightRequiem.exe");

            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Darklight/Scenes/DarklightCampaign.unity",
                    "Assets/Darklight/Scenes/DarklightCombatLab.unity"
                },
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4
            };

            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();

            Debug.Log("[LuciusTestBuild] Building standalone Windows player to: " + exePath);
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            string resultText = $"Result: {summary.result}\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\nBytes: {summary.totalSize}\nTime: {summary.totalTime.TotalSeconds:F1}s\nPath: {exePath}";
            Debug.Log("[LuciusTestBuild] " + resultText);
            File.WriteAllText(Path.Combine(outputDir, "build-result.txt"), resultText);
            File.WriteAllText(Path.Combine(outputDir,"build-result.json"),JsonUtility.ToJson(new Receipt{
                result=summary.result.ToString(),path=exePath,errors=(int)summary.totalErrors,warnings=(int)summary.totalWarnings,
                bytes=summary.totalSize,seconds=summary.totalTime.TotalSeconds,unityVersion=Application.unityVersion,
                createdUtc=DateTime.UtcNow.ToString("O")},true));

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Build failed with result: " + summary.result + " (errors: " + summary.totalErrors + ")");
            }
        }
    }
}

