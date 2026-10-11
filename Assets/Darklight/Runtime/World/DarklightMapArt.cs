using System;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed class DarklightMapShape
    {
        public Vector2[] points;
        public bool Contains(Vector2 p)
        {
            bool inside=false;
            for(int i=0,j=points.Length-1;i<points.Length;j=i++)
            {
                var a=points[i];var b=points[j];
                if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
            }
            return inside;
        }
    }
    [Serializable] public sealed class DarklightMapForeground
    {
        public string name;public Vector2 foot;public Vector2[] outline;
    }
    [Serializable] public sealed class DarklightMapArt
    {
        public int width,height;public float pixelsPerUnit=24;
        public Vector2 spawn,ally,relic,portal;public Vector2[] arenas,checkpoints;
        public DarklightMapShape[] walk,blocked;public DarklightMapForeground[] foreground;
        public Vector2 World(Vector2 p)=>new Vector2(p.x-width*.5f,height*.5f-p.y)/pixelsPerUnit;
        public Vector2 Pixel(Vector2 p)=>new Vector2(p.x*pixelsPerUnit+width*.5f,height*.5f-p.y*pixelsPerUnit);
        public bool Contains(Vector2 p)
        {
            p=Pixel(p);bool inside=false;
            foreach(var shape in walk)if(shape.Contains(p)){inside=true;break;}
            if(!inside)return false;
            foreach(var shape in blocked)if(shape.Contains(p))return false;
            return true;
        }
    }
}
