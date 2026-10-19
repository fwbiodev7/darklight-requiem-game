using UnityEngine;
namespace Darklight
{
    // One small mesh follows the complete body pose; no detached character art is used.
    public sealed class DarklightSmallTransformationAura:MonoBehaviour
    {
        Mesh mesh;MeshRenderer renderer;Material material;public Sprite PresentedFrame{get;private set;}
        DarklightCharacterView characterView;
        readonly Vector3[] vertices=new Vector3[576];readonly Color[] colors=new Color[576];readonly int[] indices=new int[864];
        public void Present(SpriteRenderer body,int form,float clock,bool dead)
        {
            if(mesh==null)
            {
                mesh=new Mesh{name="Small continuous transformation aura"};mesh.MarkDynamic();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                renderer=gameObject.AddComponent<MeshRenderer>();material=Resources.Load<Material>("Darklight/SmallTransformationAura");renderer.sharedMaterial=material;
                for(int i=0;i<144;i++){int v=i*4,t=i*6;indices[t]=v;indices[t+1]=v+1;indices[t+2]=v+2;indices[t+3]=v;indices[t+4]=v+2;indices[t+5]=v+3;}
            }
            bool show=form>0&&!dead&&body.sprite!=null;renderer.enabled=show;PresentedFrame=show?body.sprite:null;if(!show)return;
            transform.localPosition=Vector3.zero;transform.localRotation=Quaternion.identity;transform.localScale=Vector3.one;
            renderer.sortingLayerID=body.sortingLayerID;renderer.sortingOrder=body.sortingOrder-1;
            characterView??=body.GetComponentInParent<DarklightCharacterView>();
            var bounds=characterView!=null?characterView.ReferenceBodyBounds:DarklightCharacterView.FallbackBodyBounds;
            float h=bounds.size.y;float radius=Mathf.Clamp(h*.24f,.16f,.66f);
            Color tint=form==3?new Color(.76f,.9f,1):form==2?new Color(.66f,.12f,1):new Color(1,.06f,.12f);
            bool reduced=DarklightCampaign.Instance?.ReducedFlashes==true;
            int quads=18;
            if(form==3)
            {
                quads=0;float time=reduced?0:clock;
                for(int bolt=0;bolt<10;bolt++)
                {
                    float side=bolt%2==0?-1:1,t=Mathf.Repeat(time/6f+bolt*.381966f,1),y=bounds.min.y+h*(.04f+t*.67f);
                    Vector3 previous=new Vector3(side*(radius+.025f),y);
                    for(int s=0;s<5;s++)
                    {
                        float tooth=Mathf.Sin(bolt*19+s*23+Mathf.Floor(time*7)%42)*h*.035f;
                        Vector3 next=new Vector3(side*(radius+.025f)+tooth,y+(s+1)*h*.065f);
                        Segment(previous,next,h*.013f,new Color(.2f,.65f,1,.48f),ref quads);
                        Segment(previous,next,h*.0045f,new Color(.85f,.97f,1,.84f),ref quads);previous=next;
                    }
                }
                for(int s=0;s<18;s++)
                {
                    float a=s/18f*Mathf.PI*2,b=(s+1)/18f*Mathf.PI*2,r=radius*(1.04f+.08f*Mathf.Sin(s*4+Mathf.Floor(time*6)%36));
                    Vector3 start=new Vector3(Mathf.Cos(a)*r,bounds.min.y+.04f+Mathf.Sin(a)*radius*.23f),end=new Vector3(Mathf.Cos(b)*r,bounds.min.y+.04f+Mathf.Sin(b)*radius*.23f);
                    Segment(start,end,h*.009f,new Color(.18f,.63f,1,.42f),ref quads);Segment(start,end,h*.003f,new Color(.85f,.98f,1,.75f),ref quads);
                }
            }
            else
            {
                quads=0;float time=reduced?0:clock;
                for(int i=0;i<18;i++)
                {
                    float t=Mathf.Repeat(time/6f+i*.381966f,1),side=i%2==0?-1:1;
                    float y=bounds.min.y+h*(.07f+.77f*t),x=side*(radius+.03f*Mathf.Sin(t*7+i));
                    float alpha=Mathf.Sin(t*Mathf.PI)*.65f;
                    if(form==2)
                    {
                        Shard(new Vector3(x,y),h*.019f,new Color(.59f,.1f,1,alpha),ref quads);
                        Shard(new Vector3(x,y),h*.006f,new Color(.9f,.65f,1,alpha),ref quads);
                        continue;
                    }
                    Vector3 previous=new Vector3(x,y);
                    for(int s=0;s<3;s++)
                    {
                        float f=(s+1)/3f;Vector3 next=new Vector3(x+Mathf.Sin(f*4+t*Mathf.PI*2+i)*h*.024f,y+f*h*.14f);
                        Segment(previous,next,h*.014f*(1-f*.65f),new Color(1,.015f,.065f,alpha*(1-f*.65f)),ref quads);
                        Segment(previous,next,h*.004f*(1-f*.65f),new Color(1,.22f,.065f,alpha*(1-f*.5f)),ref quads);previous=next;
                    }
                }
                for(int s=0;s<18;s++)
                {
                    float a=s/18f*Mathf.PI*2,b=(s+1)/18f*Mathf.PI*2,r=radius*(1.04f+.025f*Mathf.Sin(time*Mathf.PI/3));
                    Vector3 start=new Vector3(Mathf.Cos(a)*r,bounds.min.y+.035f+Mathf.Sin(a)*radius*.23f),end=new Vector3(Mathf.Cos(b)*r,bounds.min.y+.035f+Mathf.Sin(b)*radius*.23f);
                    Segment(start,end,h*.009f,new Color(tint.r,tint.g,tint.b,.18f),ref quads);
                    Segment(start,end,h*.003f,new Color(tint.r,tint.g,tint.b,.38f),ref quads);
                }
            }
            mesh.Clear();mesh.SetVertices(vertices,0,quads*4);mesh.SetColors(colors,0,quads*4);mesh.SetTriangles(indices,0,quads*6,0,false);mesh.RecalculateBounds();
        }
        void Segment(Vector3 start,Vector3 end,float width,Color color,ref int quads)
        {
            Vector3 direction=(end-start).normalized,normal=new Vector3(-direction.y,direction.x)*width;int k=quads++*4;
            vertices[k]=start+normal;vertices[k+1]=start-normal;vertices[k+2]=end-normal;vertices[k+3]=end+normal;
            colors[k]=colors[k+1]=colors[k+2]=colors[k+3]=color;
        }
        void Shard(Vector3 center,float radius,Color color,ref int quads)
        {
            int k=quads++*4;vertices[k]=center+Vector3.up*radius*1.8f;vertices[k+1]=center+Vector3.right*radius;
            vertices[k+2]=center-Vector3.up*radius*1.8f;vertices[k+3]=center-Vector3.right*radius;
            colors[k]=colors[k+1]=colors[k+2]=colors[k+3]=color;
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
