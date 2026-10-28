using UnityEngine;
using UnityEngine.UI;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        RectTransform gameOverArtwork;
        Image gameOverMist;
        float gameOverOpenedAt;
        void EnsureGameOverArtwork()
        {
            if(gameOverArtwork!=null)return;
            gameOverArtwork=RectAt(overlay,"GameOverVespera",0,0,640,360);
            gameOverArtwork.SetSiblingIndex(menuRoot.GetSiblingIndex());
            Image Layer(string name,float x,float y,float width,float height,Sprite sprite,Color tint)
            {
                var image=ImageAt(gameOverArtwork,name,x,y,width,height,sprite);
                image.material=null;image.color=tint;return image;
            }
            Layer("VesperaAprovada",0,0,640,360,Resources.Load<Sprite>("Darklight/Menu/Vespera"),new Color(.78f,.7f,.8f));
            Layer("Penumbra",0,0,640,360,null,new Color(.015f,.008f,.025f,.38f));
            var logo=Layer("MarcaOriginal",54,30,182,62,Resources.Load<Sprite>("Darklight/Branding/GameLogo"),new Color(.88f,.8f,.8f));
            logo.preserveAspect=true;
            Layer("PainelEscuro",308,20,316,324,null,new Color(.025f,.016f,.035f,.94f));
            var frame=Layer("MolduraOriginal",308,20,316,324,UIArt(0,extras[6]),new Color(.6f,.39f,.46f,.42f));
            frame.type=Image.Type.Sliced;frame.pixelsPerUnitMultiplier=8.333333f;
            Layer("LinhaCarmesim",308,42,1,280,null,new Color(.52f,.055f,.12f,.9f));
            var shrine=Layer("SeloOriginal",64,113,162,174,Resources.Load<Sprite>("Darklight/Props/CheckpointShrine"),Color.white);
            shrine.preserveAspect=true;
            gameOverMist=Layer("NevoaOriginal",-20,234,680,112,Resources.Load<Sprite>("Darklight/Menu/Mist"),new Color(.67f,.59f,.74f,.16f));
            var seal=TextAt(gameOverArtwork,"SeloDeRetorno",42,293,206,14,6);seal.text="O SELO AINDA TE AGUARDA";seal.alignment=TextAnchor.MiddleCenter;
            var note=TextAt(gameOverArtwork,"PromessaDoSelo",44,313,202,22,5);note.text="Retome seu caminho na forma normal.";note.alignment=TextAnchor.UpperCenter;note.color=new Color(.66f,.6f,.67f);
            var title=TextAt(gameOverArtwork,"Identidade",336,46,264,12,5);title.text="D A R K L I G H T   /   R E Q U I E M";title.color=new Color(.66f,.53f,.59f);
            Layer("Divisor",340,117,252,1,null,new Color(.43f,.13f,.21f,.7f));
        }
        void TickGameOverArtwork()
        {
            if(gameOverArtwork==null)return;
            bool visible=panel=="Death"&&!Training;
            gameOverArtwork.gameObject.SetActive(visible);
            if(!visible)return;
            float elapsed=Time.unscaledTime-gameOverOpenedAt;
            menuFade.alpha=Mathf.Clamp01(elapsed/.3f);
            gameOverMist.rectTransform.anchoredPosition=new Vector2(-20+(ReducedFlashes?0:Mathf.Sin(elapsed*.12f)*8),-234);
        }
    }
}
