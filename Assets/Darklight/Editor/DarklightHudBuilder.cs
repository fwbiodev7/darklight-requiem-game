using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightHudBuilder
    {
        static Font font;
        public static void Build(DarklightCombatLab lab,Sprite[] art,Material material)
        {
            font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Darklight/Art/Fonts/PressStart2P-Regular.ttf");
            if(font==null) throw new System.InvalidOperationException("Audited pixel font is missing.");
            font.material.mainTexture.filterMode=FilterMode.Point;
            var go=new GameObject("DarklightPixelHUD",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;canvas.pixelPerfect=true;
            var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
            var root=new GameObject("HUD640x360",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(go.transform,false);
            root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.sizeDelta=new Vector2(640,360);
            var hud=go.AddComponent<DarklightPixelHUD>();hud.lab=lab;hud.canvas=canvas;hud.scaler=scaler;hud.pixelFont=font;lab.PixelHUD=hud;
            hud.worldImage=Box(root,"Mundo640x360",0,0,640,360).gameObject.AddComponent<UnityEngine.UI.RawImage>();hud.worldImage.raycastTarget=false;
            Picture(root,"VidaMoldura",art[0],material,12,12,224,75);
            Label(root,"Nome","FABIO",86,26,126,12);
            hud.healthFill=Fill(root,"Vida",84,45,132,16,new Color(.68f,.035f,.12f));
            hud.healthText=Label(root,"VidaValor","100 / 100",84,48,132,12,true);
            Picture(root,"HabilidadeMoldura",art[1],material,428,12,200,75);
            Label(root,"HabilidadeNome","ONDA DO VEU",498,28,114,12);
            hud.skillFill=Fill(root,"HabilidadeProntidao",498,46,104,15,new Color(.7f,.07f,.18f));
            hud.skillText=Label(root,"HabilidadeTempo","PRONTA",498,48,104,12,true);
            Picture(root,"Katana",art[2],material,12,292,28,28);Label(root,"Leve","LEVE",44,294,60,12);hud.lightKey=Label(root,"LeveBotao","M1",44,309,60,12);
            Picture(root,"Parry",art[3],material,112,292,28,28);Label(root,"Defesa","PARRY",144,294,62,12);hud.parryKey=Label(root,"ParryBotao","Q",144,309,62,12);
            Picture(root,"Esquiva",art[4],material,216,292,28,28);Label(root,"EsquivaNome","ESQUIVA",248,294,76,12);hud.dodgeKey=Label(root,"EsquivaBotao","ESPACO",248,309,76,12);
            Label(root,"HabilidadeBotao","F / RB: ONDA",440,309,176,12);
            hud.help=Label(root,"Ajuda","WASD: MOVER  E: REINICIAR  ESC: PAUSA",12,337,610,12);
            Label(root,"Modo","TREINO",564,290,64,12);
            hud.death=Label(root,"Morte","",150,145,340,46,true);hud.death.color=new Color(1,.5f,.55f);hud.death.gameObject.SetActive(false);
            hud.targetFrames=new RectTransform[lab.Targets.Length];hud.targetFills=new UnityEngine.UI.Image[lab.Targets.Length];hud.targetTexts=new UnityEngine.UI.Text[lab.Targets.Length];
            for(int i=0;i<lab.Targets.Length;i++)
            {
                var frame=Box(root,"VidaAlvo"+i,0,0,64,13);hud.targetFrames[i]=frame;
                Picture(frame,"Moldura",art[5],material,0,0,64,13);
                hud.targetFills[i]=Fill(frame,"Vida",8,5,48,4,new Color(.65f,.035f,.12f));
                hud.targetTexts[i]=Label(frame,"Valor","150/150",0,-12,64,11,true);
            }
            string path="Assets/Darklight/UI/";System.IO.Directory.CreateDirectory(path);
            PrefabUtility.SaveAsPrefabAsset(go,path+"DarklightPixelHUD.prefab");
        }
        static RectTransform Box(Transform parent,string name,float x,float y,float width,float height)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(width,height);return rect;
        }
        static UnityEngine.UI.Image Picture(Transform parent,string name,Sprite sprite,Material material,float x,float y,float width,float height)
        {
            var image=Box(parent,name,x,y,width,height).gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.material=material;image.raycastTarget=false;return image;
        }
        static UnityEngine.UI.Image Fill(Transform parent,string name,float x,float y,float width,float height,Color color)
        {
            var image=Box(parent,name,x,y,width,height).gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;
            image.type=UnityEngine.UI.Image.Type.Filled;image.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;image.fillOrigin=0;
            image.sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Darklight/Art/LabFloor.asset")[1] as Sprite;
            if(image.sprite==null) foreach(var asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Darklight/Art/LabFloor.asset")) if(asset is Sprite s) image.sprite=s;
            return image;
        }
        static UnityEngine.UI.Text Label(Transform parent,string name,string value,float x,float y,float width,float height,bool centered=false)
        {
            var text=Box(parent,name,x,y,width,height).gameObject.AddComponent<UnityEngine.UI.Text>();text.font=font;text.fontSize=8;text.text=value;text.color=new Color(.91f,.88f,.84f);
            text.alignment=centered?TextAnchor.MiddleCenter:TextAnchor.UpperLeft;text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Overflow;text.verticalOverflow=VerticalWrapMode.Overflow;
            var outline=text.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=new Color(.025f,.025f,.035f);outline.effectDistance=new Vector2(1,-1);return text;
        }
    }
}
