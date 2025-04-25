Shader "Uriel/Ray"  
{  
    Properties  
    {  
        _VolumeTexture ("Volume Texture", 3D) = "white" {}  
        _Alpha ("Alpha", Range(0.001, 1.0)) = 0.137  
        _Threshold ("Density Threshold", Range(0.0, 1.0)) = 0.1
         _RampThreshold ("Ramp Threshold", Range(0.0, 1.0)) = 0.5
        _ThresholdThin ("Density Threshold Thin", Range(0.0, 0.01)) = 0.0   
        _Quality ("Quality", Range(1, 20)) = 8  
        _StepSize ("Step Size", Range(0.001, 0.1)) = 0.01  
        _RampTex ("Ramp Texture", 2D) = "white" {}  
        _SlicePosition ("Slice Position", Range(0.0, 1.0)) = 0.5  
        _SliceThickness ("Slice Thickness", Range(0.0, 0.5)) = 0.1  
        _SliceEnabled ("Slice Enabled", Range(0, 1)) = 0  
    }  
    
    SubShader  
    {  
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }  
        Blend SrcAlpha OneMinusSrcAlpha  
        ZWrite On  
        Cull Front  
        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
            #include "UnityCG.cginc"  

            struct appdata  
            {  
                float4 vertex : POSITION;  
                float2 uv : TEXCOORD0;  
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;  
                float3 objectPos : TEXCOORD0;  
                float3 viewRayOS : TEXCOORD1;  
                float3 camPosOS : TEXCOORD2;  
            };  

            sampler3D _VolumeTexture;  
            float _Alpha;  
            float _Threshold;  
            int _Quality;  
            float _StepSize;  
            sampler2D _RampTex;  
            float _SlicePosition;  
            float _SliceThickness;  
            float _SliceEnabled;  
            float _ThresholdThin;
            float _RampThreshold;
            
            v2f vert (appdata v)  
            {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.objectPos = v.vertex.xyz;  
                
                // Camera position in object space  
                float3 worldCamPos = _WorldSpaceCameraPos;  
                o.camPosOS = mul(unity_WorldToObject, float4(worldCamPos, 1.0)).xyz;  
                
                // View ray in object space  
                o.viewRayOS = normalize(v.vertex.xyz - o.camPosOS);  
                
                return o;  
            }  
            
            float4 sampleVolume(float3 pos)  
            {  
                // Sample volume texture  
                float density = tex3D(_VolumeTexture, pos).r;  
                float threshold = _ThresholdThin + _Threshold;
                // Threshold adjustment  
                density = max(0, density - threshold) / (1.0 - threshold);  
                
                // Apply color ramp  
                float4 color = tex2D(_RampTex, float2(density, _RampThreshold));  
                
                // Apply slice plane if enabled  
                if (_SliceEnabled > 0.5) {  
                    float distance = abs(pos.y - _SlicePosition);  
                    if (distance > _SliceThickness * 0.5) {  
                        color.a = 0;  
                    }  
                }  
                
                return color;  
            }  
            
            fixed4 frag (v2f i) : SV_Target  
            {  
                // Transform coordinates to 0-1 range for texture sampling  
                float3 rayStart = i.objectPos * 0.5 + 0.5;  
                float3 rayDir = normalize(i.viewRayOS);  
                
                // Calculate ray box intersection in 0-1 space  
                float3 invRayDir = 1.0 / rayDir;  
                float3 t1 = (float3(0, 0, 0) - rayStart) * invRayDir;  
                float3 t2 = (float3(1, 1, 1) - rayStart) * invRayDir;  
                
                float3 tMin = min(t1, t2);  
                float3 tMax = max(t1, t2);  
                
                float tnear = max(max(tMin.x, tMin.y), tMin.z);  
                float tfar = min(min(tMax.x, tMax.y), tMax.z);  
                
                // Skip if ray misses the box  
                if (tnear > tfar || tfar < 0)  
                    return float4(0, 0, 0, 0);  
                
                // Start ray from entry point  
                tnear = max(0, tnear);  
                float3 rayPos = rayStart + rayDir * tnear;  
                
                // Calculate step size based on quality setting  
                float stepSize = _StepSize / _Quality;  
                
                // Ray march through volume  
                float4 result = float4(0, 0, 0, 0);  
                
                // Calculate number of steps based on ray length and step size  
                int maxSteps = (int)ceil((tfar - tnear) / stepSize);  
                maxSteps = min(maxSteps, 1000); // Safety cap  

                [loop]
                for (int step = 0; step < maxSteps; step++)  
                {  
                    // Current position along the ray  
                    float3 pos = rayPos + rayDir * (step * stepSize);  
                    
                    // Break if we exit the volume  
                    if (any(pos < 0) || any(pos > 1))  
                        break;  
                    
                    // Sample the volume at current position  
                    float4 sample = sampleVolume(pos);  
                    
                    // Scale alpha by our global alpha control  
                    float alpha = sample.a * _Alpha * stepSize * 10.0;  
                    
                    // Pre-multiply alpha  
                    sample.rgb *= alpha;  
                    
                    // Front-to-back compositing  
                    result.rgb += (1.0 - result.a) * sample.rgb;  
                    result.a += (1.0 - result.a) * alpha;  
                    
                    // Early ray termination  
                    if (result.a >= 0.99)  
                        break;  
                }  
                
                return result;  
            }  
            ENDCG  
        }  
    }  
}  