Shader "Uriel/CreatureSurface"  
{  
    Properties  
    {  
        _Color ("Color", Color) = (1,1,1,1)  
        _Glossiness ("Smoothness", Range(0,1)) = 0.5  
        _Metallic ("Metallic", Range(0,1)) = 0.0  
        _NormalStrength ("Normal Strength", Range(0,5)) = 1.0  
        _Harmonics ("Harmonics", Int) = 1  
        _RampTex ("Ramp Texture", 2D) = "white" {}  
        _RampThreshold ("Ramp Threshold", Range(0.0, 1.0)) = 0.5  
        _Speed ("Speed", Range(0.0, 1.0)) = 0.5  
        _DisplacementStrength ("Displacement", Range(0,1)) = 0.3  
    }  
    SubShader  
    {  
        Tags { "RenderType"="Opaque" }  
        LOD 200  
  
        CGPROGRAM  
        // Physically based Standard lighting model  
        #pragma surface surf Standard fullforwardshadows vertex:vert  
        #pragma target 4.0  
  
        struct Gene  
        {  
            int iterations;  
            int shift;  
            float frequency;  
            float amplitude;  
            int operation;  
            float3 offset;  
            float scale;  
            float phase;  
        };  

        StructuredBuffer<Gene> _GeneBuffer;  
        
        float3 _Offset;  
        int _GeneCount;  
        float4x4 _Shape;  
        sampler2D _RampTex;  
        float _RampThreshold;  
        float _Speed;  
        int _Harmonics;  
        half _Glossiness;  
        half _Metallic;  
        fixed4 _Color;  
        float _NormalStrength;  
        float _DisplacementStrength;  
  
        struct Input  
        {  
            float3 worldPos;  
            float3 localPos;  
            float3 volumePos;  
        };  
  
        // Convert HSV to RGB for coloring  
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
        
        // Calculate the interference pattern value at a position  
        float calculateInterference(float3 pos) {  
            float h = 0.0;  
            for (int i = 0; i < _GeneCount; i++)  
            {  
                const Gene gene = _GeneBuffer[i];  
                for (int k = 0; k < gene.iterations; k++) {  
                    const float3 source = (gene.offset - _Offset);  
                    const float dist = saturate(distance(pos, source));  
                    h += sin(dist * gene.frequency + _Time.y * _Speed) * gene.amplitude;  
                }  
            }  
            return h;  
        }  
        
        void vert(inout appdata_full v, out Input o) {  
            UNITY_INITIALIZE_OUTPUT(Input, o);  
            o.localPos = v.vertex.xyz;  
            o.volumePos = mul(_Shape, v.vertex);  
            
            // Apply displacement along normal based on interference pattern  
            float displacement = calculateInterference(o.volumePos) * _DisplacementStrength;  
            v.vertex.xyz += v.normal * displacement;  
        }  
        
        void surf(Input IN, inout SurfaceOutputStandard o)  
        {  
            // Calculate the main interference pattern  
            float h = calculateInterference(IN.volumePos);  
            
            // Calculate normals by sampling nearby points  
            float epsilon = 0.01;  
            float3 dx = float3(epsilon, 0, 0);  
            float3 dy = float3(0, epsilon, 0);  
            float3 dz = float3(0, 0, epsilon);  
            
            float hx1 = calculateInterference(IN.volumePos + dx);  
            float hx2 = calculateInterference(IN.volumePos - dx);  
            float hy1 = calculateInterference(IN.volumePos + dy);  
            float hy2 = calculateInterference(IN.volumePos - dy);  
            float hz1 = calculateInterference(IN.volumePos + dz);  
            float hz2 = calculateInterference(IN.volumePos - dz);  
            
            // Compute gradient for normal mapping  
            float3 grad = float3(hx1 - hx2, hy1 - hy2, hz1 - hz2) / (2.0 * epsilon);  
            float3 normal = normalize(float3(grad.x, grad.y, 1.0));  
            
            // Apply color from the pattern  
            float3 color = hsv2rgb(h, 1, 1) * _Color.rgb;  
            o.Albedo = color;  
            
            // Apply the calculated normal  
            o.Normal = normalize(float3(-grad.x, -grad.y, 1.0) * _NormalStrength);  
            
            // Apply other surface properties  
            o.Metallic = _Metallic;  
            o.Smoothness = _Glossiness;  
            o.Alpha = 1.0;  
        }  
        ENDCG  
    }  
    FallBack "Diffuse"  
}  