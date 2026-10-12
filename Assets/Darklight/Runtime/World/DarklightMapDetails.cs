using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    // Detail chunks change texture density, never world geometry or mission coordinates.
    public sealed class DarklightMapDetails:MonoBehaviour
    {
        [Serializable] public sealed class Chunk{public string asset;public int x,y,width,height;public float scale=4;}
        [Serializable] public sealed class Manifest{public Chunk[] chunks;}
        readonly Dictionary<Chunk,Sprite> loaded=new();
        public Manifest Data{get;private set;}
        public static DarklightMapDetails Create(Transform root,DarklightMapDefinition map,Sprite original,Material material)
        {
            int index=0;for(int i=0;i<10;i++)if(ReferenceEquals(map,DarklightMapDefinition.ForChapter(i))){index=i;break;}
            var json=Resources.Load<TextAsset>("Darklight/Maps/Details/map"+(index+1)+"Manifest");if(json==null)return null;
            var data=JsonUtility.FromJson<Manifest>(json.text);if(data?.chunks==null||data.chunks.Length==0)return null;
            var owner=root.gameObject.AddComponent<DarklightMapDetails>();owner.Data=data;
            var ordered=new List<Chunk>(data.chunks);ordered.Sort((a,b)=>a.scale.CompareTo(b.scale));
            foreach(var chunk in ordered)
            {
                if(chunk.width<=0||chunk.height<=0||chunk.scale<1||chunk.x<0||chunk.y<0||chunk.x+chunk.width>map.Art.width||chunk.y+chunk.height>map.Art.height)throw new InvalidOperationException("Invalid detail chunk: "+chunk.asset);
                var sprite=Resources.Load<Sprite>(chunk.asset);if(sprite==null)continue;
                if(sprite.rect.width!=Mathf.RoundToInt(chunk.width*chunk.scale)||sprite.rect.height!=Mathf.RoundToInt(chunk.height*chunk.scale))throw new InvalidOperationException("Detail chunk dimensions: "+chunk.asset);
                owner.loaded.Add(chunk,sprite);var obj=new GameObject("TrechoDetalhado_"+chunk.x+"_"+chunk.y);obj.transform.SetParent(root,false);obj.transform.localPosition=map.Art.World(new Vector2(chunk.x+chunk.width*.5f,chunk.y+chunk.height*.5f));
                var r=obj.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=-199+Mathf.RoundToInt(chunk.scale);obj.transform.localScale=Vector3.one*(sprite.pixelsPerUnit/(map.Art.pixelsPerUnit*chunk.scale));
            }
            return owner;
        }
        public IEnumerable<Rect> Partition(Rect source)
        {
            // Uniform source cells make foreground crops align exactly with ground chunks.
            const int w=384,h=256;
            for(int y=Mathf.FloorToInt(source.yMin/h);y<Mathf.CeilToInt(source.yMax/h);y++)for(int x=Mathf.FloorToInt(source.xMin/w);x<Mathf.CeilToInt(source.xMax/w);x++)
            {float left=Mathf.Max(source.xMin,x*w),right=Mathf.Min(source.xMax,(x+1)*w),top=Mathf.Max(source.yMin,y*h),bottom=Mathf.Min(source.yMax,(y+1)*h);if(right>left&&bottom>top)yield return Rect.MinMaxRect(left,top,right,bottom);}
        }
        public Sprite Foreground(Sprite original,DarklightMapDefinition map,Rect part)
        {
            Chunk best=null;Sprite image=null;
            foreach(var pair in loaded){var c=pair.Key;if(part.xMin<c.x||part.yMin<c.y||part.xMax>c.x+c.width||part.yMax>c.y+c.height)continue;if(best==null||c.scale>best.scale){best=c;image=pair.Value;}}
            if(best!=null){var c=best;
                var rect=new Rect((part.x-c.x)*c.scale,(c.y+c.height-part.yMax)*c.scale,part.width*c.scale,part.height*c.scale);
                return Sprite.Create(image.texture,rect,new Vector2(.5f,0),map.Art.pixelsPerUnit*c.scale,0,SpriteMeshType.FullRect);}
            return Sprite.Create(original.texture,new Rect(original.rect.x+part.x,map.Art.height-part.yMax+original.rect.y,part.width,part.height),new Vector2(.5f,0),map.Art.pixelsPerUnit,0,SpriteMeshType.FullRect);
        }
    }
}
