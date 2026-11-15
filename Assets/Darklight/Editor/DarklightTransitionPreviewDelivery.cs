using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Darklight.Editor
{
    [InitializeOnLoad]
    public static class DarklightTransitionPreviewDelivery
    {
        const string Running="Darklight.TransitionPreview.Running";
        const string Output="outputs/session20261114/official-transitions";
        static readonly TestRunnerApi Runner;
        static DarklightTransitionPreviewDelivery()
        {
            Runner=ScriptableObject.CreateInstance<TestRunnerApi>();Runner.RegisterCallbacks(new Results());
        }
        // Batch invocation must omit -quit: the real PlayMode scene loads asynchronously.
        [MenuItem("Darklight/Capture Official Transformation Validation")]
        public static void Capture()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)
                throw new InvalidOperationException("Finish compilation and leave Play Mode before capture.");
            Directory.CreateDirectory(Output);
            SessionState.SetBool(Running,true);
            SessionState.SetBool(Running+".PriorSilent",SessionState.GetBool("Darklight.SilentChecks",false));
            SessionState.SetString(Running+".PriorOutput",SessionState.GetString("Darklight.Tests.Output",""));
            SessionState.SetBool("Darklight.SilentChecks",true);
            // Activates existing save-isolation contract before loading the scene.
            SessionState.SetString("Darklight.Tests.Output",Path.GetFullPath(Output));
            Runner.Execute(new ExecutionSettings(new Filter
            {
                testMode=TestMode.PlayMode,
                testNames=new[]{"Darklight.Tests.DarklightTransformationPreviewCaptureTests.CaptureThreeCandidateTransitions",
                    "Darklight.Tests.DarklightTransformationLifecycleTests","Darklight.Tests.DarklightTransformationTests"}
            }));
        }
        sealed class Results:ICallbacks
        {
            public void RunStarted(ITestAdaptor tests){}
            public void TestStarted(ITestAdaptor test){}
            public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result)
            {
                if(!SessionState.GetBool(Running,false))return;
                SessionState.SetBool(Running,false);
                TestRunnerApi.SaveResultToFile(result,Output+"/capture-results.xml");
                SessionState.SetBool("Darklight.SilentChecks",SessionState.GetBool(Running+".PriorSilent",false));
                SessionState.SetString("Darklight.Tests.Output",SessionState.GetString(Running+".PriorOutput",""));
                bool success=result.FailCount==0&&result.PassCount>0;
                Debug.Log("Official transformation runtime validation "+(success?"completed.":"failed; inspect capture-results.xml."));
                if(Application.isBatchMode)EditorApplication.delayCall+=()=>EditorApplication.Exit(success?0:1);
            }
        }
    }
}
