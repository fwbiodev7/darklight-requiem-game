using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        public bool Resolved {get;private set;}
        public bool ExpandedBoss=>(int)kind>=11;
        public int Phase {get;private set;}=1;
        float phaseTransition;bool purificationReady;
        public bool PurificationReady=>kind==RequiemEnemyKind.KaelFractured&&purificationReady&&!Resolved;
        public bool AcceptDamage=>!Resolved&&!purificationReady&&phaseTransition<=0;
        public void RestorePhase(int phase)
        {
            if(!ExpandedBoss)return;
            Phase=Mathf.Clamp(phase,1,3);phaseTwo=Phase>1;
            if(Phase>1)Health.TakeDamage(Health.MaxHealth*(Phase==2?.35f:.68f));
            phaseTransition=.85f;
        }
        public float ProtectPurification(float damage)
        {
            if(kind!=RequiemEnemyKind.KaelFractured)return damage;
            float floor=Health.MaxHealth*.08f;
            if(Health.CurrentHealth-damage>floor)return damage;
            purificationReady=true;state=0;timer=1;dash=Vector2.zero;body.linearVelocity=Vector2.zero;warning?.Hide();
            DarklightCampaign.Instance.Notice("KAEL AJOELHOU — APROXIME-SE E INTERAJA");
            return Mathf.Max(0,Health.CurrentHealth-floor);
        }
        public void Purify()
        {
            if(!PurificationReady)return;
            Resolved=true;body.linearVelocity=Vector2.zero;GetComponent<Collider2D>().enabled=false;warning?.Hide();art.color=Color.white;Show(0);
        }
        bool TickExpandedBoss(DarklightCampaign game,float dt)
        {
            if(!ExpandedBoss)return false;
            if(Resolved||purificationReady){body.linearVelocity=Vector2.zero;warning?.Hide();Show(0);return true;}
            phaseTransition=Mathf.Max(0,phaseTransition-dt);
            int wanted=Health.HealthPercent<=.33f?3:Health.HealthPercent<=.66f?2:1;
            if(wanted>Phase)
            {
                Phase++;phaseTwo=true;phaseTransition=.85f;state=0;dash=Vector2.zero;body.linearVelocity=Vector2.zero;timer=1.05f;warning?.Hide();
                game.Progress.bossPhase=Phase;game.SaveBossPhase();
                game.Notice("FASE "+Phase+": "+PhaseName);game.BossPhaseTwo();
                if(kind==RequiemEnemyKind.Nocthar&&Phase==3){DarklightCombatFX.Transformation(transform.position,2,true);Show(0);game.FinalAwakening();}
                return true;
            }
            if(phaseTransition>0){body.linearVelocity=Vector2.zero;Show(0);return true;}
            return false;
        }
        public string PhaseName=>kind==RequiemEnemyKind.Nocthar?new[]{"REI DO VAZIO","ECLIPSE VIVO","RÉQUIEM FINAL"}[Phase-1]:
            kind==RequiemEnemyKind.Valtherion?new[]{"JURAMENTO DE FERRO","SANGUE ANCESTRAL","GUARDIÃO DA LINHAGEM"}[Phase-1]:
            kind==RequiemEnemyKind.Seraphine?new[]{"LÂMINAS DA NOITE","CORTEJO ESPECTRAL","RAINHA CARMESIM"}[Phase-1]:
            new[]{"DUELO FRATURADO","VOZES DO VAZIO","ÚLTIMO VESTÍGIO"}[Phase-1];
        bool ExpandedRanged=>kind==RequiemEnemyKind.Valtherion&&Phase==2||kind==RequiemEnemyKind.KaelFractured&&Phase==2||kind==RequiemEnemyKind.Nocthar&&serial%3==1;
        bool ExpandedArea=>kind==RequiemEnemyKind.Seraphine&&Phase==3||kind==RequiemEnemyKind.Nocthar&&serial%3==2;
        bool expandedShot,expandedArea;
        void PrepareExpandedAttack()
        {expandedShot=ExpandedRanged;expandedArea=ExpandedArea;}
        float ExpandedSpeed=>kind==RequiemEnemyKind.Seraphine?3.5f:Phase==3?3.15f:2.3f;
        float ExpandedWindup=>expandedArea?.9f:kind==RequiemEnemyKind.Valtherion&&Phase==1?.85f:.6f;
        bool PerformExpanded(DarklightCampaign game,DarklightCombatLab lab,float damage,float range)
        {
            if(!ExpandedBoss)return false;
            if(expandedShot)
            {
                int count=Phase==1?3:5;
                for(int i=0;i<count;i++)game.Fire(transform.position+Vector3.up*.35f,Quaternion.Euler(0,0,(i-(count-1)*.5f)*18)*locked,damage*.7f,4.2f);
                if(kind==RequiemEnemyKind.Valtherion||kind==RequiemEnemyKind.KaelFractured)
                {
                    Vector2 side=Quaternion.Euler(0,0,90)*locked;
                    DarklightNavigation.SafeStep(body,side*(serial%2==0?1.6f:-1.6f),.32f);
                    game.SpawnBossHazard(lab.Player.transform.position,1.35f,damage*.65f,.9f);
                }
            }
            else
            {
                Vector2 delta=(Vector2)lab.Player.transform.position-body.position;
                if(delta.magnitude<range&&(expandedArea||Vector2.Angle(locked,delta)<70)&&DarklightNavigation.Clear(body.position,lab.Player.transform.position))
                    lab.Player.GetComponent<DarklightCombatant>().Receive(damage,delta.normalized,expandedArea);
                if(!expandedArea)dash=locked*(kind==RequiemEnemyKind.Seraphine?6:Phase==3?4.5f:3);
                if(kind==RequiemEnemyKind.Valtherion&&Phase==3||kind==RequiemEnemyKind.KaelFractured&&Phase==3)
                    game.SpawnBossHazard(body.position+locked*1.4f,1.4f,damage*.6f,.75f);
            }
            if(kind==RequiemEnemyKind.Seraphine&&Phase>=2&&serial%3==0)game.SpawnBossSupport(body.position);
            if(kind==RequiemEnemyKind.Nocthar&&Phase>=2)
            {
                // Temporary dangerous floors never alter the authored collider geometry or routes.
                game.SpawnBossHazard(lab.Player.transform.position,1.8f,damage*.8f,1.05f);
                if(Phase==3)game.SpawnBossHazard(body.position-locked*2.8f,1.5f,damage*.6f,1.2f);
            }
            return true;
        }
    }
    public sealed class DarklightBossHazard:MonoBehaviour
    {
        Vector2 origin;float radius,damage,delay,elapsed;
        DarklightAttackWarning warning;DarklightEnemy owner;
        public void Initialize(Vector2 position,float size,float hit,float windup,DarklightEnemy boss)
        {origin=position;radius=size;damage=hit;delay=windup;owner=boss;warning=gameObject.AddComponent<DarklightAttackWarning>();warning.Show(origin,Vector2.up,radius,DarklightThreat.Area,0);}
        void Update()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            if(game==null||lab==null||owner==null||owner.Health.IsDead||owner.Resolved){Destroy(gameObject);return;}
            float dt=lab.CombatDelta*game.EnemyTimeScale;if(dt<=0)return;
            elapsed+=dt;warning.Show(origin,Vector2.up,radius,DarklightThreat.Area,elapsed/delay);
            if(elapsed<delay)return;
            Vector2 delta=(Vector2)lab.Player.transform.position-origin;
            if(delta.magnitude<=radius&&DarklightNavigation.Clear(origin,lab.Player.transform.position))lab.Player.GetComponent<DarklightCombatant>().Receive(damage,delta.normalized,true);
            Destroy(gameObject);
        }
    }
    public sealed partial class DarklightCampaign
    {
        internal void SaveBossPhase()=>SaveCheckpoint();
        internal void SpawnBossHazard(Vector2 origin,float radius,float damage,float windup)
        {
            var boss=CurrentBoss;if(boss==null)return;
            var obj=new GameObject("ZonaDoChefe");obj.transform.SetParent(levelRoot,false);
            obj.AddComponent<DarklightBossHazard>().Initialize(origin,radius,damage,windup,boss);
        }
    }
}
