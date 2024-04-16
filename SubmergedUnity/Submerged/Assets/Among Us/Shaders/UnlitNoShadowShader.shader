Shader "Unlit/NoShadowShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Mask ("Mask", Float) = 8
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Cull Off
		Lighting Off

		Pass
		{
			Stencil 
			{
				Ref 2
				Comp [_Mask]
				Pass Replace
				Fail Keep
				ZFail Keep
            }

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
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}


			float clipCheck;
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 ocol = tex2D(_MainTex, i.uv);
				clipCheck = ocol.a - 0.30f;
				clip(clipCheck);
				return ocol;
			}
			ENDCG
		}
	}

	Fallback Off
}
