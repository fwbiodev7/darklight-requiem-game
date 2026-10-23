using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Interactive sequence: launch > aerial to chase > light/kick > heavy to throw >
    // aerial to catch up above the victim > heavy to dive. No automatic damage.
    public sealed class DarklightAirCombo : MonoBehaviour
    {
        public float Height { get; private set; }
        public bool Chasing { get; private set; }
        public bool AboveTarget { get; private set; }
        public int Contacts { get; private set; }
        public DarklightEnemy Target { get; private set; }
        public bool Active => Height > .01f || Chasing;
        readonly Dictionary<AttackDefinition, AttackDefinition[]> banks=new();
        readonly List<Object> owned=new();
        DarklightFighter fighter;Rigidbody2D body;SpriteRenderer art;
        AttackDefinition chase;float chaseAge,idle,echo,commandLife;int queuedMove=-1;bool thrown,diving,chaseQueued;
        void Awake()
        {
            fighter=GetComponent<DarklightFighter>();body=GetComponent<Rigidbody2D>();art=GetComponentInChildren<SpriteRenderer>();
            chase=ScriptableObject.CreateInstance<AttackDefinition>();owned.Add(chase);
            chase.displayName="Perseguição Aérea";chase.kind=StrikeKind.Aerial;chase.startup=.08f;chase.active=0;chase.recovery=.12f;chase.damage=0;
        }
        AttackDefinition Move(int index)
        {
            if(index==3&&fighter.loadout.airFinisher!=null)return fighter.loadout.airFinisher;
            var l=fighter.loadout;var source=index==0?l.lights[0]:l.heavy??l.lights[0];
            if(!banks.TryGetValue(source,out var moves))
            {
                moves=new AttackDefinition[4];banks.Add(source,moves);
                for(int i=0;i<4;i++)
                {
                    var a=moves[i]=Instantiate(source);owned.Add(a);a.name="Air_"+source.name+"_"+i;
                    a.displayName=new[]{"Sequência Suspensa","Chute de Ruptura","Arremesso Celeste","Impacto Descendente"}[i];
                    a.kind=i==0?StrikeKind.Light:StrikeKind.Heavy;a.startup=i==3?.18f:.08f;a.active=i==0?.16f:.12f;a.recovery=i==3?.3f:.18f;
                    a.damage=source.damage*(i==0?.6f:i==3?1.4f:.85f);a.range=Mathf.Clamp(source.range,1.4f,2.4f);
                    a.hitPulses=i==0&&l.weapon==3?3:1;a.damage/=a.hitPulses;
                    a.launch=a.pullTarget=a.blink=false;a.cooldown=0;a.lungeSpeed=0;a.knockback=i==2?1.8f:.03f;
                    a.airReaction=i==2?DarklightAirReaction.Throw:i==3?DarklightAirReaction.Slam:DarklightAirReaction.Suspend;
                    a.finisher=i==3;a.windupMove=.4f;a.recoveryMove=.65f;a.recoveryChainFraction=.2f;
                    if(l.weapon==2)a.ruinStrike=i==0?DarklightRuinStrike.Punch:i==1?DarklightRuinStrike.SideKick:i==2?DarklightRuinStrike.SpinKick:DarklightRuinStrike.AerialSlam;
                }
            }
            return moves[index];
        }
        public void BeforeCombat(float dt,ref CombatInput input)
        {
            var m=fighter.Model;
            if(dt<=0)
            {
                if(Target!=null&&Target.Airborne&&m.State!=CombatState.Hurt&&m.State!=CombatState.Dead)
                {
                    if(input.aerialPressed){chaseQueued=true;commandLife=.28f;input.aerialPressed=false;}
                    if(Active&&(input.lightPressed||input.heavyPressed))
                    {queuedMove=input.heavyPressed?(AboveTarget?3:Contacts>=3?2:1):0;commandLife=.28f;input.lightPressed=input.heavyPressed=false;}
                }
                return;
            }
            commandLife-=dt;if(commandLife<=0){queuedMove=-1;chaseQueued=false;}
            if(m.State==CombatState.Dead||m.State==CombatState.Hurt){CancelFlight(dt);return;}
            if(Target==null||Target.Health.IsDead||!Target.gameObject.activeInHierarchy||!Target.Airborne)
            {
                Target=null;Chasing=false;AboveTarget=false;diving=false;
                Height=Mathf.MoveTowards(Height,0,dt*6);return;
            }
            if(input.dodgePressed||input.parryPressed){CancelFlight(dt);return;}
            idle+=dt;
            if(Active&&idle>1.1f){CancelFlight(dt);return;}
            float distance=Vector2.Distance(body.position,Target.transform.position);
            if(input.aerialPressed&&distance<6&&!Chasing){chaseQueued=true;commandLife=.28f;input.aerialPressed=false;}
            if(chaseQueued&&!Chasing&&m.TryAirFollowup(chase,input))
            {chaseQueued=false;Chasing=true;chaseAge=idle=0;AboveTarget=thrown;}
            if(Chasing)
            {
                chaseAge+=dt;
                Vector2 offset=body.position-(Vector2)Target.transform.position;
                Vector2 side=offset.sqrMagnitude>.01f?offset.normalized:-m.Facing;
                Vector2 destination=(Vector2)Target.transform.position+side*.65f;
                DarklightNavigation.SafeStep(body,Vector2.ClampMagnitude(destination-body.position,dt*15),.28f);
                Height=Mathf.MoveTowards(Height,Target.AirHeight+(AboveTarget?.55f:0),dt*12);
                if(chaseAge>.12f&&Vector2.Distance(body.position,destination)<.2f){Chasing=false;idle=0;}
                else if(chaseAge>.45f){CancelFlight(dt);return;}
                input.lightPressed=input.heavyPressed=input.skillPressed=input.weaponSpecialPressed=false;
            }
            else if(Active)
            {
                if(diving)Height=Mathf.MoveTowards(Height,Target.AirHeight,dt*15);
                else Height=Mathf.MoveTowards(Height,Target.AirHeight+(AboveTarget?.55f:0),dt*7);
                int move=input.heavyPressed?(AboveTarget?3:Contacts>=3?2:1):0;
                if(input.lightPressed||input.heavyPressed){queuedMove=move;commandLife=.28f;input.lightPressed=input.heavyPressed=false;}
                input.heavyHeld=input.heavyReleased=false;
                if(queuedMove>=0&&m.TryAirFollowup(Move(queuedMove),input))
                {idle=0;diving=queuedMove==3;queuedMove=-1;}
                // Aerial is a contextual command here; it never queues a second launcher.
                input.aerialPressed=false;
            }
            if(Active&&m.State==CombatState.Attack&&m.Attack==chase)input.move=Vector2.zero;
            if(Chasing||diving)
            {echo-=dt;if(echo<=0){echo=.055f;DarklightCombatFX.Echo(art,.14f);}}
        }
        public bool AcceptTarget(DarklightEnemy enemy)
        {
            if(enemy==null)return !Active;
            if(!Active)return !enemy.Airborne||enemy.AirHeight<.65f;
            return enemy.Airborne&&Mathf.Abs(enemy.AirHeight-Height)<(AboveTarget?.85f:.7f);
        }
        public void ConfirmContact(DarklightEnemy enemy,AttackDefinition attack)
        {
            if(enemy==null||enemy.Health.IsDead)return;
            if(attack.launch||attack.airReaction==DarklightAirReaction.Launch)
            {
                if(enemy.Airborne&&!Active){Target=enemy;Contacts=0;thrown=AboveTarget=diving=false;idle=0;}
                return;
            }
            if(!Active||!enemy.Airborne)return;
            Target=enemy;Contacts++;idle=0;
            if(attack.airReaction==DarklightAirReaction.Throw){thrown=true;AboveTarget=false;}
            if(attack.airReaction==DarklightAirReaction.Slam){diving=true;DarklightCombatLab.Instance?.Hitstop(.055f);}
        }
        public void ResetMotion(){Target=null;Height=0;Chasing=AboveTarget=thrown=diving=chaseQueued=false;Contacts=0;idle=echo=commandLife=0;queuedMove=-1;}
        void CancelFlight(float dt){float fall=Mathf.MoveTowards(Height,0,dt*6);ResetMotion();Height=fall;}
        void OnDestroy(){foreach(var a in owned)if(a!=null){if(Application.isPlaying)Destroy(a);else DestroyImmediate(a);}}
    }
}
