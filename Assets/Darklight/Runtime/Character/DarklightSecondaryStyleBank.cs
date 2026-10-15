using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    [Serializable] public sealed class SecondaryStylePage
    {
        public string name,resourcePath,paletteResourcePath;
        public string[] spriteNames;
        [NonSerialized] public Sprite[] frames;
        [NonSerialized] public Texture2D mask;
    }
    [Serializable] public sealed class SecondaryStyleCatalog{public SecondaryStylePage[] pages;}
    public static class DarklightSecondaryStyleBank
    {
        static SecondaryStyleCatalog catalog;
        static bool attempted;
        static SecondaryStylePage Page(string name)
        {
            if(!attempted){attempted=true;var text=Resources.Load<TextAsset>("Darklight/SecondaryStyleCatalog");if(text!=null)catalog=JsonUtility.FromJson<SecondaryStyleCatalog>(text.text);}
            if(catalog?.pages!=null)foreach(var p in catalog.pages)if(p.name==name)return p;return null;
        }
        public static Sprite[] Load(string name)
        {
            var page=Page(name);if(page==null)return Array.Empty<Sprite>();
            if(page.frames==null)
            {
                var byName=new Dictionary<string,Sprite>();foreach(var s in Resources.LoadAll<Sprite>(page.resourcePath))byName[s.name]=s;
                var ordered=new Sprite[page.spriteNames.Length];for(int i=0;i<ordered.Length;i++)if(!byName.TryGetValue(page.spriteNames[i],out ordered[i]))throw new InvalidOperationException("Incomplete secondary art: "+name);
                page.frames=ordered;
            }
            return page.frames;
        }
        public static void Attach(SpriteRenderer art){if(art!=null&&art.GetComponent<DarklightSecondaryStyleView>()==null)art.gameObject.AddComponent<DarklightSecondaryStyleView>();}
        public static void Apply(SpriteRenderer art)
        {
            if(art.sprite==null)return;var page=Page(art.sprite.texture.name);if(page==null)return;
            int index=Array.IndexOf(page.spriteNames,art.sprite.name);if(index<0)return;
            page.mask??=Resources.Load<Texture2D>(page.paletteResourcePath);
            var block=new MaterialPropertyBlock();art.GetPropertyBlock(block);block.SetTexture("_VampireMask",page.mask);block.SetFloat("_WholeFrameId",index+1);art.SetPropertyBlock(block);
        }
        public static void Clear(){catalog=null;attempted=false;}
    }
    [DefaultExecutionOrder(1500)]
    public sealed class DarklightSecondaryStyleView:MonoBehaviour
    {
        SpriteRenderer art;Sprite previous;
        void Awake(){art=GetComponent<SpriteRenderer>();}
        void LateUpdate(){if(art!=null&&art.sprite!=previous){previous=art.sprite;DarklightSecondaryStyleBank.Apply(art);}}
    }
}
