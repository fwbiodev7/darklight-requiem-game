using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight.Editor
{
    public static class DarklightRemasterBuilder
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-production-20261014/";
        [Serializable] sealed class Result{public bool success;public int errors,warnings;public ulong bytes;public double seconds;public string executable;}
        public static string Build()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Darklight/Resources/Darklight/Rendering/Pipeline.asset");
            if(pipeline==null||GraphicsSettings.defaultRenderPipeline!=pipeline||Enumerable.Range(0,QualitySettings.names.Length).Any(i=>QualitySettings.GetRenderPipelineAssetAt(i)!=pipeline))throw new InvalidOperationException("Production renderer must be assigned to every quality");
            var catalog=AssetDatabase.LoadAssetAtPath<TodayCharacterCatalog>("Assets/Darklight/Resources/Darklight/TodayCharacterCatalog.asset");
            if(catalog.pages.Length!=55||catalog.pages.Any(p=>p.age!=0||!p.name.Contains("Young")))throw new InvalidOperationException("Fixed young character coverage required");
            for(int chapter=1;chapter<=10;chapter++)if(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Darklight/Resources/Darklight/Loading/preview"+chapter+".png")==null)throw new InvalidOperationException("Dedicated loading illustrations required");
            var old=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Darklight/Resources"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>(p.Contains("Rock")||p.Contains("Requiem"))&&System.Text.RegularExpressions.Regex.IsMatch(p,"Stage[1-4]"));if(old.Any())throw new InvalidOperationException("Retired age art remains");
            string output="C:/Users/Usuario/Documents/Codex/2026-10-10/f/outputs/remaster-20261014/check-"+DateTime.Now.ToString("HHmmss")+"/Windows";Directory.CreateDirectory(output);
            string executable=Path.Combine(output,"DarklightRequiem.exe");File.WriteAllText(Work+"build-running.json","{\"executable\":\""+executable.Replace("\\","/")+"\"}");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=executable,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
            var result=new Result{success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,executable=executable};string json=JsonUtility.ToJson(result,true);File.WriteAllText(Work+"build-result.json",json);if(!result.success)throw new InvalidOperationException("Remaster build failed");return json;
        }
    }
}
