using System;
using UnityEngine;
namespace Darklight
{
    public static class DarklightMapLayout
    {
        public const float TileSize=4f/3f;
        public static readonly Rect CameraBounds=DarklightMapDefinition.ForChapter(0).Bounds;
        public static void Build(Transform root,int chapter,Sprite[] floors,Sprite[] architecture,Material material)
        {
            var authored=DarklightMapDefinition.ForChapter(chapter);
            var image=Resources.Load<Sprite>("Darklight/Maps/map"+(chapter+1));
            if(authored.Art!=null&&image!=null){DarklightPaintedMap.Build(root,authored,image,material);return;}
            if(floors?.Length!=24||architecture?.Length!=24)throw new InvalidOperationException("Six map kits require 24 floor and architecture sprites.");
            var map=DarklightMapDefinition.ForChapter(chapter);int start=chapter*4;
            var terrain=new GameObject("Pisos_64px").transform;terrain.SetParent(root,false);
            var grid=map.GridBounds;
            for(int y=grid.yMin;y<grid.yMax;y++) for(int x=grid.xMin;x<grid.xMax;x++)
            {
                var cell=new Vector2Int(x,y);bool open=map.Walkable(cell);Vector2 foot=(new Vector2(x,y)+Vector2.one*.5f)*TileSize;
                bool roomFloor=false;foreach(var room in map.Rooms)if(room.Contains(cell)){roomFloor=true;break;}
                int variant=open?((x*17+y*13)%31==0?2:roomFloor?0:1):0;
                var tile=Art(terrain,"Piso_"+x+"_"+y,floors[start+variant],foot,material);tile.transform.localScale=Vector3.one*TileSize/tile.sprite.bounds.size.x;
                tile.flipX=(x&1)==1;tile.flipY=(y&1)==1;tile.sortingOrder=-200;
                if(!open){tile.color=new Color(.27f,.28f,.34f);Collider(root,"TerrenoFechado",foot,Vector2.one*TileSize);}
                if(!open && map.Walkable(cell+Vector2Int.down))
                {
                    // Three floor cells per facade preserve architecture scale relative to Fabio.
                    if((x-grid.xMin)%3!=0)continue;
                    float width=TileSize*3;
                    var wall=Art(root,"Fachada",architecture[start],foot+Vector2.right*TileSize-Vector2.up*TileSize*.5f,material);
                    wall.transform.localScale=Vector3.one*width/wall.sprite.bounds.size.x;
                    var fade=wall.gameObject.AddComponent<DarklightMapOcclusion>();fade.width=width;fade.height=wall.bounds.size.y;
                }
            }
            for(int i=0;i<map.Rooms.Length;i++)
            {
                var room=map.Rooms[i];Vector2 a=new Vector2(room.xMin+1.3f,room.yMax-1.4f)*TileSize;
                Vector2 b=new Vector2(room.xMax-1.3f,room.yMin+1.2f)*TileSize;
                Prop(root,"MarcoDaArea_"+i,architecture[start+2],a,chapter==4?2.6f:2.0f,material,.5f);
                Prop(root,"Mobilia_"+i,architecture[start+3],b,chapter==4?1.0f:1.2f,material,.8f);
                Prop(root,"ConjuntoLateral_"+i,architecture[start+3],new Vector2(room.xMin+1.4f,room.yMin+2.1f)*TileSize,chapter==4?.8f:1.0f,material,.65f);
                if(i>0)Prop(root,"MarcoLeste_"+i,architecture[start+2],new Vector2(room.xMax-1.2f,room.yMax-2.2f)*TileSize,chapter==4?2.2f:1.8f,material,.45f);
                if(i==0||i==4)
                {
                    var entry=Art(root,"Entrada",architecture[start+1],new Vector2(room.center.x,room.yMax)*TileSize,material);
                    entry.transform.localScale=Vector3.one*2.7f/entry.sprite.bounds.size.x;
                    var fade=entry.gameObject.AddComponent<DarklightMapOcclusion>();fade.width=2.7f;fade.height=entry.bounds.size.y;
                }
            }
            foreach(var p in map.Arenas)
            {
                var seal=Art(terrain,"MarcoDoSelo",floors[start+3],p,material);seal.transform.localScale=Vector3.one*TileSize/seal.sprite.bounds.size.x;seal.sortingOrder=-198;
            }
            var nav=root.gameObject.AddComponent<DarklightNavigation>();nav.Initialize(map);
        }
        static SpriteRenderer Art(Transform root,string name,Sprite sprite,Vector2 foot,Material material)
        {
            var obj=new GameObject(name);obj.transform.SetParent(root,false);obj.transform.localPosition=foot;
            var art=obj.AddComponent<SpriteRenderer>();art.sprite=sprite;art.sharedMaterial=material;art.sortingOrder=100-Mathf.RoundToInt(foot.y*10);return art;
        }
        static void Prop(Transform root,string name,Sprite sprite,Vector2 foot,float height,Material material,float width)
        {
            var art=Art(root,name,sprite,foot,material);art.transform.localScale=Vector3.one*height/sprite.bounds.size.y;
            Collider(root,name+"_Base",foot+Vector2.up*.12f,new Vector2(Mathf.Min(width,art.bounds.size.x*.8f),.3f));
            var fade=art.gameObject.AddComponent<DarklightMapOcclusion>();fade.width=art.bounds.size.x;fade.height=height;
        }
        static void Collider(Transform root,string name,Vector2 point,Vector2 size)
        {var obj=new GameObject(name);obj.transform.SetParent(root,false);obj.transform.localPosition=point;obj.AddComponent<BoxCollider2D>().size=size;}
    }
}
