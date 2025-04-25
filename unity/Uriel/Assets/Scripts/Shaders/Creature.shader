Shader "Uriel/Creature"  
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
            
            float3 _Offset;
            int _GeneCount;
            float _Speed;
            sampler2D _GradientLUT;  
            float _GradientMultiplier;
            float _GradientThreshold;
            float4 _Light;
            float _SpecularThreshold;
            float _SpecularMultiplier;
            float _Shininess;
            
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
            
            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)  
            {  
                v2f o;  
                float size = 1.0;  
                float4x4 m = float4x4(  
                    size,0,0,0,   
                    0,size,0,0,  
                    0,0,size,0,  
                    0,0,0,0);
                float4 pos = mul(m, i.vertex);  
                o.vertex = UnityObjectToClipPos(pos);  
                o.worldNormal = UnityObjectToWorldNormal(i.normal);
                float4 worldPosRaw = mul(unity_ObjectToWorld, i.vertex);  
                o.worldPos = worldPosRaw.xyz;
                return o;   
            }

            float sampleField(float3 pos)
            {
                float h = 0.0;
                 for (int i = 0; i < _GeneCount; i++)
                {
                    const Gene gene = _GeneBuffer[i];
                    for (int k = 0; k < gene.iterations; k++) {
                        
                        const float3 source = (gene.offset - _Offset);
                        const float dist = saturate(distance(pos, source)  * gene.scale);
                        switch (gene.operation)
                        {
                            case 0:
                                float s = sin(dist * gene.frequency + (UNITY_PI / (8) * gene.phase)) * gene.amplitude * (gene.shift + 1);
                                for (int j = 0; j < gene.shift; j++)
                                {
                                    s = j % 2 ==0 ? cos(s + dist  * gene.frequency + j) : sin(s + dist  * gene.frequency + j) ;
                                }
                                h += s ;
                                
                               // NdotL += sin(dist * gene.frequency + (UNITY_PI / (8) * gene.phase)) * gene.amplitude * (gene.shift + 1);
                            
                            break;
                            case 1:
                                 h += sin(dist + cos(dist + sin(dist * gene.frequency + gene.phase + _Time * _Speed))) * gene.amplitude;
                            break;
                            case 2:
                                h += smoothstep(sin(dist * gene.frequency  + _Time * _Speed) * gene.amplitude, 1.0, 0.01);
                            break;
                            case 3:
                                h += sin(dist * gene.frequency  + _Time * _Speed) * gene.amplitude;
                                h += sin((dist * gene.frequency  + _Time * _Speed) + UNITY_PI / gene.phase) * gene.amplitude;
                            break;
                            default:
                                break;
                        }
                    }
                }
                return h;
            }
            
            fixed4 frag(v2f id) : SV_Target  
            {
                float value = sampleField(id.worldPos);
                float3 normalDir = normalize(id.worldNormal);
                float3 lightDir = normalize(float3(1,2,3));
                float3 viewDir = normalize(UnityWorldSpaceViewDir(id.worldPos));
                float3 halfwayDir = normalize(lightDir + viewDir);  
                float NdotL = saturate(dot(normalDir, lightDir));
                float3 diffuseLighting = NdotL * _Light.rgb;
                float NdotH = saturate(dot(normalDir, halfwayDir));  
                float3 diffuseColor = tex2D(_GradientLUT, float2(value * _GradientThreshold, 0)) * _GradientMultiplier;   
                float specularValue = saturate(value * _SpecularThreshold) * _SpecularMultiplier;  
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
                float3 specularLighting = pow(NdotH, _Shininess) * specularValue * _Light.rgb;  
                float3 finalColor = (ambient + diffuseLighting) * diffuseColor + specularLighting;  
                return float4(finalColor, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  