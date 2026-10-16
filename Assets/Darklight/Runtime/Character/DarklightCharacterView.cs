using UnityEngine;
namespace Darklight
{
    public sealed class DarklightCharacterView:MonoBehaviour
    {
        public const float WorldVisualScale=1.325f;
        static Vector3 RenderScale=>Vector3.one*WorldVisualScale;
        public Sprite[] idle,walk,slash,actions,motion;
        Sprite[] baseIdle,baseWalk,baseSlash,baseActions,baseMotion;
        bool rock,previewing;int previewForm;WeaponDefinition previewWeapon;
        SpriteRenderer display;CharacterAppearanceSystem appearance;DarklightOriginalVampireView originalForms;
        int trailSerial=-1;float nextTrail,airHeight;
        DarklightCombatTrail combatTrail;DarklightAbyssLocomotion abyssLocomotion;
        public bool UsesUserWholeSprite{get;private set;}
        public bool UsesWholeCharacterSprite{get;private set;}
        public bool UsesPoseFallback{get;private set;}
        // Kept for existing diagnostics, with no age selection behind these aliases.
        public bool UsesCompleteAgeSprite=>UsesWholeCharacterSprite;
        public bool UsesAgePoseFallback=>UsesPoseFallback;
        void Awake(){display=GetComponentInChildren<SpriteRenderer>();appearance=GetComponent<CharacterAppearanceSystem>();}
        public void SetSkin(int index,Sprite[] skinIdle,Sprite[] skinMotion,Sprite[] skinActions,Sprite[] skinSlash)
        {
            baseIdle??=idle;baseWalk??=walk;baseSlash??=slash;baseActions??=actions;baseMotion??=motion;
            rock=index==1;idle=rock?skinIdle:baseIdle;motion=rock?skinMotion:baseMotion;actions=rock?skinActions:baseActions;slash=rock?skinSlash:baseSlash;walk=rock?null:baseWalk;
            if(display!=null){display.sprite=TodayCharacterBank.StablePose(index,0,0,0)??(idle?.Length>0?idle[0]:display.sprite);display.transform.localScale=RenderScale;}
        }
        public void PresentPreview(CombatModel model,Vector2 movement,int skin,int ignoredAge,int form,WeaponDefinition weapon)
        {
            var source=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();
            if(baseIdle==null){idle=source.baseIdle??source.idle;walk=source.baseWalk??source.walk;slash=source.baseSlash??source.slash;actions=source.baseActions??source.actions;motion=source.baseMotion??source.motion;}
            var game=DarklightCampaign.Instance;SetSkin(skin,game.rockIdle,game.rockMotion,game.rockActions,game.rockSlash);
            appearance??=GetComponent<CharacterAppearanceSystem>()??gameObject.AddComponent<CharacterAppearanceSystem>();appearance.profile=game.Appearance.profile;
            previewing=true;previewForm=form;previewWeapon=weapon;Present(model,movement);previewing=false;
        }
        public static int DirectionIndex(Vector2 direction)=>Mathf.RoundToInt(Mathf.Atan2(-direction.x,-direction.y)*Mathf.Rad2Deg/45f+8f)%8;
        void LateUpdate()
        {
            if(display==null||display.transform==transform)return;
            var camera=DarklightCombatLab.Instance?.WorldCamera;
            float grid=camera==null?48f:(camera.targetTexture!=null?camera.targetTexture.height:720f)/(camera.orthographicSize*2f);
            var p=transform.position;display.transform.position=new Vector3(Mathf.Round(p.x*grid)/grid,Mathf.Round((p.y+airHeight)*grid)/grid,p.z);
        }
        public void Present(CombatModel model,Vector2 movement)
        {
            if(display==null)return;
            int direction=DirectionIndex(model.Facing),skin=rock?1:0;
            var game=DarklightCampaign.Instance;var equipped=previewing?previewWeapon:game?.Weapons?.Equipped;int weapon=equipped==null?0:(int)equipped.id;
            airHeight=model.State==CombatState.Attack&&model.Attack.kind==StrikeKind.Aerial?Mathf.Sin(Mathf.Clamp01(model.StateTime/model.Attack.Duration)*Mathf.PI)*.35f:0;
            if(!previewing&&game?.AbyssRider?.Active==true)airHeight+=game.AbyssRider.JumpHeight+game.AbyssRider.RunOverLift;
            var selected=!previewing&&skin==1&&game?.AbyssRider?.Active==true?DarklightAbyssSpriteBank.Resolve(game.AbyssRider,model):TodayCharacterBank.Resolve(skin,0,weapon,direction,model,movement);
            bool locomotionSelected=false;
            if(!previewing&&skin==1&&game?.AbyssRider?.Active==true)
            {
                abyssLocomotion??=GetComponent<DarklightAbyssLocomotion>()??gameObject.AddComponent<DarklightAbyssLocomotion>();
                var gait=abyssLocomotion.Present(model,movement,game.PresentedForm);
                if(gait!=null){selected=gait;locomotionSelected=true;}
            }
            UsesPoseFallback=false;UsesUserWholeSprite=false;
            if(selected==null){selected=TodayCharacterBank.StablePose(skin,0,weapon,direction);UsesPoseFallback=selected!=null;}
            if(selected==null){selected=DarklightUserSpriteBank.Resolve(skin,0,weapon,model,movement);UsesUserWholeSprite=selected!=null;}
            if(selected!=null)display.sprite=selected;
            display.transform.localScale=RenderScale;display.flipX=locomotionSelected?abyssLocomotion.Mirror:!previewing&&game?.AbyssRider?.Active==true?DarklightAbyssSpriteBank.Mirror(model):UsesUserWholeSprite&&model.Facing.x<-.1f;
            display.transform.localRotation=model.State==CombatState.Dead&&(UsesPoseFallback||UsesUserWholeSprite)?Quaternion.Euler(0,0,-75):Quaternion.Euler(0,0,!previewing&&game?.AbyssRider?.Active==true?game.AbyssRider.LeanAngle+game.AbyssRider.RunOverPitch:0);
            UsesWholeCharacterSprite=selected!=null&&!UsesUserWholeSprite;
            display.color=model.State==CombatState.Hurt?new Color(1,.4f,.4f):model.State==CombatState.Parry?new Color(.7f,.8f,1):Color.white;
            display.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10f);appearance?.Present(display,direction,skin,game?.Form??0,model);
            originalForms??=GetComponent<DarklightOriginalVampireView>()??gameObject.AddComponent<DarklightOriginalVampireView>();
            originalForms.Present(display,skin,0,direction,previewing?previewForm:game?.PresentedForm??0,model.Clock,model.State==CombatState.Dead,UsesUserWholeSprite||UsesWholeCharacterSprite,model.State==CombatState.Dodge);
            if(combatTrail==null){var obj=new GameObject("RastrosDeCombate");obj.transform.SetParent(transform,false);combatTrail=obj.AddComponent<DarklightCombatTrail>();}
            combatTrail.Present(display,weapon,model,previewing?previewForm:game?.PresentedForm??0);
            if(model.State==CombatState.Attack&&model.ActiveThisTick&&trailSerial!=model.AttackSerial){trailSerial=model.AttackSerial;DarklightCombatFX.Echo(display,.10f);}
            if(model.State==CombatState.Dodge&&model.Clock>=nextTrail){nextTrail=model.Clock+.05f;DarklightCombatFX.Echo(display,.12f);}
        }
    }
}
