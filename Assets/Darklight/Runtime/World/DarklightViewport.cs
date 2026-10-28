using UnityEngine;
namespace Darklight
{
    public static class DarklightViewport
    {
        public static Rect Fit(int width,int height)
        {
            width=Mathf.Max(1,width);height=Mathf.Max(1,height);
            float scale=Mathf.Min(width/1280f,height/720f);
            return new Rect((width-1280*scale)*.5f,(height-720*scale)*.5f,1280*scale,720*scale);
        }
    }
}
