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
    int frequency;
    float amplitude;
    float phase;
    float radius;
    float density;
    float scale;
};

struct Operation
{
    uint type;
    float4x4 transform;
};
 
struct Particle 
{
    float3 position;
    float3 oldPosition;
    float charge;
    float size;
    float mass;
};

struct Modulation
{
    float time;
    float frequency;
    float phase;
    float amplitude;
};

static const Modulation DEFAULT_MOD = (Modulation) 0; 

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

#define MaxIterations 1

float solveKepler(float meanAnomaly, float eccentricity)
{
    float E = meanAnomaly + eccentricity * sin(meanAnomaly);
    return E;
}

float ellipticalSine(float time, float eccentricity)
{
    const float eccentricAnomaly = solveKepler(time, eccentricity);
    return sqrt(1 - eccentricity * eccentricity) * sin(eccentricAnomaly);
}

float triWave(float freq, float dutyCycle, float amplitude)
{
    float t = (freq) % 1.0f;

    float value;
        
    if (t < dutyCycle)
    {
        value = -amplitude + (2 * amplitude * t / dutyCycle);
    }
    else
    {
        float fallT = (t - dutyCycle) / (1 - dutyCycle);
        value = amplitude - (2 * amplitude * fallT);
    }
        
    return value;
}

float squareWave(float frequency, float dutyCycle, float amplitude, float smoothing)
{
        float t = (frequency) % 1.0f;
    
        if (smoothing <= 0)
        {
            return (t < dutyCycle ? amplitude : -amplitude);
        }
    
        float value;
    
        float distFromDutyTransition = min(
            abs(t - dutyCycle),
            abs(t - dutyCycle - 1)
        );
        
        float distFromZeroTransition = min(
            t,
            1.0 - t
        );
    
        float nearestTransition = min(distFromDutyTransition, distFromZeroTransition);
    
        float smoothRange = smoothing;
        
        if (nearestTransition < smoothRange)
        {
            float ratio = nearestTransition / smoothRange;
            
            if (t < dutyCycle && t > 0 && t < 1.0 - dutyCycle)
                value = lerp(0, amplitude, ratio);
            else if (t > dutyCycle && t < 1.0)
                value = lerp(0, -amplitude, ratio);
            else if (t < dutyCycle)
                value = amplitude;
            else
                value = -amplitude;
        }
        else
        {
            value = (t < dutyCycle ? amplitude : -amplitude);
        }
        
        return value;
    
}

float map(float value, float min1, float max1, float min2, float max2)
{
     const float perc = (value - min1) / (max1 - min1);
    return  perc * (max2 - min2) + min2;
}

float sampleCube(const float3 pos, Photon photon)
{
    float d = 0.0;
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int z = -1; z <= 1; z++)
            {
                const float3 p = float3(x,y,z);
                const float dist = saturate(distance(pos * photon.scale, p) * photon.radius);
                d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
            }
        }
    }
    return d;
}

float sampleMatrix(const float3 pos, Photon photon, const float4x4 transform)
{
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
   
    float rad = photon.radius;
  
    for (int x = -1; x <= 1; x++)
    {
       
        for (int y = -1; y <= 1; y++)
        {
          
            for (int z = -1; z <= 1; z++)
            {
                const float3 p = float3(x,y,z);
                float3 transformed = p;
                float dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
                d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
            }
        }
    }
    return d;
}



float sampleDragonCurve3D(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    // Simplified 3D dragon curve using Lindenmayer system approach
    const int iterations = 4;
    float3 points[16];
    int pointCount = 0;
    
    // Start with simple L-shape
    points[pointCount++] = float3(0, 0, 0);
    points[pointCount++] = float3(1, 0, 0);
    points[pointCount++] = float3(1, 1, 0);
    
    // Dragon curve iterations in 3D
    for (int iter = 0; iter < iterations && pointCount < 16; iter++) {
        float scale = pow(0.7, iter);
        float3 center = float3(0.5, 0.5, iter * 0.2);
        
        // Add spiral component
        for (int i = 0; i < 3 && pointCount < 16; i++) {
            float angle = float(i + iter * 3) * 0.785; // 45 degrees
            float3 spiral = float3(
                cos(angle) * scale,
                sin(angle) * scale,
                iter * 0.2
            );
            points[pointCount++] = center + spiral;
        }
    }
    
    for (int i = 0; i < pointCount; i++) {
        float dist = saturate(distance(pos * photon.scale, points[i] + offset * photon.scale) * rad);
        d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    }
    return d;
}

