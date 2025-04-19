Shader "Uriel/Sky"  
{  
    SubShader  
    {  
        Tags  
        {  
            "RenderType" = "Opaque"  
            "Queue" = "Transparent" 
        }  

        // Enable alpha blending  
     //   Blend SrcAlpha OneMinusSrcAlpha  
        //ZWrite Off  
 

        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  

            #include "UnityCG.cginc"  

            struct appdata_t  
            {  
                float4 vertex : POSITION;  
                float4 color : COLOR;  
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;  
                fixed4 color : COLOR;  
            };  
            float3 hsv2rgb(float h, float s, float v) {
                h = frac(h);
                float i = floor(h * 6.0);
                float f = h * 6.0 - i;
                float p = v * (1.0 - s);
                float q = v * (1.0 - f * s);
                float t = v * (1.0 - (1.0 - f) * s);
                if(i == 0.0) return float3(v, t, p);
                if(i == 1.0) return float3(q, v, p);
                if(i == 2.0) return float3(p, v, t);
                if(i == 3.0) return float3(p, q, v);
                if(i == 4.0) return float3(t, p, v);
                return float3(v, p, q);
            }

            StructuredBuffer<float4x4> Particles;  

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)  
            {  
                v2f o;
                const float4x4 p = Particles[instanceID];
                float size = p[0][0];
                if(p[3][1] < 0.2)
                {
                    size = 0;
                }
                const float4x4 m = float4x4(
                    size,0,0,p[0][3],
                    0,size,0,p[1][3],
                    0,0,size,p[2][3],
                    1,1,1,1);
                const float4 pos = mul(m, i.vertex);  
                o.vertex = UnityObjectToClipPos(pos);
                const float3 col = hsv2rgb(p[3][2], p[3][0], p[3][1]);
                o.color = float4(col, 0.1); // Alpha of 0.5 for 50% transparency  
                return o;  
            }  

            fixed4 frag(v2f i) : SV_Target  
            {  
                return i.color; // This now includes the alpha value  
            }  
            ENDCG  
        }  
    }  
}  