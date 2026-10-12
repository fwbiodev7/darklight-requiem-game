using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightCampaignBuilder
    {
        const string Root="Assets/Darklight/";
        [Serializable] class Frame {public float[] rect,pivot;}
        [Serializable] class Frames {public Frame[] frames;public float pixelsPerUnit;}
        public static void AssembleAndBuild()
        {
            Assemble();BuildWindows();
        }
        public static void Assemble()
        {
            AssetDatabase.Refresh();
            // The laboratory remains an independent development scene; reuse its approved actor, input and HUD.
            EditorSceneManager.OpenScene(Root+"Scenes/DarklightCombatLab.unity");
            var lab=UnityEngine.Object.FindAnyObjectByType<DarklightCombatLab>();
            foreach(var target in lab.Targets)if(target!=null)UnityEngine.Object.DestroyImmediate(target.gameObject);
            lab.Targets=Array.Empty<DarklightTrainingTarget>();
            foreach(var obj in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(obj!=null&&(obj.name=="PisoDeTeste"||obj.name=="LimiteDoTreino"))UnityEngine.Object.DestroyImmediate(obj.gameObject);
            var smoke=lab.GetComponent<DarklightBuildSmoke>();if(smoke!=null)UnityEngine.Object.DestroyImmediate(smoke);
            foreach(var frame in lab.PixelHUD.targetFrames)if(frame!=null)UnityEngine.Object.DestroyImmediate(frame.gameObject);
            lab.PixelHUD.targetFrames=Array.Empty<RectTransform>();lab.PixelHUD.targetFills=Array.Empty<UnityEngine.UI.Image>();lab.PixelHUD.targetTexts=Array.Empty<UnityEngine.UI.Text>();
            var game=lab.gameObject.AddComponent<DarklightCampaign>();
            game.enemyBarFrame=AssetDatabase.LoadAllAssetsAtPath(Root+"Art/HudAprovado.png").OfType<Sprite>().First(s=>s.name.Contains("Target"));
            game.pixelMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Art/PixelSprite.mat");
            game.floorTiles=Slice("FloorTilesV2",false);game.architecture=Slice("ArchitectureV2",true);game.enemyFrames=Slice("Enemies",true);game.bossFrames=Slice("Bosses",true);game.extras=Slice("Extras",true);game.formFrames=Slice("Forms",true);game.enemyAttacks=Slice("EnemyAttacks",true);game.bossAttacks=Slice("BossAttacks",true);lab.Player.GetComponent<DarklightCharacterView>().motion=Slice("FabioMotion",true);lab.Player.GetComponent<DarklightCharacterView>().actions=Slice("Actions",true);
            DarklightUserArtImporter.Apply(game);
            string[] names={"him-join-me-in-death","him-wings-of-a-butterfly","metallica-seek-and-destroy-remastered","metallica-creeping-death","metallica-enter-sandman"};
            game.tracks=new AudioClip[5];
            for(int i=0;i<5;i++)
            {
                string path=Root+"Audio/Music/"+names[i]+".mp3";
                var importer=AssetImporter.GetAtPath(path) as AudioImporter;
                // Public source snapshots may omit optional, locally supplied music.
                if(importer==null){game.tracks[i]=null;continue;}
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.7f;settings.preloadAudioData=false;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.loadInBackground=true;importer.SaveAndReimport();
                game.tracks[i]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if(game.tracks[i]==null||game.tracks[i].loadType!=AudioClipLoadType.Streaming)throw new InvalidOperationException("Music import failed: "+path);
            }
            game.menuMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Music/metal-in-the-night-def-lev.mp3");
            var menuImporter=AssetImporter.GetAtPath(Root+"Audio/Music/metal-in-the-night-def-lev.mp3") as AudioImporter;
            if(menuImporter!=null){var settings=menuImporter.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.7f;settings.preloadAudioData=false;menuImporter.defaultSampleSettings=settings;menuImporter.forceToMono=false;menuImporter.loadInBackground=true;menuImporter.SaveAndReimport();}
            // Retiming uses Unity assets, preserving their GUIDs and references.
            string[] attackNames={"Leve1","Leve2","Leve3","Pesado","Carregado","ContraAtaque","OndaDoVeu"};
            float[] startups={.055f,.065f,.08f,.10f,.15f,.035f,.085f},actives={.075f,.08f,.10f,.11f,.14f,.09f,.12f},recoveries={.14f,.15f,.18f,.20f,.23f,.13f,.18f};
            for(int i=0;i<attackNames.Length;i++){var attack=AssetDatabase.LoadAssetAtPath<AttackDefinition>(Root+"Data/Attacks/"+attackNames[i]+".asset");attack.startup=startups[i];attack.active=actives[i];attack.recovery=recoveries[i];EditorUtility.SetDirty(attack);}
            DarklightWeaponBuilder.Apply(game,lab.Player);
            DarklightAppearanceBuilder.Apply(game,lab.Player);
            lab.WorldCamera.orthographicSize=7.5f;
            lab.WorldCamera.GetComponent<DarklightLabCamera>().bounds=DarklightMapLayout.CameraBounds;lab.WorldCamera.GetComponent<DarklightLabCamera>().followOffset=new Vector2(0,.75f);
            // Preview the first map in Edit mode; runtime creates its own traversable world.
            var preview=new GameObject("VesperaPreview");DarklightMapLayout.Build(preview.transform,0,game.floorTiles,game.architecture,game.pixelMaterial);
            preview.AddComponent<DarklightEditorPreview>();
            Vector3 spawn=DarklightMapDefinition.ForChapter(0).Spawn;lab.Player.transform.position=spawn;lab.WorldCamera.transform.position=spawn+new Vector3(0,.75f,-10);
            lab.gameObject.AddComponent<DarklightDeliveryCapture>();
            string scene=Root+"Scenes/DarklightCampaign.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true),new EditorBuildSettingsScene(Root+"Scenes/DarklightCombatLab.unity",true)};
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(scene);
            PlayerSettings.productName="DARKLIGHT REQUIEM";PlayerSettings.companyName="Fabio";PlayerSettings.bundleVersion="0.1.0-alpha";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            AssetDatabase.SaveAssets();
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/campanha"));Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output,"montagem.json"),JsonUtility.ToJson(new Evidence{chapters=6,enemies=game.enemyFrames.Length/4,bosses=game.bossFrames.Length/4,extras=game.extras.Length,tracks=game.tracks.Length,scene=scene,musicStreaming=true},true));
            Debug.Log("DARKLIGHT campaign assembled: six maps, five enemy families, six bosses, original cast and five supplied music tracks.");
        }
        [Serializable] class Evidence {public int chapters,enemies,bosses,extras,tracks;public string scene;public bool musicStreaming;}
        static Sprite[] Slice(string name,bool transparent)
        {
            string path=Root+"Art/Campaign/"+name+".png";
            var sheet=JsonUtility.FromJson<Frames>(File.ReadAllText(Root+"Art/Campaign/"+name+"Rects.json"));var frames=sheet.frames;
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=transparent;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.spritePixelsPerUnit=48;importer.isReadable=false;importer.SaveAndReimport();
            var factories=new SpriteDataProviderFactories();factories.Init();var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var cap=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(cap==null||!cap.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite | EEditCapability.EditSpriteRect | EEditCapability.EditPivot | EEditCapability.EditSpriteName | EEditCapability.EditBorder))throw new InvalidOperationException("Sprite importer capability unavailable.");
            var namesProvider=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(namesProvider==null)throw new InvalidOperationException("Sprite name provider unavailable.");
            var old=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);int height=texture.height;
            var rects=frames.Select((f,i)=>new SpriteRect{name=name+"_"+i,spriteID=old.TryGetValue(name+"_"+i,out var id)?id:GUID.Generate(),rect=new Rect(f.rect[0],height-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]),alignment=SpriteAlignment.Custom,border=name=="Extras" && (i==6||i==7)?(i==6?new Vector4(80,80,80,80):new Vector4(60,60,60,60)):Vector4.zero,pivot=f.pivot?.Length==2?new Vector2(f.pivot[0],f.pivot[1]):name=="ArchitectureV2"?new Vector2(.5f,0):transparent&&name!="Extras"?new Vector2(.5f,.03f):Vector2.one*.5f}).ToArray();
            provider.SetSpriteRects(rects);namesProvider.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();if(name=="FloorTilesV2")importer.spritePixelsPerUnit=168;if(name=="ArchitectureV2")importer.spritePixelsPerUnit=192;if(name=="Forms")importer.spritePixelsPerUnit=frames.Max(f=>f.rect[3])/2;if(name=="Actions")importer.spritePixelsPerUnit=frames.Where((f,i)=>i%4==1).Max(f=>f.rect[3])/2;importer.SaveAndReimport();
            if(sheet.pixelsPerUnit>0){importer.spritePixelsPerUnit=sheet.pixelsPerUnit;importer.SaveAndReimport();}
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name,s=>s);
            return rects.Select(r=>sprites[r.name]).ToArray();
        }
        public static void BuildWindows()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/campanha/Windows"));Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=Path.Combine(output,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Path.Combine(output,"resultado-build.txt"),$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nwarnings={report.summary.totalWarnings}\nbytes={report.summary.totalSize}\ntime={report.summary.totalTime}");
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Campaign build failed.");
        }
        public static void OpenForPlayer()
        {
            EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.None;EditorSettings.enterPlayModeOptionsEnabled=false;
            EditorSceneManager.OpenScene(Root+"Scenes/DarklightCampaign.unity");EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Root+"Scenes/DarklightCampaign.unity");
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            // Open for manual inspection. Entering Play (and starting the soundtrack) belongs to the user.
        }
    }
}
