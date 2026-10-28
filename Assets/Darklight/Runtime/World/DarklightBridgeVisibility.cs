using UnityEngine;
namespace Darklight
{
    // Prevent the opaque ground baked into a rectangular foreground tree from
    // hiding a walker on a bridge. Uses the complete map/character textures;
    // no raster element is extracted, painted, reconstructed or assembled.
    public sealed class DarklightBridgeVisibility:MonoBehaviour
    {
        static readonly Rect[] Decks={new(522,226,103,56),new(621,462,103,51),new(1090,632,146,63)};
        static readonly int Visible=Shader.PropertyToID("_BridgeActorVisible"),BoundsId=Shader.PropertyToID("_BridgeActorBounds");
        DarklightMapDefinition map;SpriteRenderer player;
        public bool ProtectingPlayer{get;private set;}
        public static bool IsBridgePixel(Vector2 pixel)
        {
            foreach(var rect in Decks)
                if(Rect.MinMaxRect(rect.xMin-6,rect.yMin-6,rect.xMax+6,rect.yMax+6).Contains(pixel))return true;
            return false;
        }
        public static void Attach(Transform root,DarklightMapDefinition definition)
        {
            if(!ReferenceEquals(definition,DarklightMapDefinition.ForChapter(4)))return;
            var component=root.gameObject.AddComponent<DarklightBridgeVisibility>();component.map=definition;
            var block=new MaterialPropertyBlock();
            foreach(var region in definition.Art.foreground)
            {
                var parent=root.Find(region.name);if(parent==null)continue;
                foreach(var art in parent.GetComponentsInChildren<SpriteRenderer>())
                {art.GetPropertyBlock(block);block.SetFloat("_BridgeForeground",1);art.SetPropertyBlock(block);}
            }
            Shader.SetGlobalFloat(Visible,0);
        }
        void LateUpdate()
        {
            var lab=DarklightCombatLab.Instance;
            ProtectingPlayer=lab?.Player!=null&&IsBridgePixel(map.Art.Pixel(lab.Player.transform.position-transform.position));
            if(ProtectingPlayer)
            {
                if(player==null)player=lab.Player.transform.Find("FabioVisual")?.GetComponent<SpriteRenderer>();
                if(player!=null)
                {
                    var b=player.bounds;Shader.SetGlobalVector(BoundsId,new Vector4(b.min.x,b.min.y,b.max.x,b.max.y));
                }
                else ProtectingPlayer=false;
            }
            Shader.SetGlobalFloat(Visible,ProtectingPlayer?1:0);
        }
        void OnDestroy(){Shader.SetGlobalFloat(Visible,0);}
    }
}
