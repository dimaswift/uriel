using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class PhotonBuffer : MonoBehaviour
    {
        private Sky sky;
        private ComputeBuffer buffer;

        private int currentSize;
        
        public PhotonBuffer Init(Sky sky)
        {
            if (sky.photons == null || sky.photons.Count == 0)
            {
                Debug.LogError($"{sky} is empty!");
                return this;
            }
            if (buffer != null) buffer.Release();
            this.sky = sky;
            buffer = new ComputeBuffer(sky.photons.Count, Marshal.SizeOf(typeof(Photon)));
            buffer.SetData(sky.photons);
            currentSize = sky.photons.Count;
            return this;
        }
        
        public PhotonBuffer LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            if (buffer == null) return this;
            shader.SetBuffer(id, ShaderProps.PhotonBuffer, buffer);
            shader.SetInt(ShaderProps.PhotonCount, buffer.count);
            return this;
        }

        public PhotonBuffer LinkMaterial(Material mat)
        {
            if (buffer == null) return this;
            mat.SetBuffer(ShaderProps.PhotonBuffer, buffer);
            mat.SetInt(ShaderProps.PhotonCount, buffer.count);
            return this;
        }

        private void Update()
        {
            if (buffer == null || sky == null)
            {
                return;
            }

            if (currentSize != sky.photons.Count)
            {
                Init(sky);
                return;
            }
            
            buffer.SetData(sky.photons);
        }

        private void OnDestroy()
        {
            buffer?.Release();
        }
    }
}