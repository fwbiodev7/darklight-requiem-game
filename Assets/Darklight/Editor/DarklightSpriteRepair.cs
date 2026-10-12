using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightSpriteRepair
    {
        [Serializable] class Frame {public string name;public float[] rect,pivot;}
        [Serializable] class Sheet {public Frame[] frames;public float pixelsPerUnit;}
        static readonly string[] Sheets={"Campaign/Actions","Campaign/BossAttacks","Campaign/Bosses","Campaign/Enemies","Campaign/EnemyAttacks","Campaign/FabioMotion","Campaign/Forms","User/requiem_daggers","User/requiem_chain","User/rock_daggers","User/rock_chain","User/rockForms","User/skinActions","User/skinIdle","User/skinMotion","User/skinSlash"};
        public static string DetectPipeline()
        {
            var asset=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            return asset==null?"BuiltIn":asset.GetType().FullName;
        }
        public static void Apply()
        {
            if(DetectPipeline()!="BuiltIn")throw new InvalidOperationException("Expected audited Built-in pipeline.");
            AssetDatabase.Refresh();
            foreach(var name in Sheets)
            {
                string path="Assets/Darklight/Art/"+name+".png";
                var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText("Assets/Darklight/Art/"+name+"Rects.json"));
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)throw new InvalidOperationException("Missing sprite importer "+path);
                importer.maxTextureSize=4096;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                var factory=new SpriteDataProviderFactories();factory.Init();
                var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
                if(provider==null)throw new InvalidOperationException("Missing sprite data provider "+path);
                provider.InitSpriteEditorDataProvider();
                var capabilities=provider.GetDataProvider<ISpriteFrameEditCapability>();
                var required=EEditCapability.EditSpriteRect|EEditCapability.EditPivot;
                if(capabilities==null||!capabilities.GetEditCapability().HasCapability(required))throw new InvalidOperationException("Sprite edit capabilities unavailable "+path);
                var rects=provider.GetSpriteRects();
                if(rects.Length!=sheet.frames.Length)throw new InvalidOperationException("Sprite identity count mismatch "+path);
                int height=AssetDatabase.LoadAssetAtPath<Texture2D>(path).height;
                var indexed=rects.ToDictionary(r=>r.name);
                for(int i=0;i<sheet.frames.Length;i++)
                {
                    var f=sheet.frames[i];string spriteName=f.name??Path.GetFileNameWithoutExtension(path)+"_"+i;
                    if(!indexed.TryGetValue(spriteName,out var rect))throw new InvalidOperationException("Missing existing sprite ID "+spriteName);
                    rect.rect=new Rect(f.rect[0],height-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]);
                    rect.alignment=SpriteAlignment.Custom;rect.pivot=new Vector2(f.pivot[0],f.pivot[1]);
                }
                provider.SetSpriteRects(rects);provider.Apply();
                importer.spritePixelsPerUnit=sheet.pixelsPerUnit;importer.maxTextureSize=4096;
                importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            const string menuPath="Assets/Darklight/Audio/Music/metal-in-the-night-def-lev.mp3";
            var audio=AssetImporter.GetAtPath(menuPath) as AudioImporter;
            if(audio==null)throw new InvalidOperationException("Missing supplied menu track");
            var samples=audio.defaultSampleSettings;samples.loadType=AudioClipLoadType.Streaming;samples.compressionFormat=AudioCompressionFormat.Vorbis;samples.quality=.7f;samples.preloadAudioData=false;
            audio.defaultSampleSettings=samples;audio.forceToMono=false;audio.loadInBackground=true;audio.SaveAndReimport();
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
            var game=UnityEngine.Object.FindAnyObjectByType<DarklightCampaign>();
            game.menuMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(menuPath);
            var player=UnityEngine.Object.FindAnyObjectByType<DarklightFighter>();
            DarklightAppearanceBuilder.Apply(game,player,true);
            EditorUtility.SetDirty(game);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            // Keep the standalone training scene on the same corrected action sheets.
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCombatLab.unity");
            var view=UnityEngine.Object.FindAnyObjectByType<DarklightCharacterView>();
            Sprite[] Frames(string name)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Darklight/Art/Campaign/"+name+".png").OfType<Sprite>().OrderBy(s=>int.Parse(s.name.Substring(s.name.LastIndexOf('_')+1))).ToArray();
            view.motion=Frames("FabioMotion");view.actions=Frames("Actions");EditorUtility.SetDirty(view);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
            QualitySettings.antiAliasing=0;QualitySettings.anisotropicFiltering=AnisotropicFiltering.Disable;
            AssetDatabase.SaveAssets();
            Debug.Log("Sprite repair: 16 separated atlases; original sprite IDs retained; menu track imported as streaming audio.");
        }
    }
}
