using UnityEngine;
using UnityEngine.UI;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        RectTransform gothicOuter,gothicList,gothicDetails,gothicHeader,gothicFooter;
        DarklightHUDDiamond gothicGem;
        Image gothicWeaponArt;Text gothicWeaponTitle;int gothicWeaponSelection=-1;
        RectTransform GothicCard(string name,float x,float y,float w,float h,bool clear=false)
        {
            var rect=RectAt(menuRoot,name,x,y,w,h);DarklightHUDTheme.Frame(rect,new Color(.73f,.47f,.89f));
            rect.GetComponent<Image>().color=clear?Color.clear:new Color(.018f,.009f,.028f,.94f);return rect;
        }
        void CreateGothicMenuChrome()
        {
            // Approved sliced art and live controls, rather than a flattened menu screenshot.
            gothicOuter=GothicCard("MolduraGotica",8,8,624,344,true);
            gothicHeader=GothicCard("CabecalhoGotico",22,20,596,56);
            gothicList=GothicCard("ListaGotica",20,90,286,240);
            gothicDetails=GothicCard("DetalhesGoticos",318,90,302,240);
            gothicFooter=GothicCard("RodapeGotico",20,333,600,18);
            foreach(var rect in new[]{gothicOuter,gothicHeader,gothicList,gothicDetails,gothicFooter})rect.SetSiblingIndex(0);
            gothicGem=RectAt(menuRoot,"GemaDoMenu",316,14,8,12).gameObject.AddComponent<DarklightHUDDiamond>();
            gothicGem.color=new Color(.78f,.19f,.96f);gothicGem.raycastTarget=false;
            gothicWeaponTitle=TextAt(menuRoot,"NomeDoEquipamento",332,104,272,18,8);gothicWeaponTitle.alignment=TextAnchor.MiddleCenter;
            gothicWeaponArt=ImageAt(menuRoot,"EquipamentoDoArsenal",388,126,160,86,null);gothicWeaponArt.preserveAspect=true;
        }
        static void PlaceGothic(RectTransform r,float x,float y,float w,float h)
        {r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        void LayoutGothicMenuChrome()
        {
            bool visible=panel!="Death"&&panel!="Dialogue"&&panel!=""&&panel!="Loading",main=panel=="Main",pause=panel=="Pause";
            gothicOuter.gameObject.SetActive(visible);gothicFooter.gameObject.SetActive(visible);
            gothicHeader.gameObject.SetActive(visible&&!main&&!pause);gothicList.gameObject.SetActive(visible&&!main&&!pause);
            gothicDetails.gameObject.SetActive(visible&&!main&&!pause);gothicGem.gameObject.SetActive(visible);
            gothicWeaponArt.gameObject.SetActive(visible&&panel=="Weapons");gothicWeaponTitle.gameObject.SetActive(visible&&panel=="Weapons");if(!visible)return;
            if(panel!="Weapons")gothicWeaponSelection=-1;
            menuRoot.Find("Moldura").gameObject.SetActive(false);menuRoot.anchoredPosition=Vector2.zero;menuRoot.sizeDelta=new Vector2(640,360);
            PlaceGothic(gothicOuter,pause?174:8,pause?34:8,pause?292:624,pause?286:344);
            gothicOuter.GetComponent<Image>().color=pause?new Color(.018f,.009f,.028f,.96f):Color.clear;
            bool options=panel=="Options";PlaceGothic(gothicList,20,90,options?190:286,240);PlaceGothic(gothicDetails,options?224:318,90,options?396:302,240);
            PlaceGothic(gothicGem.rectTransform,316,pause?28:14,8,12);
            heading.alignment=TextAnchor.MiddleCenter;heading.horizontalOverflow=HorizontalWrapMode.Wrap;heading.verticalOverflow=VerticalWrapMode.Truncate;heading.fontSize=pause?10:11;
            PlaceGothic(heading.rectTransform,pause?190:40,pause?94:35,pause?260:560,28);
            menuFooter.horizontalOverflow=HorizontalWrapMode.Wrap;menuFooter.verticalOverflow=VerticalWrapMode.Truncate;PlaceGothic(menuFooter.rectTransform,32,338,504,10);menuFooter.fontSize=4;
            gameLogo.gameObject.SetActive((main||pause)&&gameLogo.sprite!=null);PlaceGothic(gameLogo.rectTransform,pause?240:196,pause?42:22,pause?160:248,pause?48:82);
            menuTagline.alignment=TextAnchor.MiddleCenter;PlaceGothic(menuTagline.rectTransform,190,110,260,12);
            if(main&&menuSave!=null)PlaceGothic(menuSave.rectTransform,22,21,212,10);
            else if(!pause&&panel!="Skins")
            {
                PlaceGothic(bodyText.rectTransform,options?244:332,110,options?350:272,204);bodyText.fontSize=6;bodyText.lineSpacing=1.3f;
                bodyText.horizontalOverflow=HorizontalWrapMode.Wrap;bodyText.verticalOverflow=VerticalWrapMode.Truncate;
            }
            if(pause){PlaceGothic(menuRoot.Find("ContextoDaFase") as RectTransform,190,297,260,18);PlaceGothic(pauseLocation.rectTransform,198,299,244,14);pauseLocation.fontSize=4;pauseLocation.lineSpacing=1;}
            if(panel=="Skins")
            {
                PlaceGothic(skinPreview.rectTransform,332,108,112,182);PlaceGothic(skinDescription.rectTransform,456,116,148,154);
                skinDescription.fontSize=5;skinDescription.lineSpacing=1.3f;skinDescription.verticalOverflow=VerticalWrapMode.Truncate;
                PlaceGothic(skinEquipped.rectTransform,332,296,272,16);skinEquipped.fontSize=5;
            }
            if(panel=="Weapons"){PlaceGothic(bodyText.rectTransform,336,220,264,94);bodyText.fontSize=5;UpdateGothicArsenal();}
            if(font.material.mainTexture!=null)font.material.mainTexture.filterMode=FilterMode.Point;
        }
        void UpdateGothicArsenal()
        {
            if(panel!="Weapons"||gothicWeaponSelection==selection)return;
            gothicWeaponSelection=selection;int index=selection==1?2:0;
            gothicWeaponTitle.text=index==0?"KATANA":"PUNHOS / GREVAS";gothicWeaponArt.sprite=WeaponIcon(weaponDefinitions[index]);
            bodyText.text=(index==0?"Precisao, alcance e cortes encadeados.":"Pressao corpo a corpo com socos e chutes.")+"\n\nNIVEL DE FABIUS: "+Progress.level+"\nATAQUE: "+Progress.attack+"  MAESTRIA: "+Progress.mastery+"\nDANO DA CAMPANHA: "+Progress.Damage.ToString("0.00")+"x\n\n"+lab.Input.BindingLabel("Light")+": LEVE   "+lab.Input.BindingLabel("Heavy")+": PESADO\nConsulte COMBOS E ESPECIAIS para os comandos.";
        }
        void BuildCharacterMenu()
        {
            heading.text="PERSONAGEM E REGISTRO";bodyText.text="Acompanhe Fabius, suas memorias e os recursos da campanha.\n\nSuas escolhas e os dados existentes sao preservados.";
            Button("ESTATISTICAS DA FASE",()=>Open("StageResults"));Button("TRANSFORMACOES",()=>Open("Transformations"));Button("HABILIDADES",()=>Open("Skills"));
            Button("INVENTARIO",()=>Open("Inventory"));Button("DIARIO",()=>Open("Journal"));Button("DARK RESONANCE",()=>Open("MP3"));Button("VISUAL",()=>Open("Skins"));Button("VOLTAR",()=>Open("Pause"));
        }
    }
}
