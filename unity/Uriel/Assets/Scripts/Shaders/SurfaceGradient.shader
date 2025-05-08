Shader "Uriel/SurfaceGradient"  
{  
    Properties  
    {  
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 2.0)) = 0.5
        _Multiplier ("Multiplier", Range(-3.14, 3.14)) = 1
        _Depth("Depth", Range(-3.14, 3.14)) = 1.0
        _Steps("Steps", Range(1.0, 100.0)) = 1.0
        _Min("Min", Range(-3.0, 3.0)) = 1.0
        _Max("Max", Range(-3.0, 3.0)) = 1.0
        _Frequency("Frequency", Range(0, 0.5)) = 0.5
        _Amplitude("Amplitude", Range(0, 0.5)) = 0.5
        
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
                float3 world_normal : TEXCOORD0;
                float3 world_pos : TEXCOORD1;
            };
            
            sampler2D _Gradient;  
            float _Multiplier;
            float _Threshold;
            float _Depth;
            float _Min;
            float _Max;
            int _Steps;
            float _Frequency;
            float _Amplitude;
            
            uint _PhotonCount;
            uint _Mode;
            StructuredBuffer<Photon> _PhotonBuffer;
            
            v2f vert(const appdata_t input)  
            {  
                v2f o;
                o.world_normal = UnityObjectToWorldNormal(input.normal);
                o.world_pos = mul(unity_ObjectToWorld, input.vertex);
                o.vertex = UnityObjectToClipPos(input.vertex); 
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                const float3 origin =  id.world_pos;
                const float ray_length = sqrt(length(origin));
                const float value = rayMarchField(origin, normalize(origin), ray_length, _Steps, _Min, _Max, _Depth,
                    _Frequency, _Amplitude, _PhotonCount, _PhotonBuffer);
                const float3 diffuse_color = tex2D(_Gradient, float2(value * _Threshold, 0)) * _Multiplier;
                return float4(diffuse_color, 1);  
            }  
            
            ENDCG   
        }  
    }  
}  