Shader "Darklight/Prototype/YoungSpriteLit"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite",2D)="white"{}
        _MaskTex("Lighting mask",2D)="white"{}
        _NormalMap("Normal",2D)="bump"{}
        _VampireMask("Original palette selectors",2D)="black"{}
        _Color("Tint",Color)=(1,1,1,1)
        _VampireForm("Form",Float)=0
        _WholeSprite("Whole character",Float)=0
        _AlphaClip("Pixel alpha",Range(0,1))=.65
        _WholeFrameId("Complete figure identity",Float)=0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            Tags {"LightMode"="Universal2D"}
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex LitVertex
            #pragma fragment LitFragment
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            struct Attributes {COMMON_2D_INPUTS half4 color:COLOR; UNITY_SKINNED_VERTEX_INPUTS};
            struct Varyings {COMMON_2D_LIT_OUTPUTS half4 color:COLOR;};
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"
            TEXTURE2D(_VampireMask);SAMPLER(sampler_VampireMask);
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;float _VampireForm,_WholeSprite,_AlphaClip,_WholeFrameId;
            CBUFFER_END
            Varyings LitVertex(Attributes input)
            {
                SetUpSpriteInstanceProperties();input.positionOS=UnityFlipSprite(input.positionOS,unity_SpriteProps.xy);
                Varyings o=CommonLitVertex(input);o.color=input.color*_Color*unity_SpriteColor;return o;
            }
            half4 LitFragment(Varyings input):SV_Target
            {
                half4 main=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv);clip(main.a-_AlphaClip);
                half4 selectors=SAMPLE_TEXTURE2D(_VampireMask,sampler_VampireMask,input.uv);
                if(_WholeFrameId>.5)clip(.49-abs(round(selectors.a*255)-_WholeFrameId));
                if(_VampireForm>.5)
                {
                    half3 palette=_VampireForm>2.5?half3(.95,.96,1):_VampireForm>1.5?half3(.62,.16,.96):half3(.86,.035,.08);
                    if(_WholeSprite>.5&&_VampireForm>2.5&&selectors.b>.1)
                        main.rgb=lerp(half3(.12,.14,.18),half3(.97,.98,1),saturate((main.r-.08)*1.7));
                    if(selectors.r>.5)main.rgb=palette;
                    else if(selectors.g>.1)main.rgb=lerp(main.rgb,palette*max(.3,max(main.r,max(main.g,main.b))*1.3),selectors.g*.85);
                }
                main*=input.color;SurfaceData2D surfaceData;InputData2D inputData;
                InitializeSurfaceData(main.rgb,main.a,half4(1,1,1,1),half3(0,0,1),surfaceData);
                InitializeInputData(input.uv,input.lightingUV,inputData);
                return CombinedShapeLightShared(surfaceData,inputData);
            }
            ENDHLSL
        }
    }
}
