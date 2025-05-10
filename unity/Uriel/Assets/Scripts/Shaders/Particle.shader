Shader "Uriel/Particle"  
{  
    Properties  
    {  
        _Threshold ("Threshold", Range(0.0, 10.0)) = 0.5
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.5
    }  
    SubShader  
    {  
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }  
        Blend SrcAlpha OneMinusDstColor  
        
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
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;
                Particle particle: TEXCOORD2;
            };
            
            StructuredBuffer<Particle> _Particles;
            float3 _LightSource;

            float _Multiplier;
            float _Threshold;
            
            float _Alpha;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)  
            {  
                v2f o;
                
                const Particle p = _Particles[instanceID];
                float4x4 m = float4x4(
                    p.size, 0, 0, p.position.x,
                    0, p.size, 0, p.position.y,
                    0, 0, p.size, p.position.z,
                    0, 0, 0, 1);
                const float4 pos = mul(m, i.vertex);  
                o.vertex = UnityObjectToClipPos(pos);
                o.particle = p;
                return o;  
            }  

            fixed4 frag(v2f i) : SV_Target  
            {
                const float3 diffuse_color = hsv2rgb(i.particle.charge * _Threshold, 1.0, 1.0) * _Multiplier;
                return float4(diffuse_color, _Alpha);  
            }   
            ENDCG  
        }  
    }  
}  