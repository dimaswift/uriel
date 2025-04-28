#ifndef URIEL
#define URIEL

#define PI 3.14159265359

struct Gene 
{
    uint iterations;
    float frequency;
    float amplitude;
    float3 source;
    float scale;
    float phase;
    int harmonic;
}; 
 
float sampleField(float3 pos, uint geneCount, StructuredBuffer<Gene> buffer)
{
    float result = 0.0;
    for (uint i = 0; i < geneCount; i++)
    {
        const Gene gene = buffer[i]; 
        const float dist = saturate(distance(pos, gene.source) * gene.scale);
        for (uint j = 0; j < gene.iterations; j++)
        {
            float d = dist * gene.frequency;
            
            result += j % 2 ==0 ? sin(d) * gene.amplitude : cos(d)  * gene.amplitude;
        }
      
    }
    
    return result; 
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