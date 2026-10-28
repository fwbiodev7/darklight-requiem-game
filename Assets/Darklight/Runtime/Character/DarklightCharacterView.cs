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
        Vector2 contactOffset;float contactUntil;
        public int ConfirmedContactPresentations{get;private set;}
        public void ConfirmContactPose(Vector2 facing)
        {
            ConfirmedContactPresentations++;
            if(DarklightCampaign.Instance?.ReducedFlashes==true){contactUntil=0;return;}
            contactOffset=facing.normalized*.055f;contactUntil=Time.unscaledTime+.14f;
        }
        DarklightCombatTrail combatTrail;DarklightAbyssLocomotion abyssLocomotion;
        readonly DarklightLivingPresentation living=new();
        HealthSystem livingHealth;
        public bool UsesUserWholeSprite{get;private set;}
        public bool UsesWholeCharacterSprite{get;private set;}
        public bool UsesWeaponRedesignSprite{get;private set;}
        public bool UsesPoseFallback{get;private set;}
        public bool UsesMotorcyclePoseFallback{get;private set;}
        public static Bounds FallbackBodyBounds=>new Bounds(Vector3.up*1.315f,new Vector3(1.5f,2.63f,0));
        // Effects use normal frontal katana idle for this skin, independently
        // of the displayed attack, direction, weapon or transformation.
        public Bounds ReferenceBodyBounds
        {
            get
            {
                var neutral=rock?TodayCharacterBank.StablePose(1,0,0,0):DarklightWeaponRedesignBank.CurrentIdle(0,0);
                return neutral!=null&&neutral.bounds.size.y>0?neutral.bounds:FallbackBodyBounds;
            }
        }
        // Kept for existing diagnostics, with no age selection behind these aliases.
        public bool UsesCompleteAgeSprite=>UsesWholeCharacterSprite;
        public bool UsesAgePoseFallback=>UsesPoseFallback;
        void Awake(){display=GetComponentInChildren<SpriteRenderer>();appearance=GetComponent<CharacterAppearanceSystem>();}
        public void SetSkin(int index,Sprite[] skinIdle,Sprite[] skinMotion,Sprite[] skinActions,Sprite[] skinSlash)
        {
            baseIdle??=idle;baseWalk??=walk;baseSlash??=slash;baseActions??=actions;baseMotion??=motion;
            rock=index==1;idle=rock?skinIdle:baseIdle;motion=rock?skinMotion:baseMotion;actions=rock?skinActions:baseActions;slash=rock?skinSlash:baseSlash;walk=rock?null:baseWalk;
            if(display!=null){display.sprite=index==0?DarklightWeaponRedesignBank.CurrentIdle(0,0):TodayCharacterBank.StablePose(index,0,0,0)??(idle?.Length>0?idle[0]:display.sprite);display.transform.localScale=RenderScale;}
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
            var p=transform.position;
            if(DarklightCombatLab.Instance?.Paused!=true&&contactUntil>Time.unscaledTime)
            {float t=(contactUntil-Time.unscaledTime)/.14f;p+=(Vector3)contactOffset*(t*t);}
            display.transform.position=new Vector3(Mathf.Round(p.x*grid)/grid,Mathf.Round((p.y+airHeight)*grid)/grid,p.z);
        }
        public void Present(CombatModel model,Vector2 movement)
        {
            if(display==null)return;
            int direction=DirectionIndex(model.Facing),skin=rock?1:0;
            var game=DarklightCampaign.Instance;var equipped=previewing?previewWeapon:game?.Weapons?.Equipped;int weapon=equipped==null?0:(int)equipped.id;
            bool ruinFists=weapon==2&&DarklightRuinFistsBank.Ready;
            airHeight=!ruinFists&&model.State==CombatState.Attack&&model.Attack.kind==StrikeKind.Aerial?Mathf.Sin(Mathf.Clamp01(model.StateTime/model.Attack.Duration)*Mathf.PI)*.35f:0;
            if(!previewing&&game?.AbyssRider?.Active==true)airHeight+=game.AbyssRider.JumpHeight+game.AbyssRider.RunOverLift;
            if(!previewing&&GetComponent<DarklightFighter>()?.AirCombo is {Active:true} air)airHeight=air.Height;
            UsesMotorcyclePoseFallback=false;
            bool riding=!previewing&&game?.AbyssRider?.Active==true;
            bool currentOnly=skin==0&&(weapon==0||weapon==2||weapon==3);
            bool riderFallback=true;
            var selected=riding?DarklightMotorcycleRedesignBank.ResolveRequiem(game.AbyssRider,model,game.PresentedForm,out riderFallback,movement):
                weapon==3&&currentOnly?DarklightMotorcycleRedesignBank.DualSawsPose(direction,model)??DarklightMotorcycleRedesignBank.Riding(0,direction,0):currentOnly?null:TodayCharacterBank.Resolve(skin,0,weapon,direction,model,movement);
            if(riding)UsesMotorcyclePoseFallback=riderFallback;
            if(!previewing)
            {
                livingHealth??=GetComponent<HealthSystem>();
                living.Observe(model.Clock,model.State,movement,livingHealth!=null?livingHealth.HealthPercent:1,game?.Form??0,
                    game?.PresentationThreat??false,game?.Transforming==true);
                if(!currentOnly&&model.State==CombatState.Neutral && game?.Transforming!=true && game?.AbyssRider?.Active!=true)
                    selected=TodayCharacterBank.LivingPose(skin,weapon,direction,living)??selected;
            }
            bool locomotionSelected=false;
            if(!previewing&&skin==1&&game?.AbyssRider?.Active==true)
            {
                abyssLocomotion??=GetComponent<DarklightAbyssLocomotion>()??gameObject.AddComponent<DarklightAbyssLocomotion>();
                var gait=abyssLocomotion.Present(model,movement,game.PresentedForm);
                if(gait!=null){selected=gait;locomotionSelected=true;}
            }
            UsesPoseFallback=false;UsesUserWholeSprite=false;
            if(selected==null&&!currentOnly){selected=TodayCharacterBank.StablePose(skin,0,weapon,direction);UsesPoseFallback=selected!=null;}
            if(selected==null&&!currentOnly){selected=DarklightUserSpriteBank.Resolve(skin,0,weapon,model,movement);UsesUserWholeSprite=selected!=null;}
            UsesWeaponRedesignSprite=DarklightWeaponRedesignBank.TryResolve(skin,weapon,previewing?previewForm:game?.PresentedForm??0,
                direction,model,movement,selected,out var redesigned,previewing?null:living.Clip,
                previewing?-1:living.Clip=="Idle"?living.BreathClock:living.ClipTime);
            if(UsesWeaponRedesignSprite){selected=redesigned;UsesPoseFallback=UsesUserWholeSprite=false;}
            if(selected!=null||currentOnly)display.sprite=selected;
            display.transform.localScale=RenderScale;display.flipX=!UsesWeaponRedesignSprite&&!DarklightMotorcycleRedesignBank.IsAuthored(selected)&&(locomotionSelected?abyssLocomotion.Mirror:!previewing&&game?.AbyssRider?.Active==true?DarklightAbyssSpriteBank.Mirror(model):UsesUserWholeSprite&&model.Facing.x<-.1f);
            display.transform.localRotation=model.State==CombatState.Dead&&(UsesPoseFallback||UsesUserWholeSprite)?Quaternion.Euler(0,0,-75):Quaternion.Euler(0,0,!previewing&&game?.AbyssRider?.MountedPresentation==true?game.AbyssRider.LeanAngle+game.AbyssRider.RunOverPitch:0);
            UsesWholeCharacterSprite=selected!=null&&!UsesUserWholeSprite;
            display.color=model.State==CombatState.Hurt?new Color(1,.4f,.4f):model.State==CombatState.Parry?new Color(.7f,.8f,1):Color.white;
            display.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10f);appearance?.Present(display,direction,skin,game?.Form??0,model);
            originalForms??=GetComponent<DarklightOriginalVampireView>()??gameObject.AddComponent<DarklightOriginalVampireView>();
            originalForms.Present(display,skin,0,direction,previewing?previewForm:game?.PresentedForm??0,model.Clock,model.State==CombatState.Dead,UsesUserWholeSprite||UsesWholeCharacterSprite,model.State==CombatState.Dodge,!previewing);
            DarklightCanonicalComplexion.Apply(display,model.State!=CombatState.Dead);
            if(model.State!=CombatState.Dead)DarklightAbyssSpriteBank.ApplyWholeOwnership(display);
            DarklightWeaponRedesignBank.ApplyAuthoredOwnership(display);
            DarklightMotorcycleRedesignBank.ApplyOwnership(display);
            if(combatTrail==null){var obj=new GameObject("RastrosDeCombate");obj.transform.SetParent(transform,false);combatTrail=obj.AddComponent<DarklightCombatTrail>();}
            combatTrail.Present(display,weapon,model,previewing?previewForm:game?.PresentedForm??0);
            if(model.State==CombatState.Attack&&model.ActiveThisTick&&trailSerial!=model.AttackSerial){trailSerial=model.AttackSerial;DarklightCombatFX.Echo(display,.10f);}
            if(model.State==CombatState.Dodge&&model.Clock>=nextTrail){nextTrail=model.Clock+.05f;DarklightCombatFX.Echo(display,.12f);}
        }
    }
}
