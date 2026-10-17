using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Darklight.EditorTools
{
    // Metadata is authored by Unity's public importer API, never by editing .meta files.
    public sealed class TodayAudioImporter : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (assetPath.Contains("/Audio/Expansion/") || assetPath.Contains("/Audio/Review20261017/") || assetPath.EndsWith("/RockTheme.mp3") || assetPath.EndsWith("/RequiemTheme.mp3"))
                Configure((AudioImporter)assetImporter, assetPath);
        }
        static void Configure(AudioImporter importer, string path)
        {
            bool music = path.Contains("/Music/"), loop = path.Contains("dl-ambient-") && !path.Contains("thunder") && !path.Contains("drip") && !path.Contains("creature") && !path.Contains("whisper") && !path.Contains("portal");
            bool streaming = music || loop || path.EndsWith("park_ambience_wind.wav") || path.EndsWith("dungeon_ambient_1.ogg");
            importer.forceToMono = !music;
            importer.loadInBackground = streaming;
            var settings = importer.defaultSampleSettings;
            settings.loadType = streaming ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = streaming ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            settings.quality = music ? .9f : .7f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = !streaming;
            importer.defaultSampleSettings = settings;
        }
        public static object ApplyAndAudit()
        {
            DarklightAutomation.Silent = true; var audit = new List<object>();
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] {"Assets/Darklight/Resources/Darklight/Audio", "Assets/Darklight/Resources/Darklight/Music"}))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("/Expansion/") && !path.EndsWith("RockTheme.mp3") && !path.EndsWith("RequiemTheme.mp3") && !path.EndsWith("park_ambience_wind.wav") && !path.EndsWith("dungeon_ambient_1.ogg")) continue;
                var importer = (AudioImporter)AssetImporter.GetAtPath(path); var before = importer.defaultSampleSettings;
                Configure(importer, path); importer.SaveAndReimport();
                importer = (AudioImporter)AssetImporter.GetAtPath(path); var after = importer.defaultSampleSettings;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) throw new InvalidOperationException("Missing imported audio: " + path);
                audit.Add(new {path, beforeLoad=before.loadType.ToString(), afterLoad=after.loadType.ToString(), codec=after.compressionFormat.ToString(), channels=clip.channels, frequency=clip.frequency, seconds=clip.length, background=importer.loadInBackground, preload=after.preloadAudioData});
            }
            return audit;
        }
    }
}
