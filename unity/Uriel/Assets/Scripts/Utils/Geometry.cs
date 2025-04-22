using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Utils
{
   
    public static class PolyhedronGenerator  
    {  
        // Phi (Golden Ratio) used in several polyhedra  
        private const float phi = 1.618033988749895f;  
        private const float invPhi = 0.618033988749895f;  

        /// <summary>  
        /// Enum defining different polyhedron types  
        /// </summary>  
        public enum PolyhedronType  
        {  
            Tetrahedron,  
            Cube,  
            Octahedron,  
            Dodecahedron,  
            Icosahedron,  
            Pyramid
            // Can add more types here (like truncated versions, etc.)  
        }

        private static void GeneratePyramidVertices(List<Vector3> vertices, int sides, float height, float radius)
        {
            // Ensure we have at least 3 sides  
            sides = Mathf.Max(3, sides);

            // Add the apex (top point)  
            vertices.Add(new Vector3(0, height, 0));

            // Add the base vertices  
            float angleStep = 2f * Mathf.PI / sides;
        
            for (int i = 0; i < sides; i++)
            {
                float angle = i * angleStep;
                float x = radius * Mathf.Cos(angle);
                float z = radius * Mathf.Sin(angle);
                vertices.Add(new Vector3(x, -height, z)); // Base at y = -0.5  
            }
        }  
        
        /// <summary>  
        /// Generates vertices for the specified polyhedron  
        /// </summary>  
        /// <param name="type">Type of polyhedron to generate</param>  
        /// <param name="scale">Scale to apply to vertices (default: uniform 1)</param>  
        /// <param name="offset">Offset to apply to vertices (default: no offset)</param>  
        /// <param name="origin">Origin point for the polyhedron (default: world origin)</param>  
        /// <param name="vertices">List that will be filled with vertices (will be resized if needed)</param>  
        public static List<Vector3> GenerateVertices(List<Vector3> vertices, 
            PolyhedronType type,  
            int sides, float height, float radius,
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
                case PolyhedronType.Tetrahedron:  
                    GenerateTetrahedronVertices(baseVertices);  
                    break;  
                case PolyhedronType.Cube:  
                    GenerateCubeVertices(baseVertices);  
                    break;  
                case PolyhedronType.Octahedron:  
                    GenerateOctahedronVertices(baseVertices);  
                    break;  
                case PolyhedronType.Dodecahedron:  
                    GenerateDodecahedronVertices(baseVertices);  
                    break;  
                case PolyhedronType.Icosahedron:  
                    GenerateIcosahedronVertices(baseVertices);  
                    break;  
                case PolyhedronType.Pyramid:
                    GeneratePyramidVertices(baseVertices, sides, height, radius);
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
            
        /// <summary>  
        /// Generates face indices for the specified polyhedron  
        /// Useful for mesh generation  
        /// </summary>  
        /// <param name="type">Type of polyhedron</param>  
        /// <param name="triangles">List that will be filled with triangle indices (will be resized)</param>  
        public static void GenerateFaceIndices(PolyhedronType type, List<int> triangles)  
        {  
            triangles.Clear();  
                
            switch (type)  
            {  
                case PolyhedronType.Tetrahedron:  
                    // 4 triangular faces  
                    triangles.AddRange(new int[] {  
                        0, 1, 2,  
                        0, 2, 3,  
                        0, 3, 1,  
                        1, 3, 2  
                    });  
                    break;  
                case PolyhedronType.Cube:  
                    // 6 square faces (12 triangles)  
                    triangles.AddRange(new int[] {  
                        0, 1, 2, 0, 2, 3, // bottom face  
                        4, 7, 6, 4, 6, 5, // top face  
                        0, 4, 5, 0, 5, 1, // front face  
                        1, 5, 6, 1, 6, 2, // right face  
                        2, 6, 7, 2, 7, 3, // back face  
                        3, 7, 4, 3, 4, 0  // left face  
                    });  
                    break;  
                case PolyhedronType.Octahedron:  
                    // 8 triangular faces  
                    triangles.AddRange(new int[] {  
                        0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1,  
                        5, 2, 1, 5, 3, 2, 5, 4, 3, 5, 1, 4  
                    });  
                    break;  
                case PolyhedronType.Dodecahedron:  
                    // 12 pentagonal faces (36 triangles)  
                    GenerateDodecahedronFaces(triangles);  
                    break;  
                case PolyhedronType.Icosahedron:  
                    // 20 triangular faces  
                    GenerateIcosahedronFaces(triangles);  
                    break;  
            }  
        }  

        #region Vertex Generators for Each Polyhedron  
            
        private static void GenerateTetrahedronVertices(List<Vector3> vertices)  
        {  
            // Regular tetrahedron with unit edge length, centered at origin  
            float a = 1.0f / 3.0f;  
            float b = (8.0f / 9.0f);  
            float c = Mathf.Sqrt(b);  
            float d = Mathf.Sqrt(2.0f / 9.0f);  
                
            vertices.Add(new Vector3(0, 0, c));  
            vertices.Add(new Vector3(-d, -a, 0));  
            vertices.Add(new Vector3(-d, a, 0));  
            vertices.Add(new Vector3(2 * d, 0, 0));  
        }  
            
        private static void GenerateCubeVertices(List<Vector3> vertices)  
        {  
            // Unit cube with vertices at [+-0.5, +-0.5, +-0.5]  
            float s = 0.5f;  
                
            vertices.Add(new Vector3(-s, -s, -s)); // 0: bottom-left-back  
            vertices.Add(new Vector3(s, -s, -s));  // 1: bottom-right-back  
            vertices.Add(new Vector3(s, -s, s));   // 2: bottom-right-front  
            vertices.Add(new Vector3(-s, -s, s));  // 3: bottom-left-front  
            vertices.Add(new Vector3(-s, s, -s));  // 4: top-left-back  
            vertices.Add(new Vector3(s, s, -s));   // 5: top-right-back  
            vertices.Add(new Vector3(s, s, s));    // 6: top-right-front  
            vertices.Add(new Vector3(-s, s, s));   // 7: top-left-front  
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
            
        private static void GenerateDodecahedronVertices(List<Vector3> vertices)  
        {  
            // Dodecahedron vertices (20 vertices)  
            // Constructed from cubes and golden ratio  
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
            
        #region Face Generators  
            
        private static void GenerateDodecahedronFaces(List<int> triangles)  
        {  
            // This is a simplified implementation for the dodecahedron faces  
            // For a complete implementation, we'd need to define all 36 triangles  
            // Here's a starting point for the dodecahedron's pentagonal faces  
                
            // Note: These indices need to match your vertex order properly  
            // A proper implementation would map each pentagon to 3 triangles  
                
            // This is a placeholder - exact indices would require precise vertex mapping  
            int[][] pentagonIndices = new int[][]  
            {  
                new int[] { 0, 1, 2, 3, 4 },  
                new int[] { 0, 5, 6, 7, 1 },  
                // ... and so on for all 12 pentagonal faces  
            };  
                
            // Convert each pentagon into 3 triangles (fan triangulation)  
            foreach (int[] pentagon in pentagonIndices)  
            {  
                // For each pentagon, add 3 triangles (0,1,2), (0,2,3), (0,3,4)  
                triangles.Add(pentagon[0]);  
                triangles.Add(pentagon[1]);  
                triangles.Add(pentagon[2]);  
                    
                triangles.Add(pentagon[0]);  
                triangles.Add(pentagon[2]);  
                triangles.Add(pentagon[3]);  
                    
                triangles.Add(pentagon[0]);  
                triangles.Add(pentagon[3]);  
                triangles.Add(pentagon[4]);  
            }  
                
            // Note: A complete implementation would define all 12 pentagons precisely  
        }  
            
        private static void GenerateIcosahedronFaces(List<int> triangles)  
        {  
            // Icosahedron faces (20 triangular faces)  
            // This would be a full list of all triangle indices  
            // Based on the vertex ordering from GenerateIcosahedronVertices  
            int[][] faceIndices = new int[][]  
            {  
                new int[] { 0, 4, 5 },  new int[] { 0, 5, 9 },  
                new int[] { 0, 9, 1 },  new int[] { 0, 1, 8 },  
                new int[] { 0, 8, 4 },  new int[] { 1, 9, 7 },  
                new int[] { 1, 7, 6 },  new int[] { 1, 6, 8 },  
                new int[] { 2, 5, 4 },  new int[] { 2, 4, 10 },  
                new int[] { 2, 10, 3 }, new int[] { 2, 3, 11 },  
                new int[] { 2, 11, 5 }, new int[] { 3, 10, 6 },  
                new int[] { 3, 6, 7 },  new int[] { 3, 7, 11 },  
                new int[] { 4, 8, 10 }, new int[] { 5, 11, 9 },  
                new int[] { 6, 10, 8 }, new int[] { 7, 9, 11 }  
            };  
                
            // Add all triangle faces  
            foreach (int[] face in faceIndices)  
            {  
                triangles.Add(face[0]);  
                triangles.Add(face[1]);  
                triangles.Add(face[2]);  
            }  
        }  
            
        #endregion  
            
        #region Utility Functions  
            
        /// <summary>  
        /// Creates a Unity mesh from the polyhedron vertices  
        /// </summary>  
        public static Mesh CreateMesh(PolyhedronType type, int sides, Vector3 scale = default, Vector3 offset = default, Vector3 origin = default)  
        {  
            Mesh mesh = new Mesh();  
            List<Vector3> vertices = new List<Vector3>();  
            List<int> triangles = new List<int>();  
                
            GenerateVertices(vertices, type, sides, 1, 1, scale,  offset, origin);  
            GenerateFaceIndices(type, triangles);  
                
            mesh.SetVertices(vertices);  
            mesh.SetTriangles(triangles, 0);  
                
            // Create normals (pointing outward from center)  
            Vector3[] normals = new Vector3[vertices.Count];  
            for (int i = 0; i < vertices.Count; i++)  
            {  
                normals[i] = (vertices[i] - origin).normalized;  
            }  
            mesh.SetNormals(normals);  
                
            // Calculate bounds and optimize  
            mesh.RecalculateBounds();  
            mesh.Optimize();  
                
            return mesh;  
        }  
            
        #endregion  
    }
}