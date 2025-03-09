Shader "Custom/GroundChanger"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "" {}
        _MaskTex("Mask Texture", 2D) = "" {}
        _NewColor("New Color", Color) = (1,1,1,1)  // 기본값 설정 (흰색)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            uniform fixed4 _NewColor;  // `Color` 타입을 `fixed4`로 선언


            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                if ( maskColor.a > 0)
                {
                    return _NewColor;  
                }
                return baseColor;
            }
            ENDCG
        }
    }
}
