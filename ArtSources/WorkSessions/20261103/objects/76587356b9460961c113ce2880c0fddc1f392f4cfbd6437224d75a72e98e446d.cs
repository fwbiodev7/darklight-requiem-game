using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightAtmospherePresetFactory
    {
        // Optional: runtime uses identical defaults without an authored asset.
        // AssetDatabase creates its own GUID; never hand-author a .meta file.
        [MenuItem("Darklight/Atmosphere/Create or select Rua das Cinzas preset")]
        public static void CreateOrSelectRuaPreset()
        {
            const string parent="Assets/Darklight/Resources/Darklight";
            const string folder=parent+"/Atmosphere";
            const string path=folder+"/GothicEclipseRuaDasCinzas.asset";
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(parent,"Atmosphere");
            var preset=AssetDatabase.LoadAssetAtPath<DarklightAtmospherePreset>(path);
            if(preset==null)
            {
                // Preserve a conflicting asset instead of overwriting it.
                if(AssetDatabase.LoadMainAssetAtPath(path)!=null)
                {
                    Debug.LogError("An unrelated asset already occupies "+path);
                    return;
                }
                preset=ScriptableObject.CreateInstance<DarklightAtmospherePreset>();
                AssetDatabase.CreateAsset(preset,path);
                AssetDatabase.SaveAssets();
            }
            Selection.activeObject=preset;
            EditorGUIUtility.PingObject(preset);
        }
    }
}
