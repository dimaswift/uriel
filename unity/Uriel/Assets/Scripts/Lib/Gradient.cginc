#ifndef GRADIENT_INCLUDED
#define GRADIENT_INCLUDED

sampler2D _Gradient;  
float _Multiplier;
float _Threshold;

float3 sampleGradient(float value)
{
    return tex2D(_Gradient, float2(value * _Threshold, 0)) * _Multiplier;
}

#endif