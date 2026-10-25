using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        int guideWeapon;
        void BuildWeaponComboGuide()
        {
            guideWeapon=Mathf.Clamp(guideWeapon,0,2);heading.text="COMBOS E ESPECIAIS";
            string light=lab.Input.BindingLabel("Light"),heavy=lab.Input.BindingLabel("Heavy");
            string identity=guideWeapon==0?"REQUIEM\nCortes precisos; a quarta entrada fecha um golpe duplo.":guideWeapon==1?"PRESAS DO VÉU\nEstocadas rápidas; feche a sequência com a execução dupla.":"PUNHOS DA RUÍNA\nSoco → chute → giro → martelo no chão.";
            string special=guideWeapon==0?"ECLIPSE PARTIDO — dois cortes amplos. Recarga 6,5 s.":guideWeapon==1?"DANÇA DAS PRESAS — avanço sombrio e cinco contatos. Recarga 8,5 s.":"ESPIRAL DA RUÍNA — giro de três impactos e finalização. Recarga 7 s.";
            bodyText.text=identity+"\n\n"+light+" ×4: COMBO\n"+light+", pausa, "+light+": VARIAÇÃO\n"+light+", "+light+", "+heavy+": FINAL PESADO\nSEGURAR "+heavy+": CARREGADO\n"+lab.Input.BindingLabel("Aerial")+": ATAQUE AÉREO\n"+lab.Input.BindingLabel("WeaponSpecial")+": "+special+"\n\nAlterne golpes e armas após acertar para elevar o estilo. Receber dano quebra a sequência.";
            bodyText.rectTransform.anchoredPosition=new Vector2(312,-59);bodyText.rectTransform.sizeDelta=new Vector2(260,240);bodyText.fontSize=7;bodyText.lineSpacing=1.45f;
            for(int i=0;i<3;i++){int value=i;Button(weaponDefinitions[i].displayName,()=>{guideWeapon=value;RefreshPanel();},true,WeaponIcon(weaponDefinitions[i]));}
            Button("CAVALEIRO DO ABISMO",ShowAbyssHelp);Button("VOLTAR",()=>Open("Weapons"));
        }
    }
}
