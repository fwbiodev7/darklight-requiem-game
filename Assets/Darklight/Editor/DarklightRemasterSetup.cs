using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight.Editor
{
    public static class DarklightRemasterSetup
    {
        const string Folder="Assets/Darklight/Resources/Darklight/Rendering/";
        const string Prototype="Assets/Darklight/Editor/RemasterPrototype/";
        const string Output="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-production-20261014/";
        [Serializable] sealed class Snapshot {public MaterialRecord[] materials;public string graphics;public string[] quality;}
        [Serializable] sealed class MaterialRecord {public string path,guid,shader,mainTexture,normal,metallicGloss,specGloss,emission;public Color color,emissionColor,specColor;public Vector2 scale,offset;public float metallic,glossiness,cutoff,mode;}
        static string PathOf(UnityEngine.Object value)=>value==null?"":AssetDatabase.GetAssetPath(value);
        public static string Prepare()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            if(!File.Exists(Output+"materials-before.json"))
            {
                var snapshots=AssetDatabase.FindAssets("t:Material",new[]{"Assets/Darklight"}).Select(guid=>
                {
                    var path=AssetDatabase.GUIDToAssetPath(guid);var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                    string Tex(string key)=>m.HasProperty(key)?PathOf(m.GetTexture(key)):"";float Scalar(string key)=>m.HasProperty(key)?m.GetFloat(key):0;Color Colour(string key)=>m.HasProperty(key)?m.GetColor(key):Color.white;
                    return new MaterialRecord{path=path,guid=guid,shader=m.shader.name,mainTexture=Tex("_MainTex"),normal=Tex("_BumpMap"),metallicGloss=Tex("_MetallicGlossMap"),specGloss=Tex("_SpecGlossMap"),emission=Tex("_EmissionMap"),color=Colour("_Color"),emissionColor=Colour("_EmissionColor"),specColor=Colour("_SpecColor"),scale=m.HasProperty("_MainTex")?m.GetTextureScale("_MainTex"):Vector2.one,offset=m.HasProperty("_MainTex")?m.GetTextureOffset("_MainTex"):Vector2.zero,metallic=Scalar("_Metallic"),glossiness=Scalar("_Glossiness"),cutoff=Scalar("_Cutoff"),mode=Scalar("_Mode")};
                }).ToArray();
                File.WriteAllText(Output+"materials-before.json",JsonUtility.ToJson(new Snapshot{materials=snapshots,graphics=PathOf(GraphicsSettings.defaultRenderPipeline),quality=Enumerable.Range(0,QualitySettings.names.Length).Select(i=>PathOf(QualitySettings.GetRenderPipelineAssetAt(i))).ToArray()},true));
            }
            if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Darklight/Resources/Darklight","Rendering");
            foreach(string name in new[]{"Renderer2D.asset","Pipeline.asset","Volume.asset"})if(AssetDatabase.LoadMainAssetAtPath(Folder+name)==null&&!AssetDatabase.CopyAsset(Prototype+name,Folder+name))throw new InvalidOperationException("Cannot copy "+name);
            var renderer=AssetDatabase.LoadAssetAtPath<Renderer2DData>(Folder+"Renderer2D.asset");
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Folder+"Pipeline.asset");
            var so=new SerializedObject(pipeline);var list=so.FindProperty("m_RendererDataList");if(list==null)throw new InvalidOperationException("URP renderer interface unavailable");list.arraySize=1;list.GetArrayElementAtIndex(0).objectReferenceValue=renderer;so.FindProperty("m_DefaultRendererIndex").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();pipeline.msaaSampleCount=1;pipeline.renderScale=1;pipeline.supportsHDR=true;
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder+"Volume.asset");
            if(profile.components.Count!=4||profile.components.Any(c=>PathOf(c)!=Folder+"Volume.asset"))throw new InvalidOperationException("Volume overrides must be persisted in production asset");
            CreateMaterial("SpriteLit.mat","Darklight/Remaster/SpriteLit");CreateMaterial("WholeShadow.mat","Darklight/Remaster/WholeShadow");
            GraphicsSettings.defaultRenderPipeline=pipeline;
            int quality=QualitySettings.GetQualityLevel();
            for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}
            QualitySettings.SetQualityLevel(quality,false);
            EditorUtility.SetDirty(renderer);EditorUtility.SetDirty(pipeline);EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();
            var before=JsonUtility.FromJson<Snapshot>(File.ReadAllText(Output+"materials-before.json"));
            foreach(var record in before.materials)
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>(record.path);if(m==null||AssetDatabase.AssetPathToGUID(record.path)!=record.guid||m.shader.name!=record.shader)throw new InvalidOperationException("Source material changed: "+record.path);
                if(m.HasProperty("_MainTex")&&(PathOf(m.GetTexture("_MainTex"))!=record.mainTexture||m.GetTextureScale("_MainTex")!=record.scale||m.GetTextureOffset("_MainTex")!=record.offset))throw new InvalidOperationException("Source maps changed: "+record.path);
                if(m.HasProperty("_Color")&&m.GetColor("_Color")!=record.color)throw new InvalidOperationException("Source tint changed: "+record.path);
            }
            string result="{\"success\":true,\"sourceMaterialsPreserved\":true,\"materials\":"+before.materials.Length+",\"qualityLevels\":"+QualitySettings.names.Length+",\"persistedVolumeOverrides\":4}";File.WriteAllText(Output+"migration-result.json",result);return result;
        }
        static void CreateMaterial(string file,string name)
        {
            if(AssetDatabase.LoadAssetAtPath<Material>(Folder+file)!=null)return;
            var shader=Shader.Find(name);if(shader==null||!shader.isSupported)throw new InvalidOperationException("Unsupported shader: "+name);AssetDatabase.CreateAsset(new Material(shader),Folder+file);
        }
    }
}
