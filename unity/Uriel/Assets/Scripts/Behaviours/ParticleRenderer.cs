using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;
using Random = UnityEngine.Random;

namespace Uriel.Behaviours
{
    public class ParticleRenderer : MonoBehaviour
    {
        [SerializeField] private int capacity;
        [SerializeField] private bool initOnAwake;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;

        private Particle[] particlesList;
        private ComputeBuffer particlesBuffer;
        private ComputeBuffer meshBuffer;
        
        private void Awake()
        {
            if (!initOnAwake) return;
            Init();
        }
        
        public ParticleRenderer SetUp(Mesh mesh, Material material, int capacity)
        {
            this.material = material;
            this.mesh = mesh;
            this.capacity = capacity;
            return this;
        }

        public ParticleRenderer SetUp(int capacity)
        {
            this.capacity = capacity;
            return this;
        }
        
        public ParticleRenderer Init()
        {
            if (material == null || mesh == null)
            {
                Debug.LogError($"Particle Renderer '{name}' is missing mesh or material");
                return this;
            }

            if (capacity <= 0)
            {
                Debug.LogError($"Particle Renderer '{name}' has zero capacity");
                return this;
            }   
            
            CleanUp();
            particlesList = new Particle[capacity];
            particlesBuffer = new ComputeBuffer(capacity, Marshal.SizeOf(typeof(Particle)));
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)capacity;
            meshBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            meshBuffer.SetData(args);
            return LinkMaterial(material);
        }

        private void CleanUp()
        {
            particlesBuffer?.Release();
            meshBuffer?.Release();
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
                var pos = Random.insideUnitSphere * radius;
                particlesList[i] = new Particle()
                {
                    position = pos,
                    size = size,
                    charge = 0,
                    mass = 1f
                };
            }
            particlesBuffer.SetData(particlesList);
        }

        public void ArrangeInACube(int sideCount, float radius, float size)
        {
            int i = 0;
            int half = sideCount / 2;
            for (int x = -half; x < half; x++)
            {
                for (int y = -half; y < half; y++)
                {
                    for (int z = -half; z < half; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z) * radius;
                        particlesList[i++] = new Particle()
                        {
                            position = pos,
                            size = size,
                            charge = 0,
                            mass = 1f
                        };
                    }
                }
            }
          
            particlesBuffer.SetData(particlesList);
        }

        
        public ParticleRenderer LinkMaterial(Material material)
        {
            material.SetBuffer(ShaderProps.Particles, particlesBuffer);
            material.SetInt(ShaderProps.ParticlesCount, particlesBuffer.count);
            return this;
        }

        public void Draw()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material,
                new Bounds(transform.position, Vector3.one * (float.MaxValue)), meshBuffer);
        }

        private void OnDestroy()
        {
            CleanUp();
        }

        public Material GetMaterial()
        {
            return material;
        }
    }
}