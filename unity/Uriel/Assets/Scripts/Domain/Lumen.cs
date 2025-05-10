using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Lumen")]
    public class Lumen : ScriptableObject
    {
        public List<Photon> photons = new();

        private ComputeBuffer buffer;

        public void LinkMaterial(Material material)
        {
            if (buffer == null)
            {
                if (!CreateBuffer()) return;
            }
            material.SetBuffer(ShaderProps.PhotonBuffer, buffer);
            material.SetInt(ShaderProps.PhotonCount, buffer.count);
        }

        public void LinkComputeKernel(ComputeShader computeShader, int kernelIndex = 0)
        {
            if (buffer == null) 
            {
                if (!CreateBuffer()) return;
            }
            computeShader.SetBuffer(kernelIndex, ShaderProps.PhotonBuffer, buffer);
            computeShader.SetInt(ShaderProps.PhotonCount, buffer.count);
        }

        public void UpdateTransform(Matrix4x4 m)
        {
            for (int i = 0; i < photons.Count; i++)
            {
                Photon p = photons[i];
                p.transform = m;
                photons[i] = p;
            }
        }

        
        public void Update()
        {
            if (buffer == null)
            {
                if (!CreateBuffer()) return;
            }
            if (buffer.count != photons.Count)
            {
                if (!CreateBuffer()) return;
            }
            
            
            buffer.SetData(photons);
        }

        public void EnsureBufferExists()
        {
            if (buffer != null) return;
            buffer = new ComputeBuffer(photons.Count, Marshal.SizeOf(typeof(Photon)));
            buffer.SetData(photons);
        }
        
        public bool CreateBuffer()
        {
            DisposeBuffer();
            if (photons.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < photons.Count; i++)
            {
                Photon p = photons[i];
                if (p.transform == Matrix4x4.zero)
                {
                    p.transform = Matrix4x4.identity;
                    photons[i] = p;
                }
            }
            
            buffer = new ComputeBuffer(photons.Count, Marshal.SizeOf(typeof(Photon)));
            buffer.SetData(photons);
            
            return true;
        }
        
        public void DisposeBuffer()
        {
            if (buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
        }

        private void OnDisable()
        {
            DisposeBuffer();
        }

        private void OnDestroy()
        {
            DisposeBuffer();
        }
    }
}