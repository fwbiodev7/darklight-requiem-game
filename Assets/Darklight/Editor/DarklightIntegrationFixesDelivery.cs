using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Darklight.Editor
{
    [InitializeOnLoad]
    public static class DarklightIntegrationFixesDelivery
    {
        const string Key = "Darklight.IntegrationFixes.Stage";
        const string Output = "work/codex-fixes-20261113";
        static readonly TestRunnerApi Runner;
        static DarklightIntegrationFixesDelivery()
        {
            Runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            Runner.RegisterCallbacks(new Results());
        }

        [MenuItem("Darklight/Validate Fabius Lucius Arena and Build")]
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
                throw new InvalidOperationException("Finish compilation and leave Play Mode before validation.");
            Directory.CreateDirectory(Output);
            DarklightAutomation.Silent = true;
            SessionState.SetBool("Darklight.SilentChecks", true);
            DarklightFabiusFormsImporter.ImportAll();
            SessionState.SetInt(Key, 1);
            Runner.Execute(new ExecutionSettings(new Filter {
                testMode = TestMode.EditMode,
                testNames = new[] {
                    "Darklight.Tests.DarklightFabiusFormsTests",
                    "Darklight.Tests.DarklightLuciusAnimationIntegrationTests",
                    "Darklight.Tests.DarklightLuciusSpriteBankTests",
                    "Darklight.Tests.DarklightLuciusCombatEncounterTests"
                }
            }));
        }

        [MenuItem("Darklight/Validate Presentation")]
        public static void ValidatePresentation()
        {
            SessionState.SetBool("Darklight.SilentChecks",true);
            SessionState.SetInt(Key,2);
            Runner.Execute(new ExecutionSettings(new Filter {
                testMode=TestMode.PlayMode,
                testNames=new[] { "Darklight.Tests.DarklightIntegrationFixesTests" }
            }));
        }
        [MenuItem("Darklight/Build Validated Fixes")]
        public static void BuildValidated()
        {
            DarklightLuciusTestBuild.BuildTo("outputs/build-windows-codex-fixes-20261113/Windows");
        }

        sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                int stage = SessionState.GetInt(Key, 0);
                if (stage == 0) return;
                TestRunnerApi.SaveResultToFile(result, Output + (stage == 1 ? "/editmode.xml" : "/playmode.xml"));
                if (result.FailCount > 0 || result.PassCount == 0)
                {
                    SessionState.SetInt(Key, 0);
                    Debug.LogError("Integration checks failed. Build stopped. See " + Output);
                    return;
                }
                EditorApplication.delayCall += () => {
                    if (stage == 1)
                    {
                        SessionState.SetInt(Key, 2);
                        Runner.Execute(new ExecutionSettings(new Filter {
                            testMode = TestMode.PlayMode,
                            testNames = new[] { "Darklight.Tests.DarklightIntegrationFixesTests" }
                        }));
                    }
                    else
                    {
                        SessionState.SetInt(Key, 0);
                        Debug.Log("Integration tests passed. Inspect screenshots, then select Build Validated Fixes.");
                    }
                };
            }
        }
    }
}
