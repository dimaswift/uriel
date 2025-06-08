using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Uriel.Domain;
using Uriel.Utils;
using Object = UnityEngine.Object;

namespace Uriel.Behaviours
{
    public class RecursiveVolumeWriter : IDisposable
    {
        public RenderTexture Texture => texture;
        
        private Vector3Int dimensions;
        private RenderTexture texture;
        private RenderTexture core;
        private ComputeShader compute;
        private Vector3Int groups;
        private PhotonBuffer photonBuffer;

        private int hash;
        private float appliedScale;
        private readonly List<Photon> appliedPhotons = new();
        
        public RecursiveVolumeWriter(ComputeShader compute, PhotonBuffer photonBuffer, int x, int y, int z, RenderTexture core, FilterMode filterMode = FilterMode.Bilinear)
        {
            Init(compute, photonBuffer, x, y, z, core, filterMode);
        }

        public RecursiveVolumeWriter(ComputeShader compute, PhotonBuffer photonBuffer, RenderTexture core, int resolution, FilterMode filterMode = FilterMode.Bilinear)
        {
            Init(compute, photonBuffer, resolution, resolution, resolution, core, filterMode);
        }
        
        private void Init(ComputeShader computeSource, PhotonBuffer buffer, int x, int y, int z, RenderTexture coreTexture, FilterMode filterMode = FilterMode.Bilinear)
        {
            core = coreTexture;
            photonBuffer = buffer;
            dimensions = new Vector3Int(x, y, z);
            groups = computeSource.GetGroups(dimensions, 0);
            compute = Object.Instantiate(computeSource);
            var desc = new RenderTextureDescriptor(dimensions.x, dimensions.y, 0)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = dimensions.z,
                enableRandomWrite = true,
                useMipMap = false,
                graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,
            };
            
            texture = new RenderTexture(desc);
            texture.filterMode = filterMode;
            texture.Create();
        }

        public void Run(float scale)
        {
            // bool changed = false;
            //
            // if (!Mathf.Approximately(appliedScale, scale))
            // {
            //     changed = true;
            //     appliedScale = scale;
            // }
            //
            // var count = photonBuffer.Buffer.photons.Count;
            //
            // while (appliedPhotons.Count < count)
            // {
            //     appliedPhotons.Add(new Photon());
            // }
            //
            // for (int i = 0; i < count; i++)
            // {
            //     var current = photonBuffer.Buffer.photons[i];
            //     var applied = appliedPhotons[i];
            //     if (!Equals(current, applied))
            //     {
            //         appliedPhotons[i] = current;
            //         changed = true;
            //     }
            // }
            //
            // if (!changed)
            // {
            //     return;
            // }
            
            compute.SetFloat(ShaderProps.Scale, scale);
            photonBuffer.LinkComputeKernel(compute);
            compute.SetInts(ShaderProps.Dims, dimensions);
            compute.SetInts(ShaderProps.CoreDimensions, new Vector3Int(core.width, core.height, core.volumeDepth));
            compute.SetTexture(0, ShaderProps.Field, texture);
            compute.SetTexture(0, ShaderProps.Core, core);
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