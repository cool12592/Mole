Shader "Custom/GroundChanger"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "" {}
        _MaskTex("Mask Texture", 2D) = "" {}        
        _RoadTex("Road Texture", 2D) = "" {}
        _UnderGroundTex("Under Ground Texture", 2D) = "" {}
        _RoadOutLineTex("Road Outline Texture", 2D) = "" {}

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
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
            sampler2D _UnderGroundTex;
            sampler2D _RoadOutLineTex;


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
                fixed4 underGroundColor = tex2D(_UnderGroundTex, i.uv);
                fixed4 roadOutLineColor = tex2D(_RoadOutLineTex, i.uv);

                if (roadColor.a > 0.1)
                {
                    fixed4 finalColor = underGroundColor * roadColor;
                    //finalColor.rgb *= brightness;
                    finalColor.a = 0.2; // 투명도 유지

                    return finalColor;
                }

                if (maskColor.a > 0.1)
                {
                    fixed4 finalColor = underGroundColor * maskColor;// * 0.5;
                    finalColor.a = 1;
                    return finalColor; // 바로 반환 (이후 검사 안 함)
                }
                if (roadOutLineColor.a > 0.1)
                {
                    fixed4 finalColor = roadOutLineColor;
                    //finalColor.rgb *= brightness;
                    finalColor.a = 1; // 투명도 유지

                    return finalColor;
                }
    
                clip(-1); // 해당 픽셀을 완전히 삭제 (투명)
                return baseColor;
            }
            ENDCG
        }
    }
}
