using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class ParticleHarbor : MonoBehaviour
    {
        [SerializeField] private float scale = 1;
        [SerializeField] private float radius = 5;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material mat;
        [SerializeField] private int capacity = 64;
        [SerializeField] private Solid type;
        [SerializeField] private float particleSize = 1f;
        [SerializeField] private Harbor harbor;
        [SerializeField] private ComputeShader compute;
        
        private WaveBuffer waveBuffer;
        private ParticleRenderer particleRenderer;
        private ComputeBuffer particlesBuffer;
  
        private int initKernel;
        
        private void Awake()
        {
            initKernel = compute.FindKernel("Init");
            
            
            waveBuffer = gameObject.AddComponent<WaveBuffer>();
            particleRenderer = gameObject.AddComponent<ParticleRenderer>();

            particleRenderer.Init(mesh, mat, capacity)
                .LinkComputeKernel(compute, initKernel);

            waveBuffer.Init(harbor)
                .LinkComputeKernel(compute, initKernel);
            
            particleRenderer.Randomize(radius, particleSize);
        }
        
        

        private void Update()
        {
            compute.SetFloat(ShaderProps.Scale, scale);
            compute.SetFloat(ShaderProps.Size, particleSize);
            compute.SetInt(ShaderProps.SolidType, (int) type);
            compute.Dispatch(initKernel, Mathf.CeilToInt(capacity / 64f), 1, 1);
         
        }
    }
}