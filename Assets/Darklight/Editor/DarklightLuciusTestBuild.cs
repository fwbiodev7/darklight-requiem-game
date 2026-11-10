using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightLuciusTestBuild
    {
        public static void Build()
        {
            DarklightAutomation.Silent = true;
            AudioListener.pause = true;
            AudioListener.volume = 0;

            string outputDir = Path.GetFullPath("outputs/lucius-test-build-20261110/Windows");
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

            Debug.Log("[LuciusTestBuild] Building standalone Windows player to: " + exePath);
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            string resultText = $"Result: {summary.result}\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\nBytes: {summary.totalSize}\nTime: {summary.totalTime.TotalSeconds:F1}s\nPath: {exePath}";
            Debug.Log("[LuciusTestBuild] " + resultText);
            File.WriteAllText(Path.Combine(outputDir, "build-result.txt"), resultText);

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Build failed with result: " + summary.result + " (errors: " + summary.totalErrors + ")");
            }
        }
    }
}

