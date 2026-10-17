using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        bool screenTransitionBusy;
        RectTransform veilRoot,cueRoot;
        CanvasGroup veilGroup,cueGroup;
        Text veilTitle,cueTitle,cueNote;
        Image cueShade;
        float cueStarted,cueDuration,battleSettleUntil;
        bool bossCue;

        void EnsureTransitionVeil()
        {
            if(veilRoot!=null)return;
            veilRoot=RectAt(uiRoot,"TransicaoAprovada",0,0,640,360);
            var bg=veilRoot.gameObject.AddComponent<Image>();bg.color=new Color(.012f,.008f,.023f);bg.raycastTarget=true;
            veilGroup=veilRoot.gameObject.AddComponent<CanvasGroup>();veilGroup.blocksRaycasts=true;
            var mist=ImageAt(veilRoot,"NevoaOriginal",0,218,640,126,Resources.Load<Sprite>("Darklight/Menu/Mist"));
            mist.material=null;mist.color=new Color(.65f,.54f,.76f,.18f);
            var seal=ImageAt(veilRoot,"SeloOriginal",278,74,84,130,Resources.Load<Sprite>("Darklight/Props/CheckpointShrine"));
            seal.preserveAspect=true;seal.material=null;
            veilTitle=TextAt(veilRoot,"DestinoDoRetorno",120,235,400,20,10);veilTitle.alignment=TextAnchor.MiddleCenter;
            var note=TextAt(veilRoot,"Continuidade",120,268,400,14,5);note.text="DARKLIGHT: REQUIEM";note.alignment=TextAnchor.MiddleCenter;note.color=new Color(.61f,.52f,.66f);
            veilRoot.gameObject.SetActive(false);
        }
        IEnumerator FadeVeil(bool covering,string label,float duration)
        {
            EnsureTransitionVeil();veilRoot.gameObject.SetActive(true);veilRoot.SetAsLastSibling();veilTitle.text=label;
            float start=Time.unscaledTime;
            while(Time.unscaledTime-start<duration)
            {
                veilGroup.alpha=DarklightTransitionTiming.Fade((Time.unscaledTime-start)/duration,covering);
                yield return null;
            }
            veilGroup.alpha=covering?1:0;
            if(!covering)veilRoot.gameObject.SetActive(false);
        }
        IEnumerator ReturnThroughSeal()
        {
            screenTransitionBusy=true;lab.SetPaused(true);lab.Player.Model.SuspendInput();
            yield return FadeVeil(true,"RETORNANDO AO ÚLTIMO SELO",.22f);
            Exception failure=null;
            try
            {
                StopPower();
                var saved=DarklightAutomation.Silent||PlaytestSession?null:DarklightProgress.Load();
                if(saved!=null)Progress=saved;
                deathMenuPending=false;LoadMap(Progress.chapter);Close();
            }
            catch(Exception ex){failure=ex;Open("Death");}
            yield return FadeVeil(false,"O CAMINHO AINDA TE ESPERA",.36f);
            screenTransitionBusy=false;menuDismissFrame=Time.frameCount;lab.Player.Model.SuspendInput();lab.SetPaused(panel!="");
            if(failure!=null){Debug.LogException(failure);Notice("Não foi possível retomar o ponto salvo.");}
        }
        IEnumerator PortalPhase(int chapter,Action load,Action ready)
        {
            screenTransitionBusy=true;lab.SetPaused(true);lab.Player.Model.SuspendInput();
            yield return FadeVeil(true,"ATRAVESSANDO O VÉU",.26f);
            ShowLoadingScreen(chapter);
            yield return LoadPhase(chapter,load,ready);
            yield return FadeVeil(false,"DO OUTRO LADO DO VÉU",.38f);
            screenTransitionBusy=false;menuDismissFrame=Time.frameCount;lab.Player.Model.SuspendInput();lab.SetPaused(panel!="");
        }
        void EnsurePresentationCue()
        {
            if(cueRoot!=null)return;
            cueRoot=RectAt(uiRoot,"ApresentacaoAprovada",0,0,640,360);
            cueRoot.SetSiblingIndex(1); // after the world viewport, before the HUD and menus
            cueGroup=cueRoot.gameObject.AddComponent<CanvasGroup>();cueGroup.blocksRaycasts=false;cueGroup.interactable=false;
            cueShade=ImageAt(cueRoot,"Penumbra",0,0,640,360,null);
            var mist=ImageAt(cueRoot,"NevoaOriginal",68,10,504,86,Resources.Load<Sprite>("Darklight/Menu/Mist"));mist.material=null;mist.color=new Color(.5f,.4f,.62f,.25f);
            cueTitle=TextAt(cueRoot,"Titulo",130,27,380,26,12);cueTitle.alignment=TextAnchor.MiddleCenter;cueTitle.color=new Color(.91f,.71f,.74f);
            cueNote=TextAt(cueRoot,"Legenda",150,58,340,12,5);cueNote.alignment=TextAnchor.MiddleCenter;
            cueRoot.gameObject.SetActive(false);
        }
        void ShowPresentationCue(string title,string note,bool boss)
        {
            EnsurePresentationCue();bossCue=boss;cueStarted=Time.unscaledTime;cueDuration=boss?1.5f:1.05f;
            cueTitle.text=title;cueTitle.fontSize=boss?12:8;cueNote.text=note;
            cueShade.color=new Color(.025f,.008f,.035f,boss?(ReducedFlashes?.06f:.13f):0);
        }
        void BeginBattleSettle(){battleSettleUntil=Time.unscaledTime+1.2f;}
        void PulseCheckpoint()
        {
            var prop=levelRoot!=null?levelRoot.Find("Checkpoint"):null;
            if(prop==null || !prop.TryGetComponent<SpriteRenderer>(out var source))return;
            var pulse=prop.GetComponent<DarklightCheckpointPulse>()??prop.gameObject.AddComponent<DarklightCheckpointPulse>();
            pulse.Present(source,ReducedFlashes);
        }
        void TickApprovedPresentation()
        {
            if(lab?.Player==null)return;
            var model=lab.Player.Model;
            if(!Active || health==null || health.IsDead)
            {if(cueRoot!=null)cueRoot.gameObject.SetActive(false);return;}
            bool moving=model.Motion.sqrMagnitude>.01f;
            bool important=model.State!=CombatState.Neutral || moving || Transforming;
            if(battleSettleUntil>Time.unscaledTime && !important)
            {
                battleSettleUntil=0;ShowPresentationCue("O SELO SE CALOU","O caminho está livre.",false);
            }
            if(cueRoot==null)return;
            float elapsed=Time.unscaledTime-cueStarted;
            if((!bossCue&&important) || elapsed>.15f&&lab.Input.Menu("Submit").WasPressedThisFrame())cueDuration=0;
            cueGroup.alpha=DarklightTransitionTiming.Cue(elapsed,cueDuration);
            cueRoot.gameObject.SetActive(cueGroup.alpha>0);
        }
    }
}
