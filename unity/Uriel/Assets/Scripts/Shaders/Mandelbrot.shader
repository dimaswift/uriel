Shader "Uriel/Mandelbrot"
{
    Properties
    {
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 2.0)) = 0.5
        _Multiplier ("Multiplier", Range(-3.14, 3.14)) = 1
        _Steps("Steps", Range(1.0, 100.0)) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Scripts/Lib/Mandelbrot.cginc"
            #include "Assets/Scripts/Lib/Gradient.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 world_pos : TEXCOORD1;
            };
            
            int _Steps;
  
            v2f vert(const appdata_t input)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(input.vertex);
                o.world_pos = mul(unity_ObjectToWorld, input.vertex);
                return o;
            }

            fixed4 frag(const v2f id) : SV_Target
            {
                int iterations = computeMandelbrot(id.world_pos, _Steps);
                float3 col = sampleGradient(float(iterations) / _Steps);
                return float4(col, 1);
            }
            
            ENDCG
        }
    }
}