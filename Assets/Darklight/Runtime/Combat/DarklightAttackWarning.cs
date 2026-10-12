using UnityEngine;
namespace Darklight
{
    public enum DarklightThreat { Parryable, Area, Projectile }
    // Cached geometric floor marks. Their shape describes the actual damage footprint.
    public sealed class DarklightAttackWarning : MonoBehaviour
    {
        static readonly Sprite[] shapes=new Sprite[3];
        SpriteRenderer art;
        public DarklightThreat Threat {get;private set;}
        public float Radius {get;private set;}
        public bool Visible=>art!=null&&art.enabled;
        static Sprite Shape(DarklightThreat kind)
        {
            int index=(int)kind;if(shapes[index]!=null)return shapes[index];
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="Sinal_"+kind};
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=(x+.5f-size*.5f)/(size*.5f),dy=(y+.5f-size*.5f)/(size*.5f);float r=Mathf.Sqrt(dx*dx+dy*dy);
                float angle=Mathf.Abs(Mathf.Atan2(dx,dy))*Mathf.Rad2Deg;bool mark;
                if(kind==DarklightThreat.Projectile)mark=dy>=0&&dy<.96f&&(Mathf.Abs(dx)<.025f||dy>.78f&&Mathf.Abs(Mathf.Abs(dx)-(.96f-dy)*.7f)<.025f);
                else if(kind==DarklightThreat.Area)mark=r<.99f&&(r>.94f||r>.84f&&r<.88f||r>.86f&&((x+y)/4)%2==0||Mathf.Abs(dx-dy)<.025f&&r<.18f||Mathf.Abs(dx+dy)<.025f&&r<.18f);
                else mark=r<.99f&&angle<=70&&(r>.94f||angle>67||r<.17f&&Mathf.Abs(dx)<.035f);
                if(mark)pixels[y*size+x]=new Color32(255,255,255,255);
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);return shapes[index]=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),size);
        }
        public void Show(Vector2 origin,Vector2 direction,float radius,DarklightThreat kind,float progress)
        {
            if(art==null){art=gameObject.AddComponent<SpriteRenderer>();art.sortingOrder=79;art.gameObject.layer=transform.parent!=null?transform.parent.gameObject.layer:0;}
            Threat=kind;Radius=radius;art.sprite=Shape(kind);art.enabled=true;
            transform.position=origin;transform.localScale=Vector3.one*(radius*2);
            transform.rotation=direction.sqrMagnitude>.01f?Quaternion.FromToRotation(Vector3.up,direction):Quaternion.identity;
            Color color=kind==DarklightThreat.Area?new Color(.95f,.12f,.22f):kind==DarklightThreat.Projectile?new Color(.72f,.45f,1):new Color(1,.82f,.4f);
            color.a=Mathf.Lerp(.42f,.76f,Mathf.Clamp01(progress));art.color=color;
        }
        public void Hide(){if(art!=null)art.enabled=false;}
    }
}
