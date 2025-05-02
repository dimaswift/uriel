using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    
    public class MeshHarbor : MonoBehaviour
    {
        [SerializeField] private string saveDirectory;
        [SerializeField] private bool calculateNormals;
        [SerializeField] private MeshRenderer target;
        [SerializeField] private Harbor harbor;
        [SerializeField] private ComputeShader compute;
  
        [SerializeField] private float frequency = 1;
        [SerializeField] private float amplitude = 1;
        [SerializeField] private float scale = 1;
        [SerializeField] private float phase = 1;
        [SerializeField] private float speed = 1;
        
        private ComputeBuffer inputVertexBuffer;
        private ComputeBuffer outputVertexBuffer;
        private ComputeBuffer normalBuffer;
        private ComputeBuffer waveBuffer;
        
        private Mesh currentMesh;
        private Vector3[] vertices;
        private Vector3[] normals;
        private Material mat;

        public ComputeBuffer GetOutputVertexBuffer() => outputVertexBuffer;
        
        private void Awake()
        {
            currentMesh = target.GetComponent<MeshFilter>().mesh;
            mat = target.material;
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

        private void UpdateWaveBuffer()
        {
            if (waveBuffer != null && waveBuffer.count == harbor.waves.Count)
            {
                waveBuffer.SetData(harbor.waves);
                return;
            }

            if (harbor.waves.Count == 0)
            {
                return;
            }
            
            if (waveBuffer != null) waveBuffer.Release();

            waveBuffer = new ComputeBuffer(harbor.waves.Count, Marshal.SizeOf(typeof(Wave)));
            compute.SetBuffer(0, ShaderProps.WaveBuffer, waveBuffer);
            compute.SetInt(ShaderProps.WaveCount, harbor.waves.Count);
            mat.SetBuffer(ShaderProps.WaveBuffer, waveBuffer);
            mat.SetInt(ShaderProps.WaveCount, harbor.waves.Count);
            waveBuffer.SetData(harbor.waves);
        }

        private void Update()
        {
            if (inputVertexBuffer == null)
            {
                return;
            }

            UpdateWaveBuffer();
            
            compute.SetFloat(ShaderProps.Frequency, frequency);
            compute.SetFloat(ShaderProps.Amplitude, amplitude);
        
            compute.SetFloat(ShaderProps.Speed, speed);
            compute.SetFloat(ShaderProps.Time, Time.time);
            compute.SetFloat(ShaderProps.Phase, phase);
            compute.SetFloat(ShaderProps.Scale, scale);
            compute.Dispatch(0, Mathf.CeilToInt(inputVertexBuffer.count / 64f), 1, 1);
            outputVertexBuffer.GetData(vertices);
            currentMesh.SetVertices(vertices);

            
            if (calculateNormals)
            {
                currentMesh.RecalculateNormals();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                FileUtils.ExportMeshToASCIISTL(currentMesh, Path.Combine(saveDirectory, "/Generated/STL/" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper()));
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