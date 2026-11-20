using UnityEngine;
namespace Darklight
{
    [DefaultExecutionOrder(1900)]
    public sealed class DarklightMapOcclusion : MonoBehaviour
    {
        public float width,height;
        SpriteRenderer[] art;
        int[] originalOrders;
        bool bakedForeground;
        public void UseBakedForeground() => bakedForeground=true;
        void Awake()
        {
            art=GetComponentsInChildren<SpriteRenderer>();
            originalOrders=System.Array.ConvertAll(art,piece=>piece.sortingOrder);
        }
        void LateUpdate()
        {
            for(int i=0;i<art.Length;i++)
            {
                var piece=art[i];
                if(piece==null)continue;
                int order=DarklightRemaster.OrderBehindActors(piece.bounds,piece.sortingLayerID,originalOrders[i]);
                // Painted crops repeat the ground, so hiding the overlap reveals the intact map.
                if(bakedForeground)piece.forceRenderingOff=order!=originalOrders[i];
                else piece.sortingOrder=order;
            }
        }
    }
}
