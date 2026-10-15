Shader "Darklight/Prototype/WholeSilhouetteShadow"
{
    Properties { [PerRendererData] _MainTex("Whole sprite",2D)="white"{} _VampireMask("Owner selectors",2D)="black"{} _WholeFrameId("Whole identity",Float)=0 _ShadowColor("Shadow",Color)=(.02,.03,.04,.4) }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags {"LightMode"="Universal2D"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A{float3 positionOS:POSITION;float2 uv:TEXCOORD0;};struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);TEXTURE2D(_VampireMask);SAMPLER(sampler_VampireMask);
            CBUFFER_START(UnityPerMaterial)
            float4 _ShadowColor;float _WholeFrameId;
            CBUFFER_END
            V vert(A a){V v;v.positionCS=TransformObjectToHClip(a.positionOS);v.uv=a.uv;return v;}
            half4 frag(V v):SV_Target
            {
                float alpha=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,v.uv).a;clip(alpha-.65);
                float owner=round(SAMPLE_TEXTURE2D(_VampireMask,sampler_VampireMask,v.uv).a*255);if(_WholeFrameId>.5)clip(.49-abs(owner-_WholeFrameId));
                return half4(_ShadowColor.rgb,alpha*_ShadowColor.a);
            }
            ENDHLSL
        }
    }
}
