using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // The supplied composition is the ground layer. Geometry adds collision and depth without painting over it.
    public sealed class DarklightPaintedMap : MonoBehaviour
    {
        readonly List<Sprite> temporary=new();
        public static void Build(Transform root,DarklightMapDefinition map,Sprite image,Material material)
        {
            var owner=root.gameObject.AddComponent<DarklightPaintedMap>();
            var ground=new GameObject("MapaIlustrado").AddComponent<SpriteRenderer>();ground.transform.SetParent(root,false);
            ground.sprite=image;ground.transform.localScale=Vector3.one*(image.pixelsPerUnit/map.Art.pixelsPerUnit);ground.sharedMaterial=material;ground.sortingOrder=-200;
            var details=DarklightMapDetails.Create(root,map,image,material);
            foreach(var region in map.Art.foreground)
            {
                var foot=map.Art.World(region.foot);var obj=new GameObject(region.name);obj.transform.SetParent(root,false);obj.transform.localPosition=foot;
                float left=float.MaxValue,right=float.MinValue,top=float.MaxValue,bottom=float.MinValue;
                foreach(var point in region.outline){left=Mathf.Min(left,point.x);right=Mathf.Max(right,point.x);top=Mathf.Min(top,point.y);bottom=Mathf.Max(bottom,point.y);}
                // All six authored maps use rectangular foreground cutouts. FullRect sprites
                // reject OverrideGeometry; cropping at creation keeps each piece from covering
                // the entire opaque map, without requiring readable textures or duplicate PNGs.
                if(details!=null)
                {
                    var source=Rect.MinMaxRect(left,top,right,bottom);
                    foreach(var part in details.Partition(source)){var sprite=details.Foreground(image,map,part);owner.temporary.Add(sprite);var piece=new GameObject("TrechoFrontal");piece.transform.SetParent(obj.transform,false);piece.transform.position=map.Art.World(new Vector2(part.center.x,part.yMax));var renderer=piece.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;renderer.sortingOrder=100-Mathf.RoundToInt(foot.y*10);}
                    var fade=obj.AddComponent<DarklightMapOcclusion>();fade.width=(right-left)/map.Art.pixelsPerUnit;fade.height=(bottom-top)/map.Art.pixelsPerUnit;
                }
                else
                {
                    var crop=new Rect(image.rect.x+left,image.rect.y+map.Art.height-bottom,right-left,bottom-top);var pivot=new Vector2((region.foot.x-left)/crop.width,(bottom-region.foot.y)/crop.height);
                    var sprite=Sprite.Create(image.texture,crop,pivot,map.Art.pixelsPerUnit,0,SpriteMeshType.FullRect);owner.temporary.Add(sprite);var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;renderer.sortingOrder=100-Mathf.RoundToInt(foot.y*10);var fade=obj.AddComponent<DarklightMapOcclusion>();fade.width=renderer.bounds.size.x;fade.height=renderer.bounds.size.y;
                }
            }
            BuildGeometry(root,map);
        }
        internal static void BuildGeometry(Transform root,DarklightMapDefinition map)
        {
            // Merge closed navigation cells into runs, then merge identical runs vertically.
            // This keeps collision aligned to the image while avoiding one object per blocked cell.
            var active=new Dictionary<(int x,int width),RectInt>();var grid=map.GridBounds;
            for(int y=grid.yMin;y<grid.yMax;y++)
            {
                var runs=new HashSet<(int x,int width)>();int start=grid.xMin;
                while(start<grid.xMax)
                {
                    if(map.Walkable(new Vector2Int(start,y))){start++;continue;}
                    int end=start+1;while(end<grid.xMax&&!map.Walkable(new Vector2Int(end,y)))end++;
                    runs.Add((start,end-start));start=end;
                }
                var completed=new List<(int x,int width)>();
                foreach(var key in active.Keys)if(!runs.Contains(key))completed.Add(key);
                foreach(var key in completed){Collider(root,active[key],map.CellSize);active.Remove(key);}
                foreach(var key in runs)
                {
                    if(active.TryGetValue(key,out var rect)){rect.height++;active[key]=rect;}
                    else active[key]=new RectInt(key.x,y,key.width,1);
                }
            }
            foreach(var rect in active.Values)Collider(root,rect,map.CellSize);
            float s=map.CellSize;var b=map.Bounds;
            Wall(root,new Vector2(b.xMin-s,b.center.y),new Vector2(s*2,b.height+s*4));
            Wall(root,new Vector2(b.xMax+s,b.center.y),new Vector2(s*2,b.height+s*4));
            Wall(root,new Vector2(b.center.x,b.yMin-s),new Vector2(b.width,s*2));
            Wall(root,new Vector2(b.center.x,b.yMax+s),new Vector2(b.width,s*2));
            var nav=root.gameObject.AddComponent<DarklightNavigation>();nav.Initialize(map);
        }
        static void Collider(Transform root,RectInt rect,float size)=>Wall(root,(new Vector2(rect.x,rect.y)+new Vector2(rect.width,rect.height)*.5f)*size,new Vector2(rect.width,rect.height)*size);
        static void Wall(Transform root,Vector2 point,Vector2 size)
        {var obj=new GameObject("LimiteDoCenario");obj.transform.SetParent(root,false);obj.transform.localPosition=point;obj.AddComponent<BoxCollider2D>().size=size;}
        void OnDestroy(){foreach(var sprite in temporary)if(sprite!=null){if(Application.isPlaying)Destroy(sprite);else DestroyImmediate(sprite);}}
    }
}
