using UnityEngine;

namespace Darklight
{
    public sealed class DarklightCharacterView : MonoBehaviour
    {
        public const float WorldVisualScale=1.325f;
        static Vector3 RenderScale=>Vector3.one*WorldVisualScale;
        public Sprite[] idle, walk, slash, actions, motion;
        Sprite[] baseIdle,baseWalk,baseSlash,baseActions,baseMotion;bool rock;
        readonly Sprite[][] rockWalking=new Sprite[3][];
        int walkingStage=-1,walkingSkin=-1;
        Sprite[] AuthoredWalking(int weapon)
        {
            weapon=Mathf.Clamp(weapon,0,2);
            int stage=previewing?previewAge:appearance?.Stage??0;
            int skin=rock?1:0;
            if(walkingStage!=stage||walkingSkin!=skin){System.Array.Clear(rockWalking,0,rockWalking.Length);walkingStage=stage;walkingSkin=skin;}
            if(rockWalking[weapon]==null)
            {
                var frames=Resources.LoadAll<Sprite>("Darklight/Walking/"+(rock?"Rock":"Requiem")+"Walk"+new[]{"Katana","Daggers","Chain"}[weapon]+(stage==0?"":"Stage"+stage));
                System.Array.Sort(frames,(a,b)=>int.Parse(a.name.Substring(a.name.LastIndexOf('_')+1)).CompareTo(int.Parse(b.name.Substring(b.name.LastIndexOf('_')+1))));
                if(frames.Length!=32&&stage>0) { frames=Resources.LoadAll<Sprite>("Darklight/Walking/"+(rock?"Rock":"Requiem")+"Walk"+new[]{"Katana","Daggers","Chain"}[weapon]); System.Array.Sort(frames,(a,b)=>int.Parse(a.name.Substring(a.name.LastIndexOf('_')+1)).CompareTo(int.Parse(b.name.Substring(b.name.LastIndexOf('_')+1)))); }
                rockWalking[weapon]=frames;
            }
            return rockWalking[weapon];
        }
        public void SetSkin(int index,Sprite[] skinIdle,Sprite[] skinMotion,Sprite[] skinActions,Sprite[] skinSlash)
        {
            baseIdle??=idle;baseWalk??=walk;baseSlash??=slash;baseActions??=actions;baseMotion??=motion;
            rock=index==1;idle=rock?skinIdle:baseIdle;motion=rock?skinMotion:baseMotion;actions=rock?skinActions:baseActions;slash=rock?skinSlash:baseSlash;walk=rock?null:baseWalk;
            if(display!=null){display.sprite=idle[0];display.transform.localScale=RenderScale;}
        }
        readonly DarklightAnimationCatalog catalog=new();
        int trailSerial=-1;float nextTrail,airHeight;
        SpriteRenderer display;
        CharacterAppearanceSystem appearance;
        DarklightOriginalVampireView originalForms;
        public bool UsesUserWholeSprite {get;private set;}
        public bool UsesCompleteAgeSprite {get;private set;}
        bool previewing;int previewAge,previewForm;WeaponDefinition previewWeapon;
        public void PresentPreview(CombatModel model,Vector2 movement,int skin,int age,int form,WeaponDefinition weapon)
        {
            var source=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();
            if(baseIdle==null){idle=source.baseIdle??source.idle;walk=source.baseWalk??source.walk;slash=source.baseSlash??source.slash;actions=source.baseActions??source.actions;motion=source.baseMotion??source.motion;}
            var game=DarklightCampaign.Instance;SetSkin(skin,game.rockIdle,game.rockMotion,game.rockActions,game.rockSlash);
            appearance??=gameObject.AddComponent<CharacterAppearanceSystem>();appearance.profile=game.Appearance.profile;appearance.PreviewStage(age);
            previewing=true;previewAge=age;previewForm=form;previewWeapon=weapon;Present(model,movement);previewing=false;
        }
        public static int DirectionIndex(Vector2 direction) => Mathf.RoundToInt(Mathf.Atan2(-direction.x, -direction.y) * Mathf.Rad2Deg / 45f + 8f) % 8;
        void Awake() { display = GetComponentInChildren<SpriteRenderer>();appearance=GetComponent<CharacterAppearanceSystem>(); }
        void LateUpdate()
        {
            if (display == null || display.transform == transform) return;
            Vector3 p = transform.position;
            var camera = DarklightCombatLab.Instance?.WorldCamera;
            float grid = camera == null ? 48f : (camera.targetTexture!=null?camera.targetTexture.height:720f) / (camera.orthographicSize * 2f);
            display.transform.position = new Vector3(Mathf.Round(p.x * grid) / grid, Mathf.Round((p.y+airHeight) * grid) / grid, p.z);
        }
        public void Present(CombatModel model, Vector2 movement)
        {
            int direction = DirectionIndex(model.Facing);

            airHeight=model.State==CombatState.Attack&&model.Attack.kind==StrikeKind.Aerial?Mathf.Sin(Mathf.Clamp01(model.StateTime/model.Attack.Duration)*Mathf.PI)*.35f:0;
            Sprite next = null;
            if ((model.State == CombatState.Attack || model.ActiveThisTick) && slash != null && slash.Length == 32)
            {
                int frame = model.ActiveThisTick ? 1 : model.StateTime < model.Attack.startup ? 0 : model.StateTime < model.Attack.startup + model.Attack.active ? 1 :
                    model.StateTime < model.Attack.startup + model.Attack.active + model.Attack.recovery * .5f ? 2 : 3;
                next = slash[direction * 4 + frame];
            }
            else if (rock && movement.sqrMagnitude > .01f && motion?.Length==24)
                next=motion[direction*3+((int)(model.Clock*12f)%2==0?0:2)];
            else if (movement.sqrMagnitude > .01f && walk != null && walk.Length == 32)
                next = walk[direction * 4 + (int)(model.Clock * 12f) % 4];
            else if (idle != null && idle.Length == 8) next = idle[direction];
            if(actions?.Length==32)
            {
                int action=model.State==CombatState.Dodge?0:model.State==CombatState.Parry?1:model.State==CombatState.Hurt?2:model.State==CombatState.Dead?3:-1;
                if(action>=0)next=actions[direction*4+action];
                if(action==0&&motion?.Length==24)next=rock?motion[direction*3+(model.StateTime<.16f?1:2)]:model.StateTime<.045f?actions[direction*4]:motion[direction*3+(model.StateTime<.16f?1:2)];
            }
            if (next != null)
            {
                display.sprite = next;
                // Importers share an authored standing-height PPU within each animation sheet.
                // Keep that scale when crouching, swinging or changing facing.
                display.transform.localScale=RenderScale;
            }
            var equipped=previewing?previewWeapon:DarklightCampaign.Instance?.Weapons?.Equipped;
            if(equipped!=null&&equipped.id!=DarklightWeaponId.Katana)
            {
                var poses=equipped.Poses(rock?1:0);
                if(poses?.Length==64)
                {
                    int frame=movement.sqrMagnitude>.01f?1+(int)(model.Clock*12)%2:0;
                    if(model.State==CombatState.Attack||model.ActiveThisTick)frame=model.Attack.kind==StrikeKind.Aerial?6:model.Attack.kind==StrikeKind.WeaponSpecial||model.Attack.kind==StrikeKind.Skill?7:model.Attack.kind==StrikeKind.Heavy||model.Attack.kind==StrikeKind.Charged?5:model.ActiveThisTick?4:3;
                    if(model.State==CombatState.Parry)frame=3;if(model.State==CombatState.Dodge)frame=2;
                    if(model.State==CombatState.Hurt||model.State==CombatState.Dead)frame=0;
                    if(DarklightCampaign.Instance.Weapons.Swapping)frame=3;
                    display.sprite=poses[direction*8+frame];display.transform.localScale=RenderScale;
                }
            }
            display.transform.localRotation=model.State==CombatState.Dead&&equipped!=null&&equipped.id!=DarklightWeaponId.Katana?Quaternion.Euler(0,0,-75):Quaternion.identity;
            // True walking is selected after weapon poses so equipment cannot restore attack stances.
            if((rock||(appearance?.Stage??0)>0)&&model.State==CombatState.Neutral&&!model.ActiveThisTick&&movement.sqrMagnitude>.01f&&!(DarklightCampaign.Instance?.Weapons?.Swapping??false))
            {
                var steps=AuthoredWalking(equipped==null?0:(int)equipped.id);
                if(steps.Length==32){display.sprite=steps[direction*4+(int)(model.Clock*10f)%4];display.transform.localScale=RenderScale;}
            }
            // Whole aged bodies replace only their matching authored pose; never paint extra hair or shoulders.
            if(appearance!=null&&model.State==CombatState.Neutral&&!model.ActiveThisTick&&movement.sqrMagnitude<.01f&&!(DarklightCampaign.Instance?.Weapons?.Swapping??false)&&(equipped==null||equipped.id==DarklightWeaponId.Katana))
            {
                var body=appearance.IdleBody(rock?1:0,direction,model.Clock,false);
                if(body!=null)display.sprite=body;
            }
            var exact=catalog.Parry(rock?1:0,previewing?previewAge:appearance?.Stage??0,equipped==null?0:(int)equipped.id,direction,model)??catalog.Dodge(rock?1:0,previewing?previewAge:appearance?.Stage??0,equipped==null?0:(int)equipped.id,direction,model)??catalog.Light(rock?1:0,previewing?previewAge:appearance?.Stage??0,equipped==null?0:(int)equipped.id,direction,model);
            if(exact!=null){display.sprite=exact;display.transform.localScale=RenderScale;}
            var campaign=DarklightCampaign.Instance;
            if(appearance!=null)display.sprite=appearance.ResolveBody(display.sprite);
            var userSprite=DarklightUserSpriteBank.Resolve(rock?1:0,previewing?previewAge:appearance?.Stage??0,equipped==null?0:(int)equipped.id,model,movement);
            UsesUserWholeSprite=userSprite!=null;
            display.flipX=false;
            if(UsesUserWholeSprite)
            {
                display.sprite=userSprite;display.transform.localScale=RenderScale;
                display.flipX=model.Facing.x<-.1f;
                display.transform.localRotation=model.State==CombatState.Dead?Quaternion.Euler(0,0,-75):Quaternion.identity;
            }
            UsesCompleteAgeSprite=false;
            if(!UsesUserWholeSprite)
            {
                var complete=DarklightWholeCharacterBank.Resolve(rock?1:0,previewing?previewAge:appearance?.Stage??0,equipped==null?0:(int)equipped.id,direction,model,movement);
                if(complete!=null){display.sprite=complete;UsesCompleteAgeSprite=true;}
            }
            display.color = model.State == CombatState.Hurt ? new Color(1, .4f, .4f) : model.State == CombatState.Parry ? new Color(.7f, .8f, 1) : Color.white;
            display.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10f);
            appearance?.Present(display,direction,rock?1:0,campaign?.Form??0,model);
            originalForms??=GetComponent<DarklightOriginalVampireView>()??gameObject.AddComponent<DarklightOriginalVampireView>();
            originalForms.Present(display,rock?1:0,appearance?.Stage??0,direction,previewing?previewForm:campaign?.PresentedForm??0,model.Clock,model.State==CombatState.Dead,UsesUserWholeSprite||UsesCompleteAgeSprite,model.State==CombatState.Dodge);
            if(model.State==CombatState.Attack && model.ActiveThisTick && trailSerial!=model.AttackSerial)
            {trailSerial=model.AttackSerial;DarklightCombatFX.Echo(display,.10f);}
            if(model.State==CombatState.Dodge && model.Clock>=nextTrail)
            {nextTrail=model.Clock+.05f;DarklightCombatFX.Echo(display,.12f);}

        }
    }
}

