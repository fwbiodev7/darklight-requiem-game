using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightLuciusDelivery
    {
        public static void ImportInitialIdle()
        {
            const string path="Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalIdle8V1.png";
            DarklightWeaponRedesignImporter.ImportStandalone(path);
            AssetDatabase.SaveAssets();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if(sprites.Length!=8)throw new InvalidDataException("Lucius Idle must import eight whole native views");
            var data=Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root+"catalog");
            if(data==null||!DarklightLuciusSpriteBank.ValidCatalog(JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(data.text)))
                throw new InvalidDataException("Invalid separate Lucius catalog");
            DarklightLuciusSpriteBank.ClearCache();
            Debug.Log("Lucius: eight static views imported; no campaign/boss registration or animation completeness claimed");
        }
    }
}
