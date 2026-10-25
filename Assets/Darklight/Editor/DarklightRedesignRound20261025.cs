using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightRedesignRound20261025
    {
        public static void Import()
        {
            DarklightAutomation.Silent=true;
            AudioListener.pause=true;
            AudioListener.volume=0;
            DarklightWeaponRedesignImporter.Import();
            DarklightMotorcycleRedesignImporter.Import();
            AssetDatabase.SaveAssets();
            Debug.Log("REDESIGN_20261025_IMPORT_OK: editor only; gameplay and sound were not started.");
        }
    }
}
