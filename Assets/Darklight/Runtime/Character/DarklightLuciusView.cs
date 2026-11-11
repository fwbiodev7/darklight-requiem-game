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
        Vector2 previousPosition;
        bool hasPosition,walking;
        float walkTime;
        // One authored walk cycle covers the ground traversed at this speed.
        const float WalkReferenceSpeed=2.3f;
        public SpriteRenderer Renderer=>art;
        public bool UsesIdleFallback {get;private set;}
        void LateUpdate()
        {
            if(art!=null&&art.transform!=transform)
                art.transform.position=DarklightCharacterView.SnapRenderPosition(transform.position);
        }
        public bool Present(CombatModel model,Vector2 movement=default)
        {
            if(model==null)return false;
            string action=DarklightWeaponRedesignBank.Action(model,movement);
            int facingDirection=DarklightCharacterView.DirectionIndex(model.Facing);
            // Until a native evasive clip exists, a grounded sidestep uses the
            // actual walk cycle, rather than moving a static resting sprite.
            if(model.State==CombatState.Dodge&&movement.sqrMagnitude>.01f&&
                !DarklightLuciusSpriteBank.TryActionPage(appearance,"Dodge",facingDirection,null,out _))action="Walk";
            float time=model.State==CombatState.Neutral?model.Clock:model.StateTime;
            Vector2 position=transform.position;
            if(action=="Walk")
            {
                if(!walking)walkTime=0;
                else if(hasPosition)walkTime+=Vector2.Distance(position,previousPosition)/WalkReferenceSpeed;
                time=walkTime;
            }
            walking=action=="Walk";previousPosition=position;hasPosition=true;
            // Pursuit aim can differ from actual travel (flanking/retreating).
            // Locomotion faces travel; a committed sword stroke keeps its aim.
            int direction=walking?DarklightCharacterView.DirectionIndex(movement):facingDirection;
            string key=action=="Attack"?model.Attack?.animationKey:null;
            if(action=="Attack"&&DarklightLuciusSpriteBank.TryActionPage(appearance,action,direction,key,out var page))
                time=DarklightLuciusSpriteBank.AttackTime(page,model.Attack,
                    model.State==CombatState.Neutral&&model.ActiveThisTick?model.Attack.startup:model.StateTime);
            return PresentPose(action,direction,time,key);
        }
        public bool PresentPose(string action,int direction,float time,string attackKey=null)
        {
            if(!DarklightLuciusSpriteBank.TryPose(appearance,action,direction,time,out var sprite,out bool fallback,attackKey))
            {
                if(art==null)
                {
                    var child=new GameObject("LuciusVisual");child.transform.SetParent(transform,false);
                    art=child.AddComponent<SpriteRenderer>();
                }
                if(art.sprite==null && DarklightCampaign.Instance!=null)
                {
                    var bFrames = DarklightCampaign.Instance.bossFrames;
                    if(bFrames!=null && bFrames.Length>20) art.sprite = bFrames[20];
                }
                UsesIdleFallback=true;
                if(art.sprite!=null)
                {
                    if(DarklightCampaign.Instance?.pixelMaterial is Material material)art.sharedMaterial=material;
                    art.sortingOrder=DarklightSorting.OrderForY(transform.position.y);
                    return true;
                }
                return false;
            }
            if(art==null)
            {
                var child=new GameObject("LuciusVisual");child.transform.SetParent(transform,false);
                art=child.AddComponent<SpriteRenderer>();
            }
            if(DarklightCampaign.Instance?.pixelMaterial is Material pixelMaterial&&art.sharedMaterial!=pixelMaterial)
                art.sharedMaterial=pixelMaterial;
            art.sprite=sprite;art.flipX=art.flipY=false;
            float scale=visualScale>0&&!float.IsNaN(visualScale)&&!float.IsInfinity(visualScale)?visualScale:1.325f;
            art.transform.localScale=Vector3.one*scale;
            art.transform.localRotation=Quaternion.identity;
            art.sortingOrder=DarklightSorting.OrderForY(transform.position.y);art.color=Color.white;
            // Unity native objects cannot be created in MonoBehaviour field
            // initializers. Presentation is explicit and runs on the main thread.
            block??=new MaterialPropertyBlock();art.GetPropertyBlock(block);
            block.SetFloat("_WholeSprite",1);block.SetFloat("_WholeFrameId",0);
            if(appearance==DarklightLuciusAppearance.Arconte)
            {
                block.SetFloat("_DarkForm",1);
                block.SetFloat("_VampireForm",0.5f);
                block.SetVector("_CanonicalSkinShift",new Vector4(0.1f,0.9f,0.3f,1f));
            }
            else
            {
                block.SetFloat("_DarkForm",0);block.SetFloat("_VampireForm",0);
                block.SetVector("_CanonicalSkinShift",Vector4.zero);
            }
            block.SetFloat("_Physique",0);block.SetFloat("_LayerOcclusion",0);
            block.SetFloat("_EraseHead",0);block.SetFloat("_RuinFists",0);
            block.SetTexture("_VampireMask",Texture2D.blackTexture);
            art.SetPropertyBlock(block);UsesIdleFallback=fallback;return true;
        }
    }
}
