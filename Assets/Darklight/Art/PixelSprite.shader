Shader "Darklight/PixelSprite"
{
    Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) _AlphaClip ("Edge threshold", Range(0,1)) = 0.65 _VampireMask ("Original selectors", 2D) = "black" {} _VampireForm ("Vampire palette", Float) = 0 _WholeSprite ("Complete supplied character", Float) = 0 _EraseHead ("Age attachment", Float) = 0 _DarkForm ("Form", Float) = 0 _HeadThreshold ("Hair height", Float) = 1.5 _WingOnly ("Spectral overlay", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" "DisableBatching"="True" }
        Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; float height:TEXCOORD1; float side:TEXCOORD2; float2 sourceUV:TEXCOORD3; };
            sampler2D _VampireMask; float _VampireForm,_EraseHead,_WholeSprite; sampler2D _MainTex; fixed4 _Color; float _AlphaClip,_DarkForm,_HeadThreshold,_WingOnly,_ResonanceTint;
            sampler2D _OcclusionTex;float _LayerOcclusion,_OcclusionChin,_Physique;float4 _OcclusionScale,_OcclusionOffset,_OcclusionRect,_OcclusionStep;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; o.height=v.vertex.y;o.side=v.vertex.x;o.sourceUV=v.vertex.xy*_OcclusionScale.xy+_OcclusionOffset.xy; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv); clip(c.a-_AlphaClip);
                if(_LayerOcclusion>.5)
                {
                    bool inside=i.sourceUV.x>=_OcclusionRect.x&&i.sourceUV.y>=_OcclusionRect.y&&i.sourceUV.x<=_OcclusionRect.z&&i.sourceUV.y<=_OcclusionRect.w;
                    fixed4 base=inside?tex2D(_OcclusionTex,i.sourceUV):fixed4(0,0,0,0);
                    if(_LayerOcclusion>1.5)
                    {
                        float nearby=0;
                        for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)nearby=max(nearby,tex2D(_OcclusionTex,i.sourceUV+float2(x,y)*_OcclusionStep.xy).a);
                        if(nearby<.65||base.a>=.65)discard;
                    }
                    else if(base.a>=.65)
                    {
                        bool steel=base.r>.4&&abs(base.r-base.g)<.07&&abs(base.r-base.b)<.12;
                        bool skin=base.r>.6&&base.g>.25&&base.g<.78&&base.b<.65&&base.r>base.g;
                        bool blade=base.r>.5&&base.g<.18&&base.b<.25;
                        if(blade||skin||i.sourceUV.y<_OcclusionChin&&steel)discard;
                    }
                }
                if(_WingOnly>.5) { if(!(c.b>c.r*1.15 && c.r>c.g*1.2))discard; }
                if(_DarkForm>2.5 && i.height>_HeadThreshold && c.r>.1 && c.r<.58 && c.r>c.g*1.16 && c.g>c.b*1.08)
                    c.rgb=lerp(float3(.35,.37,.42),float3(.92,.94,1),saturate(c.r*1.6));
                if(_DarkForm>1.5 && c.r>.5 && c.g<.3 && c.b<.3)
                    c.rgb=_DarkForm>2.5?float3(.9,.92,1):float3(.65,.15,1);
                // Retain the Rock skin's bare arms during every animated action.
                if(_DarkForm>.5 && i.height>.45 && i.height<_HeadThreshold-.2 && abs(i.side)>.16 && c.r>.65 && c.g>.25 && c.g<.72 && c.b<.6)
                    c.rgb=lerp(c.rgb,_DarkForm>2.5?float3(.95,.96,1):_DarkForm>1.5?float3(.65,.2,1):float3(1,.12,.18),.24);
                if(_ResonanceTint>.5)c.rgb=max(c.r,max(c.g,c.b));
                if(_Physique>.5&&i.height>.6&&i.height<_HeadThreshold-.2&&abs(i.side)>.16&&c.r>.65&&c.g>.25&&c.g<.72&&c.b<.6)
                    c.rgb*=1-(_Physique>1.5?.07:.035);
                fixed4 selectors=tex2D(_VampireMask,i.uv);
                if(_VampireForm>.5)
                {
                    fixed3 palette=_VampireForm>2.5?fixed3(.95,.96,1):_VampireForm>1.5?fixed3(.62,.16,.96):fixed3(.86,.035,.08);
                    if(_WholeSprite>.5&&_VampireForm>2.5&&selectors.b>.1)
                        c.rgb=lerp(float3(.12,.14,.18),float3(.97,.98,1),saturate((c.r-.08)*1.7));
                    if(selectors.r>.5)c.rgb=palette;
                    else if(selectors.g>.1)c.rgb=lerp(c.rgb,palette*max(.3,max(c.r,max(c.g,c.b))*1.3),selectors.g*.85);
                }
                return c*i.color;
            }
            ENDCG
        }
    }
}
