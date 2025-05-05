#ifndef PENTATOPE_INCLUDED
#define PENTATOPE_INCLUDED

static const float4 simplex4D[5] = {
    float4(1, 1, 1, -1.0f / sqrt(5)),
    float4(1, -1, -1, -1.0f / sqrt(5)),
    float4(-1, 1, -1, -1.0f / sqrt(5)),
    float4(-1, -1, 1, -1.0f / sqrt(5)),
    float4(0, 0, 0, 4.0f / sqrt(5))
};

cbuffer BarycentricWeights : register(b0)
{
    float weights[5]; // External control!
}

float4 interpolateBarycentric()
{
    float4 result = float4(0, 0, 0, 0);
    for (int i = 0; i < 5; i++)
    {
        result += weights[i] * simplex4D[i];
    }
    return result;
}

float3 projectTo3D(float4 point4D)
{
    float w = 2.0f + point4D.w;
    return point4D.xyz / w;
}

#endif
