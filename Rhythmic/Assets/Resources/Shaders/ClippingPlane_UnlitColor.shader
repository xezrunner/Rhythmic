Shader "ClippingPlane_UnlitColor" {
Properties {
	_Color ("Color", Color) = (0, 0, 0, 1)
    _Plane ("Plane", Float) = (0, 0, 0, 0)
	_InversePlane ("InversePlane", Float) = (0, 0, 0, 0)
	_PlaneEnabled ("PlaneEnabled", Int) = 0
	_InversePlaneEnabled ("InversePlaneEnabled", Int) = 0
    
	[HDR]_Emission ("Emission", color) = (0,0,0)
    [HDR]_CutoffColor("Cutoff Color", Color) = (1,0,0,0)
}

SubShader {
    Tags { "RenderType"="Opaque" }
	Lighting Off
	AlphaTest Greater 0.5
    LOD 200

	CGPROGRAM
	#pragma target 3.0
	#pragma surface surf Unlit

	half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
		return fixed4(0,0,0,0);//half4(s.Albedo, s.Alpha);
         }

	fixed4 _Color;

	float4 _Plane;
	float4 _InversePlane;
	int _PlaneEnabled;
	int _InversePlaneEnabled;

	half3 _Emission;
	float4 _CutoffColor;

    //input struct which is automatically filled by unity
		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float facing : VFACE;
		};

		//the surface shader function which sets parameters the lighting function then uses
		void surf (Input i, inout SurfaceOutput o) 
		{
			o.Albedo = _Color.rgb * _Color.a;
			o.Alpha = _Color.a;

			if (_PlaneEnabled == 1 | _InversePlaneEnabled == 1)
			{
				//calculate signed distance to plane
				float distance = dot(i.worldPos, _Plane.xyz);
				distance = distance + _Plane.w;

				float inverse_distance = dot(i.worldPos, _InversePlane.xyz);
				inverse_distance = inverse_distance + _InversePlane.w;

				//discard surface above plane
				if (_PlaneEnabled == 1)
					clip(-distance);
				if (_InversePlaneEnabled == 1)
					clip(inverse_distance);
			}

			float facing = i.facing * 0.5 + 0.5;
			o.Emission = lerp(_CutoffColor, _Emission, facing);
		}

        ENDCG
	}
}