Shader "Uriel/LitSurfaceGradient"  
{  
    Properties  
    {  
        _LightSource ("LightSource", Vector) = (0,1,1)
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0.0, 5.0)) = 0.25
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 5.0)) = 0.5
        _Shininess("Shininess", Range(0.0, 500.0)) = 1.0
        
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
            #include "Assets/Scripts/Lib/CustomLight.cginc"
            #include "Assets/Scripts/Lib/Gradient.cginc"
            
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
            
            float _Depth;
            float _Min;
            float _Max;
            int _Steps;
            float _Frequency;
            float _Amplitude;
            
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
                    _Frequency, _Amplitude);
                const float3 diffuse_color = applyCustomLighting(sampleGradient(value), id.world_normal, id.world_normal);
                return float4(diffuse_color, 1);  
            }  
            
            ENDCG   
        }  
    }  
}  