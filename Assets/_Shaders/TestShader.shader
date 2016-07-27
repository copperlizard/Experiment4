Shader "Custom/TestShader" 
{	
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_NoiseTex("Noise Texture", 2D) = "white" {}
	}	

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard alpha vertex:vert addshadow

		//Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0	

		#include "UnityCG.cginc"

		#pragma glsl

		sampler2D _MainTex;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _NoiseTex;

		//Water parameters
		float4 _WaterCenterPos;
		float4 _WaterBounds;

		float _WaterScale;
		float _WaterSpeed;
		float _WaterDistance;
		float _WaterTime;
		float _WaterNoiseStrength;
		float _WaterNoiseWalk;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;			
		};

		//The wave function
		float3 getWavePos(float3 pos, float2 uv)
		{
			pos.y = 0.0;

			float waveType = uv.y * _WaterBounds.z;
			//float waveType = pos.z;
			//float waveType = uv.y * _WaterBounds.z - _WaterCenterPos.z; //pos.z;

			pos.y += sin((_WaterTime * _WaterSpeed + waveType) / _WaterDistance) * _WaterScale;

			//Add noise
			//pos.y += tex2Dlod(_NoiseTex, float4(pos.x, pos.z + sin(_WaterTime * 0.1), 0.0, 0.0) * _WaterNoiseWalk).a * _WaterNoiseStrength;			

			//pos.y += cnoise(uv * 30.0 + float2(_WaterTime * 0.01, _WaterTime * 0.01 * cos(_WaterTime  * 0.05)) * _WaterNoiseWalk) * _WaterNoiseStrength;

			return pos;
		}

		void vert(inout appdata_full VertIN)
		{
			//VertIN.vertex = mul(UNITY_MATRIX_MVP, VertIN.vertex);

			//Get the global position of the vertice
			float4 worldPos = mul(_Object2World, VertIN.vertex);
				
			//Convert the position back to local
			//float4 localPos = mul(_World2Object, worldPos);

			float3 withWave = getWavePos(worldPos.xyz, VertIN.texcoord.xy);
			//float3 withWave = getWavePos(worldPos.xyz, worldPos.xz);

			//Convert the position back to local
			float4 localPos = mul(_World2Object, float4(withWave, worldPos.w));

			//Assign the modified vertice
			VertIN.vertex = localPos;
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = _Color;
			
			float xdist = IN.worldPos.x - floor(IN.worldPos.x);
			float zdist = IN.worldPos.z - floor(IN.worldPos.z);
			if (xdist < 0.0)
			{
				xdist = -xdist;
			}

			if (zdist < 0.0)
			{
				zdist = -zdist;
			}

			if (xdist < 0.1)
			{
				c *= 0.0;
			}

			if (zdist < 0.1)
			{
				c *= 0.0;
			}

			o.Albedo = c.rgb;
			//Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
