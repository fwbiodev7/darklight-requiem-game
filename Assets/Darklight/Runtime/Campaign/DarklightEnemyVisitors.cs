using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        public DarklightVisitor Visitor {get;private set;}
        Sprite visitorDefeat;float visitorCycle;Vector2 visitorDash;
        public void ConfigureVisitor(DarklightVisitor visitor,Sprite defeated){Visitor=visitor;visitorDefeat=defeated;}
        bool TickVisitor(DarklightCampaign game,DarklightCombatLab lab,float dt)
        {
            if(Visitor==DarklightVisitor.None)return false;
            art.flipX=false;art.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10);
            if(Airborne){body.linearVelocity=Vector2.zero;Show(3);return true;}
            if(stagger>0){body.linearVelocity=Vector2.zero;art.color=new Color(1,.75f,.75f);Show(3);warning?.Hide();return true;}
            Vector2 delta=(Vector2)lab.Player.transform.position-body.position;float distance=delta.magnitude;
            bool moss=Visitor==DarklightVisitor.MossDetonator,neon=Visitor==DarklightVisitor.NeonRevenant;
            float reach=moss?2.6f:neon?4.2f:2.7f;
            if(state==0)
            {
                art.color=Color.white;warning?.Hide();facing=delta.normalized;
                var motion=distance>(moss?1.65f:neon?2.4f:2.1f)?facing*(moss?1.65f:neon?3.1f:2.3f):Vector2.zero;
                body.linearVelocity=DarklightNavigation.SlidingVelocity(body,motion,Time.fixedDeltaTime);
                Show(motion.sqrMagnitude>.01f?1+(int)(clock*7)%2:0);
                if(timer<=0&&distance<reach&&DarklightNavigation.Clear(body.position,lab.Player.transform.position)&&game.RequestAttack(this))
                {
                    state=1;windup=timer=moss?1.0f:neon?.48f:.68f;locked=facing;body.linearVelocity=Vector2.zero;
                    game.EnemyAudioWarning(this);
                    if(!moss&&!neon)guardUntil=clock+.22f;
                }
            }
            else if(state==1)
            {
                body.linearVelocity=Vector2.zero;ShowAttack(0);
                art.color=Color.Lerp(Color.white,moss?new Color(.65f,1,.25f):neon?new Color(.3f,.9f,1):new Color(1,.7f,.4f),.35f*(1-timer/windup));
                if(warning==null){warning=new GameObject("SinalDoViajante").AddComponent<DarklightAttackWarning>();warning.transform.SetParent(transform,false);}
                warning.Show(body.position,locked,reach,moss?DarklightThreat.Area:DarklightThreat.Parryable,1-timer/windup);
                if(timer<=0)
                {
                    warning.Hide();state=2;timer=neon?.24f:.18f;serial++;visitorCycle=0;
                    game.AudioDirector?.EnemyIdentity(kind,moss?"heavy":"attack",transform.position);
                    visitorDash=neon?locked*7:Vector2.zero;ShowAttack(2);
                    if(moss){VisitorContact(lab,2.6f,24,true);DarklightCombatFX.MotorcycleImpact(transform.position,locked,DarklightMotorcycleImpact.Ground);}
                    else if(!neon)VisitorContact(lab,2.7f,19,false);
                }
            }
            else if(state==2)
            {
                ShowAttack(2);art.color=Color.white;
                body.linearVelocity=DarklightNavigation.SlidingVelocity(body,visitorDash,Time.fixedDeltaTime);
                if(neon&&visitorCycle==0&&Vector2.Distance(body.position,lab.Player.transform.position)<1.5f)
                {VisitorContact(lab,1.6f,16,false);visitorCycle=1;}
                if(timer<=0){state=3;timer=moss?1.5f:neon?.8f:1.0f;body.linearVelocity=Vector2.zero;}
            }
            else
            {
                body.linearVelocity=Vector2.zero;ShowAttack(3);art.color=Color.white;
                if(timer<=0)
                {
                    state=0;timer=.3f;
                    if(neon&&serial%2==0)for(int i=-1;i<=1;i++)game.Fire(transform.position,Quaternion.Euler(0,0,i*15)*locked,9,4.2f);
                }
            }
            return true;
        }
        void VisitorContact(DarklightCombatLab lab,float range,float damage,bool area)
        {
            Vector2 delta=(Vector2)lab.Player.transform.position-body.position;
            if(delta.magnitude<=range&&(area||Vector2.Angle(locked,delta)<65)&&DarklightNavigation.Clear(body.position,lab.Player.transform.position))
                lab.Player.GetComponent<DarklightCombatant>().Receive(damage,delta.normalized,area);
        }
    }
}
