using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;
namespace Darklight.Tests
{
    [InitializeOnLoad] public static class DarklightDeliveryTestRunner
    {
        const string OutputKey="Darklight.Tests.Output", BackupKey="Darklight.Tests.Backup";
        static TestRunnerApi api;
        [Serializable] sealed class Pref {public string key,type,text;public bool exists;public int number;public float scalar;}
        [Serializable] sealed class Save {public string path,data;}
        [Serializable] sealed class Protection {public List<Pref> prefs=new();public List<Save> saves=new();}
        static DarklightDeliveryTestRunner(){api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Callback());EditorApplication.update+=KeepSilent;}
        static void KeepSilent(){if(SessionState.GetBool("Darklight.SilentChecks",false))AudioListener.volume=0;}
        static void Begin(string output,Filter filter,string captureFolder=null)
        {
            if(!string.IsNullOrEmpty(SessionState.GetString(OutputKey,"")))throw new InvalidOperationException("A protected test run is already active");
            Directory.CreateDirectory(Path.GetDirectoryName(output));var backup=new Protection();
            void Prefs(string type,params string[] keys)
            {
                foreach(var key in keys)backup.prefs.Add(new Pref{key=key,type=type,exists=PlayerPrefs.HasKey(key),scalar=type=="float"?PlayerPrefs.GetFloat(key):0,number=type=="int"?PlayerPrefs.GetInt(key):0,text=type=="string"?PlayerPrefs.GetString(key):null});
            }
            Prefs("float","Darklight.Master","Darklight.Music","Darklight.Effects","Darklight.Ambient","Darklight.UI","Darklight.Enemies","Darklight.Player");
            Prefs("int","Darklight.Flashes","Darklight.FrameRate","Darklight.Fullscreen","Darklight.Height","Darklight.Lessons","Darklight.Shake","Darklight.Skin","Darklight.Tips","Darklight.VSync","Darklight.Width");
            Prefs("string","Darklight.Bindings","Darklight.Tests.CaptureOutput");
            if(Directory.Exists(Application.persistentDataPath))foreach(var path in Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories))backup.saves.Add(new Save{path=path,data=Convert.ToBase64String(File.ReadAllBytes(path))});
            string protection=output+".protection.json";File.WriteAllText(protection,JsonUtility.ToJson(backup,true));
            SessionState.SetString(OutputKey,output);SessionState.SetString(BackupKey,protection);SessionState.SetBool("Darklight.SilentChecks",true);
            if(captureFolder!=null){PlayerPrefs.SetString("Darklight.Tests.CaptureOutput",captureFolder);PlayerPrefs.Save();}
            AudioListener.volume=0;DarklightAutomation.Silent=true;api.Execute(new ExecutionSettings(filter));
        }
        public static void Audio(string output)=>Begin(output,new Filter{testMode=TestMode.PlayMode,groupNames=new[]{"^Darklight.Tests.DarklightAudioSettingsTests\\."}});
        public static void FinalEdit(string output)=>Begin(output,new Filter{testMode=TestMode.EditMode,groupNames=new[]{"^Darklight.Tests\\."}});
        public static void FinalPlay(string output)=>Begin(output,new Filter{testMode=TestMode.PlayMode,groupNames=new[]{"^Darklight.Tests\\.","!^Darklight.Tests.DarklightFinalCaptureTests", "!^Darklight.Tests.DarklightNarrativePlayTests.AudioPlaybackIsBoundedMutedAndStopsWhenAutomationReturnsToSilence$","!^Darklight.Tests.DarklightAwakeningMusicTests.AwakeningMusicPausesAndResumesPositionAndDoesNotOverlapMainMenu$"}});
        public static void Focused(string output,string pattern)=>Begin(output,new Filter{testMode=TestMode.PlayMode,groupNames=new[]{pattern}});
        public static void FocusedEdit(string output,string pattern)=>Begin(output,new Filter{testMode=TestMode.EditMode,groupNames=new[]{pattern}});
        public static void Capture(string output)=>Begin(output,new Filter{testMode=TestMode.PlayMode,groupNames=new[]{"^Darklight.Tests.DarklightFinalCaptureTests\\."}},Path.Combine(Path.GetDirectoryName(output),"captures-final"));
        sealed class Callback:ICallbacks
        {
            public void RunStarted(ITestAdaptor test){AudioListener.volume=0;DarklightAutomation.Silent=true;}
            public void TestStarted(ITestAdaptor test){AudioListener.volume=0;DarklightAutomation.Silent=true;}
            public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result)
            {
                string path=SessionState.GetString(OutputKey,"");if(string.IsNullOrEmpty(path))return;TestRunnerApi.SaveResultToFile(result,path);
                var backup=JsonUtility.FromJson<Protection>(File.ReadAllText(SessionState.GetString(BackupKey,"")));
                foreach(var p in backup.prefs){if(!p.exists)PlayerPrefs.DeleteKey(p.key);else if(p.type=="float")PlayerPrefs.SetFloat(p.key,p.scalar);else if(p.type=="int")PlayerPrefs.SetInt(p.key,p.number);else PlayerPrefs.SetString(p.key,p.text);}PlayerPrefs.Save();
                bool savesPreserved=backup.saves.All(s=>File.Exists(s.path)&&Convert.ToBase64String(File.ReadAllBytes(s.path))==s.data);
                File.WriteAllText(path+".summary.json","{\"passed\":"+result.PassCount+",\"failed\":"+result.FailCount+",\"skipped\":"+result.SkipCount+",\"savesPreserved\":"+(savesPreserved?"true":"false")+",\"prefsRestored\":true}");
                // An independently running user's game may save during QA. Do not overwrite it blindly.
                if(!savesPreserved)Debug.LogError("Protected QA detected a changed save. Backup retained; inspect before restoring because the user's game is open.");
                SessionState.EraseString(OutputKey);SessionState.EraseString(BackupKey);DarklightAutomation.Silent=true;AudioListener.volume=0;
            }
        }
    }
}
