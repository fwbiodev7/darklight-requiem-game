using UnityEngine;

namespace Darklight
{
    public static class DarklightHUDTheme
    {
        static Sprite frame;
        public static readonly Color Charcoal=new(.018f,.023f,.029f,.98f);
        public static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public static void Frame(RectTransform rect,Color? tint=null)
        {
            var bg=rect.GetComponent<UnityEngine.UI.Image>()??rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            bg.sprite=null;bg.material=null;bg.type=UnityEngine.UI.Image.Type.Simple;bg.color=Charcoal;bg.raycastTarget=false;
            var existing=rect.Find("ContornoHUD");
            var edge=existing!=null?existing.GetComponent<UnityEngine.UI.Image>():Rect(rect,"ContornoHUD",0,0,rect.rect.width,rect.rect.height).gameObject.AddComponent<UnityEngine.UI.Image>();
            var rt=edge.rectTransform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.pivot=Vector2.one*.5f;rt.offsetMin=rt.offsetMax=Vector2.zero;
            frame??=Resources.Load<Sprite>("Darklight/HUD/Frame");edge.sprite=frame;edge.type=UnityEngine.UI.Image.Type.Sliced;edge.fillCenter=false;edge.pixelsPerUnitMultiplier=4;
            edge.color=tint??new Color(.84f,.79f,.76f);edge.raycastTarget=false;edge.transform.SetAsFirstSibling();
        }
        public static UnityEngine.UI.Text Key(Transform parent,Font font,string name,float x,float y,float w,float h)
        {
            var r=Rect(parent,name,x,y,w,h);Frame(r);
            var t=Rect(r,"Tecla",2,1,w-4,h-2).gameObject.AddComponent<UnityEngine.UI.Text>();t.font=font;t.fontSize=4;t.color=new Color(.95f,.93f,.88f);
            t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
    }
    // A native UI mesh keeps the portrait mask sharp at the HUD's integer pixel scale.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DarklightHUDDiamond : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;var c=r.center;var v=UnityEngine.UIVertex.simpleVert;v.color=color;
            foreach(var p in new[]{new Vector2(c.x,r.yMax),new Vector2(r.xMax,c.y),new Vector2(c.x,r.yMin),new Vector2(r.xMin,c.y)}){v.position=p;mesh.AddVert(v);}
            mesh.AddTriangle(0,1,2);mesh.AddTriangle(0,2,3);
        }
    }
}



