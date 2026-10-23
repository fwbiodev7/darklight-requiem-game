using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Collections.Generic;
using System.Text.Json;

// Executes actual managed test methods against actual compiled project assemblies.
// This is not a replacement for Unity EditMode/PlayMode or engine-native JsonUtility tests.
class ManagedChecks
{
    static int Main(string[] args)
    {
        string root=Path.GetFullPath(args[0]);
        string unity="C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/Managed/UnityEngine";
        string checks=Path.Combine(root,"work/game-over-validation");
        string nunit=Directory.GetDirectories(Path.Combine(root,"Library/PackageCache"),"com.unity.ext.nunit@*")[0]+"/net472/unity-custom";
        AssemblyLoadContext.Default.Resolving+=(ctx,name)=>
        {
            foreach(string dir in new[]{checks,unity,nunit})
            {string path=Path.Combine(dir,name.Name+".dll");if(File.Exists(path))return ctx.LoadFromAssemblyPath(path);}
            return null;
        };
        var tests=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(checks,"Darklight.EditModeTests.dll"));
        string[] names={"DarklightDeathReversionTests","DarklightReturnPointTests","DarklightLivingPresentationTests","DarklightAttackFrameTimingTests","DarklightTransitionTimingTests"};
        string[] native={"OldSavesKeepTheirExistingChapterAndCheckpointFallback","NewFieldsRoundTripWithoutLosingExistingCampaignProgress"};
        var results=new List<object>();int passed=0,failed=0,skipped=0;
        foreach(string name in names)
        {
            var type=tests.GetType("Darklight.Tests."+name,true);var instance=Activator.CreateInstance(type);
            foreach(var method in type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))
            {
                var attributes=method.GetCustomAttributesData();
                var cases=attributes.Where(a=>a.AttributeType.Name=="TestCaseAttribute").ToArray();
                if(cases.Length==0&&!attributes.Any(a=>a.AttributeType.Name=="TestAttribute"))continue;
                if(native.Contains(method.Name)){skipped++;results.Add(new{test=name+"."+method.Name,status="requires Unity JsonUtility"});continue;}
                foreach(var item in cases.Length==0?new CustomAttributeData[]{null}:cases)
                {
                    object[] parameters=item==null?Array.Empty<object>():item.ConstructorArguments.Select(a=>a.Value).ToArray();
                    string id=name+"."+method.Name+"("+string.Join(",",parameters)+")";
                    try{method.Invoke(instance,parameters);passed++;results.Add(new{test=id,status="passed"});}
                    catch(Exception ex){failed++;results.Add(new{test=id,status="failed",error=(ex.InnerException??ex).ToString()});}
                }
            }
        }
        File.WriteAllText(Path.Combine(checks,"managed-results.json"),JsonSerializer.Serialize(new{kind="managed tests outside Unity; no gameplay/audio",passed,failed,skipped,results},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Managed tests: {passed} passed, {failed} failed, {skipped} require Unity.");
        return failed==0?0:1;
    }
}
