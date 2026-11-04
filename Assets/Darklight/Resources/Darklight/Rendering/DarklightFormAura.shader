Shader "Darklight/FormAura"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _FormColor ("Transformation Aura Tint", Color) = (1,1,1,1)
        [PerRendererData] _FormIntensity ("Aura Intensity", Range(0,1)) = 0
        _RimColor ("Rim Light Color", Color) = (0.8, 0.88, 1.0, 0.6)
        _RimPower ("Rim Light Power", Range(0, 1)) = 0.35
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _FormColor;
            float _FormIntensity;
            fixed4 _RimColor;
            float _RimPower;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Form resonance pulse
                if (_FormIntensity > 0.01)
                {
                    fixed3 auraGlow = lerp(c.rgb, _FormColor.rgb * (c.r * 0.4 + c.g * 0.4 + c.b * 0.2), _FormIntensity);
                    c.rgb = lerp(c.rgb, auraGlow, _FormIntensity);
                }

                // AAA Gothic Eclipse Rim Light (silhouette pop against dark rain)
                if (_RimPower > 0.01 && c.a > 0.1)
                {
                    // Sample alpha neighbors
                    float aUp    = tex2D(_MainTex, IN.texcoord + float2(0, _MainTex_TexelSize.y)).a;
                    float aDown  = tex2D(_MainTex, IN.texcoord - float2(0, _MainTex_TexelSize.y)).a;
                    float aRight = tex2D(_MainTex, IN.texcoord + float2(_MainTex_TexelSize.x, 0)).a;
                    float aLeft  = tex2D(_MainTex, IN.texcoord - float2(_MainTex_TexelSize.x, 0)).a;

                    float edge = (1.0 - (aUp * aDown * aRight * aLeft)) * _RimPower;
                    c.rgb += _RimColor.rgb * edge * c.a;
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
