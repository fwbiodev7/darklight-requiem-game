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
                if(target==null||target.player||target.Health.IsDead||game?.AbyssRider?.AcceptStrikeTarget(target)==false)continue;
                var enemy=target.GetComponent<DarklightEnemy>();
                if(fighter.AirCombo?.AcceptTarget(enemy)==false)continue;
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
                    bool finalPulse=pulse==Mathf.Clamp(attack.hitPulses,1,6)-1;
                    float force=finalPulse&&attack.finalKnockback>0?attack.finalKnockback:attack.knockback;
                    float damage=model.AttackDamage*(game?.AbyssRider?.Damage??1)*(game?.DamageMultiplier??1)*(game?.VampireDamage(attack.kind)??1);
                    if(!target.Receive(damage,delta.normalized*force))continue;
                    model.ConfirmHit();
                    fighter.GetComponent<DarklightCharacterView>()?.ConfirmContactPose(model.Facing);
                    DarklightCombatLab.Instance?.WorldCamera.GetComponent<DarklightLabCamera>()?.ConfirmContact(point);
                    DarklightCombatLab.Instance?.Coach?.Record(DarklightLesson.Strike);
                    hitstop=Mathf.Max(hitstop,attack.hitPulses>1?.012f:attack.hitstop+(game?.Form==1?.008f:0));
                    game?.WeaponHit(target,attack,model.AttackSerial);
                    enemy?.Struck(force*(game?.Resonance.Impact??1),attack.kind!=StrikeKind.Light);
                    enemy?.TechnicalImpact(attack,game?.Form??0);
                    var reaction=attack.airReaction!=DarklightAirReaction.None?attack.airReaction:attack.launch?DarklightAirReaction.Launch:enemy?.Airborne==true?DarklightAirReaction.Suspend:DarklightAirReaction.None;
                    if(!finalPulse&&(reaction==DarklightAirReaction.Slam||reaction==DarklightAirReaction.Throw||reaction==DarklightAirReaction.Launch))reaction=DarklightAirReaction.Suspend;
                    if(reaction!=DarklightAirReaction.Throw&&reaction!=DarklightAirReaction.Slam||finalPulse)
                    {enemy?.ReactAir(attack,reaction);fighter.AirCombo?.ConfirmContact(enemy,attack);}
                    game?.AbyssRider?.ConfirmRunOverHit(enemy,attack);
                    if(attack.pullTarget)enemy?.PullToward(body.position,game?.Form==2?2.1f:1.6f);
                    DarklightCombatFX.Impact((Vector3)point+Vector3.up*(enemy?.AirHeight??0),attack.kind!=StrikeKind.Light);
                }
            }
            if(hitstop>0)DarklightCombatLab.Instance.Hitstop(Mathf.Min(.065f,hitstop));
        }
    }
}
