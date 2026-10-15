using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public float AmbientVolume=.45f,InterfaceVolume=.35f;
        int optionsCategory;DarklightPauseBlur pauseBlur;
        RectTransform pauseSceneRoot;UnityEngine.UI.Text pauseLocation,skinDescription,skinEquipped;
        UnityEngine.UI.Image skinPreview;
        Sprite[] requiemPreviewIdle;
        readonly Dictionary<string,Sprite[]> skinPreviewCache=new();
        readonly Dictionary<int,AudioSliderRow> settingsSliders=new();
        int draggedAudioSlider=-1;bool audioPreferencesDirty;float audioSaveAt;
        sealed class AudioSliderRow {public UnityEngine.UI.Slider slider;public UnityEngine.UI.Text percent;public RectTransform track;public Func<float> read;public Action<float> write;}
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
            AmbientVolume=PlayerPrefs.GetFloat("Darklight.Ambient",.45f);InterfaceVolume=PlayerPrefs.GetFloat("Darklight.UI",.35f);InitializeAudioPreferences();
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
                int preview=selection<2?selection:SelectedSkin;
                skinPreview.sprite=TodayCharacterBank.YoungIdle(preview,0,0,Time.unscaledTime)??TodayCharacterBank.StablePose(preview,0,0,0);
                skinDescription.text=(preview==1?"ROCK\n\nColete de couro, rebites e correntes. Presenca direta e rebelde.":"REQUIEM\n\nSobretudo escuro e katana ancestral. Atravessa Vespera entre o sangue e o eclipse.");
                skinEquipped.text=SelectedSkin==preview?"EQUIPADO":"ENTER / A: EQUIPAR";skinEquipped.color=SelectedSkin==preview?new Color(.72f,.08f,.15f):new Color(.84f,.82f,.78f);
            }
            foreach(var row in settingsSliders.Values)if(row.slider!=null){int percent=VolumePercent(row.read());row.slider.SetValueWithoutNotify(percent);row.percent.text=percent+"%";}
            if(audioPreferencesDirty&&Time.unscaledTime>=audioSaveAt&&(Mouse.current==null||!Mouse.current.leftButton.isPressed))FlushAudioPreferences();
        }
        void BuildVisualPicker()
        {
            heading.text="VISUAL";bodyText.text="";
            Button("REQUIEM",()=>{SelectSkin(0);RefreshPanel();});Button("ROCK",()=>{SelectSkin(1);RefreshPanel();});Button("VOLTAR",()=>Open(returnPanel));
        }
        void SliderRow(string name,Func<float> read,Action<float> write)
        {
            int index=buttons.Count;
            Button(name,()=>write((VolumePercent(read())+settingsDirection)/100f));
            var root=buttons[index].rect;
            var percent=TextAt(root,"Percentual",324,0,40,12,6);percent.alignment=TextAnchor.MiddleRight;
            var track=RectAt(root,"ControleVolume",22,13,328,12);
            var back=ImageAt(track,"Trilho",0,4,328,4,null);back.color=new Color(.16f,.17f,.19f);
            var fillArea=RectAt(track,"AreaPreenchimento",0,4,328,4);
            var fill=ImageAt(fillArea,"Preenchimento",0,0,328,4,null);fill.color=new Color(.72f,.07f,.15f);
            fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
            var handleArea=RectAt(track,"AreaAlca",4,1,320,10);
            var handle=ImageAt(handleArea,"Alca",0,0,8,10,null);handle.color=new Color(.95f,.9f,.84f);
            handle.rectTransform.anchorMin=handle.rectTransform.anchorMax=new Vector2(0,.5f);handle.rectTransform.pivot=new Vector2(.5f,.5f);handle.rectTransform.anchoredPosition=Vector2.zero;handle.rectTransform.sizeDelta=new Vector2(8,0);
            var slider=track.gameObject.AddComponent<UnityEngine.UI.Slider>();
            slider.minValue=0;slider.maxValue=100;slider.wholeNumbers=true;slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;
            slider.direction=UnityEngine.UI.Slider.Direction.LeftToRight;slider.transition=UnityEngine.UI.Selectable.Transition.None;
            slider.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
            slider.SetValueWithoutNotify(VolumePercent(read()));
            slider.onValueChanged.AddListener(value=>write(value/100f));
            settingsSliders[index]=new AudioSliderRow{slider=slider,percent=percent,track=track,read=read,write=write};
        }
        void SetVolume(string key,float value,Action<float> assign)
        {
            value=VolumePercent(value)/100f;assign(value);PlayerPrefs.SetFloat(key,value);audioPreferencesDirty=true;audioSaveAt=Time.unscaledTime+.3f;
        }
        void FlushAudioPreferences(){if(!audioPreferencesDirty)return;PlayerPrefs.Save();audioPreferencesDirty=false;}
        bool TickAudioDrag(Vector2 mouse)
        {
            if(draggedAudioSlider<0)return false;
            if(panel!="Options"||!settingsSliders.TryGetValue(draggedAudioSlider,out var row)){draggedAudioSlider=-1;FlushAudioPreferences();return false;}
            if(Mouse.current==null||!Mouse.current.leftButton.isPressed){draggedAudioSlider=-1;FlushAudioPreferences();return false;}
            WriteAudioPointer(row,mouse);return true;
        }
        void WriteAudioPointer(AudioSliderRow row,Vector2 mouse)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(row.track,mouse,null,out var p);
            float value=Mathf.Clamp01((p.x-4)/Mathf.Max(1,row.track.rect.width-8));int percent=VolumePercent(value);
            if(percent!=VolumePercent(row.read()))row.write(percent/100f);
            row.slider.SetValueWithoutNotify(percent);row.percent.text=percent+"%";
        }
        void LayoutAudioSlider(int index,float width)
        {
            if(!settingsSliders.TryGetValue(index,out var row))return;
            var label=buttons[index].text;label.fontSize=6;label.rectTransform.anchoredPosition=new Vector2(22,0);label.rectTransform.sizeDelta=new Vector2(width-78,12);
            row.percent.rectTransform.anchoredPosition=new Vector2(width-48,0);
            row.track.sizeDelta=new Vector2(width-44,12);
            var back=row.track.Find("Trilho") as RectTransform;back.sizeDelta=new Vector2(width-44,4);
            (row.track.Find("AreaPreenchimento") as RectTransform).sizeDelta=new Vector2(width-44,4);
            (row.track.Find("AreaAlca") as RectTransform).sizeDelta=new Vector2(width-52,12);
        }
        void BuildSettings()
        {
            heading.text="OPCOES";bodyText.text="";FlushAudioPreferences();draggedAudioSlider=-1;settingsSliders.Clear();
            for(int i=0;i<6;i++){int tab=i;Button(SettingsCategories[i],()=>{optionsCategory=tab;RefreshPanel();selection=tab;});}
            switch(optionsCategory)
            {
                case 0:
                    Button("DICAS DE COMBATE: "+(lab.Coach?.TipsEnabled==true?"SIM":"NAO"),()=>{lab.Coach?.ToggleTips();RefreshPanel();});
                    Button("REVER TREINO CONTEXTUAL",()=>{lab.Coach?.Restart();Notice("DICAS REINICIADAS");RefreshPanel();});break;
                case 1:
                    SliderRow("VOLUME GERAL",()=>MasterVolume,v=>SetVolume("Darklight.Master",v,x=>{MasterVolume=x;AudioListener.volume=DarklightAutomation.Silent?0:x;}));
                    SliderRow("MUSICA",()=>MusicVolume,v=>SetVolume("Darklight.Music",v,x=>MusicVolume=x));
                    SliderRow("EFEITOS",()=>EffectsVolume,v=>SetVolume("Darklight.Effects",v,x=>EffectsVolume=x));
                    SliderRow("AMBIENTE",()=>AmbientVolume,v=>SetVolume("Darklight.Ambient",v,x=>AmbientVolume=x));
                    SliderRow("INTERFACE",()=>InterfaceVolume,v=>SetVolume("Darklight.UI",v,x=>InterfaceVolume=x));
                    SliderRow("INIMIGOS",()=>EnemyVolume,v=>SetVolume("Darklight.Enemies",v,x=>EnemyVolume=x));
                    SliderRow("PERSONAGEM",()=>PlayerVolume,v=>SetVolume("Darklight.Player",v,x=>PlayerVolume=x));break;
                case 2:
                    Button("QUALIDADE: "+DarklightRemaster.QualityNames[DarklightRemaster.QualityLevel],()=>{GetComponent<DarklightRemaster>()?.SetQuality((DarklightRemaster.QualityLevel+settingsDirection+4)%4);RefreshPanel();});
                    Button("TELA: "+(Screen.fullScreen?"CHEIA":"JANELA"),()=>{Screen.fullScreen=!Screen.fullScreen;PlayerPrefs.SetInt("Darklight.Fullscreen",Screen.fullScreen?1:0);RefreshPanel();});
                    Button("RESOLUCAO: "+Screen.width+" x "+Screen.height,()=>CycleResolution());
                    Button("SINCRONIZACAO: "+(QualitySettings.vSyncCount>0?"SIM":"NAO"),()=>{QualitySettings.vSyncCount=QualitySettings.vSyncCount>0?0:1;PlayerPrefs.SetInt("Darklight.VSync",QualitySettings.vSyncCount);RefreshPanel();});
                    Button("LIMITE: "+(Application.targetFrameRate<=0?"LIVRE":Application.targetFrameRate+" FPS"),()=>{Application.targetFrameRate=Application.targetFrameRate==60?120:60;PlayerPrefs.SetInt("Darklight.FrameRate",Application.targetFrameRate);RefreshPanel();});break;
                case 3:
                    Button("REMAPEAR COMANDOS",()=>Open("Controls"));
                    Button("RESTAURAR COMANDOS",()=>{lab.Input.RestoreBindings();RefreshPanel();});break;
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
            if(panel!="Options"||!click||!settingsSliders.TryGetValue(index,out var row))return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(row.track,mouse,null,out var point);
            if(point.y>4||point.y<-16)return false;
            draggedAudioSlider=index;WriteAudioPointer(row,mouse);return true;
        }
    }
}
