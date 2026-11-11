using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public enum DarklightActionParticleKind { Dodge,Defense,Damage,Death }
    // Separate, bounded one-shot effects; character textures and palettes stay intact.
    public sealed class DarklightActionParticles:MonoBehaviour
    {
        public const string Resource="Darklight/ActionParticles20261101/ActionParticles";
        public const string WeaponResource="Darklight/FastWeaponParticles20261101/FastWeaponDodge";
        [Serializable] sealed class Frame { public string id; public int durationMs; }
        [Serializable] sealed class Sheet { public string status; public Frame[] frames; }
        sealed class Burst { public SpriteRenderer art; public int clip; public float age,life; public bool active,weapon; }
        sealed class Observation { public CombatModel model; public CombatState state; public float time; }
        static DarklightActionParticles instance;
        readonly Burst[] pool=new Burst[16];
        readonly Sprite[] frames=new Sprite[12];readonly int[] durations=new int[12],counts=new int[4];
        readonly Sprite[] weaponFrames=new Sprite[9];readonly int[] weaponDurations=new int[9];
        readonly Dictionary<Component,Observation> observers=new();
        bool attempted,ready,weaponAttempted,weaponReady;int cursor;
        public static DarklightActionParticles Current=>instance;
        public int Capacity=>pool.Length;
        public int ActiveCount { get { int n=0;foreach(var b in pool)if(b!=null&&b.active)n++;return n; } }
        public int Count(DarklightActionParticleKind kind)=>counts[(int)kind];
        public int MotorcycleGuardBursts {get;private set;}
        public Vector3 LastMotorcycleGuardPoint {get;private set;}
        public Sprite LastPresentedSprite {get;private set;}
        public float LastOpacity {get;private set;}
        public float LastDiameter {get;private set;}
        public float LastLifetime {get;private set;}
        static DarklightActionParticles Get()
        {
            if(instance!=null)return instance;
            var lab=DarklightCombatLab.Instance;if(lab==null)return null;
            var obj=new GameObject("ParticulasAcao");obj.transform.SetParent(lab.transform,false);
            return instance=obj.AddComponent<DarklightActionParticles>();
        }
        bool Load()
        {
            if(attempted)return ready;attempted=true;
            var text=Resources.Load<TextAsset>(Resource);if(text==null)return false;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);
            if(sheet?.frames?.Length!=12||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified")return false;
            var named=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(Resource))if(!named.TryAdd(sprite.name,sprite))return false;
            for(int i=0;i<frames.Length;i++)
            {
                if(sheet.frames[i].durationMs<=0||!named.TryGetValue(sheet.frames[i].id,out frames[i]))return false;
                durations[i]=sheet.frames[i].durationMs;
            }
            return ready=true;
        }
        bool LoadWeapons()
        {
            if(weaponAttempted)return weaponReady;weaponAttempted=true;
            var text=Resources.Load<TextAsset>(WeaponResource);if(text==null)return false;
            var sheet=JsonUtility.FromJson<Sheet>(text.text);
            if(sheet?.frames?.Length!=9||sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified")return false;
            var named=new Dictionary<string,Sprite>(StringComparer.Ordinal);
            foreach(var sprite in Resources.LoadAll<Sprite>(WeaponResource))if(!named.TryAdd(sprite.name,sprite))return false;
            for(int i=0;i<9;i++)
            {
                if(sheet.frames[i].durationMs<=0||!named.TryGetValue(sheet.frames[i].id,out weaponFrames[i]))return false;
                weaponDurations[i]=sheet.frames[i].durationMs;
            }
            return weaponReady=true;
        }
        public static void Observe(Component owner,CombatModel model)
            =>Observe(owner,model,null,-1);
        public static void Observe(Component owner,CombatModel model,SpriteRenderer art,int weapon)
        {
            if(owner==null||model==null)return;
            var fx=Get();if(fx==null)return;
            if(!fx.observers.TryGetValue(owner,out var previous))fx.observers[owner]=previous=new Observation();
            bool entered=previous.model!=model||previous.state!=model.State||model.StateTime<previous.time;
            previous.model=model;previous.state=model.State;previous.time=model.StateTime;
            if(!entered)return;
            DarklightActionParticleKind kind;
            switch(model.State)
            {
                case CombatState.Dodge:kind=DarklightActionParticleKind.Dodge;break;
                case CombatState.Parry:kind=DarklightActionParticleKind.Defense;break;
                case CombatState.Hurt:kind=DarklightActionParticleKind.Damage;break;
                case CombatState.Dead:kind=DarklightActionParticleKind.Death;break;
                default:return;
            }
            if(kind==DarklightActionParticleKind.Defense&&weapon==3&&art!=null&&
                DarklightWeaponRedesignBank.TryMotorcycleGuardContact(DarklightCampaign.Instance?.PresentedForm??0,
                    DarklightCharacterView.DirectionIndex(model.Facing),art.sprite,out var contact,out bool visible))
            {
                fx.LastMotorcycleGuardPoint=art.transform.TransformPoint(new Vector3(contact.x,contact.y,0));
                // Count the defense entry even when its contact is hidden behind
                // the torso. The generic defense sprite would obscure this art.
                fx.counts[(int)kind]++;
                if(visible)
                {
                    fx.MotorcycleGuardBursts++;
                    DarklightCombatImpacts.EmitMotorcycle(fx.LastMotorcycleGuardPoint,model.Facing,DarklightMotorcycleImpact.Guard);
                }
                return;
            }
            float height=kind==DarklightActionParticleKind.Dodge?.12f:kind==DarklightActionParticleKind.Death?.4f:.85f;
            Emit(kind,owner.transform.position+Vector3.up*height,model.Facing,owner.transform.position.y);
        }
        public static void Emit(DarklightActionParticleKind kind,Vector3 point,Vector2 facing=default,float? groundY=null)
        {
            int clip=(int)kind;if(clip<0||clip>3)return;
            var fx=Get();if(fx==null||!fx.Load())return;
            bool weapon=kind==DarklightActionParticleKind.Dodge&&fx.LoadWeapons();
            if(weapon)
            {
                int id=(int)(DarklightCampaign.Instance?.Weapons?.Equipped?.id??DarklightWeaponId.Katana);
                clip=id==2?1:id==3?2:0;
            }
            var selected=weapon?fx.weaponFrames:fx.frames;
            var timing=weapon?fx.weaponDurations:fx.durations;
            var b=fx.pool[fx.cursor];
            if(b==null)
            {
                var obj=new GameObject("ParticulaAcao");obj.transform.SetParent(fx.transform,false);
                b=fx.pool[fx.cursor]=new Burst {art=obj.AddComponent<SpriteRenderer>()};
                if(DarklightCampaign.Instance?.pixelMaterial!=null)b.art.sharedMaterial=DarklightCampaign.Instance.pixelMaterial;
                var block=new MaterialPropertyBlock();block.SetFloat("_AlphaClip",.05f);block.SetFloat("_WholeSprite",1);block.SetTexture("_VampireMask",Texture2D.blackTexture);b.art.SetPropertyBlock(block);
            }
            fx.cursor=(fx.cursor+1)%fx.pool.Length;b.clip=clip;b.age=0;b.life=0;b.active=true;b.weapon=weapon;
            for(int i=0;i<3;i++)b.life+=timing[clip*3+i]/1000f;
            b.art.sprite=selected[clip*3];b.art.transform.position=point;b.art.enabled=true;
            // Refresh pooled bursts at their ground contact, independently of effect height.
            b.art.sortingOrder=DarklightSorting.OrderForY(groundY??point.y)+1;
            bool reduced=DarklightCampaign.Instance?.ReducedFlashes==true;
            float diameter=kind==DarklightActionParticleKind.Death?1.5f:kind==DarklightActionParticleKind.Dodge?1.15f:1.05f;
            if(reduced)diameter*=.55f;
            b.art.transform.localScale=Vector3.one*diameter/Mathf.Max(.001f,b.art.sprite.bounds.size.x);
            b.art.transform.localRotation=kind==DarklightActionParticleKind.Dodge&&facing.sqrMagnitude>.01f?
                Quaternion.Euler(0,0,Mathf.Atan2(facing.y,facing.x)*Mathf.Rad2Deg):Quaternion.identity;
            fx.LastOpacity=reduced?.33f:.85f;fx.LastDiameter=diameter;
            b.art.color=new Color(1,1,1,fx.LastOpacity);fx.LastPresentedSprite=b.art.sprite;fx.LastLifetime=b.life;fx.counts[(int)kind]++;
            if(kind==DarklightActionParticleKind.Dodge)
            {
                int wId=(int)(DarklightCampaign.Instance?.Weapons?.Equipped?.id??DarklightWeaponId.Katana);
                DarklightCombatParticlesV2.TryEmitDodge(wId,point,facing,groundY);
            }
        }
        void Update()
        {
            var lab=DarklightCombatLab.Instance;
            if(lab?.Paused!=true||lab.Player?.Model?.State==CombatState.Dead)Advance(Time.unscaledDeltaTime);
        }
        void Advance(float delta)
        {
            if(delta<=0||float.IsNaN(delta)||float.IsInfinity(delta))return;
            foreach(var b in pool)
            {
                if(b==null||!b.active)continue;b.age+=delta;
                if(b.age>=b.life){b.active=false;b.art.enabled=false;continue;}
                float elapsed=b.age*1000;int phase=0;
                var timing=b.weapon?weaponDurations:durations;var selected=b.weapon?weaponFrames:frames;
                while(phase<2&&elapsed>=timing[b.clip*3+phase]){elapsed-=timing[b.clip*3+phase];phase++;}
                b.art.sprite=selected[b.clip*3+phase];LastPresentedSprite=b.art.sprite;
            }
        }
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
