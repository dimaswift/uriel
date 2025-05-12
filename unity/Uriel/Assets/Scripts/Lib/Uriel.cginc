#ifndef URIEL_INCLUDED
#define URIEL_INCLUDED

#include "PlatonicSolids.cginc"

#define PI 3.14159265359
#define PHI 1.618033988749895

struct Photon 
{
    float4x4 transform;
    uint iterations;
    uint type;
    float frequency;
    float amplitude;
    float phase;
    float radius;
    float density;
};

struct Particle 
{
    float3 position;
    float3 velocity;
    float charge;
    float size;
    float mass;
};

uint _PhotonCount;
StructuredBuffer<Photon> _PhotonBuffer;

float3x3 createRotationMatrix(float latitudeDegrees, float longitudeDegrees);

float3 rotatePointByLatLong(const float3 p, float latitude_degrees, const float longitude_degrees);

Photon createPhoton(uint type, float4x4 transform, uint iterations, float frequency,   
                float amplitude, float density, float phase, float radius)  
{  
    Photon w;  
    w.iterations = iterations;  
    w.frequency = frequency;  
    w.amplitude = amplitude;
    w.phase = phase;
    w.density = density;  
    w.radius  = radius;
    w.type = type;
    w.transform = transform;
    return w;  
}  

float sampleField(const float3 pos, const float3 vertex, const Photon photon, const uint size)
{
    const float3 offset = float3(photon.transform[0][3], photon.transform[1][3], photon.transform[2][3]);
    const float3 transformed = mul(vertex, photon.transform).xyz + offset;
    const float dist = saturate(distance(pos, transformed) * (1.0 / max(1, photon.radius * PI)));
    return sin(dist * (photon.frequency) + (photon.phase) * photon.transform[3][3]) * photon.amplitude * (1.0 / size); 
}


float sampleField(const float3 pos, const Photon photon)
{
    float density = 0.0;
    const uint size = getPlatonicSize(photon.type);
    
    for (uint j = 1; j <= photon.iterations; ++j)
    {
        for (uint i = 0; i < size; i++)
        {
            const float3 vertex = getPlatonicVertex(photon.type, i);
            density += sampleField(pos, vertex * (1 + (j * photon.density)), photon, size);
        }
    }
    return density; 
}

float sampleField(float3 pos)
{
    float density = 0.0;
    for (uint i = 0; i < _PhotonCount; i++)
    {
        const Photon photon = _PhotonBuffer[i]; 
        density += sampleField(pos, photon);
    }
    return density; 
}

float rayMarchFieldCycle(float3 origin, float length, uint steps, float depth, float frequency)
{
    const float step_size = length / steps;
    const float3 dir = normalize(origin);
    float total_density = 0.0;
    for (uint i = 0; i < steps; ++i)
    {
        const float3 target = origin + dir * i * step_size * depth;
        const float density = sampleField(target);
        total_density += sin(density * frequency * 0.01);
    }
    return total_density;
}

float rayMarchField(float3 origin, float3 dir, float length, uint steps, float min, float max,
    float depth, float frequency, float amplitude)
{
    const float step_size = length / steps;
    float total_density = 0.0;
    for (uint i = 0; i < steps; ++i)
    {
        const float3 target = origin + dir * i * step_size  * depth;
        const float density = sampleField(target);
        total_density += sin(smoothstep(min, max, density * frequency)  * amplitude);
    }
    return total_density;
}

float3x3 createRotationMatrix(float latitudeDegrees, float longitudeDegrees)  
{  
    const float lat_rad = radians(latitudeDegrees);  
    const float long_rad = radians(longitudeDegrees);  

    const float cos_long = cos(long_rad);  
    const float sin_long = sin(long_rad);  
 
    const float cosLat = cos(lat_rad);  
    const float sinLat = sin(lat_rad);  

    const float3x3 long_rotation = float3x3(  
        cos_long, 0, sin_long,  
        0, 1, 0,  
        -sin_long, 0, cos_long  
    );  

    const float3x3 lat_rotation = float3x3(  
        1, 0, 0,  
        0, cosLat, -sinLat,  
        0, sinLat, cosLat  
    );  

    return mul(lat_rotation, long_rotation);  
}  

float3 rotatePointByLatLong(const float3 p, float latitude_degrees, const float longitude_degrees)
{  
    const float3x3 rotation_matrix = createRotationMatrix(latitude_degrees, longitude_degrees);  
    return mul(rotation_matrix, p);  
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