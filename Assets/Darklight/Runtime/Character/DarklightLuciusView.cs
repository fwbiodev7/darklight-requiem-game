using UnityEngine;

namespace Darklight
{
    // Explicit presentation calls only: adding this component does not create a
    // boss, tick AI, access a campaign, play audio or subscribe to gameplay events.
    public sealed class DarklightLuciusView:MonoBehaviour
    {
        [SerializeField] SpriteRenderer art;
        public DarklightLuciusAppearance appearance=DarklightLuciusAppearance.Normal;
        [Min(.001f)] public float visualScale=1.325f;
        MaterialPropertyBlock block;
        public SpriteRenderer Renderer=>art;
        public bool UsesIdleFallback {get;private set;}
        public bool Present(CombatModel model,Vector2 movement=default)
        {
            if(model==null)return false;
            string action=DarklightWeaponRedesignBank.Action(model,movement);
            float time=model.State==CombatState.Neutral?model.Clock:model.StateTime;
            return PresentPose(action,DarklightCharacterView.DirectionIndex(model.Facing),time,
                action=="Attack"?model.Attack?.animationKey:null);
        }
        public bool PresentPose(string action,int direction,float time,string attackKey=null)
        {
            if(!DarklightLuciusSpriteBank.TryPose(appearance,action,direction,time,out var sprite,out bool fallback,attackKey))return false;
            if(art==null)
            {
                var child=new GameObject("LuciusVisual");child.transform.SetParent(transform,false);
                art=child.AddComponent<SpriteRenderer>();
            }
            art.sprite=sprite;art.flipX=art.flipY=false;
            float scale=visualScale>0&&!float.IsNaN(visualScale)&&!float.IsInfinity(visualScale)?visualScale:1.325f;
            art.transform.localScale=Vector3.one*scale;
            art.sortingOrder=DarklightSorting.OrderForY(transform.position.y);art.color=Color.white;
            // Unity native objects cannot be created in MonoBehaviour field
            // initializers. Presentation is explicit and runs on the main thread.
            block??=new MaterialPropertyBlock();art.GetPropertyBlock(block);
            block.SetFloat("_WholeSprite",1);block.SetFloat("_WholeFrameId",0);block.SetFloat("_VampireForm",0);
            block.SetFloat("_DarkForm",0);block.SetFloat("_Physique",0);block.SetFloat("_LayerOcclusion",0);
            block.SetFloat("_EraseHead",0);block.SetFloat("_RuinFists",0);
            block.SetTexture("_VampireMask",Texture2D.blackTexture);block.SetVector("_CanonicalSkinShift",Vector4.zero);
            art.SetPropertyBlock(block);UsesIdleFallback=fallback;return true;
        }
    }
}
