using System;
using Unity.Mathematics;
using UnityEngine;

namespace Uriel.Behaviours
{
    public class CelestialSphere : MonoBehaviour
    {
        [SerializeField] private float threshold;
        [Range(0f, 1f)] [SerializeField] private float angle = 1;
        [SerializeField] private int phase = 1;
        [SerializeField] private float density = 1;
        [SerializeField] private float frequency;
        [Range(0f, 1f)] [SerializeField] private float cubeSize = 0.1f;
        [SerializeField] private Matrix4x4 config;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float speed = 1;
        [SerializeField] private int resolution = 64;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Material material;
        [SerializeField] Mesh mesh;
        
        private ComputeBuffer meshBuffer;
        private ComputeBuffer particlesBuffer;
        private ComputeBuffer cellsBuffer;
        private int kernel;
        private int ResolutionCubed => resolution * resolution * resolution;
     
        

        public struct Cell
        {
            public float4x4 data;
            
            public static int Size()
            {
                return sizeof(float) * 4 * 4;
            }
        }
        
        private void Start()
        {
            kernel = compute.FindKernel("CSMain");
            particlesBuffer = new ComputeBuffer(ResolutionCubed, sizeof(float) * 4 * 4);
            cellsBuffer = new ComputeBuffer(ResolutionCubed, sizeof(float) * 4 * 4);
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)ResolutionCubed;
            meshBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            meshBuffer.SetData(args);
            material.SetBuffer("Particles", particlesBuffer);
            compute.SetBuffer(kernel, "Particles", particlesBuffer);
        }

        private void Update()  
        {
            compute.SetMatrix("Config", config);
            
            compute.SetFloat("Time", Time.time * speed);
            compute.SetFloat("Threshold", threshold);
            compute.SetFloat("Density", density);
            compute.SetInt("Resolution", resolution);
            compute.SetInt("Phase", phase);
            compute.SetFloat("Frequency", frequency);
            compute.SetFloat("Angle", angle);
            compute.SetVector("Offset", offset);
            compute.SetFloat("Size", cubeSize);
            compute.Dispatch(0, Mathf.CeilToInt(ResolutionCubed / 64.0f), 1, 1);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material,
                new Bounds(transform.position, Vector3.one * (float.MaxValue)), meshBuffer);
        } 

        private void OnDestroy() 
        {
            particlesBuffer?.Release();
            particlesBuffer = null;
            meshBuffer?.Release();
            meshBuffer = null;
        }
    }

}
