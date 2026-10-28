using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Darklight.Editor
{
    // Technical runtime delivery only. Candidate preview art is not imported.
    public static class DarklightRuntimeReviewBuilder
    {
        const string Package="outputs/fidelidade-original-20261018/pacote-v6/";
        const string Destination="outputs/revisao-runtime-20261019/Windows";
        [Serializable]sealed class Result
        {
            public bool success,focusedEditModeValidated,focusedGameplayValidated,previewArtImported=false,audioAuditioned=false;
            public int errors,warnings;public double seconds;public string version,executable,scope;
        }
        static void CheckTests()
        {
            var first=XDocument.Load(Package+"QA-UNITY-EDITMODE-20261019.xml").Descendants("test-case");
            var corrected=XDocument.Load(Package+"QA-UNITY-PALETTE-20261019.xml").Descendants("test-case");
            var final=first.Concat(corrected).GroupBy(t=>(string)t.Attribute("fullname")).Select(g=>g.Last()).ToArray();
            if(final.Length!=10||final.Any(t=>(string)t.Attribute("result")!="Passed"))
                throw new InvalidOperationException("Ten focused EditMode cases must pass, including the corrected palette fixture");
            var gameplay=XDocument.Load(Package+"QA-UNITY-GAMEPLAY-20261019.xml").Root;
            if((int)gameplay.Attribute("passed")!=6||(int)gameplay.Attribute("failed")!=0||(int)gameplay.Attribute("skipped")!=0)
                throw new InvalidOperationException("Six focused current-game tests must pass");
        }
        static void CheckProtectedFiles()
        {
            string baseline=File.ReadAllText("outputs/plano-implementacao-20261017/morte-preservada-sha256.json");
            var files=Directory.GetFiles("outputs/transformacoes-animacoes-20261017","morte-*.png")
                .Concat(new[]{"Assets/Darklight/Runtime/Character/DarklightDeathReversion.cs","Assets/Darklight/Runtime/Character/DarklightOriginalVampireView.cs"}).ToArray();
            if(files.Length!=8)throw new InvalidOperationException("Exactly eight protected files required");
            using var sha=SHA256.Create();
            foreach(string path in files)
            {
                string hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();
                if(!baseline.Contains("\""+hash+"\""))throw new InvalidOperationException("Protected source changed: "+path);
            }
        }
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build requires Edit mode");
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            CheckTests();CheckProtectedFiles();
            string previous=PlayerSettings.bundleVersion;string output=Path.GetFullPath(Destination);
            try
            {
                PlayerSettings.bundleVersion="1.0.13";AssetDatabase.SaveAssets();Directory.CreateDirectory(output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},
                    locationPathName=Path.Combine(output,"DarklightRequiem.exe"),
                    target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
                CheckProtectedFiles();
                var result=new Result{success=report.summary.result==BuildResult.Succeeded,
                    errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,
                    seconds=report.summary.totalTime.TotalSeconds,version="1.0.13",executable=Path.Combine(output,"DarklightRequiem.exe"),
                    focusedEditModeValidated=true,focusedGameplayValidated=true,
                    scope="Runtime contact timing, dagger fourth strike, form-colored ribbons, stable aura/burst and duplicate-chain fix. Existing integrated art retained. Candidate preview art remains external."};
                File.WriteAllText(Path.Combine(output,"../build-result.json"),JsonUtility.ToJson(result,true));
                if(!result.success)throw new InvalidOperationException("Runtime review build failed");
            }
            finally{PlayerSettings.bundleVersion=previous;AssetDatabase.SaveAssets();}
        }
    }
}
