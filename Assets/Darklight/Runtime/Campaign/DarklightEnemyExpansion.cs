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
        public bool AcceptDamage=>!Resolved&&!purificationReady&&phaseTransition<=0&&luciusTransformationTime<=0;
        public void RestorePhase(int phase)
        {
            if(!ExpandedBoss)return;
            Phase=Mathf.Clamp(phase,1,3);phaseTwo=Phase>1;
            if(Phase>1)Health.TakeDamage(Health.MaxHealth*(Phase==2?.35f:.68f));
            phaseTransition=.85f;
        }
        public float ProtectPurification(float damage)
        {
            var campaign=DarklightCampaign.Instance;
            if(IsLucius && campaign!=null && campaign.Progress!=null && campaign.Progress.luciusStance>0 &&
                Health.CurrentHealth-damage<=1f)
            {
                Resolved=true;
                body.linearVelocity=Vector2.zero;
                warning?.Hide();
                GetComponent<Collider2D>().enabled=false;
                LuciusView?.PresentPose("Hurt",DarklightCharacterView.DirectionIndex(facing),0);
                campaign.FinishLuciusDuel(this);
                return Mathf.Max(0,Health.CurrentHealth-1f);
            }
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
            if(phaseTransition>0){HoldExpandedBossTransition();return true;}
            int wanted=Health.HealthPercent<=.33f?3:Health.HealthPercent<=.66f?2:1;
            if(wanted>Phase)
            {
                Phase++;phaseTwo=true;phaseTransition=.85f;timer=1.05f;
                HoldExpandedBossTransition();
                game.Progress.bossPhase=Phase;game.SaveBossPhase();
                game.Notice("FASE "+Phase+": "+PhaseName);game.BossPhaseTwo();
                if(kind==RequiemEnemyKind.Nocthar&&Phase==3){DarklightCombatFX.Transformation(transform.position,2,true);game.FinalAwakening();}
                return true;
            }
            return false;
        }
        void HoldExpandedBossTransition()
        {
            StopEnemyAttack();
            ResetEnemyStride();
            damageTintUntil=0;
            art.color=Color.white;
            Show(0);
        }
        public string PhaseName=>kind==RequiemEnemyKind.Nocthar?new[]{"REI DO VAZIO","ECLIPSE VIVO","RÉQUIEM FINAL"}[Phase-1]:
            kind==RequiemEnemyKind.Valtherion?new[]{"JURAMENTO DE FERRO","SANGUE ANCESTRAL","GUARDIÃO DA LINHAGEM"}[Phase-1]:
            kind==RequiemEnemyKind.Seraphine?new[]{"LÂMINAS DA NOITE","CORTEJO ESPECTRAL","RAINHA CARMESIM"}[Phase-1]:
            new[]{"DUELO FRATURADO","VOZES DO VAZIO","ÚLTIMO VESTÍGIO"}[Phase-1];
        bool ExpandedRanged=>kind==RequiemEnemyKind.Valtherion&&Phase==2||kind==RequiemEnemyKind.KaelFractured&&Phase==2||kind==RequiemEnemyKind.Nocthar&&serial%3==1;
        bool ExpandedArea=>kind==RequiemEnemyKind.Seraphine&&Phase==3||kind==RequiemEnemyKind.Nocthar&&serial%3==2;
        float ExpandedSpeed=>kind==RequiemEnemyKind.Seraphine?3.5f:Phase==3?3.15f:2.3f;
        void PerformExpanded(DarklightCampaign game,DarklightCombatLab lab,float damage)
        {
            if(committedThreat==DarklightThreat.Projectile)
            {
                FireEnemySpread(game,damage*.7f,4.2f);
                if(kind==RequiemEnemyKind.Valtherion||kind==RequiemEnemyKind.KaelFractured)
                {
                    Vector2 side=Quaternion.Euler(0,0,90)*locked;
                    DarklightNavigation.SafeStep(body,side*(serial%2==0?1.6f:-1.6f),.32f);
                    game.SpawnBossHazard(this,lab.Player.transform.position,1.35f,damage*.65f,.9f);
                }
            }
            else
            {
                TryEnemyMelee(lab,damage);
                if(!AreaAttack)dash=locked*(kind==RequiemEnemyKind.Seraphine?6:Phase==3?4.5f:3);
                if(kind==RequiemEnemyKind.Valtherion&&Phase==3||kind==RequiemEnemyKind.KaelFractured&&Phase==3)
                    game.SpawnBossHazard(this,body.position+locked*1.4f,1.4f,damage*.6f,.75f);
            }
            if(kind==RequiemEnemyKind.Seraphine&&Phase>=2&&serial%3==0)game.SpawnBossSupport(body.position);
            if(kind==RequiemEnemyKind.Nocthar&&Phase>=2)
            {
                const float targetRadius=1.8f, flankRadius=1.5f;
                Vector2 target=lab.Player.transform.position;
                game.SpawnBossHazard(this,target,targetRadius,damage*.8f,1.05f);
                Vector2 flank=body.position-locked*2.8f;
                float combinedRadius=targetRadius+flankRadius;
                if(Phase==3&&(flank-target).sqrMagnitude>=combinedRadius*combinedRadius)
                    game.SpawnBossHazard(this,flank,flankRadius,damage*.6f,1.2f);
            }
        }
    }
    public sealed class DarklightBossHazard:MonoBehaviour
    {
        Vector2 origin;float radius,damage,delay,elapsed;
        DarklightAttackWarning warning;DarklightEnemy owner;int ownerPhase;
        public void Initialize(Vector2 position,float size,float hit,float windup,DarklightEnemy boss)
        {origin=position;radius=size;damage=hit;delay=windup;owner=boss;ownerPhase=boss.Phase;warning=gameObject.AddComponent<DarklightAttackWarning>();warning.Show(origin,Vector2.up,radius,DarklightThreat.Area,0);}
        void Update()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            if(game==null||lab==null||owner==null||owner.Health.IsDead||!owner.AcceptDamage||owner.Phase!=ownerPhase){Destroy(gameObject);return;}
            var player=lab.Player;
            if(player==null||player.Model.State==CombatState.Dead){Destroy(gameObject);return;}
            float dt=lab.CombatDelta*game.EnemyTimeScale;if(dt<=0)return;
            elapsed+=dt;warning.Show(origin,Vector2.up,radius,DarklightThreat.Area,elapsed/delay);
            if(elapsed<delay)return;
            Vector2 delta=(Vector2)player.transform.position-origin;
            if(delta.magnitude<=radius&&DarklightNavigation.Clear(origin,player.transform.position))player.GetComponent<DarklightCombatant>().Receive(damage,delta.normalized,true);
            Destroy(gameObject);
        }
    }
    public sealed partial class DarklightCampaign
    {
        internal void SaveBossPhase()=>SaveCheckpoint();
        internal void SpawnBossHazard(DarklightEnemy boss,Vector2 origin,float radius,float damage,float windup)
        {
            if(boss==null||boss.Health.IsDead||!boss.AcceptDamage)return;
            var obj=new GameObject("ZonaDoChefe");obj.transform.SetParent(levelRoot,false);
            obj.AddComponent<DarklightBossHazard>().Initialize(origin,radius,damage,windup,boss);
        }
    }
}
