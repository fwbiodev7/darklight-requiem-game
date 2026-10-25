using System.IO;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightMotorcycleRedesignImporter
    {
        [MenuItem("DARKLIGHT/Arte/Importar figuras novas da moto")]
        public static void Import()
        {
            int imported=0;
            for(int form=0;form<4;form++)
            {
                string path="Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.Resource(form)+".png";
                if(!File.Exists(Path.ChangeExtension(path,"json")))continue;
                DarklightWeaponRedesignImporter.ImportStandalone(path);imported++;
            }
            AssetDatabase.SaveAssets();DarklightMotorcycleRedesignBank.ClearCache();
            Debug.Log($"MOTORCYCLE_REDESIGN_IMPORTED: {imported} form sheets; eight static riding views and ground slam S only. Other motorcycle actions remain in their authored banks.");
        }
    }
}
