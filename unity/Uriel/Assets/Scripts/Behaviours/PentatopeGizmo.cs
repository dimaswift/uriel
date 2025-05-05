
using UnityEngine;
using System.Collections.Generic;

public class PentatopeGizmo : MonoBehaviour
{
    [Header("Visualization Settings")]
    public bool drawVertices = true;
    public bool drawEdges = true;
    public bool drawFaces = false;
    public bool drawCells = false;
    
    [Header("Appearance")]
    public float vertexSize = 0.1f;
    public float edgeThickness = 2f;
    public Color vertexColor = Color.yellow;
    public Color edgeColor = Color.cyan;
    public Color faceColor = new Color(1f, 0.5f, 0.5f, 0.3f);
    
    [Header("4D Rotation Settings")]
    public float xwRotationSpeed = 15f;
    public float ywRotationSpeed = 10f;
    public float zwRotationSpeed = 8f;
    public float xyRotationSpeed = 5f;
    public bool autoRotate = true;
    
    [Header("Projection")]
    [Range(1f, 5f)]
    public float wOffset = 2f; // Affects 4D perspective projection
    
    // The 5 vertices of the 5-cell
    private Vector4[] vertices = new Vector4[5];
    
    // Edges (pairs of vertex indices)
    private int[,] edges = new int[10, 2];
    
    // Faces (triplets of vertex indices)
    private int[,] faces = new int[10, 3];
    
    // Cells (groups of 4 vertex indices forming tetrahedra)
    private int[,] cells = new int[5, 4];
    
    // 4D Rotation angles
    private float xwAngle, ywAngle, zwAngle, xyAngle;
    
    void OnEnable()
    {
        InitializePentachoron();
    }
    
