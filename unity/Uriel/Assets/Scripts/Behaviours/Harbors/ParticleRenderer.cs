using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Uriel.Domain;
using Random = UnityEngine.Random;

namespace Uriel.Behaviours
{
    public class ParticleRenderer : MonoBehaviour
    {
        private ComputeBuffer particlesBuffer;
        private ComputeBuffer meshBuffer;
        private Mesh mesh;
        private Material mat;

        private float4x4[] particlesList;

        public ParticleRenderer Init(Mesh mesh, Material mat, int capacity)
        {
            
            this.mesh = mesh;
            particlesList = new float4x4[capacity];
            particlesBuffer = new ComputeBuffer(capacity, sizeof(float) * 4 * 4);
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)capacity;
            meshBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            meshBuffer.SetData(args);
            this.mat = mat;
            return LinkMaterial(mat);
        }

        public ParticleRenderer LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            shader.SetBuffer(id, ShaderProps.Particles, particlesBuffer);
            shader.SetInt(ShaderProps.ParticlesCount, particlesBuffer.count);
            return this;
        }

        public void Randomize(float radius, float size)
        {
            for (int i = 0; i < particlesList.Length; i++)
            {
                var p = Random.insideUnitSphere * radius;
                particlesList[i] = new (size, 0, 0, p.x, 
                                                0, size, 0, p.y, 
                                                0, 0, size, p.z,
                                                0, 0, 0, 1);
            }
            particlesBuffer.SetData(particlesList);
        }
        
        public ParticleRenderer LinkMaterial(Material material)
        {
            material.SetBuffer(ShaderProps.Particles, particlesBuffer);
            material.SetInt(ShaderProps.ParticlesCount, particlesBuffer.count);
            return this;
        }

        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, mat,
                new Bounds(transform.position, Vector3.one * (float.MaxValue)), meshBuffer);
        }

        private void OnDestroy()
        {
            particlesBuffer?.Release();
            meshBuffer?.Release();
        }
    }
}