Shader "Uriel/ParticleInterference"  
{  
    Properties  
    {  
        _Threshold ("Threshold", Range(0.0, 10.0)) = 1
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
       
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
            #include "UnityCG.cginc"
            #include "Assets/Scripts/Lib/Uriel.cginc"
            #include "Assets/Scripts/Lib/Gradient.cginc"

            StructuredBuffer<Particle> _Particles;
            uint _ParticlesCount;
            
            struct appdata_t  
            {  
                float4 vertex : POSITION;
                float4 color : COLOR;
            };  

            struct v2f  
            {
                float4 vertex : SV_POSITION;  
                float3 world_pos : TEXCOORD0;
            };  
            

            v2f vert(const appdata_t input)  
            {  
                v2f o;
                o.vertex = UnityObjectToClipPos(input.vertex);
                o.world_pos = mul(unity_ObjectToWorld, input.vertex);
                return o;   
            }
            
            fixed4 frag(const v2f id) : SV_Target  
            {
                float density = 0.0;
             
                for (int i = 0; i < _ParticlesCount; i++)
                {
                    Particle p = _Particles[i];
                    if(p.size <= 0) continue;
                //    const float dist = saturate(distance(id.world_pos * p.mass, p.position));
                    const float dist = saturate(distance(id.world_pos * p.mass, p.position));
                    const float freq = p.charge;
                    const float phase = 0;
                    const float amp = _Multiplier;
                    density += sin(dist * freq + phase) * amp;
                }
                const float3 c = hsv2rgb(density * _Threshold, 1, 1);
                const float grey = (c.r + c.g + c.b) / 3.0;
                return float4(c, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  