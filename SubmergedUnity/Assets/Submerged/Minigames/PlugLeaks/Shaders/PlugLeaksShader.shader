Shader "Task/PlugLeaks"
{
    Properties
    {
    	// Default
	    [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    	
    	// Settings
    	[Toggle(Enabled)] _Enabled ("Enabled", Float) = 1
        _Alpha("Alpha", Range(0, 1)) = 0
    	_RedColor ("Red Color", Color) = (1, 0, 0, 1)
    	[HideInInspector] _GreenColor ("Green Color", Color) = (0, 1, 0, 1) // UNUSED
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
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
				float3 worldPos : TEXCOORD1;

			};

            sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
            
            float _Enabled;
            float _Alpha;

            fixed4 _RedColor;
            fixed4 _GreenColor;

            v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
            	o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 ocol = tex2D(_MainTex, i.uv);
			    fixed4 col = fixed4( _RedColor.rgb * ocol.r + _GreenColor.rgb * ocol.g, ocol.a * _Alpha);
                return col * _Enabled;
			}
			ENDCG
        }
    }
    Fallback Off
}