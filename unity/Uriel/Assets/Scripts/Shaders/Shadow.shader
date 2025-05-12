Shader "Uriel/Shadow"  
{  
    Properties  
    {  
        _Depth ("Depth", Range(0, 10)) = 1
        _Steps ("Steps", Range(1, 256)) = 64
        _GradientLUT ("Gradient LUT", 2D) = "white" {}
        _GradientThreshold ("Gradient Threshold", Range(0.0, 1.0)) = 0.5
        _GradientMultiplier ("Gradient Multiplier", Range(0.0, 10.0)) = 1
        _Frequency ("Frequency", Range(0.0, 1.1)) = 0.05
        _Strength ("Strengh", Range(0.0, 1.0)) = 0.5
        _Min ("Min", Range(2.4, 2.6)) = 2.5
        _Max ("Max", Range(2.4, 2.6)) = 2.5
        _Grayscale ("Grayscale", Range(0, 1)) = 1
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
                float3 world_normal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };  
            
    
            uint _Steps;
            sampler2D _GradientLUT;  
            float _GradientMultiplier;
            float _GradientThreshold;
            float _Frequency;
            float _Strength;
            float _Min;
            float _Max;
            int _Grayscale;
            int _Depth;
    
            
            float3 _Source;

            v2f vert(appdata_t input, uint instanceID: SV_InstanceID)  
            {  
                v2f o;  

                o.world_normal = UnityObjectToWorldNormal(input.normal);
                float4 pos = input.vertex;  
                o.vertex = UnityObjectToClipPos(pos);  
                float4 worldPosRaw = mul(unity_ObjectToWorld, input.vertex);  
                o.worldPos = worldPosRaw.xyz;
                return o;   
            }
            
            fixed4 frag(v2f id) : SV_Target  
            {
                float total = rayMarchField(_Source, id.worldPos, length(id.worldPos - _Source), _Steps, _Depth,
        _Frequency, _Min, _Max,  _Strength);
                float3 diffuseColor;
                if(_Grayscale == 0)
                {
                    diffuseColor = tex2D(_GradientLUT, float2(total * _GradientThreshold, 0)) * _GradientMultiplier;
                }
                else
                {
                    diffuseColor = saturate(total);
                }
                return float4(diffuseColor, 1);  
            }  
            
            ENDCG  
        }  
    }  
}  