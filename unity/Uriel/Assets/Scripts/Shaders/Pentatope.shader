Shader "Uriel/Pentatope"  
{  
    Properties  
    {  
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 10000.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _WaveFrequency("Wave Frequency", Float) = 5.0
        _WaveAmplitude("Wave Amplitude", Range(-0.1, 0.1)) = 0.1
        _WaveDensity("Wave Density", Float) = 0.5
        _WaveDepth("Wave Depth", Float) = 1.0
        _X("X", Int) = 1
        _Y("Y", Int) = 1
        _Z("Z", Int) = 1
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
                float3 world_normal : TEXCOORD0;
                float3 world_pos : TEXCOORD1;
            };

            
            sampler2D _Gradient;  
            float _Multiplier;
            float _Threshold;
            float _WaveFrequency;
            float _WaveAmplitude;
            float _WaveDensity;
            float _WaveDepth;
            int _X, _Y, _Z;

            v2f vert(const appdata_t input)  
            {  
                v2f o;
                o.world_normal = UnityObjectToWorldNormal(input.normal);
                o.world_pos = mul(unity_ObjectToWorld, input.vertex);
                o.vertex = UnityObjectToClipPos(input.vertex); 
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                float value = 0;

                for (int y = -_Y; y <= _Y; y++)
                {
                    for (int x = -_X; x <= _X; x++)
                    {
                        for (int z = -_Z; z <= _Z; z++)
                        {
                            const float3 offset2 = float3(x, y, z) * _WaveDepth;
                            const float dist = distance(id.world_pos * _WaveDensity, offset2);
                            value += sin(dist * _WaveFrequency) * 0.000001 * _WaveAmplitude; 
                        }
                    }
                }
                const float3 diffuse_color = hsv2rgb(value * (_Threshold * 1000), 1, 1) * _Multiplier;
                return float4(diffuse_color.x,diffuse_color.x,diffuse_color.x, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  