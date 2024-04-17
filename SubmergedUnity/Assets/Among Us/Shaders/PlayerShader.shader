Shader "Unlit/PlayerShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BackColor ("Shadow Color", Color) = (1,0,0,1)
		_BodyColor ("Body Color", Color) = (1,1,0,1)
		_VisorColor ("VisorColor", Color) = (0,1,1,1)
		_Outline("Outline", Range(0, 1)) = 0
		_OutlineColor ("OutlineColor", Color) = (1,1,1,1)
		_Desaturate ("Desaturate", Range(0, 1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers xbox360 ps3
			
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

			fixed4 _BodyColor;
			fixed4 _VisorColor;
			fixed4 _BackColor;
			
			float _Outline;
			fixed4 _OutlineColor;

			float _Desaturate;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
						
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 ocol = tex2D(_MainTex, i.uv);
				fixed mx = max(ocol.r, max(ocol.g, ocol.b));
				fixed mn = min(ocol.r, min(ocol.g, ocol.b));

				fixed4 col = fixed4( saturate(_BodyColor.rgb * ocol.r + _VisorColor.rgb * ocol.g + _BackColor.rgb * ocol.b), ocol.a);
				if (mx < 0.001 ||
					abs(1 - mn / mx) < .45)
				{
					col.rgb = ocol.rgb;
				}

				float2 uv = i.uv;
				fixed4 outline = _OutlineColor;
				 
				outline.a = col.a;

				fixed4 pixelUp = tex2D(_MainTex, uv + fixed2(0, _MainTex_TexelSize.y));
				fixed4 pixelDown = tex2D(_MainTex, uv - fixed2(0, _MainTex_TexelSize.y));
				fixed4 pixelRight = tex2D(_MainTex, uv + fixed2(_MainTex_TexelSize.x, 0));
				fixed4 pixelLeft = tex2D(_MainTex, uv - fixed2(_MainTex_TexelSize.x, 0));
				outline.a += pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;

				pixelUp = tex2D(_MainTex, uv + fixed2(0, 2 * _MainTex_TexelSize.y));
				pixelDown = tex2D(_MainTex, uv - fixed2(0, 2 * _MainTex_TexelSize.y));
				pixelRight = tex2D(_MainTex, uv + fixed2(2 * _MainTex_TexelSize.x, 0));
				pixelLeft = tex2D(_MainTex, uv - fixed2(2 * _MainTex_TexelSize.x, 0));
				outline.a += pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;

				pixelUp = tex2D(_MainTex, uv + fixed2(0, 3 * _MainTex_TexelSize.y));
				pixelDown = tex2D(_MainTex, uv - fixed2(0, 3 * _MainTex_TexelSize.y));
				pixelRight = tex2D(_MainTex, uv + fixed2(3 * _MainTex_TexelSize.x, 0));
				pixelLeft = tex2D(_MainTex, uv - fixed2(3 * _MainTex_TexelSize.x, 0));
				outline.a += pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;

				outline.a *= saturate(_Outline);

				// float d = (col.r + col.g + col.g) / 3;
				// col = lerp(col, fixed4(d, d, d, col.a), _Desaturate);

				return lerp(outline, col, round(col.a + .05f)) * i.color;
			}
			ENDCG
		}
	}

	Fallback Off
}
