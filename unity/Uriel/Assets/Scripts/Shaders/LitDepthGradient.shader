Shader "Uriel/LitDepthGradient"  
{  
    Properties  
    {  
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 10.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0.0, 10.0)) = 1.0
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 10.0)) = 1.0
        _Shininess("Shininess", Range(0.0, 360.0)) = 1.0
        _Depth ("Depth", Range(0, 15)) = 0
        _Min ("Min", Range(-3, 3)) = 0
        _Max ("Max", Range(-3, 3)) = 0
        _Displacement ("Displacement", Range(0, 1)) = 0
        _MinDisplacement ("Min Displacement", Range(-3, 3)) = 0
        _MaxDisplacement ("Max Displacement", Range(-3, 3)) = 0
        _Smoothness ("Smoothness", Range(-3, 3)) = 0
        _Steps ("Steps", Range(1, 50)) = 1
        _Frequency ("Frequency", Range(-3, 3)) = 0
        _Amplitude ("Amplitude", Range(-3, 3)) = 0
        _Scale ("Scale", Float) = 1
        _Speed ("Speed", Float) = 1
        _Phase ("Phase", Float) = 1
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

            #include "Assets/Scripts/Lib/CustomLight.cginc"
            #include "AutoLight.cginc"  
            #include "UnityCG.cginc"
            #include "Assets/Scripts/Lib/Gradient.cginc"
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
                float3 normal : TEXCOORD1;
            };
            
            float _Depth;
            float _Min;
            float _Max;
            float _Smoothness;
            
            int _Steps;
            float _Frequency;
            float _Amplitude;
            float _Displacement;
            float _MinDisplacement;
            float _MaxDisplacement;
            float _Scale;
            float _Speed;
            float _Phase;
            

            float sample(float3 pos, float3 normal)
            {
                const float step_size = length(pos) / _Steps;
                float total_density = 0.0;
                for (uint i = 0; i < _Steps; ++i)
                {
                    const float3 target = pos + normal * i * step_size  * _Displacement;
                    const float density = sampleField(target);
                    total_density += smoothstep(_Min, _Max, density * _Frequency)  * _Amplitude;
                }
                return total_density;
            }
            
            float3 sampleDisplacedField(float3 pos, float3 normal, float epsilon)
            {
                
                const float density = sampleField(pos);
                return pos + normal * density * epsilon;
            }
                        
            float3 constructFieldTriangleNormal(float3 center, float3 normal, float displacement)
            {
                const float3 tangent = normalize(cross(normal, float3(0, 1, 0)));
                const float3 bitangent = cross(normal, tangent);
                const float eps = 0.0001;
                const float3 v0_offset = tangent + bitangent;
                const float3 v0 = center + v0_offset * eps;
                const float twoPiOver3 = 2.09439510239;
                const float3 v1_offset = tangent * cos(twoPiOver3) + bitangent * sin(twoPiOver3);
                const float3 v2_offset = tangent * cos(twoPiOver3 * twoPiOver3) + bitangent * sin(2.0 * twoPiOver3);
                const float3 v1 = center + v1_offset * eps;
                const float3 v2 = center + v2_offset * eps;
                const float3 p0 = sampleDisplacedField(v0, normal, _Depth * displacement);
                const float3 p1 = sampleDisplacedField(v1, normal, _Depth * displacement);
                const float3 p2 = sampleDisplacedField(v2, normal, _Depth * displacement);

                const float3 triNormal = normalize(cross(p1 - p0, p2 - p0));

                return triNormal;
            }
            
            v2f vert(const appdata_t input)  
            {  
                v2f o;
                float3 v = input.vertex;
                float3 dir = input.normal;
             
                for (int i = 0; i < _Steps; ++i)
                {
                    float3 prev = v;
                    const float density = sampleField(v);
                    v += dir * density * _Scale;
                    dir = constructFieldTriangleNormal(v, dir, 1);
                }
                   o.world_pos = mul(unity_ObjectToWorld, v);
                o.vertex = UnityObjectToClipPos(v);
                o.normal = UnityObjectToWorldNormal(dir);
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                float3 color = sampleGradient(sampleField(id.world_pos));
                float3 n = id.normal;
                color = applyCustomLighting(color, id.world_pos, n);
                return float4(color, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  