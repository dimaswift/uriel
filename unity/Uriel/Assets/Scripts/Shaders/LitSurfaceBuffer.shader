Shader "Uriel/LitSurfaceBuffer"  
{  
    Properties  
    {  
        _LightSource ("LightSource", Vector) = (0,1,1)
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 10.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _Light ("Light", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0.0, 5.0)) = 0.25
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 5.0)) = 0.5
        _Shininess("Shininess", Range(0.0, 500.0)) = 1.0
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
            
            float3 _LightSource;
            sampler2D _Gradient;  
            float _Multiplier;
            float _Threshold;
            float4 _Light;
            float _SpecularThreshold;
            float _SpecularMultiplier;
            float _Shininess;

            uint _WaveCount;
            StructuredBuffer<Wave> _WaveBuffer;
            
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
                float value = 0.0;
                for (int i = 0; i < _WaveCount; ++i)
                {
                    value += sampleShape(id.world_pos, id.world_normal, _WaveBuffer[i]);
                }
                const float3 diffuse_color = tex2D(_Gradient, float2(value * (_Threshold), 0)) * _Multiplier;
                const float3 normal_dir = normalize(id.world_normal);
                const float3 ambient = ShadeSH9(float4(normal_dir, 1));  
                const float3 light_dir = normalize(_LightSource);
                const float3 view_dir = normalize(UnityWorldSpaceViewDir(id.world_pos));
                const float3 halfway_dir = normalize(light_dir + view_dir);  
                const float l = saturate(dot(normal_dir, light_dir));
                const float3 diffuse_lighting = l  * _Light.rgb;
                const float h = saturate(dot(normal_dir, halfway_dir));  
                const float specular_value = _SpecularThreshold * _SpecularMultiplier;  
                const float3 specular_lighting = pow(h, _Shininess) * specular_value * _Light.rgb;  
                const float3 final_color = (diffuse_lighting + ambient) * diffuse_color + specular_lighting;  
                return float4(final_color, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  