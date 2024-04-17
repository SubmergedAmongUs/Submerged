Shader "Custom/ElevatorShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_DepartUpwards ("Depart Upwards", Range(0, 1)) = 0
		_DepartDownwards ("Depart Downwards", Range(0, 1)) = 0
		_MaskLayer ("MaskLayer", Int) = 1
	}
	SubShader
	{
		
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			//Stencil 
			//{
			//	Ref [_MaskLayer]
			//	Comp Always
			//	Pass Replace
   //         }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color    : COLOR;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			float _DepartUpwards;
			float _DepartDownwards;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;

				//Move Elevator along uv.y
				o.uv.y -= o.uv.y / 10 * _DepartUpwards;
				o.uv.y += o.uv.y / 10 * _DepartDownwards;

				return o;
			}

			float _SpriteUVOffsetYMin;
			float _SpriteUVOffsetYMax;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);



				col.a *= 1 - smoothstep(0.25, 1, _DepartUpwards);
				col.a *= 1 - smoothstep(0.25, 1, _DepartDownwards);


				
				//Adjusted Clip threshold to allow elevator to fade out further
				//clip(col.a - .05);
				return col * _Color * i.color;
			}
			ENDCG
		}
	}
}
