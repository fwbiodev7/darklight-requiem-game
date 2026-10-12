using System.Linq;
using UnityEngine;

namespace Darklight
{
    public sealed class DarklightMenuBackdrop : MonoBehaviour
    {
        UnityEngine.UI.Image character,mist;
        Sprite[][] poses;
        Material atmosphere;
        int skin;
        RectTransform[] particles;
        UnityEngine.UI.Image[] particleImages;
        UnityEngine.UI.Image Layer(string name,float x,float y,float w,float h,Sprite sprite)
        {
            var r=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)).GetComponent<RectTransform>();
            r.SetParent(transform,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
            var image=r.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=false;image.sprite=sprite;return image;
        }
        public void Initialize()
        {
            var background=Layer("CidadeDeVespera",0,0,640,360,Resources.Load<Sprite>("Darklight/Menu/Vespera"));
            var shader=Resources.Load<Shader>("Darklight/Menu/MenuAtmosphere");
            if(shader!=null){atmosphere=new Material(shader);background.material=atmosphere;}
            mist=Layer("Nevoa",244,213,470,98,Resources.Load<Sprite>("Darklight/Menu/Mist"));mist.color=new Color(1,1,1,.085f);
            poses=new[]{Resources.LoadAll<Sprite>("Darklight/Menu/RequiemPose").OrderBy(s=>s.name).ToArray(),Resources.LoadAll<Sprite>("Darklight/Menu/RockPose").OrderBy(s=>s.name).ToArray()};
            character=Layer("FabioSelecionado",475,153,145,165,null);character.preserveAspect=true;
            particles=new RectTransform[8];particleImages=new UnityEngine.UI.Image[8];
            for(int i=0;i<8;i++){var p=Layer("ParticulaCarmesim"+i,400,260,1,1,null);particleImages[i]=p;particles[i]=p.rectTransform;}
            SetSkin(0);
        }
        public void ShowCharacter(bool visible){if(character!=null)character.gameObject.SetActive(visible);}
        public void SetSkin(int selected)
        {
            skin=Mathf.Clamp(selected,0,1);
            if(poses==null)return;
            var frames=poses[skin];character.enabled=frames.Length>0;
            if(frames.Length>0)character.sprite=frames[0];
        }
        void Update()
        {
            if(poses==null)return;
            float t=Time.unscaledTime;var frames=poses[skin];
            // Wind advances 0,1,2,3,2,1; no positional tweening of the character or boots.
            int phase=Mathf.FloorToInt(t/1.1f)%6;int frame=phase<4?phase:6-phase;
            if(frames.Length>0)character.sprite=frames[frame%frames.Length];
            mist.rectTransform.anchoredPosition=new Vector2(244+Mathf.Round(Mathf.Sin(t*.035f)*26),-213);
            atmosphere?.SetFloat("_ReducedFlashes",DarklightCampaign.Instance?.ReducedFlashes==true?1:0);
            for(int i=0;i<particles.Length;i++)
            {
                float p=Mathf.Repeat(t*.026f+i*.139f,1);
                particles[i].anchoredPosition=new Vector2(361+i*31+Mathf.Round(Mathf.Sin(t*.1f+i)*4),-Mathf.Round(310-p*60));
                particleImages[i].color=new Color(.68f,.05f,.1f,p<.2f?Mathf.Sin(p/.2f*Mathf.PI)*.42f:0);
            }
        }
        void OnDestroy(){if(atmosphere!=null)Destroy(atmosphere);}
    }
}
