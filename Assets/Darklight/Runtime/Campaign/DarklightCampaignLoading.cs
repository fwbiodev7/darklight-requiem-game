using System;
using System.Collections;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        static readonly string[] PhaseTeasers={
            "Quando as luzes se apagam, uma espada ainda responde.",
            "Entre os discos esquecidos, as vozes perdidas ainda procuram quem as escute.",
            "As pedras da catedral lembram um sangue que você ainda não compreende.",
            "Atrás das portas do laboratório, alguém tentou transformar o Véu em uma arma.",
            "Do outro lado da ponte, até as memórias podem ganhar forma.",
            "No alto de Vespera, o eclipse esconde mais do que seu criador prometeu.",
            "Uma inscrição antiga conhece o símbolo da sua espada.",
            "A cidade perdeu o amanhecer. Uma luz ainda resiste no abrigo.",
            "No silêncio do trono, uma voz familiar pode responder de outro jeito.",
            "No coração do eclipse, o futuro dos dois mundos espera uma escolha."
        };
        RectTransform loadingRoot;UnityEngine.UI.Image loadingPreview,loadingPulse,loadingGem;
        UnityEngine.UI.Text loadingTitle,loadingChapter,loadingTeaser,loadingState,loadingHint;
        bool phaseLoading,loadingReady,loadingContinue;float loadingStarted;
        readonly DarklightLoadingProgress loadingProgress=new();int pendingMapChapter=-1;
        float loadingRailLeft=48,loadingRailTop=320,loadingRailWidth=544,loadingPulseHeight=3;
        public float LoadingProgress=>loadingProgress.Value;
        void SetLoadingProgress(float progress,string stage){if(phaseLoading)loadingProgress.Report(progress,stage);}
        public bool PhaseLoading=>phaseLoading;
        public int LoadingChapter{get;private set;}=-1;
        public static string PhaseTeaser(int chapter)=>PhaseTeasers[Mathf.Clamp(chapter,0,PhaseTeasers.Length-1)];
        void EnsureLoadingScreen()
        {
            if(loadingRoot!=null)return;
            loadingRoot=RectAt(uiRoot,"PassagemDeFase",0,0,640,360);
            var bg=loadingRoot.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new Color(.018f,.022f,.035f,1);bg.raycastTarget=true;
            var gothic=Resources.Load<Sprite>("Darklight/Loading/gothic-loading-20261117");
            bool themed=gothic!=null;
            if(themed){bg.sprite=gothic;bg.color=Color.white;bg.preserveAspect=true;loadingRailLeft=88;loadingRailTop=310;loadingRailWidth=504;loadingPulseHeight=5;}
            ImageAt(loadingRoot,"LinhaSuperior",28,27,584,1,null).color=new Color(.34f,.23f,.38f);
            var label=TextAt(loadingRoot,"ProximaFase",28,12,584,11,5);label.text="ATRAVESSANDO O VÉU  /  PRÓXIMA FASE";label.color=new Color(.75f,.61f,.78f);
            loadingTitle=TextAt(loadingRoot,"Destino",28,38,584,20,11);loadingTitle.alignment=TextAnchor.MiddleCenter;
            loadingChapter=TextAt(loadingRoot,"Capitulo",28,63,584,12,5);loadingChapter.alignment=TextAnchor.MiddleCenter;loadingChapter.color=new Color(.63f,.67f,.75f);
            float mapX=themed?60:28,mapY=themed?77:84,mapW=themed?322:366,mapH=themed?207:232;
            ImageAt(loadingRoot,"BordaDoMapa",mapX,mapY,mapW,mapH,null).color=new Color(.27f,.3f,.4f);
            ImageAt(loadingRoot,"FundoDoMapa",mapX+1,mapY+1,mapW-2,mapH-2,null).color=new Color(.025f,.03f,.045f);
            loadingPreview=ImageAt(loadingRoot,"PreviaDoMapa",mapX+1,mapY+1,mapW-2,mapH-2,null);loadingPreview.preserveAspect=true;loadingPreview.material=null;
            loadingPreview.rectTransform.pivot=new Vector2(.5f,.5f);loadingPreview.rectTransform.anchoredPosition=new Vector2(mapX+mapW*.5f,-mapY-mapH*.5f);
            float teaserX=themed?408:416,teaserW=themed?164:192;
            var omen=TextAt(loadingRoot,"TituloDoPressagio",teaserX,113,teaserW,13,5);omen.text="DO OUTRO LADO...";omen.color=new Color(.75f,.61f,.78f);
            loadingTeaser=TextAt(loadingRoot,"Pressagio",teaserX,135,teaserW,themed?125:137,themed?7:8);loadingTeaser.alignment=TextAnchor.MiddleLeft;loadingTeaser.horizontalOverflow=HorizontalWrapMode.Wrap;loadingTeaser.verticalOverflow=VerticalWrapMode.Truncate;
            if(themed)
            {
                ImageAt(loadingRoot,"MolduraDoSelo",loadingRailLeft-2,loadingRailTop-2,loadingRailWidth+4,9,null).color=new Color(.49f,.28f,.56f);
                ImageAt(loadingRoot,"InteriorDoSelo",loadingRailLeft-1,loadingRailTop-1,loadingRailWidth+2,7,null).color=new Color(.025f,.012f,.035f);
            }
            ImageAt(loadingRoot,"Trilho",loadingRailLeft,loadingRailTop,loadingRailWidth,loadingPulseHeight,null).color=themed?new Color(.12f,.025f,.065f):new Color(.17f,.19f,.27f);
            loadingPulse=ImageAt(loadingRoot,"Pulso",loadingRailLeft,loadingRailTop,44,loadingPulseHeight,null);loadingPulse.color=themed?new Color(.78f,.045f,.17f):new Color(.72f,.35f,.66f);
            if(themed)
            {
                var shine=ImageAt(loadingPulse.rectTransform,"ReflexoCarmesim",0,0,0,1,null);shine.color=new Color(1,.32f,.42f);
                shine.rectTransform.anchorMax=new Vector2(1,1);
                loadingGem=ImageAt(loadingRoot,"PontaDoSelo",0,0,4,4,null);loadingGem.color=new Color(.85f,.28f,.46f);
                loadingGem.rectTransform.pivot=new Vector2(.5f,.5f);loadingGem.rectTransform.localRotation=Quaternion.Euler(0,0,45);
                ImageAt(loadingGem.rectTransform,"Nucleo",1,1,2,2,null).color=new Color(1,.82f,.9f);
            }
            loadingState=TextAt(loadingRoot,"Estado",themed?88:48,themed?324:333,themed?270:360,12,5);loadingState.color=new Color(.7f,.74f,.81f);
            loadingHint=TextAt(loadingRoot,"Continuar",366,themed?324:333,226,12,5);loadingHint.alignment=TextAnchor.MiddleRight;
            loadingRoot.gameObject.SetActive(false);
            DarklightLoadingMascot.Attach(loadingRoot,themed?new Vector2(48,-298):new Vector2(6,-322));
        }
        void ShowLoadingScreen(int chapter)
        {
            EnsureLoadingScreen();LoadingChapter=chapter;loadingReady=false;loadingContinue=false;loadingStarted=Time.unscaledTime;loadingProgress.Reset();
            panel="Loading";lab.SetPaused(true);AudioDirector?.StopCombat();music.Pause();ambient.Pause();
            SetHUDVisible(false);overlay.gameObject.SetActive(false);loadingRoot.gameObject.SetActive(true);loadingRoot.SetAsLastSibling();
            loadingTitle.text=DarklightStory.Places[chapter].ToUpperInvariant();loadingChapter.text="CAPÍTULO "+(chapter+1)+"  ·  "+DarklightStory.Chapters[chapter];
            loadingTeaser.text="“"+PhaseTeaser(chapter)+"”";loadingPreview.sprite=null;loadingPreview.enabled=false;TickLoadingScreen();
        }
        void TickLoadingScreen()
        {
            if(!phaseLoading||loadingRoot==null||!loadingRoot.gameObject.activeSelf)return;loadingRoot.SetAsLastSibling();
            // A dynamic font atlas can be rebuilt after a new chapter introduces glyphs.
            if(font!=null&&font.material.mainTexture!=null)font.material.mainTexture.filterMode=FilterMode.Point;
            float elapsed=Time.unscaledTime-loadingStarted;
            loadingState.text=loadingProgress.Stage+(loadingReady?"":new string('.',(int)(elapsed*3)%4));
            loadingPulse.rectTransform.anchoredPosition=new Vector2(loadingRailLeft,-loadingRailTop);loadingPulse.rectTransform.sizeDelta=new Vector2(loadingRailWidth*loadingProgress.Value,loadingPulseHeight);
            if(loadingGem!=null){loadingGem.enabled=loadingProgress.Value>0;loadingGem.rectTransform.anchoredPosition=new Vector2(Mathf.Round(loadingRailLeft+loadingRailWidth*loadingProgress.Value),-loadingRailTop-loadingPulseHeight*.5f);loadingGem.gameObject.SetActive(loadingProgress.Value>0);}
            loadingHint.text=Mathf.RoundToInt(loadingProgress.Value*100)+"%"+(loadingReady?(lab.Input.UsingGamepad?"  A: CONTINUAR":"  ENTER: CONTINUAR"):"");
            if(loadingReady&&elapsed>.65f&&(lab.Input.Menu("Submit").WasPressedThisFrame()||UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame==true))loadingContinue=true;
        }
        void BeginPhaseTransition(int chapter,Action load,Action ready)
        {
            if(phaseLoading)return;if(chapter<0||chapter>=DarklightStory.Chapters.Length)throw new ArgumentOutOfRangeException(nameof(chapter));
            phaseLoading=true;StartCoroutine(PortalPhase(chapter,load,ready));
        }
        IEnumerator LoadPhase(int chapter,Action load,Action ready)
        {
            // Present the loading screen before the synchronous map construction begins.
            yield return null;yield return null;
            var preview=Resources.LoadAsync<Sprite>("Darklight/Loading/preview"+(chapter+1));yield return preview;
            loadingPreview.sprite=preview.asset as Sprite;loadingPreview.enabled=loadingPreview.sprite!=null;SetLoadingProgress(.04f,"CARREGANDO O DESTINO");
            var map=Resources.LoadAsync<Sprite>("Darklight/Maps/map"+(chapter+1));yield return map;SetLoadingProgress(.10f,"CENÁRIO CARREGADO");yield return null;
            Exception failure=null;string progressBefore=JsonUtility.ToJson(Progress);
            pendingMapChapter=-1;try{load();}catch(Exception ex){failure=ex;}
            if(failure==null&&pendingMapChapter>=0)yield return DarklightLoadingSequence.Run(BuildMapInStages(pendingMapChapter),error=>failure=error);
            if(failure==null)
            {
                loadingProgress.Complete();panel="Loading";lab.SetPaused(true);SetHUDVisible(false);overlay.gameObject.SetActive(false);loadingRoot.gameObject.SetActive(true);loadingReady=true;
                float readyAt=Time.unscaledTime;while(Time.unscaledTime-readyAt<.8f&&!loadingContinue)yield return null;
            }
            phaseLoading=false;LoadingChapter=-1;loadingRoot.gameObject.SetActive(false);Close();
            if(failure!=null){Debug.LogException(failure);Progress=JsonUtility.FromJson<DarklightProgress>(progressBefore);LoadMap(Progress.chapter);Open("Pause");Notice("Não foi possível preparar a próxima fase. Seu ponto anterior foi preservado.");yield break;}
            ready?.Invoke();
        }
        void TravelToChapter(int chapter)
        {
            BeginPhaseTransition(chapter,()=>{StopPower();SaveCheckpoint();Progress.chapter=chapter;Progress.checkpoint=0;Progress.bossPhase=1;Progress.missionWave=0;Progress.completed=false;LoadMap(chapter);},()=>{SaveCheckpoint();Speak(DarklightStory.Introductions[chapter]);});
        }
    }
}

