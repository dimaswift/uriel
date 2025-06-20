using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Uriel.Rendering
{
    public class WireframeRenderer : MonoBehaviour
    {
        public Color wireColor = Color.white;
        public bool useWorldSpace = true;
        public int maxLinesPerFrame = 1000;
        private Material lineMaterial;

        private readonly List<Vector3> vertices = new();
        private readonly List<int> edges = new();
        
        private void Start()
        {
            CreateMaterials();
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }
        
        private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            Render(cam);
        }
        
        private void CreateMaterials()
        {
            lineMaterial = CreateLineMaterial("Hidden/Internal-Colored");
        }
        
        private Material CreateLineMaterial(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                shader = Shader.Find("UI/Default");
            Material mat = new Material(shader);
            mat.hideFlags = HideFlags.HideAndDontSave;
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            return mat;
        }
        
        
        private void Render(Camera cam)
        {
            GL.PushMatrix();
            
            if (useWorldSpace)
                GL.MultMatrix(transform.localToWorldMatrix);
            else
                GL.LoadProjectionMatrix(cam.projectionMatrix);
            
            lineMaterial.SetPass(0);

            GL.wireframe = false;
            
            GL.Begin(GL.LINES);
            GL.Color(wireColor);
            
            int linesToRender = Mathf.Min(edges.Count / 2, maxLinesPerFrame);
            
            for (int i = 0; i < linesToRender * 2; i += 2)
            {
                if (i + 1 < edges.Count)
                {
                    int startIdx = edges[i];
                    int endIdx = edges[i + 1];
                    
                    if (startIdx < vertices.Count && endIdx < vertices.Count)
                    {
                        GL.Vertex3(vertices[startIdx].x, vertices[startIdx].y, vertices[startIdx].z);
                        GL.Vertex3(vertices[endIdx].x, vertices[endIdx].y, vertices[endIdx].z);
                    }
                }
            }
            
            GL.End();
            GL.PopMatrix();
        }
        
        public void Set(IEnumerable<Vector3> newVertices, IEnumerable<int> newEdges)
        {
            Clear();
            vertices.AddRange(newVertices);
            edges.AddRange(newEdges);
        }
        
        public void Clear()
        {
            vertices.Clear();
            edges.Clear();
        }
        
        void OnDestroy()
        {
            if (lineMaterial != null) DestroyImmediate(lineMaterial);
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }
    }
}
