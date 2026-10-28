using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightScope26Delivery
    {
        public static void Import()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            DarklightWeaponRedesignImporter.Import();
            string path="Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.RequiemResource(0)+".png";
            if(!File.Exists(path))throw new FileNotFoundException("Missing reviewed Requiem motorcycle sheet",path);
            DarklightWeaponRedesignImporter.ImportStandalone(path);
            string idlePath="Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.RequiemIdleResource+".png";
            if(!File.Exists(idlePath))throw new FileNotFoundException("Missing reviewed Requiem motorcycle idle sheet",idlePath);
            DarklightWeaponRedesignImporter.ImportStandalone(idlePath);
            string rearIdlePath="Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.RequiemRearIdleResource+".png";
            if(!File.Exists(rearIdlePath))throw new FileNotFoundException("Missing reviewed Requiem rear mounted idle sheet",rearIdlePath);
            DarklightWeaponRedesignImporter.ImportStandalone(rearIdlePath);
            string sideIdlePath="Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.RequiemSideIdleResource+".png";
            if(!File.Exists(sideIdlePath))throw new FileNotFoundException("Missing mature Requiem side mounted idle sheet",sideIdlePath);
            DarklightWeaponRedesignImporter.ImportStandalone(sideIdlePath);
            DarklightWeaponRedesignImporter.ImportStandalone("Assets/Darklight/Resources/Darklight/CombatContacts20261027/ContactBursts.png");
            DarklightWeaponRedesignImporter.ImportStandalone("Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.RequiemDualSawsResource+".png");
            AssetDatabase.SaveAssets();DarklightWeaponRedesignBank.ClearCache();DarklightMotorcycleRedesignBank.ClearCache();
            Debug.Log("SCOPE_20261026_IMPORTED: whole figures preserved; runtime verification remains separate.");
        }
    }
}
