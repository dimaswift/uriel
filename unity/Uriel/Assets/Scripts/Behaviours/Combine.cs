
using UnityEngine;

namespace Uriel.Behaviours
{
    [System.Serializable]
    public struct CombineParameter
    {
        public Vector3Int offset;
        public float falloff;
        public LayerOperation operation;
        
        public CombineParameter(Vector3Int offset = default, float falloff = 1f, LayerOperation operation = LayerOperation.Multiply)
        {
            this.offset = offset;
            this.falloff = falloff;
            this.operation = operation;
        }
    }
    
    public enum LayerOperation
    {
        Multiply = 0,
        Add = 1,
        Subtract = 2
    }
    
    public class Combine 
    {
        private ComputeShader computeShader;
        private int kernelIndex;
        
        public RenderTexture Result { get; private set; }
        
        private static readonly int ResultPropertyId = Shader.PropertyToID("_Result");
        private static readonly int DimsPropertyId = Shader.PropertyToID("_Dims");
        private static readonly int LayerCountPropertyId = Shader.PropertyToID("_LayerCount");
        
        
        public Combine(ComputeShader compute, RenderTexture[] layers)
        {
            Initialize(compute, layers);
        }
        
        private void Initialize(ComputeShader compute, RenderTexture[] layers)
        {
            if (layers.Length < 2)
            {
                Debug.LogError("Need more than 1 layer");
                return;
            }
            
            computeShader = compute;
            kernelIndex = computeShader.FindKernel("Run");
            
            if (kernelIndex < 0)
            {
                Debug.LogError("Compute shader kernel 'Run' not found!");
                return;
            }
            
            
            if (layers.Length > 3)
            {
                Debug.LogWarning($"Only first 3 layers will be processed. Provided: {layers.Length}");
            }
            
            // Create result texture with same format as base
            CreateResultTexture(layers[0]);
            for (int i = 0; i < layers.Length; i++)
            {
                computeShader.SetTexture(kernelIndex, $"_Layer_{i}", layers[i]);
               
               
            }
            // Set up and dispatch shader
            computeShader.SetTexture(kernelIndex, ResultPropertyId, Result);
            
            // Set dimensions
            computeShader.SetInts(DimsPropertyId, Result.width, Result.height, Result.volumeDepth);
          
            computeShader.SetInt(LayerCountPropertyId, layers.Length);

        }
        
        private void CreateResultTexture(RenderTexture baseMap)
        {
            Result = new RenderTexture(baseMap.width, baseMap.height, 0, baseMap.format)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = baseMap.volumeDepth,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            
            Result.Create();
        }
        

        private void SetLayerParam(int layerIndex, CombineParameter param)
        {
            computeShader.SetInts($"_Offset_{layerIndex}", param.offset.x, param.offset.y, param.offset.z);
            computeShader.SetFloat($"_Falloff_{layerIndex}", param.falloff);
            computeShader.SetInt($"_Operation_{layerIndex}", (int) param.operation);
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
        /// Update specific layer parameters without recreating the result texture
        /// </summary>
        public void UpdateLayer(int layerIndex, CombineParameter newParams)
        {
            if (layerIndex < 0 || layerIndex > 2)
            {
                Debug.LogError($"Layer index {layerIndex} out of range [0-2]");
                return;
            }
            
            SetLayerParam(layerIndex, newParams);
        }
        
        /// <summary>
        /// Re-dispatch the shader with current parameters
        /// </summary>
        public void Run()
        {
            if (Result != null)
            {
                DispatchShader(Result.width, Result.height, Result.volumeDepth);
            }
        }
        
        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (Result != null)
            {
                Result.Release();
                Object.DestroyImmediate(Result);
                Result = null;
            }
        }
        
        ~Combine()
        {
            Dispose();
        }
    }
}
