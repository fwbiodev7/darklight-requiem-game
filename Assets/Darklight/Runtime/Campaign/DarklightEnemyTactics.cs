using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        float guardUntil,guardReady,dodgeReadAt=-1,tacticalRecovery,posture,postureIdle;
        int enemyReadSerial;
        CombatState enemyReadState;
        float luciusLaunchWindowUntil;
        int chainRemaining;
        public readonly DarklightAirBody AirBody=new();
        public bool Airborne=>AirBody.Airborne;
        public float AirHeight=>AirBody.Height;
        public float Posture=>posture;
        public bool Guarding=>clock<guardUntil;
        public bool LuciusLaunchExposed=>IsLucius&&clock<luciusLaunchWindowUntil;
        void OpenLuciusLaunchWindow(float duration)
        {if(IsLucius&&!Airborne)luciusLaunchWindowUntil=Mathf.Max(luciusLaunchWindowUntil,clock+Mathf.Clamp(duration,0,.65f));}
        void TickAerial(float dt)
        {
            bool landed=AirBody.Tick(dt);
            if(IsLucius&&LuciusView!=null)LuciusView.AirHeight=AirBody.Height;
            if(!landed)return;
            stagger=Mathf.Max(stagger,AirBody.SlamPending?.48f:.2f);state=0;timer=.6f;
            if(!IsLucius&&art!=null&&art.transform!=transform)art.transform.localPosition=Vector3.zero;
            if(AirBody.SlamPending)DarklightCombatFX.MotorcycleImpact(transform.position,Vector2.down,DarklightMotorcycleImpact.AerialGround);
        }
        void TickPosture(float dt)
        {
            postureIdle+=dt;
            if(postureIdle>1.5f)posture=Mathf.Max(0,posture-dt*.7f);
        }
        bool TickTactics(DarklightCampaign game, float dt, Vector2 delta, float distance, float range, float speed, bool ranged, bool sight)
        {
            TickPosture(dt);
            if(game.Training&&game.TrainingPassive)
            {
                if(HoldsGroundAttack)timer=.35f;
                StopEnemyAttack();ResetEnemyStride();art.color=Color.white;Show(0);return true;
            }
            if(Airborne){body.linearVelocity=Vector2.zero;warning?.Hide();ShowAttack(0);return true;}
            if(state!=0||stagger>0)return false;
            var profile=game.DifficultyProfile;
            if(Guarding){PresentEnemyGuard();return true;}
            var model=DarklightCombatLab.Instance.Player.Model;
            var attack=model.State==CombatState.Charging?model.Loadout?.charged??model.Loadout?.heavy:model.Attack;
            bool startupOrContact=model.State==CombatState.Charging||
                (model.State==CombatState.Attack&&attack!=null&&model.StateTime<attack.startup+attack.active);
            bool threatened=sight&&startupOrContact&&attack!=null&&distance<attack.range+.65f&&
                Vector2.Angle(model.Facing,-delta)<=85f;
            if(!threatened)dodgeReadAt=-1;
            else if(dodgeReadAt<0||enemyReadSerial!=model.AttackSerial||enemyReadState!=model.State)
            {
                enemyReadSerial=model.AttackSerial;enemyReadState=model.State;
                dodgeReadAt=clock+profile.reaction;
            }
            Vector2 side = new Vector2(-delta.y, delta.x).normalized;
            if(threatened&&clock>=dodgeReadAt&&clock>=guardReady)
            {
                bool defender=kind==RequiemEnemyKind.Armored||kind==RequiemEnemyKind.Sentinel||Boss;
                if(defender)
                {
                    SetEnemyFacing(delta);
                    guardUntil=clock+(kind==RequiemEnemyKind.Armored?.36f:.30f);
                    guardReady=clock+(game.Difficulty>0?1.7f:2.6f);
                    timer=Mathf.Max(timer,.25f);
                    PresentEnemyGuard();
                    return true;
                }
                if(game.Difficulty>0&&(kind==RequiemEnemyKind.Hunter||kind==RequiemEnemyKind.Spectral||elite)&&
                    TryEnemyEvasiveStep(side*(serial%2==0?.65f:-.65f)))
                {
                    guardReady=clock+1.8f;timer=.2f;serial++;
                    body.linearVelocity=Vector2.zero;ResetEnemyStride();
                    return true;
                }
            }
            if(kind==RequiemEnemyKind.Spectral&&serial>0&&serial%3==0&&timer<=0&&sight&&TryEnemyEvasiveStep(side*1.6f))
            {
                timer=.5f;serial++;body.linearVelocity=Vector2.zero;ResetEnemyStride();
                return true;
            }
            // A role-based sidestep prevents every pursuer from forming the same queue.
            if(sight&&!ranged&&kind!=RequiemEnemyKind.Armored&&distance>range*1.05f&&distance<6&&timer>.18f&&clock>tacticalRecovery)
            {
                float sideSign=(GetEntityId().GetHashCode()&1)==0?1f:-1f;
                Vector2 flank=(delta.normalized+side*sideSign*profile.flank).normalized;
                body.linearVelocity=DarklightNavigation.SlidingVelocity(body,flank*speed,Time.fixedDeltaTime);
                SetEnemyFacing(delta);SetEnemyTint(Color.white);PresentEnemyLocomotion();
                return true;
            }
            return false;
        }
        void PresentEnemyGuard()
        {
            body.linearVelocity=Vector2.zero;
            ResetEnemyStride();
            if(kind!=RequiemEnemyKind.Armored||!ShowEnemyPilot("ArmoredGuard20261119"))Show(0);
            SetEnemyTint(new Color(.7f,.85f,1));
        }
        public bool TryGuardHit(Vector2 force)
        {
            if(!Guarding||Health.IsDead)return false;
            var game=DarklightCampaign.Instance;var model=DarklightCombatLab.Instance?.Player?.Model;
            if(model?.Attack==null||game==null)return false;
            Vector2 toward=DarklightCombatLab.Instance.Player.transform.position-transform.position;
            if(Vector2.Angle(facing,toward)>75)return false;
            bool breaker=model.Attack.kind==StrikeKind.Charged||model.Attack.postureDamage*(game.AbyssRider?.Posture??1)>=3;
            if(breaker){guardUntil=0;posture=0;stagger=Mathf.Max(stagger,.65f);state=0;timer=.8f;OpenLuciusLaunchWindow(.55f);return false;}
            postureIdle=0;posture+=model.Attack.postureDamage*(game.AbyssRider?.Posture??1);
            if(posture>=3){guardUntil=0;stagger=.5f;state=0;posture=0;timer=.65f;OpenLuciusLaunchWindow(.5f);}
            DarklightCombatFX.Impact(transform.position,false);return true;
        }
        public void TechnicalImpact(AttackDefinition attack,int form)
        {
            postureIdle=0;posture+=attack.postureDamage*(form==1?1.55f:form>=2?1.25f:1)*(DarklightCampaign.Instance?.AbyssRider?.Posture??1);
            bool heavyBoss=Boss&&!IsLucius;
            float threshold=heavyBoss?8:kind==RequiemEnemyKind.Armored?4.5f:3;
            if(posture>=threshold&&clock>=tacticalRecovery)
            {
                posture=0;
                float breakDuration=heavyBoss?.28f:.65f;
                stagger=IsLucius?Mathf.Max(stagger,breakDuration):breakDuration;
                tacticalRecovery=clock+(heavyBoss?2.2f:1);
                state=0;timer=.65f;guardUntil=0;
                OpenLuciusLaunchWindow(.55f);
            }
            // Air reactions are applied once per accepted contact by the strike resolver.
            if(attack.finisher&&attack.airReaction!=DarklightAirReaction.Slam&&!Airborne&&!Boss&&Vulnerable&&Health.HealthPercent<=.12f)
            {DarklightCampaign.Instance?.TechnicalFinisher(transform.position);Health.TakeDamage(Health.CurrentHealth);DarklightCombatFX.Impact(transform.position,true);}
        }
        public bool ReactAir(AttackDefinition attack,DarklightAirReaction reaction)
        {
            if(Health.IsDead||!AcceptDamage)return false;
            // Lucius yields only to an earned posture/parry opening. Once in
            // flight, the shared juggle cap and landing immunity own recovery.
            bool resistant=(Boss||kind==RequiemEnemyKind.Armored)&&
                !(IsLucius&&(LuciusLaunchExposed||Airborne));
            bool accepted=AirBody.React(reaction,attack.launchSpeed,resistant);
            if(accepted)
            {
                if(IsLucius&&reaction==DarklightAirReaction.Launch)
                {
                    luciusLaunchWindowUntil=0;
                    CombatModel?.Hurt(false,.3f);
                }
                state=0;timer=.6f;guardUntil=0;warning?.Hide();body.linearVelocity=Vector2.zero;
                if(reaction==DarklightAirReaction.Throw)DarklightNavigation.SafeStep(body,(DarklightCombatLab.Instance?.Player.Model.Facing??Vector2.down)*1.8f,.3f);
            }
            return accepted;
        }
        void LateUpdate()
        {
            // LuciusView owns its visual offset. A renderer on the actor root
            // must never move the Rigidbody/colliders to represent air height.
            if(!IsLucius&&art!=null&&art.transform!=transform&&Airborne)
                art.transform.localPosition=Vector3.up*AirBody.Height;
        }
        void BeginTacticalSequence(DarklightCampaign game)
        {chainRemaining=game.DifficultyProfile.comboLength-1;if(elite)chainRemaining++;if(Boss&&phaseTwo)chainRemaining++;}
        bool ContinueTacticalSequence(DarklightCampaign game, DarklightCombatLab lab)
        {
            if(chainRemaining<=0||Health.IsDead||stagger>0)return false;
            Vector2 playerPosition=lab.Player.transform.position;
            Vector2 delta=playerPosition-body.position;
            var threat=NextEnemyThreat;
            float range=EnemyAttackRange(threat);
            if(!CanPrepareEnemyAttack(playerPosition,delta,range)||!game.RequestAttack(this,true))return false;
            chainRemaining--;
            StartEnemyAttack(game,delta,threat,range,true);
            return true;
        }
    }
}
