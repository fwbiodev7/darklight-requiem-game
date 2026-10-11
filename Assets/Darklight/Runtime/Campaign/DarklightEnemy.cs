using UnityEngine;

namespace Darklight
{
    public sealed class DarklightEnemy : MonoBehaviour
    {
        public RequiemEnemyKind kind; public Sprite[] poses,attackFrames; public bool elite;
        public HealthSystem Health {get;private set;} public bool Boss => (int)kind>=5;
        SpriteRenderer art; Rigidbody2D body; float timer,stagger,clock,corpse;
        readonly System.Collections.Generic.List<Vector2> route=new();float repath;Vector2 facing=Vector2.down;
        int state, serial; Vector2 locked, dash; bool rewarded,phaseTwo;
        public string StateName => Health.IsDead?"Derrotado":state==1?"Preparando":state==2?"Ataque":"Vigiando";
        void Awake(){Health=GetComponent<HealthSystem>();art=GetComponentInChildren<SpriteRenderer>();body=GetComponent<Rigidbody2D>();Health.OnDeath+=Reward;}
        void Update()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;if(game==null||lab==null)return;
            float dt=lab.CombatDelta*game.EnemyTimeScale;
            if(Health.IsDead&&!rewarded){rewarded=true;game.EnemyDefeated(this);GetComponent<Collider2D>().enabled=false;}
            if(dt<=0){body.linearVelocity=Vector2.zero;return;}
            if(Health.IsDead)
            {
                body.linearVelocity=Vector2.zero;
                if(!rewarded){rewarded=true;game.EnemyDefeated(this);GetComponent<Collider2D>().enabled=false;}
                corpse+=dt;art.color=new Color(.35f,.2f,.3f,Mathf.Clamp01(1-corpse/1.3f));if(corpse>1.4f)Destroy(gameObject);return;
            }
            clock+=dt;timer-=dt;stagger-=dt;
            Vector2 delta=(Vector2)lab.Player.transform.position-body.position;float distance=delta.magnitude;
            if(Boss&&!phaseTwo&&Health.HealthPercent<.5f){phaseTwo=true;game.BossPhaseTwo();game.Notice("FASE 2: "+DarklightStory.Bosses[(int)kind-5]);timer=.85f;state=0;}
            if(stagger>0){body.linearVelocity=Vector2.zero;art.color=new Color(1,.7f,.7f);Show(0);return;}
            bool ranged=kind==RequiemEnemyKind.Shooter||kind==RequiemEnemyKind.Spectral||kind==RequiemEnemyKind.Cantor||kind==RequiemEnemyKind.Lucien;
            float range=ranged?6:Boss?2.5f:1.45f;
            float speed=kind==RequiemEnemyKind.Hunter?3.3f:kind==RequiemEnemyKind.Armored?1.25f:2.15f;
            if(Boss)speed=phaseTwo?3.15f:2.4f;
            art.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10);
            if(state==0)
            {
                art.color=Color.white;
                if(kind==RequiemEnemyKind.Spectral&&serial>0&&serial%3==0&&timer<=0){DarklightNavigation.SafeStep(body,(Vector2)(Quaternion.Euler(0,0,90)*delta.normalized)*1.6f);timer=.5f;serial++;}
                bool walk=distance>range*.8f;
                repath-=dt;
                Vector2 direction=delta.normalized;
                bool sight=DarklightNavigation.Clear(body.position,lab.Player.transform.position);
                if(walk&&!sight)
                {
                    if(repath<=0){DarklightNavigation.Instance?.FindPath(body.position,lab.Player.transform.position,route);repath=.45f;}
                    while(route.Count>0 && Vector2.Distance(body.position,route[0])<.35f)route.RemoveAt(0);
                    direction=route.Count>0?(route[0]-body.position).normalized:Vector2.zero;
                }
                facing=delta.normalized;art.flipX=facing.x<-.15f;
                body.linearVelocity=ranged&&distance<2.1f&&sight?-delta.normalized*speed*.7f:walk?direction*speed*game.EnemyTimeScale:Vector2.zero;
                Show(walk?1+(int)(clock*12)%2:0);art.transform.localPosition=Vector3.up*((int)(clock*1.5f)%2)/48f;
                if(distance<range&&DarklightNavigation.Clear(body.position,lab.Player.transform.position)&&timer<=0&&game.RequestAttack(this))
                {state=1;timer=Boss?.5f:kind==RequiemEnemyKind.Hunter?.32f:.46f;locked=delta.normalized;body.linearVelocity=Vector2.zero;}
            }
            else if(state==1)
            {
                body.linearVelocity=Vector2.zero;ShowAttack(timer>(Boss?.25f:.2f)?0:1);art.color=Color.Lerp(Color.white,new Color(1,.15f,.18f),.55f+.4f*Mathf.Sin(clock*22));
                game.Telegraph(transform.position,locked,ranged?1.1f:range,kind==RequiemEnemyKind.Armored||kind==RequiemEnemyKind.Matriarch);
                if(timer<=0){state=2;timer=.25f;serial++;ShowAttack(2);Perform(game,lab,range,ranged);}
            }
            else
            {
                body.linearVelocity=dash;ShowAttack(timer>.13f?2:3);art.color=Color.white;
                if(timer<=0){state=0;dash=Vector2.zero;timer=phaseTwo?.5f:Boss?.7f:.8f;}
            }
        }
        void ShowAttack(int index)
        {
            if(attackFrames?.Length!=4){Show(3);return;}
            art.sprite=attackFrames[index];
            float target=Boss?2.15f:kind==RequiemEnemyKind.Armored?2.1f:1.65f;
            art.transform.localScale=Vector3.one*target/attackFrames[0].bounds.size.y;
        }
        void Show(int index){art.transform.localScale=Vector3.one*(Boss?2.15f:kind==RequiemEnemyKind.Armored?2.1f:1.65f)/poses[0].bounds.size.y;if(poses!=null&&poses.Length>index)art.sprite=poses[index];}
        void Perform(DarklightCampaign game,DarklightCombatLab lab,float range,bool ranged)
        {
            float damage=(Boss?14:kind==RequiemEnemyKind.Armored?15:8)+game.Progress.chapter*1.3f;
            if(elite)damage*=1.3f;game.Sound(2);
            if(ranged)
            {
                int shots=Boss?(phaseTwo?7:3):kind==RequiemEnemyKind.Spectral?3:1;
                for(int i=0;i<shots;i++){float angle=(i-(shots-1)*.5f)*18;Vector2 direction=Quaternion.Euler(0,0,angle)*locked;
                    game.Fire(transform.position+Vector3.up*.35f,direction,damage,phaseTwo?5:3.8f);}
            }
            else
            {
                bool area=kind==RequiemEnemyKind.Armored||kind==RequiemEnemyKind.Matriarch||kind==RequiemEnemyKind.Sentinel;
                Vector2 delta=lab.Player.transform.position-transform.position;
                if(delta.magnitude<range &&(area||Vector2.Angle(locked,delta)<70))
                    lab.Player.GetComponent<DarklightCombatant>().Receive(damage,delta.normalized);
                if(kind==RequiemEnemyKind.Hunter||kind==RequiemEnemyKind.Kael||kind==RequiemEnemyKind.Warden)dash=locked*(phaseTwo?5:3);
                if(Boss&&phaseTwo)
                {
                    if(kind==RequiemEnemyKind.Kael||kind==RequiemEnemyKind.Warden)
                        for(int i=-1;i<=1;i++)game.Fire(transform.position,Quaternion.Euler(0,0,i*24)*locked,damage*.65f,4.7f);
                    if(kind==RequiemEnemyKind.Sentinel||kind==RequiemEnemyKind.Matriarch)
                        for(int i=0;i<8;i++)game.Fire(transform.position,Quaternion.Euler(0,0,i*45)*Vector2.up,damage*.6f,3);
                }
            }
        }
        void Reward(){if(rewarded)return;rewarded=true;DarklightCampaign.Instance?.EnemyDefeated(this);GetComponent<Collider2D>().enabled=false;}
        public bool EvadeStrike()
        {
            if((kind!=RequiemEnemyKind.Kael&&kind!=RequiemEnemyKind.Hunter)||state!=0||serial%3!=1||timer>.3f)return false;
            Vector2 side=(Vector2)(Quaternion.Euler(0,0,90)*((Vector2)DarklightCombatLab.Instance.Player.transform.position-body.position).normalized);
            DarklightNavigation.SafeStep(body,side*.6f);timer=.6f;serial++;return true;
        }
        public void Struck(float force,bool heavy)
        {
            bool armor=kind==RequiemEnemyKind.Armored||kind==RequiemEnemyKind.Sentinel;
            if(!armor||heavy) {stagger=Boss?.12f:.28f;state=0;timer=Mathf.Max(timer,.5f);}
            if(!Boss)DarklightNavigation.SafeStep(body,DarklightCombatLab.Instance.Player.Model.Facing*Mathf.Min(.8f,force));
        }
        public void PullToward(Vector2 point,float distance)
        {
            if(Boss||kind==RequiemEnemyKind.Armored||Health.IsDead)return;
            Vector2 delta=point-body.position;float step=Mathf.Min(distance,Mathf.Max(0,delta.magnitude-1));
            DarklightNavigation.SafeStep(body,delta.normalized*step,.3f);body.linearVelocity=Vector2.zero;stagger=Mathf.Max(stagger,.28f);
        }
        void FixedUpdate(){if(DarklightCombatLab.Instance?.CombatDelta<=0)body.linearVelocity=Vector2.zero;}
    }
    public sealed class DarklightProjectile : MonoBehaviour
    {
        public Vector2 direction;public float damage,speed;float age;
        void Update()
        {
            var lab=DarklightCombatLab.Instance;if(lab==null)return;float dt=lab.CombatDelta*(DarklightCampaign.Instance?.EnemyTimeScale??1);if(dt<=0)return;
            Vector2 origin=transform.position;Vector2 step=direction*speed*dt;
            var hits=Physics2D.RaycastAll(origin,direction,step.magnitude+.12f);
            foreach(var hit in hits)
            {
                var actor=hit.collider.GetComponentInParent<DarklightCombatant>();
                if(actor!=null){if(actor.player){actor.Receive(damage,direction);Destroy(gameObject);return;}continue;}
                if(!hit.collider.isTrigger){Destroy(gameObject);return;}
            }
            transform.position+=(Vector3)step;age+=dt;if(age>4)Destroy(gameObject);
        }
    }
}
