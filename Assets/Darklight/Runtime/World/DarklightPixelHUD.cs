using UnityEngine;

namespace Darklight
{
    /// <summary>Presentation of existing laboratory data; introduces no new combat systems.</summary>
    public sealed class DarklightPixelHUD : MonoBehaviour
    {
        public DarklightCombatLab lab;
        public Canvas canvas;
        public Font pixelFont;
        public UnityEngine.UI.RawImage worldImage;
        public UnityEngine.UI.CanvasScaler scaler;
        public UnityEngine.UI.Image healthFill,skillFill;
        public UnityEngine.UI.Text healthText,skillText,lightKey,parryKey,dodgeKey,help,death;
        public RectTransform[] targetFrames;
        public UnityEngine.UI.Image[] targetFills;
        public UnityEngine.UI.Text[] targetTexts;

        void LateUpdate()
        {
            if (lab == null || !lab.Ready || lab.Player.Model == null) return;
            float scale=lab.Viewport.width/640f;
            if(pixelFont.material.mainTexture!=null) pixelFont.material.mainTexture.filterMode=FilterMode.Point;
            scaler.scaleFactor=scale;canvas.scaleFactor=scale;
            worldImage.texture=lab.WorldCamera.targetTexture;
            var health=lab.Player.GetComponent<HealthSystem>();
            healthFill.fillAmount=Mathf.Clamp01(health.CurrentHealth/health.MaxHealth);
            healthText.text=Mathf.CeilToInt(health.CurrentHealth)+" / "+Mathf.CeilToInt(health.MaxHealth);
            float remaining=lab.Player.Model.SkillRemaining;
            skillFill.fillAmount=1-Mathf.Clamp01(remaining/3f);
            skillFill.color=remaining>0 ? new Color(.35f,.22f,.29f) : new Color(.7f,.07f,.18f);
            skillText.text=remaining>0 ? remaining.ToString("0.0")+" s" : "PRONTA";
            bool pad=lab.Input.UsingGamepad;
            lightKey.text=pad?"X":"M1";parryKey.text=pad?"LB":"Q";dodgeKey.text=pad?"B":"ESPACO";
            help.text=DarklightCampaign.Instance != null ? (pad?"X: CORTE  LB: PARRY  B: ESQUIVA  START: MENU":"M1: CORTE  Q: PARRY  ESPACO: ESQUIVA  ESC: MENU") : pad?"LS: MOVER  A: REINICIAR  START: PAUSA":"WASD: MOVER  E: REINICIAR  ESC: PAUSA";
            death.gameObject.SetActive(health.IsDead && DarklightCampaign.Instance == null);
            death.text="O VEU TE ALCANCOU\n"+(pad?"A":"E")+": RECOMECAR";
            for(int i=0;i<targetFrames.Length;i++)
            {
                var target=lab.Targets[i];if(target==null) { targetFrames[i].gameObject.SetActive(false);continue; }
                Vector3 point=lab.WorldCamera.WorldToScreenPoint(target.transform.position+Vector3.up*1.9f);point.x*=640f/lab.WorldCamera.pixelWidth;point.y*=360f/lab.WorldCamera.pixelHeight;
                bool visible=point.z>0 && point.x>=32 && point.x<=608 && point.y>=38 && point.y<=320;
                targetFrames[i].gameObject.SetActive(visible);
                targetFrames[i].anchoredPosition=new Vector2(Mathf.Round(point.x)-32,-Mathf.Round(360-point.y));
                targetFills[i].fillAmount=Mathf.Clamp01(target.Health.CurrentHealth/150f);
                targetTexts[i].text=target.Health.IsDead?"VAZIO":Mathf.CeilToInt(target.Health.CurrentHealth)+"/150";
            }
        }
    }
}
