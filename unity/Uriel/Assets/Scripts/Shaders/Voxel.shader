Shader "Uriel/Voxel"  
{  
    Properties  
    {  
        _Cutoff ("Cutoff", Range(-10.0, 10.0)) = 0
        _CutoffExtra ("Cutoff Extra", Range(0.0, 1.01)) = 0
        _GradientLUT ("Gradient LUT", 2D) = "white" {}
        _GradientThreshold ("Gradient Threshold", Range(-10, 10)) = 0.0
         _GradientMultiplier ("Gradient Multiplier", Range(0.1, 10.0)) = 0.0
        _SpecularThreshold("Specular Threshold", Range(-0.1, 0.1)) = 0.0
        _SpecularMultiplier("Specular Multiplier", Range(0.0, 10.0)) = 1.0
        _Shininess("Shininess", Range(0.0, 10.0)) = 1.0
    }  
    SubShader  
    {  

        Tags  
        {  
            "RenderType" = "Opaque"  
            "Queue" = "Transparent" 
        }  
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
                float3 world_pos : TEXCOORD0;
                float3 color : TEXCOORD1;
            };
            
            
            StructuredBuffer<float4x4> _Particles;
            float _Cutoff;
            sampler2D _GradientLUT;  
            float _GradientMultiplier;
            float _GradientThreshold;
            float _SpecularThreshold;
            float _SpecularMultiplier;
            float _Shininess;
            float _CutoffExtra;
            
            uint _PhotonCount;
            StructuredBuffer<Photon> _PhotonBuffer;
            
            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)  
            {  
                v2f o;
                
                const float4x4 p = _Particles[instanceID];
                float size = p[0][0];
                if(p[3][0] > _Cutoff + _CutoffExtra)
                {
                    size = 0;
                }
                const float4x4 m = float4x4(
                    size,0,0,p[0][3],
                    0,size,0,p[1][3],
                    0,0,size,p[2][3],
                    1,1,1,1);
                const float4 pos = mul(m, i.vertex);  
                o.vertex = UnityObjectToClipPos(pos);
                const float3 finalColor = hsv2rgb(sampleField(pos * _GradientThreshold, float3(0,1,0), _PhotonCount, _PhotonBuffer), 1.0, 1.0);  
                o.world_pos = pos;
                o.color = finalColor;
                return o;  
            }  

            fixed4 frag(v2f i) : SV_Target  
            {
                return fixed4(i.color, 1.0);
            }   
            ENDCG  
        }  
    }  
}  