using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public sealed class DarklightLoadingMascotImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            const string root = "Assets/Darklight/Resources/Darklight/Loading/";
            bool mascot = assetPath == root + "Mascots/gato-ruptura.png" || assetPath == root + "Mascots/mini-fabius.png";
            bool backdrop = assetPath == root + "gothic-loading-20261117.png";
            if (!mascot && !backdrop) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = backdrop ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (backdrop) importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = mascot;
            importer.isReadable = false;
        }
    }
}
