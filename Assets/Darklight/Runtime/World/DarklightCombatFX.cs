using UnityEngine;
namespace Darklight
{
    public sealed class DarklightCombatFX : MonoBehaviour
    {
        SpriteRenderer art;Vector2 drift;float age,life;Color tint;
        public static void Impact(Vector3 position,bool heavy)
        {
            var game=DarklightCampaign.Instance;if(game==null||game.ReducedFlashes)return;
            Color color=game.Resonance.Active>=0?DarklightResonance.Colors[game.Resonance.Active]:new Color(1,.65f,.7f);
            for(int i=0;i<(heavy?7:4);i++)
            {
                var fx=Create(game.extras[5],game.pixelMaterial,position+Vector3.up*.65f,.18f,heavy?.20f:.14f,color);
                fx.drift=(Vector2)(Quaternion.Euler(0,0,i*137.5f)*Vector2.up)*(heavy?4:2.5f);
            }
            DarklightCombatLab.Instance.WorldCamera.GetComponent<DarklightLabCamera>().Impulse(heavy?2:1);
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
            var ageHead=source.transform.Find("IdadeOriginal")?.GetComponent<SpriteRenderer>();if(ageHead!=null&&ageHead.enabled)Echo(ageHead,duration);
        }
        static DarklightCombatFX Create(Sprite sprite,Material material,Vector3 position,float height,float duration,Color color)
        {
            var obj=new GameObject("RequiemImpacto");obj.transform.position=position;
            var fx=obj.AddComponent<DarklightCombatFX>();fx.art=obj.AddComponent<SpriteRenderer>();fx.art.sprite=sprite;fx.art.sharedMaterial=material;fx.art.sortingOrder=160;
            fx.tint=color;fx.art.color=color;fx.life=duration;
            var block=new MaterialPropertyBlock();block.SetFloat("_ResonanceTint",1);fx.art.SetPropertyBlock(block);
            if(height>0)obj.transform.localScale=Vector3.one*height/sprite.bounds.size.y;return fx;
        }
        void Update()
        {
            float dt=DarklightCombatLab.Instance?.CombatDelta??Time.deltaTime;if(dt<=0)return;
            age+=dt;transform.position+=(Vector3)(drift*dt);art.color=new Color(tint.r,tint.g,tint.b,tint.a*(1-age/life));
            if(age>=life)Destroy(gameObject);
        }
    }
}
