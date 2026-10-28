using UnityEngine;
using UnityEngine.UI;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public DarklightStageStatistics StageStats{get;}=new();
        RectTransform styleRoot;Text styleRank,styleHits,styleCaption,resultRank;Image styleProgress,styleSigil;
        int shownStyleRank=-1;float stylePulse;
        static readonly Color[] StyleColors={new(.63f,.67f,.72f),new(.6f,.77f,.84f),new(.45f,.78f,.9f),new(.85f,.75f,.52f),new(1,.71f,.31f),new(.82f,.48f,1),new(1,.28f,.43f)};
        void CreateStyleHUD()
        {
            styleRoot=RectAt(uiRoot,"EstiloDoCombate",512,122,120,72);DarklightHUDTheme.Frame(styleRoot);
            var title=TextAt(styleRoot,"Titulo",7,4,106,9,4);title.text="ESTILO DO COMBATE";title.color=new Color(.7f,.7f,.77f);
            styleSigil=ImageAt(styleRoot,"Selo",3,14,54,43,Resources.Load<Sprite>("Darklight/HUD/StyleSigil"));styleSigil.preserveAspect=true;styleSigil.raycastTarget=false;
            styleRank=TextAt(styleRoot,"Rank",5,19,51,29,18);styleRank.alignment=TextAnchor.MiddleCenter;
            styleHits=TextAt(styleRoot,"Acertos",61,20,53,14,7);styleCaption=TextAt(styleRoot,"Sequencia",61,35,53,18,4);
            ImageAt(styleRoot,"Trilho",8,59,104,3,null).color=new Color(.16f,.14f,.23f);
            styleProgress=ImageAt(styleRoot,"Progresso",8,59,0,3,null);
            var note=TextAt(styleRoot,"Variacao",8,64,104,7,3);note.text="VARIE GOLPES · EVITE DANO";
        }
        void TickStyleHUD()
        {
            if(styleRoot==null)return;styleRoot.gameObject.SetActive(Active);
            int rank=Style.RankIndex;if(rank>shownStyleRank&&shownStyleRank>=0)stylePulse=.18f;shownStyleRank=rank;
            stylePulse=Mathf.Max(0,stylePulse-Time.unscaledDeltaTime);styleRank.text=Style.Rank;styleRank.color=styleProgress.color=StyleColors[rank];
            styleRank.transform.localScale=Vector3.one*(1+(ReducedFlashes?0:stylePulse*.28f));
            styleHits.text=Style.ComboHits+" ACERTOS";styleCaption.text=Style.ComboHits>=3?"COMBO ATIVO":Style.ComboHits>0?"CONTINUE!":"MELHOR "+StageStats.Result?.bestCombo;
            styleProgress.rectTransform.sizeDelta=new Vector2(104*Style.RankProgress,3);
            if(styleSigil!=null)styleSigil.color=Color.Lerp(new Color(.5f,.46f,.56f),StyleColors[rank],.5f);
        }
        void BuildStageResults()
        {
            if(!Training)StageStats.Observe(Style);
            var result=Progress.StageResult(Progress.chapter);heading.text=result.completed?"FASE CONCLUÍDA":"ESTATÍSTICAS DA FASE";
            bodyText.text=DarklightStory.Places[Progress.chapter]+"\n\n"+
                "INIMIGOS DERROTADOS    "+result.kills+"\n"+
                "ESQUIVAS BEM-SUCEDIDAS    "+result.dodges+"\n"+
                "PARRIES    "+result.parries+"\n"+
                "DANO RECEBIDO    "+result.damageTaken.ToString("0.#")+"\n"+
                "COMBOS (3+ ACERTOS)    "+result.combos+"\n"+
                "MAIOR SEQUÊNCIA    "+result.bestCombo+"\n"+
                "ESPECIAIS CONFIRMADOS    "+result.specials+"\n"+
                "MELHOR ESTILO    "+DarklightStyleMeter.RankName(result.bestRank);
            bodyText.fontSize=7;bodyText.lineSpacing=1.4f;bodyText.rectTransform.anchoredPosition=new Vector2(320,-151);bodyText.rectTransform.sizeDelta=new Vector2(250,150);
            portrait.gameObject.SetActive(true);portrait.sprite=Resources.Load<Sprite>("Darklight/HUD/StyleSigil");portrait.color=StyleColors[result.bestRank];
            portrait.rectTransform.anchoredPosition=new Vector2(396,-59);portrait.rectTransform.sizeDelta=new Vector2(88,82);
            if(resultRank==null){resultRank=TextAt(portrait.transform,"MelhorRank",0,20,88,42,26);resultRank.alignment=TextAnchor.MiddleCenter;}
            resultRank.text=DarklightStyleMeter.RankName(result.bestRank);resultRank.color=StyleColors[result.bestRank];resultRank.gameObject.SetActive(true);
            Button("CONTINUAR",Close);Button("COMBOS E ESPECIAIS",()=>Open("Weapons"));Button("VOLTAR AO MENU DE PAUSA",()=>Open("Pause"));
        }
    }
}
