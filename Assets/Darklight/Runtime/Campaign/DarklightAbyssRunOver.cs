using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightAbyssRider
    {
        DarklightEnemy runOverTarget;
        Collider2D runOverCollider;
        float runOverContactTime;
        bool runOverPairIgnored;
        public bool RunOverConfirmed=>Dashing&&fighter.Model.Attack==dash&&runOverTarget!=null;
        public Vector2 RunOverImpactPoint{get;private set;}
        public bool RunOverCleared=>RunOverConfirmed&&Vector2.Dot((Vector2)capsule.bounds.center-(Vector2)runOverCollider.bounds.center,dashFacing)>RunOverExtent()+.035f;
        float RunOverExtent()
        {
            Vector2 ext=capsule.bounds.extents+runOverCollider.bounds.extents;
            return Mathf.Abs(dashFacing.x)*ext.x+Mathf.Abs(dashFacing.y)*ext.y;
        }
        // A short crest over the struck target, then the existing authored split/vault.
        // The physical capsule still sweeps against scenery and every other enemy.
        public float RunOverLift=>RunOverConfirmed&&fighter.Model.ActiveThisTick?
            Mathf.Sin(Mathf.Clamp01((fighter.Model.StateTime-runOverContactTime)/Mathf.Max(.03f,dash.startup+dash.active-runOverContactTime))*Mathf.PI)*.16f:0;
        public float RunOverPitch=>RunOverLift*55*(dashFacing.x>.2f?1:dashFacing.x<-.2f?-1:0);
        public void ConfirmRunOverHit(DarklightEnemy enemy,AttackDefinition attack)
        {
            if(!Active||attack!=dash||runOverTarget!=null||DashBlocked||enemy==null||enemy.Boss||enemy.Health.IsDead||!enemy.Vulnerable||enemy.kind==RequiemEnemyKind.Armored||enemy.kind==RequiemEnemyKind.Sentinel)return;
            var target=enemy.GetComponent<Collider2D>();
            if(target==null)return;
            runOverCollider=target;
            float exit=Vector2.Dot((Vector2)target.bounds.center-(Vector2)capsule.bounds.center,dashFacing)+RunOverExtent()+.09f;
            // Only start when there is enough bounded travel to land beyond the target.
            if(exit<0||exit>MaximumDashDistance-DashDistance){runOverCollider=null;return;}
            int count=capsule.Cast(dashFacing,new ContactFilter2D{useTriggers=true,useLayerMask=false},hits,exit);
            if(count>=hits.Length){runOverCollider=null;return;}
            for(int i=0;i<count;i++)
            {
                var col=hits[i].collider;if(col==null||col==capsule||col==target)continue;
                var actor=col.GetComponentInParent<DarklightCombatant>();
                if(actor!=null&&(actor.player||actor.Health.IsDead))continue;
                if(actor==null&&col.isTrigger)continue;
                runOverCollider=null;return; // No safe landing beyond the struck target.
            }
            runOverTarget=enemy;runOverContactTime=fighter.Model.StateTime;
            RunOverImpactPoint=enemy.transform.position;
            enemy.Control(.65f);
            // Only this confirmed victim, only until the rear wheel clears it.
            Physics2D.IgnoreCollision(capsule,target,true);runOverPairIgnored=true;
            DarklightCombatFX.MotorcycleImpact(enemy.transform.position,dashFacing,DarklightMotorcycleImpact.RunOver);
        }
        bool CanPassRunOverTarget(DarklightCombatant actor)=>RunOverConfirmed&&actor.gameObject==runOverTarget.gameObject&&runOverTarget.Vulnerable;
        void RestoreRunOverCollision(){if(runOverPairIgnored&&capsule!=null&&runOverCollider!=null)Physics2D.IgnoreCollision(capsule,runOverCollider,false);runOverPairIgnored=false;}
        void LateUpdate(){if(runOverPairIgnored&&(!Dashing||DashBlocked||RunOverCleared||!runOverTarget.Vulnerable))RestoreRunOverCollision();}
        void OnDisable(){RestoreRunOverCollision();}
        void ResetRunOver(){RestoreRunOverCollision();runOverTarget=null;runOverCollider=null;runOverContactTime=0;RunOverImpactPoint=Vector2.zero;}
    }
}
