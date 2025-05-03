using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class WaveBuffer : MonoBehaviour
    {
        private Harbor harbor;
        private ComputeBuffer buffer;

        private int currentSize;
        
        public WaveBuffer Init(Harbor harbor)
        {
            if (buffer != null) buffer.Release();
            this.harbor = harbor;
            buffer = new ComputeBuffer(harbor.waves.Count, Marshal.SizeOf(typeof(Wave)));
            buffer.SetData(harbor.waves);
            currentSize = harbor.waves.Count;
            return this;
        }

        public WaveBuffer LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            if (buffer == null) return this;
            shader.SetBuffer(id, ShaderProps.WaveBuffer, buffer);
            shader.SetInt(ShaderProps.WaveCount, buffer.count);
            return this;
        }

        public WaveBuffer LinkMaterial(Material mat)
        {
            if (buffer == null) return this;
            mat.SetBuffer(ShaderProps.WaveBuffer, buffer);
            mat.SetInt(ShaderProps.WaveCount, buffer.count);
            return this;
        }

        private void Update()
        {
            if (buffer == null || harbor == null)
            {
                return;
            }

            if (currentSize != harbor.waves.Count)
            {
                Init(harbor);
                return;
            }
            
            buffer.SetData(harbor.waves);
        }

        private void OnDestroy()
        {
            buffer?.Release();
        }
    }
}