
Shader "Uriel/Wireframe"
{
    Properties
    {
        _Color ("Wireframe Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Overlay+100"
            "IgnoreProjector"="True"
        }
        LOD 100

        Pass
        {
            // Render on top of everything
            ZTest Always
            ZWrite Off
            
            // Enable transparency
            Blend SrcAlpha OneMinusSrcAlpha
            
            // Disable culling so wireframe shows from both sides
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Use the wireframe color
                fixed4 col = _Color;
                col.a *= _Alpha;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    
    // Fallback for older hardware
    Fallback "Unlit/Transparent"
}
