#ifndef URIEL
#define URIEL

#define PI 3.14159265359

struct Wave 
{
    float3 source;
    uint ripples;
    uint harmonic;
    float frequency;
    float amplitude;
    float density;
    float phase;
};

float sampleField(float3 pos, uint count, StructuredBuffer<Wave> buffer)
{
    float result = 0.0;
    for (uint i = 0; i < count; i++)
    {
        const Wave wave = buffer[i]; 
        const float dist = saturate(distance(pos, wave.source) * wave.density);
        result += sin(dist * wave.frequency) * wave.amplitude;
        for (uint j = 0; j < wave.ripples; j++)
        {
            const float d = dist * (wave.frequency / wave.ripples);
            result += sin(d * (float(j) / wave.ripples)) * wave.amplitude;
            for (uint h = 0; h < wave.harmonic; h++)
            {
                result += saturate(sin(d * (float(h) / wave.harmonic))) * wave.amplitude;
            }
        }
     
    }
    return result; 
}

float rayMarchField(float3 origin, float3 target, uint steps, uint depth, float frequency,
    float min, float max, float amplitude, uint waveCount, StructuredBuffer<Wave> buffer)
{
    const float3 dir = target - origin;
    const float rayLength = length(dir);  
    const float stepSize = rayLength / steps;
    const float3 rayDir = normalize(dir);  
    float total = 0.0;
    for (uint i = 0; i < steps; i++)  
    {  
        const float t = i * stepSize;  
        const float3 p = origin + rayDir * t;
        float v = sampleField(p, waveCount, buffer);
        for (uint j = 0; j < depth; j++)
        {
            const float3 p_next = origin + rayDir * ((t + j) * frequency);
            const float v_next = sampleField(p_next, waveCount, buffer);
            total += smoothstep(min, max, v - v_next) * amplitude;
            v = v_next;
        }
    }
    return total;
}

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

#endif