Shader "Darklight/AtmosphereOverlay"
{
    Properties {
        [PerRendererData] _MainTex ("Soft sprite", 2D) = "white" {}
        _GroundFilter ("Ground filter only", Float) = 0
        _FilterColor ("Cold mineral palette", Color) = (.76,.85,.91,1)
        _Strength ("Filter opacity", Range(0,1)) = .6
        _Wetness ("Wet stone", Range(0,1)) = 0
        _LampReflection ("Amber reflection", Float) = 0
        _SurfaceClock ("Campaign time", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" "DisableBatching"="True" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        CGINCLUDE
        #include "UnityCG.cginc"
        struct A { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
        struct V { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float2 world:TEXCOORD1; fixed4 color:COLOR; };
        sampler2D _MainTex;
        float _GroundFilter,_Strength,_Wetness,_LampReflection,_SurfaceClock; fixed4 _FilterColor;
        V vert(A a) { V v; v.vertex=UnityObjectToClipPos(a.vertex); v.world=mul(unity_ObjectToWorld,a.vertex).xy; v.uv=a.uv; v.color=a.color; return v; }
        fixed4 frag(V v):SV_Target {
            fixed4 c=tex2D(_MainTex,v.uv)*v.color;
            if(_GroundFilter>.5) {
                float luma=dot(c.rgb,float3(.2126,.7152,.0722));
                float3 desaturated=lerp(luma.xxx,c.rgb,.80);
                // Selective map-only contrast, green-gray shadows and pale cold highlights.
                float3 cold=desaturated*_FilterColor.rgb;
                cold=lerp(cold,cold*float3(.82,.94,1.02),saturate(1-luma)*.25);
                float warm=saturate((c.r-max(c.g,c.b)) * 4);
                c.rgb=lerp(saturate((cold-.012)*1.04),c.rgb,warm);
                if (_Wetness>0) {
                    // Elongated glints in the stone, stable while the camera pans.
                    float2 pixel=floor(v.world*48)/48;
                    float basin=smoothstep(.60,.93,sin(pixel.x*1.8+pixel.y*.8)*cos(pixel.y*2.2));
                    float streak=smoothstep(.78,.98,sin(pixel.y*47+sin(pixel.x*3.5+_SurfaceClock*.45)));
                    c.rgb+=float3(.025,.04,.055)*basin*streak*_Wetness*(1-warm);
                }
                c.a*=_Strength;
            } else if (_LampReflection>.5) {
                float2 pixel=floor(v.world*48)/48;
                float stripe=sin(pixel.y*43+1.9*sin(pixel.x*4+_SurfaceClock*.6));
                c.a*=lerp(.09,.95,smoothstep(.38,.98,stripe));
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
