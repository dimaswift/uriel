#ifndef URIEL
#define URIEL

#define PI 3.14159265359
#define PHI 1.618033988749895
#define TETRAHEDRON_SIZE 4
#define OCTAHEDRON_SIZE 6
#define CUBE_SIZE 8
#define ICOSAHEDRON_SIZE 12
#define DODECAHEDRON_SIZE 20

#define BUFFER_SIZE DODECAHEDRON_SIZE
#include "PlatonicSolids.cginc"

struct Wave 
{
    float3 source;
    float2 rotation;
    uint ripples;
    uint harmonic;
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

Wave createWave(uint type, float3 source, float2 coordinates, uint ripples, uint harmonic, float frequency,   
                float amplitude, float density, float phase, float depth)  
{  
    Wave w;  
    w.source = source;  
    w.ripples = ripples;   
    w.harmonic = harmonic;  
    w.frequency = frequency;  
    w.amplitude = amplitude;  
    w.density = density;  
    w.phase = phase;
    w.type = type;
    w.rotation = coordinates;
    w.depth = depth;
    return w;  
}  

float sampleShape(const float3 pos, const float3 normal, const Wave wave)
{
    float result = 0.0;
    for (uint i = 0; i < 4; i++)
    {
        const float3 vertex = getPlatonicVertex(wave.type, wave.harmonic, i);
        const float3 rotatedVertex = rotatePointByLatLong(vertex, wave.rotation.x, wave.rotation.y);
        const float dist = saturate(distance(pos, rotatedVertex + wave.source) * wave.density);
        result += sin(dist * wave.frequency + wave.phase) * wave.amplitude; 
    }
    return result; 
}

float sampleField(float3 pos, uint count, StructuredBuffer<Wave> buffer)
{
    float result = 0.0;
    for (uint i = 0; i < count; i++)
    {
        const Wave wave = buffer[i]; 
        const float dist = saturate(distance(pos, wave.source) * wave.density);
        for (int k = 0; k < 3; k++)
        {
            result += sin(dist * i);
        }
       
        for (uint j = 0; j < wave.ripples; j++)
        {
            const float d = dist * (wave.frequency / wave.ripples);
            result += sin(d * (float(j) / wave.ripples)) * wave.amplitude;
            for (uint h = 0; h < wave.harmonic; h++)
            {
                result += saturate(sin(d * (float(h) / wave.harmonic))) * wave.amplitude;
            }
            for (uint h1 = 0; h1 < wave.harmonic; h1++)
            {
                result += cos(sqrt(d) * wave.frequency * 0.5) * wave.amplitude;
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