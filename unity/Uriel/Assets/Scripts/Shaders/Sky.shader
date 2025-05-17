Shader "Uriel/Sky"  
{  
    Properties  
    {  
        _Size ("Size", Range(0.0, 1.0)) = 1
        _Threshold ("Threshold", Range(0.0, 5.0)) = 1
    }  
    SubShader  
    {  

        Tags  
        {  
            "RenderType" = "Opaque"  
            "Queue" = "Transparent" 
        }  
        Pass  
        {  
            CGPROGRAM  

            #pragma vertex vert   
            #pragma fragment frag
            #include "Assets/Scripts/Lib/Uriel.cginc"
            #include "Assets/Scripts/Lib/Sky.cginc"
            #include "UnityCG.cginc"
            
            struct appdata_t  
            {  
                float4 vertex : POSITION;  
                float3 normal : NORMAL;
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;  
                float3 world_pos : TEXCOORD0;
                float3 color : TEXCOORD1;
            };
            
            StructuredBuffer<Star> _Stars;
            float _Size;
            float _Threshold;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)  
            {  
                v2f o;
                const Star star = _Stars[instanceID];
                float size = _Size;
                const float4x4 m = float4x4(
                    size,0,0,star.location.x,
                    0,size,0,star.location.y,
                    0,0,size,star.location.z,
                    1,1,1,1);
                const float4 pos = mul(m, i.vertex);  
                o.vertex = UnityObjectToClipPos(pos);
                
                const float3 finalColor = hsv2rgb(star.location.y * _Threshold, 1, 1);
                o.world_pos = pos;
                o.color = finalColor;
                return o;  
            }

            fixed4 frag(v2f i) : SV_Target  
            {
                return fixed4(i.color, 1.0);
            }   
            ENDCG  
        }  
    }  
}  