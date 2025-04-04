Shader "Custom/MyAdditiveShader"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BackgroundTex; // 기존 B 텍스처 유지

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 srcColor = tex2D(_MainTex, i.uv);
                fixed4 bgColor = tex2D(_BackgroundTex, i.uv);

                
               if (srcColor.a > 0)
                    return srcColor;

                return bgColor;
            }
            ENDCG
        }
    }
}
