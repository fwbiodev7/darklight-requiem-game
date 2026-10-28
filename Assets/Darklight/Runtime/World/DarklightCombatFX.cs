using UnityEngine;
namespace Darklight
{
    public sealed class DarklightCombatFX : MonoBehaviour
    {
        static readonly System.Collections.Generic.List<DarklightCombatFX> pool=new();
        SpriteRenderer art;Vector2 drift;float age,life;Color tint;MaterialPropertyBlock block;
        public static void Impact(Vector3 position,bool heavy)
        {
            var game=DarklightCampaign.Instance;if(game==null)return;
            DarklightPixelContactBurst.TryEmit(position,heavy);
            DarklightCombatImpacts.Emit(position,heavy);
            DarklightCombatLab.Instance.WorldCamera.GetComponent<DarklightLabCamera>().Impulse(heavy?2:1);
        }
        public static void MotorcycleImpact(Vector3 position,Vector2 direction,DarklightMotorcycleImpact kind)
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;if(game==null||lab==null)return;
            DarklightCombatImpacts.EmitMotorcycle(position,direction,kind);
            lab.WorldCamera.GetComponent<DarklightLabCamera>()?.Impulse(game.ReducedFlashes?1:2);
        }
        public static void Transformation(Vector3 position,int form,bool release)
        {
            var game=DarklightCampaign.Instance;if(game==null||game.ReducedFlashes||game.extras?.Length<=5)return;
            Color color=form==3?new Color(.95f,.96f,1):form==2?new Color(.65f,.22f,1):new Color(.95f,.12f,.24f);
            for(int i=0;i<12;i++)
            {
                Vector2 radial=Quaternion.Euler(0,0,i*30)*Vector2.up;
                var fx=Create(game.extras[5],game.pixelMaterial,position+Vector3.up*.65f+(Vector3)(radial*(release?.2f:.8f)),.12f,release?.38f:.3f,color);
                fx.drift=radial*(release?2.6f:-2.2f);
            }
        }
        public static void Echo(SpriteRenderer source,float duration)
        {
            if(source==null||DarklightCampaign.Instance?.ReducedFlashes==true)return;
            var game=DarklightCampaign.Instance;Color tint=game!=null&&game.Resonance.Active>=0?DarklightResonance.Colors[game.Resonance.Active]:new Color(.8f,.2f,.35f);tint.a=.28f;
            var fx=Create(source.sprite,source.sharedMaterial,source.transform.position,0,duration,tint);
            fx.transform.localScale=source.transform.lossyScale;fx.art.flipX=source.flipX;fx.art.sortingOrder=source.sortingOrder-1;
            var selectors=new MaterialPropertyBlock();source.GetPropertyBlock(selectors);selectors.SetFloat("_ResonanceTint",1);fx.art.SetPropertyBlock(selectors);
        }
        static DarklightCombatFX Create(Sprite sprite,Material material,Vector3 position,float height,float duration,Color color)
        {
            DarklightCombatFX fx=null;int active=0;
            for(int i=pool.Count-1;i>=0;i--){if(pool[i]==null){pool.RemoveAt(i);continue;}if(!pool[i].gameObject.activeSelf)fx=pool[i];else active++;}
            int limit=new[]{12,20,32,48}[DarklightRemaster.QualityLevel];
            if(active>=limit){fx=null;foreach(var item in pool)if(item.gameObject.activeSelf&&(fx==null||item.age/item.life>fx.age/fx.life))fx=item;}
            if(fx==null){var created=new GameObject("EcoDeCombate");if(DarklightCombatLab.Instance!=null)created.transform.SetParent(DarklightCombatLab.Instance.transform,false);fx=created.AddComponent<DarklightCombatFX>();fx.art=created.AddComponent<SpriteRenderer>();fx.block=new MaterialPropertyBlock();pool.Add(fx);}
            var obj=fx.gameObject;obj.transform.position=position;obj.transform.rotation=Quaternion.identity;obj.transform.localScale=Vector3.one;fx.age=0;fx.drift=Vector2.zero;fx.art.flipX=false;fx.art.sprite=sprite;fx.art.sharedMaterial=material;fx.art.sortingOrder=160;
            fx.tint=color;fx.art.color=color;fx.life=duration;
            fx.block.Clear();fx.block.SetFloat("_ResonanceTint",1);fx.art.SetPropertyBlock(fx.block);
            if(height>0)obj.transform.localScale=Vector3.one*height/sprite.bounds.size.y;obj.SetActive(true);return fx;
        }
        void Update()
        {
            float dt=DarklightCombatLab.Instance?.CombatDelta??Time.deltaTime;if(dt<=0)return;
            age+=dt;transform.position+=(Vector3)(drift*dt);art.color=new Color(tint.r,tint.g,tint.b,tint.a*(1-age/life));
            if(age>=life)gameObject.SetActive(false);
        }
    }
}
