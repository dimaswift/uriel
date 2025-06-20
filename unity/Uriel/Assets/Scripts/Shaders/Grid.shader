
Shader "Uriel/Grid"
{
    Properties
    {
        [Header(Grid Settings)]
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 1)
        _GridColor ("Grid Color", Color) = (0.8, 0.8, 0.8, 1)
        _GridSize ("Grid Size", Float) = 10.0
        _LineThickness ("Line Thickness", Range(0.005, 0.1)) = 0.02
        
        [Header(Surface)]
        _BaseMap ("Base Map", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.1
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        
        [Header(Options)]
        [Toggle] _UseWorldSpace ("Use World Space", Float) = 1
        [Toggle] _EnableAntiAliasing ("Enable Anti-Aliasing", Float) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BackgroundColor;
                float4 _GridColor;
                float _GridSize;
                float _LineThickness;
                float _Smoothness;
                float _Metallic;
                float _UseWorldSpace;
                float _EnableAntiAliasing;
            CBUFFER_END

            // Simple and stable grid function
            float SimpleGrid(float2 coords, float lineWidth, float gridScale)
            {
                // Scale coordinates
                float2 scaledCoords = coords * gridScale;
                
                // Get distance to nearest grid line
                float2 grid = abs(frac(scaledCoords) - 0.5);
                
                if (_EnableAntiAliasing > 0.5)
                {
                    // Anti-aliased version
                    float2 fw = fwidth(scaledCoords) * 0.5;
                    float2 gridAA = smoothstep(0.5 - lineWidth - fw, 0.5 - lineWidth + fw, grid);
                    return 1.0 - min(gridAA.x, gridAA.y);
                }
                else
                {
                    // Sharp version
                    float2 gridSharp = step(grid, lineWidth);
                    return max(gridSharp.x, gridSharp.y);
                }
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Standard vertex transformations
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = normalInput.normalWS;
                output.shadowCoord = GetShadowCoord(vertexInput);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Choose coordinate system
                float2 coords = _UseWorldSpace > 0.5 ? input.positionWS.xz : input.uv;
                
                // Generate grid
                float grid = SimpleGrid(coords, _LineThickness, _GridSize);
                
                // Sample base texture
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Blend grid colors
                half4 albedo = lerp(_BackgroundColor, _GridColor, grid);
                albedo *= baseColor;
                
                // Simple lighting
                Light mainLight = GetMainLight(input.shadowCoord);
                float3 normalWS = normalize(input.normalWS);
                
                // Lambert lighting
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 lighting = mainLight.color * mainLight.shadowAttenuation * NdotL;
                
                // Add ambient lighting
                float3 ambient = SampleSH(normalWS) * 0.2;
                
                // Final color
                float3 color = albedo.rgb * (lighting + ambient + 0.1); // +0.1 for minimum visibility
                
                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        // Minimal shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
