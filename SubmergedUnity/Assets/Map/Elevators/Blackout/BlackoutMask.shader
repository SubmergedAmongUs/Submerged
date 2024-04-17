Shader "Elevator/BlackoutMask"
{ // TODO Masking shader stuff
    Properties
    {

        _MainTex ("BlackTexture", 2D) = "white" {}
        _Alpha ("Alpha", Range(0, 1)) = 1
        _XRange ("XRange", Range(0, 1)) = 1
        _YRange ("YRange", Range(0, 1)) = 1
        
        _MaskLayer ("MaskLayer", Int) = 1
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

            // Custom
            float _Alpha;
            float _XRange;
            float _YRange;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 ocol = float4(0, 0, 0, 1);
                ocol.a *= abs(0.5 - i.uv.x) >= _XRange || abs(0.5 - i.uv.y) >= _YRange ? _Alpha : 0;
                return ocol;
            }
            ENDCG
        }
    }

    Fallback Off
}