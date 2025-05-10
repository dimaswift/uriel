Shader "Uriel/LitDisplacement"  
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
        _Roughness ("Roughness", Range(0.0, 1.0)) = 0
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
                float3 world_pos : TEXCOORD1;
            };
            
            float _Roughness;
            
            float3 rayMarchAdjustedNormal(float3 pos, float3 normal)
            {
                float3 n = normalize(normal);
                float3 t = normalize(cross(n, float3(0.0, 1.0, 0.0)));
                float3 b = cross(n, t);
                float epsilon = 0.0001 * (1.01 - _Roughness);
                float valueCenter = sampleField(pos);
                float valueT = sampleField(pos + t * epsilon);
                float valueB = sampleField(pos + b * epsilon);
                float3 displacedCenter = pos + n * valueCenter;
                float3 displacedT = (pos + t * epsilon) + n * valueT;
                float3 displacedB = (pos + b * epsilon) + n * valueB;
                float3 displacedNormal = normalize(cross(displacedT - displacedCenter, displacedB - displacedCenter));
                return displacedNormal;
            }
            
            v2f vert(const appdata_t input)  
            {  
                v2f o;
                float4 v = input.vertex;
                o.world_pos = mul(unity_ObjectToWorld, v);
                o.vertex = UnityObjectToClipPos(v);
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                float3 color = sampleGradient(sampleField(id.world_pos));
                float3 n = rayMarchAdjustedNormal(id.world_pos, id.vertex);
                color = applyCustomLighting(color, id.world_pos, n);
                return float4(color, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  