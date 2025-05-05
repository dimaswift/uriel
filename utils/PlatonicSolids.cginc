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

// Unified sequence struct for all platonic solids
struct PlatonicSequence
{
    uint indices[120]; // Perfect cycle length of 120 (LCM of all solid sizes)
};

// Buffer size constants
#define SEQUENCE_COUNT {SEQUENCE_COUNT}
#define SEQUENCE_BUFFER_SIZE 120
#define TOTAL_VERTICES 50

// Perfect cycles information (120 is the LCM of 4,6,8,12,20):
// TETRAHEDRON: 30 complete cycles in buffer (4 vertices)
// OCTAHEDRON: 20 complete cycles in buffer (6 vertices)
// CUBE: 15 complete cycles in buffer (8 vertices)
// ICOSAHEDRON: 10 complete cycles in buffer (12 vertices)
// DODECAHEDRON: 6 complete cycles in buffer (20 vertices)

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

// Vertex offsets for each solid in the unified vertex buffer
static const uint VERTEX_OFFSETS[5] = {
    0,  // TETRAHEDRON vertices start offset
    4,  // OCTAHEDRON vertices start offset
    10,  // CUBE vertices start offset
    18,  // ICOSAHEDRON vertices start offset
    30,  // DODECAHEDRON vertices start offset
};

// Sequence offset for each solid in the combined sequence buffer
static const uint SEQUENCE_OFFSETS[5] = {
    0,  // TETRAHEDRON sequences start offset
    6,  // OCTAHEDRON sequences start offset
    12,  // CUBE sequences start offset
    18,  // ICOSAHEDRON sequences start offset
    24,  // DODECAHEDRON sequences start offset
};

// Unified buffer containing vertices for all platonic solids
static const float3 PLATONIC_VERTICES[50] = {
    // TETRAHEDRON vertices (offset 0)
    float3(0.35355339, 0.35355339, 0.35355339),  // 0
    float3(0.35355339, -0.35355339, -0.35355339),  // 1
    float3(-0.35355339, 0.35355339, -0.35355339),  // 2
    float3(-0.35355339, -0.35355339, 0.35355339),  // 3
    // OCTAHEDRON vertices (offset 4)
    float3(1.0f, 0.0f, 0.0f),  // 4
    float3(-1.0f, 0.0f, 0.0f),  // 5
    float3(0.0f, 1.0f, 0.0f),  // 6
    float3(0.0f, -1.0f, 0.0f),  // 7
    float3(0.0f, 0.0f, 1.0f),  // 8
    float3(0.0f, 0.0f, -1.0f),  // 9
    // CUBE vertices (offset 10)
    float3(-0.5f, -0.5f, -0.5f),  // 10
    float3(0.5f, -0.5f, -0.5f),  // 11
    float3(-0.5f, 0.5f, -0.5f),  // 12
    float3(0.5f, 0.5f, -0.5f),  // 13
    float3(-0.5f, -0.5f, 0.5f),  // 14
    float3(0.5f, -0.5f, 0.5f),  // 15
    float3(-0.5f, 0.5f, 0.5f),  // 16
    float3(0.5f, 0.5f, 0.5f),  // 17
    // ICOSAHEDRON vertices (offset 18)
    float3(0.000000, 0.525731, 0.850651),  // 18
    float3(0.000000, -0.525731, 0.850651),  // 19
    float3(0.000000, 0.525731, -0.850651),  // 20
    float3(0.000000, -0.525731, -0.850651),  // 21
    float3(0.525731, 0.850651, 0.000000),  // 22
    float3(-0.525731, 0.850651, 0.000000),  // 23
    float3(0.525731, -0.850651, 0.000000),  // 24
    float3(-0.525731, -0.850651, 0.000000),  // 25
    float3(0.850651, 0.000000, 0.525731),  // 26
    float3(0.850651, 0.000000, -0.525731),  // 27
    float3(-0.850651, 0.000000, 0.525731),  // 28
    float3(-0.850651, 0.000000, -0.525731),  // 29
    // DODECAHEDRON vertices (offset 30)
    float3(0.577350, 0.577350, 0.577350),  // 30
    float3(0.577350, 0.577350, -0.577350),  // 31
    float3(0.577350, -0.577350, 0.577350),  // 32
    float3(0.577350, -0.577350, -0.577350),  // 33
    float3(-0.577350, 0.577350, 0.577350),  // 34
    float3(-0.577350, 0.577350, -0.577350),  // 35
    float3(-0.577350, -0.577350, 0.577350),  // 36
    float3(-0.577350, -0.577350, -0.577350),  // 37
    float3(0.000000, 0.356822, 0.934172),  // 38
    float3(0.000000, -0.356822, 0.934172),  // 39
    float3(0.000000, 0.356822, -0.934172),  // 40
    float3(0.000000, -0.356822, -0.934172),  // 41
    float3(0.934172, 0.000000, 0.356822),  // 42
    float3(-0.934172, 0.000000, 0.356822),  // 43
    float3(0.934172, 0.000000, -0.356822),  // 44
    float3(-0.934172, 0.000000, -0.356822),  // 45
    float3(0.356822, 0.934172, 0.000000),  // 46
    float3(-0.356822, 0.934172, 0.000000),  // 47
    float3(0.356822, -0.934172, 0.000000),  // 48
    float3(-0.356822, -0.934172, 0.000000)  // 49
};

