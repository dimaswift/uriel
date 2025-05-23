Shader "Uriel/Clay"  
{  
    Properties  
    {  
        _Threshold ("Threshold", Range(0.0, 10.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _Alpha ("Alpha", Float) = 0.5
        _Depth ("Depth", Float) = 0.5
        _Scale ("Scale", Float) = 0.5
    }  
    SubShader  
    {  
        Tags { "RenderType" = "Opaque" }  
       // Blend SrcAlpha OneMinusDstColor  
        
        Pass  
        {  
            CGPROGRAM  

            #pragma vertex vert   
            #pragma fragment frag  
            #include "Assets/Scripts/Lib/Uriel.cginc"
            #include "UnityCG.cginc" 
            
            struct appdata_t  
            {  
                float4 vertex : POSITION;
                float3 normal : NORMAL;  
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;
                Particle particle: TEXCOORD2;
                float3 world_pos: TEXCOORD1;
                float3 normal : TEXCOORD0;
                
            };
            
            StructuredBuffer<Particle> _Particles;
            float3 _LightSource;

            float _Multiplier;
            float _Threshold;
            float _Depth;
            float _Scale;
            float _Alpha;

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
                const float3 p0 = sampleDisplacedField(v0, normal, _Depth * _Scale);
                const float3 p1 = sampleDisplacedField(v1, normal, _Depth * _Scale);
                const float3 p2 = sampleDisplacedField(v2, normal, _Depth * _Scale);
                const float3 triNormal = normalize(cross(p1 - p0, p2 - p0));
                return triNormal;
            }

            v2f vert(appdata_t input, uint instanceID: SV_InstanceID)  
            {  
                v2f o;
                
                const Particle p = _Particles[instanceID];
                float4x4 m = float4x4(
                    p.size, 0, 0, p.position.x,
                    0, p.size, 0, p.position.y,
                    0, 0, p.size, p.position.z,
                    0, 0, 0, 1);
                const float4 pos = mul(m, input.vertex);  
               
                o.particle = p;
              //  o.world_pos = mul(unity_ObjectToWorld, pos);
              //  o.density = sampleField(o.world_pos);

               float3 v = pos;
               float3 dir = input.normal;
             
                for (int i = 0; i < 5; ++i)
                {
                    const float density = sampleField(v);
                    dir = constructFieldTriangleNormal(v, dir);
                    v += dir * density * _Scale;
                }
                o.world_pos = mul(unity_ObjectToWorld, v);
                o.vertex = UnityObjectToClipPos(v);
                o.normal = UnityObjectToWorldNormal(dir);
                return o;  
            }  

            fixed4 frag(v2f i) : SV_Target  
            {
                const float3 diffuse_color = hsv2rgb(sampleField(i.world_pos) * _Threshold, 1.0, 1.0);
                return float4(diffuse_color, _Alpha);  
            }   
            ENDCG  
        }  
    }  
}  