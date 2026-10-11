using System;
using UnityEngine;
namespace Darklight
{
    // Set before scene startup. Automated gameplay checks never start an audio clip.
    public static class DarklightAutomation
    {
        public static bool Silent { get; set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            Silent = Application.isBatchMode || Array.Exists(Environment.GetCommandLineArgs(), a => a == "-darklightSilent" || a.StartsWith("--requiem-") && a.Contains("capture"));
#if UNITY_EDITOR
            Silent |= UnityEditor.SessionState.GetBool("Darklight.SilentChecks", false);
#endif
        }
    }
}
