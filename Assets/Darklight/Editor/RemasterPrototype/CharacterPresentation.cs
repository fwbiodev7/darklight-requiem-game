using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;
namespace Darklight.Editor
{
    public static partial class RemasterPrototype
    {
        sealed class ActorProof
        {
            public SpriteRenderer actor;public Sprite sprite;public Vector3 scale;public MaterialPropertyBlock block;
            public Mesh mesh;public MeshRenderer shadow;public int skin=-1;
        }
        static readonly List<ActorProof> actors=new();
        static Material effectMaterial,shadowMaterial;
        static GameObject transformationEffects;
        static void SetupCharacterPresentation()
        {
            effectMaterial=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));transient.Add(effectMaterial);
            shadowMaterial=new Material(Shader.Find("Darklight/Prototype/WholeSilhouetteShadow"));transient.Add(shadowMaterial);
            var body=DarklightCombatLab.Instance.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            foreach(var r in oldMaterials.Keys.Where(r=>r!=null && (r==body||r.name=="RockPrototypeCompanion"||r.GetComponentInParent<DarklightEnemy>()!=null)))
            {
                var a=new ActorProof{actor=r,sprite=r.sprite,scale=r.transform.localScale,block=new MaterialPropertyBlock(),skin=r==body?0:r.name=="RockPrototypeCompanion"?1:-1};r.GetPropertyBlock(a.block);
                var go=new GameObject("GroundShadow_"+r.name);go.transform.SetParent(root.transform,false);
                a.mesh=new Mesh{name="Whole silhouette projection"};transient.Add(a.mesh);go.AddComponent<MeshFilter>().sharedMesh=a.mesh;
                a.shadow=go.AddComponent<MeshRenderer>();a.shadow.sharedMaterial=shadowMaterial;a.shadow.sortingLayerID=r.sortingLayerID;a.shadow.sortingOrder=9990;actors.Add(a);
            }
            MoveReviewLight(0);
        }
        public static void ApplyTransformation(int form)
        {
            if(transformationEffects!=null)UnityEngine.Object.DestroyImmediate(transformationEffects);
            transformationEffects=new GameObject("TransformationDetails");transformationEffects.transform.SetParent(root.transform,false);
            if(soloMode){PresentSoloIdle(form,0);UpdateCharacterShadows();return;}
            foreach(var a in actors.Where(a=>a.skin>=0))
            {
                if(form==0){a.actor.sprite=a.sprite;a.actor.transform.localScale=a.scale;a.actor.SetPropertyBlock(a.block);continue;}
                string name=(a.skin==0?"Requiem":"Rock")+(form==1?"Crimson":form==2?"Violet":"White")+"YoungPrototype";
                var sprite=AssetDatabase.LoadAllAssetsAtPath(Folder+"TransformationsYoung.png").OfType<Sprite>().FirstOrDefault(s=>s.name==name);
                if(sprite==null)throw new InvalidOperationException("Missing complete transformation: "+name);
                float normalHeight=a.sprite.bounds.size.y*a.scale.y;
                a.actor.sprite=sprite;a.actor.transform.localScale=Vector3.one*(normalHeight/sprite.bounds.size.y);
                var block=new MaterialPropertyBlock();a.actor.GetPropertyBlock(block);block.SetFloat("_VampireForm",0);block.SetFloat("_WholeFrameId",a.skin*3+form);block.SetFloat("_WholeSprite",1);block.SetTexture("_VampireMask",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"TransformationsYoung_Palette.png"));a.actor.SetPropertyBlock(block);
                var aura=a.actor.transform.Find("AuraVampiraOriginal");if(aura!=null)aura.gameObject.SetActive(false);
                var small=a.actor.transform.Find("AuraPequenaContinua");if(small==null){small=new GameObject("AuraPequenaContinua").transform;small.SetParent(a.actor.transform,false);small.gameObject.AddComponent<DarklightSmallTransformationAura>();}small.GetComponent<DarklightSmallTransformationAura>().Present(a.actor,form,0,false);
                Color c=form==1?new Color(1,.06f,.14f):form==2?new Color(.64f,.15f,1):new Color(.8f,.94f,1);
                Vector3 p=a.actor.transform.position;
                for(int i=0;i<9;i++)
                {
                    float angle=i*2.39996f;Vector3 point=p+new Vector3(Mathf.Cos(angle)*(.48f+i%3*.12f),.35f+i*.29f,0);
                    Diamond(transformationEffects.transform,point,.025f+i%2*.015f,c*.8f,10010);
                }
            }
            UpdateCharacterShadows();
        }
        public static void MoveReviewLight(float turn)
        {
            var lamp=root.transform.Find("LanternWarm").GetComponent<Light2D>();Vector2 p=DarklightCombatLab.Instance.Player.transform.position;
            float angle=turn*Mathf.PI*2;lamp.transform.position=p+new Vector2(Mathf.Cos(angle)*2.6f,Mathf.Sin(angle)*1.8f+.6f);
            var marker=root.transform.Find("MovingLampMarker");if(marker!=null)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            var go=new GameObject("MovingLampMarker");go.transform.SetParent(root.transform,false);Vector3 center=lamp.transform.position;
            Diamond(go.transform,center,.11f,new Color(1,.78f,.35f),11050);Diamond(go.transform,center,.035f,Color.white,11051);
            UpdateCharacterShadows();
        }
        static void UpdateCharacterShadows()
        {
            var light=root.transform.Find("LanternWarm").GetComponent<Light2D>();Vector2 lamp=light.transform.position;
            foreach(var a in actors)
            {
                var r=a.actor;if(r==null||r.sprite==null)continue;var sprite=r.sprite;Vector2 foot=r.transform.position;
                Vector2 away=(foot-lamp).normalized;float length=Mathf.Clamp(Vector2.Distance(foot,lamp)*.35f,.55f,1.5f);
                var input=sprite.vertices;var vertices=new Vector3[input.Length];
                for(int i=0;i<input.Length;i++)
                {
                    Vector3 v=r.transform.TransformPoint(input[i]);float h=Mathf.Max(0,v.y-foot.y);
                    vertices[i]=foot+new Vector2((v.x-foot.x)*.8f,0)+away*h*length;
                }
                a.mesh.Clear();a.mesh.vertices=vertices;a.mesh.uv=sprite.uv;a.mesh.triangles=sprite.triangles.Select(i=>(int)i).ToArray();a.mesh.RecalculateBounds();
                var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);block.SetTexture("_MainTex",sprite.texture);block.SetColor("_ShadowColor",new Color(.015f,.025f,.045f,.38f));a.shadow.SetPropertyBlock(block);
            }
        }
        public static Vector2[] ShadowDirections()=>actors.OrderBy(a=>a.skin==0?0:1).Select(a=>(Vector2)(a.mesh.bounds.center-a.actor.transform.position)).ToArray();
        public static void AuraStep(int form,float clock){if(soloMode)PresentSoloIdle(form,clock);foreach(var a in actors.Where(a=>a.skin>=0)){var aura=a.actor.transform.Find("AuraPequenaContinua");if(aura!=null)aura.GetComponent<DarklightSmallTransformationAura>().Present(a.actor,form,clock,false);}UpdateCharacterShadows();}
        public static void TransitionStep(int form,float progress)
        {
            var previous=root.transform.Find("TransitionBurst");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var oldLight=root.transform.Find("AwakeningLight");if(oldLight!=null)UnityEngine.Object.DestroyImmediate(oldLight.gameObject);
            ApplyTransformation(progress<.52f?0:form);AuraStep(progress<.52f?0:form,progress*2.2f);SetWeaponLight(progress<.52f?0:form);
            if(progress<=0||progress>=1)return;
            var parent=new GameObject("TransitionBurst");parent.transform.SetParent(root.transform,false);
            float rise=Mathf.Clamp01(progress*2.8f),fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,1,progress)),energy=Mathf.Sin(progress*Mathf.PI)*fade;
            Color c=form==3?new Color(.78f,.91f,1):form==2?new Color(.68f,.12f,1):new Color(1,.04f,.09f);c.a=energy;
            Light("AwakeningLight",DarklightCombatLab.Instance.Player.transform.position+Vector3.up,c,energy*2.3f,3.6f);
            foreach(var a in actors.Where(a=>a.skin>=0))
            {
                Vector3 p=a.actor.transform.position;
                // A horizontal sigil expands beneath the full character, then settles away.
                for(int ring=0;ring<2;ring++)
                {
                    var line=new GameObject("AwakeningSigil").AddComponent<LineRenderer>();line.transform.SetParent(parent.transform,false);line.sharedMaterial=effectMaterial;line.widthMultiplier=ring==0?.085f:.035f;line.sortingOrder=9998;line.loop=true;line.positionCount=48;line.startColor=line.endColor=c;
                    for(int i=0;i<48;i++){float angle=i/48f*Mathf.PI*2;float radius=(.35f+rise*.7f)*(ring==0?1:.77f);line.SetPosition(i,p+new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius*.28f));}
                }
                for(int i=0;i<12;i++)
                {
                    float side=i%2==0?-1:1,xx=side*(.48f+i%3*.19f),height=.65f+rise*(2.1f+i%4*.35f);
                    if(form==2){Vector3 pos=p+new Vector3(xx,.3f+Mathf.Repeat(progress*2+i*.33f,1)*height);Diamond(parent.transform,pos,.105f*energy,c,11012);Diamond(parent.transform,pos,.035f*energy,new Color(.91f,.72f,1,energy),11013);continue;}
                    if(form==1){Flame(parent.transform,p+Vector3.right*xx,height,.09f,progress*7+i,c,11012);continue;}
                    var line=new GameObject("SilverLightning").AddComponent<LineRenderer>();line.transform.SetParent(parent.transform,false);line.sharedMaterial=effectMaterial;line.positionCount=9;line.sortingOrder=11012;line.startColor=c;line.endColor=new Color(c.r,c.g,c.b,0);line.widthMultiplier=.055f;
                    for(int j=0;j<9;j++){float t=j/8f;float bend=Mathf.Sin(t*7+progress*15+i)*(.035f+t*.1f);line.SetPosition(j,p+new Vector3(xx+bend,.1f+t*height));}
                }
                if(form==2)
                {
                    Ribbon(parent.transform,"VioletAscent",p+Vector3.up*1.4f,.85f,progress*5,progress*5+4.5f,.11f,c,11011);
                    for(int i=0;i<4;i++)Streak(parent.transform,p+new Vector3((i-1.5f)*.3f,.1f),p+new Vector3((i-1.5f)*.35f,2.5f*rise),.025f,c,11010);
                }
                for(int i=0;i<8;i++){float angle=i*2.39996f+progress*3;Vector3 d=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle));Vector3 center=p+Vector3.up*1.4f;Streak(parent.transform,center+d*.8f,center+d*(.9f+energy*.55f),.018f,c,11013);}
            }
            UpdateCharacterShadows();
        }
        static void Flame(Transform parent,Vector3 origin,float height,float width,float phase,Color color,int sorting)
        {
            const int n=12;var v=new Vector3[n*2];var colors=new Color[n*2];var indices=new List<int>();
            for(int i=0;i<n;i++)
            {
                float t=i/(n-1f),bend=Mathf.Sin(t*8+phase)*(.02f+t*.15f),half=width*(1-t)*(.5f+.5f*Mathf.Sin(t*8+phase));
                v[i*2]=origin+new Vector3(bend-half,.08f+t*height);v[i*2+1]=origin+new Vector3(bend+half,.08f+t*height);
                colors[i*2]=new Color(color.r,color.g,color.b,color.a*(1-t)*.55f);colors[i*2+1]=Color.Lerp(color,new Color(1,.26f,.06f),.38f);colors[i*2+1].a=color.a*(1-t)*.9f;
                if(i<n-1)indices.AddRange(new[]{i*2,i*2+1,i*2+2,i*2+1,i*2+3,i*2+2});
            }
            Geometry(parent,"AwakeningFlame",v,indices.ToArray(),colors,sorting);
        }
        static void RestoreCharacterPresentation()
        {
            soloMode=false;
            foreach(var a in actors.Where(a=>a.actor!=null)){a.actor.sprite=a.sprite;a.actor.transform.localScale=a.scale;a.actor.SetPropertyBlock(a.block);var aura=a.actor.transform.Find("AuraVampiraOriginal");if(aura!=null)aura.gameObject.SetActive(true);}actors.Clear();
        }
        static MeshRenderer Geometry(Transform parent,string name,Vector3[] points,int[] indices,Color[] colors,int sorting)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var mesh=new Mesh{name=name};mesh.vertices=points;mesh.triangles=indices;mesh.colors=colors;mesh.uv=new Vector2[points.Length];mesh.RecalculateBounds();transient.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=effectMaterial;r.sortingOrder=sorting;return r;
        }
        static void Diamond(Transform parent,Vector3 center,float radius,Color color,int order)
        {
            Geometry(parent,"PixelSpark",new[]{center+Vector3.up*radius*1.8f,center+Vector3.right*radius,center-Vector3.up*radius*1.8f,center-Vector3.right*radius},new[]{0,1,2,0,2,3},Enumerable.Repeat(color,4).ToArray(),order);
        }
        static void Ribbon(Transform parent,string name,Vector3 center,float radius,float start,float end,float width,Color color,int order)
        {
            const int n=32;var v=new Vector3[n*2];var c=new Color[v.Length];var indices=new List<int>();
            for(int i=0;i<n;i++)
            {
                float t=i/(n-1f),angle=Mathf.Lerp(start,end,t),w=width*Mathf.Sin(t*Mathf.PI)*(.3f+.7f*t);Vector3 dir=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle));
                v[2*i]=center+dir*(radius-w);v[2*i+1]=center+dir*(radius+w);
                c[2*i]=new Color(color.r,color.g,color.b,color.a*t*.22f);c[2*i+1]=new Color(color.r,color.g,color.b,color.a*t);
                if(i<n-1)indices.AddRange(new[]{2*i,2*i+1,2*i+2,2*i+1,2*i+3,2*i+2});
            }
            Geometry(parent,name,v,indices.ToArray(),c,order);
        }
        static void Streak(Transform parent,Vector3 start,Vector3 end,float width,Color color,int order)
        {
            Vector3 d=(end-start).normalized,n=new Vector3(-d.y,d.x)*width;
            Geometry(parent,"ImpactStreak",new[]{start+n,start-n,end},new[]{0,1,2},new[]{color,color,new Color(color.r,color.g,color.b,0)},order);
        }
        public static void CombatStep(int weapon,Vector2 facing,float phase)
        {
            var previous=root.transform.Find("AttackPreview");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var oldStrike=root.transform.Find("StrikeLight");if(oldStrike!=null)UnityEngine.Object.DestroyImmediate(oldStrike.gameObject);
            var parent=new GameObject("AttackPreview");parent.transform.SetParent(root.transform,false);SetWeaponLight(0);
            Vector3 center=DarklightCombatLab.Instance.Player.transform.position+Vector3.up*.9f;
            Color color=weapon==2?new Color(.74f,.21f,1):new Color(1,.065f,.12f);Color hot=weapon==2?new Color(1,.7f,1):new Color(1,.86f,.62f);
            float angle=Mathf.Atan2(facing.y,facing.x),head=angle+Mathf.Lerp(-1.3f,1.3f,phase),radius=weapon==2?2.35f:weapon==1?1.1f:1.7f;
            float fade=Mathf.Clamp01((1-phase)*2.5f);color.a=fade;hot.a=fade;
            int count=weapon==1?2:1;
            for(int a=0;a<count;a++)
            {
                Vector3 offset=weapon==1?new Vector3(0,a==0?.32f:-.25f):Vector3.zero;float side=a==1?-.35f:0;
                Ribbon(parent.transform,"SlashWake",center+offset,radius,head-1.6f+side,head+side,.22f,color,11001);
                Ribbon(parent.transform,"HotCuttingEdge",center+offset,radius+.025f,head-1.25f+side,head+side,.032f,hot,11002);
                Ribbon(parent.transform,"TrailingFracture",center+offset,radius-.17f,head-1.65f+side,head-.18f+side,.025f,color,11000);
            }
            if(weapon==2)
            {
                for(int i=0;i<18;i++)
                {
                    float t=i/17f,aa=Mathf.Lerp(head-1.55f,head,t);Vector3 pos=center+new Vector3(Mathf.Cos(aa),Mathf.Sin(aa))*radius;
                    float rotation=aa+(i%2==0?0:Mathf.PI*.5f);Vector3 along=new Vector3(Mathf.Cos(rotation),Mathf.Sin(rotation))*.075f,across=new Vector3(-along.y,along.x)*.52f;
                    var line=new GameObject("SpectralChainLink").AddComponent<LineRenderer>();line.transform.SetParent(parent.transform,false);line.sharedMaterial=effectMaterial;line.widthMultiplier=.019f;line.loop=true;line.positionCount=4;line.SetPositions(new[]{pos+along,pos+across,pos-along,pos-across});line.startColor=line.endColor=hot;line.sortingOrder=11003;
                }
            }
            Vector3 hit=center+(Vector3)facing*(radius+.05f);float burst=Mathf.Clamp01((phase-.27f)*3);
            if(burst>0)
            {
                Ribbon(parent.transform,"ImpactRing",hit,.15f+burst*.45f,0,Mathf.PI*2,.024f,color,11005);
                for(int i=0;i<16;i++)
                {
                    float aa=i*2.39996f;Vector3 d=new Vector3(Mathf.Cos(aa),Mathf.Sin(aa));
                    Streak(parent.transform,hit+d*.08f,hit+d*(.25f+burst*(.3f+i%4*.13f)),.018f+i%3*.008f,i%3==0?hot:color,11006);
                    Diamond(parent.transform,hit+d*(.18f+burst*.65f),.018f,hot,11007);
                }
                Diamond(parent.transform,hit,.095f*fade,hot,11008);
            }
            Light("StrikeLight",hit,color,fade*1.4f,2.5f);UpdateCharacterShadows();
        }
    }
}
