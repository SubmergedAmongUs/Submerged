Shader "Unlit/MapShader" {
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
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


			fixed4 frag(v2f i) : SV_Target 
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed c = (-col.z) + col.x;
				fixed x = 0.100000001f;
				fixed3 c2 = i.color.xyz * fixed3(c, c, c) + col.zzz;
				fixed4 alt = fixed4(c2.x, c2.y, c2.z, col.w);
				fixed4 output = step(x,c) * alt + step(c,x) * col;

				output.x = clamp(output.x, 0.0, 1.0);
				output.y = clamp(output.y, 0.0, 1.0);
				output.z = clamp(output.z, 0.0, 1.0);
				output.w = col.w * i.color.w;
				output.w = clamp(output.w, 0.0, 1.0);

				return output;
			}
		ENDCG
		}
	}
}