using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
namespace Darklight.Editor
{
    [InitializeOnLoad]
    public static partial class RemasterPrototype
    {
        const string Folder="Assets/Darklight/Editor/RemasterPrototype/";
        const string Output="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-prototype-20261014/";
        static RenderPipelineAsset graphics,quality;
        static Camera camera;
        static bool enabled,oldHDR;
        static Vector3 oldCameraPosition;static float oldCameraSize,oldCameraAspect;
        static readonly Dictionary<SpriteRenderer,Material> oldMaterials=new();
        static readonly List<UnityEngine.Object> transient=new();
        static UniversalAdditionalCameraData addedCamera;
        static bool ownsCameraData,oldPostProcessing;static LayerMask oldVolumeMask;static int oldCullingMask;static CameraClearFlags oldClearFlags;static Color oldBackground;
        static readonly Dictionary<Behaviour,bool> productionBehaviours=new();
        static readonly Dictionary<GameObject,int> originalLayers=new();
        static GameObject root;
        [Serializable] class PipelineProof{public string graphics,quality;public int level;public string[] qualityAssets;}
        [Serializable] class MaterialProof{public string path,guid,shader,mainTexture,normal,metallicGloss,specGloss,emission;public Color color,emissionColor;public Vector2 scale,offset;public float metallic,glossiness,cutoff,mode;}
        static RemasterPrototype(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.ExitingPlayMode)Restore();};}
        static string PathOf(UnityEngine.Object o)=>o==null?"":AssetDatabase.GetAssetPath(o);
        static PipelineProof Settings()=>new PipelineProof{graphics=PathOf(GraphicsSettings.defaultRenderPipeline),quality=PathOf(QualitySettings.renderPipeline),level=QualitySettings.GetQualityLevel(),qualityAssets=Enumerable.Range(0,QualitySettings.names.Length).Select(i=>PathOf(QualitySettings.GetRenderPipelineAssetAt(i))).ToArray()};
        static T AddOverride<T>(VolumeProfile p)where T:VolumeComponent
        {
            if(p.TryGet<T>(out var existing))return existing;
            var c=p.Add<T>(true);c.name=typeof(T).Name;AssetDatabase.AddObjectToAsset(c,p);EditorUtility.SetDirty(c);EditorUtility.SetDirty(p);return c;
        }
        public static string SetupAssets()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Prepare prototype assets in edit mode");
            Directory.CreateDirectory(Output);Directory.CreateDirectory(Folder);
            File.WriteAllText(Output+"pipeline-before.json",JsonUtility.ToJson(Settings(),true));
            var proofs=new List<MaterialProof>();
            foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Darklight"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                Texture Tex(string name)=>m.HasProperty(name)?m.GetTexture(name):null;
                float Number(string name)=>m.HasProperty(name)?m.GetFloat(name):0;
                Color Colour(string name)=>m.HasProperty(name)?m.GetColor(name):Color.white;
                var proof=new MaterialProof{path=path,guid=guid,shader=m.shader.name,mainTexture=PathOf(Tex("_MainTex")),normal=PathOf(Tex("_BumpMap")),metallicGloss=PathOf(Tex("_MetallicGlossMap")),specGloss=PathOf(Tex("_SpecGlossMap")),emission=PathOf(Tex("_EmissionMap")),color=Colour("_Color"),emissionColor=Colour("_EmissionColor"),metallic=Number("_Metallic"),glossiness=Number("_Glossiness"),cutoff=Number("_Cutoff"),mode=Number("_Mode"),scale=m.HasProperty("_MainTex")?m.GetTextureScale("_MainTex"):Vector2.one,offset=m.HasProperty("_MainTex")?m.GetTextureOffset("_MainTex"):Vector2.zero};proofs.Add(proof);
            }
            File.WriteAllText(Output+"materials-before.json","["+string.Join(",",proofs.Select(p=>JsonUtility.ToJson(p,true)))+"]");
            var renderer=AssetDatabase.LoadAssetAtPath<Renderer2DData>(Folder+"Renderer2D.asset");
            if(renderer==null){renderer=ScriptableObject.CreateInstance<Renderer2DData>();AssetDatabase.CreateAsset(renderer,Folder+"Renderer2D.asset");}
            if(renderer.lightBlendStyles==null||renderer.lightBlendStyles.Length==0)throw new InvalidOperationException("2D light styles missing");
            var pipe=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Folder+"Pipeline.asset");
            if(pipe==null){pipe=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipe,Folder+"Pipeline.asset");}
            pipe.msaaSampleCount=1;pipe.renderScale=1;pipe.supportsHDR=true;pipe.useSRPBatcher=true;
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder+"Volume.asset");
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Folder+"Volume.asset");}
            var grade=AddOverride<ColorAdjustments>(profile);grade.postExposure.Override(.15f);grade.contrast.Override(9);grade.saturation.Override(-3);grade.colorFilter.Override(new Color(.93f,.97f,1));
            var bloom=AddOverride<Bloom>(profile);bloom.threshold.Override(.88f);bloom.intensity.Override(.14f);bloom.scatter.Override(.42f);
            var tone=AddOverride<Tonemapping>(profile);tone.mode.Override(TonemappingMode.Neutral);
            var vignette=AddOverride<Vignette>(profile);vignette.intensity.Override(.17f);vignette.smoothness.Override(.65f);
            EditorUtility.SetDirty(renderer);EditorUtility.SetDirty(pipe);EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();AssetDatabase.ImportAsset(Folder+"Volume.asset");
            var saved=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder+"Volume.asset");
            if(saved.components.Count(c=>c!=null)!=4)throw new InvalidOperationException("Post-processing overrides did not persist");
            if(pipe.rendererDataList.Length==0||pipe.rendererDataList[0]!=renderer)throw new InvalidOperationException("2D renderer not valid");
            return "Prototype assets saved; graphics pipeline remains "+(GraphicsSettings.defaultRenderPipeline==null?"Built-in":GraphicsSettings.defaultRenderPipeline.name);
        }
        static Light2D Light(string name,Vector2 position,Color color,float intensity,float radius,bool global=false)
        {
            var o=new GameObject(name);o.transform.SetParent(root.transform,false);o.transform.position=position;
            var light=o.AddComponent<Light2D>();light.lightType=global?Light2D.LightType.Global:Light2D.LightType.Point;
            light.color=color;light.intensity=intensity;light.lightOrder=0;light.blendStyleIndex=0;
            if(!global){light.pointLightOuterRadius=radius;light.pointLightInnerRadius=radius*.1f;light.falloffIntensity=.68f;light.shadowIntensity=.65f;}
            var so=new SerializedObject(light);var layers=so.FindProperty("m_ApplyToSortingLayers");
            if(layers==null)throw new InvalidOperationException("Light sorting layer interface changed");
            layers.arraySize=SortingLayer.layers.Length;for(int i=0;i<layers.arraySize;i++)layers.GetArrayElementAtIndex(i).intValue=SortingLayer.layers[i].id;so.ApplyModifiedPropertiesWithoutUndo();
            return light;
        }
        public static string Enable()
        {
            if(!EditorApplication.isPlaying||enabled)throw new InvalidOperationException("Prototype needs a protected play session");
            graphics=GraphicsSettings.defaultRenderPipeline;quality=QualitySettings.renderPipeline;enabled=true;
            var game=DarklightCampaign.Instance;camera=DarklightCombatLab.Instance.WorldCamera;oldHDR=camera.allowHDR;oldCameraPosition=camera.transform.position;oldCameraSize=camera.orthographicSize;oldCameraAspect=camera.aspect;
            oldCullingMask=camera.cullingMask;oldClearFlags=camera.clearFlags;oldBackground=camera.backgroundColor;
            foreach(var component in UnityEngine.Object.FindObjectsByType<Behaviour>(FindObjectsSortMode.None))
                if(component is DarklightRemaster||component is Light2D||component is Volume){productionBehaviours[component]=component.enabled;component.enabled=false;}
            var pipe=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Folder+"Pipeline.asset");
            if(pipe==null)throw new InvalidOperationException("Prepare assets before prototype");
            GraphicsSettings.defaultRenderPipeline=pipe;QualitySettings.renderPipeline=pipe;
            var shader=Shader.Find("Darklight/Prototype/YoungSpriteLit");
            if(shader==null||!shader.isSupported)throw new InvalidOperationException("Young sprite lighting shader is unavailable");
            var clones=new Dictionary<Material,Material>();
            foreach(var sprite in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if(sprite.sharedMaterial==null)continue;oldMaterials[sprite]=sprite.sharedMaterial;
                if(!clones.TryGetValue(sprite.sharedMaterial,out var lit))
                {
                    var source=sprite.sharedMaterial;lit=new Material(shader);lit.name="Prototype_"+source.name;
                    if(source.HasProperty("_MainTex")){lit.SetTexture("_MainTex",source.GetTexture("_MainTex"));lit.SetTextureScale("_MainTex",source.GetTextureScale("_MainTex"));lit.SetTextureOffset("_MainTex",source.GetTextureOffset("_MainTex"));}
                    if(source.HasProperty("_Color"))lit.SetColor("_Color",source.GetColor("_Color"));clones[source]=lit;transient.Add(lit);
                }
                sprite.sharedMaterial=lit;
            }
            root=new GameObject("RemasterPrototype_ReviewOnly");
            var volume=root.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder+"Volume.asset");
            addedCamera=camera.GetComponent<UniversalAdditionalCameraData>();ownsCameraData=addedCamera==null;if(ownsCameraData)addedCamera=camera.gameObject.AddComponent<UniversalAdditionalCameraData>();oldPostProcessing=addedCamera.renderPostProcessing;oldVolumeMask=addedCamera.volumeLayerMask;addedCamera.renderPostProcessing=true;addedCamera.SetRenderer(0);camera.allowHDR=true;
            Vector2 p=DarklightCombatLab.Instance.Player.transform.position;
            Light("AmbientBlue",p,new Color(.42f,.57f,.86f),.65f,0,true);
            Light("WarmKey",p+new Vector2(-3.8f,2.3f),new Color(1,.52f,.2f),.95f,6.5f);
            Light("ColdMoon",p+new Vector2(4.5f,1.5f),new Color(.2f,.46f,1),1.6f,6);
            Light("LanternWarm",p+new Vector2(.6f,-1.6f),new Color(1,.62f,.28f),1.65f,5);
            Light("WeaponCrimson",p+new Vector2(.55f,.9f),new Color(1,.08f,.16f),.7f,2.8f);
            Light("MemoryTeal",game.Map.Ally+Vector2.up,new Color(.1f,.85f,.72f),.9f,4.5f);
            int shadows=0;
            foreach(var collider in UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
            {
                if(collider.isTrigger||collider.GetComponent<ShadowCaster2D>()!=null)continue;
                if(Vector2.Distance(collider.bounds.center,p)>12)continue;
                var caster=collider.gameObject.AddComponent<ShadowCaster2D>();transient.Add(caster);shadows++;
            }
            var embers=new GameObject("Embers");embers.transform.SetParent(root.transform,false);embers.transform.position=(Vector3)p+Vector3.up;
            var particles=embers.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(3,5);main.startSpeed=new ParticleSystem.MinMaxCurve(.08f,.22f);main.startSize=new ParticleSystem.MinMaxCurve(.018f,.035f);main.startColor=new Color(1,.45f,.13f,.3f);main.maxParticles=70;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=particles.emission;emission.rateOverTime=12;
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(14,7,.01f);
            var renderer=particles.GetComponent<ParticleSystemRenderer>();var particleMaterial=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));transient.Add(particleMaterial);renderer.sharedMaterial=particleMaterial;renderer.sortingOrder=500;
            particles.useAutoRandomSeed=false;particles.randomSeed=17;particles.Simulate(3,true,true);particles.Play();
            SetupCharacterPresentation();
            return "Native2D lights6; shadow casters"+shadows+"; moving character silhouette shadows; saved volume4 overrides; atmospheric embers";
        }
        public static void SetWeaponLight(int form)
        {
            var light=root==null?null:root.transform.Find("WeaponCrimson")?.GetComponent<Light2D>();
            if(light!=null){light.color=form==3?new Color(.72f,.85f,1):form==2?new Color(.68f,.15f,1):new Color(1,.08f,.16f);light.intensity=form==0?.55f:.85f;}
        }
        public static void AttackPreview(int weapon,Vector2 facing)=>CombatStep(weapon,facing,.58f);
        public static void CinematicCameraStep(float dt,Vector2 facing)
        {
            Vector3 target=oldCameraPosition+(Vector3)facing*.7f;
            camera.orthographicSize=Mathf.Lerp(camera.orthographicSize,oldCameraSize*.94f,1-Mathf.Exp(-dt*5));
            Vector3 p=Vector3.Lerp(camera.transform.position,target,1-Mathf.Exp(-dt*7));
            float grid=camera.targetTexture.height/(camera.orthographicSize*2);p.x=Mathf.Round(p.x*grid)/grid;p.y=Mathf.Round(p.y*grid)/grid;camera.transform.position=p;
        }
        public static string Restore()
        {
            if(!enabled)return "Already restored";
            RestoreCharacterPresentation();
            foreach(var pair in oldMaterials)if(pair.Key!=null)pair.Key.sharedMaterial=pair.Value;oldMaterials.Clear();
            foreach(var o in transient)if(o!=null)UnityEngine.Object.DestroyImmediate(o);transient.Clear();
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);root=null;
            if(addedCamera!=null){if(ownsCameraData)UnityEngine.Object.DestroyImmediate(addedCamera);else{addedCamera.renderPostProcessing=oldPostProcessing;addedCamera.volumeLayerMask=oldVolumeMask;}}addedCamera=null;
            foreach(var pair in originalLayers)if(pair.Key!=null)pair.Key.layer=pair.Value;originalLayers.Clear();
            foreach(var pair in productionBehaviours)if(pair.Key!=null)pair.Key.enabled=pair.Value;productionBehaviours.Clear();
            if(camera!=null){camera.allowHDR=oldHDR;camera.cullingMask=oldCullingMask;camera.clearFlags=oldClearFlags;camera.backgroundColor=oldBackground;camera.transform.position=oldCameraPosition;camera.orthographicSize=oldCameraSize;camera.aspect=oldCameraAspect;camera.ResetProjectionMatrix();}
            GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;enabled=false;
            File.WriteAllText(Output+"pipeline-restored.json",JsonUtility.ToJson(Settings(),true));
            return "Original graphics and quality pipelines restored";
        }
    }
}

