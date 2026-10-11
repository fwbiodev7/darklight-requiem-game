using UnityEngine;
namespace Darklight
{
    public sealed class DarklightNPCView : MonoBehaviour
    {
        public Sprite[] frames;
        SpriteRenderer art;Vector3 foot;float clock;int direction;
        void Awake(){art=GetComponent<SpriteRenderer>();foot=transform.position;}
        void LateUpdate()
        {
            var lab=DarklightCombatLab.Instance;if(lab==null||frames==null||frames.Length<8)return;
            clock+=lab.CombatDelta;
            Vector2 delta=lab.Player.transform.position-foot;
            if(delta.sqrMagnitude<36&&delta.sqrMagnitude>.1f)direction=DarklightCharacterView.DirectionIndex(delta);
            art.sprite=frames[direction];
            transform.position=foot+Vector3.up*((int)(clock*1.2f)%2)/48f;
            art.sortingOrder=100-Mathf.RoundToInt(foot.y*10);
        }
    }
}
