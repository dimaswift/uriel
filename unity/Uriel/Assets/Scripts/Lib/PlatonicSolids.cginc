#ifndef PLATONIC_SOLIDS_INCLUDED
#define PLATONIC_SOLIDS_INCLUDED

struct Solid {  
    static const uint TETRAHEDRON = 0;  
    static const uint OCTAHEDRON = 1;  
    static const uint CUBE = 2;  
    static const uint ICOSAHEDRON = 3;
    static const uint DODECAHEDRON = 4;
    static const uint MATRIX = 5;
};


#define SOLIDS 6
#define TETRAHEDRON_SIZE 4
#define OCTAHEDRON_SIZE 6
#define CUBE_SIZE 8
#define ICOSAHEDRON_SIZE 12
#define DODECAHEDRON_SIZE 20
#define MATRIX_SIZE 29

#define TOTAL_VERTICES TETRAHEDRON_SIZE + OCTAHEDRON_SIZE + CUBE_SIZE + ICOSAHEDRON_SIZE + DODECAHEDRON_SIZE + MATRIX_SIZE

static const uint SOLID_SIZES[SOLIDS] = {
    TETRAHEDRON_SIZE,  // TETRAHEDRON
    OCTAHEDRON_SIZE,  // OCTAHEDRON
    CUBE_SIZE,  // CUBE
    ICOSAHEDRON_SIZE,  // ICOSAHEDRON
    DODECAHEDRON_SIZE,  // DODECAHEDRON
    MATRIX_SIZE,  // MATRIX
};

// Vertex offsets for each solid in the unified vertex buffer
static const uint VERTEX_OFFSETS[SOLIDS] = {
    0, // TETRAHEDRON
    4, // OCTAHEDRON
    10,// CUBE
    18,// ICOSAHEDRON
    30,// DODECAHEDRON
    50 // MATRIX
};

// Unified buffer containing vertices for all platonic solids
static float3 PLATONIC_VERTICES[TOTAL_VERTICES] = {
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
    float3(-0.356822, -0.934172, 0.000000),  // 49
    // MATRIX vertices (offset 50)
    float3(-1, -1, -1),
    float3(-1, -1, 0),
    float3(-1, -1, 1),
    float3(-1, 0, -1),
    float3(-1, 0, 0),
    float3(-1, 0, 1),
    float3(-1, 1, -1),
    float3(-1, 1, 0),
    float3(-1, 1, 1),
    float3(0, -1, -1),
    float3(0, -1, 0),
    float3(0, -1, 1),
    float3(0, 0, -1),
    float3(0, 0, 0),
    float3(0, 0, 1),
    float3(0, 1, -1),
    float3(0, 1, 0),
    float3(0, 1, 1),
    float3(1, -1, -1),
    float3(1, -1, 0),
    float3(1, -1, 1),
    float3(1, 0, -1),
    float3(1, 0, 0),
    float3(1, 0, 1),
    float3(1, 1, -1),
    float3(1, 1, 0),
    float3(1, 1, 1),
    float3(0, 2, 0),
    float3(0, 2, 0),
};

uint getPlatonicSize(uint solidType)
{
    return SOLID_SIZES[solidType];
}

float3 getPlatonicVertex(uint solidType, uint vertexIndex)
{
    const uint vertexOffset = VERTEX_OFFSETS[solidType];
    return PLATONIC_VERTICES[vertexOffset + vertexIndex];
}

#endif // PLATONIC_SOLIDS_INCLUDED
