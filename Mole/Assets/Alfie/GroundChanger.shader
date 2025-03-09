Shader "Custom/GroundChanger"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "" {}
        _MaskTex("Mask Texture", 2D) = "" {}        
        _RoadTex("Road Texture", 2D) = "" {}
        _PaintColor("Paint Color", Color) = (1,1,1,1)  // 기본값 설정 (흰색)
        _RoadColor("Road Color", Color) = (1,1,1,1)  // 기본값 설정 (흰색)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            sampler2D _RoadTex;
            uniform fixed4 _PaintColor;  // `Color` 타입을 `fixed4`로 선언
            uniform fixed4 _RoadColor;  // `Color` 타입을 `fixed4`로 선언


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
                fixed4 roadColor = tex2D(_RoadTex, i.uv);


                if ( maskColor.a > 0.1)
                {
                    return _PaintColor;  
                }
                else if ( roadColor.r > 0.1)
                {
                    return _RoadColor;  
                }

                clip(-1); // 해당 픽셀을 완전히 삭제 (투명)
                return baseColor;
            }
            ENDCG
        }
    }
}
