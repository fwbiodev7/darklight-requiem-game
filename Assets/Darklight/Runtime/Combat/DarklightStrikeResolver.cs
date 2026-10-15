using UnityEngine;
namespace Darklight
{
    public static class DarklightStrikeResolver
    {
        static readonly RaycastHit2D[] blockers=new RaycastHit2D[32];
        public static void Resolve(DarklightFighter fighter,Rigidbody2D body,Collider2D[] contacts)
        {
            var model=fighter.Model;var attack=model.Attack;var game=DarklightCampaign.Instance;
            float extra=game?.StrikeRangeBonus??0;
            int count=Physics2D.OverlapCircle(body.position,attack.range+extra,new ContactFilter2D{useTriggers=true,useLayerMask=false},contacts);
            float hitstop=0;
            for(int i=0;i<count;i++)
            {
                var target=contacts[i].GetComponentInParent<DarklightCombatant>();
                if(target==null||target.player||target.Health.IsDead)continue;
                Vector2 point=contacts[i].ClosestPoint(body.position);
                if(!DarklightHitGeometry.Contains(attack,body.position,model.Facing,point,extra,model.ContactProgress,model.ContactStartProgress))continue;
                Vector2 delta=point-body.position;
                int walls=Physics2D.Raycast(body.position,delta.normalized,new ContactFilter2D{useTriggers=false,useLayerMask=false},blockers,delta.magnitude);
                bool blocked=walls>=blockers.Length;
                for(int b=0;b<walls;b++)if(blockers[b].collider.GetComponentInParent<DarklightCombatant>()==null&&blockers[b].collider.gameObject!=fighter.gameObject){blocked=true;break;}
                if(blocked)continue;
                for(int pulse=0;pulse<Mathf.Clamp(attack.hitPulses,1,6);pulse++)
                {
                    if(!model.RegisterPulseHit(target,pulse))continue;
                    float damage=model.AttackDamage*(game?.DamageMultiplier??1)*(game?.VampireDamage(attack.kind)??1);
                    if(!target.Receive(damage,delta.normalized*attack.knockback))continue;
                    model.ConfirmHit();
                    DarklightCombatLab.Instance.Coach?.Record(DarklightLesson.Strike);
                    hitstop=Mathf.Max(hitstop,attack.hitPulses>1?.012f:attack.hitstop+(game?.Form==1?.008f:0));
                    game?.WeaponHit(target,attack,model.AttackSerial);
                    var enemy=target.GetComponent<DarklightEnemy>();
                    enemy?.Struck(attack.knockback*(game?.Resonance.Impact??1),attack.kind!=StrikeKind.Light);
                    enemy?.TechnicalImpact(attack,game?.Form??0);
                    if(attack.pullTarget)enemy?.PullToward(body.position,game?.Form==2?2.1f:1.6f);
                    DarklightCombatFX.Impact(target.transform.position,attack.kind!=StrikeKind.Light);
                }
            }
            if(hitstop>0)DarklightCombatLab.Instance.Hitstop(Mathf.Min(.065f,hitstop));
        }
    }
}
