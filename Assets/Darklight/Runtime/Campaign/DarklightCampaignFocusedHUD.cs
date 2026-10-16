using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        readonly Dictionary<string,CanvasGroup> focusedGroups=new();
        readonly string[] focusedWeapon={"ArmaHUD","ArmaEquipada","Arma","ArmaDescricao","TrocaArma"};
        readonly string[] focusedControls={"TeclaAtaque","RotuloAtaque","TeclaParry","RotuloParry","TeclaEsquiva","RotuloEsquiva","MenuHUD"};
        readonly string[] focusedSkill={"HabilidadeMoldura","EmblemaOnda","HabilidadeNome","FundoOnda","HabilidadeBarra","HabilidadeTexto","TeclaOnda"};
        int focusedWeaponId=-1,focusedPower=-1,focusedPotions=-1,focusedLevel=-1,focusedXP=-1;
        float focusedWeaponUntil,focusedPowerUntil,focusedPotionUntil,focusedProgressUntil;
        void FocusedElement(string name,bool show)
        {
            if(!focusedGroups.TryGetValue(name,out var group))
            {
                var item=uiRoot.Find(name);if(item==null)return;
                group=item.GetComponent<CanvasGroup>();if(group==null)group=item.gameObject.AddComponent<CanvasGroup>();
                group.alpha=show?1:0;group.blocksRaycasts=false;group.interactable=false;focusedGroups[name]=group;
            }
            group.alpha=Mathf.MoveTowards(group.alpha,show?1:0,Time.unscaledDeltaTime*6);
        }
        void TickFocusedHUD()
        {
            if(uiRoot==null||lab.Player?.Model==null)return;
            float now=Time.unscaledTime;int weapon=Weapons?.Equipped==null?-1:(int)Weapons.Equipped.id;
            if(weapon!=focusedWeaponId){if(focusedWeaponId>=0)focusedWeaponUntil=now+3;focusedWeaponId=weapon;}
            if(Resonance.Selected!=focusedPower){if(focusedPower>=0)focusedPowerUntil=now+3;focusedPower=Resonance.Selected;}
            if(Progress.potions!=focusedPotions){if(focusedPotions>=0)focusedPotionUntil=now+3;focusedPotions=Progress.potions;}
            if(Progress.level!=focusedLevel||Progress.xp!=focusedXP)
            {if(focusedLevel>=0)focusedProgressUntil=now+3;focusedLevel=Progress.level;focusedXP=Progress.xp;}
            bool training=Active&&Training;
            foreach(var name in focusedWeapon)FocusedElement(name,Active&&(training||now<focusedWeaponUntil));
            foreach(var name in focusedControls)FocusedElement(name,training);
            bool skill=Active&&(training||lab.Player.Model.SkillRemaining>0);
            foreach(var name in focusedSkill)FocusedElement(name,skill);
            // These labels originate in the laboratory and retain their bindings/data.
            FocusedElement(lab.PixelHUD.skillFill.name,skill);FocusedElement(lab.PixelHUD.skillText.name,skill);
            bool potion=Active&&(training||health.HealthPercent<.45f||now<focusedPotionUntil);
            FocusedElement("TeclaElixir",potion);FocusedElement("ElixiresHUD",potion);
            if(Form>0)statusText.text=new[]{"","RUPTURA","ECLIPSE","ABSOLUTO"}[Form]+" "+FormTime.ToString("0")+"s";
            FocusedElement("Progresso",Active&&(training||Form>0||now<focusedProgressUntil));
            FocusedElement("MP3Compacto",Active&&(training||Resonance.Active>=0||now<focusedPowerUntil));
            // Health, blood, active gear timing, boss warnings and interaction prompts remain intact.
        }
    }
}
