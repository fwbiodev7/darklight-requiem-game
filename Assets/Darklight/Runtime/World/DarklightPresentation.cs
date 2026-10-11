using UnityEngine;

namespace Darklight
{
    public static class DarklightPresentation
    {
        public static void Configure(Camera camera)
        {
            if(camera==null)return;
            camera.allowMSAA=false;camera.allowHDR=false;camera.allowDynamicResolution=false;
        }
    }
}
