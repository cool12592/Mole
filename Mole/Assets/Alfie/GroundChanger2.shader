Shader "Custom/GroundChanger2"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "" {}
        _MaskTex("Mask Texture", 2D) = "" {}
        _MaskTex2("Mask Texture2", 2D) = "" {}
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
            sampler2D _MaskTex2;
            uniform fixed4 _BaseColor;  // `Color` 타입을 `fixed4`로 선언

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                fixed4 maskColor = tex2D(_MaskTex, i.uv);
                fixed4 maskColor2 = tex2D(_MaskTex2, i.uv);

                if ( maskColor.r >= 0.5)
                {
                    return maskColor2;  
                }
               
                clip(-1); // 해당 픽셀을 완전히 삭제 (투명)
                return mainColor;
            }
            ENDCG
        }
    }
}
