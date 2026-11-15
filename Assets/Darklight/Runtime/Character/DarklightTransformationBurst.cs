using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    public sealed class DarklightTransformationBurst:MonoBehaviour
    {
        Mesh mesh;MeshRenderer display;Light2D lightSource;
        DarklightCharacterView characterView;
        readonly Vector3[] vertices=new Vector3[4096];readonly Color[] colors=new Color[4096];readonly int[] indices=new int[6144];int quads;
        float unit,baseY;Color tint;
        public void Present(SpriteRenderer body,int form,float progress,bool entering,bool visible,Color? effectTint=null)
        {
            if(form==2)form=1; // Serafim has a crimson transformation effect.
            if(mesh==null)
            {
                mesh=new Mesh{name="Whole character awakening burst"};mesh.MarkDynamic();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                display=gameObject.AddComponent<MeshRenderer>();display.sharedMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");
                for(int i=0;i<1024;i++){int v=i*4,t=i*6;indices[t]=v;indices[t+1]=v+1;indices[t+2]=v+2;indices[t+3]=v;indices[t+4]=v+2;indices[t+5]=v+3;}
                var go=new GameObject("LuzDoDespertar");go.transform.SetParent(transform,false);lightSource=go.AddComponent<Light2D>();lightSource.lightType=Light2D.LightType.Point;lightSource.pointLightOuterRadius=3.6f;lightSource.pointLightInnerRadius=.2f;
            }
            bool show=visible&&body.sprite!=null&&progress>0&&progress<1;
            display.enabled=show;lightSource.enabled=show;if(!show)return;
            transform.localPosition=Vector3.zero;transform.localRotation=Quaternion.identity;transform.localScale=Vector3.one;
            characterView??=body.GetComponentInParent<DarklightCharacterView>();
            var bounds=characterView!=null?characterView.ReferenceBodyBounds:DarklightCharacterView.FallbackBodyBounds;
            unit=bounds.size.y/2.63f;baseY=bounds.min.y;quads=0;
            float rise=Mathf.Clamp01(progress*2.8f),fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,1,progress));
            float energy=Mathf.Sin(progress*Mathf.PI)*fade*(entering?1:.6f);
            bool reduced=DarklightCampaign.Instance?.ReducedFlashes==true;if(reduced)energy*=.35f;
            tint=form==3?new Color(.78f,.91f,1,energy):form==2?new Color(.68f,.12f,1,energy):new Color(1,.04f,.09f,energy);
            // Keep energy behind the figure so the expression and weapon remain legible.
            display.sortingLayerID=body.sortingLayerID;display.sortingOrder=body.sortingOrder-1;
            lightSource.color=effectTint??tint;lightSource.intensity=energy*(reduced?.3f:.85f);lightSource.transform.localPosition=Vector3.up*(baseY+bounds.size.y*.48f);
            for(int ring=0;ring<2;ring++)for(int i=0;i<48;i++)
            {
                float a=i/48f*Mathf.PI*2,b=(i+1)/48f*Mathf.PI*2,r=(.35f+rise*.7f)*(ring==0?1:.77f);
                Segment(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.28f),new Vector3(Mathf.Cos(b)*r,Mathf.Sin(b)*r*.28f),ring==0?.0425f:.0175f,tint);
            }
            for(int i=0;i<12;i++)
            {
                float x=(i%2==0?-1:1)*(.64f+i%3*.13f),height=.55f+rise*(1.45f+i%4*.16f);
                if(form==2)
                {
                    Vector3 p=new Vector3(x,.3f+Mathf.Repeat(progress*2+i*.33f,1)*height);Shard(p,.105f*energy,tint);Shard(p,.035f*energy,new Color(.94f,.97f,1,energy));continue;
                }
                Vector3 previous=new Vector3(x,.1f);
                for(int j=1;j<=8;j++)
                {
                    float t=j/8f,bend=Mathf.Sin(t*7+progress*(reduced?0:15)+i)*(.025f+t*.065f);
                    Vector3 next=new Vector3(x*(1+.22f*Mathf.Sin(t*Mathf.PI))+bend,.1f+t*height);
                    Color color=tint;color.a*=.6f*(1-t*.85f);Segment(previous,next,(form==3?.045f:.075f)*(1-t*.85f),color);
                    if(form==3)Segment(previous,next,.009f*(1-t*.8f),new Color(.94f,.99f,1,color.a*.65f));
                    else Segment(previous,next,.015f*(1-t*.8f),new Color(1,.22f,.065f,color.a*.65f));previous=next;
                }
            }
            if(form==2)
            {
                for(int i=0;i<32;i++){float a=progress*5+i/32f*4.5f,b=progress*5+(i+1)/32f*4.5f;Segment(Vector3.up*1.4f+new Vector3(Mathf.Cos(a),Mathf.Sin(a))*.85f,Vector3.up*1.4f+new Vector3(Mathf.Cos(b),Mathf.Sin(b))*.85f,.04f,tint);}
                for(int i=0;i<4;i++)Segment(new Vector3((i-1.5f)*.3f,.1f),new Vector3((i-1.5f)*.35f,2.5f*rise),.0125f,new Color(.94f,.97f,1,energy));
            }
            for(int i=0;i<8;i++){float a=i*2.39996f+progress*3;Vector3 d=new Vector3(Mathf.Cos(a),Mathf.Sin(a));Segment(Vector3.up*1.4f+d*.8f,Vector3.up*1.4f+d*(.9f+energy*.55f),.009f,tint);}
            if(effectTint.HasValue)
            {
                Color palette=effectTint.Value;
                for(int i=0;i<quads*4;i++)
                {
                    Color c=colors[i];float brightness=Mathf.Max(c.r,Mathf.Max(c.g,c.b));
                    colors[i]=new Color(palette.r*brightness,palette.g*brightness,palette.b*brightness,c.a);
                }
            }
            mesh.Clear();mesh.SetVertices(vertices,0,quads*4);mesh.SetColors(colors,0,quads*4);mesh.SetTriangles(indices,0,quads*6,0,false);mesh.RecalculateBounds();
        }
        void Segment(Vector3 start,Vector3 end,float width,Color color)
        {
            Vector3 d=(end-start).normalized,n=new Vector3(-d.y,d.x)*width;int k=quads++*4;
            Vector3 support=Vector3.up*baseY;
            vertices[k]=(start+n)*unit+support;vertices[k+1]=(start-n)*unit+support;vertices[k+2]=(end-n)*unit+support;vertices[k+3]=(end+n)*unit+support;colors[k]=colors[k+1]=colors[k+2]=colors[k+3]=color;
        }
        void Shard(Vector3 p,float r,Color color)
        {
            Vector3 support=Vector3.up*baseY;
            int k=quads++*4;vertices[k]=(p+Vector3.up*r*1.8f)*unit+support;vertices[k+1]=(p+Vector3.right*r)*unit+support;vertices[k+2]=(p-Vector3.up*r*1.8f)*unit+support;vertices[k+3]=(p-Vector3.right*r)*unit+support;colors[k]=colors[k+1]=colors[k+2]=colors[k+3]=color;
        }
        public void Hide(){if(display!=null)display.enabled=false;if(lightSource!=null)lightSource.enabled=false;}
        void OnDisable()=>Hide();
        void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
