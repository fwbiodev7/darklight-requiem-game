using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Darklight.Editor
{
    // Production UI builders, rendered in EditMode with explicit sample statistics.
    // Does not run scene Start/Update, load a player save, or enter Play.
    public static class DarklightInterfaceReview
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Set(object target,string field,object value)=>target.GetType().GetField(field,Private).SetValue(target,value);
        static T Get<T>(object target,string field)=>(T)target.GetType().GetField(field,Private).GetValue(target);
        static object Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,Private).Invoke(target,args);
        public static void Capture()
        {
            DarklightAutomation.Silent=true;
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity",OpenSceneMode.Single);
            var game=UnityEngine.Object.FindFirstObjectByType<DarklightCampaign>();var lab=game.GetComponent<DarklightCombatLab>();
            var input=new DarklightInput();typeof(DarklightCombatLab).GetProperty("Input").SetValue(lab,input);
            Set(game,"lab",lab);Set(game,"health",lab.Player.GetComponent<HealthSystem>());Set(game,"tones",Array.Empty<AudioClip>());
            foreach(string source in new[]{"music","ambient","sfx"})Set(game,source,game.gameObject.AddComponent<AudioSource>());
            var profiles=new DarklightCombatProfiles();game.weaponDefinitions=profiles.Create(game.weaponDefinitions);
            var controller=lab.Player.GetComponent<DarklightWeaponController>();controller.Initialize(game);typeof(DarklightCampaign).GetProperty("Weapons").SetValue(game,controller);
            Call(game,"CreateUI");Call(game,"InitializeAtlas");
            var root=Get<RectTransform>(game,"uiRoot");var overlay=Get<RectTransform>(game,"overlay");
            var canvas=lab.PixelHUD.canvas;canvas.renderMode=RenderMode.ScreenSpaceCamera;lab.PixelHUD.scaler.enabled=false;canvas.scaleFactor=2;
            var camera=canvas.worldCamera;if(camera==null){camera=new GameObject("InterfaceReviewCamera").AddComponent<Camera>();canvas.worldCamera=camera;}
            camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.03f,.035f,.05f);
            var target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();camera.targetTexture=target;
            string output=Path.GetFullPath("outputs/preview-darklight-20261022/ui");Directory.CreateDirectory(output);
            var readback=new Texture2D(1280,720,TextureFormat.RGBA32,false,false);var previous=RenderTexture.active;
            void Shot(string name)
            {
                Call(game,"TickMenuPolish");Call(game,"TickMinimalHUD");Call(game,"RestoreMinimalHUDVisibility");
                Canvas.ForceUpdateCanvases();DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;
                readback.ReadPixels(new Rect(0,0,1280,720),0,0);readback.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),readback.EncodeToPNG());
            }
            try
            {
                game.Progress.chapter=4;var result=game.Progress.StageResult(4);
                result.kills=37;result.dodges=14;result.parries=6;result.hits=83;result.combos=9;result.bestCombo=24;result.bestRank=6;result.specials=8;result.damageTaken=42.5f;result.completed=true;
                game.StageStats.Begin(result);Set(game,"panel","");overlay.gameObject.SetActive(false);
                lab.PixelHUD.worldImage.texture=Resources.Load<Texture2D>("Darklight/Maps/map5");
                var mini=root.Find("Minimapa");mini.gameObject.SetActive(true);
                mini.Find("Territorio/Microarte").GetComponent<RawImage>().texture=Resources.Load<Texture2D>("Darklight/Minimap/Map5");
                mini.Find("Territorio/Geometria").gameObject.SetActive(false);
                root.Find("DestinoAcompanhado").GetComponent<CanvasGroup>().alpha=1;
                for(int rank=0;rank<7;rank++)
                {
                    game.Style.Reset();int move=0;while(game.Style.RankIndex<rank&&move<80){game.Style.Hit(move,move%3,true,true);move++;}
                    Call(game,"TickStyleHUD");Shot("hud-rank-"+game.Style.Rank);
                }
                Call(game,"Open","StageResults");Shot("estatisticas");
                Call(game,"Open","WeaponCombos");for(int weapon=0;weapon<3;weapon++){Set(game,"guideWeapon",weapon);Call(game,"RefreshPanel");Shot("combos-arma-"+weapon);}
                Set(game,"phaseLoading",true);Call(game,"ShowLoadingScreen",4);
                var preview=Get<Image>(game,"loadingPreview");preview.sprite=Resources.Load<Sprite>("Darklight/Loading/preview5");preview.enabled=preview.sprite!=null;
                var progress=Get<DarklightLoadingProgress>(game,"loadingProgress");
                float[] values={.04f,.14f,.72f,.88f,.99f,1};string[] stages={"CARREGANDO O DESTINO","MONTANDO O CENÁRIO","PREPARANDO PERSONAGEM E COLISÕES","PREPARANDO OBJETIVOS E ATMOSFERA","CONCLUINDO A PASSAGEM","CAMINHO ABERTO"};
                for(int i=0;i<values.Length;i++)
                {
                    if(i==values.Length-1){progress.Complete();Set(game,"loadingReady",true);}else progress.Report(values[i],stages[i]);
                    Call(game,"TickLoadingScreen");Shot("loading-"+Mathf.RoundToInt(values[i]*100));
                }
                File.WriteAllText(Path.Combine(output,"capture.json"),"{\"productionUIBuilders\":true,\"sampleStatistics\":true,\"playMode\":false,\"audio\":false,\"screens\":17}\n");
                Debug.Log("Interface preview: 17 production-UI captures in EditMode with sample data; no Play, music or player-save writes.");
            }
            finally
            {
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(readback);
                profiles.Dispose();input.Dispose();
            }
        }
    }
}
