using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightAbyssRider
    {
        AttackDefinition vertical,brutal,motorExecution;
        public AttackDefinition[] ExpandedMoves {get;private set;}
        public bool AerialExecution=>Active&&fighter.Model.State==CombatState.Attack&&fighter.Model.Attack==motorExecution;
        bool RidingAttack(AttackDefinition a)=>a==dash||a==brutal;
        bool GroundSlam(AttackDefinition a)=>a==slam||a==vertical;
        void ExpandMoves(CombatLoadout l)
        {
            var saw=Make("Serra Infernal",StrikeKind.Light,.22f,.56f,.36f,5,1.8f,115,4,1);
            saw.airReaction=DarklightAirReaction.Launch;saw.launchSpeed=3.8f;saw.knockback=.06f;
            var spin=Make("Giro Devastador",StrikeKind.Heavy,.32f,.42f,.46f,8,2.2f,360,3,1.5f);
            spin.hitShape=HitShape.Sweep;spin.knockback=.8f;spin.finisher=true;
            vertical=Make("Esmagamento Vertical",StrikeKind.Skill,.58f,.16f,.58f,40,2.35f,360,1,5);
            vertical.cooldown=4.4f;vertical.knockback=.85f;vertical.airReaction=DarklightAirReaction.Slam;
            var wheel=Make("Roda Destruidora",StrikeKind.Heavy,.24f,.48f,.4f,6,1.85f,120,4,1);
            wheel.lungeSpeed=1.3f;wheel.knockback=.06f;wheel.airReaction=DarklightAirReaction.Launch;wheel.launchSpeed=4.4f;
            brutal=Make("Cavalgada Brutal",StrikeKind.WeaponSpecial,.32f,.46f,.54f,10,1.85f,125,3,1.4f);
            brutal.lungeSpeed=1.7f;brutal.knockback=.5f;brutal.cooldown=5;
            motorExecution=Make("Execução Aérea Motorizada",StrikeKind.Heavy,.16f,.3f,.4f,10,2.1f,150,4,1.4f);
            motorExecution.airReaction=DarklightAirReaction.Slam;motorExecution.finisher=true;motorExecution.knockback=.03f;
            ExpandedMoves=new[]{saw,spin,vertical,wheel,brutal,motorExecution};
            // Both original branches remain reachable in the expanded light sequence.
            var lights=new AttackDefinition[l.lights.Length+4];System.Array.Copy(l.lights,lights,l.lights.Length);
            int n=l.lights.Length;lights[n]=l.pauseBranch;lights[n+1]=l.heavyBranch;lights[n+2]=saw;lights[n+3]=spin;
            l.lights=lights;l.heavyBranch=wheel;l.skill=vertical;l.airFinisher=motorExecution;
        }
    }
}