// Combined platonic sequence buffer with perfect 120-element ping-pong patterns
static const PlatonicSequence PLATONIC_SEQUENCES[30] = {
    // TETRAHEDRON sequences (index 0 to 5)
    // Sequence 0 (30 complete cycles)
    {
        {0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1}
    },

    // Sequence 1 (30 complete cycles)
    {
        {3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2}
    },

    // Sequence 2 (30 complete cycles)
    {
        {0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3}
    },

    // Sequence 3 (30 complete cycles)
    {
        {0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1, 0, 1, 3, 2, 3, 1}
    },

    // Sequence 4 (30 complete cycles)
    {
        {1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0, 1, 0, 2, 3, 2, 0}
    },

    // Sequence 5 (30 complete cycles)
    {
        {2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1, 2, 1, 3, 0, 3, 1}
    },

    // OCTAHEDRON sequences (index 6 to 11)
    // Sequence 0 (20 complete cycles)
    {
        {0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1}
    },

    // Sequence 1 (20 complete cycles)
    {
        {5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4}
    },

    // Sequence 2 (20 complete cycles)
    {
        {0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5}
    },

    // Sequence 3 (20 complete cycles)
    {
        {0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1, 0, 1, 2, 5, 4, 3, 4, 5, 2, 1}
    },

    // Sequence 4 (20 complete cycles)
    {
        {2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1, 2, 1, 0, 3, 4, 5, 4, 3, 0, 1}
    },

    // Sequence 5 (20 complete cycles)
    {
        {3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2, 3, 2, 4, 1, 5, 0, 5, 1, 4, 2}
    },

    // CUBE sequences (index 12 to 17)
    // Sequence 0 (15 complete cycles)
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7}
    },

    // Sequence 1 (15 complete cycles)
    {
        {7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1, 0}
    },

    // Sequence 2 (15 complete cycles)
    {
        {0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4}
    },

    // Sequence 3 (15 complete cycles)
    {
        {0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4, 5, 6, 7, 3, 2, 1, 0, 1, 2, 3, 7, 6, 5, 4}
    },

    // Sequence 4 (15 complete cycles)
    {
        {3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7, 6, 5, 4, 0, 1, 2, 3, 2, 1, 0, 4, 5, 6, 7}
    },

    // Sequence 5 (15 complete cycles)
    {
        {4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0, 7, 1, 6, 2, 5, 3, 4, 3, 5, 2, 6, 1, 7, 0}
    },

    // ICOSAHEDRON sequences (index 18 to 23)
    // Sequence 0 (10 complete cycles)
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
    },

    // Sequence 1 (10 complete cycles)
    {
        {11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2}
    },

    // Sequence 2 (10 complete cycles)
    {
        {0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7}
    },

    // Sequence 3 (10 complete cycles)
    {
        {0, 1, 2, 3, 4, 5, 11, 10, 9, 8, 7, 6, 7, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 11, 10, 9, 8, 7, 6, 7, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 11, 10, 9, 8, 7, 6, 7, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 11, 10, 9, 8, 7, 6, 7, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 11, 10, 9, 8, 7, 6, 7, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 11, 10, 9, 8}
    },

    // Sequence 4 (10 complete cycles)
    {
        {5, 4, 3, 2, 1, 0, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 6, 7, 8, 9}
    },

    // Sequence 5 (10 complete cycles)
    {
        {6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1, 11, 0, 11, 1, 10, 2, 9, 3, 8, 4, 7, 5, 6, 5, 7, 4, 8, 3, 9, 2, 10, 1}
    },

    // DODECAHEDRON sequences (index 24 to 29)
    // Sequence 0 (6 complete cycles)
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5}
    },

    // Sequence 1 (6 complete cycles)
    {
        {19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14}
    },

    // Sequence 2 (6 complete cycles)
    {
        {0, 19, 1, 18, 2, 17, 3, 16, 4, 15, 5, 14, 6, 13, 7, 12, 8, 11, 9, 10, 9, 11, 8, 12, 7, 13, 6, 14, 5, 15, 4, 16, 3, 17, 2, 18, 1, 19, 0, 19, 1, 18, 2, 17, 3, 16, 4, 15, 5, 14, 6, 13, 7, 12, 8, 11, 9, 10, 9, 11, 8, 12, 7, 13, 6, 14, 5, 15, 4, 16, 3, 17, 2, 18, 1, 19, 0, 19, 1, 18, 2, 17, 3, 16, 4, 15, 5, 14, 6, 13, 7, 12, 8, 11, 9, 10, 9, 11, 8, 12, 7, 13, 6, 14, 5, 15, 4, 16, 3, 17, 2, 18, 1, 19, 0, 19, 1, 18, 2, 17}
    },

    // Sequence 3 (6 complete cycles)
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5}
    },

    // Sequence 4 (6 complete cycles)
    {
        {9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4}
    },

    // Sequence 5 (6 complete cycles)
    {
        {10, 9, 11, 8, 12, 7, 13, 6, 14, 5, 15, 4, 16, 3, 17, 2, 18, 1, 19, 0, 19, 1, 18, 2, 17, 3, 16, 4, 15, 5, 14, 6, 13, 7, 12, 8, 11, 9, 10, 9, 11, 8, 12, 7, 13, 6, 14, 5, 15, 4, 16, 3, 17, 2, 18, 1, 19, 0, 19, 1, 18, 2, 17, 3, 16, 4, 15, 5, 14, 6, 13, 7, 12, 8, 11, 9, 10, 9, 11, 8, 12, 7, 13, 6, 14, 5, 15, 4, 16, 3, 17, 2, 18, 1, 19, 0, 19, 1, 18, 2, 17, 3, 16, 4, 15, 5, 14, 6, 13, 7, 12, 8, 11, 9, 10, 9, 11, 8, 12, 7}
    }

};

