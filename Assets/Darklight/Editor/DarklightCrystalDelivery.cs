using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightCrystalDelivery
    {
        public static void Import()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            DarklightLivingIdleImporter.Import();DarklightCanonicalComplexion.ClearCache();
            DarklightLivingIdleCapture.CaptureSkinReview();DarklightLivingIdleCapture.CaptureAll();
            string output="outputs/restauracao-cristais-20261017";Directory.CreateDirectory(output);
            File.Copy("outputs/implementacao-fase2-20261017/pele-original-frente-unity.png",output+"/cristais-frente-unity.png",true);
            AssetDatabase.SaveAssets();
        }
    }
}
