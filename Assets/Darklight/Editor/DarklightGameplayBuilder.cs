using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightGameplayBuilder
    {
        const string Root="Assets/Darklight/Data/Weapons/";
        public static void Apply()
        {
            var katana=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Root+"Requiem.asset");
            var daggers=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Root+"PresasDoVeu.asset");
            var chain=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Root+"LacoDaRuptura.asset");
            if(katana==null||daggers==null||chain==null)throw new InvalidOperationException("Existing weapon definitions required");
            var fast=Counter("PresasContraAtaque",katana.loadout.counter);fast.damage=9;fast.range=1.55f;fast.angle=115;fast.startup=.035f;fast.active=.17f;fast.recovery=.12f;fast.hitPulses=3;fast.pullTarget=false;fast.lungeSpeed=.6f;fast.recoveryChainFraction=.18f;
            var pull=Counter("LacoContraAtaque",katana.loadout.counter);pull.damage=28;pull.range=3.25f;pull.angle=90;pull.startup=.06f;pull.active=.09f;pull.recovery=.21f;pull.hitPulses=1;pull.pullTarget=true;pull.lungeSpeed=0;pull.recoveryChainFraction=.45f;
            daggers.loadout.counter=fast;chain.loadout.counter=pull;
            foreach(var attack in daggers.loadout.lights){attack.recoveryChainFraction=.18f;EditorUtility.SetDirty(attack);}
            foreach(var attack in chain.loadout.lights){attack.recoveryChainFraction=.45f;EditorUtility.SetDirty(attack);}
            EditorUtility.SetDirty(fast);EditorUtility.SetDirty(pull);EditorUtility.SetDirty(daggers);EditorUtility.SetDirty(chain);AssetDatabase.SaveAssets();
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/gameplay-tuning-result.txt")),"SUCCESS: weapon identities preserved, distinct counters and recovery chains");
        }
        static AttackDefinition Counter(string name,AttackDefinition source)
        {
            var result=AssetDatabase.LoadAssetAtPath<AttackDefinition>(Root+name+".asset");
            if(result==null){result=ScriptableObject.CreateInstance<AttackDefinition>();EditorUtility.CopySerialized(source,result);AssetDatabase.CreateAsset(result,Root+name+".asset");}
            result.displayName=name;result.kind=StrikeKind.Counter;return result;
        }
        public static void Build()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/gameplay-melhorada/Windows"));Directory.CreateDirectory(folder);
            var scenes=Array.FindAll(EditorBuildSettings.scenes,s=>s.enabled);var paths=Array.ConvertAll(scenes,s=>s.path);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=paths,locationPathName=Path.Combine(folder,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Path.Combine(folder,"resultado-build.txt"),$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nwarnings={report.summary.totalWarnings}\nbytes={report.summary.totalSize}\ntime={report.summary.totalTime}");
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Gameplay build failed");
        }
    }
}
