Shader "Uriel/Voxel"  
{  
    Properties  
    {  
        _Shell ("Shell", Range(1, 128)) = 12  
        _ColorScale ("Color scale", Range(0.0, 10.00)) = 6.12  
        _Frequency ("Frequency", Range(0.0, 100.0)) = 10.0  
        _Amplitude ("Amplitude", Range(0.0, 10.00)) = 1.0  
        _Offset ("Offset", Vector) = (0,0,0) 
        _MinColor ("Min Color", Range(0.0, 1.0)) = 0.0  
        _MaxColor ("Max Color", Range(0.0, 1.0)) = 1.0  
    }  
    SubShader  
    {  
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }  
       // Blend SrcAlpha OneMinusSrcAlpha  
        ZWrite On
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
                float4 color : COLOR;  
            };  

            struct v2f  
            {  
                float4 vertex : SV_POSITION;  
                float3 volumePos : TEXCOORD0; // Position for volume texture sampling  
            };  

            int _Shell;
            float _ColorScale;
            float _Frequency;
            float _Amplitude;
            float3 _Offset;
            float _MinColor;
            float _MaxColor;
            
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
                float size = 1;  
                float4x4 m = float4x4(  
                    size,0,0,0,   
                    0,size,0,0,  
                    0,0,size,0,  
                    0,0,0,1);
                float4 pos = mul(m, i.vertex);  
                o.vertex = UnityObjectToClipPos(pos);  
                o.volumePos = pos.xyz + _Offset;
                
                return o;   
            }  

            fixed4 frag(v2f id) : SV_Target  
            {
                float h = 0.0;
                float s = 0.0;
                float v = 0.0;
                for (int i = 0; i < _Shell; i++) {
                    
                    const float a = float(i) / float(_Shell) * UNITY_PI;
                    const float3 source = float3(sin(a), cos(a), -sin(a));
                    const float d_mirror = distance(id.volumePos, source)  * _ColorScale;
                    h += sin(d_mirror * (_Frequency + _SinTime)) * _Amplitude;
                    s += cos(sqrt(d_mirror) * (_Frequency )) * _Amplitude;
                    v += cos(d_mirror * d_mirror * (_Frequency )) * _Amplitude;
                }
                
                float3 col = hsv2rgb(h,1,1);
                return float4(col.x, col.x, col.x, 1.0);  
            }  
            
            ENDCG  
        }  
    }  
}  