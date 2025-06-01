using System;
using UnityEngine;
using UnityEngine.Rendering;
using Uriel.Domain;
using Uriel.Utils;
using Object = UnityEngine.Object;

namespace Uriel.Behaviours
{
    public class VolumeWriter : IDisposable
    {
        public RenderTexture Texture => texture;
        
        private Vector3Int dimensions;
        private RenderTexture texture;
        private ComputeShader compute;
        private Vector3Int groups;
        private PhotonBuffer photonBuffer;
        
        
        public VolumeWriter(ComputeShader compute, PhotonBuffer photonBuffer, int x, int y, int z)
        {
            Init(compute, photonBuffer, x, y, z);
        }

        public VolumeWriter(ComputeShader compute, PhotonBuffer photonBuffer, int resolution)
        {
            Init(compute, photonBuffer, resolution, resolution, resolution);
        }
        
        private void Init(ComputeShader compute, PhotonBuffer photonBuffer, int x, int y, int z)
        {
            this.photonBuffer = photonBuffer;
            dimensions = new Vector3Int(x, y, z);
            groups = compute.GetGroups(dimensions, 0);
            this.compute = Object.Instantiate(compute);
            var desc = new RenderTextureDescriptor(dimensions.x, dimensions.y, 0)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = dimensions.z,
                enableRandomWrite = true,
                useMipMap = false,
                graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat
            };
            texture = new RenderTexture(desc);
            texture.Create();
        }

        public void Run(float scale)
        {
            
            compute.SetFloat(ShaderProps.Scale, scale);
            photonBuffer.LinkComputeKernel(compute);
            compute.SetInts(ShaderProps.Dims, dimensions);
            compute.SetTexture(0, ShaderProps.Field, texture);
            compute.Dispatch(0, groups.x, groups.y, groups.z);
        }
        
        public void Dispose()
        {
            if (compute)
            {
                Object.Destroy(compute);
            }
            if (texture != null)
            {
                texture.Release();
                texture = null;
            }
        }
    }
}