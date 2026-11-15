using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightSession14Delivery
    {
        public static void ImportOfficialTransformations()
        {
            DarklightAutomation.Silent = true;
            SessionState.SetBool("Darklight.SilentChecks", true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            const string root = "Assets/Darklight/Resources/Darklight/OfficialTransitions20261114/";
            const string clips = "Assets/Darklight/Animations/OfficialTransitions20261114/";
            Directory.CreateDirectory(clips);
            string[] names = { "FabiusRupturaOfficial20261114", "FabiusSerafimOfficial20261114", "LuciusArconteOfficial20261114" };
            for (int kind = 0; kind < names.Length; kind++)
            {
                string name = names[kind], png = root + name + ".png";
                DarklightWeaponRedesignImporter.ImportStandalone(png);
                var sheet = JsonUtility.FromJson<OfficialSheet>(File.ReadAllText(root + name + ".json"));
                var sprites = AssetDatabase.LoadAllAssetsAtPath(png).OfType<Sprite>().ToDictionary(s => s.name);
                var keys = new ObjectReferenceKeyframe[sheet.frames.Length + 1];
                float time = 0;
                for (int i = 0; i < sheet.frames.Length; i++)
                {
                    keys[i] = new ObjectReferenceKeyframe { time = time, value = sprites[sheet.frames[i].id] };
                    time += sheet.frames[i].durationMs / 1000f;
                }
                keys[keys.Length - 1] = new ObjectReferenceKeyframe { time = time, value = keys[keys.Length - 2].value };
                string clipPath = clips + name + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) { clip = new AnimationClip { name = name, frameRate = 60 }; AssetDatabase.CreateAsset(clip, clipPath); }
                var binding = EditorCurveBinding.PPtrCurve(kind == 2 ? "LuciusVisual" : "FabioVisual", typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings); EditorUtility.SetDirty(clip);
            }
            AssetDatabase.SaveAssets(); DarklightOfficialTransitionBank.ClearCache();
            for (int kind = 1; kind <= 3; kind++)
                if (!DarklightOfficialTransitionBank.TryPose(kind, 1, out _)) throw new InvalidOperationException("Official transition import failed: " + kind);
            Debug.Log("Official transformations imported in order: Ruptura, Serafim, Lucius; three bound AnimationClips created.");
        }
        [Serializable] public sealed class OfficialFrame { public string id; public int durationMs; }
        [Serializable] public sealed class OfficialSheet { public OfficialFrame[] frames; }

        public static void ImportSources()
        {
            DarklightAutomation.Silent = true;
            SessionState.SetBool("Darklight.SilentChecks", true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            DarklightFabiusFormsImporter.ImportSession14();
            string root = "Assets/Darklight/Resources/Darklight/";
            foreach (var folder in new[] { "LuciusNormal20261109", "LuciusArconte20261111" })
            {
                foreach (string path in Directory.GetFiles(root + folder, "*Session20261114.png").OrderBy(p => p))
                    DarklightWeaponRedesignImporter.ImportStandalone(path.Replace('\\', '/'));
                AssetDatabase.ImportAsset(root + folder + "/catalog.json", ImportAssetOptions.ForceSynchronousImport);
            }
            AssetDatabase.SaveAssets();
            DarklightLuciusSpriteBank.ClearCache();
            DarklightFabiusFormsBank.ClearCache();
            Debug.Log("Session14: selective whole-figure source import completed; runtime tests still required.");
        }
    }
}
