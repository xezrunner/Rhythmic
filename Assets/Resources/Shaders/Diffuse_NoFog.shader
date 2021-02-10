// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Diffuse NoFog" {
Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        LOD 100

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
            #pragma surface surf PPL alpha noshadow novertexlights nolightmap vertex:vert nofog

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            //#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            //#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Input
            {
                float2 uv_MainTex;
                float2 uv2_DetailTex;
                fixed4 color : COLOR;
                float4 worldPosition;
            };

            fixed4 _Color;

            void vert (inout appdata_t v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.worldPosition = v.vertex;

                v.color = v.color * _Color;
            }

            void surf (Input IN, inout SurfaceOutput o)
            {
                o.Albedo = _Color;
                o.Alpha = _Color.a;
            }

            half4 LightingPPL (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
            {
            #ifndef USING_DIRECTIONAL_LIGHT
                lightDir = normalize(lightDir);
            #endif

                half diffuseFactor = max(0.0, dot(s.Normal, lightDir));

                half4 c;
                c.rgb = (s.Albedo * diffuseFactor) * _LightColor0.rgb;
                c.rgb *= atten;
                c.a = s.Alpha;
                return c;
            }
        ENDCG
    }
    Fallback "UI/Lit/Transparent"
}