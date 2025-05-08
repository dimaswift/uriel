#ifndef URIEL
#define URIEL

#include "PlatonicSolids.cginc"

#define PI 3.14159265359
#define PHI 1.618033988749895

struct Photon 
{
    float3 source;
    float2 rotation;
    uint iterations;
    uint type;
    float frequency;
    float amplitude;
    float density;
    float phase;
    float depth;
};

struct Particle 
{
    float3 position;
    float3 velocity;
    float charge;
    float size;
    float mass;
};

float3x3 createRotationMatrix(float latitudeDegrees, float longitudeDegrees);

float3 rotatePointByLatLong(const float3 p, float latitude_degrees, const float longitude_degrees);

Photon createPhoton(uint type, float3 source, float2 coordinates, uint iterations, float frequency,   
                float amplitude, float density, float phase, float depth)  
{  
    Photon w;  
    w.source = source;  
    w.iterations = iterations;  
    w.frequency = frequency;  
    w.amplitude = amplitude;  
    w.density = density;  
    w.phase = phase;
    w.type = type;
    w.rotation = coordinates;
    w.depth = depth;
    return w;  
}  

float sampleField(const float3 pos, const float3 normal, int depth, const float3 vertex, const Photon photon, const uint size)
{
    const float3 rotatedVertex = rotatePointByLatLong(vertex, photon.rotation.x, photon.rotation.y);
    const float dist = saturate(distance(pos + normal * photon.depth * depth, rotatedVertex + photon.source) * photon.density / PI);
    return sin(dist * photon.frequency + photon.phase ) * (photon.amplitude + (1.0 / size)); 
}

float sampleField(const float3 pos, const float3 normal, const Photon photon)
{
    float density = 0.0;
    const uint size = getPlatonicSize(photon.type);
    
    for (uint j = 1; j <= photon.iterations; ++j)
    {
        for (uint i = 0; i < size; i++)
        {
            const float3 vertex = getPlatonicVertex(photon.type, i);
            density += sampleField(pos, normal, j, vertex, photon, size);
        }
    }
    return density; 
}

float sampleField(float3 pos, const float3 normal, uint count, StructuredBuffer<Photon> buffer)
{
    float density = 0.0;
    for (uint i = 0; i < count; i++)
    {
        const Photon photon = buffer[i]; 
        density += sampleField(pos, normal, photon);
    }
    return density; 
}

float rayMarchField(float3 origin, float3 target, uint steps, uint depth, float frequency,
    float min, float max, float amplitude, uint photonCount, StructuredBuffer<Photon> buffer)
{
    const float3 dir = target - origin;
    const float rayLength = length(dir);
    const float stepSize = rayLength / steps;
    const float3 rayDir = normalize(dir);
    float total = 0.0;
    for (uint i = 0; i < steps; i++)  
    {  
        const float t = i * stepSize;  
        const float3 p_next = origin + (rayDir * t) * frequency / rayLength ;
        const float v_next = sampleField(p_next, rayDir, photonCount, buffer);
        total += smoothstep(min, max, v_next) * amplitude;
    }
    return total;
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