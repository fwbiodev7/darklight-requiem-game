using UnityEngine;
namespace Darklight
{
    public sealed class DarklightCombatTrail:MonoBehaviour
    {
        Mesh mesh;MeshRenderer display;int quads;
        readonly Vector3[] vertices=new Vector3[2048];readonly Color[] colors=new Color[2048];readonly int[] indices=new int[3072];
        public void Present(SpriteRenderer body,int weapon,CombatModel model,int form)
        {
            if(mesh==null)
            {
                mesh=new Mesh{name="Combat ribbons and chain links"};mesh.MarkDynamic();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;display=gameObject.AddComponent<MeshRenderer>();display.sharedMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");
                for(int i=0;i<512;i++){int v=i*4,t=i*6;indices[t]=v;indices[t+1]=v+1;indices[t+2]=v+2;indices[t+3]=v;indices[t+4]=v+2;indices[t+5]=v+3;}
            }
            bool show=model.State==CombatState.Attack&&model.Attack!=null&&model.StateTime>=model.Attack.startup*.85f;display.enabled=show;if(!show)return;
            quads=0;transform.position=body.transform.position+Vector3.up*.9f;transform.rotation=Quaternion.identity;transform.localScale=Vector3.one;
            display.sortingLayerID=body.sortingLayerID;display.sortingOrder=body.sortingOrder+1;
            float phase=Mathf.Clamp01((model.StateTime-model.Attack.startup)/Mathf.Max(.01f,model.Attack.active+model.Attack.recovery));
            float angle=Mathf.Atan2(model.Facing.y,model.Facing.x),head=angle+Mathf.Lerp(-1.3f,1.3f,phase),radius=weapon==2?2.35f:weapon==1?1.1f:1.7f;
            Color color=form==3?new Color(.35f,.7f,1):form==2||weapon==2?new Color(.74f,.21f,1):new Color(1,.065f,.12f);
            Color hot=form==3?new Color(1.3f,1.5f,1.7f):weapon==2?new Color(1.3f,.8f,1.3f):new Color(1.5f,1.2f,.7f);
            float fade=Mathf.Clamp01((1-phase)*2.5f);if(DarklightCampaign.Instance?.ReducedFlashes==true)fade*=.4f;color.a=hot.a=fade;
            for(int a=0;a<(weapon==1?2:1);a++)
            {
                Vector3 offset=weapon==1?Vector3.up*(a==0?.32f:-.25f):Vector3.zero;float side=a==1?-.35f:0;
                Ribbon(offset,radius,head-1.6f+side,head+side,.22f,color);Ribbon(offset,radius+.025f,head-1.25f+side,head+side,.032f,hot);
                if(DarklightRemaster.QualityLevel>0)Ribbon(offset,radius-.17f,head-1.65f+side,head-.18f+side,.025f,color);
            }
            if(weapon==2)for(int i=0;i<18;i++)
            {
                float aa=Mathf.Lerp(head-1.55f,head,i/17f);Vector3 p=new Vector3(Mathf.Cos(aa),Mathf.Sin(aa))*radius;
                for(int j=0;j<6;j++){float s=j/6f*Mathf.PI*2,e=(j+1)/6f*Mathf.PI*2;Segment(p+new Vector3(Mathf.Cos(s)*.06f,Mathf.Sin(s)*.12f),p+new Vector3(Mathf.Cos(e)*.06f,Mathf.Sin(e)*.12f),.014f,hot);}
            }
            for(int i=0;i<6;i++){float aa=head-i*.13f;Vector3 p=new Vector3(Mathf.Cos(aa),Mathf.Sin(aa))*radius;Segment(p,p+new Vector3(Mathf.Cos(aa+.3f),Mathf.Sin(aa+.3f))*(.09f+.07f*i),.008f,hot);}
            mesh.Clear();mesh.SetVertices(vertices,0,quads*4);mesh.SetColors(colors,0,quads*4);mesh.SetTriangles(indices,0,quads*6,0,false);mesh.RecalculateBounds();
        }
        void Ribbon(Vector3 center,float radius,float start,float end,float width,Color color)
        {
            for(int i=0;i<31;i++)
            {
                float t=i/31f,u=(i+1)/31f,w=width*Mathf.Sin(t*Mathf.PI)*(.3f+.7f*t),z=width*Mathf.Sin(u*Mathf.PI)*(.3f+.7f*u);float a=Mathf.Lerp(start,end,t),b=Mathf.Lerp(start,end,u);
                Vector3 d=new Vector3(Mathf.Cos(a),Mathf.Sin(a)),e=new Vector3(Mathf.Cos(b),Mathf.Sin(b));int k=quads++*4;
                vertices[k]=center+d*(radius-w);vertices[k+1]=center+d*(radius+w);vertices[k+2]=center+e*(radius+z);vertices[k+3]=center+e*(radius-z);
                colors[k]=colors[k+1]=new Color(color.r,color.g,color.b,color.a*t);colors[k+2]=colors[k+3]=new Color(color.r,color.g,color.b,color.a*u);
            }
        }
        void Segment(Vector3 a,Vector3 b,float width,Color color){Vector3 d=(b-a).normalized,n=new Vector3(-d.y,d.x)*width;int k=quads++*4;vertices[k]=a+n;vertices[k+1]=a-n;vertices[k+2]=b-n;vertices[k+3]=b+n;colors[k]=colors[k+1]=colors[k+2]=colors[k+3]=color;}
        void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
