using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightEnemy : MonoBehaviour
    {
        public RequiemEnemyKind kind; public Sprite[] poses,attackFrames; public bool elite;
        HealthSystem _health;
        public HealthSystem Health => _health != null ? _health : (_health = GetComponent<HealthSystem>());
        public bool Boss => IsLucius || (int)kind>=5;
        SpriteRenderer art;
        Rigidbody2D _body;
        public Rigidbody2D body => _body != null ? _body : (_body = GetComponent<Rigidbody2D>());
        float timer,stagger,clock,corpse,controlReady,staggerReady;
        float damageTintUntil;
        public bool Vulnerable=>!Health.IsDead&&stagger>0;
        public bool Control(float duration){if(Boss||kind==RequiemEnemyKind.Armored||Health.IsDead||clock<controlReady)return false;controlReady=clock+2;stagger=Mathf.Max(stagger,Mathf.Clamp(duration,0,.65f));state=0;body.linearVelocity=Vector2.zero;return true;}
        readonly System.Collections.Generic.List<Vector2> route=new();float repath;Vector2 facing=Vector2.down;
        float walkedDistance;
        bool presentingWalk;
        Vector2 walkSample;
        DarklightAttackWarning warning;
        float windup;
        int state, serial; Vector2 locked, dash; bool rewarded,phaseTwo;
        public string StateName => Health.IsDead?"Derrotado":state==1?"Preparando":state==2?"Ataque":"Vigiando";
        void Awake()
        {
            _health=GetComponent<HealthSystem>();art=GetComponentInChildren<SpriteRenderer>();_body=GetComponent<Rigidbody2D>();
            Health.OnDeath+=Reward;Health.OnDamageReceived+=MarkEnemyDamage;
        }
        void OnDestroy()
        {
            if(_health==null)return;
            _health.OnDeath-=Reward;_health.OnDamageReceived-=MarkEnemyDamage;
        }
        void Update()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;if(game==null||lab==null)return;
            float dt=lab.CombatDelta*game.EnemyTimeScale;
            if(!IsLucius&&warning!=null&&state!=1)warning.Hide();
            if(Health.IsDead&&warning!=null)warning.Hide();
            if(Health.IsDead&&!rewarded){rewarded=true;game.EnemyDefeated(this);GetComponent<Collider2D>().enabled=false;}
            if(dt<=0){body.linearVelocity=Vector2.zero;return;}
            if(Health.IsDead)
            {
                if(IsLucius){TickAerial(dt);TickLucius(game,lab,dt);return;}
                TickAerial(dt);
                PresentEnemyDefeat(dt);
                return;
            }
            if(!IsLucius&&Visitor==DarklightVisitor.None&&(lab.Player==null||lab.Player.Model.State==CombatState.Dead))
            {
                TickAerial(dt);
                StopEnemyAttack();
                ResetEnemyStride();
                art.color=Color.white;
                Show(0);
                return;
            }
            if(TickExpandedBoss(game,dt))return;
            clock+=dt;timer-=dt;stagger-=dt;TickAerial(dt);
            if(IsLucius){TickLucius(game,lab,dt);return;}
            if(TickVisitor(game,lab,dt))return;
            Vector2 playerPosition=lab.Player.transform.position;
            Vector2 delta=playerPosition-body.position;float distance=delta.magnitude;
            art.sortingOrder=DarklightSorting.OrderForY(transform.position.y);
            if(Boss&&!ExpandedBoss&&!phaseTwo&&Health.HealthPercent<.5f)
            {
                phaseTwo=true;game.BossPhaseTwo();game.Notice("FASE 2: "+DarklightStory.Bosses[(int)kind-5]);
                StopEnemyAttack();timer=.85f;
            }
            if(stagger>0){warning?.Hide();body.linearVelocity=Vector2.zero;SetEnemyTint(new Color(1,.7f,.7f));Show(0);ResetEnemyStride();return;}
            var threat=NextEnemyThreat;
            bool ranged=threat==DarklightThreat.Projectile;
            float range=EnemyAttackRange(threat);
            float speed=kind==RequiemEnemyKind.Hunter?3.3f:kind==RequiemEnemyKind.Armored?1.25f:2.15f;
            if(Boss)speed=ExpandedBoss?ExpandedSpeed:phaseTwo?3.15f:2.4f;
            speed*=game.DifficultyProfile.speed*game.EnemyTimeScale;
            bool sight=state==0&&DarklightNavigation.Clear(body.position,playerPosition);
            if(TickTactics(game,dt,delta,distance,range,speed,ranged,sight))return;
            if(state!=0)
            {
                TickEnemyAttack(game,lab);
                return;
            }
            if(sight&&distance<range&&timer<=0&&game.RequestAttack(this))
            {
                BeginTacticalSequence(game);
                StartEnemyAttack(game,delta,threat,range);
                return;
            }
            MoveEnemy(dt,playerPosition,delta,distance,range,speed,ranged,sight);
        }

        void MarkEnemyDamage(float amount)
        {
            if (amount > 0 && !Health.IsDead && !IsLucius && Visitor == DarklightVisitor.None)
                damageTintUntil = clock + .12f;
        }

        void SetEnemyTint(Color baseColor)
        {
            float pulse = Mathf.Clamp01((damageTintUntil - clock) / .12f);
            if (pulse > 0)
            {
                float strength = DarklightCampaign.Instance?.ReducedFlashes == true ? .12f : .40f;
                baseColor = Color.Lerp(baseColor, new Color(1f, .48f, .44f), pulse * strength);
            }
            art.color = baseColor;
        }

        void PresentEnemyDefeat(float dt)
        {
            body.linearVelocity = Vector2.zero;
            if (visitorDefeat != null) art.sprite = visitorDefeat;
            else if (corpse == 0 && Visitor == DarklightVisitor.None)
            {
                if (HoldsGroundAttack) ShowAttack(3);
                if (!Airborne) ResetEnemyStride();
            }
            corpse += dt;
            if (Visitor != DarklightVisitor.None)
                art.color = new Color(.35f,.2f,.3f,Mathf.Clamp01(1f-corpse/1.3f));
            else
            {
                // Let the authentic silhouette read briefly before the existing fade removes it.
                float fade = Mathf.InverseLerp(.12f, 1.3f, corpse);
                art.color = Color.Lerp(Color.white, new Color(.35f, .2f, .3f, 0f), fade);
            }
            if (corpse > 1.4f) Destroy(gameObject);
        }

        void MoveEnemy(float dt, Vector2 playerPosition, Vector2 delta, float distance, float range, float speed, bool ranged, bool sight)
        {
            SetEnemyTint(Color.white);
            bool pursue = !sight || distance > range * .8f;
            Vector2 direction = delta.normalized;
            repath -= dt;
            if (!sight)
            {
                if (repath <= 0)
                {
                    DarklightNavigation.Instance?.FindPath(body.position, playerPosition, route);
                    repath = .45f;
                }
                while (route.Count > 0 && Vector2.Distance(body.position, route[0]) < .35f) route.RemoveAt(0);
                direction = route.Count > 0 ? (route[0] - body.position).normalized : Vector2.zero;
            }
            SetEnemyFacing(sight ? delta : direction);
            if (ranged && sight && distance < 2.1f) direction *= -.7f;
            else if (!pursue) direction = Vector2.zero;
            body.linearVelocity = DarklightNavigation.SlidingVelocity(body, direction * speed, Time.fixedDeltaTime);
            PresentEnemyLocomotion();
        }

        void ResetEnemyStride()
        {
            presentingWalk = false;
            walkedDistance = 0;
            if (art.transform != transform) art.transform.localPosition = Vector3.zero;
        }

        void PresentEnemyLocomotion()
        {
            Vector2 position = body.position;
            if (body.linearVelocity.sqrMagnitude < .01f)
            {
                ResetEnemyStride();
                Show(0);
            }
            else
            {
                float stepLength = kind switch
                {
                    RequiemEnemyKind.Armored => .29f,
                    RequiemEnemyKind.Hunter => .34f,
                    RequiemEnemyKind.Kael or RequiemEnemyKind.KaelFractured => .36f,
                    _ => .28f
                };
                if (presentingWalk)
                    walkedDistance = Mathf.Repeat(walkedDistance + Vector2.Distance(position, walkSample), stepLength * 2f);
                walkSample = position;
                presentingWalk = true;
                Show(walkedDistance < stepLength ? 1 : 2);
            }
            if (art.transform != transform && kind == RequiemEnemyKind.Spectral)
                art.transform.localPosition = Vector3.up * Mathf.PingPong(clock * .08f, .05f);
        }
        void ShowAttack(int index)
        {
            var transformed=FinalFormFrames;
            if(transformed!=null){ShowFinalForm(index+4);return;}
            if(attackFrames?.Length!=4){Show(3);return;}
            art.sprite=attackFrames[index];
            art.transform.localScale=Vector3.one*PresentationScale;
        }
        internal bool UsesOfficialCombatArt {get;set;}
        internal float PresentationScale=>Visitor!=DarklightVisitor.None?1f:
            UsesOfficialCombatArt?DarklightCharacterView.WorldVisualScale:DarklightWorldScale.EnemyHeight(kind)/poses[0].bounds.size.y;
        bool ShowEnemyPilot(string name)
        {
            var frames=DarklightSecondaryStyleBank.Load(name);
            if(frames.Length!=1)return false;
            art.sprite=frames[0];
            art.transform.localScale=Vector3.one*DarklightWorldScale.EnemyHeight(kind)/poses[0].bounds.size.y;
            return true;
        }
        void Show(int index)
        {
            if(FinalFormFrames!=null){ShowFinalForm(index);return;}
            art.transform.localScale=Vector3.one*PresentationScale;
            if(poses!=null&&poses.Length>index)art.sprite=poses[index];
        }
        void Reward(){if(rewarded)return;rewarded=true;DarklightCampaign.Instance?.EnemyDefeated(this);GetComponent<Collider2D>().enabled=false;}
        public bool EvadeStrike()
        {
            var game=DarklightCampaign.Instance;
            if(game==null||(game.Training&&game.TrainingPassive)||dodgeReadAt<0||clock<dodgeReadAt||clock<guardReady)return false;
            if((kind!=RequiemEnemyKind.Kael&&kind!=RequiemEnemyKind.Hunter)||state!=0||serial%3!=1||timer>.3f)return false;
            Vector2 side=(Vector2)(Quaternion.Euler(0,0,90)*((Vector2)DarklightCombatLab.Instance.Player.transform.position-body.position).normalized);
            if(!TryEnemyEvasiveStep(side*.6f))return false;
            timer=.6f;serial++;guardReady=clock+1.8f;ResetEnemyStride();return true;
        }
        bool TryEnemyEvasiveStep(Vector2 offset)
        {
            if (offset.sqrMagnitude <= .0001f) return false;
            Vector2 start = body.position;
            if (!DarklightNavigation.Clear(start, start + offset))
            {
                offset = -offset;
                if (!DarklightNavigation.Clear(start, start + offset)) return false;
            }
            DarklightNavigation.SafeStep(body, offset);
            return (body.position - start).sqrMagnitude >= .04f;
        }
        public void Struck(float force,bool heavy)
        {
            if(IsLucius)return;
            bool armor=kind==RequiemEnemyKind.Armored||kind==RequiemEnemyKind.Sentinel;
            if((!armor||heavy)&&clock>=staggerReady&&(!Boss||heavy)) {stagger=Mathf.Max(stagger,Boss?.10f:.24f);staggerReady=clock+(Boss?2f:.55f);state=0;timer=Mathf.Max(timer,.45f);}
            if(!Boss)
            {
                float travel=armor?Mathf.Min(.32f,force*.4f):Mathf.Min(.8f,force);
                DarklightNavigation.SafeStep(body,DarklightCombatLab.Instance.Player.Model.Facing*travel);
            }
        }
        public void PullToward(Vector2 point,float distance)
        {
            if(Boss||kind==RequiemEnemyKind.Armored||Health.IsDead||clock<controlReady)return;
            Vector2 delta=point-body.position;float step=Mathf.Min(distance,Mathf.Max(0,delta.magnitude-1));
            DarklightNavigation.SafeStep(body,delta.normalized*step,.3f);body.linearVelocity=Vector2.zero;Control(.28f);
        }
        void FixedUpdate(){if(DarklightCombatLab.Instance?.CombatDelta<=0)body.linearVelocity=Vector2.zero;}
    }
    public sealed class DarklightProjectile : MonoBehaviour
    {
        static readonly RaycastHit2D[] rayHits=new RaycastHit2D[64];
        public Vector2 direction;public float damage,speed;float age;
        void Update()
        {
            var lab=DarklightCombatLab.Instance;if(lab==null)return;float dt=lab.CombatDelta*(DarklightCampaign.Instance?.EnemyTimeScale??1);if(dt<=0)return;
            Vector2 origin=transform.position;Vector2 step=direction*speed*dt;
            int count=Physics2D.Raycast(origin,direction,new ContactFilter2D{useTriggers=true,useLayerMask=false},rayHits,step.magnitude+.12f);
            if(count>=rayHits.Length){Destroy(gameObject);return;}
            for(int i=0;i<count;i++)
            {
                var hit=rayHits[i];
                var actor=hit.collider.GetComponentInParent<DarklightCombatant>();
                if(actor!=null){if(actor.player){actor.Receive(damage,direction);Destroy(gameObject);return;}continue;}
                if(!hit.collider.isTrigger){Destroy(gameObject);return;}
            }
            transform.position+=(Vector3)step;age+=dt;if(age>4)Destroy(gameObject);
        }
    }
}


