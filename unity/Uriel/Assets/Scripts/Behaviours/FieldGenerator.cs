using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class FieldGenerator 
    {
        public ComputeShader ComputeInstance => computeShader;
        
        private ComputeShader computeShader;
        private int kernelIndex;
        
        public RenderTexture Field { get; private set; }
        
        // Shader property IDs for better performance
        private static readonly int FieldPropertyId = Shader.PropertyToID("_Field");
        private static readonly int DimsPropertyId = Shader.PropertyToID("_Dims");
        private static readonly int SaturatePropertyId = Shader.PropertyToID("_Saturate");

        private ComputeBuffer sourcesBuffer;
        
        public FieldGenerator(ComputeShader compute, Vector3Int dimensions)
        {
            Initialize(compute, dimensions);
        }
        
        private void Initialize(ComputeShader compute, Vector3Int dimensions)
        {
            computeShader = Object.Instantiate(compute);
            kernelIndex = computeShader.FindKernel("Run");
            
            if (kernelIndex < 0)
            {
                Debug.LogError("Compute shader kernel 'Run' not found!");
                return;
            }
            
            // Validate inputs
            if (dimensions.x <= 0 || dimensions.y <= 0 || dimensions.z <= 0)
            {
                Debug.LogError($"Invalid dimensions: {dimensions}. All dimensions must be positive.");
                return;
            }

            CreateFieldTexture(dimensions);
        }
        
        private void CreateFieldTexture(Vector3Int dimensions)
        {
            Field = new RenderTexture(dimensions.x, dimensions.y, 0, RenderTextureFormat.RFloat)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = dimensions.z,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            
            Field.Create();
        }
        
        private void GenerateField(Vector3Int dimensions, FieldConfig config)
        {
            computeShader.SetTexture(kernelIndex, FieldPropertyId, Field);
            computeShader.SetInts(DimsPropertyId, dimensions.x, dimensions.y, dimensions.z);
            computeShader.SetBool(SaturatePropertyId, config.saturate);
            // Dispatch shader
            DispatchShader(dimensions.x, dimensions.y, dimensions.z);
        }
        
        private void DispatchShader(int width, int height, int depth)
        {
            // Calculate thread groups (shader uses 4x4x4 threads per group)
            int threadGroupsX = Mathf.CeilToInt(width / 4f);
            int threadGroupsY = Mathf.CeilToInt(height / 4f);
            int threadGroupsZ = Mathf.CeilToInt(depth / 4f);
            
            computeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
        }
        
        /// <summary>
        /// Regenerate the field with new parameters
        /// </summary>
        public void Run(FieldConfig config)
        {
            if (Field != null)
            {
                Vector3Int dimensions = new Vector3Int(Field.width, Field.height, Field.volumeDepth);
                GenerateField(dimensions, config);
            }
        }

        public void SetSources(List<WaveSource> sources)
        {
            if (sourcesBuffer != null && sourcesBuffer.count != sources.Count)
            {
                sourcesBuffer.Release();
                sourcesBuffer = null;
            }

            if (sourcesBuffer != null)
            {
                sourcesBuffer.SetData(sources);
                return;
            }

            sourcesBuffer = new ComputeBuffer(sources.Count, Marshal.SizeOf(typeof(WaveSource)));
            
            computeShader.SetBuffer(kernelIndex, "_Sources", sourcesBuffer);
            computeShader.SetInt("_SourceCount", sourcesBuffer.count);
            sourcesBuffer.SetData(sources);
            
        }
        
        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (Field != null)
            {
                Field.Release();
                Object.DestroyImmediate(Field);
                Field = null;
            }
            
            sourcesBuffer?.Release();

            Object.Destroy(computeShader);
        }
        
        ~FieldGenerator()
        {
            Dispose();
        }
    }
}

