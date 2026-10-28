using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    // Bounded, authored contact animations. Entry is reached only after a real hit.
    public sealed class DarklightPixelContactBurst:MonoBehaviour
    {
        const string Resource="Darklight/CombatContacts20261027/ContactBursts";
        [Serializable] sealed class Frame {public string id;public int durationMs;}
        [Serializable] sealed class Sheet {public string status;public bool wholeFigures;public Frame[] frames;}
        sealed class Burst {public SpriteRenderer art;public int clip;public float age,life;public bool active;}
        static DarklightPixelContactBurst instance;
        readonly Burst[] pool=new Burst[16];readonly Sprite[] frames=new Sprite[12];readonly int[] milliseconds=new int[12];
        bool attempted,ready;int cursor;
        public int StartedCount{get;private set;}
        public Sprite LastPresentedSprite{get;private set;}
        public static DarklightPixelContactBurst Current=>instance;
        bool Load()
        {
            if(attempted)return ready;attempted=true;
            var text=Resources.Load<TextAsset>(Resource);if(text==null)return false;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);
            if(sheet?.frames?.Length!=12||!sheet.wholeFigures||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified")return false;
            var named=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(Resource))if(!named.TryAdd(sprite.name,sprite))return false;
            var names=new HashSet<string>();
            for(int i=0;i<12;i++)
            {
                var frame=sheet.frames[i];
                if(frame==null||!names.Add(frame.id)||frame.durationMs<=0||frame.durationMs>200||!named.TryGetValue(frame.id,out frames[i]))return false;
                milliseconds[i]=frame.durationMs;
            }
            return ready=true;
        }
        public static bool TryEmit(Vector3 position,bool heavy)
        {
            var lab=DarklightCombatLab.Instance;var game=DarklightCampaign.Instance;if(lab==null||game==null)return false;
            if(instance==null||!instance.transform.IsChildOf(lab.transform))
            {var obj=new GameObject("ContatosPixelArt");obj.transform.SetParent(lab.transform,false);instance=obj.AddComponent<DarklightPixelContactBurst>();}
            if(!instance.Load())return false;
            int clip=heavy?2:game.Weapons?.Equipped?.id==DarklightWeaponId.Katana?1:0;
            instance.Begin(position+Vector3.up*.65f,clip,game);return true;
        }
        void Begin(Vector3 point,int clip,DarklightCampaign game)
        {
            var burst=pool[cursor];
            if(burst==null)
            {
                var obj=new GameObject("ImpactoPixel");obj.transform.SetParent(transform,false);
                burst=pool[cursor]=new Burst{art=obj.AddComponent<SpriteRenderer>()};
                burst.art.sharedMaterial=game.pixelMaterial;burst.art.sortingOrder=900;
                var block=new MaterialPropertyBlock();block.SetFloat("_WholeSprite",1);block.SetTexture("_VampireMask",Texture2D.blackTexture);burst.art.SetPropertyBlock(block);
            }
            cursor=(cursor+1)%pool.Length;burst.clip=clip;burst.age=0;burst.life=0;burst.active=true;
            for(int i=0;i<4;i++)burst.life+=milliseconds[clip*4+i]/1000f;
            burst.art.sprite=frames[clip*4];burst.art.enabled=true;burst.art.transform.position=point;
            float width=Mathf.Max(.01f,burst.art.sprite.bounds.size.x,burst.art.sprite.bounds.size.y);
            burst.art.transform.localScale=Vector3.one*(clip==2?1.15f:.82f)/width;
            burst.art.color=new Color(1,1,1,game.ReducedFlashes?.4f:1);
            LastPresentedSprite=burst.art.sprite;StartedCount++;
        }
        void Update()
        {
            if(DarklightCombatLab.Instance?.Paused==true)return;
            Advance(Time.unscaledDeltaTime);
        }
        void Advance(float delta)
        {
            foreach(var burst in pool)
            {
                if(burst==null||!burst.active)continue;
                burst.age+=delta;
                if(burst.age>=burst.life){burst.active=false;burst.art.enabled=false;continue;}
                float elapsed=burst.age*1000;int phase=0;
                while(phase<3&&elapsed>=milliseconds[burst.clip*4+phase]){elapsed-=milliseconds[burst.clip*4+phase];phase++;}
                burst.art.sprite=frames[burst.clip*4+phase];LastPresentedSprite=burst.art.sprite;
            }
        }
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
