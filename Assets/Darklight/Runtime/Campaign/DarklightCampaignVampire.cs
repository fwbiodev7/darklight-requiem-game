using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public DarklightBlood Blood {get;}=new DarklightBlood();
        public bool VampireCasting=>vampireCast>=0;
        public float VampireCastProgress=>Mathf.Clamp01(vampireCastTime/.5f);
        public bool ShadowInvulnerable=>shadowProtection>0;
        int vampireCast=-1,vampirePulses;float vampireCastTime,shadowProtection;
        readonly Collider2D[] vampireContacts=new Collider2D[128];
        readonly HashSet<DarklightCombatant> vampireHit=new();
        public string VampireStatus=>DarklightProgress.MasteryNames[Progress.vampireMastery]+"\n"+DarklightVampireSkills.Names[Progress.vampireSelected]+" | "+Blood.Recovery[Progress.vampireSelected].ToString("0.0")+"s";
        public void SelectVampireSkill(int skill){if(!DarklightVampireSkills.Unlocked(Progress,skill))return;Progress.vampireSelected=skill;SaveCheckpoint();}
        public bool TryVampireSkill()
        {
            if(!Active||MenuDismissedThisFrame||WearingHeadphones||VampireCasting||health.IsDead)return false;
            var model=lab.Player.Model;if(model.State==CombatState.Hurt||model.State==CombatState.Dead||model.State==CombatState.Dodge||model.State==CombatState.Parry||model.State==CombatState.Charging)return false;
            if(model.State==CombatState.Attack&&model.StateTime<model.Attack.startup+model.Attack.active)return false;
            int skill=Progress.vampireSelected;
            if(!Blood.Activate(skill,Progress)){Notice("PODER BLOQUEADO, SEM SANGUE OU EM RECARGA");return false;}
            model.SuspendInput();vampireCast=skill;vampireCastTime=0;vampirePulses=0;Notice(DarklightVampireSkills.Names[skill]);return true;
        }
        void TickVampire(float dt)
        {
            Blood.Tick(dt);shadowProtection=Mathf.Max(0,shadowProtection-dt);
            if(lab.Input.Action("VampireNext").WasPressedThisFrame())
                for(int n=1;n<=6;n++){int candidate=(Progress.vampireSelected+n)%6;if(DarklightVampireSkills.Unlocked(Progress,candidate)){Progress.vampireSelected=candidate;Notice(DarklightVampireSkills.Names[candidate]);break;}}
            if(lab.Input.Action("Vampire").WasPressedThisFrame())TryVampireSkill();
            if(!VampireCasting)return;
            if(lab.Player.Model.State==CombatState.Hurt||health.IsDead){CancelVampireCast();return;}
            vampireCastTime+=dt;
            if(vampireCast==(int)VampireAbility.Claws)
            {
                while(vampirePulses<3&&vampireCastTime>=.12f+vampirePulses*.07f){VampireHit(1.75f,90,7,false);vampirePulses++;}
            }
            else if(vampirePulses==0&&vampireCastTime>=.18f)
            {
                vampirePulses++;
                switch((VampireAbility)vampireCast)
                {
                    case VampireAbility.ShadowStep:
                        DarklightNavigation.SafeStep(lab.Player.GetComponent<Rigidbody2D>(),lab.Player.Model.Facing*2.3f,.3f);shadowProtection=.16f;
                        DarklightCombatFX.Echo(lab.Player.GetComponentInChildren<SpriteRenderer>(),.15f);break;
                    case VampireAbility.Embrace: VampireHit(3.4f,360,5,true);break;
                    case VampireAbility.Judgment: VampireHit(4.2f,100,40,false);break;
                }
            }
            if(vampireCastTime>=(vampireCast==(int)VampireAbility.Judgment?.75f:.5f))CancelVampireCast();
        }
        void VampireHit(float range,float angle,float damage,bool pull)
        {
            var origin=(Vector2)lab.Player.transform.position;vampireHit.Clear();
            int count=Physics2D.OverlapCircle(origin,range,new ContactFilter2D{useTriggers=true},vampireContacts);
            for(int i=0;i<count;i++)
            {
                var target=vampireContacts[i].GetComponentInParent<DarklightCombatant>();
                if(target==null||target.player||target.Health.IsDead||!vampireHit.Add(target))continue;
                Vector2 delta=(Vector2)target.transform.position-origin;
                if(Vector2.Angle(lab.Player.Model.Facing,delta)>angle*.5f||!DarklightNavigation.Clear(origin,target.transform.position,.05f))continue;
                if(!target.Receive(damage*DamageMultiplier,delta.normalized))continue;
                var enemy=target.GetComponent<DarklightEnemy>();
                if(pull){enemy?.PullToward(origin,1.2f);enemy?.Control(.6f);}else enemy?.Struck(.2f,true);
                DarklightCombatFX.Impact(target.transform.position,vampireCast==(int)VampireAbility.Judgment);
                // Ability damage deliberately generates neither blood nor resonance energy.
            }
            Sound(1);
        }
        void CancelVampireCast(){vampireCast=-1;vampireCastTime=0;}
        void RestoreVampire(){Blood.Restore(Progress);CancelVampireCast();shadowProtection=0;}
        void StopVampire(){CancelVampireCast();shadowProtection=0;Blood.StopTemporary();}
        public void WeaponHit(DarklightCombatant target,AttackDefinition attack,int serial)
        {
            AudioDirector?.Contact(target,attack);Strike(attack.kind);
            if(Blood.ThirstRemaining>0&&(attack.kind==StrikeKind.Counter||attack.kind==StrikeKind.Charged))
                health.Heal(Blood.AllowHealing(health.MaxHealth*.012f,health.MaxHealth));
            var enemy=target.GetComponent<DarklightEnemy>();
            if((Progress.progressionNodes&2)!=0&&enemy!=null&&!enemy.Boss&&!target.Health.IsDead&&target.Health.HealthPercent<.18f&&enemy.Vulnerable&&(attack.kind==StrikeKind.Counter||attack.kind==StrikeKind.Charged))
            {
                if(target.Receive(target.Health.CurrentHealth+1,lab.Player.Model.Facing))
                {Blood.Gain(6);style=Mathf.Min(450,style+20);if(Blood.ThirstRemaining>0)health.Heal(Blood.AllowHealing(health.MaxHealth*.02f,health.MaxHealth));AudioDirector?.Play("impact-impactPunch_heavy",DarklightAudioCategory.Combat,.48f,target.transform.position,.8f);Notice("FINALIZADOR");}
            }
        }
        public float VampireDamage(StrikeKind kind)=>kind==StrikeKind.Counter&&(Progress.progressionNodes&1)!=0?1.2f:1;
        public float ProtectAwakening(float damage)
        {
            if(Progress.chapter!=0||Progress.awakeningState!=0||CurrentBoss==null||CurrentBoss.kind!=RequiemEnemyKind.Kael||health.CurrentHealth-damage>health.MaxHealth*.25f)return damage;
            Progress.awakeningState=1;Progress.CompleteMilestone(0,1);Progress.vampireSkills|=1;Progress.formsUnlocked=Mathf.Max(Progress.formsUnlocked,1);
            Form=1;FormTime=8;Blood.Set(Mathf.Max(Blood.Amount,40));health.Heal(health.MaxHealth*.18f);Notice("SANGUE DESPERTO — H / RS CIMA: GARRAS");AudioDirector?.Awakening();SaveCheckpoint();
            return Mathf.Min(damage,Mathf.Max(0,health.CurrentHealth-health.MaxHealth*.15f));
        }
        void VampireChapterReward()
        {
            if(Progress.chapter==0){Progress.awakeningState=2;Progress.CompleteMilestone(0,1);Progress.vampireSkills|=1;}
            if(Progress.chapter==3)Progress.CompleteMilestone(3,2);
            if(Progress.chapter==8)Progress.CompleteMilestone(8,3);
        }
        void BuildVampireSkills()
        {
            heading.text="CAMINHO DO SANGUE";bodyText.text=VampireStatus+"\nPONTOS: "+Progress.points+"\nH / RS CIMA: ativar\nK / RS BAIXO: escolher\n\n"+DarklightVampireSkills.Descriptions[Progress.vampireSelected]+"\nCusto: "+DarklightVampireSkills.Costs[Progress.vampireSelected]+" sangue\nRecarga: "+DarklightVampireSkills.Cooldowns[Progress.vampireSelected]+"s\nCura conjunta limitada a 3%/s.";
            for(int i=0;i<6;i++)
            {
                int skill=i;bool known=DarklightVampireSkills.Unlocked(Progress,i);
                Button(DarklightVampireSkills.Names[i]+(known?(Progress.vampireSelected==i?" [ATIVA]":""):" [1 PT]"),()=>{if(!known)Progress.LearnVampireSkill(skill);else Progress.vampireSelected=skill;SaveCheckpoint();RefreshPanel();},known||Progress.points>0&&Progress.vampireMastery>=DarklightVampireSkills.Mastery[i]);
            }
            Button("VOLTAR",()=>Open("Skills"));
        }
        void BuildSkillPaths()
        {
            heading.text="HABILIDADES";bodyText.text="PONTOS: "+Progress.points+"\n\nLAMINA: contra-ataques e finalizadores.\nSANGUE: habilidades conquistadas na historia.\nVEU: controle e duracao das formas.\n\n"+DarklightProgress.MasteryNames[Progress.vampireMastery]+"\nA idade visual nao concede poderes.";
            Button("LAMINA: PRECISAO [1 PT]",()=>LearnNode(0),(Progress.progressionNodes&1)==0&&Progress.points>0);
            Button("LAMINA: FINALIZAR [1 PT]",()=>LearnNode(1),(Progress.progressionNodes&2)==0&&Progress.points>0&&(Progress.progressionNodes&1)!=0);
            Button("SANGUE: HABILIDADES",()=>Open("VampireSkills"));
            Button("VEU: DOMINIO "+Progress.mastery+"/5",()=>Upgrade(2),Progress.points>0&&Progress.mastery<5);
            Button("LAMINA: FORCA "+Progress.attack+"/5",()=>Upgrade(0),Progress.points>0&&Progress.attack<5);
            Button("SANGUE: VIGOR "+Progress.vitality+"/5",()=>Upgrade(1),Progress.points>0&&Progress.vitality<5);
            Button("FORMA: "+new[]{"AUTOMATICA","RUPTURA","ECLIPSE","ABSOLUTO"}[Progress.preferredForm],()=>{Progress.preferredForm=(Progress.preferredForm+1)%(Progress.FormUnlocked+1);SaveCheckpoint();RefreshPanel();},Progress.FormUnlocked>0);
            Button("VOLTAR",()=>Open("Pause"));
        }
        void LearnNode(int node){if(node<0||node>1||Progress.points<1||(Progress.progressionNodes&(1<<node))!=0)return;Progress.points--;Progress.progressionNodes|=1<<node;SaveCheckpoint();RefreshPanel();}
    }
}
