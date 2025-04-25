Shader "Uriel/CreatureMap"  
{  
    Properties  
    {  
        _Speed ("Speed", Range(0.0, 1.0)) = 0.5
        _GradientLUT ("Gradient LUT", 2D) = "white" {}
        _GradientThreshold ("Gradient Threshold", Range(0.0, 10.0)) = 0.5
        _GradientMultiplier ("Gradient Multiplier", Range(0.0, 10.0)) = 1
        _Light ("Light", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0.0, 10.0)) = 1.0
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 10.0)) = 1.0
        _Shininess("Shininess", Range(0.0, 10.0)) = 1.0
        _UseLight("Use Light", Range(0, 1)) = 0
    }  
    SubShader  
    {  
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }  
        Cull Off 
        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag
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
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };  
            
    
            
            sampler2D _GradientLUT;  
            float _GradientMultiplier;
            float _GradientThreshold;
            float4 _Light;
            float _SpecularThreshold;
            float _SpecularMultiplier;
            float _Shininess;
            uint _UseLight;
            uint _GeneCount;
            StructuredBuffer<Gene> _GeneBuffer; 
            
            v2f vert(appdata_t input, uint instanceID: SV_InstanceID)  
            {  
                v2f o;  
                float size = 1.0;
                o.worldNormal = UnityObjectToWorldNormal(input.normal);
                float4x4 m = float4x4(  
                    size,0,0,0,   
                    0,size,0,0,  
                    0,0,size,0,  
                    0,0,0,0);
                float4 pos = mul(m, input.vertex);  
                o.vertex = UnityObjectToClipPos(pos);  
                float4 worldPosRaw = mul(unity_ObjectToWorld, input.vertex);  
                o.worldPos = worldPosRaw.xyz;
                return o;   
            }
            
            fixed4 frag(v2f id) : SV_Target  
            {
                float value = sampleField(id.worldPos, _GeneCount, _GeneBuffer);
                const float3 diffuseColor = tex2D(_GradientLUT, float2(value * _GradientThreshold, 0)) * _GradientMultiplier;
                float3 finalColor;
                if (_UseLight == 1)
                {
                    const float3 normalDir = normalize(id.worldNormal);
                    const float3 lightDir = normalize(float3(1,2,3));
                    const float3 viewDir = normalize(UnityWorldSpaceViewDir(id.worldPos));
                    const float3 halfwayDir = normalize(lightDir + viewDir);  
                    const float ndot_l = saturate(dot(normalDir, lightDir));
                    const float3 diffuseLighting = ndot_l  * _Light.rgb;
                    const float ndot_h = saturate(dot(normalDir, halfwayDir));  
                    const float specularValue = saturate(value * _SpecularThreshold) * _SpecularMultiplier;  
                    const float3 specularLighting = pow(ndot_h, _Shininess) * specularValue * _Light.rgb;  
                    finalColor = (diffuseLighting) * diffuseColor + specularLighting;  
                }
                else
                {
                    finalColor = diffuseColor;  
                }
          
                return float4(finalColor, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  