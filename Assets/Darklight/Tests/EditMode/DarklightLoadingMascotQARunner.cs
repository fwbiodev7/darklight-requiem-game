using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    [InitializeOnLoad] public static class DarklightLoadingMascotQARunner
    {
        const string Key="Darklight.LoadingMascot.QA";
        static DarklightLoadingMascotQARunner(){EditorApplication.update+=Finish;}
        public static void Edit()=>Run(false);
        public static void Play()=>Run(true);
        static void Run(bool play)
        {
            string path=Path.GetFullPath("outputs/loading-mascots-20261117/"+(play?"playmode":"editmode")+"-"+DateTime.UtcNow.ToString("yyyyMMddTHHmmss")+".xml");
            SessionState.SetString(Key,path);DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            if(play)DarklightDeliveryTestRunner.Focused(path,"^Darklight.Tests.DarklightLoadingMascotGameplayTests\\.");
            else DarklightDeliveryTestRunner.FocusedEdit(path,"^Darklight.Tests.DarklightLoadingMascotTests\\.");
        }
        [Serializable] sealed class Summary {public int passed,failed,skipped;public bool savesPreserved,prefsRestored;}
        static void Finish()
        {
            string path=SessionState.GetString(Key,"");
            if(path==""||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(path+".summary.json"))return;
            var result=JsonUtility.FromJson<Summary>(File.ReadAllText(path+".summary.json"));SessionState.EraseString(Key);
            EditorApplication.Exit(result.passed==2&&result.failed==0&&result.skipped==0&&result.savesPreserved&&result.prefsRestored?0:1);
        }
    }
}
