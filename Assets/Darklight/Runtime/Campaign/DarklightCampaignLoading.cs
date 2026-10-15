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
        RectTransform loadingRoot;UnityEngine.UI.Image loadingPreview,loadingPulse;
        UnityEngine.UI.Text loadingTitle,loadingChapter,loadingTeaser,loadingState,loadingHint;
        bool phaseLoading,loadingReady,loadingContinue;float loadingStarted;
        public bool PhaseLoading=>phaseLoading;
        public int LoadingChapter{get;private set;}=-1;
        public static string PhaseTeaser(int chapter)=>PhaseTeasers[Mathf.Clamp(chapter,0,PhaseTeasers.Length-1)];
        void EnsureLoadingScreen()
        {
            if(loadingRoot!=null)return;
            loadingRoot=RectAt(uiRoot,"PassagemDeFase",0,0,640,360);
            var bg=loadingRoot.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new Color(.018f,.022f,.035f,1);bg.raycastTarget=true;
            ImageAt(loadingRoot,"LinhaSuperior",28,27,584,1,null).color=new Color(.34f,.23f,.38f);
            var label=TextAt(loadingRoot,"ProximaFase",28,12,584,11,5);label.text="ATRAVESSANDO O VÉU  /  PRÓXIMA FASE";label.color=new Color(.75f,.61f,.78f);
            loadingTitle=TextAt(loadingRoot,"Destino",28,38,584,20,11);loadingTitle.alignment=TextAnchor.MiddleCenter;
            loadingChapter=TextAt(loadingRoot,"Capitulo",28,63,584,12,5);loadingChapter.alignment=TextAnchor.MiddleCenter;loadingChapter.color=new Color(.63f,.67f,.75f);
            ImageAt(loadingRoot,"BordaDoMapa",28,84,366,232,null).color=new Color(.27f,.3f,.4f);
            ImageAt(loadingRoot,"FundoDoMapa",29,85,364,230,null).color=new Color(.025f,.03f,.045f);
            loadingPreview=ImageAt(loadingRoot,"PreviaDoMapa",29,85,364,230,null);loadingPreview.preserveAspect=true;loadingPreview.material=null;
            loadingPreview.rectTransform.pivot=new Vector2(.5f,.5f);loadingPreview.rectTransform.anchoredPosition=new Vector2(211,-200);
            var omen=TextAt(loadingRoot,"TituloDoPressagio",416,113,192,13,5);omen.text="DO OUTRO LADO...";omen.color=new Color(.75f,.61f,.78f);
            loadingTeaser=TextAt(loadingRoot,"Pressagio",416,135,192,137,8);loadingTeaser.alignment=TextAnchor.MiddleLeft;loadingTeaser.horizontalOverflow=HorizontalWrapMode.Wrap;loadingTeaser.verticalOverflow=VerticalWrapMode.Truncate;
            ImageAt(loadingRoot,"Trilho",48,321,544,1,null).color=new Color(.17f,.19f,.27f);
            loadingPulse=ImageAt(loadingRoot,"Pulso",48,320,44,3,null);loadingPulse.color=new Color(.72f,.35f,.66f);
            loadingState=TextAt(loadingRoot,"Estado",48,333,360,12,5);loadingState.color=new Color(.7f,.74f,.81f);
            loadingHint=TextAt(loadingRoot,"Continuar",366,333,226,12,5);loadingHint.alignment=TextAnchor.MiddleRight;
            loadingRoot.gameObject.SetActive(false);
        }
        void ShowLoadingScreen(int chapter)
        {
            EnsureLoadingScreen();LoadingChapter=chapter;loadingReady=false;loadingContinue=false;loadingStarted=Time.unscaledTime;
            panel="Loading";lab.SetPaused(true);AudioDirector?.StopCombat();music.Pause();ambient.Pause();
            SetHUDVisible(false);overlay.gameObject.SetActive(false);loadingRoot.gameObject.SetActive(true);loadingRoot.SetAsLastSibling();
            loadingTitle.text=DarklightStory.Places[chapter].ToUpperInvariant();loadingChapter.text="CAPÍTULO "+(chapter+1)+"  ·  "+DarklightStory.Chapters[chapter];
            loadingTeaser.text="“"+PhaseTeaser(chapter)+"”";loadingPreview.sprite=null;loadingPreview.enabled=false;TickLoadingScreen();
        }
        void TickLoadingScreen()
        {
            if(!phaseLoading)return;loadingRoot.SetAsLastSibling();
            float elapsed=Time.unscaledTime-loadingStarted;
            loadingState.text=loadingReady?"CAMINHO ABERTO":"PREPARANDO A PRÓXIMA FASE"+new string('.',(int)(elapsed*3)%4);
            loadingPulse.rectTransform.anchoredPosition=new Vector2(48+Mathf.PingPong(elapsed*.45f,1)*500,-320);
            loadingHint.text=loadingReady?(lab.Input.UsingGamepad?"A / X: CONTINUAR":"ENTER / CLIQUE: CONTINUAR"):"";
            if(loadingReady&&elapsed>.65f&&(lab.Input.Menu("Submit").WasPressedThisFrame()||UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame==true))loadingContinue=true;
        }
        void BeginPhaseTransition(int chapter,Action load,Action ready)
        {
            if(phaseLoading)return;if(chapter<0||chapter>=DarklightStory.Chapters.Length)throw new ArgumentOutOfRangeException(nameof(chapter));
            phaseLoading=true;ShowLoadingScreen(chapter);StartCoroutine(LoadPhase(chapter,load,ready));
        }
        IEnumerator LoadPhase(int chapter,Action load,Action ready)
        {
            // Present the loading screen before the synchronous map construction begins.
            yield return null;yield return null;
            loadingPreview.sprite=Resources.Load<Sprite>("Darklight/Loading/preview"+(chapter+1));loadingPreview.enabled=loadingPreview.sprite!=null;
            yield return null;yield return null;
            Exception failure=null;string progressBefore=JsonUtility.ToJson(Progress);
            try{load();}catch(Exception ex){failure=ex;Progress=JsonUtility.FromJson<DarklightProgress>(progressBefore);}
            if(failure==null)
            {
                panel="Loading";lab.SetPaused(true);SetHUDVisible(false);overlay.gameObject.SetActive(false);loadingRoot.gameObject.SetActive(true);loadingReady=true;
                while(Time.unscaledTime-loadingStarted<2.8f&&!loadingContinue)yield return null;
            }
            phaseLoading=false;LoadingChapter=-1;loadingRoot.gameObject.SetActive(false);Close();
            if(failure!=null){Debug.LogException(failure);LoadMap(Progress.chapter);Notice("Não foi possível preparar a próxima fase.");yield break;}
            ready?.Invoke();
        }
        void TravelToChapter(int chapter)
        {
            BeginPhaseTransition(chapter,()=>{StopPower();SaveCheckpoint();Progress.chapter=chapter;Progress.checkpoint=0;Progress.bossPhase=1;Progress.missionWave=0;Progress.completed=false;LoadMap(chapter);SaveCheckpoint();},()=>Speak(DarklightStory.Introductions[chapter]));
        }
    }
}

