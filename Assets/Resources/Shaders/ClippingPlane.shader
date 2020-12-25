Shader "ClippingPlane"{
	//show values to edit in inspector
	Properties{
		_Color ("Tint", Color) = (0, 0, 0, 1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff("AlphaCutoff", Range(0,1)) = 0.5
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_BumpStrength("BumpStrength", Range(0, 1)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Metallic ("Metalness", Range(0, 1)) = 0

		_Plane ("Plane", Float) = (0, 0, 0, 0)
		_InversePlane ("InversePlane", Float) = (0, 0, 0, 0)
		_PlaneEnabled ("PlaneEnabled", Int) = 0
		_InversePlaneEnabled ("InversePlaneEnabled", Int) = 0

		[HDR]_Emission ("Emission", color) = (0,0,0)
		[HDR]_CutoffColor("Cutoff Color", Color) = (1,0,0,0)
	}

	SubShader{
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType"="Opaque"}

		// render faces regardless if they point towards the camera or away from it
		BlendOp Add
        Blend One Zero
        ZWrite On
        Cull Off

		CGPROGRAM
		//the shader is a surface shader, meaning that it will be extended by unity in the background 
		//to have fancy lighting and other features
		//our surface shader function is called surf and we use our custom lighting model
		//fullforwardshadows makes sure unity adds the shadow passes the shader might need
		//vertex:vert makes the shader use vert as a vertex shader function

		#pragma target 3.0
		#pragma surface surf Standard

		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;
		float _Cutoff;

		float _BumpStrength;
		half _Smoothness;
		half _Metallic;
		half3 _Emission;

		float4 _Plane;
		float4 _InversePlane;
		int _PlaneEnabled;
		int _InversePlaneEnabled;

		float4 _CutoffColor;

		//input struct which is automatically filled by unity
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
			float facing : VFACE;
		};

		//the surface shader function which sets parameters the lighting function then uses
		void surf (Input i, inout SurfaceOutputStandard o) 
		{
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
			
			//normal color stuff
			fixed4 col = tex2D(_MainTex, i.uv_MainTex);
			col *= _Color;
			o.Albedo = col.rgb * facing;

			// Clip transparency
			clip(col.a - _Cutoff);

			fixed3 normal = UnpackNormal (tex2D (_BumpMap, i.uv_BumpMap));
			o.Normal = lerp(float3(0.5, 0.5, 1), normal, _BumpStrength);
			//o.Normal = UnpackNormal (tex2D (_BumpMap, i.uv_BumpMap));

			o.Metallic = _Metallic * facing;
			o.Smoothness = _Smoothness * facing;
			o.Emission = lerp(_CutoffColor, _Emission, facing);
		}
		ENDCG
	}
	FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}