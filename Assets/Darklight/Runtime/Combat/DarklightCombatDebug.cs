using UnityEngine;
namespace Darklight
{
    // Runtime line overlays are available in the playable training arena, not only in the Editor.
    public sealed class DarklightCombatDebug:MonoBehaviour
    {
        LineRenderer line;Vector3[] points=new Vector3[35];DarklightFighter fighter;DarklightCombatant actor;Material owned;
        void Awake()
        {
            fighter=GetComponent<DarklightFighter>();actor=GetComponent<DarklightCombatant>();
            line=new GameObject("HitboxTreino").AddComponent<LineRenderer>();line.transform.SetParent(transform,false);
            owned=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")??Shader.Find("Sprites/Default"));line.sharedMaterial=owned;
            line.useWorldSpace=true;line.widthMultiplier=.025f;line.sortingOrder=1000;line.loop=false;
        }
        void LateUpdate()
        {
            var game=DarklightCampaign.Instance;bool visible=game!=null&&game.Training&&game.TrainingHitboxes;
            line.enabled=visible;if(!visible)return;
            var center=(Vector2)transform.position;
            if(fighter!=null&&fighter.Model.State==CombatState.Attack)
            {
                var attack=fighter.Model.Attack;float range=attack.range+game.StrikeRangeBonus;
                if(attack.hitShape==HitShape.Thrust)
                {
                    var forward=fighter.Model.Facing.normalized;var side=new Vector2(-forward.y,forward.x)*attack.width*.5f;
                    line.positionCount=5;line.SetPosition(0,center-side);line.SetPosition(1,center+forward*range-side);line.SetPosition(2,center+forward*range+side);line.SetPosition(3,center+side);line.SetPosition(4,center-side);
                    line.startColor=line.endColor=fighter.Model.ActiveThisTick?Color.red:Color.yellow;return;
                }
                float facing=Mathf.Atan2(fighter.Model.Facing.y,fighter.Model.Facing.x)*Mathf.Rad2Deg;
                points[0]=center;
                for(int i=0;i<=32;i++){float a=(facing-attack.angle*.5f+attack.angle*i/32)*Mathf.Deg2Rad;points[i+1]=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*range;}
                points[34]=center;line.positionCount=35;line.SetPositions(points);
                line.startColor=line.endColor=fighter.Model.ActiveThisTick?Color.red:Color.yellow;
            }
            else
            {
                var col=GetComponent<Collider2D>();var bounds=col.bounds;
                for(int i=0;i<35;i++){float a=i/34f*Mathf.PI*2;points[i]=(Vector2)bounds.center+new Vector2(Mathf.Cos(a)*bounds.extents.x,Mathf.Sin(a)*bounds.extents.y);}
                line.positionCount=35;line.SetPositions(points);line.startColor=line.endColor=actor!=null&&actor.player?Color.cyan:Color.green;
            }
        }
        void OnDestroy(){if(owned!=null)Destroy(owned);}
    }
}
