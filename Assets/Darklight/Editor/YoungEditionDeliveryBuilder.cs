using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Darklight.Editor
{
    public static class YoungEditionDeliveryBuilder
    {
        public static string Build()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            if(GraphicsSettings.defaultRenderPipeline!=null||QualitySettings.renderPipeline!=null)throw new InvalidOperationException("Prototype must restore Built-in before production build");
            var catalog=AssetDatabase.LoadAssetAtPath<TodayCharacterCatalog>("Assets/Darklight/Resources/Darklight/TodayCharacterCatalog.asset");
            if(catalog.pages.Length!=55||catalog.pages.Any(p=>p.age!=0||!p.name.Contains("Young")))throw new InvalidOperationException("Expected fixed young character coverage");
            var old=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Darklight/Resources"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>(p.Contains("Rock")||p.Contains("Requiem"))&&System.Text.RegularExpressions.Regex.IsMatch(p,"Stage[1-4]"));
            if(old.Any())throw new InvalidOperationException("Retired age texture remains: "+string.Join(",",old));
            string output="C:/Users/Usuario/Documents/Codex/2026-10-10/f/outputs/young-edition-20261014/Windows";Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=Path.Combine(output,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            string result=JsonUtility.ToJson(new Result{success=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,path=output},true);
            File.WriteAllText("C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/young-only-20261014/build-result.json",result);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Young edition build failed");return result;
        }
        [Serializable] class Result{public bool success;public int errors,warnings;public ulong bytes;public double seconds;public string path;}
    }
}
