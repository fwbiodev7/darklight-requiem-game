using UnityEngine;
namespace Darklight
{
    [DefaultExecutionOrder(1750)]public sealed class DarklightCombatImpacts:MonoBehaviour
    {
        static DarklightCombatImpacts instance;
        struct Hit{public Vector3 p;public Vector2 direction;public Color color;public float age,life;public int form,weapon,seed;public bool heavy,active;}
        readonly Hit[] hits=new Hit[16];readonly Vector3[] vertices=new Vector3[8192];readonly Color[] colors=new Color[8192];readonly int[] indices=new int[12288];
        Mesh mesh;MeshRenderer display;int cursor,quads,serial;
        public static void Emit(Vector3 position,bool heavy)
        {
            var lab=DarklightCombatLab.Instance;var game=DarklightCampaign.Instance;if(lab==null||game==null)return;
            if(instance==null||!instance.transform.IsChildOf(lab.transform)){var obj=new GameObject("ImpactosDeCombate");obj.transform.SetParent(lab.transform,false);instance=obj.AddComponent<DarklightCombatImpacts>();}instance.Add(position,heavy,game,lab);
        }
        void Add(Vector3 position,bool heavy,DarklightCampaign game,DarklightCombatLab lab)
        {
            if(mesh==null)
            {
                mesh=new Mesh{name="Pooled combat contact geometry"};mesh.MarkDynamic();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                display=gameObject.AddComponent<MeshRenderer>();display.sharedMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");display.sortingOrder=900;
                for(int i=0;i<2048;i++){int v=i*4,t=i*6;indices[t]=v;indices[t+1]=v+1;indices[t+2]=v+2;indices[t+3]=v;indices[t+4]=v+2;indices[t+5]=v+3;}
            }
            int form=game.PresentedForm;Color color=form==3?new Color(.55f,.86f,1):form==2?new Color(.7f,.19f,1):form==1?new Color(1,.08f,.15f):new Color(1,.49f,.19f);
            var direction=(Vector2)(position-lab.Player.transform.position);if(direction.sqrMagnitude<.001f)direction=Vector2.right;
            hits[cursor]=new Hit{p=position+Vector3.up*.65f,direction=direction.normalized,color=color,life=heavy?.36f:.24f,form=form,weapon=(int)game.Weapons.Equipped.id,seed=serial++,heavy=heavy,active=true};cursor=(cursor+1)%hits.Length;
        }
        void LateUpdate()
        {
            if(mesh==null)return;float dt=DarklightCombatLab.Instance?.CombatDelta??Time.deltaTime;if(dt<=0)return;quads=0;
            bool reduced=DarklightCampaign.Instance?.ReducedFlashes==true;int quality=DarklightRemaster.QualityLevel;
            for(int n=0;n<hits.Length;n++)
            {
                var h=hits[n];if(!h.active)continue;h.age+=dt;if(h.age>=h.life){h.active=false;hits[n]=h;continue;}hits[n]=h;
                float t=h.age/h.life,fade=(1-t)*(reduced?.35f:1),size=h.heavy?1.2f:.8f;Color color=h.color;color.a=fade;
                Vector3 center=transform.InverseTransformPoint(h.p);int rays=quality==0?5:quality==1?8:12;
                for(int i=0;i<rays;i++)
                {
                    float angle=i*2.399963f+h.seed*.61f;Vector2 dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));dir=(dir+h.direction*.65f).normalized;
                    Vector3 start=center+(Vector3)dir*(.035f+t*size*(.55f+i%3*.15f));Vector3 end=start+(Vector3)dir*(.06f+(1-t)*.15f);
                    Segment(start,end,.025f*(1-t)+.008f,color);
                    if(h.form==3&&quality>0){Vector3 bend=end+(Vector3)new Vector2(-dir.y,dir.x)*(.06f*(i%2==0?1:-1));Segment(end,bend,.013f,color);Segment(bend,bend+(Vector3)dir*.09f,.012f,color);}
                }
                if(h.heavy||h.weapon==2)for(int i=0;i<20;i++)
                {
                    if(i%4==0)continue;float a=i*Mathf.PI*.1f,b=(i+1)*Mathf.PI*.1f,r=.08f+t*size*.65f;
                    Segment(center+new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.55f),center+new Vector3(Mathf.Cos(b)*r,Mathf.Sin(b)*r*.55f),.02f*(1-t),color);
                }
                if(!reduced&&t<.4f)
                {
                    Color core=Color.white;core.a=(1-t/.4f)*.7f;float r=.08f*(1-t/.4f)*(h.heavy?1.4f:1);
                    Segment(center-Vector3.up*r,center+Vector3.up*r,r*.5f,core);Segment(center-Vector3.right*r,center+Vector3.right*r,r*.5f,core);
                }
            }
            display.enabled=quads>0;if(quads==0)return;mesh.Clear();mesh.SetVertices(vertices,0,quads*4);mesh.SetColors(colors,0,quads*4);mesh.SetIndices(indices,0,quads*6,MeshTopology.Triangles,0);mesh.RecalculateBounds();
        }
        void Segment(Vector3 from,Vector3 to,float width,Color color)
        {
            if(quads>=2048||width<=0)return;var delta=to-from;if(delta.sqrMagnitude<.000001f)return;Vector3 normal=new Vector3(-delta.y,delta.x).normalized*width*.5f;int v=quads++*4;
            vertices[v]=from-normal;vertices[v+1]=from+normal;vertices[v+2]=to+normal;vertices[v+3]=to-normal;for(int i=0;i<4;i++)colors[v+i]=color;
        }
        void OnDestroy(){if(instance==this)instance=null;if(mesh!=null)Destroy(mesh);}
    }
}
