#ifndef PLATONIC_SOLIDS_INCLUDED
#define PLATONIC_SOLIDS_INCLUDED

// Platonic Solids Enum
enum Solid
{
    TETRAHEDRON = 0,
    OCTAHEDRON = 1,
    CUBE = 2,
    ICOSAHEDRON = 3,
    DODECAHEDRON = 4,
};

// Buffer size constant
#define PLATONIC_BUFFER_SIZE 120

// Solid size constants
#define TETRAHEDRON_SIZE 4
#define OCTAHEDRON_SIZE 6
#define CUBE_SIZE 8
#define ICOSAHEDRON_SIZE 12
#define DODECAHEDRON_SIZE 20

// Solid sizes array
static const uint SOLID_SIZES[5] = {
    4,  // TETRAHEDRON
    6,  // OCTAHEDRON
    8,  // CUBE
    12,  // ICOSAHEDRON
    20,  // DODECAHEDRON
};

// Solid offsets in combined buffer
static const uint SOLID_OFFSETS[5] = {
    0,  // TETRAHEDRON
    4,  // OCTAHEDRON
    10,  // CUBE
    18,  // ICOSAHEDRON
    30,  // DODECAHEDRON
};

// Combined platonic solids buffer with symmetric patterns
static const float3 PLATONIC_SOLIDS[120] = {
    float3(0.35355339, 0.35355339, 0.35355339), // Symmetric pattern 0, index 0
    float3(0.35355339, -0.35355339, -0.35355339), // Symmetric pattern 0, index 1
    float3(-0.35355339, 0.35355339, -0.35355339), // Symmetric pattern 0, index 2
    float3(-0.35355339, -0.35355339, 0.35355339), // Symmetric pattern 0, index 3
    float3(-0.35355339, -0.35355339, 0.35355339), // Symmetric pattern 0, index 4
    float3(-0.35355339, 0.35355339, -0.35355339), // Symmetric pattern 0, index 5
    float3(0.35355339, -0.35355339, -0.35355339), // Symmetric pattern 0, index 6
    float3(0.35355339, 0.35355339, 0.35355339), // Symmetric pattern 0, index 7
    float3(0.35355339, -0.35355339, -0.35355339), // Symmetric pattern 0, index 8
    float3(0.35355339, 0.35355339, 0.35355339), // Symmetric pattern 0, index 9
    float3(-0.35355339, -0.35355339, 0.35355339), // Symmetric pattern 0, index 10
    float3(-0.35355339, 0.35355339, -0.35355339), // Symmetric pattern 0, index 11
    float3(-0.35355339, 0.35355339, -0.35355339), // Symmetric pattern 0, index 12
    float3(-0.35355339, -0.35355339, 0.35355339), // Symmetric pattern 0, index 13
    float3(0.35355339, 0.35355339, 0.35355339), // Symmetric pattern 0, index 14
    float3(0.35355339, -0.35355339, -0.35355339), // Symmetric pattern 0, index 15
    float3(0.35355339, 0.35355339, 0.35355339), // Symmetric pattern 0, index 16
    float3(0.35355339, -0.35355339, -0.35355339), // Symmetric pattern 0, index 17
    float3(-0.35355339, 0.35355339, -0.35355339), // Symmetric pattern 0, index 18
    float3(-0.35355339, -0.35355339, 0.35355339), // Symmetric pattern 0, index 19
    float3(-0.35355339, -0.35355339, 0.35355339), // Symmetric pattern 0, index 20
    float3(-0.35355339, 0.35355339, -0.35355339), // Symmetric pattern 0, index 21
    float3(0.35355339, -0.35355339, -0.35355339), // Symmetric pattern 0, index 22
    float3(0.35355339, 0.35355339, 0.35355339), // Symmetric pattern 0, index 23
    float3(1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 24
    float3(-1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 25
    float3(0.0f, 1.0f, 0.0f), // Symmetric pattern 1, index 26
    float3(0.0f, -1.0f, 0.0f), // Symmetric pattern 1, index 27
    float3(0.0f, 0.0f, 1.0f), // Symmetric pattern 1, index 28
    float3(0.0f, 0.0f, -1.0f), // Symmetric pattern 1, index 29
    float3(0.0f, 0.0f, -1.0f), // Symmetric pattern 1, index 30
    float3(0.0f, 0.0f, 1.0f), // Symmetric pattern 1, index 31
    float3(0.0f, -1.0f, 0.0f), // Symmetric pattern 1, index 32
    float3(0.0f, 1.0f, 0.0f), // Symmetric pattern 1, index 33
    float3(-1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 34
    float3(1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 35
    float3(0.0f, 1.0f, 0.0f), // Symmetric pattern 1, index 36
    float3(-1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 37
    float3(1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 38
    float3(0.0f, 0.0f, -1.0f), // Symmetric pattern 1, index 39
    float3(0.0f, 0.0f, 1.0f), // Symmetric pattern 1, index 40
    float3(0.0f, -1.0f, 0.0f), // Symmetric pattern 1, index 41
    float3(0.0f, -1.0f, 0.0f), // Symmetric pattern 1, index 42
    float3(0.0f, 0.0f, -1.0f), // Symmetric pattern 1, index 43
    float3(0.0f, 0.0f, 1.0f), // Symmetric pattern 1, index 44
    float3(1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 45
    float3(0.0f, 1.0f, 0.0f), // Symmetric pattern 1, index 46
    float3(-1.0f, 0.0f, 0.0f), // Symmetric pattern 1, index 47
    float3(-0.5f, -0.5f, -0.5f), // Symmetric pattern 2, index 48
    float3(0.5f, -0.5f, -0.5f), // Symmetric pattern 2, index 49
    float3(-0.5f, 0.5f, -0.5f), // Symmetric pattern 2, index 50
    float3(0.5f, 0.5f, -0.5f), // Symmetric pattern 2, index 51
    float3(-0.5f, -0.5f, 0.5f), // Symmetric pattern 2, index 52
    float3(0.5f, -0.5f, 0.5f), // Symmetric pattern 2, index 53
    float3(-0.5f, 0.5f, 0.5f), // Symmetric pattern 2, index 54
    float3(0.5f, 0.5f, 0.5f), // Symmetric pattern 2, index 55
    float3(0.5f, 0.5f, 0.5f), // Symmetric pattern 2, index 56
    float3(-0.5f, 0.5f, 0.5f), // Symmetric pattern 2, index 57
    float3(0.5f, -0.5f, 0.5f), // Symmetric pattern 2, index 58
    float3(-0.5f, -0.5f, 0.5f), // Symmetric pattern 2, index 59
    float3(0.5f, 0.5f, -0.5f), // Symmetric pattern 2, index 60
    float3(-0.5f, 0.5f, -0.5f), // Symmetric pattern 2, index 61
    float3(0.5f, -0.5f, -0.5f), // Symmetric pattern 2, index 62
    float3(-0.5f, -0.5f, -0.5f), // Symmetric pattern 2, index 63
    float3(0.5f, 0.5f, -0.5f), // Symmetric pattern 2, index 64
    float3(-0.5f, 0.5f, -0.5f), // Symmetric pattern 2, index 65
    float3(0.5f, -0.5f, -0.5f), // Symmetric pattern 2, index 66
    float3(-0.5f, -0.5f, -0.5f), // Symmetric pattern 2, index 67
    float3(0.5f, 0.5f, 0.5f), // Symmetric pattern 2, index 68
    float3(-0.5f, 0.5f, 0.5f), // Symmetric pattern 2, index 69
    float3(0.5f, -0.5f, 0.5f), // Symmetric pattern 2, index 70
    float3(-0.5f, -0.5f, 0.5f), // Symmetric pattern 2, index 71
    float3(0.000000, 0.525731, 0.850651), // Symmetric pattern 3, index 72
    float3(0.000000, -0.525731, 0.850651), // Symmetric pattern 3, index 73
    float3(0.000000, 0.525731, -0.850651), // Symmetric pattern 3, index 74
    float3(0.000000, -0.525731, -0.850651), // Symmetric pattern 3, index 75
    float3(0.525731, 0.850651, 0.000000), // Symmetric pattern 3, index 76
    float3(-0.525731, 0.850651, 0.000000), // Symmetric pattern 3, index 77
    float3(0.525731, -0.850651, 0.000000), // Symmetric pattern 3, index 78
    float3(-0.525731, -0.850651, 0.000000), // Symmetric pattern 3, index 79
    float3(0.850651, 0.000000, 0.525731), // Symmetric pattern 3, index 80
    float3(0.850651, 0.000000, -0.525731), // Symmetric pattern 3, index 81
    float3(-0.850651, 0.000000, 0.525731), // Symmetric pattern 3, index 82
    float3(-0.850651, 0.000000, -0.525731), // Symmetric pattern 3, index 83
    float3(-0.850651, 0.000000, -0.525731), // Symmetric pattern 3, index 84
    float3(-0.850651, 0.000000, 0.525731), // Symmetric pattern 3, index 85
    float3(0.850651, 0.000000, -0.525731), // Symmetric pattern 3, index 86
    float3(0.850651, 0.000000, 0.525731), // Symmetric pattern 3, index 87
    float3(-0.525731, -0.850651, 0.000000), // Symmetric pattern 3, index 88
    float3(0.525731, -0.850651, 0.000000), // Symmetric pattern 3, index 89
    float3(-0.525731, 0.850651, 0.000000), // Symmetric pattern 3, index 90
    float3(0.525731, 0.850651, 0.000000), // Symmetric pattern 3, index 91
    float3(0.000000, -0.525731, -0.850651), // Symmetric pattern 3, index 92
    float3(0.000000, 0.525731, -0.850651), // Symmetric pattern 3, index 93
    float3(0.000000, -0.525731, 0.850651), // Symmetric pattern 3, index 94
    float3(0.000000, 0.525731, 0.850651), // Symmetric pattern 3, index 95
    float3(0.577350, 0.577350, 0.577350), // Symmetric pattern 4, index 96
    float3(0.577350, 0.577350, -0.577350), // Symmetric pattern 4, index 97
    float3(0.577350, -0.577350, 0.577350), // Symmetric pattern 4, index 98
    float3(0.577350, -0.577350, -0.577350), // Symmetric pattern 4, index 99
    float3(-0.577350, 0.577350, 0.577350), // Symmetric pattern 4, index 100
    float3(-0.577350, 0.577350, -0.577350), // Symmetric pattern 4, index 101
    float3(-0.577350, -0.577350, 0.577350), // Symmetric pattern 4, index 102
    float3(-0.577350, -0.577350, -0.577350), // Symmetric pattern 4, index 103
    float3(0.000000, 0.356822, 0.934172), // Symmetric pattern 4, index 104
    float3(0.000000, -0.356822, 0.934172), // Symmetric pattern 4, index 105
    float3(0.000000, 0.356822, -0.934172), // Symmetric pattern 4, index 106
    float3(0.000000, -0.356822, -0.934172), // Symmetric pattern 4, index 107
    float3(0.934172, 0.000000, 0.356822), // Symmetric pattern 4, index 108
    float3(-0.934172, 0.000000, 0.356822), // Symmetric pattern 4, index 109
    float3(0.934172, 0.000000, -0.356822), // Symmetric pattern 4, index 110
    float3(-0.934172, 0.000000, -0.356822), // Symmetric pattern 4, index 111
    float3(0.356822, 0.934172, 0.000000), // Symmetric pattern 4, index 112
    float3(-0.356822, 0.934172, 0.000000), // Symmetric pattern 4, index 113
    float3(0.356822, -0.934172, 0.000000), // Symmetric pattern 4, index 114
    float3(-0.356822, -0.934172, 0.000000), // Symmetric pattern 4, index 115
    float3(-0.356822, -0.934172, 0.000000), // Symmetric pattern 4, index 116
    float3(0.356822, -0.934172, 0.000000), // Symmetric pattern 4, index 117
    float3(-0.356822, 0.934172, 0.000000), // Symmetric pattern 4, index 118
    float3(0.356822, 0.934172, 0.000000) // Symmetric pattern 4, index 119
};

// Utility function to get a vertex from any solid
float3 GetPlatonicVertex(uint solidType, uint vertexIndex)
{
    // Get the size and base offset for this solid
    uint solidSize = SOLID_SIZES[solidType];
    uint baseOffset = SOLID_OFFSETS[solidType];

    // Calculate the index with wraparound
    uint actualIndex = baseOffset + (vertexIndex % solidSize);

    return PLATONIC_SOLIDS[actualIndex];
}

// Individual solid access functions
float3 GetTETRAHEDRONVertex(uint vertexIndex)
{
    return GetPlatonicVertex(0, vertexIndex);
}

float3 GetOCTAHEDRONVertex(uint vertexIndex)
{
    return GetPlatonicVertex(1, vertexIndex);
}

float3 GetCUBEVertex(uint vertexIndex)
{
    return GetPlatonicVertex(2, vertexIndex);
}

float3 GetICOSAHEDRONVertex(uint vertexIndex)
{
    return GetPlatonicVertex(3, vertexIndex);
}

float3 GetDODECAHEDRONVertex(uint vertexIndex)
{
    return GetPlatonicVertex(4, vertexIndex);
}

#endif // PLATONIC_SOLIDS_INCLUDED
