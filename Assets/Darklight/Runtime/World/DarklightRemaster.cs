using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    [DefaultExecutionOrder(1800)]
    public sealed class DarklightRemaster:MonoBehaviour
    {
        public static readonly string[] QualityNames={"BAIXA","MEDIA","ALTA","ULTRA"};
        static int previewQuality=-1;
        public static int QualityLevel=>previewQuality>=0?previewQuality:Mathf.Clamp(PlayerPrefs.GetInt("Darklight.VisualQuality",2),0,3);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetPreviewQuality()=>previewQuality=-1;
        Camera camera;Transform player,mapRoot;Volume volume;ParticleSystem embers;
        readonly List<Light2D> lamps=new();readonly List<DarklightWholeShadow> shadows=new();
        readonly Dictionary<Material,Material> clones=new();Material spriteTemplate,shadowMaterial;
        float nextScan;int mapChapter=-1;Light2D weaponLight;
        public void Initialize(Camera camera,Transform player,Transform mapRoot,int chapter)
        {
            this.camera=camera;this.player=player;this.mapRoot=mapRoot;mapChapter=chapter;
            spriteTemplate??=Resources.Load<Material>("Darklight/Rendering/SpriteLit");shadowMaterial??=Resources.Load<Material>("Darklight/Rendering/WholeShadow");
            if(spriteTemplate==null||shadowMaterial==null)return;
            if(volume==null)
            {
                volume=gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=Resources.Load<VolumeProfile>("Darklight/Rendering/Volume");
                var data=camera.GetComponent<UniversalAdditionalCameraData>()??camera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.SetRenderer(0);data.renderPostProcessing=QualityLevel>0;data.antialiasing=AntialiasingMode.None;data.volumeLayerMask=1;
                camera.allowHDR=true;camera.allowMSAA=false;camera.allowDynamicResolution=false;
                CreateEmbers();
            }
            foreach(var light in lamps)if(light!=null){light.enabled=false;Destroy(light.gameObject);}lamps.Clear();
            Point("LuaAmbiente",player.position,new Color(.42f,.57f,.86f),.7f,0,true);
            Point("LuzFria",player.position+new Vector3(4.5f,1.5f),new Color(.2f,.46f,1),1.1f,7);
            Point("LuzQuente",player.position+new Vector3(-3.8f,2.3f),new Color(1,.52f,.2f),.85f,6.5f);
            if(DarklightCampaign.Instance?.Map!=null)
            {
                var map=DarklightCampaign.Instance.Map;
                foreach(var p in map.Checkpoints)Point("Lamparina",p+new Vector2(.6f,-1.6f),new Color(1,.62f,.28f),1.4f,5);
                Point("Memoria",map.Ally+Vector2.up,new Color(.1f,.85f,.72f),.85f,4.5f);
                Point("Portal",map.Portal,new Color(.58f,.16f,1),.8f,4);
            }
            weaponLight=Point("LuzDaArma",player.position,new Color(1,.08f,.16f),.45f,2.8f);
            ApplyQuality();SyncScene();nextScan=0;
        }
        Light2D Point(string name,Vector3 point,Color color,float intensity,float radius,bool global=false)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.position=point;
            var light=go.AddComponent<Light2D>();light.lightType=global?Light2D.LightType.Global:Light2D.LightType.Point;light.color=color;light.intensity=intensity;
            if(!global){light.pointLightOuterRadius=radius;light.pointLightInnerRadius=.15f;light.shadowIntensity=.6f;}
            lamps.Add(light);return light;
        }
        void CreateEmbers()
        {
            var go=new GameObject("ParticulasDoEclipse");go.transform.SetParent(transform,false);embers=go.AddComponent<ParticleSystem>();embers.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=embers.main;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(3,5);main.startSpeed=new ParticleSystem.MinMaxCurve(.08f,.22f);main.startSize=new ParticleSystem.MinMaxCurve(.018f,.035f);main.startColor=new Color(1,.45f,.13f,.3f);main.maxParticles=70;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=embers.emission;emission.rateOverTime=12;var shape=embers.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(16,9,1);
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");renderer.sortingOrder=500;embers.Play();
        }
        public void SetQuality(int quality,bool persist=true)
        {
            quality=Mathf.Clamp(quality,0,3);previewQuality=persist?-1:quality;
            if(persist){PlayerPrefs.SetInt("Darklight.VisualQuality",quality);PlayerPrefs.Save();}ApplyQuality();
        }
        void ApplyQuality()
        {
            int q=QualityLevel;int limit=new[]{4,8,12,16}[q];
            for(int i=0;i<lamps.Count;i++)lamps[i].enabled=i<limit||lamps[i]==weaponLight;
            if(volume!=null){volume.enabled=q>0;volume.weight=q==1?.65f:1;}
            if(camera!=null){var data=camera.GetComponent<UniversalAdditionalCameraData>();if(data!=null)data.renderPostProcessing=q>0;}
            if(embers!=null){var main=embers.main;main.maxParticles=new[]{20,40,70,96}[q];var emission=embers.emission;emission.rateOverTime=new[]{3,7,12,16}[q];}
        }
        public void SyncScene()
        {
            foreach(var r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if(r.sharedMaterial==null||r.sharedMaterial.shader.name!="Darklight/PixelSprite")continue;
                var source=r.sharedMaterial;
                if(!clones.TryGetValue(source,out var lit))
                {
                    lit=new Material(spriteTemplate){name="Remaster_"+source.name};
                    if(source.HasProperty("_MainTex")){lit.SetTexture("_MainTex",source.GetTexture("_MainTex"));lit.SetTextureScale("_MainTex",source.GetTextureScale("_MainTex"));lit.SetTextureOffset("_MainTex",source.GetTextureOffset("_MainTex"));}
                    if(source.HasProperty("_Color"))lit.SetColor("_Color",source.GetColor("_Color"));if(source.HasProperty("_AlphaClip"))lit.SetFloat("_AlphaClip",source.GetFloat("_AlphaClip"));clones.Add(source,lit);
                }
                r.sharedMaterial=lit;
                bool actor=r.name=="FabioVisual"||r.GetComponentInParent<DarklightEnemy>()!=null||r.name=="IcaroNox"||r.name=="LyraMoreau"||r.name=="KaelAliado";
                if(actor&&r.GetComponent<DarklightWholeShadow>()==null){var shadow=r.gameObject.AddComponent<DarklightWholeShadow>();shadow.Initialize(r,shadowMaterial,this);shadows.Add(shadow);}
            }
            if(mapRoot!=null)foreach(var collider in mapRoot.GetComponentsInChildren<Collider2D>())
            {
                if(collider.isTrigger||collider.GetComponent<ShadowCaster2D>()!=null)continue;
                collider.gameObject.AddComponent<ShadowCaster2D>();
            }
        }
        public Vector2 ShadowDirection(Vector2 foot,out float length)
        {
            Light2D key=null;float score=0;
            foreach(var light in lamps)
            {
                if(light==null||!light.enabled||light.lightType==Light2D.LightType.Global||light==weaponLight)continue;
                float candidate=light.intensity/(1+((Vector2)light.transform.position-foot).sqrMagnitude);if(candidate>score){score=candidate;key=light;}
            }
            if(key==null){length=.5f;return new Vector2(.8f,-.6f);}
            var away=foot-(Vector2)key.transform.position;length=Mathf.Clamp(away.magnitude*.35f,.55f,1.5f);return away.normalized;
        }
        void LateUpdate()
        {
            if(spriteTemplate==null||player==null)return;
            float time=DarklightCombatLab.Instance?.Player.Model.Clock??Time.time;
            if(weaponLight!=null)
            {
                weaponLight.transform.position=player.position+Vector3.up*.9f;int form=DarklightCampaign.Instance?.PresentedForm??0;
                weaponLight.color=form==3?new Color(.72f,.9f,1):form==2?new Color(.64f,.15f,1):new Color(1,.08f,.16f);
                weaponLight.intensity=form>0?.7f:.3f;
            }
            for(int i=3;i<lamps.Count-1;i++)if(lamps[i]!=null)lamps[i].intensity=1.15f+Mathf.Sin(time*2.1f+i)*.055f;
            if(embers!=null){embers.transform.position=camera.transform.position+Vector3.forward*10;var main=embers.main;main.simulationSpeed=DarklightCampaign.Instance?.Active==true?1:0;}
            if(Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.35f;SyncScene();}
        }
        void OnDestroy(){foreach(var material in clones.Values)Destroy(material);}
    }
    [DefaultExecutionOrder(2000)] public sealed class DarklightWholeShadow:MonoBehaviour
    {
        SpriteRenderer source;DarklightRemaster lights;Mesh mesh;MeshRenderer display;Sprite previous;
        Vector2[] points;Vector3[] vertices;int[] triangles;MaterialPropertyBlock block;
        public void Initialize(SpriteRenderer source,Material material,DarklightRemaster lights)
        {
            this.source=source;this.lights=lights;var go=new GameObject("SombraDaFiguraInteira");go.transform.SetParent(source.transform.parent,false);
            mesh=new Mesh{name="Whole moving silhouette"};mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;display=go.AddComponent<MeshRenderer>();display.sharedMaterial=material;block=new MaterialPropertyBlock();
        }
        void LateUpdate()
        {
            if(source==null||display==null)return;
            display.enabled=source.enabled&&source.sprite!=null&&DarklightRemaster.QualityLevel>0&&source.gameObject.activeInHierarchy;
            if(!display.enabled)return;
            if(previous!=source.sprite)
            {
                previous=source.sprite;points=previous.vertices;vertices=new Vector3[points.Length];var input=previous.triangles;triangles=new int[input.Length];for(int i=0;i<input.Length;i++)triangles[i]=input[i];mesh.Clear();mesh.vertices=vertices;mesh.uv=previous.uv;mesh.triangles=triangles;
            }
            Vector2 foot=source.transform.position;Vector2 away=lights.ShadowDirection(foot,out float length);
            for(int i=0;i<points.Length;i++){Vector3 point=source.transform.TransformPoint(points[i]);float h=Mathf.Max(0,point.y-foot.y);Vector3 world=foot+new Vector2((point.x-foot.x)*.8f,0)+away*h*length;vertices[i]=display.transform.InverseTransformPoint(world);}
            mesh.vertices=vertices;mesh.RecalculateBounds();display.sortingLayerID=source.sortingLayerID;display.sortingOrder=source.sortingOrder-2;
            source.GetPropertyBlock(block);block.SetTexture("_MainTex",previous.texture);block.SetColor("_ShadowColor",new Color(.015f,.025f,.045f,.35f));display.SetPropertyBlock(block);
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(display!=null)Destroy(display.gameObject);}
    }
}
