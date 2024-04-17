Shader "Custom/Sprites/Outline"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Outline("Outline", Float) = 0
		_OutlineColor ("OutlineColor", Color) = (1,1,1,1)
		_AddColor ("AddColor", Color) = (0,0,0,0)
		_OutlineThickness ("OutlineSize", Range(0,100)) = 3
		[Toggle(OUTLINE_FLIP)]_OutlineFlip("Interior Outline", Float) = 0.0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

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

			float _OutlineThickness;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}


			fixed4 HandleOutline(fixed4 col, float2 uv, v2f i)
			{
				fixed4 outline = fixed4(_OutlineColor.rgb, 0);
				
				outline.a = col.a;

				#if OUTLINE_FLIP
				float nearbyAlpha = 0.0f;
				#endif
					for (float d = 0; d <= 2; d += 0.1)
					{
						fixed2 arc = fixed2(_OutlineThickness * _MainTex_TexelSize.x * sin(d * UNITY_PI),
						_OutlineThickness * _MainTex_TexelSize.y * cos(d * UNITY_PI));

						fixed4 pixelArc = tex2D(_MainTex, i.uv + arc);

						#if OUTLINE_FLIP
							nearbyAlpha += pixelArc.a;
						#else
							outline.a += pixelArc.a;
						#endif
					}

				#if OUTLINE_FLIP
				float alphaDifference = saturate(abs(nearbyAlpha / 12.0f - outline.a) * _Outline);

				outline.a *= alphaDifference;

				return lerp(col * i.color, outline, (outline.a));
				#else
				outline.a *= saturate(_Outline) * i.color.a;

				return lerp(outline , col * i.color, round(col.a+.1));
				#endif
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col = HandleOutline(col, i.uv, i);

				return fixed4(col.rgb + _AddColor.rgb * .3, col.a);
			}
		ENDCG
		}
	}
}
