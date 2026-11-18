using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    // Focused combat/save/audio only. Menu QA was explicitly left to the user.
    [InitializeOnLoad]public static class DarklightCombat20QARunner
    {
        const string Key="Darklight.Combat20.QA";
        static DarklightCombat20QARunner(){EditorApplication.update+=Finish;}
        public static void Edit()
        {
            string path=Path.GetFullPath("outputs/retomada-20261117/combat20-edit-"+DateTime.UtcNow.ToString("yyyyMMddTHHmmss")+".xml");
            SessionState.SetString(Key,path);DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            DarklightDeliveryTestRunner.FocusedEdit(path,"^Darklight.Tests.(DarklightCombat20Tests|DarklightAirControllerTests|DarklightAirExpansionTests|DarklightSimplifiedScopeTests|DarklightLuciusMusicTests|DarklightWeaponTechniqueTests)\\.");
        }
        [Serializable]sealed class Summary{public int passed,failed,skipped;public bool savesPreserved,prefsRestored;}
        static void Finish()
        {
            string path=SessionState.GetString(Key,"");if(path==""||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(path+".summary.json"))return;
            var result=JsonUtility.FromJson<Summary>(File.ReadAllText(path+".summary.json"));SessionState.EraseString(Key);
            EditorApplication.Exit(result.passed>0&&result.failed==0&&result.skipped==0&&result.savesPreserved&&result.prefsRestored?0:1);
        }
    }
}
