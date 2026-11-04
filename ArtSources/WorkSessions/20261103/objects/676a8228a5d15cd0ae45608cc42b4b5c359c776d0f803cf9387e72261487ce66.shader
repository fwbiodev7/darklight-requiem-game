Shader "Darklight/AtmosphereOverlay"
{
    Properties {
        [PerRendererData] _MainTex ("Soft sprite", 2D) = "white" {}
        _GroundFilter ("Ground filter only", Float) = 0
        _FilterColor ("Cold mineral palette", Color) = (.76,.85,.91,1)
        _Strength ("Filter opacity", Range(0,1)) = .6
        _MapExposure ("Scenery exposure stops", Range(-.5,.5)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        CGINCLUDE
        #include "UnityCG.cginc"
        struct A { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
        struct V { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
        sampler2D _MainTex;
        float _GroundFilter,_Strength,_MapExposure; fixed4 _FilterColor;
        V vert(A a) { V v; v.vertex=UnityObjectToClipPos(a.vertex); v.uv=a.uv; v.color=a.color; return v; }
        fixed4 frag(V v):SV_Target {
            fixed4 c=tex2D(_MainTex,v.uv)*v.color;
            if(_GroundFilter>.5) {
                float luma=dot(c.rgb,float3(.2126,.7152,.0722));
                float3 desaturated=lerp(luma.xxx,c.rgb,.80);
                // Selective map-only contrast, green-gray shadows and pale cold highlights.
                float3 cold=desaturated*_FilterColor.rgb;
                cold=lerp(cold,cold*float3(.82,.94,1.02),saturate(1-luma)*.25);
                // Exposure applies only inside the existing ground-filter branch.
                cold*=exp2(_MapExposure);
                c.rgb=saturate((cold-.012)*1.04);
                c.a*=_Strength;
            }
            return c;
        }
        ENDCG
        Pass { Tags { "LightMode"="Universal2D" } CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        ENDCG }
        Pass { Tags { "LightMode"="SRPDefaultUnlit" } CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        ENDCG }
    }
}
