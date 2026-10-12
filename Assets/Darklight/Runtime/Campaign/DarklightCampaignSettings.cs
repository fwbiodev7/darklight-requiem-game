using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public float AmbientVolume=.45f,InterfaceVolume=.35f;
        int optionsCategory,skinPreviewStage;DarklightPauseBlur pauseBlur;
        RectTransform pauseSceneRoot;UnityEngine.UI.Text pauseLocation,skinDescription,skinEquipped;
        UnityEngine.UI.Image skinPreview;
        Sprite[] requiemPreviewIdle;
        readonly Dictionary<string,Sprite[]> skinPreviewCache=new();
        readonly Dictionary<int,(UnityEngine.UI.Image fill,Func<float> read,Action<float> write)> settingsSliders=new();
        static readonly string[] SettingsCategories={"GERAL","AUDIO","VIDEO","CONTROLES","ACESSIBILIDADE","EXTRAS"};
        static readonly Vector2Int[] WindowSizes={new(1280,720),new(1600,900),new(1920,1080)};
        void InitializeMenuPolish()
        {
            pauseSceneRoot=RectAt(uiRoot,"CenaAtualDesfocada",0,0,640,360);pauseSceneRoot.SetSiblingIndex(overlay.GetSiblingIndex());
            pauseBlur=pauseSceneRoot.gameObject.AddComponent<DarklightPauseBlur>();pauseBlur.Initialize(lab.WorldCamera);
            var dim=ImageAt(pauseSceneRoot,"EscurecerCena",0,0,640,360,null);dim.color=new Color(.01f,.008f,.015f,.58f);
            var card=ImageAt(menuRoot,"ContextoDaFase",449,25,178,112,null);card.color=new Color(.015f,.012f,.024f,.62f);card.transform.SetSiblingIndex(1);
            var accent=ImageAt(card.transform,"LinhaCarmesim",0,0,.5f,112,null);accent.color=new Color(.62f,.05f,.11f);
            pauseLocation=TextAt(menuRoot,"LocalDaPausa",454,30,168,75,6);pauseLocation.horizontalOverflow=HorizontalWrapMode.Wrap;pauseLocation.lineSpacing=1.5f;
            skinPreview=ImageAt(menuRoot,"PreviaVisualAnimada",267,108,180,195,null);skinPreview.preserveAspect=true;skinPreview.material=null;
            skinDescription=TextAt(menuRoot,"DescricaoVisual",457,128,160,105,6);skinDescription.horizontalOverflow=HorizontalWrapMode.Wrap;skinDescription.lineSpacing=1.6f;
            skinEquipped=TextAt(menuRoot,"VisualEquipado",457,264,160,22,6);skinEquipped.alignment=TextAnchor.MiddleCenter;
            AmbientVolume=PlayerPrefs.GetFloat("Darklight.Ambient",.45f);InterfaceVolume=PlayerPrefs.GetFloat("Darklight.UI",.35f);
        }
        void TickMenuPolish()
        {
            bool paused=CinematicPause;
            pauseBlur.Show(paused);pauseSceneRoot.gameObject.SetActive(paused);
            bool skin=panel=="Skins";skinPreview.gameObject.SetActive(skin);skinDescription.gameObject.SetActive(skin);skinEquipped.gameObject.SetActive(skin);
            pauseLocation.gameObject.SetActive(panel=="Pause");
            menuRoot.Find("ContextoDaFase").gameObject.SetActive(panel=="Pause");
            if(panel=="Pause"){bodyText.gameObject.SetActive(false);pauseLocation.text=(ExploringNewRegion?Map.Art.title:DarklightStory.Chapters[Progress.chapter])+"\n"+(ExploringNewRegion?Map.Art.title:DarklightStory.Places[Progress.chapter])+"\n\n"+Objective;}
            if(skin)
            {
                int preview=selection<2?selection:SelectedSkin;int stage=skinPreviewStage;
                string key=(preview==1?"Rock":"Requiem")+"IdleStage"+stage;
                if(!skinPreviewCache.TryGetValue(key,out var poses))
                {
                    poses=stage>0?Resources.LoadAll<Sprite>("Darklight/AppearanceBodies/"+key):preview==1?rockIdle:requiemPreviewIdle;
                    if(stage>0)Array.Sort(poses,(a,b)=>int.Parse(a.name[(a.name.LastIndexOf('_')+1)..]).CompareTo(int.Parse(b.name[(b.name.LastIndexOf('_')+1)..])));
                    skinPreviewCache[key]=poses;
                }
                skinPreview.sprite=poses!=null&&poses.Length>0?poses[poses.Length==16?(int)(Time.unscaledTime*2)%2:0]:null;
                skinDescription.text=(preview==1?"ROCK\n\nColete de couro, rebites e correntes. Presenca direta e rebelde.":"REQUIEM\n\nSobretudo escuro e katana ancestral. Atravessa Vespera entre o sangue e o eclipse.")+"\n\nAPARENCIA "+(stage+1)+" / 5";
                skinEquipped.text=SelectedSkin==preview?"EQUIPADO":"ENTER / A: EQUIPAR";skinEquipped.color=SelectedSkin==preview?new Color(.72f,.08f,.15f):new Color(.84f,.82f,.78f);
            }
            foreach(var slider in settingsSliders.Values)if(slider.fill!=null)slider.fill.rectTransform.sizeDelta=new Vector2(105*slider.read(),2);
        }
        void BuildVisualPicker()
        {
            skinPreviewStage=returnPanel=="Main"?(DarklightProgress.Load()?.appearanceStage??0):Appearance.Stage;
            heading.text="VISUAL";bodyText.text="";
            Button("REQUIEM",()=>{SelectSkin(0);RefreshPanel();});Button("ROCK",()=>{SelectSkin(1);RefreshPanel();});Button("VOLTAR",()=>Open(returnPanel));
        }
        void SliderRow(string name,Func<float> read,Action<float> write)
        {
            int index=buttons.Count;
            Button(name+"     "+Mathf.RoundToInt(read()*100)+"%",()=>{write(Mathf.Clamp01(read()+settingsDirection*.05f));RefreshPanel();});
            var back=ImageAt(buttons[index].rect,"TrilhoVolume",257,20,105,2,null);back.color=new Color(.15f,.16f,.19f);
            var fill=ImageAt(back.transform,"ValorVolume",0,0,105*read(),2,null);fill.color=new Color(.65f,.07f,.14f);settingsSliders[index]=(fill,read,write);
        }
        void SetVolume(string key,float value,Action<float> assign){assign(value);PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();}
        void BuildSettings()
        {
            heading.text="OPCOES";bodyText.text="";settingsSliders.Clear();
            for(int i=0;i<6;i++){int tab=i;Button(SettingsCategories[i],()=>{optionsCategory=tab;RefreshPanel();selection=tab;});}
            switch(optionsCategory)
            {
                case 0:
                    Button("DICAS DE COMBATE: "+(lab.Coach?.TipsEnabled==true?"SIM":"NAO"),()=>{lab.Coach?.ToggleTips();RefreshPanel();});
                    Button("REVER TREINO CONTEXTUAL",()=>{lab.Coach?.Restart();Notice("DICAS REINICIADAS");RefreshPanel();});break;
                case 1:
                    SliderRow("MUSICA",()=>MusicVolume,v=>SetVolume("Darklight.Music",v,x=>MusicVolume=x));
                    SliderRow("EFEITOS",()=>EffectsVolume,v=>SetVolume("Darklight.Effects",v,x=>EffectsVolume=x));
                    SliderRow("AMBIENTE",()=>AmbientVolume,v=>SetVolume("Darklight.Ambient",v,x=>AmbientVolume=x));
                    SliderRow("INTERFACE",()=>InterfaceVolume,v=>SetVolume("Darklight.UI",v,x=>InterfaceVolume=x));break;
                case 2:
                    Button("TELA: "+(Screen.fullScreen?"CHEIA":"JANELA"),()=>{Screen.fullScreen=!Screen.fullScreen;PlayerPrefs.SetInt("Darklight.Fullscreen",Screen.fullScreen?1:0);RefreshPanel();});
                    Button("RESOLUCAO: "+Screen.width+" x "+Screen.height,()=>CycleResolution());
                    Button("SINCRONIZACAO: "+(QualitySettings.vSyncCount>0?"SIM":"NAO"),()=>{QualitySettings.vSyncCount=QualitySettings.vSyncCount>0?0:1;PlayerPrefs.SetInt("Darklight.VSync",QualitySettings.vSyncCount);RefreshPanel();});
                    Button("LIMITE: "+(Application.targetFrameRate<=0?"LIVRE":Application.targetFrameRate+" FPS"),()=>{Application.targetFrameRate=Application.targetFrameRate==60?120:60;PlayerPrefs.SetInt("Darklight.FrameRate",Application.targetFrameRate);RefreshPanel();});break;
                case 3:
                    Button("REMAPEAR COMANDOS",()=>Open("Controls"));
                    Button("RESTAURAR COMANDOS",()=>{lab.Input.Asset.RemoveAllBindingOverrides();PlayerPrefs.DeleteKey("Darklight.Bindings");RefreshPanel();});break;
                case 4:
                    Button("FLASHES REDUZIDOS: "+(ReducedFlashes?"SIM":"NAO"),()=>{ReducedFlashes=!ReducedFlashes;PlayerPrefs.SetInt("Darklight.Flashes",ReducedFlashes?1:0);RefreshPanel();});
                    Button("TREMER CAMERA: "+(PlayerPrefs.GetInt("Darklight.Shake",1)==1?"SIM":"NAO"),()=>{PlayerPrefs.SetInt("Darklight.Shake",1-PlayerPrefs.GetInt("Darklight.Shake",1));RefreshPanel();});break;
                case 5:
                    Button("EXPLORAR REGIOES",()=>Open("Regions"));Button("CREDITOS",()=>Open("Credits"));break;
            }
            Button("VOLTAR",()=>Open(returnPanel));
        }
        void ApplySavedVideoPreferences()
        {
            if(Application.isEditor||DarklightAutomation.Silent)return;
            var args=Environment.GetCommandLineArgs();bool explicitSize=Array.Exists(args,x=>x=="-screen-width"||x=="-screen-height"||x=="-screen-fullscreen");
            if(!explicitSize&&(PlayerPrefs.HasKey("Darklight.Width")||PlayerPrefs.HasKey("Darklight.Fullscreen")))
            {int width=PlayerPrefs.GetInt("Darklight.Width",1280),height=PlayerPrefs.GetInt("Darklight.Height",720);bool valid=Array.Exists(WindowSizes,size=>size.x==width&&size.y==height);Screen.SetResolution(valid?width:1280,valid?height:720,PlayerPrefs.GetInt("Darklight.Fullscreen",0)==1);}
            if(PlayerPrefs.HasKey("Darklight.VSync"))QualitySettings.vSyncCount=Mathf.Clamp(PlayerPrefs.GetInt("Darklight.VSync"),0,1);
            if(PlayerPrefs.HasKey("Darklight.FrameRate"))Application.targetFrameRate=PlayerPrefs.GetInt("Darklight.FrameRate")==120?120:60;
        }
        void CycleResolution()
        {
            int closest=0;for(int i=0;i<WindowSizes.Length;i++)if(Mathf.Abs(WindowSizes[i].x-Screen.width)<Mathf.Abs(WindowSizes[closest].x-Screen.width))closest=i;
            int next=(closest+settingsDirection+WindowSizes.Length)%WindowSizes.Length;Screen.SetResolution(WindowSizes[next].x,WindowSizes[next].y,Screen.fullScreen);
            PlayerPrefs.SetInt("Darklight.Width",WindowSizes[next].x);PlayerPrefs.SetInt("Darklight.Height",WindowSizes[next].y);RefreshPanel();
        }
        bool TickSettingsPointer(int index,Vector2 mouse,bool click)
        {
            if(panel!="Options"||!settingsSliders.TryGetValue(index,out var slider)||!click&&!Mouse.current.leftButton.isPressed)return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(buttons[index].rect,mouse,null,out var point);
            if(point.x<245)return false;slider.write(Mathf.Clamp01((point.x-257)/105));buttons[index].text.text=new[]{"MUSICA","EFEITOS","AMBIENTE","INTERFACE"}[index-6]+"     "+Mathf.RoundToInt(slider.read()*100)+"%";return true;
        }
    }
}
