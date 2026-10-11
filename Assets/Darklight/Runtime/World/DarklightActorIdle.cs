using UnityEngine;
namespace Darklight
{
    public sealed class DarklightActorIdle : MonoBehaviour
    {
        Vector3 foot;float clock;
        void Awake(){foot=transform.position;}
        void Update()
        {
            clock+=DarklightCombatLab.Instance?.CombatDelta??0;
            // Two discreet pixel positions, never filtered or scaled between frames.
            transform.position=foot+Vector3.up*((int)(clock*1.2f)%2)/48f;
        }
    }
}
