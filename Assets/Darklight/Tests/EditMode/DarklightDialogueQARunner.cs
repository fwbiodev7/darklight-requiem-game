using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    [InitializeOnLoad]public static class DarklightDialogueQARunner
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/dialogo-textura-20261015/";
        static DarklightDialogueQARunner(){EditorApplication.update+=Watch;}
        static void Watch(){if(!SessionState.GetBool("Darklight.Dialogue.QA",false)||SessionState.GetString("Darklight.Tests.Output","")!=""||!File.Exists(Work+"dialogue-tests.xml.summary.json"))return;var r=JsonUtility.FromJson<Result>(File.ReadAllText(Work+"dialogue-tests.xml.summary.json"));SessionState.EraseBool("Darklight.Dialogue.QA");EditorApplication.Exit(r.failed==0?0:1);}
        [System.Serializable]sealed class Result{public int failed;}
        public static void Run(){if(File.Exists(Work+"dialogue-tests.xml.summary.json"))File.Delete(Work+"dialogue-tests.xml.summary.json");SessionState.SetBool("Darklight.Dialogue.QA",true);DarklightAutomation.Silent=true;AudioListener.volume=0;DarklightDeliveryTestRunner.Focused(Work+"dialogue-tests.xml","^Darklight.Tests.(DarklightGameplayTests.(DialogueUsesRemappedInteractAndCheckpointHasDistinctArt|LongDialoguePaginatesAndMouseDismissDoesNotAttackOrRepeatCallback|DialogueTypesPausesAndAdvancesWithKeyboardAndController)|DarklightDialogueVisualChecks.*)$");}
    }
}