float sampleFractalTree(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    // 3-level branching tree
    float3 branches[26];
    int branchCount = 0;
    
    // Trunk
    branches[branchCount++] = float3(0, 0, 0);
    
    // Level 1 branches
    float3 directions[8] = {
        float3(0.707, 0, 0.707),   float3(-0.707, 0, 0.707),
        float3(0, 0.707, 0.707),   float3(0, -0.707, 0.707),
        float3(0.5, 0.5, 0.707),   float3(-0.5, 0.5, 0.707),
        float3(0.5, -0.5, 0.707),  float3(-0.5, -0.5, 0.707)
    };
    
    for (int i = 0; i < 8 && branchCount < 26; i++) {
        branches[branchCount++] = directions[i] * 0.6;
        
        // Level 2 sub-branches
        for (int j = 0; j < 2 && branchCount < 26; j++) {
            float angle = float(j) * 1.047; // 60 degrees
            float3 rotated = float3(
                directions[i].x * cos(angle) - directions[i].y * sin(angle),
                directions[i].x * sin(angle) + directions[i].y * cos(angle),
                directions[i].z
            );
            branches[branchCount++] = directions[i] * 0.6 + rotated * 0.3;
        }
    }
    
    for (int i = 0; i < branchCount; i++) {
        float dist = saturate(distance(pos * photon.scale, branches[i] + offset * photon.scale) * rad);
        float branchWeight = 1.0 / (1.0 + float(i / 8)); // Smaller branches have less weight
        d += sin(dist * photon.frequency + photon.phase) * photon.amplitude * branchWeight;
    }
    return d;
}

float sampleApollonian(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    // Base configuration: 4 mutually tangent spheres
    float4 spheres[12] = {
        // Format: (x, y, z, radius)
        float4(0, 0, 0.5, 0.5),           // Top
        float4(0, 0, -0.5, 0.5),          // Bottom
        float4(0.866, 0, 0, 0.5),         // Right
        float4(-0.433, 0.75, 0, 0.5),     // Left front
        float4(-0.433, -0.75, 0, 0.5),    // Left back
        
        // Smaller spheres (next iteration)
        float4(0.289, 0.25, 0.25, 0.167),
        float4(0.289, -0.25, 0.25, 0.167),
        float4(-0.144, 0.25, 0.25, 0.167),
        float4(-0.144, -0.25, 0.25, 0.167),
        float4(0.289, 0.25, -0.25, 0.167),
        float4(0.289, -0.25, -0.25, 0.167),
        float4(-0.144, 0, 0, 0.167)
    };
    
    for (int i = 0; i < 12; i++) {
        float3 p = spheres[i].xyz;
        float sphereRad = spheres[i].w;
        float dist = saturate(distance(pos * photon.scale, p + offset * photon.scale) * rad);
        d += sin(dist * photon.frequency + photon.phase) * photon.amplitude * sphereRad;
    }
    return d;
}

float sampleMandelbulb(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    const int numSamples = 8;
    const float power = 8.0;
    
    for (int i = 0; i < numSamples; i++) {
        float theta = (float(i) / float(numSamples)) * 2.0 * 3.14159;
        
        // Sample points around mandelbulb surface
        for (int j = 0; j < numSamples; j++) {
            float phi = (float(j) / float(numSamples)) * 3.14159;
            
            float r = 0.8; // Approximate surface distance
            float x = r * sin(phi) * cos(theta);
            float y = r * sin(phi) * sin(theta);
            float z = r * cos(phi);
            
            float3 p = float3(x, y, z);
            float dist = saturate(distance(pos * photon.scale, p + offset * photon.scale) * rad);
            d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
        }
    }
    return d;
}

float sampleMengerSponge(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    const int iterations = 3;
    static float scales[4] = {1.0, 0.333, 0.111, 0.037};
    
    for (int iter = 0; iter < iterations + 1; iter++) {
        float scale = scales[iter];
        
        // Generate 20 points (27 - 7 removed middle points)
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                for (int z = -1; z <= 1; z++) {
                    // Skip the middle cross pattern
                    int centerCount = (x == 0 ? 1 : 0) + (y == 0 ? 1 : 0) + (z == 0 ? 1 : 0);
                    if (centerCount >= 2) continue;
                    
                    float3 p = float3(x, y, z) * scale;
                    float dist = saturate(distance(pos * photon.scale, p + offset * photon.scale) * rad);
                    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude * scale;
                }
            }
        }
    }
    return d;
}

float sampleSierpinski(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    // Base tetrahedron vertices
    float3 tetraVertices[4] = {
        float3(1, 1, 1),
        float3(1, -1, -1),
        float3(-1, 1, -1),
        float3(-1, -1, 1)
    };
    
    // Multiple scales for fractal effect
    float scales[3] = {1.0, 0.5, 0.25};
    
    for (int scale = 0; scale < 3; scale++) {
        for (int v = 0; v < 4; v++) {
            float3 p = tetraVertices[v] * scales[scale];
            float dist = saturate(distance(pos * photon.scale, p + offset * photon.scale) * rad);
            d += sin(dist * photon.frequency + photon.phase) * photon.amplitude * scales[scale];
        }
    }
    return d;
}

