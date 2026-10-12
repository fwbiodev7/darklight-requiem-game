using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Darklight
{
    // Authored world coordinates stay unchanged; only the visual surface is rebuilt.
    public sealed class DarklightModularMap:MonoBehaviour
    {
        readonly List<Tile> temporary=new();
        public Tilemap Ground{get;private set;}
        static int Theme(int chapter)=>chapter<6?chapter:chapter==6?2:chapter==7?0:chapter==8?4:5;
        public static void Build(Transform root,DarklightMapDefinition map,int chapter,Sprite[] floors,Sprite[] architecture,Material material)
        {
            var owner=root.gameObject.AddComponent<DarklightModularMap>();int theme=Theme(chapter)*4;float cellSize=map.CellSize;
            var grid=new GameObject("CenarioModular",typeof(Grid)).GetComponent<Grid>();grid.transform.SetParent(root,false);grid.cellSize=new Vector3(cellSize,cellSize,1);
            var ground=new GameObject("PisoEmBlocos",typeof(Tilemap),typeof(TilemapRenderer));ground.transform.SetParent(grid.transform,false);owner.Ground=ground.GetComponent<Tilemap>();var renderer=ground.GetComponent<TilemapRenderer>();renderer.sortingOrder=-200;renderer.sharedMaterial=material;renderer.mode=TilemapRenderer.Mode.Chunk;
            var tiles=new Tile[4];for(int i=0;i<4;i++){tiles[i]=ScriptableObject.CreateInstance<Tile>();tiles[i].sprite=floors[theme+i];tiles[i].flags=TileFlags.None;tiles[i].colliderType=Tile.ColliderType.None;owner.temporary.Add(tiles[i]);}
            var bounds=map.GridBounds;var block=new BoundsInt(bounds.xMin,bounds.yMin,0,bounds.width,bounds.height,1);var buffer=new TileBase[bounds.width*bounds.height];
            for(int y=bounds.yMin;y<bounds.yMax;y++)for(int x=bounds.xMin;x<bounds.xMax;x++){uint hash=Hash(x,y);bool open=map.Walkable(new Vector2Int(x,y));int variant=open?(hash%17==0?2:0):1;buffer[(y-bounds.yMin)*bounds.width+x-bounds.xMin]=tiles[variant];}
            owner.Ground.SetTilesBlock(block,buffer);
            for(int y=bounds.yMin;y<bounds.yMax;y++)for(int x=bounds.xMin;x<bounds.xMax;x++){var cell=new Vector3Int(x,y,0);var sprite=owner.Ground.GetSprite(cell);float scale=cellSize/sprite.bounds.size.x;owner.Ground.SetTransformMatrix(cell,Matrix4x4.Scale(Vector3.one*scale));owner.Ground.SetColor(cell,map.Walkable(new Vector2Int(x,y))?Color.white:new Color(.24f,.25f,.30f));}
            // Long northern boundaries become facades. Their bases remain in the existing blocked cells.
            for(int y=bounds.yMin;y<bounds.yMax;y++)for(int x=bounds.xMin;x<bounds.xMax-5;x++)
            {
                bool boundary=true;for(int k=0;k<6;k++)if(map.Walkable(new Vector2Int(x+k,y))||!map.Walkable(new Vector2Int(x+k,y-1))){boundary=false;break;}
                if(!boundary)continue;Vector2 foot=new Vector2(x+3,y+.05f)*cellSize;var facade=Art(root,"FachadaModular",architecture[theme],foot,material);float width=cellSize*6;facade.transform.localScale=Vector3.one*width/facade.sprite.bounds.size.x;Occlusion(facade);x+=5;
            }
            // Reuse each theme's landmark and furniture only inside existing solid islands.
            int index=0;foreach(var shape in map.Art.blocked)
            {
                Vector2 low=shape.points[0],high=low;foreach(var p in shape.points){low=Vector2.Min(low,p);high=Vector2.Max(high,p);}float width=(high.x-low.x)/map.Art.pixelsPerUnit,height=(high.y-low.y)/map.Art.pixelsPerUnit;
                if(width>7||height>7||width<.8f||height<.6f)continue;
                Vector2 pixel=new Vector2((low.x+high.x)*.5f,high.y);Vector2 point=map.Art.World(pixel)+Vector2.up*cellSize*.3f;
                var detail=Art(root,"DetalheSolido_"+index,architecture[theme+(index%2==0?2:3)],point,material);float visualHeight=Mathf.Clamp(height+1.1f,1.3f,3.8f);detail.transform.localScale=Vector3.one*visualHeight/detail.sprite.bounds.size.y;Occlusion(detail);index++;
            }
            foreach(var point in map.Arenas){var seal=Art(root,"SeloNoPiso",floors[theme+3],point,material);seal.sortingOrder=-198;seal.transform.localScale=Vector3.one*(cellSize*4)/seal.sprite.bounds.size.x;}
            DarklightPaintedMap.BuildGeometry(root,map);
        }
        static uint Hash(int x,int y){unchecked{return (uint)(x*73856093^y*19349663);}}
        static SpriteRenderer Art(Transform root,string name,Sprite sprite,Vector2 point,Material material){var obj=new GameObject(name);obj.transform.SetParent(root,false);obj.transform.localPosition=point;var r=obj.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=100-Mathf.RoundToInt(point.y*10);return r;}
        static void Occlusion(SpriteRenderer art){var fade=art.gameObject.AddComponent<DarklightMapOcclusion>();fade.width=art.bounds.size.x;fade.height=art.bounds.size.y;}
        void OnDestroy(){foreach(var tile in temporary)if(tile!=null){if(Application.isPlaying)Destroy(tile);else DestroyImmediate(tile);}}
    }
}
