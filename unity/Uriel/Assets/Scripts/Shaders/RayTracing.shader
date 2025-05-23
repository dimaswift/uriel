Shader "Uriel/RayTracing"
{
    Properties
    {
        _Gradient ("Gradient", 2D) = "white" {}
        _GradientStart ("Gradient Start", Range(0.0, 10.0)) = 0
        _GradientEnd ("Gradient End", Range(0.0, 10.0)) = 0
        _Threshold ("Threshold", Range(0.0, 1.0)) = 10
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 10
        
        _SpecularThreshold("Specular Threshold", Range(0.0, 10.0)) = 1.0
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 10.0)) = 1.0
        _Shininess("Shininess", Range(0.0, 360.0)) = 1.0
        
        _MaxSteps ("Max Ray Steps", Int) = 100
        _MaxDistance ("Max Ray Distance", Float) = 100
        _Epsilon ("Surface Epsilon", Float) = 0.001
        _StepSize ("Ray Step Size", Range(0.0001, 1.0)) = 0.1
        _MaxBounces ("Max Bounces", Range(1, 10)) = 3
        
        // Material thresholds
        _ReflectiveMin ("Reflective Min", Range(-1, 1)) = 1.0
        _ReflectiveMax ("Reflective Max", Float) = 1.1
        _DiffuseMin ("Diffuse Min", Range(-1, 5)) = 0.5
        _DiffuseMax ("Diffuse Max", Range(-1, 5)) = 0.9
        _AbsorptionCoef ("Absorption Coefficient", Range(0, 1)) = 0.1
        
        // Colors
        _SkyColor ("Sky Color", Color) = (0.5, 0.7, 1.0, 1.0)
        _LightSource ("Light Direction", Vector) = (0.577, 0.577, 0.577, 0)
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)
        _AmbientColor ("Ambient Color", Color) = (0.2, 0.2, 0.2, 1)
        
         _Refraction ("Refraction", Float) = 0.001
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Scripts/Lib/Uriel.cginc"
            #include "Assets/Scripts/Lib/Gradient.cginc"
            #include "Assets/Scripts/Lib/CustomLight.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 rayOrigin : TEXCOORD1;
                float3 rayDir : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            int _MaxSteps;
            float _MaxDistance;
            float _Epsilon;
            float _StepSize;
            int _MaxBounces;
            
            float _ReflectiveMin;
            float _ReflectiveMax;
            float _DiffuseMin;
            float _DiffuseMax;
            float _AbsorptionCoef;
            float _Refraction;
            float _Depth;
            
            float4 _SkyColor;
            float4 _LightDir;
            float4 _AmbientColor;
            float _GradientStart;
            float _GradientEnd;
  
            // Calculate normal using gradient
            float3 calcNormal(float3 p)
            {
                float2 e = float2(_Epsilon, 0);
                const float d0 = sampleField(p + e.xyy);
                const float d1 = sampleField(p - e.xyy);
                const float d2 = sampleField(p + e.yxy);
                const float d3 = sampleField(p - e.yxy);
                const float d4 = sampleField(p + e.yyx);
                const float d5 = sampleField(p - e.yyx);
                float3 n = float3(
                    d0 - d1,
                    d2 - d3,
                    d4 - d5
                );
                return normalize(n);
            }
            

            // March ray through the field and find an intersection
            bool rayMarch(float3 rayOrigin, float3 rayDir, out float3 hitPos, out float density)
            {
                float t = 0.0;
                
                for (int i = 0; i < _MaxSteps; i++) {
                  //  if (t > _MaxDistance) break;
                    
                    hitPos = rayOrigin + rayDir * t;
                    density = sampleField(hitPos);
                    if (density >= _DiffuseMin || density > _DiffuseMax) {
                        return true;
                    }
                    t += _StepSize;
                }
                
                return false;
            }

           // Simple random function for Russian roulette
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // Trace ray with bounces
            float4 traceRay(float3 rayOrigin, float3 rayDir)
            {
                float3 color = float3(0, 0, 0);
                float3 throughput = float3(1, 1, 1);
          
                [loop]
                for (int bounce = 0; bounce < _MaxBounces; bounce++)
                {
                    float3 hitPos;
                    float density;

                    if (!rayMarch(rayOrigin, rayDir, hitPos, density)) {
                        color += throughput * _SkyColor.rgb;
                        break;
                    }
               
                    const float3 normal = calcNormal(hitPos);
                    
                    const float3 lighting = applyCustomLighting(float3(1,1,1), hitPos, normal);
          
                    color += sampleGradient(sampleField(hitPos + normal * _Refraction)) * lighting;
                    
                    float p = max(throughput.r, max(throughput.g, throughput.b));
                    
                    if (bounce > 2) {
                        if (random(hitPos.xy + float2(bounce, 0)) > p)
                            break;
                        throughput /= p;
                    }

                    if (dot(throughput, throughput) < 0.001)
                        break;
   
                    rayOrigin = hitPos + normal * _Epsilon * 2.0;
                    rayDir = reflect(rayDir, normal);
                }
                
                return float4(color, 1.0);
            }
            
    
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                const float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.rayOrigin = _WorldSpaceCameraPos;
                o.rayDir = normalize(worldPos - o.rayOrigin);
                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return traceRay(i.rayOrigin, i.rayDir);
            }
            
            ENDCG
        }
    }
}