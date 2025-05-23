Shader "Uriel/UnlitDepth"  
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
                const float value = sampleField(id.world_pos);
                const float3 c = hsv2rgb(value * _Threshold, 1, 1);
                const float grey = (c.r + c.g + c.b) / 3.0;
                return float4(grey,grey,grey, 1) * _Multiplier;  
            }  
            
            ENDCG  
        }  
    }  
}  