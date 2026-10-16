using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        float guardUntil,guardReady,dodgeReadAt=-1,tacticalRecovery,posture,postureIdle,airTime,airVelocity,airHeight,airReady;
        int aerialHits,chainRemaining;
        public bool Airborne=>airHeight>.01f;
        public float Posture=>posture;
        public bool Guarding=>clock<guardUntil;
        void TickAerial(float dt)
        {
            airReady=Mathf.Max(0,airReady-dt);
            if(airHeight<=0&&airVelocity<=0)return;
            airVelocity-=dt*8;airHeight=Mathf.Max(0,airHeight+airVelocity*dt);
            if(airHeight==0){airVelocity=0;airReady=.8f;aerialHits=0;stagger=Mathf.Max(stagger,.2f);art.transform.localPosition=Vector3.zero;}
        }
        bool TickTactics(DarklightCampaign game,float dt,Vector2 delta,float distance,float range,float speed,bool ranged)
        {
            postureIdle+=dt;if(postureIdle>1.5f)posture=Mathf.Max(0,posture-dt*.7f);
            if(game.Training&&game.TrainingPassive){body.linearVelocity=Vector2.zero;Show(0);return true;}
            if(Airborne){body.linearVelocity=Vector2.zero;warning?.Hide();ShowAttack(0);return true;}
            if(state!=0||stagger>0)return false;
            var profile=game.DifficultyProfile;
            if(clock<guardUntil){body.linearVelocity=Vector2.zero;Show(0);art.color=new Color(.7f,.85f,1);return true;}
            var model=DarklightCombatLab.Instance.Player.Model;
            bool threatened=(model.State==CombatState.Charging||model.State==CombatState.Attack)&&distance<(model.Attack?.range??2)+.8f;
            if(!threatened)dodgeReadAt=-1;else if(dodgeReadAt<0)dodgeReadAt=clock+profile.reaction;
            if(threatened&&clock>=dodgeReadAt&&clock>=guardReady)
            {
                bool defender=kind==RequiemEnemyKind.Armored||kind==RequiemEnemyKind.Sentinel||Boss;
                if(defender){guardUntil=clock+.3f;guardReady=clock+(game.Difficulty>0?1.7f:2.6f);timer=Mathf.Max(timer,.25f);return true;}
                if(game.Difficulty>0&&(kind==RequiemEnemyKind.Hunter||kind==RequiemEnemyKind.Spectral||elite))
                {var side=(Vector2)(Quaternion.Euler(0,0,serial%2==0?90:-90)*delta.normalized);DarklightNavigation.SafeStep(body,side*.65f);guardReady=clock+1.8f;timer=.2f;serial++;return true;}
            }
            // A role-based sidestep prevents every pursuer from forming the same queue.
            if(!ranged&&distance>range*.9f&&distance<6&&timer>.18f&&clock>tacticalRecovery)
            {
                float sideSign=(GetEntityId().GetHashCode()&1)==0?1:-1;
                var flank=(delta.normalized+(Vector2)(Quaternion.Euler(0,0,90)*delta.normalized)*sideSign*profile.flank).normalized;
                var desired=flank*speed;
                body.linearVelocity=DarklightNavigation.SlidingVelocity(body,desired,Time.fixedDeltaTime);
                facing=delta.normalized;Show(1+(int)(clock*12)%2);
                if(distance>range*1.05f)return true;
            }
            return false;
        }
        public bool TryGuardHit(Vector2 force)
        {
            if(!Guarding||Health.IsDead)return false;
            var game=DarklightCampaign.Instance;var model=DarklightCombatLab.Instance?.Player?.Model;
            if(model?.Attack==null||game==null)return false;
            Vector2 toward=DarklightCombatLab.Instance.Player.transform.position-transform.position;
            if(Vector2.Angle(facing,toward)>75)return false;
            bool breaker=model.Attack.kind==StrikeKind.Charged||model.Attack.postureDamage*(game.AbyssRider?.Posture??1)>=3;
            if(breaker){guardUntil=0;posture=0;stagger=Mathf.Max(stagger,.65f);state=0;timer=.8f;return false;}
            posture+=model.Attack.postureDamage*(game.AbyssRider?.Posture??1);
            if(posture>=3){guardUntil=0;stagger=.5f;state=0;posture=0;timer=.65f;}
            DarklightCombatFX.Impact(transform.position,false);return true;
        }
        public void TechnicalImpact(AttackDefinition attack,int form)
        {
            postureIdle=0;posture+=attack.postureDamage*(form==1?1.55f:form==3?1.25f:1)*(DarklightCampaign.Instance?.AbyssRider?.Posture??1);
            float threshold=Boss?8:kind==RequiemEnemyKind.Armored?4.5f:3;
            if(posture>=threshold&&clock>=tacticalRecovery)
            {posture=0;stagger=Boss?.28f:.65f;tacticalRecovery=clock+(Boss?2.2f:1);state=0;timer=.65f;guardUntil=0;}
            if(attack.launch&&!Boss&&kind!=RequiemEnemyKind.Armored&&airReady<=0&&aerialHits<3)
            {airHeight=Mathf.Max(.05f,airHeight);airVelocity=2.6f;aerialHits++;state=0;timer=.6f;}
            if(attack.finisher&&!Boss&&Vulnerable&&Health.HealthPercent<=.12f)
            {DarklightCampaign.Instance?.TechnicalFinisher(transform.position);Health.TakeDamage(Health.CurrentHealth);DarklightCombatFX.Impact(transform.position,true);}
        }
        void LateUpdate(){if(art!=null&&Airborne)art.transform.localPosition=Vector3.up*airHeight;}
        void BeginTacticalSequence(DarklightCampaign game)
        {chainRemaining=game.DifficultyProfile.comboLength-1;if(elite)chainRemaining++;if(Boss&&phaseTwo)chainRemaining++;}
        bool ContinueTacticalSequence(DarklightCampaign game)
        {
            if(chainRemaining<=0||Health.IsDead||stagger>0)return false;
            var player=DarklightCombatLab.Instance.Player;
            if(Vector2.Distance(body.position,player.transform.position)>7)return false;
            var viewport=DarklightCombatLab.Instance.WorldCamera.WorldToViewportPoint(transform.position);
            if(viewport.x<.02f||viewport.x>.98f||viewport.y<.02f||viewport.y>.98f)return false;
            chainRemaining--;state=1;timer=windup=Mathf.Max(.32f,.48f*game.DifficultyProfile.windup);
            locked=((Vector2)player.transform.position-body.position).normalized;dash=Vector2.zero;
            PrepareExpandedAttack();game.EnemyAudioWarning(this);return true;
        }
    }
}
