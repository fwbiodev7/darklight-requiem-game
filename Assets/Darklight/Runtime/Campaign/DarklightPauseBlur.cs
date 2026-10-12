using UnityEngine;
namespace Darklight
{
    public sealed class DarklightPauseBlur : MonoBehaviour
    {
        Camera source;UnityEngine.UI.RawImage image;Material blur;RenderTexture intermediate,result;
        public bool Visible=>image!=null&&image.enabled;
        public Texture SceneTexture=>image==null?null:image.texture;
        public void Initialize(Camera camera)
        {
            source=camera;image=gameObject.AddComponent<UnityEngine.UI.RawImage>();image.raycastTarget=false;image.color=Color.white;
            var shader=Resources.Load<Shader>("Darklight/Menu/PauseBlur");if(shader!=null)blur=new Material(shader);
            image.enabled=false;
        }
        public void Show(bool active){if(image!=null)image.enabled=active;}
        void LateUpdate()
        {
            if(!Visible||source==null||source.targetTexture==null)return;
            if(blur==null){image.texture=source.targetTexture;return;}
            if(result==null)
            {
                intermediate=new RenderTexture(640,360,0){filterMode=FilterMode.Bilinear,name="PauseBlurHorizontal"};intermediate.Create();
                result=new RenderTexture(640,360,0){filterMode=FilterMode.Bilinear,name="PauseCurrentScene"};result.Create();
            }
            blur.SetVector("_Direction",new Vector4(1,0,0,0));Graphics.Blit(source.targetTexture,intermediate,blur);
            blur.SetVector("_Direction",new Vector4(0,1,0,0));Graphics.Blit(intermediate,result,blur);image.texture=result;
        }
        void OnDestroy(){if(blur!=null)Destroy(blur);if(intermediate!=null){intermediate.Release();Destroy(intermediate);}if(result!=null){result.Release();Destroy(result);}}
    }
}
