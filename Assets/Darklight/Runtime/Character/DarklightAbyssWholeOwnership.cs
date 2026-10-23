using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public static partial class DarklightAbyssSpriteBank
    {
        [Serializable] public sealed class WholePage {public string name;public string[] spriteNames;}
        [Serializable] public sealed class WholeManifest {public WholePage[] pages;}
        static readonly Dictionary<string,WholePage> wholePages=new();
        static readonly Dictionary<string,Texture2D> wholeMasks=new();
        static bool wholeAttempted;
        static MaterialPropertyBlock wholeBlock;
        static void ClearWholeOwnership(){wholePages.Clear();wholeMasks.Clear();wholeAttempted=false;}
        public static void ApplyWholeOwnership(SpriteRenderer renderer)
        {
            if(renderer?.sprite==null)return;
            if(!wholeAttempted)
            {
                wholeAttempted=true;var text=Resources.Load<TextAsset>("Darklight/AbyssPrototype/WholeOwnership");
                if(text!=null)foreach(var page in JsonUtility.FromJson<WholeManifest>(text.text).pages)wholePages[page.name]=page;
            }
            var sprite=renderer.sprite;
            if(!wholePages.TryGetValue(sprite.texture.name,out var source))return;
            int index=Array.IndexOf(source.spriteNames,sprite.name);if(index<0)return;
            if(!wholeMasks.TryGetValue(source.name,out var mask))
            {mask=Resources.Load<Texture2D>("Darklight/AbyssPrototype/"+source.name+"_Owner");wholeMasks[source.name]=mask;}
            wholeBlock??=new MaterialPropertyBlock();renderer.GetPropertyBlock(wholeBlock);
            wholeBlock.SetFloat("_WholeSprite",1);wholeBlock.SetFloat("_VampireForm",0);
            wholeBlock.SetFloat("_WholeFrameId",index+1);wholeBlock.SetTexture("_VampireMask",mask);
            renderer.SetPropertyBlock(wholeBlock);
        }
    }
}