float sampleDoubleHelix(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    const int numTurns = photon.iterations;
    const int pointsPerTurn = 8;
    const int totalPoints = numTurns * pointsPerTurn;
    
    for (int i = 0; i < totalPoints; i++) {
        float t = float(i) / float(totalPoints - 1);
        float angle = t * 2.0 * 3.14159 * numTurns;
        float y = (t - 0.5) * photon.density; // -1 to 1
        
        // First helix
        float3 point1 = float3(cos(angle) * 0.5, y, sin(angle) * 0.5);
        // Second helix (180° offset)
        float3 point2 = float3(cos(angle + 3.14159) * 0.5, y, sin(angle + 3.14159) * 0.5);
        
        float dist1 = saturate(distance(pos * photon.scale, point1 + offset * photon.scale) * rad);
        float dist2 = saturate(distance(pos * photon.scale, point2 + offset * photon.scale) * rad);
        
        d += sin(dist1 * photon.frequency + photon.phase) * photon.amplitude;
        d += sin(dist2 * photon.frequency + photon.phase) * photon.amplitude;
    }
    return d;
}

float sampleFibonacci(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    const float goldenAngle = 2.399963; // 2π * (1 - 1/φ)
    const int numPoints = 21; // Fibonacci number
    
    for (int i = 0; i < numPoints; i++) {
        float y = 1.0 - (i / float(numPoints - 1)) * 2.0; // y from 1 to -1
        float radius = sqrt(1.0 - y * y);
        float theta = goldenAngle * i;
        
        float3 p = float3(cos(theta) * radius, y, sin(theta) * radius);
        float3 transformed = p;
        float dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
        d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    }
    return d;
}


float sampleBCC(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    // BCC: 8 corners + 1 center
    float3 points[9] = {
        float3(-1,-1,-1), float3(1,-1,-1), float3(-1,1,-1), float3(1,1,-1),
        float3(-1,-1,1), float3(1,-1,1), float3(-1,1,1), float3(1,1,1),
        float3(0,0,0) // Body center
    };
    
    for (int i = 0; i < 9; i++) {
        float3 transformed = points[i];
        float dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
        d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    }
    return d;
}

float sampleFCC(const float3 pos, Photon photon, const float4x4 transform) {
    float d = 0.0;
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float rad = photon.radius;
    
    // FCC lattice points: 8 corners + 6 face centers
    float3 points[14] = {
        // Corners
        float3(-1,-1,-1), float3(1,-1,-1), float3(-1,1,-1), float3(1,1,-1),
        float3(-1,-1,1), float3(1,-1,1), float3(-1,1,1), float3(1,1,1),
        // Face centers
        float3(0,-1,0), float3(0,1,0), float3(-1,0,0), 
        float3(1,0,0), float3(0,0,-1), float3(0,0,1)
    };
    
    for (int i = 0; i < 14; i++) {
        float3 transformed = points[i];
        float dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
        d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    }
    return d;
}

float sampleOctahedron(const float3 pos, Photon photon, const float4x4 transform)
{
    float d = 0.0;
    const float3 p0 = float3(1.0f, 0.0f, 0.0f);
    const float3 p1 = float3(-1.0f, 0.0f, 0.0f);
    const float3 p2 = float3(0.0f, 1.0f, 0.0f);
    const float3 p3 = float3(0.0f, -1.0f, 0.0f);
    const float3 p4 = float3(0.0f, 0.0f, 1.0f);
    const float3 p5 = float3(0.0f, 0.0f, -1.0f);

    
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float3 transformed = p0;
    float rad = photon.radius;
    
    float dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;

    transformed = p1;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    
    transformed = p2;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;

    transformed = p3;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;

    transformed = p4;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;

    transformed = p5;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    
    return d;
}

float sampleTetrahedral(const float3 pos, Photon photon, const float4x4 transform)
{
    float d = 0.0;
    const float3 p0 = float3(0.35355339, 0.35355339, 0.35355339);
    const float3 p1 = float3(0.35355339, -0.35355339, -0.35355339);
    const float3 p2 = float3(-0.35355339, 0.35355339, -0.35355339);
    const float3 p3 = float3(-0.35355339, -0.35355339, 0.35355339);
    
    float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    float3 transformed = p0;
    float rad = photon.radius;
    
    float dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;

    transformed = p1;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    
    transformed = p2;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;

    transformed = p3;
    dist = saturate(distance(pos * photon.scale, transformed + offset * photon.scale) * rad);
    d += sin(dist * photon.frequency + photon.phase) * photon.amplitude;
    
    return d;
}


