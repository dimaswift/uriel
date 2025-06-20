
Shader "Uriel/Wireframe"
{
    Properties
    {
        _WireColor ("Wire Color", Color) = (1, 1, 1, 1)
        _WireWidth ("Wire Width", Range(0.5, 5.0)) = 1.0
        _FillColor ("Fill Color", Color) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma target 4.0

            #include "UnityCG.cginc"

            float4 _WireColor;
            float _WireWidth;
            float4 _FillColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            v2g vert(appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;
                
                o.pos = input[0].pos;
                o.barycentric = float3(1, 0, 0);
                triStream.Append(o);
                
                o.pos = input[1].pos;
                o.barycentric = float3(0, 1, 0);
                triStream.Append(o);
                
                o.pos = input[2].pos;
                o.barycentric = float3(0, 0, 1);
                triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                float3 deltas = fwidth(i.barycentric);
                float3 smoothing = deltas * _WireWidth;
                float3 thickness = smoothstep(float3(0, 0, 0), smoothing, i.barycentric);
                
                float minThickness = min(thickness.x, min(thickness.y, thickness.z));
                
                return lerp(_WireColor, _FillColor, minThickness);
            }
            ENDCG
        }
    }
}
