using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Utils
{
    public static class PlatonicSolids  
    {  
        // Phi (Golden Ratio) used in several polyhedra  
        private const float phi = 1.618033988749895f;  
        private const float invPhi = 0.618033988749895f;  

        /// <summary>  
        /// Enum defining different polyhedron types  
        /// </summary>  
        public enum Type  
        {  
            Tetrahedron,  
            Cube,  
            Octahedron,  
            Dodecahedron,  
            Icosahedron
            // Can add more types here (like truncated versions, etc.)  
        }

        public enum Mode
        {
            Vertex, // 4 main corners  
            Edge, // 6 edge points (requires offset)  
            Face // 4 face centers  
        }

        private static readonly int[,] TetrahedronEdges = new int[,]
        {
            { 0, 1 }, { 0, 2 }, { 0, 3 },
            { 1, 2 }, { 1, 3 }, { 2, 3 }
        };

        private static readonly int[,] TetrahedronFaces = new int[,]
        {
            { 0, 1, 2 },
            { 0, 1, 3 },
            { 0, 2, 3 },
            { 1, 2, 3 }
        };

        /// <summary>  
        /// Generates vertices for the specified polyhedron  
        /// </summary>
        /// <param name="vertices">List that will be filled with vertices (will be resized if needed)</param>
        /// <param name="type">Type of polyhedron to generate</param>
        /// <param name="mode">Vertex mode</param>
        /// <param name="edgeOffset">Edge offset</param>
        /// <param name="scale">Scale to apply to vertices (default: uniform 1)</param>
        /// <param name="offset">Offset to apply to vertices (default: no offset)</param>
        /// <param name="origin">Origin point for the polyhedron (default: world origin)</param>
        public static List<Vector3> GenerateVertices(List<Vector3> vertices, 
            Type type, Mode mode, Vector2 uv,
            Vector3 scale = default,  
            Vector3 offset = default,  
            Vector3 origin = default)  
        {  
            // Initialize default parameters  
            if (scale == default) scale = Vector3.one;  
            if (offset == default) offset = Vector3.zero;  
            if (origin == default) origin = Vector3.zero;  
            if (vertices == null) vertices = new List<Vector3>();  
                
            // Clear existing vertices  
            vertices.Clear();  
                
            // Generate base vertices based on type  
            List<Vector3> baseVertices = new List<Vector3>();  
            switch (type)  
            {  
                case Type.Tetrahedron:  
                    GenerateTetrahedronPoints(baseVertices, mode, uv.x);  
                    break;  
                case Type.Cube:  
                    GenerateCubePoints(baseVertices, mode, uv);  
                    break;  
                case Type.Octahedron:  
                    GenerateOctahedronVertices(baseVertices);  
                    break;  
                case Type.Dodecahedron:
                    GenerateDodecahedronPoints(baseVertices, mode, uv);  
                    break;  
                case Type.Icosahedron:  
                    GenerateIcosahedronVertices(baseVertices);  
                    break;
            }  
                
            // Apply transformations to vertices  
            foreach (Vector3 baseVertex in baseVertices)  
            {  
                // Apply scale, offset, and origin  
                Vector3 transformedVertex = Vector3.Scale(baseVertex, scale) + offset + origin;  
                vertices.Add(transformedVertex);  
            }

            return vertices;
        }  
            
        
        #region Vertex Generators for Each Polyhedron

        private static List<Vector3> GenerateTetrahedronVertexPositions()
        {
            float s = 1f / Mathf.Sqrt(8f);
            return new List<Vector3>
            {
                new Vector3(s, s, s), // 0  
                new Vector3(s, -s, -s), // 1  
                new Vector3(-s, s, -s), // 2  
                new Vector3(-s, -s, s), // 3  
            };
        }

        public static void GenerateTetrahedronPoints(
            List<Vector3> points,
            Mode mode = Mode.Vertex,
            float edgeOffset = 0.5f // for Edge mode; 0 = start, 1 = end, 0.5 = middle  
        )
        {
            points.Clear();
            var v = GenerateTetrahedronVertexPositions();

            switch (mode)
            {
                case Mode.Vertex:
                    points.AddRange(v);
                    break;

                case Mode.Edge:
                    for (int i = 0; i < TetrahedronEdges.GetLength(0); i++)
                    {
                        int a = TetrahedronEdges[i, 0];
                        int b = TetrahedronEdges[i, 1];
                        Vector3 pos = Vector3.Lerp(v[a], v[b], edgeOffset);
                        points.Add(pos);
                    }

                    break;

                case Mode.Face:
                    for (int i = 0; i < TetrahedronFaces.GetLength(0); i++)
                    {
                        int a = TetrahedronFaces[i, 0];
                        int b = TetrahedronFaces[i, 1];
                        int c = TetrahedronFaces[i, 2];
                        Vector3 center = (v[a] + v[b] + v[c]) / 3f;
                        points.Add(center);
                    }

                    break;
            }
        }

         private static readonly int[,] CubeEdges = new int[,]  
    {  
        { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 }, // Bottom face edges  
        { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 }, // Top face edges  
        { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }  // Connecting edges  
    };  

    // Vertex indices for the 6 faces (quads) of the cube.  
    // Order matters for bilinear interpolation: A, B, C, D  
    // where standard UV (0,0)=A, (1,0)=B, (1,1)=C, (0,1)=D  
    private static readonly int[,] CubeFaces = new int[,]  
    {  
        { 0, 1, 2, 3 }, // Bottom (-Y) A=0, B=1, C=2, D=3  
        { 4, 7, 6, 5 }, // Top (+Y)    A=4, B=7, C=6, D=5 (Adjusted order for consistent UV)  
        { 0, 4, 5, 1 }, // Back (-Z)   A=0, B=4, C=5, D=1  
        { 3, 2, 6, 7 }, // Front (+Z)  A=3, B=2, C=6, D=7  
        { 0, 3, 7, 4 }, // Left (-X)   A=0, B=3, C=7, D=4  
        { 1, 5, 6, 2 }  // Right (+X)  A=1, B=5, C=6, D=2  
    };  

    // Helper to get the base 8 vertices  
    private static List<Vector3> _GenerateCubeVertexPositions()  
    {  
        float s = 0.5f;  
        return new List<Vector3>  
        {  
            new Vector3(-s, -s, -s), // 0: ---  
            new Vector3( s, -s, -s), // 1: +--  
            new Vector3( s, -s,  s), // 2: +-+  
            new Vector3(-s, -s,  s), // 3: --+  
            new Vector3(-s,  s, -s), // 4: -+-  
            new Vector3( s,  s, -s), // 5: ++-  
            new Vector3( s,  s,  s), // 6: +++  
            new Vector3(-s,  s,  s)  // 7: -++  
        };  
    }  

    // --- Public Method ---  

    public static void GenerateCubePoints(
        List<Vector3> points,
        Mode mode = Mode.Vertex, 
        Vector2 uv = default // UV coordinates for Edge/Face modes  
    )
    {
        points.Clear(); // Ensure the list is empty  
        var v = _GenerateCubeVertexPositions(); // Get the 8 base vertices  

        switch (mode)
        {
            case Mode.Vertex:
                points.AddRange(v); // Add all 8 vertices  
                break;

            case Mode.Edge:
                for (int i = 0; i < CubeEdges.GetLength(0); i++)
                {
                    int idxA = CubeEdges[i, 0];
                    int idxB = CubeEdges[i, 1];
                    // Interpolate using uv.x, clamped between 0 and 1  
                    float t = Mathf.Clamp01(uv.x);
                    Vector3 pos = Vector3.Lerp(v[idxA], v[idxB], t);
                    points.Add(pos);
                }

                break;

            case Mode.Face:
                for (int i = 0; i < CubeFaces.GetLength(0); i++)
                {
                    // Get the four vertex indices for this face (A, B, C, D)  
                    int idxA = CubeFaces[i, 0];
                    int idxB = CubeFaces[i, 1];
                    int idxC = CubeFaces[i, 2];
                    int idxD = CubeFaces[i, 3];

                    // Get the actual vertex positions  
                    Vector3 posA = v[idxA];
                    Vector3 posB = v[idxB];
                    Vector3 posC = v[idxC];
                    Vector3 posD = v[idxD];

                    // Remap the user UVs (where 0,0 is center) to standard UVs (where 0,0 is corner A)  
                    // u_std = user_u + 0.5  
                    // v_std = user_v + 0.5  
                    float u_std = Mathf.Clamp01(uv.x + 0.5f);
                    float v_std = Mathf.Clamp01(uv.y + 0.5f);

                    // Bilinear interpolation using standard UV coordinates  
                    // P(u,v) = (1-u)(1-v)A + u(1-v)B + uvC + (1-u)vD  
                    Vector3 pos = (1f - u_std) * (1f - v_std) * posA +
                                  u_std * (1f - v_std) * posB +
                                  u_std * v_std * posC +
                                  (1f - u_std) * v_std * posD;

                    points.Add(pos);
                }

                break;
        }
    }

        private static void GenerateOctahedronVertices(List<Vector3> vertices)  
        {  
            // Octahedron with vertices on the primary axes  
            vertices.Add(new Vector3(0, 1, 0));    // top  
            vertices.Add(new Vector3(1, 0, 0));    // right  
            vertices.Add(new Vector3(0, 0, 1));    // front  
            vertices.Add(new Vector3(-1, 0, 0));   // left  
            vertices.Add(new Vector3(0, 0, -1));   // back  
            vertices.Add(new Vector3(0, -1, 0));   // bottom  
        }  
            
        // Define edges (30 edges connecting vertices)  
        private static readonly int[,] DodecahedronEdges = new int[,]  
        {  
            // These are the 30 edges of a dodecahedron  
            { 0, 1 }, { 0, 4 }, { 0, 8 }, { 1, 2 }, { 1, 9 },  
            { 2, 3 }, { 2, 10 }, { 3, 4 }, { 3, 11 }, { 4, 5 },  
            { 5, 6 }, { 5, 12 }, { 6, 7 }, { 6, 13 }, { 7, 8 },  
            { 7, 14 }, { 8, 9 }, { 9, 10 }, { 10, 11 }, { 11, 12 },  
            { 12, 13 }, { 13, 14 }, { 14, 15 }, { 15, 16 }, { 15, 19 },  
            { 16, 17 }, { 16, 18 }, { 17, 18 }, { 17, 19 }, { 18, 19 }  
        };  

        // Dodecahedron has 12 pentagonal faces  
        private static readonly int[,] DodecahedronFaces = new int[,]  
        {  
            { 0, 1, 2, 3, 4 },      // Face 1  
            { 0, 4, 5, 6, 7 },      // Face 2  
            { 0, 7, 8, 9, 1 },      // Face 3  
            { 1, 9, 10, 11, 2 },    // Face 4  
            { 2, 11, 12, 13, 3 },   // Face 5  
            { 3, 13, 14, 5, 4 },    // Face 6  
            { 5, 14, 15, 16, 6 },   // Face 7  
            { 6, 16, 17, 8, 7 },    // Face 8  
            { 8, 17, 18, 10, 9 },   // Face 9  
            { 10, 18, 19, 12, 11 }, // Face 10  
            { 12, 19, 15, 14, 13 }, // Face 11  
            { 15, 19, 18, 17, 16 }  // Face 12  
        };  

        // Helper to get the base vertices  
        private static List<Vector3> _GenerateDodecahedronVertexPositions()  
        {  
            List<Vector3> vertices = new List<Vector3>();  
            float a = 1.0f;  
            float b = 1.0f / phi;  
            float c = phi;  
                    
            // Add all permutations of (±1, ±1, ±1)  
            for (int i = -1; i <= 1; i += 2)  
                for (int j = -1; j <= 1; j += 2)  
                    for (int k = -1; k <= 1; k += 2)  
                        vertices.Add(new Vector3(i * a, j * a, k * a));  
                    
            // Add all even permutations of (0, ±phi, ±1/phi)  
            vertices.Add(new Vector3(0, b, c));  
            vertices.Add(new Vector3(0, -b, c));  
            vertices.Add(new Vector3(0, b, -c));  
            vertices.Add(new Vector3(0, -b, -c));  
                    
            vertices.Add(new Vector3(c, 0, b));  
            vertices.Add(new Vector3(-c, 0, b));  
            vertices.Add(new Vector3(c, 0, -b));  
            vertices.Add(new Vector3(-c, 0, -b));  
                    
            vertices.Add(new Vector3(b, c, 0));  
            vertices.Add(new Vector3(-b, c, 0));  
            vertices.Add(new Vector3(b, -c, 0));  
            vertices.Add(new Vector3(-b, -c, 0));  
                    
            // Scale to make unit edge length  
            float scale = 1.0f / (a * Mathf.Sqrt(3.0f));  
            for (int i = 0; i < vertices.Count; i++)  
            {  
                vertices[i] *= scale;  
            }  

            return vertices;  
        }  

// Public method that supports different modes  
        public static void GenerateDodecahedronPoints(  
            List<Vector3> points,  
            Mode mode = Mode.Vertex,  
            Vector2 uv = default // UV coordinates for Edge/Face modes  
        )  
        {  
            points.Clear();  
            var vertices = _GenerateDodecahedronVertexPositions();  

            switch (mode)  
            {  
                case Mode.Vertex:  
                    points.AddRange(vertices);  
                    break;  

                case Mode.Edge:  
                    for (int i = 0; i < DodecahedronEdges.GetLength(0); i++)  
                    {  
                        int idxA = DodecahedronEdges[i, 0];  
                        int idxB = DodecahedronEdges[i, 1];  
                        // Interpolate using uv.x, clamped between 0 and 1  
                        float t = Mathf.Clamp01(uv.x);  
                        Vector3 pos = Vector3.Lerp(vertices[idxA], vertices[idxB], t);  
                        points.Add(pos);  
                    }  
                    break;  

                case Mode.Face:
                    for (int i = 0; i < DodecahedronFaces.GetLength(0); i++)
                    {
                        Vector3 faceCenter = Vector3.zero;
                        int vertexCount = 5; // Dodecahedron faces are pentagons  

                        // Sum the positions of the vertices defining this face  
                        for (int v = 0; v < vertexCount; v++)
                        {
                            int vertexIndex = DodecahedronFaces[i, v];
                            // Basic check to avoid index out of bounds  
                            if (vertexIndex >= 0 && vertexIndex < vertices.Count)
                            {
                                faceCenter += vertices[vertexIndex];
                            }
                            else
                            {
                                // Log an error if the index is invalid  
                                Debug.LogError(
                                    $"Invalid vertex index {vertexIndex} found in DodecahedronFaces definition for face {i}.");
                                // Skip this face to prevent further errors  
                                goto NextFace;
                            }
                        }

                        // Calculate the average position (the geometric center)  
                        faceCenter /= vertexCount;

                        // Add the calculated center point for this face  
                        points.Add(faceCenter);

                        NextFace: ; // Label to jump to if an invalid index was found  
                    }  
                    break;  
            }  
        }  
            
        private static void GenerateIcosahedronVertices(List<Vector3> vertices)  
        {  
            // Icosahedron with 12 vertices  
            // Based on 3 orthogonal golden rectangles  
            float a = 1.0f;  
            float b = phi;  
                
            // Add vertices based on permutations of coordinates  
            // (0, ±1, ±φ), (±1, ±φ, 0), (±φ, 0, ±1)  
            vertices.Add(new Vector3(0, a, b));  
            vertices.Add(new Vector3(0, -a, b));  
            vertices.Add(new Vector3(0, a, -b));  
            vertices.Add(new Vector3(0, -a, -b));  
                
            vertices.Add(new Vector3(a, b, 0));  
            vertices.Add(new Vector3(-a, b, 0));  
            vertices.Add(new Vector3(a, -b, 0));  
            vertices.Add(new Vector3(-a, -b, 0));  
                
            vertices.Add(new Vector3(b, 0, a));  
            vertices.Add(new Vector3(-b, 0, a));  
            vertices.Add(new Vector3(b, 0, -a));  
            vertices.Add(new Vector3(-b, 0, -a));  
                
            // Scale to make unit edge length  
            float edgeLength = 2.0f; // Base edge length before scaling  
            float scale = 1.0f / edgeLength;  
            for (int i = 0; i < vertices.Count; i++)  
            {  
                vertices[i] *= scale;  
            }  
        }  
            
        #endregion  
        
    }
}