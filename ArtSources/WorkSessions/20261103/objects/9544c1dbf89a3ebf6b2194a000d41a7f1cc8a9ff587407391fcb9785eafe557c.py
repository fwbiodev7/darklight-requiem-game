from pathlib import Path
import hashlib, json, difflib

root=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba/outputs/darklight-requiem-game')
stage=root.parent/'gothic-eclipse-20261103/start'
runtime='Assets/Darklight/Runtime/Campaign/'
shader='Assets/Darklight/Resources/Darklight/Rendering/AtmosphereOverlay.shader'
controller=runtime+'DarklightAtmosphereController.cs'
sources={p:(root/p).read_bytes() for p in (controller,shader)}
out={}
out[runtime+'DarklightAtmospherePreset.cs']='''using UnityEngine;

namespace Darklight
{
    // First-map scenery preset. Exposure never touches the global Volume or actors.
    [CreateAssetMenu(fileName="GothicEclipseRuaDasCinzas", menuName="Darklight/Atmosphere/Gothic Eclipse preset")]
    public sealed class DarklightAtmospherePreset : ScriptableObject
    {
        public const string RuaResourcePath="Darklight/Atmosphere/GothicEclipseRuaDasCinzas";
        [SerializeField] string displayName="Gothic Eclipse / Rua das Cinzas";
        [Header("Scenery palette only")]
        [SerializeField] Color groundPalette=new Color(.72f,.80f,.93f);
        [SerializeField, Range(0,1)] float groundStrength=.32f;
        [SerializeField, Range(-.5f,.5f)] float mapExposureStops=-.15f;
        [Header("Existing low fog overlay")]
        [SerializeField] Color fogColor=new Color(.55f,.67f,.79f);
        [SerializeField, Range(0,.1f)] float fogOpacity=.055f;
        [SerializeField, Range(0,2)] int fogVariant=0;
        [SerializeField, Range(.2f,.6f)] float fogHeight=.38f;
        [SerializeField, Range(0,.1f)] float fogDriftSpeed=.035f;
        [Header("Existing moon overlay")]
        [SerializeField] Color moonColor=new Color(.65f,.78f,.98f);
        [SerializeField, Range(0,.08f)] float moonOpacity=.045f;
        [SerializeField, Range(0,2)] int moonVariant=1;
        [Header("Existing rain pool; exterior only")]
        [SerializeField, Range(0,74)] int rainDrops=64;
        [SerializeField, Range(-1,1)] float wind=.40f;

        public string DisplayName=>string.IsNullOrEmpty(displayName)?"Gothic Eclipse":displayName;
        public Color GroundPalette=>groundPalette;
        public float GroundStrength=>Mathf.Clamp01(groundStrength);
        public float MapExposureStops=>Mathf.Clamp(mapExposureStops,-.5f,.5f);
        public Color FogColor=>fogColor;
        public float FogOpacity=>Mathf.Clamp(fogOpacity,0,.1f);
        public int FogVariant=>Mathf.Clamp(fogVariant,0,2);
        public float FogHeight=>Mathf.Clamp(fogHeight,.2f,.6f);
        public float FogDriftSpeed=>Mathf.Clamp(fogDriftSpeed,0,.1f);
        public Color MoonColor=>moonColor;
        public float MoonOpacity=>Mathf.Clamp(moonOpacity,0,.08f);
        public int MoonVariant=>Mathf.Clamp(moonVariant,0,2);
        public int RainDrops=>Mathf.Clamp(rainDrops,0,74);
        public float Wind=>Mathf.Clamp(wind,-1,1);
    }
}
'''
out['Assets/Darklight/Editor/DarklightAtmospherePresetFactory.cs']='''using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightAtmospherePresetFactory
    {
        // Optional: runtime uses identical defaults without an authored asset.
        // AssetDatabase creates its own GUID; never hand-author a .meta file.
        [MenuItem("Darklight/Atmosphere/Create or select Rua das Cinzas preset")]
        public static void CreateOrSelectRuaPreset()
        {
            const string parent="Assets/Darklight/Resources/Darklight";
            const string folder=parent+"/Atmosphere";
            const string path=folder+"/GothicEclipseRuaDasCinzas.asset";
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(parent,"Atmosphere");
            var preset=AssetDatabase.LoadAssetAtPath<DarklightAtmospherePreset>(path);
            if(preset==null)
            {
                // Preserve a conflicting asset instead of overwriting it.
                if(AssetDatabase.LoadMainAssetAtPath(path)!=null)
                {
                    Debug.LogError("An unrelated asset already occupies "+path);
                    return;
                }
                preset=ScriptableObject.CreateInstance<DarklightAtmospherePreset>();
                AssetDatabase.CreateAsset(preset,path);
                AssetDatabase.SaveAssets();
            }
            Selection.activeObject=preset;
            EditorGUIUtility.PingObject(preset);
        }
    }
}
'''
c=sources[controller].decode('utf-8-sig').replace('\r\n','\n')
def replace(old,new):
    global c
    assert c.count(old)==1, (old,c.count(old))
    c=c.replace(old,new)
