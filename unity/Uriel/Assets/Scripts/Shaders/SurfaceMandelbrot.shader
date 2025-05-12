Shader "Uriel/SurfaceMandelbrot"
{
    Properties
    {
        _OrbitOrigin ("Origin", Vector) = (0,1,1)
        _Gradient ("Gradient", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 2.0)) = 0.5
        _Multiplier ("Multiplier", Range(-3.14, 3.14)) = 1
        _Steps("Steps", Range(1.0, 100.0)) = 1.0
        _OrbitPlaneCount("Orbital Planes", Range(1, 32)) = 4
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
            #include "Assets/Scripts/Lib/Uriel.cginc"
            #include "Assets/Scripts/Lib/CustomLight.cginc"
            #include "Assets/Scripts/Lib/Gradient.cginc"

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
            
            int _Steps;
            float2 _OrbitOrigin;
            int _OrbitPlaneCount;

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
                const float value = sampleMandelbrot(id.world_pos, _OrbitOrigin, _Steps, _OrbitPlaneCount);
                const float3 diffuse_color = sampleGradient(value);
                return float4(diffuse_color, 1);
            }
            ENDCG
        }
    }
}