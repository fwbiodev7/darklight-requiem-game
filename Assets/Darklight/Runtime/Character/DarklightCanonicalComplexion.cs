using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    // Shared Requiem-rest pigment for the inventoried player source textures.
    // Original files/geometry, hair, costumes, blades and protected death stay intact.
    public static class DarklightCanonicalComplexion
    {
        [Serializable] public sealed class Entry { public string texture;public float[] shift,sourceMedian; }
        [Serializable] public sealed class Data { public string reference;public float[] referenceMedian;public Entry[] entries; }
        static Dictionary<string,Vector4> palette;
        static readonly Dictionary<Texture,Vector4> cache=new();
        static readonly MaterialPropertyBlock block=new();
        static readonly int Shift=Shader.PropertyToID("_CanonicalSkinShift");
        public static void ClearCache(){palette=null;cache.Clear();}
        public static Vector4 ForTexture(Texture texture)
        {
            if(texture==null)return Vector4.zero;
            if(cache.TryGetValue(texture,out var value))return value;
            if(palette==null)
            {
                palette=new Dictionary<string,Vector4>(StringComparer.Ordinal);
                var source=Resources.Load<TextAsset>("Darklight/CanonicalComplexion");
                var data=source==null?null:JsonUtility.FromJson<Data>(source.text);
                if(data?.entries!=null)foreach(var entry in data.entries)
                    if(entry.shift?.Length==3)palette[entry.texture]=new Vector4(entry.shift[0],entry.shift[1],entry.shift[2],1);
            }
            palette.TryGetValue(texture.name,out value);cache[texture]=value;return value;
        }
        public static void Apply(SpriteRenderer renderer,bool enabled=true)
        {
            if(renderer==null)return;
            renderer.GetPropertyBlock(block);
            block.SetVector(Shift,enabled&&renderer.sprite!=null?ForTexture(renderer.sprite.texture):Vector4.zero);
            renderer.SetPropertyBlock(block);
        }
    }
}
