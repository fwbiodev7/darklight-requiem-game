using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightExpansionBuild
    {
        [Serializable] sealed class Receipt
        {public string result,executable;public int errors,warnings;public ulong bytes;public double seconds;public bool audioAuditioned=false;}
        public static void Build()
        {
            foreach(var check in new[]{("../work/expansion-regression-20261023.xml",65),("../work/expansion-render-regression-20261023.xml",6),("../work/expansion-play-silent-20261023.xml",7)})
            {
                var root=XDocument.Load(check.Item1).Root;var cases=root.Descendants("test-case").ToArray();
                if(cases.Length!=check.Item2||cases.Any(c=>(string)c.Attribute("result")!="Passed"))throw new InvalidOperationException("Current focused checks must pass: "+check.Item1);
            }
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            string output=Path.GetFullPath("outputs/expansao-20261023/Windows");Directory.CreateDirectory(output);
            string executable=Path.Combine(output,"DarklightRequiem.exe");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=executable,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4});
            var summary=report.summary;var receipt=new Receipt{result=summary.result.ToString(),executable=executable,errors=(int)summary.totalErrors,warnings=(int)summary.totalWarnings,bytes=summary.totalSize,seconds=summary.totalTime.TotalSeconds};
            File.WriteAllText("outputs/expansao-20261023/build-result.json",JsonUtility.ToJson(receipt,true));
            if(summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Expansion Windows build failed");
            Debug.Log("EXPANSION_BUILD_OK: "+executable);
        }
    }
}
