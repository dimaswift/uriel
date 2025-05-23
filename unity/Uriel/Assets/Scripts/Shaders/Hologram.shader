Shader "Uriel/Hologram"  
{  
    Properties  
    {  
        
        _Depth ("Depth", Range(-1, 1)) = 0
        _Displacement ("Displacement", Range(0, 1)) = 0
        _Steps ("Steps", Range(1, 100)) = 1
        _Refraction ("Refraction", Range(-3, 3)) = 0
        _Hue ("Hue", Range(0, 2)) = 1
        _Value ("Value", Range(0, 2)) = 1
        _Saturation ("Saturation", Range(0, 2)) = 1
        _Multiplier ("Multiplier", Range(0, 2)) = 1
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
                float3 world_pos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };
            
            float _Depth;
            int _Steps;
            float _Refraction;
            float _Displacement;
            float _Hue;
            float _Value;
            float _Saturation;
            float _Multiplier;
            
            float3 sampleDisplacedField(float3 pos, float3 normal, float epsilon)
            {
                const float density = sampleField(pos);
                return pos + normal * density * epsilon;
            }
            
            float3 constructFieldTriangleNormal(float3 center, float3 normal)
            {
                const float3 tangent = normalize(cross(normal, float3(0, 1, 0)));
                const float3 bitangent = cross(normal, tangent);
                const float eps = 0.1 * _Refraction;
                const float3 v0_offset = tangent + bitangent;
                const float3 v0 = center + v0_offset * eps;
                const float twoPiOver3 = 2.09439510239;
                const float3 v1_offset = tangent * cos(twoPiOver3) + bitangent * sin(twoPiOver3);
                const float3 v2_offset = tangent * cos(twoPiOver3 * twoPiOver3) + bitangent * sin(2.0 * twoPiOver3);
                const float3 v1 = center + v1_offset * eps;
                const float3 v2 = center + v2_offset * eps;
                const float3 p0 = sampleDisplacedField(v0, normal, _Depth * 0.1);
                const float3 p1 = sampleDisplacedField(v1, normal, _Depth * 0.1);
                const float3 p2 = sampleDisplacedField(v2, normal, _Depth * 0.1);
                const float3 triNormal = normalize(cross(p1 - p0, p2 - p0));
                return triNormal;
            }

            float3 calcNormal(float3 p)
            {
                float2 e = float2(0.001, 0);
                float3 n = float3(
                    sampleField(p + e.xyy) - sampleField(p - e.xyy),
                    sampleField(p + e.yxy) - sampleField(p - e.yxy),
                    sampleField(p + e.yyx) - sampleField(p - e.yyx)
                );
                return normalize(n);
            }
            
            float3 rayMarch(float3 origin, float3 dir) {
                
                const float m = 1.0 / _Steps;
                float3 col = float3(0,0,0); 
                for (int i = 0; i < _Steps; ++i)
                {
                    const float3 pos = origin + dir * _Displacement * i;
                    const float density = sampleField(pos);
                    dir = calcNormal(pos);
                    col += hsv2rgb(density * _Hue * 0.001, _Value, _Saturation) * m;
                }
                return col * _Multiplier;
            }
            
            v2f vert(const appdata_t input)  
            {  
                v2f o;
                float3 v = input.vertex;
                const float3 dir = input.normal;
                o.world_pos = mul(unity_ObjectToWorld, v);
                o.vertex = UnityObjectToClipPos(v);
                o.normal = UnityObjectToWorldNormal(dir);
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