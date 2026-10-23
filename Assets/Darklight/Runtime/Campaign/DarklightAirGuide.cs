namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        void BuildAirGuide()
        {
            heading.text="COMBATE AÉREO";
            string air=lab.Input.BindingLabel("Aerial"),light=lab.Input.BindingLabel("Light"),heavy=lab.Input.BindingLabel("Heavy");
            bodyText.text=air+": lançar; nos Punhos, use o pesado (uppercut).\n"+
                air+" novamente após acertar: perseguir.\n"+light+": sequência com a arma atual.\n"+heavy+": golpe forte; após 3 contatos, arremessar.\n"+
                air+" após arremesso: alcançar por cima.\n"+heavy+" por cima: golpe descendente.\n\n"+
                "Direcione cada golpe e troque de arma na recuperação.\nSem novos comandos, o combo termina e você aterrissa.\nEsquiva, dano e morte interrompem a perseguição.\nChefes e blindados resistem ao lançamento.\n\n"+
                "Moto: a roda mantém a suspensão; o golpe por cima executa a finalização motorizada. Continue o combo após cair.";
            GuideLayout();Button("COMBOS DAS ARMAS",()=>Open("WeaponCombos"));Button("CAVALEIRO DO ABISMO",ShowAbyssHelp);Button("VOLTAR",()=>Open("CombatOptions"));
        }
        void GuideLayout()
        {bodyText.rectTransform.anchoredPosition=new UnityEngine.Vector2(312,-59);bodyText.rectTransform.sizeDelta=new UnityEngine.Vector2(260,240);bodyText.fontSize=7;bodyText.lineSpacing=1.35f;}
        void BuildMotorcycleExpansionGuide()
        {
            heading.text="ARSENAL MOTORIZADO";
            string light=lab.Input.BindingLabel("AbyssSaw"),heavy=lab.Input.BindingLabel("Heavy");
            bodyText.text="Os três golpes iniciais e as variações anteriores foram preservados.\n\n"+
                light+" ×6: Serra Infernal, 4 contatos e lançamento.\n"+light+" ×7: Giro Devastador, varredura ao redor.\n"+
                "2 golpes + "+heavy+": Roda Destruidora, avanço e lançamento.\n"+lab.Input.BindingLabel("Skill")+": Esmagamento Vertical (22 sangue).\n"+
                "Mover + "+lab.Input.BindingLabel("AbyssBike")+": Cavalgada Brutal (18 sangue).\n"+
                "Montado: pesado derrapa; habilidade esmaga; recuperação desmonta.\n"+
                "Aéreo: lance, persiga, acerte 3 contatos, pesado arremessa, persiga novamente e pesado executa a finalização motorizada.";
            GuideLayout();Button("GUIA AÉREO",()=>Open("AirCombat"));Button("VOLTAR",ShowAbyssHelp);
        }
    }
}
