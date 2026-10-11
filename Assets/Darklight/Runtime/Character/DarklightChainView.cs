using UnityEngine;
namespace Darklight
{
    // A fixed pool animates articulated links without runtime allocations or physics joints.
    public sealed class DarklightChainView:MonoBehaviour
    {
        public Sprite link,blade;public Material material;
        readonly SpriteRenderer[] parts=new SpriteRenderer[17];DarklightFighter fighter;DarklightWeaponController weapons;
        void Awake(){fighter=GetComponent<DarklightFighter>();weapons=GetComponent<DarklightWeaponController>();for(int i=0;i<parts.Length;i++){parts[i]=new GameObject("Elo"+i).AddComponent<SpriteRenderer>();parts[i].transform.SetParent(transform,false);parts[i].sprite=i==16?blade:link;parts[i].sharedMaterial=material;parts[i].enabled=false;parts[i].transform.localScale=Vector3.one*(i==16?.42f:.22f)/parts[i].sprite.bounds.size.x;}}
        void LateUpdate()
        {
            var m=fighter.Model;bool visible=weapons?.Equipped?.id==DarklightWeaponId.Chain&&m!=null&&m.State==CombatState.Attack&&m.Attack.kind!=StrikeKind.Skill&&m.Attack.kind!=StrikeKind.Counter;
            foreach(var r in parts)r.enabled=visible;if(!visible)return;
            float t=Mathf.Clamp01(m.StateTime/m.Attack.Duration),reach=Mathf.Sin(t*Mathf.PI)*m.Attack.range;
            bool circle=m.Attack.angle>=300;float turn=circle?t*720:Mathf.Lerp(-m.Attack.angle*.5f,m.Attack.angle*.5f,t);
            Vector2 facing=Quaternion.Euler(0,0,turn)*m.Facing;Vector2 start=(Vector2)transform.position+Vector2.up*.9f;
            Vector2 tip=start+facing*reach;foreach(var hit in Physics2D.RaycastAll(transform.position,facing,reach))if(!hit.collider.isTrigger&&hit.collider.GetComponentInParent<DarklightCombatant>()==null){tip=start+facing*Mathf.Min(reach,hit.distance);break;}
            Vector2 normal=new(-facing.y,facing.x);Color tint=DarklightCampaign.Instance?.Form==2?new Color(.7f,.35f,1):DarklightCampaign.Instance?.Form==3?new Color(.95f,.96f,1):Color.white;
            for(int i=0;i<parts.Length;i++){float u=(i+1f)/parts.Length;Vector2 p=Vector2.Lerp(start,tip,u)+normal*Mathf.Sin(u*Mathf.PI)*Mathf.Sin(t*Mathf.PI*2)*.25f;p=new Vector2(Mathf.Round(p.x*48)/48,Mathf.Round(p.y*48)/48);parts[i].transform.position=p;parts[i].transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(facing.y,facing.x)*Mathf.Rad2Deg);parts[i].sortingOrder=101-Mathf.RoundToInt(transform.position.y*10);parts[i].color=tint;}
        }
    }
}
