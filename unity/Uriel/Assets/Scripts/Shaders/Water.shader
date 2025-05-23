Shader "Uriel/Water"  
{  
    Properties  
    {  
        _Gradient ("Gradient", 2D) = "white" {}
        _GradientStart ("Gradient Start", Range(0.0, 10.0)) = 0
        _GradientEnd ("Gradient End", Range(0.0, 10.0)) = 0
        _Threshold ("Threshold", Range(0.0, 1.0)) = 10
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 10
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0.0, 10.0)) = 1.0
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 10.0)) = 1.0
        _Shininess("Shininess", Range(0.0, 360.0)) = 1.0
        _Depth ("Depth", Range(-0.1, 0.1)) = 0.025
        _Displacement ("Displacement", Range(0, 1)) = 1
        _Smoothness ("Smoothness", Range(-1, 1)) = 0.17
        _Steps ("Steps", Range(1, 50)) = 1
        _Frequency ("Frequency", Range(-3, 3)) = 0
        _Amplitude ("Amplitude", Range(-3, 3)) = 0
        _Scale ("Scale", Range(-0.1, 0.1)) = 0.005
        
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
                float4 color : COLOR;
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;  
                float3 world_pos : TEXCOORD0;
            };
            
            float _Depth;
            float _Smoothness;
            int _Steps;
            float _Frequency;
            float _Amplitude;
            float _Displacement;
            float _Scale;
            float _GradientStart;
            float _GradientEnd;
            StructuredBuffer<Modulation> _ModulationBuffer;
            

            float3 calcNormal(float3 p)
            {
                float2 e = float2(_Depth, 0);
                float3 n = float3(
                    sampleField(p + e.xyy, _ModulationBuffer[0]) - sampleField(p - e.xyy, _ModulationBuffer[0]),
                    sampleField(p + e.yxy, _ModulationBuffer[0]) - sampleField(p - e.yxy, _ModulationBuffer[0]),
                    sampleField(p + e.yyx, _ModulationBuffer[0]) - sampleField(p - e.yyx, _ModulationBuffer[0])
                );
                return normalize(n);
            }
            
            float3 sampleDisplacedField(float3 pos, float3 normal, float epsilon)
            {
                const float density = sampleField(pos);
                return pos + normal * density * epsilon;
            }
            
            float3 constructFieldTriangleNormal(float3 center, float3 normal)
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
                const float3 p0 = sampleDisplacedField(v0, normal, _Depth);
                const float3 p1 = sampleDisplacedField(v1, normal, _Depth);
                const float3 p2 = sampleDisplacedField(v2, normal, _Depth);
                const float3 triNormal = normalize(cross(p1 - p0, p2 - p0));
                return triNormal;
            }
            
            
            float3 rayMarch(float3 origin, float3 dir)
            {
                float3 pos = origin;
                float marchPhase = 0.0;
                float v = 0;
                float3 current_dir = dir;
                for (int i = 0; i < _Steps; i++)
                {
                    pos += current_dir * sin(marchPhase) * _Scale;
                    marchPhase += _Smoothness;
                    float3 n = calcNormal(pos);
                    v += applyCustomLighting(_LightColor, pos, n).x * _Frequency;
                    current_dir += n * _Displacement;
                }
                
                return sampleGradient(map(v, -1, 1,  _GradientStart, _GradientEnd));
            }
            
            v2f vert(const appdata_t input)  
            {  
                v2f o;
                float3 v = input.vertex;
                o.world_pos = mul(unity_ObjectToWorld, v);
                o.vertex = UnityObjectToClipPos(v);
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                float3 color = rayMarch(id.world_pos, UnityWorldSpaceViewDir(id.world_pos));
                return float4(color, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  