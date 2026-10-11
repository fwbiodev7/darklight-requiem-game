using UnityEngine;
namespace Darklight
{
    public sealed class DarklightMapOcclusion : MonoBehaviour
    {
        public float width,height;
        SpriteRenderer art;
        void Awake(){art=GetComponent<SpriteRenderer>();}
        void LateUpdate()
        {
            var lab=DarklightCombatLab.Instance;if(lab==null||lab.Player==null)return;
            var delta=lab.Player.transform.position-transform.position;
            // Foreground furniture fades only when its upper silhouette would conceal the hero.
            bool covers=delta.y>.15f&&delta.y<height&&Mathf.Abs(delta.x)<width*.5f+.25f;
            art.color=new Color(1,1,1,covers?.32f:1);
        }
    }
}
