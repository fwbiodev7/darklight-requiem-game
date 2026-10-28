using UnityEngine;
using UnityEngine.UI;
namespace Darklight
{
    public enum DarklightLesson { Strike, Dodge, Parry }
    public sealed class DarklightCombatCoach : MonoBehaviour
    {
        DarklightCombatLab lab;Text text;RectTransform card;int mask,hits;float completedTime,nextAbyssTip;
        public int Learned=>mask;
        public bool TipsEnabled {get;private set;}=true;
        public void Initialize(DarklightCombatLab value){lab=value;mask=DarklightAutomation.Silent?0:PlayerPrefs.GetInt("Darklight.Lessons",0);TipsEnabled=PlayerPrefs.GetInt("Darklight.Tips",1)!=0;}
        public void ToggleTips(){TipsEnabled=!TipsEnabled;if(!DarklightAutomation.Silent)PlayerPrefs.SetInt("Darklight.Tips",TipsEnabled?1:0);}
        public void Restart(){mask=hits=0;completedTime=0;TipsEnabled=true;if(!DarklightAutomation.Silent){PlayerPrefs.SetInt("Darklight.Lessons",0);PlayerPrefs.SetInt("Darklight.Tips",1);}}
        public void Record(DarklightLesson lesson)
        {
            if(lab==null||lab.Paused||!lab.Ready)return;
            if(lesson==DarklightLesson.Strike&&++hits<3)return;
            mask|=1<<(int)lesson;if(!DarklightAutomation.Silent)PlayerPrefs.SetInt("Darklight.Lessons",mask);
        }
        void LateUpdate()
        {
            if(lab?.PixelHUD==null)return;
            if(card==null)
            {
                var parent=lab.PixelHUD.transform.Find("HUD640x360");if(parent==null)return;
                card=new GameObject("DicaDeCombate",typeof(RectTransform)).GetComponent<RectTransform>();card.SetParent(parent,false);card.anchorMin=card.anchorMax=card.pivot=new Vector2(0,1);card.anchoredPosition=new Vector2(458,-287);card.sizeDelta=new Vector2(174,44);
                var bg=card.gameObject.AddComponent<Image>();bg.color=new Color(.035f,.025f,.04f,.9f);bg.raycastTarget=false;DarklightHUDTheme.Frame(card);
                var edge=new GameObject("Borda",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();edge.SetParent(card,false);edge.anchorMin=new Vector2(0,0);edge.anchorMax=new Vector2(0,1);edge.pivot=Vector2.zero;edge.sizeDelta=new Vector2(1,0);edge.GetComponent<Image>().color=new Color(.55f,.12f,.2f);edge.GetComponent<Image>().raycastTarget=false;
                var r=new GameObject("Texto",typeof(RectTransform),typeof(Text)).GetComponent<RectTransform>();r.SetParent(card,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(6,3);r.offsetMax=new Vector2(-5,-3);text=r.GetComponent<Text>();text.font=lab.PixelHUD.pixelFont;text.fontSize=4;text.lineSpacing=1.6f;text.color=new Color(.95f,.9f,.82f);text.raycastTarget=false;
            }
            bool active=lab.Ready&&!lab.Paused&&TipsEnabled&&(DarklightCampaign.Instance==null||DarklightCampaign.Instance.Active&&DarklightCampaign.Instance.Training);
            var rider=DarklightCampaign.Instance?.AbyssRider;
            if(rider?.Active==true)
            {
                card.gameObject.SetActive(active);if(active&&Time.unscaledTime>=nextAbyssTip){text.text=rider.LessonText(lab.Input);nextAbyssTip=Time.unscaledTime+.15f;}return;
            }
            if(mask==7&&completedTime<5&&active)completedTime+=lab.CombatDelta;
            card.gameObject.SetActive(active&&(mask!=7||completedTime<5));if(!active)return;
            var input=lab.Input;
            string weapon=lab.Player.GetComponent<DarklightWeaponController>()?.Equipped?.id switch{DarklightWeaponId.Daggers=>"PRESAS: curto alcance; golpes rápidos.",DarklightWeaponId.Chain=>(DarklightRuinFistsBank.Ready?"PUNHOS DA RUÍNA: socos, chutes e finalizações.":"CORRENTE: alcance; pesado puxa o alvo."),_=>"KATANA: precisão; parry abre contra-ataque."};
            text.text=(mask&1)==0?input.BindingLabel("Light")+": ACERTE 3 GOLPES  "+Mathf.Min(hits,3)+"/3\n"+weapon:(mask&2)==0?input.BindingLabel("Dodge")+": ESQUIVE DE UM GOLPE\nAnel duplo vermelho: saia da área." :(mask&4)==0?input.BindingLabel("Parry")+": PARRY NO IMPACTO\nArco dourado: encare quem ataca.":"TREINO CONCLUÍDO\nParry + "+input.BindingLabel("Light")+": contra-ataque.";
        }
    }
}
