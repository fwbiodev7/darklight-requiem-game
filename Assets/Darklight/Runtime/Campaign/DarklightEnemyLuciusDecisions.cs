using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        int luciusReadSerial=-1,luciusPreviousPattern=-1;
        CombatState luciusReadState;
        float luciusReadAt=-1,luciusPunishUntil;
        bool luciusAnsweredThreat;

        bool ReadLuciusThreat(CombatModel target,float distance,Vector2 towardPlayer,bool sight,float reaction)
        {
            float reach=target?.State==CombatState.Charging?(target.Loadout?.charged?.range??2f):(target?.Attack?.range??2f);
            bool visible=target!=null&&sight&&
                (target.State==CombatState.Attack||target.State==CombatState.Charging)&&
                distance<reach+.45f&&
                Vector2.Angle(target.Facing,-towardPlayer)<=85;
            if(!visible){luciusReadAt=-1;luciusReadSerial=-1;luciusAnsweredThreat=false;return false;}
            if(luciusReadAt<0||luciusReadSerial!=target.AttackSerial||luciusReadState!=target.State)
            {
                luciusReadSerial=target.AttackSerial;luciusReadState=target.State;
                luciusReadAt=clock+Mathf.Max(.12f,reaction*(phaseTwo?.85f:1));luciusAnsweredThreat=false;
            }
            return !luciusAnsweredThreat&&clock>=luciusReadAt&&clock>=luciusPunishUntil;
        }

        int ChooseLuciusPattern(float distance,CombatModel target)
        {
            var attacks=CombatModel?.Loadout?.lights;
            if(attacks==null||attacks.Length<3)return 0;
            bool recovery=target?.State==CombatState.Attack&&target.Attack!=null&&
                target.StateTime>=target.Attack.startup+target.Attack.active;
            int preferred=distance>attacks[0].range*.9f||recovery?2:
                distance<attacks[1].range*.7f?0:luciusAttackStep;
            // Vary the existing choreographies, but never force an out-of-range
            // attack simply to avoid a repeat. All attacks still lock their aim.
            for(int pass=0;pass<2;pass++)for(int i=0;i<3;i++)
            {
                int candidate=(preferred+i)%3;
                if(distance>attacks[candidate].range+.05f||pass==0&&candidate==luciusPreviousPattern)continue;
                return candidate;
            }
            return -1;
        }
        bool TryLuciusDecisionAttack(DarklightCampaign game,DarklightFighter target,float distance,Vector2 aim)
        {
            if(timer>0||clock<luciusPunishUntil)return false;
            int pattern=ChooseLuciusPattern(distance,target.Model);
            if(pattern<0||game!=null&&!game.RequestAttack(this))return false;
            ExecuteLuciusAttack(pattern,aim,game);luciusPreviousPattern=pattern;
            luciusAttackStep=(pattern+1)%3;return true;
        }
        void FinishLuciusExchange(DarklightCampaign game)
        {
            bool connected=CombatModel.AttackConnected;
            float opening=connected ? .10f : LuciusFinalExchange ? .22f : phaseTwo ? .25f : .30f;
            luciusPunishUntil=clock+opening;
            timer=Mathf.Max(opening,(LuciusFinalExchange?(luciusAttackStep==0?.75f:.15f):phaseTwo?.35f:.55f)*
                (game?.DifficultyProfile.cooldown??1f));
        }
    }
}