float sampleField(const float3 pos, const float3 vertex,
    const Photon photon, const float4x4 transform, Modulation m = DEFAULT_MOD)
{
 
    const float3 offset = float3(transform[0][3], transform[1][3], transform[2][3]);
    const float3 transformed = mul(vertex, transform).xyz + (offset * photon.scale);
    const float dist = saturate(distance(pos * photon.scale, transformed) * (1.0 / max(0.01, photon.radius * PI)));
    const float freq = photon.frequency;
    const float phase = photon.phase;
    const float amp = photon.amplitude;
    return sin((dist * freq + phase)) * amp ;
}

float3 sampleFieldNormal(const float3 pos, const float3 vertex,
    const Photon photon, const uint size, Modulation m = DEFAULT_MOD)
{
    const float3 offset = float3(photon.transform[0][3], photon.transform[1][3], photon.transform[2][3]);
    const float3 transformed = mul(vertex, photon.transform).xyz + offset;
    const float dist = saturate(distance(pos * photon.scale, transformed) * (1.0 / max(0.01, photon.radius * PI)));
    const float freq = photon.frequency + (m.time * m.frequency);
    const float phase = photon.phase + m.time * m.phase;
    const float amp = photon.amplitude + sin(m.amplitude * m.time) * (1.0 / size);
    return sin((dist * freq + phase)) * amp ;
}

float sampleField(const float3 pos, float4x4 transform, const Photon photon, Modulation m = DEFAULT_MOD)
{
    float density = 0.0;
    const uint size = getPlatonicSize(photon.type);
    
    for (uint j = 1; j <= photon.iterations; ++j)
    {
        for (uint i = 0; i < size; i++)
        {
            const float3 vertex = getPlatonicVertex(photon.type, i);
            density += sampleField(pos, vertex * (1 + (j * photon.density)), photon, transform, m);
        }
    }
    return density; 
}



float sampleField(float3 pos, float4x4 transform, Modulation m = DEFAULT_MOD)
{
    float density = 0.0;
    for (uint i = 0; i < _PhotonCount; i++)
    {
        const Photon photon = _PhotonBuffer[i]; 
        density += sampleField(pos, transform, photon,  m);
    }
    
    return density; 
}

float sampleField(float3 pos, Modulation m = DEFAULT_MOD)
{
    float4x4 i = float4x4(
        1,0,0,0,
        0,1,0,0,
        0,0,1,0,
        0,0,0,1);
    return sampleField(pos, i, m);
}

float sampleMandelbrot(float3 pos, float2 plane_coord, uint iterations,
    uint orbitPlaneCount, Modulation m = DEFAULT_MOD)
{
    float x = 0;
    float y = 0;
    int step = 0;
    const int count = max(1, orbitPlaneCount);
    float value = 0.0;
    for (int i = 0; i < _PhotonCount; ++i)
    {
        const Photon photon = _PhotonBuffer[i];
             
        while (step < iterations)
        {
            const float x_temp = x * x - y * y + plane_coord.x;
            y = 2 * x * y + plane_coord.y; 
            x = x_temp;
            step++;
            for (int plane = 0; plane < count; ++plane)
            {
                const float angle = ((2 * PI) / count) * plane;
                const float3 axis = float3(1, 0, 0);
                const float3 offsetDir = float3(0, cos(angle), sin(angle));
                const float3 orbitPos = float3(x, 0, y);
                const float3 rotatedPos = orbitPos.x * axis + orbitPos.y * offsetDir + orbitPos.z * cross(
                    axis, offsetDir);
                value += sampleField(pos, rotatedPos * photon.density, photon, count, m);
            }
        }
    }
    return value;
}

float rayMarchField(float3 origin, float4x4 transform, float3 dir, float length, uint steps, float min, float max,
    float depth, float frequency, float amplitude, Modulation m = DEFAULT_MOD)
{
    const float step_size = length / steps;
    float total_density = 0.0;
    for (uint i = 0; i < steps; ++i)
    {
        const float3 target = origin + dir * i * step_size  * depth;
        const float density = sampleField(target, transform, m);
        total_density += sin(smoothstep(min, max, density * frequency)  * amplitude);
    }
    return total_density;
}

float rayMarchField(float3 origin, float3 dir, float length, uint steps, float min, float max,
    float depth, float frequency, float amplitude, Modulation m = DEFAULT_MOD)
{
    float4x4 i = float4x4(
        1,0,0,0,
        0,1,0,0,
        0,0,1,0,
        0,0,0,1);
    return rayMarchField(origin, i, dir, length, steps, min, max, depth, frequency, amplitude, m);   
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