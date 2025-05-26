using System;
using System.IO;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(PhotonBuffer))]
    public class MeshEcho : MonoBehaviour
    {
        [SerializeField] private string saveDirectory;
        [SerializeField] private bool calculateNormals;
        [SerializeField] private MeshRenderer target;
        [SerializeField] private ComputeShader compute;
  
        [SerializeField] private float frequency = 1;
        [SerializeField] private float amplitude = 1;
        [SerializeField] private float scale = 1;
        [SerializeField] private float phase = 1;
        [SerializeField] private float speed = 1;
        [SerializeField] private float min = 0.5f;
        [SerializeField] private float max = 1;
        [SerializeField] private int steps = 1;
        
        private ComputeBuffer inputVertexBuffer;
        private ComputeBuffer outputVertexBuffer;
        private ComputeBuffer normalBuffer;
      
        
        private Mesh currentMesh;
        private Vector3[] vertices;
        private Vector3[] normals;
        private Material mat;

        public ComputeBuffer GetOutputVertexBuffer() => outputVertexBuffer;
        public ComputeBuffer GetNormalBuffer() => normalBuffer;

        private void Awake()
        {
            currentMesh = target.GetComponent<MeshFilter>().mesh;
            mat = target.material;
            gameObject.GetComponent<PhotonBuffer>()
                .LinkMaterial(mat)
                .LinkComputeKernel(compute);
            GenerateBuffers();
        }

        private void GenerateBuffers()
        {
            if (inputVertexBuffer != null) 
                inputVertexBuffer.Release();
            if (normalBuffer != null) 
                normalBuffer.Release();
            if (outputVertexBuffer != null)
                outputVertexBuffer.Release();
            
            vertices = currentMesh.vertices;
            normals = currentMesh.normals;

            inputVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
            normalBuffer = new ComputeBuffer(normals.Length, sizeof(float) * 3);
            outputVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
            compute.SetBuffer(0, "_InputVertexBuffer", inputVertexBuffer);
            compute.SetBuffer(0, "_NormalBuffer", normalBuffer);
            compute.SetBuffer(0, "_OutputVertexBuffer", outputVertexBuffer);
            inputVertexBuffer.SetData(vertices);
            normalBuffer.SetData(normals);
          
        }

        private void Update()
        {
            if (inputVertexBuffer == null)
            {
                return;
            }
            
            compute.SetFloat(ShaderProps.Frequency, frequency);
            compute.SetFloat(ShaderProps.Amplitude, amplitude);
        
            compute.SetFloat(ShaderProps.Speed, speed);
            compute.SetFloat(ShaderProps.Time, Time.time);
            compute.SetFloat(ShaderProps.Phase, phase);
            compute.SetFloat(ShaderProps.Scale, scale);
            compute.SetFloat(ShaderProps.Min, min);
            compute.SetFloat(ShaderProps.Max, max);
            compute.SetInt(ShaderProps.Steps, steps);
            
            compute.Dispatch(0, Mathf.CeilToInt(inputVertexBuffer.count / 64f), 1, 1);
            outputVertexBuffer.GetData(vertices);
            currentMesh.SetVertices(vertices);

            
            if (calculateNormals)
            {
                currentMesh.RecalculateNormals();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                FileUtils.ExportMeshToASCIISTL(currentMesh, Path.Combine(Application.dataPath, "/Generated/STL/" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper()));
            }
        }

        
        private void OnDestroy()
        {
            if (inputVertexBuffer != null) inputVertexBuffer.Release();
            if (normalBuffer != null) normalBuffer.Release();
            if (outputVertexBuffer != null) outputVertexBuffer.Release();
        }
    }
}