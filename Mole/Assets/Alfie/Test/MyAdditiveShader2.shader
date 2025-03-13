Shader "Custom/MyAdditiveShader2"
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
            sampler2D _MaskTex1; 
            sampler2D _MaskTex2; 
            sampler2D _MaskTex3; 

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
                fixed4 maskColor1 = tex2D(_MaskTex1, i.uv);
                fixed4 maskColor2 = tex2D(_MaskTex2, i.uv);
                fixed4 maskColor3 = tex2D(_MaskTex3, i.uv);

                
               if (maskColor1.r > 0.1)
               {
                  return fixed4(0,0,0,0);
               }
               else if (maskColor2.r > 0.1)
               {
                  return fixed4(0,0,0,0);
               }
               else if (maskColor3.r > 0.1)
               {
                  return fixed4(0,0,0,0);
               }
               
               return srcColor;
            }
            ENDCG
        }
    }
}
