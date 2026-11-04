using UnityEngine;

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
