Shader "Uriel/UnlitSurface"  
{  
    Properties  
    {  
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 10.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
    }  
    SubShader  
    {  
        Tags { "RenderType" = "Opaque" }  
        Cull Off 
        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Scripts/Lib/Uriel.cginc"
       
            struct appdata_t  
            {  
                float4 vertex : POSITION;
                float3 normal : NORMAL;  
                float4 color : COLOR;
            };  

            struct v2f  
            {
                float4 vertex : SV_POSITION;  
                float3 world_pos : TEXCOORD0;
            };  
            
            sampler2D _Gradient;  
            float _Multiplier;
            float _PowerThreshold;
            uint _WaveCount;
            StructuredBuffer<Wave> _WaveBuffer; 
            
            v2f vert(const appdata_t input)  
            {  
                v2f o;
                o.vertex = UnityObjectToClipPos(input.vertex);
                o.world_pos = mul(unity_ObjectToWorld, input.vertex);
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                const float value = sampleField(id.world_pos, _WaveCount, _WaveBuffer);
                const float3 finalColor  = tex2D(_Gradient, float2(value * _PowerThreshold, 0)) * _Multiplier;
                return float4(finalColor, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  