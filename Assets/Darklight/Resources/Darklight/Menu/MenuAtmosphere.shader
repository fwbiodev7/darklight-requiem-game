Shader "Darklight/MenuAtmosphere"
{
    Properties { [PerRendererData] _MainTex("Texture",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _ReducedFlashes("Reduced flashes",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex;float4 _MainTex_TexelSize;fixed4 _Color;float _ReducedFlashes;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float2 uv=i.uv;fixed4 c=tex2D(_MainTex,uv);
                float t=_Time.y;
                float eclipse=length((uv-float2(.72,.885))*float2(1.777,1));
                float sky=smoothstep(.68,.85,uv.y)*(1-smoothstep(.75,.84,uv.x))*smoothstep(.13,.21,eclipse);
                float shift=floor(sin(t*.024)*9)*_MainTex_TexelSize.x;
                fixed4 moving=tex2D(_MainTex,saturate(uv+float2(shift,0)));
                c=lerp(c,moving,sky*.22);
                float ring=exp(-pow((eclipse-.071)*28,2));
                c.rgb+=float3(.02,.001,.004)*ring*(.5+.5*sin(t*.7))*(1-_ReducedFlashes);
                float lights=step(.16,c.r)*step(c.g*2.3,c.r)*step(uv.y,.61)*step(.4,uv.x);
                float phase=floor(uv.x*83)+floor(uv.y*79)*3;
                c.rgb*=1-lights*.075*(.5+.5*sin(t*.83+phase))*(1-_ReducedFlashes);
                return c*i.color;
            }
            ENDCG
        }
    }
}
