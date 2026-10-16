Shader "Darklight/DefinitiveAnimationPreview"
{
    Properties { [PerRendererData] _MainTex("Sprite",2D)="white" {} _Mask("Selectors",2D)="black" {} _Form("Form",Float)=1 _CutHead("Age",Float)=0 _WeaponOnly("Weapon",Float)=0 _Raw("Unchanged source",Float)=0 _AlphaClip("Alpha",Range(0,1))=.65 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "DisableBatching"="True" }
        Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex,_Mask;float _Form,_CutHead,_WeaponOnly,_Raw,_AlphaClip;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-_AlphaClip);fixed4 m=tex2D(_Mask,i.uv);
                if(_WeaponOnly>.5&&m.b<.5)discard;
                if(_WeaponOnly<.5&&_CutHead>.5&&m.a>.5&&m.b<.5)discard;
                if(_Raw<.5&&_Form>1.5)
                {
                    if(m.r>.5)c.rgb=(_Form>2.5?fixed3(.94,.96,1):fixed3(.64,.10,.98))*max(c.r,max(c.g,c.b));
                    if(_Form>2.5&&m.g>.5)c.rgb=lerp(fixed3(.32,.32,.37),fixed3(.95,.95,1),saturate(c.r*1.6));
                }
                return c*i.color;
            }
            ENDCG
        }
    }
}
