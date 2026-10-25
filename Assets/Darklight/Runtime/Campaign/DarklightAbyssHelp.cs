using UnityEngine;
using UnityEngine.UI;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        Image abyssHelpImage;readonly Image[] abyssGears=new Image[3];float nextAbyssHUD;int lastAbyssMistakes;float abyssMistakeUntil;
        string AbyssKey(string action)=>lab.Input.BindingLabel(action);
        public bool MarkAbyssEquipped()
        {
            if(Weapons?.Equipped?.id!=DarklightWeaponId.AbyssRider||Progress.abyssTutorialSeen)return false;
            Progress.abyssTutorialSeen=true;return !DarklightAutomation.Silent;
        }
        public void ShowAbyssHelp()=>Open("AbyssHelp");
        void BuildWeaponGuides()
        {
            heading.text="ARMAS";bodyText.text="Escolha uma arma ou consulte o guia.\n\nCavaleiro do Abismo: exclusivo da skin Rock.\n\nNa campanha, desperte o motor selado junto à memória da Cripta dos Primeiros.\n\nA arena permite testar a arma e suas formas sem alterar a campanha.";
            for(int i=0;i<3;i++){int index=i;Button(weaponDefinitions[i].displayName,()=>{Weapons.Request(index);Close();},Weapons.Unlocked(i),WeaponIcon(weaponDefinitions[i]));}
            Button("COMBOS E ESPECIAIS",()=>{guideWeapon=Mathf.Clamp((int)(Weapons?.Equipped?.id??DarklightWeaponId.Katana),0,2);Open("WeaponCombos");});Button("CAVALEIRO DO ABISMO · GUIA",ShowAbyssHelp);Button("VOLTAR",()=>Open("Pause"));
        }
        void BuildAbyssHelp()
        {
            heading.text="CAVALEIRO DO ABISMO";
            bodyText.text=AbyssKey("AbyssSaw")+"  SERRAS: toque para combinar golpes.\nSegure após acertar: serrar (6 + 2 por contato).\n"+
                AbyssKey("AbyssBike")+"  MOTO: montar e investir (18 sangue).\n"+AbyssKey("AbyssDrift")+"  NA MOTO: derrapar (+10 sangue).\n"+
                AbyssKey("Skill")+"  ESMAGAR com a moto (22 sangue).\n"+AbyssKey("Dodge")+"  ESQUIVA    "+AbyssKey("Parry")+"  PARRY\n\n"+
                "I → II → III: acerte e toque no brilho ENGATE!\nErros reduzem a rotação; segurar não sobe nível.\nIII + alvo exposto: "+AbyssKey("AbyssBike")+" FINAL (35 sangue).\n"+
                "Montado: "+AbyssKey("AbyssDrift")+" derrapa; "+AbyssKey("Skill")+" esmaga.\nApós o salto, "+AbyssKey("AbyssSaw")+" continua o combo.\n"+AbyssKey("Potion")+" a pé: elixir. Todos os botões são remapeáveis.";
            Button("EQUIPAR CAVALEIRO",()=>{Weapons.Request(3);Close();},Weapons.Unlocked(3));
            Button("NOVOS GOLPES",()=>Open("MotorcycleExpansion"));Button("COMBATE AÉREO",()=>Open("AirCombat"));
            Button("TREINAR PASSO A PASSO",StartAbyssLessons);Button("REMAPEAR COMANDOS",()=>Open("Controls"));Button("VOLTAR",()=>Open("Weapons"));
            if(abyssHelpImage==null){abyssHelpImage=ImageAt(menuRoot,"GuiaVisualCavaleiro",65,195,150,130,DarklightAbyssSpriteBank.Icon);abyssHelpImage.preserveAspect=true;abyssHelpImage.raycastTarget=false;}
            abyssHelpImage.gameObject.SetActive(true);
        }
        void StartAbyssLessons()
        {
            if(!Training)EnterTraining();SelectSkin(1,false);SetTrainingForm(0);Progress.abyssTutorialSeen=true;AbyssRider.ResetLessons();Weapons.Request(3);Close();Notice("TREINO CAVALEIRO: SIGA AS ETAPAS NO CANTO DA TELA");
        }
        void PresentAbyssHUD()
        {
            bool visible=AbyssRider?.Active==true&&Active;
            if(abyssGauge==null&&visible){abyssGauge=TextAt(uiRoot,"Engrenagens",8,65,165,11,5);abyssGauge.alignment=TextAnchor.UpperLeft;for(int i=0;i<3;i++)abyssGears[i]=ImageAt(uiRoot,"NivelEngrenagem"+i,8+i*28,79,24,2,null);}
            if(abyssGauge==null)return;abyssGauge.gameObject.SetActive(visible);foreach(var bar in abyssGears)if(bar!=null)bar.gameObject.SetActive(visible);if(!visible)return;
            if(Time.unscaledTime<nextAbyssHUD)return;nextAbyssHUD=Time.unscaledTime+.08f;
            var gear=AbyssRider.Gear;if(gear.Mistakes>lastAbyssMistakes)abyssMistakeUntil=Time.unscaledTime+1;lastAbyssMistakes=gear.Mistakes;
            string level=gear.Level==3?"III":gear.Level==2?"II":"I";

            abyssGauge.color=gear.WindowOpen?new Color(1,.83f,.3f):gear.Level==3?new Color(1,.78f,.25f):Time.unscaledTime<abyssMistakeUntil?new Color(1,.35f,.32f):new Color(.9f,.84f,.78f);
            abyssGauge.text="ENGR. "+level+" / III"+(gear.WindowOpen?"  ENGATE!":AbyssRider.CanFinish?"  "+AbyssKey("AbyssBike")+" FINAL":Time.unscaledTime<abyssMistakeUntil?"  FORA DO RITMO":"");
            for(int i=0;i<3;i++)abyssGears[i].color=i<gear.Level?(gear.Level==3?new Color(1,.7f,.14f):new Color(.8f,.15f,.23f)):new Color(.17f,.14f,.16f);
        }
    }
}