    void InitializePentachoron()
    {
        // Initialize the 5 vertices of the 5-cell
        vertices[0] = new Vector4(0, 0, 0, 0);
        vertices[1] = new Vector4(1, 0, 0, 0);
        vertices[2] = new Vector4(0.5f, Mathf.Sqrt(3)/2, 0, 0);
        vertices[3] = new Vector4(0.5f, Mathf.Sqrt(3)/6, Mathf.Sqrt(6)/3, 0);
        vertices[4] = new Vector4(0.5f, Mathf.Sqrt(3)/6, Mathf.Sqrt(6)/12, Mathf.Sqrt(10)/4);
        
        // Scale vertices for better visualization
        for (int i = 0; i < 5; i++)
        {
            vertices[i] *= 2f;
        }
        
        // Define the 10 edges (all possible connections between vertices)
        int edgeIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = i+1; j < 5; j++)
            {
                edges[edgeIndex, 0] = i;
                edges[edgeIndex, 1] = j;
                edgeIndex++;
            }
        }
        
        // Define the 10 triangular faces
        faces[0, 0] = 0; faces[0, 1] = 1; faces[0, 2] = 2;
        faces[1, 0] = 0; faces[1, 1] = 1; faces[1, 2] = 3;
        faces[2, 0] = 0; faces[2, 1] = 1; faces[2, 2] = 4;
        faces[3, 0] = 0; faces[3, 1] = 2; faces[3, 2] = 3;
        faces[4, 0] = 0; faces[4, 1] = 2; faces[4, 2] = 4;
        faces[5, 0] = 0; faces[5, 1] = 3; faces[5, 2] = 4;
        faces[6, 0] = 1; faces[6, 1] = 2; faces[6, 2] = 3;
        faces[7, 0] = 1; faces[7, 1] = 2; faces[7, 2] = 4;
        faces[8, 0] = 1; faces[8, 1] = 3; faces[8, 2] = 4;
        faces[9, 0] = 2; faces[9, 1] = 3; faces[9, 2] = 4;
        
        // Define the 5 tetrahedral cells
        cells[0, 0] = 1; cells[0, 1] = 2; cells[0, 2] = 3; cells[0, 3] = 4; // tetrahedron excluding vertex 0
        cells[1, 0] = 0; cells[1, 1] = 2; cells[1, 2] = 3; cells[1, 3] = 4; // tetrahedron excluding vertex 1
        cells[2, 0] = 0; cells[2, 1] = 1; cells[2, 2] = 3; cells[2, 3] = 4; // tetrahedron excluding vertex 2
        cells[3, 0] = 0; cells[3, 1] = 1; cells[3, 2] = 2; cells[3, 3] = 4; // tetrahedron excluding vertex 3
        cells[4, 0] = 0; cells[4, 1] = 1; cells[4, 2] = 2; cells[4, 3] = 3; // tetrahedron excluding vertex 4
    }
    
    void Update()
    {
        if (autoRotate)
        {
            // Update 4D rotation angles
            float deltaTime = Time.deltaTime;
            xwAngle = xwRotationSpeed;
            ywAngle = ywRotationSpeed;
            zwAngle = zwRotationSpeed;
            xyAngle = xyRotationSpeed;
        }
    }
    
    // Projects a 4D point to 3D using simple perspective division
    private Vector3 ProjectTo3D(Vector4 point4D)
    {
        // Apply 4D rotations
        Vector4 rotated = Rotate4D(point4D);
        
        // Project to 3D by dividing by (w + offset)
        float w = rotated.w + wOffset;
        return new Vector3(
            rotated.x / w,
            rotated.y / w,
            rotated.z / w
        );
    }
    
    // Apply 4D rotations in different hyperplanes
    private Vector4 Rotate4D(Vector4 point)
    {
        // XW plane rotation
        float cosXW = Mathf.Cos(xwAngle * Mathf.Deg2Rad);
        float sinXW = Mathf.Sin(xwAngle * Mathf.Deg2Rad);
        float x = point.x * cosXW - point.w * sinXW;
        float w = point.x * sinXW + point.w * cosXW;
        point.x = x;
        point.w = w;
        
        // YW plane rotation
        float cosYW = Mathf.Cos(ywAngle * Mathf.Deg2Rad);
        float sinYW = Mathf.Sin(ywAngle * Mathf.Deg2Rad);
        float y = point.y * cosYW - point.w * sinYW;
        w = point.y * sinYW + point.w * cosYW;
        point.y = y;
        point.w = w;
        
        // ZW plane rotation
        float cosZW = Mathf.Cos(zwAngle * Mathf.Deg2Rad);
        float sinZW = Mathf.Sin(zwAngle * Mathf.Deg2Rad);
        float z = point.z * cosZW - point.w * sinZW;
        w = point.z * sinZW + point.w * cosZW;
        point.z = z;
        point.w = w;
        
        // XY plane rotation (standard 2D rotation)
        float cosXY = Mathf.Cos(xyAngle * Mathf.Deg2Rad);
        float sinXY = Mathf.Sin(xyAngle * Mathf.Deg2Rad);
        x = point.x * cosXY - point.y * sinXY;
        y = point.x * sinXY + point.y * cosXY;
        point.x = x;
        point.y = y;
        
        return point;
    }
    
    // This function is called by Unity in the editor to draw gizmos
    void OnDrawGizmos()
    {
        if (!enabled)
            return;
            
        // We need vertices to draw anything
        if (vertices == null || vertices.Length != 5)
            InitializePentachoron();
            
        // Project all vertices to 3D
        Vector3[] projectedVertices = new Vector3[5];
        for (int i = 0; i < 5; i++)
        {
            projectedVertices[i] = ProjectTo3D(vertices[i]);
        }
        
        // Draw edges
        if (drawEdges)
        {
            Gizmos.color = edgeColor;
            for (int i = 0; i < 10; i++)
            {
                int v1 = edges[i, 0];
                int v2 = edges[i, 1];
                Gizmos.DrawLine(projectedVertices[v1], projectedVertices[v2]);
            }
        }
        
        // Draw faces (triangles)
        if (drawFaces)
        {
            Gizmos.color = faceColor;
            for (int i = 0; i < 10; i++)
            {
                int v1 = faces[i, 0];
                int v2 = faces[i, 1];
                int v3 = faces[i, 2];
                
                Gizmos.DrawLine(projectedVertices[v1], projectedVertices[v2]);
                Gizmos.DrawLine(projectedVertices[v2], projectedVertices[v3]);
                Gizmos.DrawLine(projectedVertices[v3], projectedVertices[v1]);
                
                // Draw triangle mesh
                Vector3 center = (projectedVertices[v1] + projectedVertices[v2] + projectedVertices[v3]) / 3f;
                DrawTriangle(projectedVertices[v1], projectedVertices[v2], projectedVertices[v3]);
            }
        }
        
        // Draw vertices (must be drawn last to be visible on top)
        if (drawVertices)
        {
            Gizmos.color = vertexColor;
            foreach (Vector3 vertex in projectedVertices)
            {
                Gizmos.DrawSphere(vertex, vertexSize);
            }
        }
    }
    
    // Helper to draw a solid triangle with Gizmos
    private void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { p1, p2, p3 };
        mesh.triangles = new int[] { 0, 1, 2 };
        
        // Calculate simple normal
        Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        mesh.normals = new Vector3[] { normal, normal, normal };
        
        Gizmos.DrawMesh(mesh);
    }
}
