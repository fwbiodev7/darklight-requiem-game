using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        readonly Dictionary<UnityEngine.UI.Image,(UnityEngine.UI.Image[] edges,UnityEngine.UI.Text cursor)> menuBorders=new();
        RectTransform cinematicRoot;
        UnityEngine.UI.Image gameLogo;
        UnityEngine.UI.Text menuSubtitle,menuTagline,menuSave,menuVersion;
        CanvasGroup menuFade;
        DarklightMenuBackdrop cinematicBackdrop;
        float menuSoundAt;
        bool CinematicFront=>panel=="Main"||panel=="Skins"&&returnPanel=="Main"||panel=="ConfirmNew"||panel=="LoadGames"||panel=="Extras"||panel=="Regions"||panel=="RegionDetails"||((panel=="Options"||panel=="Controls"||panel=="Credits"||panel=="BindingDevice"||panel=="BindingSlot"||panel=="BindingConflict")&&returnPanel=="Main");

        bool CinematicPause=>panel=="Skins"&&returnPanel=="Pause"||panel=="Pause"||panel=="Transformations"||panel=="Skills"||panel=="VampireSkills"||panel=="Inventory"||panel=="Weapons"||panel=="AbyssHelp"||panel=="Journal"||panel=="MP3"||panel=="Playtest"||((panel=="Options"||panel=="Controls"||panel=="BindingDevice"||panel=="BindingSlot"||panel=="BindingConflict")&&returnPanel=="Pause");

        void CreateCinematicMenu()
        {
            cinematicRoot=RectAt(overlay,"VesperaCinematografica",0,0,640,360);
            cinematicRoot.SetAsFirstSibling();
            cinematicBackdrop=cinematicRoot.gameObject.AddComponent<DarklightMenuBackdrop>();
            cinematicBackdrop.Initialize();
            menuSubtitle=TextAt(menuRoot,"SubtituloRequiem",28,90,240,16,12);
            menuSubtitle.text="R E Q U I E M";
            gameLogo=ImageAt(menuRoot,"LogoDarklight",28,32,260,87,Resources.Load<Sprite>("Darklight/Branding/GameLogo"));gameLogo.preserveAspect=true;gameLogo.raycastTarget=false;
            menuTagline=TextAt(menuRoot,"FraseTematica",28,110,270,12,7);
            menuTagline.text="Entre o sangue e o eclipse.";
            menuVersion=TextAt(menuRoot,"Versao",553,339,75,10,5);menuVersion.text="BETA 1.0";menuVersion.alignment=TextAnchor.MiddleRight;
            menuFade=menuRoot.gameObject.AddComponent<CanvasGroup>();
        }
        public static int MenuProgressPercent(DarklightProgress saved)
        {
            if(saved==null)return 0;
            if(saved.completed)return 100;
            return Mathf.Clamp(Mathf.FloorToInt((saved.chapter+saved.checkpoint/4f)/DarklightStory.Chapters.Length*100),0,99);
        }
        public static string MenuSaveSummary(DarklightProgress saved)=>saved==null?"NENHUMA CAMPANHA SALVA":
            "CAP. "+(saved.chapter+1)+" | "+DarklightStory.Places[saved.chapter]+" | "+MenuProgressPercent(saved)+"%";
        void BuildCinematicMain()
        {
            var saved=DarklightProgress.Load();
            Button("CONTINUAR",ContinueGame,saved!=null);
            menuSave=TextAt(buttons[0].rect,"ResumoDoSave",22,21,234,10,4);
            menuSave.text=MenuSaveSummary(saved);menuSave.color=saved!=null?new Color(.66f,.68f,.69f):new Color(.38f,.4f,.42f);
            Button("NOVO JOGO",()=>{difficultyForNewGame=true;returnPanel="Main";Open("Difficulty");});
            Button("CARREGAR JOGO",()=>Open("LoadGames"));
            Button("ARENA DE TREINAMENTO",EnterTraining);
            Button("CONFIGURAÇÕES",()=>Open("Options"));
            Button("TROCAR SKIN",()=>Open("Skins"));
            Button("CRÉDITOS",()=>Open("Credits"));
            Button("SAIR",()=>Application.Quit());
        }
        static DarklightProgress ReadMenuSave(string path)
        {
            try{return File.Exists(path)?DarklightProgress.Decode(File.ReadAllText(path)):null;}
            catch(Exception){return null;}
        }
        void BuildSavePicker()
        {
            heading.text="CARREGAR JOGO";
            bodyText.text="Escolha a campanha atual ou a copia anterior.\n\nCarregar preserva os atributos, armas e memorias do arquivo escolhido.";
            var current=ReadMenuSave(DarklightProgress.Path);var backup=ReadMenuSave(DarklightProgress.Path+".bak");
            Button("SAVE ATUAL",()=>LoadMenuSave(DarklightProgress.Path),current!=null);
            Button("COPIA ANTERIOR",()=>LoadMenuSave(DarklightProgress.Path+".bak"),backup!=null);
            Button("VOLTAR",()=>Open(returnPanel));
            for(int i=0;i<2;i++)
            {
                var label=TextAt(buttons[i].rect,"ResumoDoArquivo",22,19,232,10,4);
                label.text=MenuSaveSummary(i==0?current:backup);label.color=new Color(.6f,.62f,.66f);
            }
        }
        void LoadMenuSave(string path)
        {
            var saved=ReadMenuSave(path);
            if(saved==null){RefreshPanel();return;}
            StopPower();StopMenuMusic();Progress=saved;LoadMap(saved.chapter);
            if(saved.completed)Open("Victory");else Close();
        }
        void AddCinematicBorder(UnityEngine.UI.Image image,UnityEngine.UI.Text label,int i)
        {
            image.sprite=null;image.material=null;image.type=UnityEngine.UI.Image.Type.Simple;
            image.color=new Color(.025f,.03f,.045f,.8f);
            var edges=new UnityEngine.UI.Image[4];
            for(int n=0;n<4;n++){edges[n]=ImageAt(image.transform,"Borda"+n,0,0,1,1,null);edges[n].color=new Color(.27f,.3f,.32f);}
            var cursor=TextAt(image.transform,"Selecao",7,0,10,24,7);cursor.text=">";cursor.alignment=TextAnchor.MiddleCenter;
            label.alignment=TextAnchor.MiddleLeft;label.fontSize=7;
            label.rectTransform.anchoredPosition=new Vector2(22,0);label.rectTransform.sizeDelta=new Vector2(230,24);
            menuBorders[image]=(edges,cursor);
        }
        void LayoutCinematicPanel(bool front)
        {
            bool paused=CinematicPause;bool themed=front||paused;
            cinematicRoot.gameObject.SetActive(front);menuBackdrop.enabled=!front;
            if(paused){menuBackdrop.sprite=null;menuBackdrop.color=new Color(.015f,.012f,.025f,.55f);}
            menuRoot.Find("Moldura").gameObject.SetActive(!themed);
            bool brandedMain=front&&panel=="Main"&&gameLogo.sprite!=null;
            gameLogo.gameObject.SetActive(brandedMain);heading.gameObject.SetActive(!brandedMain);
            menuSubtitle.gameObject.SetActive(front&&panel=="Main"&&!brandedMain);
            menuSubtitle.text=paused?"DARKLIGHT: REQUIEM":"R E Q U I E M";
            menuSubtitle.fontSize=paused?9:12;menuSubtitle.rectTransform.anchoredPosition=new Vector2(28,paused?-58:-90);menuSubtitle.rectTransform.sizeDelta=new Vector2(290,16);menuTagline.gameObject.SetActive(front&&panel=="Main");menuTagline.rectTransform.anchoredPosition=new Vector2(28,brandedMain?-122:-110);menuVersion.gameObject.SetActive(front);
            SetHUDVisible(!themed);menuFade.alpha=themed?.7f:1;
            menuRoot.anchoredPosition=themed?Vector2.zero:new Vector2(24,-16);menuRoot.sizeDelta=themed?new Vector2(640,360):new Vector2(592,328);
            heading.rectTransform.anchoredPosition=themed?new Vector2(28,paused?-34:-58):new Vector2(22,-18);
            heading.rectTransform.sizeDelta=themed?new Vector2(320,36):new Vector2(548,24);heading.fontSize=paused?18:front?14:10;
            menuFooter.rectTransform.anchoredPosition=themed?new Vector2(28,-338):new Vector2(22,-305);menuFooter.rectTransform.sizeDelta=themed?new Vector2(505,12):new Vector2(548,12);menuFooter.fontSize=themed?4:6;
            bodyText.gameObject.SetActive(panel!="Main");
            if(paused)
            {
                bodyText.rectTransform.anchoredPosition=new Vector2(328,-137);bodyText.rectTransform.sizeDelta=new Vector2(278,175);bodyText.fontSize=6;bodyText.lineSpacing=1.7f;
                bodyText.horizontalOverflow=HorizontalWrapMode.Wrap;bodyText.verticalOverflow=VerticalWrapMode.Truncate;
                portrait.gameObject.SetActive(false);
            }
            if(front)
            {
                bodyText.rectTransform.anchoredPosition=new Vector2(306,-125);bodyText.rectTransform.sizeDelta=new Vector2(146,156);bodyText.fontSize=6;bodyText.lineSpacing=1.7f;
                bodyText.horizontalOverflow=HorizontalWrapMode.Wrap;bodyText.verticalOverflow=VerticalWrapMode.Truncate;
                if(panel=="Main"){heading.text="DARKLIGHT";heading.fontSize=28;heading.rectTransform.anchoredPosition=new Vector2(28,-48);heading.rectTransform.sizeDelta=new Vector2(290,36);}
                if(panel=="Skins"||panel=="Credits"){bodyText.rectTransform.anchoredPosition=new Vector2(28,-224);bodyText.rectTransform.sizeDelta=new Vector2(276,102);}
                portrait.gameObject.SetActive(false);
                cinematicBackdrop.SetSkin(SelectedSkin);
            }
            for(int i=0;i<buttons.Count;i++)StyleCinematicButton(i,Mathf.Max(0,selection-7));
        }
        void StyleCinematicButton(int i,int first)
        {
            if(panel=="Death"&&!Training){StyleGameOverButton(i);return;}
            if(panel=="Pause"||panel=="Options")first=0;var b=buttons[i];b.rect.gameObject.SetActive(panel=="Options"||panel=="Pause"?true:i>=first&&i<first+8);
            if(!CinematicFront&&!CinematicPause&&panel!="Options")
            {
                b.rect.anchoredPosition=new Vector2(22,-(62+(i-first)*28));b.image.sprite=UIArt(!b.enabled?3:i==selection?2:1,extras[7]);b.image.color=Color.white;b.text.color=b.enabled?new Color(.96f,.9f,.86f):Color.gray;return;
            }
            bool tall=panel=="Main"&&i==0||panel=="LoadGames"&&i<2;
            float y=CinematicPause?96+(i-first)*20:panel=="Main"?(i==0?137:176+(i-1)*22):panel=="LoadGames"?116+(i-first)*41:116+(i-first)*27;
            float width=panel=="Pause"?214:260;float h=panel=="Pause"?20:tall?34:22;
            if(panel=="Options"){width=i<6?178:372;y=i<6?101+i*27:101+(i-6)*(optionsCategory==1?29:30);if(settingsSliders.ContainsKey(i))h=26;}
            if(panel=="Skins"){width=208;y=108+(i-first)*32;h=26;}
            b.rect.anchoredPosition=new Vector2(panel=="Options"&&i>=6?236:28,-y);b.rect.sizeDelta=new Vector2(width,h);
            b.text.rectTransform.sizeDelta=new Vector2(width-28,tall?20:h);LayoutAudioSlider(i,width);
            if(panel=="Controls"||panel=="BindingSlot"||panel=="BindingConflict"){b.text.fontSize=5;bodyText.rectTransform.anchoredPosition=new Vector2(328,-137);bodyText.rectTransform.sizeDelta=new Vector2(250,175);menuFooter.text="SETAS / LS / RODA: ESCOLHER    ENTER / A: OK    ESC / B: VOLTAR";}
            bool focused=b.enabled&&(i==selection||panel=="Options"&&i==optionsCategory);
            b.image.color=Color.Lerp(b.image.color,focused?new Color(.19f,.035f,.06f,.76f):new Color(.025f,.03f,.045f,.8f),Mathf.Min(1,Time.unscaledDeltaTime*14));
            b.text.color=b.enabled?new Color(.91f,.89f,.83f):new Color(.38f,.4f,.42f);
            if(!menuBorders.TryGetValue(b.image,out var style))return;
            Color target=!b.enabled?new Color(.17f,.19f,.22f):focused?new Color(.63f,.08f,.15f):new Color(.27f,.3f,.32f);
            for(int n=0;n<4;n++)
            {
                var edge=style.edges[n];edge.color=Color.Lerp(edge.color,target,Mathf.Min(1,Time.unscaledDeltaTime*14));
                edge.rectTransform.anchoredPosition=n==0?Vector2.zero:n==1?new Vector2(0,-(h-.5f)):n==2?Vector2.zero:new Vector2(width-.5f,0);
                edge.rectTransform.sizeDelta=n<2?new Vector2(width,.5f):new Vector2(.5f,h);
            }
            style.cursor.color=focused?new Color(.78f,.1f,.18f):new Color(.38f,.42f,.44f,b.enabled?.8f:.25f);
            style.cursor.rectTransform.sizeDelta=new Vector2(10,h);
        }
        void TickCinematicMenu(){if(CinematicFront||CinematicPause)menuFade.alpha=Mathf.MoveTowards(menuFade.alpha,1,Time.unscaledDeltaTime*3);}
        void MenuSound()
        {
            if(DarklightAutomation.Silent||tones==null||InterfaceVolume<=0||Time.unscaledTime<menuSoundAt)return;
            menuSoundAt=Time.unscaledTime+.055f;AudioDirector?.Play("interface-click_",DarklightAudioCategory.Interface,.15f);
        }
    }
}


