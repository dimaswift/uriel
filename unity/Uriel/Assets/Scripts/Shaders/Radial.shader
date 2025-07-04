Shader "Uriel/Radial"  
{  
    Properties  
    {  
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 10000.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _Frequency("Frequency", Float) = 5.0
        _Amplitude("Amplitude", Range(-0.1, 0.1)) = 0.1
        _Density("Density", Float) = 0.5
        _Depth("Depth", Float) = 1.0
        _Steps("Steps", Int) = 1
        _Saturation("Saturation", Float) = 0
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
          
            #include "AutoLight.cginc"  
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
            float _Frequency;
            float _Amplitude;
            float _Density;
            float _Depth;
            float _Saturation;
            int _Steps;

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
                float value = 0;
                float a = PI * 1.0 / max(1, _Steps);
                float d = sqrt(a);
                float num_phi = round(PI / d);
                float d_phi = PI / num_phi;
                float d_theta = a / d_phi;
                for (int m = 0; m < num_phi; ++m)
                {
                    float phi = PI * (m + 0.5) / num_phi;
                    float num_theta = round(2 * PI * sin(phi) / d_theta);
                    for (int n = 0; n < num_theta; ++n)
                    {
                        float theta = 2 * PI * n / num_theta;
                        float x = sin(phi) * cos(theta);
                        float y = sin(phi) * sin(theta);
                        float z = cos(phi);
                        const float3 offset2 = float3(x, y, z) * _Depth;
                        float dist = distance(id.world_pos, offset2 ) *  _Density;
                        for (int s = 0; s < min(1,ceil(_Saturation)); ++s)
                        {
                            dist = saturate(dist) * _Saturation;
                        }
                        value += sin(dist * _Frequency) * 0.000001 * _Amplitude; 
                    }
                }
                const float3 diffuse_color = hsv2rgb(value * (_Threshold * 1000), 1, 1) * _Multiplier;
                return float4(diffuse_color.x,diffuse_color.x,diffuse_color.x, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  