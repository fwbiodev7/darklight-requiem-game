using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    [Serializable] public sealed class OriginalVampireFrame
    {
        public Sprite source;
        public Texture2D mask;
        public Vector2 lip;
        public float headWidth;
        public bool nativeAge;
        public int protectedPixels;
    }
    // Palette data for complete source sprites. Geometry is never erased or assembled.
    public sealed class DarklightOriginalVampireCatalog:ScriptableObject
    {
        public OriginalVampireFrame[] frames;
        public Sprite[] aura;
        Dictionary<Sprite,OriginalVampireFrame> lookup;
        public OriginalVampireFrame Find(Sprite s)
        {
            if(s==null)return null;
            if(lookup==null){lookup=new();foreach(var f in frames)lookup[f.source]=f;}
            return lookup.TryGetValue(s,out var found)?found:null;
        }
    }
}
