
using UnityEngine;
using System.Collections.Generic;
using Uriel.Domain;

namespace Uriel.Rendering
{
    [RequireComponent(typeof(LineRenderer))]
    public class WireframeRenderer : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        private readonly List<Vector3> buffer = new();
        private readonly List<Vector3> secondBuffer = new();

        private Material mat;
        
        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            mat = lineRenderer.material;
        }
        

        public void SetType(SculptSolidType type)
        {
            switch (type)
            {
                case SculptSolidType.Box:
                    CreateBoxWireframe();
                    break;
                case SculptSolidType.Sphere:
                    CreateSphereWireframe();
                    break;
                case SculptSolidType.Cylinder:
                    CreateCylinderWireframe();
                    break;
                case SculptSolidType.Capsule:
                    CreateCapsuleWireframe();
                    break;
                default:
                    CreateBoxWireframe();
                    break;
            }
        }
        
        private void CreateBoxWireframe()
        {
            float halfSize = 0.5f;
            
            // Define the 8 vertices of a cube
            buffer.Clear();
            buffer.Add(new Vector3(halfSize, halfSize, halfSize));
            buffer.Add(new Vector3(halfSize, halfSize, -halfSize));
            buffer.Add(new Vector3(-halfSize, halfSize, halfSize));
            buffer.Add(new Vector3(halfSize, -halfSize, halfSize));
            buffer.Add(new Vector3(-halfSize, -halfSize, -halfSize));
            buffer.Add(new Vector3(halfSize, -halfSize, -halfSize));
            buffer.Add(new Vector3(-halfSize, -halfSize, halfSize));
            buffer.Add(new Vector3(-halfSize, halfSize, -halfSize));
            secondBuffer.Clear();
            // Create a path that draws all edges of the cube
            // We'll draw the wireframe by creating a path that visits all edges
    
            // Bottom face (y = -halfSize)
            secondBuffer.Add(buffer[3]); // bottom-front-right
            secondBuffer.Add(buffer[5]); // bottom-back-right
            secondBuffer.Add(buffer[4]); // bottom-back-left
            secondBuffer.Add(buffer[6]); // bottom-front-left
            secondBuffer.Add(buffer[3]); // back to start of bottom face
            
            // Vertical edge to top
            secondBuffer.Add(buffer[0]); // top-front-right
            
            // Top face (y = halfSize)
            secondBuffer.Add(buffer[1]); // top-back-right
            secondBuffer.Add(buffer[7]); // top-back-left
            secondBuffer.Add(buffer[2]); // top-front-left
            secondBuffer.Add(buffer[0]); // back to top-front-right
            
            // Connect remaining vertical edges
            secondBuffer.Add(buffer[1]); // top-back-right
            secondBuffer.Add(buffer[5]); // bottom-back-right (vertical edge)
            
            // Move to draw another vertical edge
            secondBuffer.Add(buffer[4]); // bottom-back-left
            secondBuffer.Add(buffer[7]); // top-back-left (vertical edge)
            
            // Final vertical edge
            secondBuffer.Add(buffer[2]); // top-front-left
            secondBuffer.Add(buffer[6]); // bottom-front-left (vertical edge)
            
            // Set up the LineRenderer
            lineRenderer.positionCount = secondBuffer.Count;
            lineRenderer.SetPositions(secondBuffer.ToArray());
        }

                
        private void CreateSphereWireframe()
        {
            buffer.Clear();
            float radius = 0.5f;
            int segments = 24; // Number of segments for circles
            int rings = 8; // Number of latitude rings
            
            // Create latitude rings
            for (int ring = 0; ring <= rings; ring++)
            {
                float y = Mathf.Cos(Mathf.PI * ring / rings) * radius;
                float ringRadius = Mathf.Sin(Mathf.PI * ring / rings) * radius;
                
                // Skip poles (top and bottom)
                if (ring == 0 || ring == rings) continue;
                
                // Create circle at this latitude
                for (int i = 0; i <= segments; i++)
                {
                    float angle = 2 * Mathf.PI * i / segments;
                    Vector3 point = new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        y,
                        Mathf.Sin(angle) * ringRadius
                    );
                    buffer.Add(point);
                }
                
                // Add break between rings (duplicate last point)
                if (ring < rings - 1)
                {
                    buffer.Add(buffer[buffer.Count - 1]);
                }
            }
            
            // Create longitude lines (meridians)
            int meridians = 8;
            for (int m = 0; m < meridians; m++)
            {
                float angle = 2 * Mathf.PI * m / meridians;
                
                for (int i = 0; i <= segments; i++)
                {
                    float t = Mathf.PI * i / segments;
                    Vector3 point = new Vector3(
                        Mathf.Cos(angle) * Mathf.Sin(t) * radius,
                        Mathf.Cos(t) * radius,
                        Mathf.Sin(angle) * Mathf.Sin(t) * radius
                    );
                    buffer.Add(point);
                }
                
                // Add break between meridians
                if (m < meridians - 1)
                {
                    buffer.Add(buffer[buffer.Count - 1]);
                }
            }
            
            lineRenderer.positionCount = buffer.Count;
            lineRenderer.SetPositions(buffer.ToArray());
        }

        private void CreateCylinderWireframe()
        {
           
            float radius = 0.5f;
            float height = 1;
            float halfHeight = height * 0.5f;
            int segments = 24;
            buffer.Clear();
            // Bottom circle
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * radius,
                    -halfHeight,
                    Mathf.Sin(angle) * radius
                );
                buffer.Add(point);
            }
            
            // Connect to top circle via vertical line
            Vector3 bottomStart = buffer[0];
            Vector3 topStart = new Vector3(bottomStart.x, halfHeight, bottomStart.z);
            buffer.Add(topStart);
            
            // Top circle
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * radius,
                    halfHeight,
                    Mathf.Sin(angle) * radius
                );
                buffer.Add(point);
            }
            
            // Add vertical lines at quarter points
            for (int i = 1; i < 4; i++)
            {
                int segmentIndex = (segments * i) / 4;
                float angle = 2 * Mathf.PI * segmentIndex / segments;
                
                Vector3 bottomPoint = new Vector3(
                    Mathf.Cos(angle) * radius,
                    -halfHeight,
                    Mathf.Sin(angle) * radius
                );
                Vector3 topPoint = new Vector3(
                    Mathf.Cos(angle) * radius,
                    halfHeight,
                    Mathf.Sin(angle) * radius
                );
                
                buffer.Add(bottomPoint);
                buffer.Add(topPoint);
            }
            
            lineRenderer.positionCount = buffer.Count;
            lineRenderer.SetPositions(buffer.ToArray());
        }

        private void CreateCapsuleWireframe()
        {
            buffer.Clear();
            float radius = 0.5f;
            float height = 1;
            float cylinderHeight = height - (2 * radius);
            float halfCylinderHeight = cylinderHeight * 0.5f;
            int segments = 24;
            
            // Bottom hemisphere
            for (int ring = segments/2; ring >= 0; ring--)
            {
                float t = Mathf.PI * ring / segments;
                float y = Mathf.Cos(t) * radius - halfCylinderHeight;
                float ringRadius = Mathf.Sin(t) * radius;
                
                for (int i = 0; i <= segments; i++)
                {
                    float angle = 2 * Mathf.PI * i / segments;
                    Vector3 point = new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        y,
                        Mathf.Sin(angle) * ringRadius
                    );
                    buffer.Add(point);
                }
                
                if (ring > 0)
                {
                    buffer.Add(buffer[buffer.Count - 1]);
                }
            }
            
            // Cylinder middle section (vertical lines)
            for (int i = 0; i < 4; i++)
            {
                float angle = 2 * Mathf.PI * i / 4;
                Vector3 bottomPoint = new Vector3(
                    Mathf.Cos(angle) * radius,
                    -halfCylinderHeight,
                    Mathf.Sin(angle) * radius
                );
                Vector3 topPoint = new Vector3(
                    Mathf.Cos(angle) * radius,
                    halfCylinderHeight,
                    Mathf.Sin(angle) * radius
                );
                
                buffer.Add(bottomPoint);
                buffer.Add(topPoint);
            }
            
            // Top hemisphere
            for (int ring = 0; ring <= segments/2; ring++)
            {
                float t = Mathf.PI * ring / segments;
                float y = Mathf.Cos(t) * radius + halfCylinderHeight;
                float ringRadius = Mathf.Sin(t) * radius;
                
                for (int i = 0; i <= segments; i++)
                {
                    float angle = 2 * Mathf.PI * i / segments;
                    Vector3 point = new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        y,
                        Mathf.Sin(angle) * ringRadius
                    );
                    buffer.Add(point);
                }
                
                if (ring < segments/2)
                {
                    buffer.Add(buffer[buffer.Count - 1]);
                }
            }
            
            lineRenderer.positionCount = buffer.Count;
            lineRenderer.SetPositions(buffer.ToArray());
        }
        

        public void SetColor(Color color)
        {
            mat.SetColor("_Color", color);
        }
    }
}