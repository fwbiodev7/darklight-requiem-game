using UnityEngine;

namespace Darklight
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Camera))]
    public sealed class DarklightLabCamera : MonoBehaviour
    {
        public Transform target;
        public Transform duelOpponent;
        public Vector2 followOffset;
        public Rect bounds=new Rect(-10,-6,20,11.8f);
        public const float GameplayHalfHeight=4.96875f;
        Camera view;
        Vector3 velocity;
        float impulseTime;int impulsePixels;
        Vector3 contactFocus;float contactUntil;
        public bool ContactFocusActive=>contactUntil>Time.unscaledTime;
        public void ConfirmContact(Vector3 point)
        {
            if(DarklightCampaign.Instance?.ReducedFlashes==true||PlayerPrefs.GetInt("Darklight.Shake",1)==0){contactUntil=0;return;}
            contactFocus=point;contactUntil=Time.unscaledTime+.16f;
        }
        public void Impulse(int pixels){if(PlayerPrefs.GetInt("Darklight.Shake",1)==0)return;impulsePixels=Mathf.Clamp(pixels,0,2);impulseTime=.09f;}
        void Awake()
        {
            view=GetComponent<Camera>();view.orthographic=true;
            Fit();if(target!=null) transform.position=Constrain(target.position+new Vector3(followOffset.x,followOffset.y,-10));
        }
        float FramingHeight => DarklightCampaign.Instance?.Training == true ? 5.5f : GameplayHalfHeight;
        void Fit() { view.orthographicSize=Mathf.Min(FramingHeight,Mathf.Min(bounds.height*.5f,bounds.width*.5f/Mathf.Max(.01f,view.aspect))); }
        Vector3 Constrain(Vector3 p)
        {
            float height=view.orthographicSize,width=height*view.aspect;
            p.x=Mathf.Clamp(p.x,bounds.xMin+width,bounds.xMax-width);
            p.y=Mathf.Clamp(p.y,bounds.yMin+height,bounds.yMax-height);return p;
        }
        void LateUpdate()
        {
            if(target==null || DarklightCombatLab.Instance?.Paused == true)return;Fit();
            Vector3 focus=target.position+new Vector3(followOffset.x,followOffset.y,-10);
            if(duelOpponent!=null && duelOpponent.gameObject.activeInHierarchy)
            {
                Vector3 separation=duelOpponent.position-target.position;
                float halfHeight=Mathf.Max(FramingHeight,Mathf.Max(Mathf.Abs(separation.y)*.5f+1.5f,
                    Mathf.Abs(separation.x)*.5f/Mathf.Max(.01f,view.aspect)+1.2f));
                view.orthographicSize=Mathf.Min(halfHeight,Mathf.Min(bounds.height*.5f,bounds.width*.5f/Mathf.Max(.01f,view.aspect)));
                focus.x=(target.position.x+duelOpponent.position.x)*.5f;
                focus.y=(target.position.y+duelOpponent.position.y)*.5f+followOffset.y;
            }
            if(ContactFocusActive){float t=(contactUntil-Time.unscaledTime)/.16f;focus+=Vector3.ClampMagnitude(new Vector3(contactFocus.x-target.position.x,contactFocus.y-target.position.y,0),1.5f)*(.18f*t);}
            Vector3 p=Vector3.SmoothDamp(transform.position,Constrain(focus),ref velocity,ContactFocusActive?.06f:.25f);
            float grid=(view.targetTexture!=null?view.targetTexture.height:720f)/(view.orthographicSize*2);
            impulseTime=Mathf.Max(0,impulseTime-(DarklightCombatLab.Instance?.CombatDelta??Time.deltaTime));
            int offset=impulseTime>0?((Time.frameCount&1)==0?impulsePixels:-impulsePixels):0;
            p=Constrain(new Vector3(Mathf.Round(p.x*grid)/grid+offset/grid,Mathf.Round(p.y*grid)/grid,p.z));
            transform.position=p;
        }
    }
}
