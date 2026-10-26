using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace Darklight.Tests
{
    [InitializeOnLoad]public static class DarklightOverhaulQARunner
    {
        static TestRunnerApi api;
        static string Work => DarklightTestOutput.GetDirectory("combat-overhaul-20261015");
        static DarklightOverhaulQARunner(){EditorApplication.update+=Watch;}
        static void Watch()
        {
            string kind=SessionState.GetString("Darklight.Overhaul.QA","");if(kind==""||SessionState.GetString("Darklight.Tests.Output","")!="")return;
            string path=Work+kind+"-tests.xml.summary.json";
            if(!System.IO.File.Exists(path))return;
            var summary=JsonUtility.FromJson<Summary>(System.IO.File.ReadAllText(path));SessionState.EraseString("Darklight.Overhaul.QA");EditorApplication.Exit(summary.failed>0?1:0);
        }
        [System.Serializable]sealed class Summary{public int failed;}
        static void Begin(string kind)
        {
            string path=Work+kind+"-tests.xml.summary.json";
            if(System.IO.File.Exists(path)){System.IO.File.Copy(path,path+".previous",true);System.IO.File.Delete(path);}
            SessionState.SetString("Darklight.Overhaul.QA",kind);Prepare();
        }
        static void Prepare(){api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new ExitWhenDone());AudioListener.volume=0;DarklightAutomation.Silent=true;}
        public static void Edit(){Begin("edit");DarklightDeliveryTestRunner.FocusedEdit(Work+"edit-tests.xml","^Darklight.Tests.(DarklightTechnicalCombatTests|DarklightCombatTests|DarklightWallSlidingTests|DarklightBindingTests)\\.");}
        public static void Play(){Begin("play");DarklightDeliveryTestRunner.Focused(Work+"play-tests.xml","^Darklight.Tests.DarklightTechnicalPlayTests\\.");}
        public static void Menus(){Begin("menus");DarklightDeliveryTestRunner.Focused(Work+"menus-tests.xml","^Darklight\\.Tests\\.(DarklightTechnicalPlayTests\\.DifficultySelector.*|DarklightCombatCaptureTests\\..*)$");}
        public static void Captures(){Begin("captures");DarklightDeliveryTestRunner.Focused(Work+"captures-tests.xml","^Darklight.Tests.DarklightCombatCaptureTests\\.");}
        public static void FinalPlay(){Begin("final-play");DarklightDeliveryTestRunner.Focused(Work+"final-play-tests.xml","^Darklight.Tests.(DarklightTechnicalPlayTests|DarklightCombatCaptureTests|DarklightPhysicalRouteTests)\\.");}
        sealed class ExitWhenDone:ICallbacks
        {
            public void RunStarted(ITestAdaptor t){}public void TestStarted(ITestAdaptor t){}public void TestFinished(ITestResultAdaptor t){}
            public void RunFinished(ITestResultAdaptor r){EditorApplication.delayCall+=()=>EditorApplication.Exit(r.FailCount>0?1:0);}
        }
    }
}
