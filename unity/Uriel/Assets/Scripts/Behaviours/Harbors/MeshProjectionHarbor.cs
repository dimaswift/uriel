using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class MeshProjectionHarbor : MonoBehaviour
    {
        [SerializeField] private MeshHarbor meshHarbor;
        [SerializeField] private Harbor harbor;
        [SerializeField] private Material material;

 
        private ComputeBuffer waveBuffer;
        
        private void Start()
        {

            var buff = meshHarbor.GetOutputVertexBuffer();
            
            material.SetBuffer("_VertexBuffer", buff);
            material.SetInt("_VertexCount", buff.count);
            waveBuffer = new ComputeBuffer(harbor.waves.Count, Marshal.SizeOf(typeof(Wave)));
            waveBuffer.SetData(harbor.waves);
            material.SetInt(ShaderProps.WaveCount, harbor.waves.Count);
            material.SetBuffer(ShaderProps.WaveBuffer, waveBuffer);
        }

        private void Update()
        {
            waveBuffer.SetData(harbor.waves);
        }

        private void OnDestroy()
        {
            waveBuffer?.Release();
        }
    }
}
