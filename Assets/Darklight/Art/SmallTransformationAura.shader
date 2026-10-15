Shader "Darklight/SmallTransformationAura"
{
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        CGINCLUDE
        #include "UnityCG.cginc"
        struct A {float4 vertex:POSITION;fixed4 color:COLOR;};struct V{float4 vertex:SV_POSITION;fixed4 color:COLOR;};
        V vert(A a){V v;v.vertex=UnityObjectToClipPos(a.vertex);v.color=a.color;return v;}
        fixed4 frag(V v):SV_Target{return v.color;}
        ENDCG
        Pass { Tags {"LightMode"="Universal2D"} CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        ENDCG }
        Pass { Tags {"LightMode"="Always"} CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        ENDCG }
    }
}
