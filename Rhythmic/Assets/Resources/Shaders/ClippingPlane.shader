Shader "ClippingPlane"
{
	//show values to edit in inspector
	Properties{
		_Color("Color", Color) = (0, 0, 0, 1)
		_Enabled("Enabled", Int) = 1
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff("AlphaCutoff", Range(0,1)) = 0.5
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_BumpStrength("BumpStrength", Range(0, 1)) = 1
		_Smoothness("Smoothness", Range(0, 1)) = 0
		_Metallic("Metalness", Range(0, 1)) = 0

		// _Plane("Plane", Float) = (0, 0, 0, 0)
		_InversePlane("InversePlane", Float) = (0, 0, 0, 0)
		_PlaneEnabled("PlaneEnabled", Int) = 0
		_InversePlaneEnabled("InversePlaneEnabled", Int) = 0

		_EmissionEnabled("EmissionEnabled", Int) = 0
		[HDR]_Emission("Emission", color) = (0,0,0)
		_EmissionMap("EmissionMap", 2D) = "white" {}
		_CutoffColor("Cutoff Color", Color) = (1,0,0,0)
	}

		SubShader
		{
			//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
			Tags{ "Queue" = "Transparent"  "RenderType" = "Transparent" "ForceNoShadowCasting" = "True"}

			// render faces regardless if they point towards the camera or away from it
			//Lighting Off
			ZTest LEqual
			ZWrite Off
			//BlendOp Add
			//Blend One Zero
			//Cull Off

			Pass {
				ZWrite On
				ColorMask 0
			}

			//UsePass "Transparent/Diffuse/FORWARD"


			CGPROGRAM
			//the shader is a surface shader, meaning that it will be extended by unity in the background 
			//to have fancy lighting and other features
			//our surface shader function is called surf and we use our custom lighting model
			//fullforwardshadows makes sure unity adds the shadow passes the shader might need
			//vertex:vert makes the shader use vert as a vertex shader function

			#pragma target 3.0
			#pragma surface surf Standard alpha

			int _Enabled;
			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _BumpMap;
			float _Cutoff;

			float _BumpStrength;
			half _Smoothness;
			half _Metallic;

			int _EmissionEnabled;
			half3 _Emission;
			sampler2D _EmissionMap;

			float3 _HorizonPoint;

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
			// step(x,y): returns whether x is bigger than y
			void surf(Input i, inout SurfaceOutputStandard o)
			{
				clip(-1 + _Enabled);
				//if (_Enabled == 0) return; // ???

				// Clipping:
				{
					float distance = dot(i.worldPos, _Plane);
					distance = distance + _Plane.w;
					clip(_PlaneEnabled * -distance);

					float inverse_distance = dot(i.worldPos, _InversePlane.xyz);
					inverse_distance = inverse_distance + _InversePlane.w;
					clip(_InversePlaneEnabled * inverse_distance); 
				}

				float facing = i.facing * 0.5 + 0.5;
				fixed4 col = tex2D(_MainTex, i.uv_MainTex) * _Color;
				o.Albedo = col.rgb * facing;

				// Emission
				//fixed4 emission = tex2D(_EmissionMap, i.uv_MainTex) * _Emission;
				//o.Emission = lerp(_CutoffColor, emission, facing);
				o.Emission = _EmissionEnabled * _Emission * tex2D(_EmissionMap, i.uv_MainTex).a;

				fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));
				o.Normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv_BumpMap), _BumpStrength);
				//o.Normal = lerp(float3(0.5, 0.5, 1), normal, _BumpStrength); // REMOVEME: This used to offset the normal map texture.

				// Clip transparency
				clip(col.a - _Cutoff);

				o.Alpha = col.a;
				o.Metallic = _Metallic * facing;
				o.Metallic *= col.a;
				o.Smoothness = _Smoothness * facing;
			}
			ENDCG
		}
		FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}