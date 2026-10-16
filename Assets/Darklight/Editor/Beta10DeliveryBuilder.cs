using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class Beta10DeliveryBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/beta-1.0-20261015/";
        public static void Build()
        {
            PlayerSettings.bundleVersion="1.0.0";
            for(int form=1;form<=3;form++)
            {
                var path="Assets/Darklight/Resources/Darklight/Music/"+TodayTransformationMusic.ResourceForForm(form)+".mp3";
                var importer=AssetImporter.GetAtPath(path) as AudioImporter;
                if(importer==null)throw new InvalidOperationException("Missing transformation theme: "+form);
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;
                importer.defaultSampleSettings=settings;importer.loadInBackground=true;importer.SaveAndReimport();
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if(clip==null||clip.loadType!=AudioClipLoadType.Streaming)throw new InvalidOperationException("Supplied themes must use streaming audio");
            }
            if(!File.ReadAllText("Assets/Darklight/Runtime/Campaign/DarklightCampaignMenu.cs").Contains("BETA 1.0"))throw new InvalidOperationException("Beta version label missing");
            AssetDatabase.SaveAssets();Directory.CreateDirectory(Work+"Windows");
            File.WriteAllText(Work+"map4-validation.txt",Beta10MapValidation.Validate());
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},
                locationPathName=Work+"Windows/DarklightRequiem.exe",
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
            File.WriteAllText(Work+"build-result.json",JsonUtility.ToJson(new Result{
                success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded,
                errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,
                seconds=report.summary.totalTime.TotalSeconds,executable=Work+"Windows/DarklightRequiem.exe"},true));
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Beta 1.0 build failed");
        }
        [Serializable]sealed class Result{public bool success;public int errors,warnings;public double seconds;public string executable;}
    }
}
