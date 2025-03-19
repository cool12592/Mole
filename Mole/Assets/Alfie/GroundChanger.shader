Shader "Custom/GroundChanger"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "" {}
        _MaskTex("Mask Texture", 2D) = "" {}        
        _RoadTex("Road Texture", 2D) = "" {}
        _UnderGroundTex("Under Ground Texture", 2D) = "" {}

        _FinishRoadTex("Finish Road Texture", 2D) = "" {}
        _PaintColor("Paint Color", Color) = (1,1,1,1)  // 기본값 설정 (흰색)
        _RoadColor("Road Color", Color) = (1,1,1,1)  // 기본값 설정 (흰색)
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

            sampler2D _FinishRoadTex;
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
                fixed4 finishRoadColor = tex2D(_FinishRoadTex, i.uv);
                fixed4 underGroundColor = tex2D(_UnderGroundTex, i.uv);

                //fixed4 finalColor = groundColor * _PaintColor;

                // 🚀 2. roadColor 칠해진 부분만 경계 판별 실행 (불필요한 연산 최소화)
                if (roadColor.a > 0.1)
                {
                    // 텍스처 크기 기반으로 texel 크기 계산 (512x512 텍스처 기준)
                    float2 texelSize = float2(1.0 / 512.0, 1.0 / 512.0);

                    // 주변 픽셀 샘플링 (경계를 찾기 위해)
                    float left   = tex2D(_RoadTex, i.uv + float2(-texelSize.x, 0)).r;
                    float right  = tex2D(_RoadTex, i.uv + float2(texelSize.x, 0)).r;
                    float up     = tex2D(_RoadTex, i.uv + float2(0, texelSize.y)).r;
                    float down   = tex2D(_RoadTex, i.uv + float2(0, -texelSize.y)).r;

                    // 경계 판별 (자신은 roadColor지만 주변이 roadColor가 없는 경우)
                    bool isEdge = (left < 0.1 || right < 0.1 || up < 0.1 || down < 0.1);

                    // 가장자리에서 점점 어두워지는 효과 적용
                    float brightness = isEdge ? 0.5 : 1.0;
                    
                    fixed4 finalColor = underGroundColor * roadColor;
                    finalColor.rgb *= brightness;
                    finalColor.a = 0.5; // 투명도 유지

                    return finalColor;
                }

                // 🚀 1. 먼저 빠르게 리턴할 수 있는 경우 처리 (불필요한 연산 방지)
                if (maskColor.a > 0.1)
                {
                    fixed4 finalColor = underGroundColor * maskColor * 0.5;
                    finalColor.a = 0.8;
                    return finalColor; // 바로 반환 (이후 검사 안 함)
                }
    
                // if (finishRoadColor.a > 0.1)
                // {
                //     return underGroundColor * finishRoadColor * 0.4; // 밝기 조절 후 반환 (이후 검사 안 함)
                // }

                

                // 🚀 3. 그 외의 경우 기본 색상 반환
                clip(-1); // 해당 픽셀을 완전히 삭제 (투명)
                return baseColor;
            }
            ENDCG
        }
    }
}
