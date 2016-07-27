Shader "Custom/WaterSurfaceShader"
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
		//Physically based Standard lighting model, and enable shadows on all light types
		//- Standard means standard lightning
		//- vertex:vert to be able to modify the vertices
		//- addshadow to make the shadows look correct after modifying the vertices
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

		float rand(float2 c) 
		{
			return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
		}	

		float4 mod289(float4 x)
		{
			return x - floor(x * (1.0 / 289.0)) * 289.0;
		}

		float4 permute(float4 x)
		{
			return mod289(((x*34.0) + 1.0)*x);
		}

		float4 taylorInvSqrt(float4 r)
		{
			return 1.79284291400159 - 0.85373472095314 * r;
		}

		float2 fade(float2 t) {
			return t*t*t*(t*(t*6.0 - 15.0) + 10.0);
		}

		// Classic Perlin noise
		float cnoise(float2 P)
		{
			float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
			float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
			Pi = mod289(Pi); // To avoid truncation effects in permutation
			float4 ix = Pi.xzxz;
			float4 iy = Pi.yyww;
			float4 fx = Pf.xzxz;
			float4 fy = Pf.yyww;

			float4 i = permute(permute(ix) + iy);

			float4 gx = frac(i * (1.0 / 41.0)) * 2.0 - 1.0;
			float4 gy = abs(gx) - 0.5;
			float4 tx = floor(gx + 0.5);
			gx = gx - tx;

			float2 g00 = float2(gx.x, gy.x);
			float2 g10 = float2(gx.y, gy.y);
			float2 g01 = float2(gx.z, gy.z);
			float2 g11 = float2(gx.w, gy.w);

			float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
			g00 *= norm.x;
			g01 *= norm.y;
			g10 *= norm.z;
			g11 *= norm.w;

			float n00 = dot(g00, float2(fx.x, fy.x));
			float n10 = dot(g10, float2(fx.y, fy.y));
			float n01 = dot(g01, float2(fx.z, fy.z));
			float n11 = dot(g11, float2(fx.w, fy.w));

			float2 fade_xy = fade(Pf.xy);
			float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
			float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
			return 2.3 * n_xy;
		}
		
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

		void vert(inout appdata_full IN)
		{
			//Get the global position of the vertice
			float4 worldPos = mul(_Object2World, IN.vertex);

			//Manipulate the position
			float3 withWave = getWavePos(worldPos, IN.texcoord.xy); //_WaterCenterPos

			//float3 withWave = getWavePos(worldPos.xyz, IN.vertex / _WaterBounds);
			
			//Convert the position back to local
			float4 localPos = mul(_World2Object, float4(withWave, worldPos.w));

			//Assign the modified vertice
			IN.vertex = localPos;
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			//Albedo comes from a texture tinted by color
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex + float2(_WaterTime * _WaterSpeed * 0.001f, _WaterTime * _WaterSpeed * 0.001f * cos(_WaterTime * _WaterSpeed * 0.01f))) * _Color;
			fixed4 c = tex2D(_MainTex, (IN.worldPos.xz * 0.001f) + float2(_WaterTime * _WaterSpeed * 0.001f, _WaterTime * _WaterSpeed * 0.001f * cos(_WaterTime * _WaterSpeed * 0.01f))) * _Color;
			//fixed4 c = _Color;

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