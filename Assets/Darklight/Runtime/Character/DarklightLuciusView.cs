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
        // Presentation offset only; the Rigidbody and ground-plane sorting stay put.
        public float AirHeight {get;set;}
        public bool UsesIdleFallback {get;private set;}
        public bool UsesDirectionalFallback {get;private set;}
        void LateUpdate()
        {
            if(art!=null&&art.transform!=transform)
                art.transform.position=DarklightCharacterView.SnapRenderPosition(transform.position+Vector3.up*Mathf.Max(0,AirHeight));
        }
        public bool Present(CombatModel model,Vector2 movement=default)
        {
            if(model==null)return false;
            string action=DarklightWeaponRedesignBank.Action(model,movement);
            int direction=DarklightCharacterView.DirectionIndex(model.Facing);
            float time=model.State==CombatState.Neutral?model.Clock:model.StateTime;
            if(action=="Dodge")
            {
                int dodgeDirection=DarklightCharacterView.DirectionIndex(model.DodgeDirection);
                if(DarklightLuciusSpriteBank.TryActionPage(appearance,action,dodgeDirection,null,out var dodge))
                {
                    direction=dodgeDirection;
                    time=DarklightLuciusSpriteBank.FitStateTime(dodge,model.StateTime,CombatModel.DodgeDuration);
                }
                else if(movement.sqrMagnitude>.01f)action="Walk";
            }
            Vector2 position=transform.position;
            if(action=="Walk")
            {
                // Pursuit aim can differ from travel. Committed sword strokes keep their aim.
                direction=DarklightCharacterView.DirectionIndex(movement);
                if(!walking)walkTime=0;
                else if(hasPosition)walkTime+=Vector2.Distance(position,previousPosition)/WalkReferenceSpeed;
                time=walkTime;
            }
            walking=action=="Walk";previousPosition=position;hasPosition=true;
            string key=action=="Attack"?model.Attack?.animationKey:null;
            if(action=="Attack"&&DarklightLuciusSpriteBank.TryActionPage(appearance,action,direction,key,out var page))
                time=DarklightLuciusSpriteBank.AttackTime(page,model.Attack,
                    model.State==CombatState.Neutral&&model.ActiveThisTick?model.Attack.startup:model.StateTime);
            if(action=="Hurt"&&DarklightLuciusSpriteBank.TryActionPage(appearance,action,direction,null,out var hurt))
                time=DarklightLuciusSpriteBank.FitStateTime(hurt,model.StateTime,model.HurtDuration);
            return PresentPose(action,direction,time,key);
        }
        public bool PresentPose(string action,int direction,float time,string attackKey=null,bool allowOfficialIdle=true)
        {
            Sprite sprite=null;bool fallback=false;
            UsesDirectionalFallback=DarklightLuciusSpriteBank.TryActionPage(appearance,action,direction,attackKey,out var selected)&&selected.directionalFallback;
            bool officialIdle=allowOfficialIdle&&appearance==DarklightLuciusAppearance.Arconte&&action=="Idle"&&direction==0&&
                DarklightOfficialTransitionBank.TryPose(3,1,out sprite);
            if(!officialIdle&&!DarklightLuciusSpriteBank.TryPose(appearance,action,direction,time,out sprite,out fallback,attackKey))
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
            // Both appearances are complete authored figures with their own colors.
            block.SetFloat("_DarkForm",0);block.SetFloat("_VampireForm",0);
            block.SetFloat("_AlphaClip",.1f);
            block.SetVector("_CanonicalSkinShift",Vector4.zero);
            block.SetFloat("_Physique",0);block.SetFloat("_LayerOcclusion",0);
            block.SetFloat("_EraseHead",0);block.SetFloat("_RuinFists",0);
            block.SetTexture("_VampireMask",Texture2D.blackTexture);
            art.SetPropertyBlock(block);
            if(officialIdle)DarklightOfficialTransitionBank.ApplyOwnership(art);
            UsesIdleFallback=fallback;return true;
        }
    }
}
