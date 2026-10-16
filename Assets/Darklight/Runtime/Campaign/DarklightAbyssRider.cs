using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightAbyssRider:MonoBehaviour
    {
        public readonly DarklightGearModel Gear=new();
        public const float DashCost=18,MaximumDashDistance=2.4f;
        public bool Active=>game!=null&&game.SelectedSkin==1&&game.Weapons?.Equipped?.id==DarklightWeaponId.AbyssRider;
        public bool Dashing=>Active&&fighter.Model.State==CombatState.Attack&&(fighter.Model.Attack==dash||fighter.Model.Attack==drift||fighter.Model.Attack==finisher);
        public bool Sustaining=>Active&&fighter.Model.State==CombatState.Attack&&fighter.Model.Attack==sustain;
        public bool CanFinish=>Active&&Gear.Level==3&&fighter.Model.WeaponSpecialRemaining<=0&&game.Blood.Amount>=FinisherCost&&FindFinisherTarget()!=null;
        public bool Slamming=>Active&&fighter.Model.State==CombatState.Attack&&fighter.Model.Attack==slam;
        public bool Finishing=>Dashing&&fighter.Model.Attack==finisher;
        public bool SlamFromMounted{get;private set;}
        public bool Drifting=>Dashing&&fighter.Model.Attack==drift;
        public bool BlocksSwap=>(Dashing||Slamming||Sustaining)&&fighter.Model.StateTime<fighter.Model.Attack.startup+fighter.Model.Attack.active+(Sustaining?.2f:.34f);
        public float JumpHeight=>Dashing?Mathf.Sin(Mathf.Clamp01((fighter.Model.StateTime-fighter.Model.Attack.startup-fighter.Model.Attack.active)/.34f)*Mathf.PI)*.28f:0;
        public bool DashBlocked{get;private set;}
        public float DashDistance{get;private set;}
        public float Speed=>Active&&!Dashing?Gear.Speed:1;
        public float AttackClockSpeed(float campaignSpeed)=>Dashing||Slamming||Sustaining?1:campaignSpeed*Speed;
        public const float SustainEntryCost=6,SustainPulseCost=2,SlamCost=22,FinisherCost=35;
        public const float DriftCost=10,MaximumDriftDistance=1.25f;
        public float DriftDistance{get;private set;}
        public float LeanAngle=>Drifting?Mathf.Sin(Mathf.Clamp01((fighter.Model.StateTime-drift.startup)/drift.active)*Mathf.PI)*-7*driftSide:0;
        public Vector2 MotionHeading{get;private set;}
        public float Damage=>Active?Gear.Damage:1;
        public float Posture=>Active?Gear.Posture:1;
        readonly List<Object> owned=new();readonly RaycastHit2D[] hits=new RaycastHit2D[64];
        DarklightCampaign game;DarklightFighter fighter;Collider2D capsule;AttackDefinition dash,drift,sustain,slam,finisher;CombatLoadout riderLoadout;DarklightEnemy finisherTarget;
        int serial=-1,impactSerial=-1;uint paidPulses;bool driftQueued,driftUsed,slamQueued;AttackDefinition pendingAttack;float reservation,driftSide=1;Vector2 dashFacing;
        public WeaponDefinition Initialize(DarklightCampaign owner)
        {
            game=owner;fighter=GetComponent<DarklightFighter>();capsule=GetComponent<Collider2D>();
            var weapon=ScriptableObject.CreateInstance<WeaponDefinition>();owned.Add(weapon);weapon.name="CavaleiroDoAbismo_Prototype";weapon.id=DarklightWeaponId.AbyssRider;weapon.displayName="CAVALEIRO DO ABISMO";weapon.unlockAfterChapter=-1;
            var l=new CombatLoadout{weapon=3,attackBuffer=.42f,defenseBuffer=.18f};
            l.lights=new[]{Make("Serra de Entrada",StrikeKind.Light,.25f,.24f,.37f,8,1.65f,115,2,1.1f),Make("Dentes Cruzados",StrikeKind.Light,.3f,.28f,.4f,8,1.85f,155,3,1.2f),Make("Giro das Engrenagens",StrikeKind.Heavy,.32f,.3f,.48f,11,2,240,2,1.5f)};
            l.heavy=Make("Quebra de Eixo",StrikeKind.Heavy,.42f,.2f,.58f,29,1.95f,120,1,3.5f);
            l.charged=Make("Sobrecarga das Serras",StrikeKind.Charged,.5f,.3f,.72f,13,2.1f,175,3,1.7f);l.charged.chargeSeconds=.65f;
            l.counter=Make("Contra-Eixo",StrikeKind.Counter,.13f,.18f,.4f,32,1.8f,125,1,4);
            slam=Make("Esmagamento do Abismo",StrikeKind.Skill,.68f,.14f,.8f,38,2.25f,360,1,5);slam.hitShape=HitShape.Sector;slam.knockback=.8f;slam.cooldown=4.4f;slam.windupMove=.03f;slam.recoveryMove=.08f;slam.recoveryChainFraction=.55f;l.skill=slam;
            l.aerial=Make("Ascensao Serrilhada",StrikeKind.Aerial,.3f,.18f,.58f,23,1.75f,120,1,2.6f);l.aerial.launch=true;l.aerial.cooldown=2;
            l.pauseBranch=Make("Engate Preciso",StrikeKind.Light,.22f,.26f,.34f,10,1.9f,160,2,1.4f);
            l.heavyBranch=Make("Queda do Motor",StrikeKind.Heavy,.44f,.2f,.65f,36,2,150,1,4);
            dash=Make("Investida Demoniaca",StrikeKind.WeaponSpecial,.44f,.31f,.62f,27,1.65f,100,1,4);dash.lungeSpeed=1.8f;dash.cooldown=4.8f;dash.knockback=.85f;dash.windupMove=.1f;dash.recoveryMove=.15f;dash.recoveryChainFraction=.64f;l.weaponSpecial=dash;
            drift=Make("Derrapagem Sangrenta",StrikeKind.WeaponSpecial,.06f,.38f,.62f,8,1.8f,290,2,1.5f);drift.lungeSpeed=1;drift.knockback=.5f;drift.recoveryChainFraction=.64f;
            sustain=Make("Serras Continuas",StrikeKind.Charged,.12f,.96f,.58f,5,1.55f,100,6,.7f);sustain.windupMove=.05f;sustain.recoveryMove=.1f;sustain.knockback=.04f;sustain.hitCancelFraction=1.35f;sustain.whiffCancelFraction=1.6f;sustain.recoveryChainFraction=.65f;
            finisher=Make("Investida Final",StrikeKind.WeaponSpecial,.48f,.34f,.72f,62,1.8f,100,1,5);finisher.lungeSpeed=2;finisher.cooldown=8.5f;finisher.knockback=.9f;finisher.recoveryChainFraction=.6f;finisher.windupMove=.02f;finisher.recoveryMove=.1f;finisher.finisher=true;
            riderLoadout=l;weapon.loadout=l;weapon.icon=DarklightAbyssSpriteBank.Icon;return weapon;
        }
        AttackDefinition Make(string name,StrikeKind kind,float startup,float active,float recovery,float damage,float range,float angle,int pulses,float posture)
        {
            var a=ScriptableObject.CreateInstance<AttackDefinition>();owned.Add(a);a.name="Abyss_"+name;a.displayName=name;a.kind=kind;a.startup=startup;a.active=active;a.recovery=recovery;a.damage=damage;a.range=range;a.angle=angle;a.hitPulses=pulses;a.postureDamage=posture;a.knockback=.22f;a.windupMove=.24f;a.recoveryMove=.3f;a.hitShape=angle>180?HitShape.Sweep:HitShape.Sector;a.hitstop=.03f;a.hitCancelFraction=1;a.whiffCancelFraction=1.6f;a.recoveryChainFraction=.4f;return a;
        }
        bool CanStart(CombatModel m)=>m.State==CombatState.Neutral||m.State==CombatState.Attack&&m.AttackConnected&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active+m.Attack.recovery*.12f;
        void Reserve(AttackDefinition attack){pendingAttack=attack;reservation=.25f;}
        DarklightEnemy FindFinisherTarget()
        {
            DarklightEnemy result=null;float best=3.1f;
            foreach(var enemy in game.MapEnemies)
            {
                if(enemy==null||!enemy.gameObject.activeInHierarchy||!enemy.AcceptDamage||!enemy.Vulnerable)continue;
                float distance=Vector2.Distance(transform.position,enemy.transform.position);
                if(distance<best&&DarklightNavigation.Clear(transform.position,enemy.transform.position)){best=distance;result=enemy;}
            }
            return result;
        }
        public bool AcceptStrikeTarget(DarklightCombatant target)=>!Finishing||finisherTarget!=null&&target.gameObject==finisherTarget.gameObject;
        public void BeforeCombat(float dt,ref CombatInput input)
        {
            if(game==null)return;
            if(dt<=0){if(Active&&game.Active&&DarklightCombatLab.Instance?.Paused==false)CaptureStoppedCommands(ref input);return;}
            var m=fighter.Model;
            bool precise=Gear.Tick(dt,Active,m.AttackSerial,m.Attack,m.StateTime,m.AttackConnected,m.State==CombatState.Attack,!Dashing&&(input.lightPressed||input.heavyPressed||input.weaponSpecialPressed),m.State==CombatState.Hurt||m.State==CombatState.Dead,game.Form);
            NoteLessons();
            if(precise){game.AudioDirector?.Play("rpg-metalClick",DarklightAudioCategory.Combat,.16f,null,Gear.Level==3?1.25f:1);DarklightCombatFX.Impact(transform.position+(Vector3)m.Facing*.45f,false);}
            if(!Active){ClearCommands();pendingAttack=null;slamQueued=driftQueued=false;return;}
            PrepareCommands(dt,ref input);
            reservation=Mathf.Max(0,reservation-dt);if(reservation<=0)pendingAttack=null;
            if(Sustaining)
            {
                input.move*=.12f;input.dodgePressed=input.parryPressed=input.aerialPressed=false;
                if(!input.lightHeld&&m.StateTime<m.Attack.startup+m.Attack.active)m.FinishAttackContact();
                if(BlocksSwap||!CanStart(m)||!input.weaponSpecialPressed&&!input.skillPressed)return;
            }
            if(Slamming){input.move=Vector2.zero;}
            if(Dashing)
            {
                if(m.StateTime<m.Attack.startup+m.Attack.active+.22f)input.aim=dashFacing;
                if(!Drifting&&!Finishing&&!DashBlocked&&!driftUsed&&m.StateTime<=m.Attack.startup+m.Attack.active&&input.heavyPressed)
                {driftQueued=true;driftSide=Vector2.Dot(input.move,new Vector2(-dashFacing.y,dashFacing.x))<0?-1:1;input.heavyPressed=false;}
                input.heavyPressed=false;
                if(input.skillPressed&&m.StateTime<=m.Attack.startup+m.Attack.active){slamQueued=true;driftQueued=false;input.skillPressed=false;}
                if(driftQueued&&m.Attack==dash&&!m.ActiveThisTick&&m.StateTime>=dash.startup+dash.active)
                {
                    driftQueued=false;
                    if(!DashBlocked&&game.Blood.Amount>=DriftCost&&m.TryWeaponFollowup(dash,drift))
                    {game.Blood.Set(game.Blood.Amount-DriftCost);driftUsed=true;DriftDistance=0;}
                }
                // A deliberate mounted slam lifts the assembled bike before the automatic vault.
                if(slamQueued&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active)
                {
                    slamQueued=false;
                    if(game.Blood.Amount>=SlamCost&&m.SkillRemaining<=0&&m.TryWeaponFollowup(m.Attack,slam))
                    {Reserve(slam);SlamFromMounted=true;}
                }
            }
            if(BlocksSwap){input.dodgePressed=input.parryPressed=input.skillPressed=input.aerialPressed=input.weaponSpecialPressed=false;}
            if(!bikeBuffered&&!slamBuffered&&!Dashing&&!Slamming&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&input.lightHeld&&m.AttackConnected&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active&&game.Blood.Amount>=SustainEntryCost)
            {if(m.TryWeaponFollowup(m.Attack,sustain)){Reserve(sustain);return;}}
            if(input.weaponSpecialPressed)
            {
                bikeBuffered=false;input.skillPressed=false;
                AttackDefinition wanted=dash;
                finisherTarget=Gear.Level==3&&game.Blood.Amount>=FinisherCost?FindFinisherTarget():null;
                if(finisherTarget!=null)
                {wanted=finisher;input.aim=((Vector2)finisherTarget.transform.position-(Vector2)transform.position).normalized;input.heavyPressed=false;}
                float cost=wanted==finisher?FinisherCost:DashCost;
                if(!CanStart(m)||m.WeaponSpecialRemaining>0||game.Blood.Amount<cost){input.weaponSpecialPressed=false;return;}
                riderLoadout.weaponSpecial=wanted;Reserve(wanted);
            }
            else if(input.skillPressed)
            {
                slamBuffered=false;
                if(!CanStart(m)||m.SkillRemaining>0||game.Blood.Amount<SlamCost){input.skillPressed=false;return;}
                SlamFromMounted=false;Reserve(slam);
            }
        }
        public void AfterCombat()
        {
            if(!Active)return;
            var m=fighter.Model;NoteLessons();
            if(m.AttackSerial!=serial)
            {
                serial=m.AttackSerial;paidPulses=0;
                float cost=m.Attack==finisher?FinisherCost:m.Attack==dash?DashCost:m.Attack==slam?SlamCost:m.Attack==sustain?SustainEntryCost:0;
                if(cost>0)
                {
                    if(pendingAttack!=m.Attack||game.Blood.Amount<cost||m.Attack==finisher&&!Gear.TryConsumeOverload()){m.FinishAttackContact();pendingAttack=null;return;}
                    game.Blood.Set(game.Blood.Amount-cost);pendingAttack=null;
                    if(m.Attack==dash||m.Attack==finisher){DashBlocked=false;DashDistance=0;DriftDistance=0;driftQueued=driftUsed=slamQueued=false;dashFacing=m.Facing;ResetRunOver();}
                    game.AudioDirector?.Play("rpg-metalLatch",DarklightAudioCategory.Combat,.2f,null,.78f,.2f);
                }
            }
            if(Sustaining&&m.ActiveThisTick)
            {
                uint fresh=m.ContactPulses&~paidPulses;int count=0;for(int p=0;p<6;p++)if((fresh&(1u<<p))!=0)count++;
                float cost=count*SustainPulseCost;
                if(game.Blood.Amount<cost){m.FinishAttackContact();return;}
                if(cost>0){game.Blood.Set(game.Blood.Amount-cost);paidPulses|=fresh;}
            }
            if((Slamming&&m.ActiveThisTick||Finishing&&m.AttackConnected)&&impactSerial!=m.AttackSerial)
            {
                impactSerial=m.AttackSerial;
                Vector3 contact=Finishing&&finisherTarget!=null?finisherTarget.transform.position:transform.position+(Vector3)m.Facing*.8f;
                DarklightCombatFX.MotorcycleImpact(contact,m.Facing,Finishing?DarklightMotorcycleImpact.Finisher:DarklightMotorcycleImpact.Ground);
                if(Finishing)game.TechnicalFinisher(finisherTarget!=null?finisherTarget.transform.position:transform.position);
            }
        }
        public Vector2 DashVelocity(Rigidbody2D body,Vector2 requested,float step)
        {
            if(!Dashing||!fighter.Model.ActiveThisTick||step<=0)return requested;
            if(DashBlocked)return Vector2.zero;
            Vector2 delta;
            if(Drifting)
            {
                float t=Mathf.Clamp01((fighter.Model.StateTime-drift.startup)/drift.active);
                float angle=(70+35*t)*driftSide*Mathf.Deg2Rad;
                Vector2 heading=new Vector2(dashFacing.x*Mathf.Cos(angle)-dashFacing.y*Mathf.Sin(angle),dashFacing.x*Mathf.Sin(angle)+dashFacing.y*Mathf.Cos(angle));
                MotionHeading=heading;delta=heading*Mathf.Lerp(3.8f,1.5f,t)*step;
            }
            else {MotionHeading=dashFacing;delta=Vector2.ClampMagnitude(requested,8.5f)*step;}
            float remaining=Drifting?MaximumDriftDistance-DriftDistance:(Finishing?3:MaximumDashDistance)-DashDistance;
            if(remaining<=.001f){fighter.Model.FinishAttackContact();return Vector2.zero;}
            delta=Vector2.ClampMagnitude(delta,remaining);float allowed=delta.magnitude;
            if(allowed<=.00001f)return Vector2.zero;
            int count=capsule.Cast(delta.normalized,new ContactFilter2D{useTriggers=true,useLayerMask=false},hits,allowed+.035f);
            bool blocked=count>=hits.Length,contactFound=false;Vector2 contactPoint=body.position;if(blocked)allowed=0;
            for(int i=0;i<count;i++)
            {
                var hit=hits[i];if(hit.collider==null||hit.rigidbody==body)continue;
                var actor=hit.collider.GetComponentInParent<DarklightCombatant>();
                if(actor!=null&&(actor.player||actor.Health.IsDead||CanPassRunOverTarget(actor)))continue;
                // Only the single confirmed, staggered light target can be rolled over. Other
                // trigger volumes (objectives, effects) never block movement.
                if(actor==null&&hit.collider.isTrigger)continue;
                float distance=Mathf.Max(0,hit.distance-.035f);
                if(distance<=allowed){contactFound=true;contactPoint=hit.point;allowed=distance;}blocked=true;
            }
            if(Drifting)DriftDistance+=allowed;else DashDistance+=allowed;
            if(blocked){if(contactFound)DarklightCombatFX.MotorcycleImpact(contactPoint,-delta.normalized,DarklightMotorcycleImpact.Collision);DashBlocked=true;driftQueued=slamQueued=false;fighter.Model.FinishAttackContact();Gear.Crash();}
            return delta.normalized*(allowed/step);
        }
        public void ResetMotion(){ClearCommands();ResetRunOver();serial=impactSerial=-1;pendingAttack=null;reservation=0;DashBlocked=false;DashDistance=DriftDistance=0;driftQueued=driftUsed=slamQueued=false;SlamFromMounted=false;paidPulses=0;finisherTarget=null;dashFacing=Vector2.down;Gear.Reset();if(riderLoadout!=null)riderLoadout.weaponSpecial=dash;}

        void OnDestroy(){foreach(var asset in owned)if(asset!=null){if(Application.isPlaying)Destroy(asset);else DestroyImmediate(asset);}owned.Clear();}
    }
    public sealed partial class DarklightCampaign
    {
        public DarklightAbyssRider AbyssRider{get;private set;}
        UnityEngine.UI.Text abyssGauge;
        void InitializeAbyssRider()
        {
            AbyssRider=lab.Player.GetComponent<DarklightAbyssRider>()??lab.Player.gameObject.AddComponent<DarklightAbyssRider>();
            var original=weaponDefinitions;weaponDefinitions=new WeaponDefinition[4];System.Array.Copy(original,weaponDefinitions,Mathf.Min(3,original.Length));weaponDefinitions[3]=AbyssRider.Initialize(this);
        }
        void StartAbyssPrototype(){DarklightAbyssSpriteBank.Preload(0);SetTrainingForm(0);SelectSkin(1,false);Weapons.Request(3);Close();Notice("PROTOTIPO NORMAL: ENGRENAGENS + INVESTIDA CURTA");}
        void UpdateAbyssHUD()=>PresentAbyssHUD();
    }
}
