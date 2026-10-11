using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    // Authored rooms and routes, in 64-pixel floor cells. Seals retain their save identifiers.
    public sealed class DarklightMapDefinition
    {
        public readonly Vector2 Spawn, Ally, Relic, Portal;
        public readonly Vector2[] Arenas, Checkpoints;
        public readonly RectInt[] Rooms;
        public readonly List<RectInt> Spaces = new();
        public Rect Bounds { get; private set; }
        public RectInt GridBounds { get; private set; }
        public float CellSize {get;private set;}=DarklightMapLayout.TileSize;
        public DarklightMapArt Art {get;private set;}
        static readonly DarklightMapDefinition[] authored=new DarklightMapDefinition[6];
        DarklightMapDefinition(DarklightMapArt art)
        {
            Art=art;CellSize=.5f;Spawn=art.World(art.spawn);Ally=art.World(art.ally);Relic=art.World(art.relic);Portal=art.World(art.portal);
            Arenas=Array.ConvertAll(art.arenas,art.World);Checkpoints=Array.ConvertAll(art.checkpoints,art.World);
            Bounds=new Rect(-art.width/art.pixelsPerUnit*.5f,-art.height/art.pixelsPerUnit*.5f,art.width/art.pixelsPerUnit,art.height/art.pixelsPerUnit);
            int x0=Mathf.FloorToInt(Bounds.xMin/CellSize),y0=Mathf.FloorToInt(Bounds.yMin/CellSize);
            GridBounds=new RectInt(x0,y0,Mathf.CeilToInt(Bounds.xMax/CellSize)-x0,Mathf.CeilToInt(Bounds.yMax/CellSize)-y0);
            var points=new[]{Spawn,Arenas[0],Arenas[1],Arenas[2],Arenas[3],Relic};Rooms=new RectInt[6];
            for(int i=0;i<points.Length;i++)Rooms[i]=new RectInt(Mathf.FloorToInt(points[i].x/CellSize)-4,Mathf.FloorToInt(points[i].y/CellSize)-4,8,8);
        }
        DarklightMapDefinition(Vector2Int[] centers, Vector2Int[] sizes, int[,] links, int relicRoom)
        {
            Rooms=new RectInt[centers.Length];
            for(int i=0;i<centers.Length;i++) { var c=centers[i];var s=sizes[i];Rooms[i]=new RectInt(c.x-s.x/2,c.y-s.y/2,s.x,s.y);Spaces.Add(Rooms[i]); }
            for(int i=0;i<links.GetLength(0);i++)
            {
                var a=centers[links[i,0]];var b=centers[links[i,1]];
                // Alternate elbow direction to give the streets, wings and courtyards different silhouettes.
                var bend=i%2==0?new Vector2Int(b.x,a.y):new Vector2Int(a.x,b.y);
                Connect(a,bend);Connect(bend,b);
            }
            Vector2 Point(Vector2Int p)=>new Vector2(p.x+.5f,p.y+.5f)*DarklightMapLayout.TileSize;
            Spawn=Point(centers[0]);Ally=Spawn+new Vector2(-2,1.4f);Relic=Point(centers[relicRoom]);
            Arenas=new[]{Point(centers[1]),Point(centers[2]),Point(centers[3]),Point(centers[4])};
            Checkpoints=new[]{Spawn,Arenas[0]+Vector2.left*2,Arenas[1]+Vector2.down*2,Arenas[2]+Vector2.left*2};Portal=Arenas[3]+Vector2.up*2;
            int x0=int.MaxValue,y0=int.MaxValue,x1=int.MinValue,y1=int.MinValue;
            foreach(var r in Spaces){x0=Mathf.Min(x0,r.xMin);y0=Mathf.Min(y0,r.yMin);x1=Mathf.Max(x1,r.xMax);y1=Mathf.Max(y1,r.yMax);}
            GridBounds=new RectInt(x0-2,y0-2,x1-x0+4,y1-y0+4);
            Bounds=new Rect(GridBounds.x*DarklightMapLayout.TileSize,GridBounds.y*DarklightMapLayout.TileSize,GridBounds.width*DarklightMapLayout.TileSize,GridBounds.height*DarklightMapLayout.TileSize);
        }
        void Connect(Vector2Int a,Vector2Int b) { Spaces.Add(new RectInt(Mathf.Min(a.x,b.x)-2,Mathf.Min(a.y,b.y)-2,Mathf.Abs(a.x-b.x)+4,Mathf.Abs(a.y-b.y)+4)); }
        public bool Walkable(Vector2Int cell)
        {
            if(Art!=null)return GridBounds.Contains(cell)&&Art.Contains((new Vector2(cell.x,cell.y)+Vector2.one*.5f)*CellSize);
            foreach(var r in Spaces)if(r.Contains(cell))return true;return false;
        }
        public Vector2 SafeSpawn(Vector2 point,float radius=.32f)
        {
            bool ClearPoint(Vector2 p)
            {
                if(!Walkable(new Vector2Int(Mathf.FloorToInt(p.x/CellSize),Mathf.FloorToInt(p.y/CellSize))))return false;
                foreach(var c in Physics2D.OverlapCircleAll(p,radius))if(!c.isTrigger&&c.GetComponentInParent<DarklightCombatant>()==null)return false;
                return true;
            }
            if(ClearPoint(point))return point;
            Vector2 best=point;float distance=float.MaxValue;
            var center=new Vector2Int(Mathf.FloorToInt(point.x/CellSize),Mathf.FloorToInt(point.y/CellSize));
            for(int y=-12;y<=12;y++)for(int x=-12;x<=12;x++)
            {
                var p=(new Vector2(center.x+x,center.y+y)+Vector2.one*.5f)*CellSize;float d=(p-point).sqrMagnitude;
                if(d<distance&&ClearPoint(p)){distance=d;best=p;}
            }
            if(distance==float.MaxValue)throw new InvalidOperationException("No clear spawn near "+point);
            return best;
        }
        public bool InArena(int index,Vector2 p)=>Vector2.Distance(p,Arenas[index])<3.8f;
        static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
        public static DarklightMapDefinition ForChapter(int chapter)
        {
            chapter=Mathf.Clamp(chapter,0,5);
            if(authored[chapter]!=null)return authored[chapter];
            var data=Resources.Load<TextAsset>("Darklight/Maps/Map"+(chapter+1)+"Data");
            if(data!=null)return authored[chapter]=new DarklightMapDefinition(JsonUtility.FromJson<DarklightMapArt>(data.text));
            Vector2Int[][] centers={
                new[]{V(-13,-6),V(-5,-5),V(0,5),V(9,-3),V(13,7),V(-9,5)},
                new[]{V(-12,0),V(-5,-6),V(3,1),V(10,-6),V(12,7),V(-5,7)},
                new[]{V(0,-11),V(-9,-4),V(8,0),V(-8,6),V(0,12),V(10,8)},
                new[]{V(-13,-7),V(-12,4),V(-2,-2),V(8,-7),V(11,6),V(1,9)},
                new[]{V(-13,-6),V(-6,2),V(4,-7),V(4,7),V(13,0),V(-8,9)},
                new[]{V(-12,-8),V(-4,-7),V(7,-2),V(-5,5),V(8,10),V(-13,8)}
            };
            Vector2Int[] sizes={V(7,7),V(8,8),V(9,8),V(8,9),V(10,9),V(6,6)};
            int[][,] paths={ new[,]{{0,1},{1,2},{2,3},{3,4},{0,5},{5,2},{1,3}},new[,]{{0,1},{1,2},{2,3},{3,4},{0,5},{5,2},{5,4}},new[,]{{0,1},{1,2},{2,3},{3,4},{2,5},{5,4},{1,3}},new[,]{{0,1},{1,2},{2,3},{3,4},{1,5},{5,4},{0,3}},new[,]{{0,1},{1,2},{2,3},{3,4},{1,5},{5,3},{2,4}},new[,]{{0,1},{1,2},{2,3},{3,4},{0,5},{5,3},{2,4}} };
            return new DarklightMapDefinition(centers[Mathf.Clamp(chapter,0,5)],sizes,paths[Mathf.Clamp(chapter,0,5)],5);
        }
    }
}
