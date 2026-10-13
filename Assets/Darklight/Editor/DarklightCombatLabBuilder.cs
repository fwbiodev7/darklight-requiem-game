using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Darklight.Editor
{
    /// <summary>Explicitly invoked by Unity CLI. Edits only assets in the Darklight subtree and product identity in this copy.</summary>
    public static class DarklightCombatLabBuilder
    {
        const string Root = "Assets/Darklight/";
        static readonly string[] Directions = { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };

        public static void Build()
        {
            int quality=QualitySettings.GetQualityLevel();
            for(int i=0;i<QualitySettings.names.Length;i++) { QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=null; }
            QualitySettings.SetQualityLevel(quality,false);
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;
            EditorBuildSettings.RemoveConfigObject("com.unity.input.settings.actions");
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                throw new InvalidOperationException("Combat lab expects the audited Built-in pipeline; aborting.");
            Directory.CreateDirectory(Root + "Data/Attacks"); Directory.CreateDirectory(Root + "Scenes");
            Directory.CreateDirectory(Root + "Settings");
            AssetDatabase.Refresh();
            foreach(string file in new[] { "DefaultVolumeProfile.asset","UniversalRenderPipelineGlobalSettings.asset" })
            {
                string path="Assets/"+file;
                if(AssetDatabase.LoadMainAssetAtPath(path)==null)continue;
                string error=AssetDatabase.MoveAsset(path,Root+"Settings/"+file);
                if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
            }
            Sprite[] idle = Slice("FabioIdle.png", 8, new[] {
                new Rect(31,38,349,461), new Rect(413,41,326,464), new Rect(773,50,297,458), new Rect(1196,41,318,460),
                new Rect(104,524,343,456), new Rect(485,528,334,453), new Rect(854,528,276,457), new Rect(1210,528,241,456) });
            Sprite[] walk = Slice("FabioWalk.png", 32, ReadRects("FabioWalkRects.json"));
            // The approved static source's NW faces the wrong diagonal; use the corrected NW walk contact pose at rest.
            idle[3] = walk[12];
            Sprite[] slash = Slice("FabioSlash.png", 32, ReadRects("FabioSlashRects.json"));
            Sprite[] dummy = Slice("TrainingDummy.png", 1);
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/PixelSprite.mat");
            if (material == null) { material = new Material(Shader.Find("Darklight/PixelSprite")); AssetDatabase.CreateAsset(material, Root + "Art/PixelSprite.mat"); }
            var loadout = new CombatLoadout {
                lights = new[] { Profile("Leve1", StrikeKind.Light, .11f,.1f,.2f,14,1.55f,110), Profile("Leve2", StrikeKind.Light,.1f,.1f,.18f,17,1.6f,115), Profile("Leve3", StrikeKind.Light,.16f,.13f,.28f,23,1.8f,140) },
                heavy = Profile("Pesado", StrikeKind.Heavy,.22f,.14f,.3f,28,1.85f,130),
                charged = Profile("Carregado", StrikeKind.Charged,.2f,.18f,.36f,44,2.1f,150),
                counter = Profile("ContraAtaque", StrikeKind.Counter,.06f,.12f,.18f,32,1.9f,120),
                skill = Profile("OndaDoVeu", StrikeKind.Skill,.18f,.15f,.34f,22,3.2f,360)
            };
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("DarklightCamera");
            var camera = cameraObject.AddComponent<Camera>(); cameraObject.tag = "MainCamera";
            camera.transform.position = new Vector3(0,0,-10); camera.orthographic = true; camera.orthographicSize = 3.75f;
            camera.backgroundColor = new Color(.055f,.06f,.085f); camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>(); DarklightPresentation.Configure(camera);
            // A display camera keeps the backbuffer and IMGUI presentation active while the world renders offscreen.
            var screenCamera = new GameObject("DarklightScreenCamera").AddComponent<Camera>();
            screenCamera.cullingMask = 0; screenCamera.depth = 100; screenCamera.clearFlags = CameraClearFlags.SolidColor;
            screenCamera.backgroundColor = Color.black; screenCamera.allowHDR = false; screenCamera.allowMSAA = false;
            var lab = new GameObject("DarklightCombatLab").AddComponent<DarklightCombatLab>(); lab.WorldCamera = camera;
            lab.gameObject.AddComponent<DarklightBuildSmoke>();
            BuildFloor(material);
            var player = new GameObject("Fabio"); player.transform.position = new Vector3(-3,-1,0);
            var visuals = new GameObject("FabioVisual"); visuals.transform.SetParent(player.transform, false);
            var display = visuals.AddComponent<SpriteRenderer>(); display.sprite = idle[0]; display.sharedMaterial = material; display.sortingOrder = 110;
            var view = player.AddComponent<DarklightCharacterView>(); view.idle = idle; view.walk = walk; view.slash = slash;
            var rigid = player.AddComponent<Rigidbody2D>(); rigid.gravityScale = 0; rigid.freezeRotation = true;
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous; rigid.interpolation = RigidbodyInterpolation2D.Interpolate;
            var feet = player.AddComponent<CapsuleCollider2D>(); feet.size = new Vector2(.42f,.28f); feet.offset = new Vector2(0,.14f); feet.sharedMaterial = DarklightNavigation.Frictionless;
            player.AddComponent<HealthSystem>(); var fighter = player.AddComponent<DarklightFighter>(); fighter.loadout = loadout;
            player.AddComponent<DarklightCombatant>().player = true; lab.Player = fighter;
            camera.transform.position = player.transform.position + new Vector3(0,0,-10);
            cameraObject.AddComponent<DarklightLabCamera>().target = player.transform;
            var fixtures = new List<DarklightTrainingTarget>();
            foreach (Vector2 position in new[] { new Vector2(-1.5f,0), new Vector2(.7f,0), new Vector2(2.9f,0) })
            {
                var target = new GameObject("AlvoDeTreino"); target.transform.position = position;
                var renderer = target.AddComponent<SpriteRenderer>(); renderer.sprite = dummy[0]; renderer.sharedMaterial = material; renderer.sortingOrder = 90;
                var hurtbox = target.AddComponent<CircleCollider2D>(); hurtbox.radius = .4f; hurtbox.offset = new Vector2(0,.2f); hurtbox.isTrigger = true;
                target.AddComponent<HealthSystem>(); target.AddComponent<DarklightCombatant>();
                fixtures.Add(target.AddComponent<DarklightTrainingTarget>());
            }
            lab.Targets = fixtures.ToArray();
            var hudFrames=JsonUtility.FromJson<DetectedFrames>(File.ReadAllText(Root+"Art/HudRects.json"));
            if(hudFrames?.frames?.Length!=6) throw new InvalidOperationException("Six HUD assets required.");
            DarklightHudBuilder.Build(lab,Slice("HudAprovado.png",6,hudFrames.frames.Select(f=>new Rect(f.rect[0],f.rect[1],f.rect[2],f.rect[3])).ToArray()),material);
            PlayerSettings.productName = "DARKLIGHT REQUIEM"; PlayerSettings.companyName = "Fabio";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            EditorSceneManager.SaveScene(scene, Root + "Scenes/DarklightCombatLab.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "Scenes/DarklightCombatLab.unity", true) };
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Root+"Scenes/DarklightCombatLab.unity");
            AssetDatabase.SaveAssets();
            string outputs = Path.GetFullPath(Path.Combine(Application.dataPath, "../../outputs/etapa02")); Directory.CreateDirectory(outputs);
            File.WriteAllText(Path.Combine(outputs,"montagem-unity.json"), JsonUtility.ToJson(new BuildEvidence {
                scene = Root + "Scenes/DarklightCombatLab.unity", pipeline = "BuiltIn", idleSprites = idle.Length,
                walkingSprites = walk.Length, slashSprites = slash.Length, attackProfiles = 7,
                originalScenesReplaced = false, audioImported = false, productName = PlayerSettings.productName,
                persistentPath = Application.persistentDataPath }, true));
            var spriteRecords = new List<SpriteRecord>();
            foreach (var group in new[] { ("idle",idle), ("walk",walk), ("slash",slash), ("dummy",dummy) })
                for (int index=0;index<group.Item2.Length;index++)
                {
                    var sprite=group.Item2[index]; var r=sprite.rect;
                    spriteRecords.Add(new SpriteRecord { action=group.Item1,index=index,name=sprite.name,
                        direction=group.Item2.Length==1?"S":Directions[group.Item2.Length==8?index:index/4],
                        source=AssetDatabase.GetAssetPath(sprite),sourceWidth=sprite.texture.width,sourceHeight=sprite.texture.height,
                        pixelsPerUnit=sprite.pixelsPerUnit,rectBottomLeft=new[] {r.x,r.y,r.width,r.height},pivotPixels=new[] {sprite.pivot.x,sprite.pivot.y} });
                }
            File.WriteAllText(Path.Combine(outputs,"sprites-importados.json"),JsonUtility.ToJson(new SpriteRecords { frames=spriteRecords.ToArray() },true));
            Debug.Log("DARKLIGHT combat laboratory saved; new product identity isolated.");
        }
        [Serializable] class BuildEvidence { public string scene,pipeline,productName,persistentPath; public int idleSprites,walkingSprites,slashSprites,attackProfiles; public bool originalScenesReplaced,audioImported; }
        [Serializable] class SpriteRecord { public string action,name,direction,source;public int index,sourceWidth,sourceHeight;public float pixelsPerUnit;public float[] rectBottomLeft,pivotPixels; }
        [Serializable] class SpriteRecords { public SpriteRecord[] frames; }

        public static void CaptureWorld()
        {
            var camera=UnityEngine.Object.FindAnyObjectByType<DarklightCombatLab>().WorldCamera;
            var previous=camera.targetTexture; var target=previous ?? new RenderTexture(640,360,24) { filterMode=FilterMode.Point };
            if(previous==null) target.Create();
            camera.targetTexture=target;camera.aspect=640f/360f;camera.Render();
            var oldActive=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(640,360,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,640,360),0,0);image.Apply();
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/etapa02/laboratorio.png"));File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=oldActive;camera.targetTexture=previous;UnityEngine.Object.DestroyImmediate(image);
            if(previous==null) {target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }

        public static void BuildWindows()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Windows build module is not installed; no automatic installation.");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/etapa02/Windows"));Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[] {Root+"Scenes/DarklightCombatLab.unity"},
                locationPathName=Path.Combine(output,"DarklightTreino.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None });
            File.WriteAllText(Path.Combine(output,"resultado-build.json"),JsonUtility.ToJson(new PlayerBuildEvidence {
                result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,
                bytes=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds },true));
            if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed; inspect saved report.");
        }
        public static void BuildSceneAndWindows() { Build(); BuildWindows(); }
        [Serializable] class PlayerBuildEvidence { public string result;public int errors,warnings;public ulong bytes;public double seconds; }
        [Serializable] class DetectedFrame { public int[] rect; }
        [Serializable] class DetectedFrames { public DetectedFrame[] frames; }
        static Rect[] ReadRects(string file)
        {
            var parsed=JsonUtility.FromJson<DetectedFrames>(File.ReadAllText(Root+"Art/"+file));
            if(parsed?.frames==null || parsed.frames.Length!=32) throw new InvalidOperationException("Expected 32 measured sprite rectangles: "+file);
            return parsed.frames.Select(f=>new Rect(f.rect[0],f.rect[1],f.rect[2],f.rect[3])).ToArray();
        }
        static AttackDefinition Profile(string name, StrikeKind kind, float startup,float active,float recovery,float damage,float range,float angle)
        {
            string path = Root + "Data/Attacks/" + name + ".asset";
            var result = AssetDatabase.LoadAssetAtPath<AttackDefinition>(path);
            if (result == null) { result = ScriptableObject.CreateInstance<AttackDefinition>(); AssetDatabase.CreateAsset(result,path); }
            result.displayName = name; result.kind = kind; result.startup=startup; result.active=active; result.recovery=recovery; result.damage=damage; result.range=range; result.angle=angle;
            EditorUtility.SetDirty(result); return result;
        }

        static Sprite[] Slice(string file, int count, Rect[] knownTopLeft = null)
        {
            string path = Root + "Art/" + file;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing source " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true; importer.isReadable = true; importer.maxTextureSize = 4096; importer.SaveAndReimport();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("No sprite data provider; operation aborted.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            var required = EEditCapability.CreateAndDeleteSprite | EEditCapability.EditSpriteRect | EEditCapability.EditPivot | EEditCapability.EditSpriteName;
            if (edit == null || !edit.GetEditCapability().HasCapability(required)) throw new InvalidOperationException("Sprite edit capabilities unavailable; operation aborted.");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            var existing = provider.GetSpriteRects().ToDictionary(r => r.name, r => r);
            var rectangles = new SpriteRect[count]; float maxBodyHeight = 0;
            for (int i = 0; i < count; i++)
            {
                Rect rect;
                if (knownTopLeft != null) { rect = knownTopLeft[i]; rect.y = texture.height - rect.yMax; }
                else if (count == 1) rect = new Rect(0,0,texture.width,texture.height);
                else
                {
                    int column=i%4,row=i/4;
                    int x0=Mathf.RoundToInt(column*texture.width/4f), x1=Mathf.RoundToInt((column+1)*texture.width/4f);
                    int y0=texture.height-Mathf.RoundToInt((row+1)*texture.height/8f), y1=texture.height-Mathf.RoundToInt(row*texture.height/8f);
                    rect=new Rect(x0,y0,x1-x0,y1-y0);
                }
                int left=(int)rect.xMin,right=(int)rect.xMax,bottom=(int)rect.yMin,top=(int)rect.yMax;
                int headX=left+(right-left)/2, headY=bottom, footY=top; int minX=right,maxX=left;
                for(int y=bottom;y<top;y++) for(int x=left;x<right;x++) if(pixels[y*texture.width+x].a>=200) headY=Mathf.Max(headY,y);
                // Anchor to the hair/torso rather than the extended sword; consistent support between frames.
                for(int y=Mathf.Max(bottom,headY-(top-bottom)/4);y<=headY;y++) for(int x=left;x<right;x++)
                    if(pixels[y*texture.width+x].a>=200 && pixels[y*texture.width+x].r < 180) { minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x); }
                if(maxX>=minX) headX=(minX+maxX)/2;
                for(int y=bottom;y<top;y++) for(int x=Mathf.Max(left,headX-(right-left)/5);x<Mathf.Min(right,headX+(right-left)/5);x++)
                    if(pixels[y*texture.width+x].a>=200) footY=Mathf.Min(footY,y);
                if(footY==top || headY==bottom) throw new InvalidOperationException("Empty sprite cell: "+file+" / "+i);
                if (count != 32 || i%4==3) maxBodyHeight=Mathf.Max(maxBodyHeight,headY-footY+1);
                bool hud=file=="HudAprovado.png";
                string name = hud ? new[] { "HudHealth","HudSkill","HudKatana","HudParry","HudDodge","HudTarget" }[i] : count==1 ? "Dummy" : Directions[count==8?i:i/4] + "_" + (count==8?0:i%4);
                rectangles[i]=new SpriteRect { name=name, rect=rect, alignment=SpriteAlignment.Custom,
                    pivot=hud?Vector2.one*.5f:new Vector2((headX-rect.xMin)/rect.width,(footY-rect.yMin)/rect.height),
                    spriteID=existing.TryGetValue(name,out var old)?old.spriteID:GUID.Generate() };
            }
            var ids=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(ids==null) throw new InvalidOperationException("Sprite name/file ID provider unavailable; operation aborted.");
            provider.SetSpriteRects(rectangles); ids.SetNameFileIdPairs(rectangles.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.spritePixelsPerUnit=maxBodyHeight/(count==1?1.5f:2f); importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name,s=>s);
            return rectangles.Select(r=>sprites[r.name]).ToArray();
        }
        static void BuildFloor(Material material)
        {
            // Test geometry only: no generated world map or campaign assets are replaced.
            var pixel = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/LabFloor.asset");
            Sprite tile;
            if (pixel == null)
            {
                pixel=new Texture2D(1,1);pixel.SetPixel(0,0,Color.white);pixel.Apply();
                tile=Sprite.Create(pixel,new Rect(0,0,1,1),Vector2.one*.5f,1);
                AssetDatabase.CreateAsset(pixel,Root+"Art/LabFloor.asset");AssetDatabase.AddObjectToAsset(tile,pixel);
            }
            else tile=AssetDatabase.LoadAllAssetsAtPath(Root+"Art/LabFloor.asset").OfType<Sprite>().Single();
            for(int x=-10;x<10;x++) for(int y=-6;y<6;y++)
            {
                var piece=new GameObject("PisoDeTeste");piece.transform.position=new Vector3(x+.5f,y+.5f,1);
                var renderer=piece.AddComponent<SpriteRenderer>();renderer.sprite=tile;renderer.sharedMaterial=material;renderer.sortingOrder=-100;
                renderer.color=((x+y)%2==0)?new Color(.13f,.13f,.17f):new Color(.15f,.15f,.19f);
            }
            // Persist the pixel texture/sprite so the scene can reopen in a clean Editor.
            foreach(var rect in new[] { new Rect(-10,-6,20,.3f),new Rect(-10,5.5f,20,.3f),new Rect(-10,-6,.3f,12),new Rect(9.7f,-6,.3f,12) })
            { var wall=new GameObject("LimiteDoTreino");wall.transform.position=rect.center;var collider=wall.AddComponent<BoxCollider2D>();collider.size=rect.size;collider.sharedMaterial=DarklightNavigation.Frictionless; }
        }
    }
}
