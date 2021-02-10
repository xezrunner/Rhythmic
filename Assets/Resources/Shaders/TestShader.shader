Shader "InfiniteSky"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // make this render super early inside transparencies
        Tags { "RenderType"="Transparent-400" }

        Pass
        {
            // don't need to write into depth buffer		
            ZWrite Off
            // invert culling; we want to render our sphere inside out
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // This is the actual shader change:
                // we'll make the depth be "on the far plane"
                #if defined(UNITY_REVERSED_Z)
                // when using reversed-Z, make the Z be just a tiny
                // bit above 0.0
                o.vertex.z = 1.0e-9f;
                #else
                // when not using reversed-Z, make Z/W be just a tiny
                // bit below 1.0
                o.vertex.z = o.vertex.w - 1.0e-6f;
                #endif
                // end of shader change
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target { return tex2D(_MainTex, i.uv); }
            ENDCG
        }
    }
}