// Utility function to get a vertex from any solid with sequence pattern
float3 GetPlatonicVertex(uint solidType, uint sequenceIndex, uint vertexIndex)
{
    // Get the solid size and offsets
    uint size = SOLID_SIZES[solidType];
    uint vertexOffset = VERTEX_OFFSETS[solidType];
    uint sequenceOffset = SEQUENCE_OFFSETS[solidType];

    // Get sequence index with wraparound
    uint seqIdx = sequenceIndex % SEQUENCE_COUNT;
    uint bufferIndex = sequenceOffset + seqIdx;

    // Get vertex index from the ping-pong sequence (modulo for perfect looping)
    uint idx = vertexIndex % SEQUENCE_BUFFER_SIZE;
    uint vertIdx = PLATONIC_SEQUENCES[bufferIndex].indices[idx];

    // Return the vertex from the unified buffer
    return PLATONIC_VERTICES[vertexOffset + vertIdx];
}

// Utility function for smooth sinusoidal animation using sequence patterns
float3 GetPlatonicVertexAnimated(uint solidType, uint sequenceIndex, float time, float speed)
{
    // Calculate smooth vertex index based on time
    float animIndex = fmod(time * speed, (float)SEQUENCE_BUFFER_SIZE);
    uint vertexIndex = (uint)animIndex;

    // Get the vertex
    return GetPlatonicVertex(solidType, sequenceIndex, vertexIndex);
}

// Individual solid access functions
float3 GetTETRAHEDRONVertex(uint sequenceIndex, uint vertexIndex)
{
    return GetPlatonicVertex(0, sequenceIndex, vertexIndex);
}

float3 GetOCTAHEDRONVertex(uint sequenceIndex, uint vertexIndex)
{
    return GetPlatonicVertex(1, sequenceIndex, vertexIndex);
}

float3 GetCUBEVertex(uint sequenceIndex, uint vertexIndex)
{
    return GetPlatonicVertex(2, sequenceIndex, vertexIndex);
}

float3 GetICOSAHEDRONVertex(uint sequenceIndex, uint vertexIndex)
{
    return GetPlatonicVertex(3, sequenceIndex, vertexIndex);
}

float3 GetDODECAHEDRONVertex(uint sequenceIndex, uint vertexIndex)
{
    return GetPlatonicVertex(4, sequenceIndex, vertexIndex);
}

// Individual animated access functions
float3 GetTETRAHEDRONVertexAnimated(uint sequenceIndex, float time, float speed)
{
    return GetPlatonicVertexAnimated(0, sequenceIndex, time, speed);
}

float3 GetOCTAHEDRONVertexAnimated(uint sequenceIndex, float time, float speed)
{
    return GetPlatonicVertexAnimated(1, sequenceIndex, time, speed);
}

float3 GetCUBEVertexAnimated(uint sequenceIndex, float time, float speed)
{
    return GetPlatonicVertexAnimated(2, sequenceIndex, time, speed);
}

float3 GetICOSAHEDRONVertexAnimated(uint sequenceIndex, float time, float speed)
{
    return GetPlatonicVertexAnimated(3, sequenceIndex, time, speed);
}

float3 GetDODECAHEDRONVertexAnimated(uint sequenceIndex, float time, float speed)
{
    return GetPlatonicVertexAnimated(4, sequenceIndex, time, speed);
}

#endif // PLATONIC_SOLIDS_INCLUDED