replace('public string ActivePresetName => Presets[PresetIndex].Name;', 'public string ActivePresetName => UsesGothicEclipse ? GothicPreset.DisplayName : Presets[PresetIndex].Name;\n        public bool GothicEclipseEnabled => gothicEclipseEnabled;\n        public bool UsesGothicEclipse => gothicEclipseEnabled && PresetIndex==0;\n        [SerializeField] bool gothicEclipseEnabled=true;\n        [SerializeField] DarklightAtmospherePreset gothicEclipsePreset;\n        DarklightAtmospherePreset runtimePreset;\n        bool ownsRuntimePreset;\n        DarklightAtmospherePreset GothicPreset\n        {\n            get\n            {\n                if(gothicEclipsePreset!=null) return gothicEclipsePreset;\n                if(runtimePreset==null)\n                {\n                    runtimePreset=Resources.Load<DarklightAtmospherePreset>(DarklightAtmospherePreset.RuaResourcePath);\n                    if(runtimePreset==null)\n                    {\n                        runtimePreset=ScriptableObject.CreateInstance<DarklightAtmospherePreset>();\n                        runtimePreset.hideFlags=HideFlags.HideAndDontSave;\n                        ownsRuntimePreset=true;\n                    }\n                }\n                return runtimePreset;\n            }\n        }')
replace('public void SetAllRegionsEnabled(bool enabled) { AllRegionsEnabled=enabled; Refresh(); }','public void SetAllRegionsEnabled(bool enabled) { AllRegionsEnabled=enabled; Refresh(); }\n        // False restores the existing map atmosphere; the settings switch disables all overlays.\n        public void SetGothicEclipseEnabled(bool enabled) { gothicEclipseEnabled=enabled; Refresh(); }\n        public void SetGothicEclipsePreset(DarklightAtmospherePreset preset) { gothicEclipsePreset=preset; Refresh(); }\n        bool Enclosed => PresetIndex==2 || PresetIndex==3 || PresetIndex==6 || PresetIndex==7 || PresetIndex==9;')
replace('var preset=Presets[PresetIndex];\n            foreach(var item in originals)', 'var preset=Presets[PresetIndex];\n            var gothic=UsesGothicEclipse ? GothicPreset : null;\n            foreach(var item in originals)')
replace('filterMaterial.SetColor("_FilterColor",preset.Ground);\n                filterMaterial.SetFloat("_Strength",.28f*intensity);','// Reuse the single map-only filter; Remaster remains the sole light/Volume owner.\n                filterMaterial.SetColor("_FilterColor",gothic!=null ? gothic.GroundPalette : preset.Ground);\n                filterMaterial.SetFloat("_Strength",(gothic!=null ? gothic.GroundStrength : .28f)*intensity);\n                filterMaterial.SetFloat("_MapExposure",gothic!=null ? gothic.MapExposureStops : 0);')
replace('int rain=quality==0 ? preset.Rain/4 : quality==1 ? preset.Rain/2 : preset.Rain;', '// Retain Remaster quality budgets and the existing pool. Interiors never receive rain.\n            int requestedRain=Enclosed ? 0 : gothic!=null ? gothic.RainDrops : preset.Rain;\n            int rain=quality==0 ? requestedRain/4 : quality==1 ? requestedRain/2 : requestedRain;')
replace('fog[i].enabled=visible && i==PresetIndex%fog.Length;', 'fog[i].enabled=visible && i==(gothic!=null ? gothic.FogVariant : PresetIndex%fog.Length);')
replace('var color=fogArt[i]!=null ? new Color(.66f,.79f,.81f) : preset.Fog;\n                color.a=.045f*intensity; fog[i].color=color;', 'var color=gothic!=null ? gothic.FogColor : fogArt[i]!=null ? new Color(.66f,.79f,.81f) : preset.Fog;\n                color.a=(gothic!=null ? gothic.FogOpacity : .045f)*intensity; fog[i].color=color;')
replace('moon[i].enabled=visible && quality>0 && i==(PresetIndex+1)%moon.Length;\n                bool enclosed=PresetIndex==2 || PresetIndex==3 || PresetIndex==6 || PresetIndex==7 || PresetIndex==9;\n                moon[i].color=new Color(.67f,.83f,.98f,(enclosed?.015f:.04f)*intensity);','moon[i].enabled=visible && quality>0 && i==(gothic!=null ? gothic.MoonVariant : (PresetIndex+1)%moon.Length);\n                var color=gothic!=null ? gothic.MoonColor : new Color(.67f,.83f,.98f);\n                color.a=(gothic!=null ? gothic.MoonOpacity : Enclosed?.015f:.04f)*intensity;\n                moon[i].color=color;')
replace('var preset=Presets[PresetIndex];\n            for(int i=0;i<fog.Length;i++)', 'var preset=Presets[PresetIndex];\n            var gothic=UsesGothicEclipse ? GothicPreset : null;\n            for(int i=0;i<fog.Length;i++)')
replace('Mathf.Sin(clock*.045f+i*2)*.15f;', 'Mathf.Sin(clock*(gothic!=null ? gothic.FogDriftSpeed : .045f)+i*2)*.15f;')
replace('Fit(fog[i],width*1.45f,height*.42f);', 'Fit(fog[i],width*1.45f,height*(gothic!=null ? gothic.FogHeight : .42f));')
replace('p.Position.x+=dt*preset.Wind*p.Speed*(p.Rain?.20f:.045f);', 'p.Position.x+=dt*(gothic!=null ? gothic.Wind : preset.Wind)*p.Speed*(p.Rain?.20f:.045f);')
replace('if(p.Position.x > 1.1f) p.Position.x-=2.2f;', 'if(p.Position.x > 1.1f) p.Position.x-=2.2f;\n                if(p.Position.x < -1.1f) p.Position.x+=2.2f;')
replace('if(filterMaterial!=null) Destroy(filterMaterial);', 'if(filterMaterial!=null) Destroy(filterMaterial);\n            if(ownsRuntimePreset && runtimePreset!=null) Destroy(runtimePreset);')
out[controller]=c
s=sources[shader].decode('utf-8-sig').replace('\r\n','\n')
s=s.replace('_Strength ("Filter opacity", Range(0,1)) = .6', '_Strength ("Filter opacity", Range(0,1)) = .6\n        _MapExposure ("Scenery exposure stops", Range(-.5,.5)) = 0')
s=s.replace('float _GroundFilter,_Strength;', 'float _GroundFilter,_Strength,_MapExposure;')
s=s.replace('c.rgb=saturate((cold-.012)*1.04);', '// Exposure applies only inside the existing ground-filter branch.\n                cold*=exp2(_MapExposure);\n                c.rgb=saturate((cold-.012)*1.04);')
out[shader]=s
patch=[]
manifest=[]
for path,content in out.items():
    target=stage/'files'/path
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_text(content,encoding='utf-8',newline='\n')
    before=sources.get(path,b'').decode('utf-8-sig').replace('\r\n','\n')
    patch.extend(difflib.unified_diff(before.splitlines(True),content.splitlines(True),fromfile='a/'+path if path in sources else '/dev/null',tofile='b/'+path))
    manifest.append({'path':path,'source_sha256':hashlib.sha256(sources[path]).hexdigest() if path in sources else None,'staged_sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'action':'modify' if path in sources else 'add'})
(stage/'gothic-eclipse-first-map.patch').write_text(''.join(patch),encoding='utf-8',newline='\n')
(stage/'manifest.json').write_text(json.dumps({'source_root':str(root),'files':manifest},indent=2)+'\n',encoding='utf-8')
print(json.dumps({'staged_files':len(out),'source_sha256':{k:hashlib.sha256(v).hexdigest() for k,v in sources.items()}},indent=2))
