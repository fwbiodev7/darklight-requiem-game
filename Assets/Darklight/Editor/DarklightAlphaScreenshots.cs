using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Darklight.Editor
{
    // Presentation only: renders the saved Unity scene in Edit mode; no simulation, inputs or audio.
    public static class DarklightAlphaScreenshots
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/site-darklight/dist/assets"));
        static void Set(DarklightCampaign game,string name,object value)=>typeof(DarklightCampaign).GetField(name,Flags).SetValue(game,value);
        static void Call(DarklightCampaign game,string name)=>typeof(DarklightCampaign).GetMethod(name,Flags).Invoke(game,null);
        static DarklightCombatLab Prepare()
        {
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");var lab=Object.FindAnyObjectByType<DarklightCombatLab>();var game=lab.GetComponent<DarklightCampaign>();Set(game,"lab",lab);Set(game,"health",lab.Player.GetComponent<HealthSystem>());return lab;
        }
        static void Save(DarklightCombatLab lab,string name)
        {
            var world=new RenderTexture(1280,720,24){filterMode=FilterMode.Point,antiAliasing=1};world.Create();lab.WorldCamera.targetTexture=world;lab.WorldCamera.aspect=640f/360;lab.WorldCamera.cullingMask=~(1<<5);Physics2D.SyncTransforms();lab.WorldCamera.Render();lab.PixelHUD.worldImage.texture=world;
            var camera=new GameObject("CaptureEditOnly").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7.5f;camera.cullingMask=1<<5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.01f;camera.farClipPlane=100;
            var target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();camera.targetTexture=target;
            foreach(var rect in lab.PixelHUD.canvas.GetComponentsInChildren<RectTransform>(true))rect.gameObject.layer=5;
            var canvas=lab.PixelHUD.canvas;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;lab.PixelHUD.scaler.scaleFactor=2;canvas.scaleFactor=2;Canvas.ForceUpdateCanvases();camera.Render();
            var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());RenderTexture.active=old;
            camera.targetTexture=null;lab.WorldCamera.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(camera.gameObject);world.Release();target.Release();Object.DestroyImmediate(world);Object.DestroyImmediate(target);
        }
        public static void BuildAndCapture(){DarklightCampaignBuilder.AssembleAndBuild();AppearanceAndMaps();}
        public static void AppearanceAndMaps(){CaptureAppearance();Capture();}
        public static void CaptureAppearance()
        {
            var lab=Prepare();var game=lab.GetComponent<DarklightCampaign>();
            var profile=AssetDatabase.LoadAssetAtPath<CharacterAppearanceProfile>("Assets/Darklight/Data/Appearance.asset");
            for(int skin=0;skin<2;skin++)for(int stage=0;stage<5;stage++)
            {
                var obj=new GameObject("AppearancePresentation"+skin+stage);obj.layer=8;obj.transform.position=new Vector3((stage-2)*2.5f,skin==0?.6f:-2.6f,0);
                var art=new GameObject("Corpo").AddComponent<SpriteRenderer>();art.transform.SetParent(obj.transform,false);art.gameObject.layer=8;art.sprite=skin==0?lab.Player.GetComponent<DarklightCharacterView>().idle[0]:game.rockIdle[0];art.sharedMaterial=game.pixelMaterial;art.transform.localScale=Vector3.one*2f/art.sprite.bounds.size.y;art.sortingOrder=200;
                var system=obj.AddComponent<CharacterAppearanceSystem>();system.profile=profile;system.PreviewStage(stage);system.Present(art,0,skin,0,null);
            }
            var camera=lab.WorldCamera;camera.transform.position=new Vector3(0,.2f,-10);camera.orthographic=true;camera.orthographicSize=3.75f;camera.cullingMask=1<<8;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.035f,.06f);camera.aspect=16f/9;
            var target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();camera.targetTexture=target;camera.Render();
            var old=RenderTexture.active;RenderTexture.active=target;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
            var path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/integracao-arte/aparencia/estagios.png"));Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,tex.EncodeToPNG());RenderTexture.active=old;
            camera.targetTexture=null;Object.DestroyImmediate(tex);target.Release();Object.DestroyImmediate(target);EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
            Debug.Log("Appearance presentation: 10 composed static previews, no gameplay, save changes or audio.");
        }
        public static void DiagnoseActor()
        {
            var lab=Prepare();var art=new GameObject("DiagnosticFabio").AddComponent<SpriteRenderer>();art.gameObject.layer=8;art.sprite=lab.Player.GetComponent<DarklightCharacterView>().idle[0];art.transform.localScale=Vector3.one*2f/art.sprite.bounds.size.y;art.color=Color.white;art.sortingOrder=30000;
            var camera=lab.WorldCamera;camera.transform.position=new Vector3(0,1,-10);camera.orthographic=true;camera.orthographicSize=3;camera.cullingMask=1<<8;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.aspect=16f/9;
            var rt=new RenderTexture(640,360,24){filterMode=FilterMode.Point};rt.Create();camera.targetTexture=rt;
            for(int i=0;i<2;i++)
            {
                art.sharedMaterial=i==0?lab.GetComponent<DarklightCampaign>().pixelMaterial:new Material(Shader.Find("Sprites/Default"));camera.Render();
                var prior=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(640,360,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,640,360),0,0);tex.Apply();
                File.WriteAllBytes(Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/diagnostic-actor-"+i+".png")),tex.EncodeToPNG());RenderTexture.active=prior;Object.DestroyImmediate(tex);
            }
            Debug.Log("Actor diagnostic bounds="+art.bounds+" sprite vertices="+art.sprite.vertices.Length+" rect="+art.sprite.rect+" pivot="+art.sprite.pivot);
            camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
        }
        public static void Capture()
        {
            Directory.CreateDirectory(Output);
            for(int i=0;i<6;i++)
            {
                var lab=Prepare();var game=lab.GetComponent<DarklightCampaign>();var preview=GameObject.Find("VesperaPreview");if(preview!=null)Object.DestroyImmediate(preview);var map=DarklightMapDefinition.ForChapter(i);var root=new GameObject("EditMapPreview");DarklightMapLayout.Build(root.transform,i,game.floorTiles,game.architecture,game.pixelMaterial);
                Vector3 p=map.Arenas[1];lab.Player.transform.position=p;var rb=lab.Player.GetComponent<Rigidbody2D>();rb.interpolation=RigidbodyInterpolation2D.None;rb.position=p;lab.WorldCamera.transform.position=p+new Vector3(0,.75f,-10);lab.WorldCamera.orthographicSize=7.5f;
                lab.Player.GetComponentInChildren<SpriteRenderer>().enabled=false;var art=new GameObject("FabioEditPreview").AddComponent<SpriteRenderer>();art.transform.position=p;art.sprite=lab.Player.GetComponent<DarklightCharacterView>().idle[0];art.transform.localScale=Vector3.one*2f/art.sprite.bounds.size.y;art.sortingOrder=100-Mathf.RoundToInt(p.y*10);art.color=Color.white;art.sharedMaterial=game.pixelMaterial;
                Call(game,"CreateUI");lab.PixelHUD.transform.Find("HUD640x360/Menus").gameObject.SetActive(false);lab.PixelHUD.death.gameObject.SetActive(false);
                SetText(lab,"Objetivo",DarklightStory.Places[i].ToUpperInvariant());SetText(lab,"Capitulo","CAP. "+(i+1));SetText(lab,"Energia","ENERGIA 0");SetText(lab,"Progresso","NV 1   XP 0/95   R:3");SetText(lab,"Ajuda","M1: CORTE   Q: PARRY   ESPACO: ESQUIVA   ESC: MENU");
                foreach(var name in new[]{"EnergiaBarra","VidaChefe","MP3Compacto"}){var element=lab.PixelHUD.transform.Find("HUD640x360/"+name);if(element!=null)element.gameObject.SetActive(false);}SetText(lab,"Arma","REQUIEM  V / LS");Save(lab,"map"+(i+1));
            }
            for(int skin=0;skin<2;skin++)
            {
                var lab=Prepare();var game=lab.GetComponent<DarklightCampaign>();if(skin==1)lab.Player.GetComponent<DarklightCharacterView>().idle=game.rockIdle;
                Call(game,"CreateUI");Call(game,"RefreshPanel");var first=lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel/Botao0");first.GetComponent<Image>().sprite=game.uiArt[2];Save(lab,skin==0?"menu":"skin-rock");
            }
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");Debug.Log("DARKLIGHT: 8 presentation images rendered in Edit mode without gameplay or audio.");
        }
        static void SetText(DarklightCombatLab lab,string name,string value){var t=lab.PixelHUD.transform.Find("HUD640x360/"+name);if(t!=null&&t.TryGetComponent<Text>(out var label))label.text=value;}
    }
}
