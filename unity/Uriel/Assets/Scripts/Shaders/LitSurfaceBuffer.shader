Shader "Uriel/LitSurfaceBuffer"  
{  
    Properties  
    {  
        _LightSource ("LightSource", Vector) = (0,1,1)
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 2.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 5.0)) = 1
        _Light ("Light", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0.0, 5.0)) = 0.25
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 5.0)) = 0.5
        _Shininess("Shininess", Range(0.0, 500.0)) = 1.0
        _Depth("Depth", Range(0.0, 1.0)) = 1.0
        _Steps("Steps", Range(1.0, 50.0)) = 1.0
        _Min("Min", Range(-3.0, 3.0)) = 1.0
        _Max("Max", Range(-3.0, 3.0)) = 1.0
        _Frequency("Frequency", Range(0, 0.5)) = 0.5
        _Mode("Mode", Range(0, 2)) = 0
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
            float _Depth;
            float _Min;
            float _Max;
            int _Steps;
            float _Frequency;
            
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
                const float3 dir = id.world_pos;
                const float rayLength = length(dir);
                const float stepSize = rayLength / _Steps;
                const float3 rayDir = normalize(dir);
                float value = 0.0;
                for (int i = 0; i < _Steps; ++i)
                {
                    const float t = i * stepSize;  
                    const float3 target = id.world_pos + (rayDir * t) * _Depth / rayLength;
                    const float density = sampleField(target, rayDir, _PhotonCount, _PhotonBuffer);
                        switch (_Mode)
                        {
                            case 0:
                                value += sin(smoothstep(_Min, _Max, density * _Frequency) * _Multiplier);
                                break;
                            case 1:
                                value += sin(density * _Frequency * 0.1) * _Multiplier;
                                break;
                            default:
                                value += step(pow(1.0 + _Multiplier * 0.01, density), _Max) * _Frequency * 2;
                                break;
                        }
                    
                }
                const float3 diffuse_color = tex2D(_Gradient, float2(value * (_Threshold * 0.01), 0));
                //const float3 diffuse_color = hsv2rgb(sin(value * _Threshold * 0.01) + _SpecularThreshold, _Shininess, _SpecularMultiplier);
                return float4(diffuse_color, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  