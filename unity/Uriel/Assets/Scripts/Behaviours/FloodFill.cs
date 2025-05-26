
using UnityEngine;
using UnityEngine.Rendering;

namespace Uriel.Behaviours
{
    public class FloodFill : System.IDisposable
    {
        private ComputeShader compute;
        private int sampleFieldKernel;
        private int placeSeedKernel;
        private int floodFillStepKernel;
        private int applyMaskKernel;
        
        private RenderTexture fieldTexture;
        private RenderTexture componentTexture;
        private RenderTexture tempTexture;
        private RenderTexture maskedFieldTexture;
        
        private ComputeBuffer componentSize;
        private ComputeBuffer propagationFlag;
        private ComputeBuffer seedPosition;
        
        private (int x, int y, int z) dimensions;
        private const int MAX_ITERATIONS = 1000;
        
        public RenderTexture FieldTexture => fieldTexture;
        public RenderTexture ComponentTexture => componentTexture;
        public RenderTexture MaskedFieldTexture => maskedFieldTexture;
        
        public FloodFill(ComputeShader floodFillCompute, int x, int y, int z)
        {
            compute = floodFillCompute;
            dimensions = (x, y, z);
            
            // Find kernels
            sampleFieldKernel = compute.FindKernel("SampleField");
            placeSeedKernel = compute.FindKernel("PlaceSeed");
            floodFillStepKernel = compute.FindKernel("FloodFillStep");
            applyMaskKernel = compute.FindKernel("ApplyComponentMask");
            
            AllocateResources();
        }
        
        /// <summary>
        /// Process the field with flood fill from a single seed point
        /// </summary>
        /// <param name="isovalue">Threshold value for solid voxels</param>
        /// <param name="seed">Seed point</param>
        public void Run(float isovalue, Vector3 seed)
        {
            // Convert degrees to radians
   
            // Set common parameters
            compute.SetInts("Dims", dimensions.x, dimensions.y, dimensions.z);
            compute.SetFloat("Isovalue", isovalue);
            compute.SetVector("Seed", seed);
       
            // Clear buffers
            componentSize.SetData(new uint[] { 0 });
            seedPosition.SetData(new uint[] { 0, 0, 0 });
            
            // Step 1: Sample the field
        
            SampleField();
            
            // Step 2: Place seed
           
            PlaceSeed();
            
            // Get actual seed position for debugging
            uint[] seedPosData = new uint[3];
            seedPosition.GetData(seedPosData);
        
            // Step 3: Flood fill iteration
            PerformFloodFill();
            
            // Step 4: Apply mask
         
            ApplyComponentMask();
            
            // Debug output
            uint[] sizeData = new uint[1];
            componentSize.GetData(sizeData);
            float percentage = (float)sizeData[0] / (dimensions.x * dimensions.y * dimensions.z) * 100f;
           
        }
        
        
        private void SampleField()
        {
            compute.SetTexture(sampleFieldKernel, "FieldTexture", fieldTexture);
            compute.SetTexture(sampleFieldKernel, "ComponentTexture", componentTexture);
            compute.SetTexture(sampleFieldKernel, "TempTexture", tempTexture);
            compute.SetTexture(sampleFieldKernel, "MaskedFieldTexture", maskedFieldTexture);
            
            compute.DispatchThreads(sampleFieldKernel, dimensions.x, dimensions.y, dimensions.z);
        }
        
        private void PlaceSeed()
        {
            compute.SetBuffer(placeSeedKernel, "ComponentSize", componentSize);
            compute.SetBuffer(placeSeedKernel, "PropagationFlag", propagationFlag);
            compute.SetBuffer(placeSeedKernel, "SeedPosition", seedPosition);
            compute.SetTexture(placeSeedKernel, "FieldTexture", fieldTexture);
            compute.SetTexture(placeSeedKernel, "ComponentTexture", componentTexture);
            compute.SetTexture(placeSeedKernel, "TempTexture", tempTexture);
            
            compute.Dispatch(placeSeedKernel, 1, 1, 1);
        }
        
        private void PerformFloodFill()
        {
            compute.SetBuffer(floodFillStepKernel, "ComponentSize", componentSize);
            compute.SetBuffer(floodFillStepKernel, "PropagationFlag", propagationFlag);
            compute.SetTexture(floodFillStepKernel, "FieldTexture", fieldTexture);
            compute.SetTexture(floodFillStepKernel, "ComponentTexture", componentTexture);
            compute.SetTexture(floodFillStepKernel, "TempTexture", tempTexture);
            
            for (int i = 0; i < MAX_ITERATIONS; i++)
            {
                // Clear propagation flag
                propagationFlag.SetData(new uint[] { 0 });
                
                // Propagate
                compute.DispatchThreads(floodFillStepKernel, dimensions.x, dimensions.y, dimensions.z);
                
                // Check if any propagation occurred
                uint[] flagData = new uint[1];
                propagationFlag.GetData(flagData);

                if (flagData[0] == 0)
                {
                   
                    break;
                }
            }

        }
        
        private void ApplyComponentMask()
        {
            compute.SetTexture(applyMaskKernel, "FieldTexture", fieldTexture);
            compute.SetTexture(applyMaskKernel, "ComponentTexture", componentTexture);
            compute.SetTexture(applyMaskKernel, "MaskedFieldTexture", maskedFieldTexture);
            
            compute.DispatchThreads(applyMaskKernel, dimensions.x, dimensions.y, dimensions.z);
        }
        
        private void AllocateResources()
        {
            // Create 3D textures
            var desc = new RenderTextureDescriptor(dimensions.x, dimensions.y, 0)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = dimensions.z,
                enableRandomWrite = true,
                useMipMap = false
            };
            
            // Field texture (float)
            desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
            fieldTexture = new RenderTexture(desc);
            fieldTexture.Create();
            
            maskedFieldTexture = new RenderTexture(desc);
            maskedFieldTexture.Create();
            
            // Component texture (uint)
            desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt;
            componentTexture = new RenderTexture(desc);
            componentTexture.Create();
            
            tempTexture = new RenderTexture(desc);
            tempTexture.Create();
            
            // Create buffers
            componentSize = new ComputeBuffer(1, sizeof(uint));
            propagationFlag = new ComputeBuffer(1, sizeof(uint));
            seedPosition = new ComputeBuffer(3, sizeof(uint)); // x, y, z position
        }
        
        public void Dispose()
        {
            fieldTexture?.Release();
            componentTexture?.Release();
            tempTexture?.Release();
            maskedFieldTexture?.Release();
            
            componentSize?.Dispose();
            propagationFlag?.Dispose();
            seedPosition?.Dispose();
        }
    }
    
    // Extension helper for dispatch
    public static class ComputeShaderExtensions
    {
        public static void DispatchThreads(this ComputeShader compute, int kernel, int x, int y, int z)
        {
            compute.GetKernelThreadGroupSizes(kernel, out uint gx, out uint gy, out uint gz);
            compute.Dispatch(kernel, (x + (int)gx - 1) / (int)gx, (y + (int)gy - 1) / (int)gy, (z + (int)gz - 1) / (int)gz);
        }
    }
}
