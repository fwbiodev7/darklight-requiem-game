using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public enum DarklightThreat { Parryable, Area, Projectile }
    // Cached geometric floor marks. Their shape describes the actual damage footprint.
    public sealed class DarklightAttackWarning : MonoBehaviour
    {
        static readonly Sprite[] shapes=new Sprite[3];
        static readonly Dictionary<int, Sprite> parryArcs = new();
        static readonly Dictionary<(int Count, int Spacing), Sprite> projectileSpreads = new();
        SpriteRenderer art, projectileOverlay;
        public DarklightThreat Threat {get;private set;}
        public float Radius {get;private set;}
        public bool Visible=>art!=null&&art.enabled;
        internal const int DefaultProjectileSpacing=18;
        internal static float ProjectileOffset(int index, int count, int spacing=DefaultProjectileSpacing) =>
            (count * spacing == 360 ? index : index - (count - 1) * .5f) * spacing;

        static bool ProjectileMark(float x, float y) => y >= 0 && y < .96f &&
            (Mathf.Abs(x) < .025f || y > .78f && Mathf.Abs(Mathf.Abs(x) - (.96f - y) * .7f) < .025f);

        static bool ProjectileSpreadMark(float x, float y, Vector2[] directions)
        {
            foreach (var direction in directions)
                if (ProjectileMark(x * direction.y - y * direction.x, x * direction.x + y * direction.y)) return true;
            return false;
        }

        static Sprite Shape(DarklightThreat kind, int halfAngle, int projectileCount = 1,int spacing=DefaultProjectileSpacing)
        {
            int index=(int)kind;
            bool customArc = kind == DarklightThreat.Parryable && halfAngle != 70;
            bool spread = kind == DarklightThreat.Projectile && projectileCount > 1;
            var pattern=(projectileCount,spacing);
            if (customArc && parryArcs.TryGetValue(halfAngle, out var arc)) return arc;
            if (spread && projectileSpreads.TryGetValue(pattern,out var cachedSpread)) return cachedSpread;
            if (!customArc && !spread && shapes[index] != null) return shapes[index];
            var directions = System.Array.Empty<Vector2>();
            if (spread)
            {
                directions = new Vector2[projectileCount];
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = ProjectileOffset(i, projectileCount,spacing) * Mathf.Deg2Rad;
                    directions[i] = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
                }
            }
            string name = customArc ? $"Sinal_{kind}_{halfAngle}" : spread ? $"Sinal_{kind}_{projectileCount}_{spacing}" : "Sinal_" + kind;
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name=name};
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=(x+.5f-size*.5f)/(size*.5f),dy=(y+.5f-size*.5f)/(size*.5f);
                bool mark;
                if(kind==DarklightThreat.Projectile)mark=spread?ProjectileSpreadMark(dx,dy,directions):ProjectileMark(dx,dy);
                else
                {
                    float r=Mathf.Sqrt(dx*dx+dy*dy);
                    if(kind==DarklightThreat.Area)mark=r<.99f&&(r>.94f||r>.84f&&r<.88f||r>.86f&&((x+y)/4)%2==0||Mathf.Abs(dx-dy)<.025f&&r<.18f||Mathf.Abs(dx+dy)<.025f&&r<.18f);
                    else
                    {
                        float angle=Mathf.Abs(Mathf.Atan2(dx,dy))*Mathf.Rad2Deg;
                        mark=r<.99f&&angle<=halfAngle&&(r>.94f||angle>halfAngle-3||r<.17f&&Mathf.Abs(dx)<.035f);
                    }
                }
                if(mark)pixels[y*size+x]=new Color32(255,255,255,255);
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
            if (customArc) parryArcs[halfAngle] = sprite;
            else if (spread) projectileSpreads[pattern] = sprite;
            else shapes[index] = sprite;
            return sprite;
        }
        public void Show(Vector2 origin,Vector2 direction,float radius,DarklightThreat kind,float progress,int halfAngle=70)
            => Present(origin,direction,radius,kind,progress,Shape(kind,halfAngle));

        internal void ShowProjectileSpread(Vector2 origin, Vector2 direction, float radius, float progress, int count)
            => Present(origin, direction, radius, DarklightThreat.Projectile, progress,
                Shape(DarklightThreat.Projectile, 70, count));

        internal void ShowProjectileOverlay(Vector2 direction, int count, int spacing, float progress)
        {
            if (projectileOverlay == null)
            {
                var child = new GameObject("TrajetoriasDoAtaque");
                child.transform.SetParent(transform, false);
                child.layer=gameObject.layer;
                projectileOverlay=child.AddComponent<SpriteRenderer>();
                projectileOverlay.sortingOrder=art.sortingOrder-1;
            }
            projectileOverlay.transform.rotation=Quaternion.FromToRotation(Vector3.up,direction);
            projectileOverlay.sprite=Shape(DarklightThreat.Projectile,70,count,spacing);
            projectileOverlay.color=new Color(.72f,.45f,1,Mathf.Lerp(.25f,.50f,Mathf.Clamp01(progress)));
            projectileOverlay.enabled=true;
        }

        void Present(Vector2 origin,Vector2 direction,float radius,DarklightThreat kind,float progress,Sprite sprite)
        {
            if(art==null){art=gameObject.AddComponent<SpriteRenderer>();art.sortingOrder=79;art.gameObject.layer=transform.parent!=null?transform.parent.gameObject.layer:0;}
            Threat=kind;Radius=radius;art.sprite=sprite;art.enabled=true;
            transform.position=origin;transform.localScale=Vector3.one*(radius*2);
            transform.rotation=direction.sqrMagnitude>.01f?Quaternion.FromToRotation(Vector3.up,direction):Quaternion.identity;
            Color color=kind==DarklightThreat.Area?new Color(.95f,.12f,.22f):kind==DarklightThreat.Projectile?new Color(.72f,.45f,1):new Color(1,.82f,.4f);
            color.a=Mathf.Lerp(.42f,.76f,Mathf.Clamp01(progress));art.color=color;
        }
        public void Hide(){if(art!=null)art.enabled=false;if(projectileOverlay!=null)projectileOverlay.enabled=false;}
    }
}
