using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed class DarklightNavigation : MonoBehaviour
    {
        public static DarklightNavigation Instance { get;private set; }
        DarklightMapDefinition map;
        readonly Dictionary<Vector2Int,Vector2Int> parents=new();readonly Queue<Vector2Int> queue=new();
        readonly Dictionary<Vector2Int,List<Vector2Int>> links=new();bool cached;
        static readonly Vector2Int[] Steps={Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
        public void Initialize(DarklightMapDefinition definition){map=definition;Instance=this;}
        Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.FloorToInt(p.x/map.CellSize),Mathf.FloorToInt(p.y/map.CellSize));
        Vector2 Center(Vector2Int c)=>(new Vector2(c.x,c.y)+Vector2.one*.5f)*map.CellSize;
        public static bool Clear(Vector2 a,Vector2 b,float radius=.3f)
        {
            Vector2 delta=b-a;
            foreach(var hit in Physics2D.CircleCastAll(a,radius,delta.normalized,delta.magnitude))
                if(!hit.collider.isTrigger&&hit.collider.GetComponentInParent<DarklightCombatant>()==null)return false;
            return true;
        }
        public bool FindPath(Vector2 from,Vector2 to,List<Vector2> route)
        {
            if(!cached)
            {
                // Scenery is static for a chapter. Physics queries are paid once, not every enemy repath.
                Physics2D.SyncTransforms();var grid=map.GridBounds;
                for(int y=grid.yMin;y<grid.yMax;y++)for(int x=grid.xMin;x<grid.xMax;x++)
                {
                    var c=new Vector2Int(x,y);if(!map.Walkable(c))continue;var neighbours=new List<Vector2Int>(4);
                    foreach(var step in Steps)if(map.Walkable(c+step)&&Clear(Center(c),Center(c+step)))neighbours.Add(c+step);
                    links[c]=neighbours;
                }
                cached=true;
            }
            route.Clear();var start=Cell(from);var end=Cell(to);parents.Clear();queue.Clear();
            if(map==null||!map.Walkable(start)||!map.Walkable(end))return false;
            parents[start]=start;queue.Enqueue(start);
            while(queue.Count>0)
            {
                var current=queue.Dequeue();if(current==end)
                {
                    for(var c=end;c!=start;c=parents[c])route.Add(Center(c));route.Reverse();route.Add(to);return true;
                }
                foreach(var next in links[current])
                {
                    if(parents.ContainsKey(next))continue;
                    parents[next]=current;queue.Enqueue(next);
                }
            }
            return false;
        }
        public static void SafeStep(Rigidbody2D body,Vector2 delta,float radius=.3f)
        {
            float distance=delta.magnitude;
            foreach(var hit in Physics2D.CircleCastAll(body.position,radius,delta.normalized,distance))
                if(!hit.collider.isTrigger&&hit.collider.GetComponentInParent<DarklightCombatant>()==null)distance=Mathf.Min(distance,Mathf.Max(0,hit.distance-.03f));
            body.position+=delta.normalized*distance;
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
