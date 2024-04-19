Shader "Unlit/CooldownShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Cooldown Color", Vector) = (0.5275862,0.5275862,0.5275862,1)
		_Percent ("PercentCool", Float) = 1
		_Desat ("Desaturation", Float) = 0
		_NormalizedUvs ("NormUvs", Vector) = (0,1,0,0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature OUTLINE_FLIP

			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			float _Outline;
			fixed4 _OutlineColor;
			fixed4 _AddColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed _Percent;
			fixed _Desat;

			fixed4 _NormalizedUvs;
			fixed4 _Color;


			fixed4 frag (v2f i) : SV_Target
			{
				fixed normalisedCooldownPercent = i.uv.y - _NormalizedUvs.z;
				normalisedCooldownPercent /= _NormalizedUvs.w;
				normalisedCooldownPercent -= _Percent;
				normalisedCooldownPercent = ceil(normalisedCooldownPercent);


				fixed4 col = tex2D(_MainTex, i.uv);
				fixed d = col.x + col.y + col.z;
				fixed4 desat = fixed4(d / 3, d / 3, d / 3, col.a);
				fixed4 desaturatedCol = -desat * _Color + col;
				col = desat * _Color;

				fixed4 output = desaturatedCol * normalisedCooldownPercent + col;
				return output * i.color;
			}

			ENDCG
			}
		}
	}