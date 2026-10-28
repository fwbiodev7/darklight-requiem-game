using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightAnimationReviewBuilder
    {
        [Serializable] sealed class Result
        {
            public bool success,playModeValidated=false,audioAuditioned=false;
            public string version,executable;public int errors,warnings;public double seconds;
        }
        public static void Build()=>BuildVersion("1.0.10","outputs/revisao-animacoes-20261017/Windows",false);
        public static void BuildCrystals()=>BuildVersion("1.0.11","outputs/restauracao-cristais-20261017/Windows",true);
        public static void BuildConsistency()
        {
            foreach(string path in new[]{"outputs/revisao-consistencia-20261017/consistency-editmode.xml","outputs/validacao-pendencias-20261017/consistency-playmode.xml"})
            {
                var result=XDocument.Load(path).Root;
                if((int)result.Attribute("failed")!=0||(int)result.Attribute("passed")<1||(int)result.Attribute("skipped")!=0)
                    throw new InvalidOperationException("Consistency validation has not passed: "+path);
            }
            BuildVersion("1.0.12","outputs/revisao-consistencia-20261017/Windows",true);
        }
        public static void BuildTechnicalFinal()
        {
            foreach(string path in new[]{"outputs/finalizacao-20261018/technical-editmode.xml","outputs/validacao-pendencias-20261017/technical-final-20261018.xml"})
            {
                var check=XDocument.Load(path).Root;
                if((int)check.Attribute("failed")!=0||(int)check.Attribute("passed")<1||(int)check.Attribute("skipped")!=0)
                    throw new InvalidOperationException("Final technical checks have not passed: "+path);
            }
            DarklightAbyssDirectionalImporter.ImportIfPresent();
            for(int form=0;form<4;form++)if(!DarklightAbyssSpriteBank.HasCompleteForm(form))throw new InvalidOperationException("Incomplete saw form: "+form);
            BuildVersion("1.0.12","outputs/finalizacao-20261018/Windows",true);
        }
        static void BuildVersion(string version,string destination,bool requirePlayMode)
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            foreach(string name in new[]{"testes-unity-integracao-idles.xml","testes-unity-audio-revisao.xml","testes-unity-apresentacao-final.xml"})
            {
                var result=XDocument.Load("outputs/implementacao-fase2-20261017/"+name).Root;
                if((int)result.Attribute("failed")!=0||(int)result.Attribute("passed")<1||(int)result.Attribute("skipped")!=0)
                    throw new InvalidOperationException("Required focused tests have not passed: "+name);
            }
            if(requirePlayMode)
            {
                foreach(string path in new[]{"outputs/restauracao-cristais-20261017/cristais-integracao.xml","outputs/validacao-pendencias-20261017/delivery-playmode.xml","outputs/validacao-pendencias-20261017/lit-palette.xml"})
                {
                    var check=XDocument.Load(path).Root;
                    if((int)check.Attribute("failed")!=0||(int)check.Attribute("passed")<1||(int)check.Attribute("skipped")!=0)throw new InvalidOperationException("Current checks have not passed: "+path);
                }
            }
            var reference=File.ReadAllText("outputs/plano-implementacao-20261017/morte-preservada-sha256.json");
            var protectedFiles=Directory.GetFiles("outputs/transformacoes-animacoes-20261017","morte-*.png")
                .Concat(new[]{"Assets/Darklight/Runtime/Character/DarklightDeathReversion.cs","Assets/Darklight/Runtime/Character/DarklightOriginalVampireView.cs"}).ToArray();
            if(protectedFiles.Length!=8)throw new InvalidOperationException("Protected death set must have eight files");
            using(var sha=SHA256.Create())foreach(string file in protectedFiles)
            {
                string hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant();
                if(!reference.Contains("\""+hash+"\""))throw new InvalidOperationException("Protected death source changed: "+file);
            }
            string output=Path.GetFullPath(destination);Directory.CreateDirectory(output);
            string previousVersion=PlayerSettings.bundleVersion;
            try
            {
                PlayerSettings.bundleVersion=version;AssetDatabase.SaveAssets();
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},
                    locationPathName=Path.Combine(output,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
                var result=new Result{success=report.summary.result==BuildResult.Succeeded,version=version,playModeValidated=requirePlayMode,executable=Path.Combine(output,"DarklightRequiem.exe"),
                    errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,seconds=report.summary.totalTime.TotalSeconds};
                File.WriteAllText(Path.Combine(output,"../build-result.json"),JsonUtility.ToJson(result,true));
                if(!result.success)throw new InvalidOperationException("Windows review build failed");
            }
            finally{PlayerSettings.bundleVersion=previousVersion;AssetDatabase.SaveAssets();}
        }
    }
}
