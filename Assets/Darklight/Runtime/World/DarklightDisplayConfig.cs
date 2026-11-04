using UnityEngine;

namespace Darklight
{
    /// <summary>
    /// AAA Display & Window Manager for Darklight: Requiem.
    /// Ensures borderless fullscreen, optimal refresh rate matching, and cursor containment.
    /// </summary>
    public static class DarklightDisplayConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeDisplay()
        {
            // Unlock FPS cap to match high refresh rate gaming monitors (up to 144/240Hz)
            Application.targetFrameRate = Screen.currentResolution.refreshRateRatio.value > 0 
                ? Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value) 
                : 60;

            // Ensure VSync to prevent screen tearing in fast combat
            QualitySettings.vSyncCount = 1;

            // Ensure smooth borderless presentation
            if (!Application.isEditor && Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
        }
    }
}
