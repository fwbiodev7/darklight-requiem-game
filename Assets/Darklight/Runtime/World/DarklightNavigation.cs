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
        static readonly RaycastHit2D[] clearanceHits=new RaycastHit2D[128],travelHits=new RaycastHit2D[128];
        static readonly Collider2D[] overlaps=new Collider2D[128];
        public void Initialize(DarklightMapDefinition definition){map=definition;Instance=this;}
        Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.FloorToInt(p.x/map.CellSize),Mathf.FloorToInt(p.y/map.CellSize));
        Vector2 Center(Vector2Int c)=>(new Vector2(c.x,c.y)+Vector2.one*.5f)*map.CellSize;
        public static bool Clear(Vector2 a,Vector2 b,float radius=.3f)
        {
            Vector2 delta=b-a;
            int count=Physics2D.CircleCast(a,radius,delta.normalized,new ContactFilter2D{useTriggers=false,useLayerMask=false},clearanceHits,delta.magnitude);
            if(count>=clearanceHits.Length)return false;
            for(int i=0;i<count;i++)if(clearanceHits[i].collider.GetComponentInParent<DarklightCombatant>()==null)return false;
            return true;
        }
        public bool FindPath(Vector2 from,Vector2 to,List<Vector2> route)
        {
            route.Clear();if(map==null)return false;
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
            var start=ConnectedCell(from);var end=ConnectedCell(to);parents.Clear();queue.Clear();
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
        Vector2Int ConnectedCell(Vector2 point)
        {
            var cell=Cell(point);
            if(links.TryGetValue(cell,out var direct)&&direct.Count>0&&Clear(point,Center(cell)))return cell;
            var best=cell;float distance=float.MaxValue;
            // A clear position near a wall can belong to a cell whose center is too
            // close to that wall. Join a nearby node only across a physical clear path.
            for(int y=-2;y<=2;y++)for(int x=-2;x<=2;x++)
            {
                var candidate=cell+new Vector2Int(x,y);var center=Center(candidate);
                float d=(center-point).sqrMagnitude;
                if(d<distance&&links.TryGetValue(candidate,out var neighbours)&&neighbours.Count>0&&Clear(point,center))
                {best=candidate;distance=d;}
            }
            return best;
        }
        const float CastSkin=.008f;
        static readonly RaycastHit2D[] movementHits=new RaycastHit2D[64];
        static PhysicsMaterial2D frictionless;
        public static PhysicsMaterial2D Frictionless { get { if(frictionless==null)frictionless=new PhysicsMaterial2D("DarklightSemAtrito") {friction=0,bounciness=0};return frictionless; } }
        static bool Blocks(RaycastHit2D hit,Vector2 direction,Rigidbody2D owner)
        {
            if(hit.collider==null||hit.collider.isTrigger||hit.rigidbody==owner||hit.collider.GetComponentInParent<DarklightCombatant>()!=null)return false;
            // Touching a wall while moving parallel to it or away from it is not a new impact.
            return Vector2.Dot(direction,hit.normal)<-.0001f;
        }
        static float CircleTravel(Rigidbody2D owner,Vector2 origin,float radius,Vector2 delta,out Vector2 normal)
        {
            normal=Vector2.zero;float length=delta.magnitude,allowed=length;
            if(length<.00001f)return 0;var direction=delta/length;
            // A dash/pull must not escape through a solid region from an invalid overlapping origin.
            int overlapping=Physics2D.OverlapCircle(origin,radius,new ContactFilter2D{useTriggers=false,useLayerMask=false},overlaps);
            if(overlapping>=overlaps.Length){normal=-direction;return 0;}
            for(int i=0;i<overlapping;i++)
                if(overlaps[i].attachedRigidbody!=owner&&overlaps[i].GetComponentInParent<DarklightCombatant>()==null&&Vector2.Distance(overlaps[i].ClosestPoint(origin),origin)<radius-.015f)
                {normal=-direction;return 0;}
            int hits=Physics2D.CircleCast(origin,radius,direction,new ContactFilter2D{useTriggers=false,useLayerMask=false},travelHits,length+CastSkin);
            if(hits>=travelHits.Length){normal=-direction;return 0;}
            for(int i=0;i<hits;i++)if(Blocks(travelHits[i],direction,owner)&&travelHits[i].distance-CastSkin<allowed)
                {allowed=Mathf.Max(0,travelHits[i].distance-CastSkin);normal=travelHits[i].normal;}
            return allowed;
        }
        public static void SafeStep(Rigidbody2D body,Vector2 delta,float radius=.3f)
        {
            if(body==null||delta.sqrMagnitude<.00000001f)return;
            var origin=body.position;float travel=CircleTravel(body,origin,radius,delta,out var normal);
            var advanced=delta.normalized*travel;
            var residual=delta-advanced;
            if(normal!=Vector2.zero)residual-=normal*Mathf.Min(0,Vector2.Dot(residual,normal));
            else residual=Vector2.zero;
            // Cast again from the advanced position: sliding may encounter a second wall at a corner.
            float slide=CircleTravel(body,origin+advanced,radius,residual,out _);
            body.position=origin+advanced+(residual.sqrMagnitude>.00000001f?residual.normalized*slide:Vector2.zero);
        }
        static float BodyTravel(Rigidbody2D body,Vector2 delta,out Vector2 normal)
        {
            normal=Vector2.zero;float length=delta.magnitude,allowed=length;
            if(length<.00001f)return 0;var direction=delta/length;
            int count=body.Cast(direction,new ContactFilter2D {useTriggers=false,useLayerMask=false},movementHits,length+CastSkin);
            for(int i=0;i<count;i++)
            {
                var hit=movementHits[i];
                if(Blocks(hit,direction,body)&&hit.distance-CastSkin<allowed)
                {allowed=Mathf.Max(0,hit.distance-CastSkin);normal=hit.normal;}
            }
            return allowed;
        }
        public static Vector2 SlidingVelocity(Rigidbody2D body,Vector2 velocity,float step)
        {
            if(body==null||step<=0||velocity.sqrMagnitude<.00000001f)return Vector2.zero;
            var delta=velocity*step;
            float travel=BodyTravel(body,delta,out var normal);
            if(normal==Vector2.zero)return velocity;
            var advanced=delta.normalized*travel;
            var residual=delta-advanced;residual-=normal*Mathf.Min(0,Vector2.Dot(residual,normal));
            // Use the actual capsule and its existing offset for ordinary physics movement.
            // The second cast is conservative from the current position, avoiding teleportation.
            float slide=BodyTravel(body,residual,out _);
            return (advanced+(residual.sqrMagnitude>.00000001f?residual.normalized*slide:Vector2.zero))/step;
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}

