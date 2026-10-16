using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        UnityEngine.UI.Image hudPortrait;
        UnityEngine.UI.Text hudSkillName,hudAttackName,hudSkillKey,hudPauseKey,hudAttackKey,hudDodgeKey,hudParryKey,hudCycleKey,hudPotionKey,hudPotionCount,hudWeaponHint;
        readonly Sprite[] hudPortraits=new Sprite[2];
        int hudPortraitSkin=-1;
        void HUDPlace(Transform item,float x,float y,float w,float h,int size=0)
        {
            if(item is not RectTransform r)return;r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
            if(size>0&&r.TryGetComponent<UnityEngine.UI.Text>(out var text))text.fontSize=size;
        }
        void CreateMinimalHUD()
        {
            // The existing health/skill objects remain connected to DarklightPixelHUD.
            var life=uiRoot.Find("VidaMoldura") as RectTransform;HUDPlace(life,46,13,125,46);DarklightHUDTheme.Frame(life);
            HUDPlace(uiRoot.Find("Nome"),58,17,102,11,7);uiRoot.Find("Nome").GetComponent<UnityEngine.UI.Text>().text="FÁBIO";
            var healthBack=ImageAt(uiRoot,"FundoVida",56,31,108,10,null);healthBack.color=new Color(.12f,.018f,.033f);healthBack.transform.SetSiblingIndex(life.GetSiblingIndex()+1);
            HUDPlace(lab.PixelHUD.healthFill.transform,57,32,106,8);HUDPlace(lab.PixelHUD.healthText.transform,57,32,106,9,6);lab.PixelHUD.healthText.alignment=TextAnchor.MiddleCenter;
            var energyBack=ImageAt(uiRoot,"FundoEnergia",56,44,108,3,null);energyBack.color=new Color(.015f,.09f,.14f);energyBack.transform.SetSiblingIndex(life.GetSiblingIndex()+1);
            HUDPlace(energyFill.transform,57,45,106,1);energyFill.color=new Color(.1f,.71f,.87f);
            HUDPlace(statusText.transform,57,51,110,9,5);energyText.gameObject.SetActive(false);
            var border=RectAt(uiRoot,"RetratoHUD",7,7,54,54).gameObject.AddComponent<DarklightHUDDiamond>();border.color=new Color(.39f,.36f,.35f);border.raycastTarget=false;
            var trim=RectAt(border.transform,"Sangue",1,1,52,52).gameObject.AddComponent<DarklightHUDDiamond>();trim.color=new Color(.25f,.035f,.055f);trim.raycastTarget=false;
            var mask=RectAt(trim.transform,"Recorte",1,1,50,50).gameObject.AddComponent<DarklightHUDDiamond>();mask.color=new Color(.018f,.022f,.03f);mask.raycastTarget=false;mask.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=true;
            hudPortrait=ImageAt(mask.transform,"Fábio",1,3,48,47,null);hudPortrait.preserveAspect=true;hudPortrait.material=null;
            for(int i=0;i<2;i++)hudPortraits[i]=Resources.Load<Sprite>("Darklight/HUD/Portrait"+i);
            HUDPlace(uiRoot.Find("HabilidadeMoldura"),8,94,124,34);DarklightHUDTheme.Frame(uiRoot.Find("HabilidadeMoldura") as RectTransform);
            ImageAt(uiRoot,"EmblemaOnda",5,92,37,38,Resources.Load<Sprite>("Darklight/HUD/Wave")).preserveAspect=true;
            HUDPlace(uiRoot.Find("HabilidadeNome"),44,97,83,9,5);hudSkillName=uiRoot.Find("HabilidadeNome").GetComponent<UnityEngine.UI.Text>();hudSkillName.text="ONDA DO VÉU";
            var waveBack=ImageAt(uiRoot,"FundoOnda",44,109,62,10,null);waveBack.color=new Color(.14f,.02f,.035f);waveBack.transform.SetSiblingIndex(uiRoot.Find("HabilidadeMoldura").GetSiblingIndex()+1);
            HUDPlace(lab.PixelHUD.skillFill.transform,45,110,60,8);HUDPlace(lab.PixelHUD.skillText.transform,45,110,60,9,5);lab.PixelHUD.skillText.alignment=TextAnchor.MiddleCenter;
            hudSkillKey=DarklightHUDTheme.Key(uiRoot,font,"TeclaOnda",111,107,18,18);
            var weaponCard=RectAt(uiRoot,"ArmaHUD",8,310,114,42);DarklightHUDTheme.Frame(weaponCard);
            HUDPlace(weaponIcon.transform,13,315,22,23);HUDPlace(weaponText.transform,40,316,78,10,5);
            weaponIcon.transform.SetAsLastSibling();weaponText.transform.SetAsLastSibling();
            hudWeaponHint=TextAt(uiRoot,"ArmaDescricao",40,327,76,9,5);hudWeaponHint.horizontalOverflow=UnityEngine.HorizontalWrapMode.Wrap;hudWeaponHint.verticalOverflow=UnityEngine.VerticalWrapMode.Truncate;
            hudCycleKey=TextAt(uiRoot,"TrocaArma",13,342,104,7,3);
            hudAttackKey=DarklightHUDTheme.Key(uiRoot,font,"TeclaAtaque",129,309,26,13);hudAttackName=TextAt(uiRoot,"RotuloAtaque",160,311,57,10,5);hudAttackName.text="Corte";
            hudParryKey=DarklightHUDTheme.Key(uiRoot,font,"TeclaParry",129,324,26,13);TextAt(uiRoot,"RotuloParry",160,326,57,10,5).text="Parry";
            hudDodgeKey=DarklightHUDTheme.Key(uiRoot,font,"TeclaEsquiva",129,339,26,13);TextAt(uiRoot,"RotuloEsquiva",160,341,57,10,5).text="Esquiva";
            hudPotionKey=DarklightHUDTheme.Key(uiRoot,font,"TeclaElixir",226,323,21,16);
            hudPotionCount=TextAt(uiRoot,"ElixiresHUD",252,327,56,10,5);
            var pause=RectAt(uiRoot,"MenuHUD",566,337,66,15);DarklightHUDTheme.Frame(pause);hudPauseKey=DarklightHUDTheme.Key(pause,font,"Tecla",2,1,29,13);TextAt(pause,"Texto",35,4,27,9,4).text="Menu";
            // Current destination is displayed in the atlas card, with its real tracking data.
            objectiveText.gameObject.SetActive(false);chapterText.gameObject.SetActive(false);
            HUDPlace(messageText.transform,203,282,232,21,5);messageText.horizontalOverflow=HorizontalWrapMode.Wrap;
            HUDPlace(powerFrame.transform,456,236,176,44);HUDPlace(powerText.transform,35,6,135,30,5);powerText.lineSpacing=1.7f;
            HUDPlace(powerFill.transform,35,37,134,2);DarklightHUDTheme.Frame(powerFrame.rectTransform);
            lab.PixelHUD.help.gameObject.SetActive(false);
        }
        void TickMinimalHUD()
        {
            if(hudPortrait==null)return;
            int skin=Mathf.Clamp(SelectedSkin,0,1);if(hudPortraitSkin!=skin){hudPortrait.sprite=hudPortraits[skin];hudPortraitSkin=skin;}
            energyFill.rectTransform.sizeDelta=new Vector2(106*Mathf.Clamp01(Energy/100),1);
            statusText.resizeTextForBestFit=true;statusText.resizeTextMinSize=4;statusText.resizeTextMaxSize=5;
            statusText.text="NV "+Progress.level+" | XP "+Progress.xp+"/"+Progress.NeededXP+" | R "+Progress.potions;
            bool abyss=AbyssRider?.Active==true;hudSkillName.text=abyss?"ESMAGAMENTO":"ONDA DO VÉU";hudAttackName.text=abyss?"Serras":"Corte";
            hudSkillKey.text=lab.Input.BindingLabel("Skill");hudAttackKey.text=lab.Input.BindingLabel(AbyssRider?.Active==true?"AbyssSaw":"Light");hudParryKey.text=lab.Input.BindingLabel("Parry");
            var dodge=lab.Input.BindingLabel("Dodge");hudDodgeKey.text=dodge=="SPACE"||dodge=="ESPAÇO"?"ESP":dodge;
            hudPauseKey.text=lab.Input.BindingLabel("Pause");hudPotionKey.text=lab.Input.BindingLabel("Potion");hudPotionCount.text="ELIXIR "+Progress.potions;
            hudCycleKey.text=lab.Input.BindingLabel("WeaponNext")+": TROCAR ARMA";
            if(Weapons?.Equipped!=null){weaponText.text=Weapons.Equipped.id==DarklightWeaponId.AbyssRider?"CAVALEIRO":Weapons.Equipped.displayName.ToUpperInvariant();hudWeaponHint.text=Weapons.Equipped.id switch{DarklightWeaponId.Daggers=>"Golpes rápidos",DarklightWeaponId.Chain=>"Longo alcance",DarklightWeaponId.AbyssRider=>"Serra e moto",_=>"Corte preciso"};}
            powerFill.rectTransform.sizeDelta=new Vector2(134*Resonance.Energy/100,2);
        }
        void RestoreMinimalHUDVisibility()
        {
            foreach(string name in new[]{"Ajuda","Energia","Objetivo","Capitulo","Katana","Leve","LeveBotao","Parry","Defesa","ParryBotao","Esquiva","EsquivaNome","EsquivaBotao","HabilidadeBotao","Modo"}){var item=uiRoot.Find(name);if(item!=null)item.gameObject.SetActive(false);}
        }
    }
}


