using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Darklight
{
    [Serializable] public sealed class DarklightAtlasRegion
    {
        public int id,width,height;
        public string fog="",tracked="";
        public bool hasWaypoint;
        public Vector2 waypoint;
        public bool[] visitedAreas;
    }
    [Serializable] public sealed class DarklightAtlasState
    {
        public int version=1;
        public List<DarklightAtlasRegion> regions=new();
        public DarklightAtlasRegion Region(int id,int width,int height)
        {
            var r=regions.Find(x=>x.id==id);
            if(r==null){r=new DarklightAtlasRegion{id=id,width=width,height=height};regions.Add(r);}
            if(r.width!=width||r.height!=height){r.width=width;r.height=height;r.fog="";r.hasWaypoint=false;r.tracked="";}
            return r;
        }
        public static string EncodeFog(bool[] cells)
        {
            var bytes=new byte[(cells.Length+7)/8];for(int i=0;i<cells.Length;i++)if(cells[i])bytes[i>>3]|=(byte)(1<<(i&7));return Convert.ToBase64String(bytes);
        }
        public static bool[] DecodeFog(string fog,int length)
        {
            var result=new bool[length];
            try{var bytes=Convert.FromBase64String(fog??"");if(bytes.Length!=(length+7)/8)return result;for(int i=0;i<length;i++)result[i]=(bytes[i>>3]&(1<<(i&7)))!=0;}catch(FormatException){}
            return result;
        }
        public static DarklightAtlasState Decode(string json)
        {
            try
            {
                var s=JsonUtility.FromJson<DarklightAtlasState>(json);
                if(s==null||s.version!=1||s.regions==null||s.regions.Count>256)return null;
                var ids=new HashSet<int>();
                foreach(var r in s.regions)if(r==null||r.id<0||!ids.Add(r.id)||r.width<1||r.height<1||r.width>2048||r.height>2048||r.width*r.height>1048576)return null;
                return s;
            }catch(Exception){return null;}
        }
        public static DarklightAtlasState Load(string path)
        {
            foreach(string p in new[]{path,path+".bak"})try{if(File.Exists(p)){var s=Decode(File.ReadAllText(p));if(s!=null)return s;}}catch(IOException){}
            return new DarklightAtlasState();
        }
        public void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(this));
            if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
        }
    }
    public enum DarklightMapSymbol { Player,Primary,Secondary,Enemy,Boss,Checkpoint,Exit,Interest,Waypoint }
    public sealed class DarklightMapPoint
    {
        public string id,title,description;
        public Vector2 position;
        public DarklightMapSymbol symbol;
        public bool completed,trackable=true;
    }
}
