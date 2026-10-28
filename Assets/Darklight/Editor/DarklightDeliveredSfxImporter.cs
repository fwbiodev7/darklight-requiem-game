using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public sealed class DarklightDeliveredSfxImporter : AssetPostprocessor
    {
        const string Root = "Assets/Darklight/Resources/Darklight/Audio/Delivery20261023/";
        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Root, System.StringComparison.Ordinal)) return;
            var importer = (AudioImporter)assetImporter;
            // Prepared WAVs are already mono. Avoid Unity downmix normalization
            // so the documented gain and .9 peak ceiling remain unchanged.
            importer.forceToMono = false;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
        }
    }
}
