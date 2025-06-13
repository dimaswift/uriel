using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public enum FieldType
    {
        InterferenceField = 0,
        Ellipsoid = 1,
        Rectangle = 2,
        Donut = 3,
        Sphere = 4,
        RoundedBox = 5,
        Cylinder = 6,
        Capsule = 7,
        Cone = 8,
        VerticalTorus = 9,
        UnionSphereBox = 10,
        SmoothUnionSphereBox = 11,
        SubtractionSphereBox = 12,
        RepeatedSpheres = 13,
        TwistedBox = 14
    }
    
    [System.Serializable]
    public struct FieldParameters
    {
        public float scale;
        public FieldType fieldType;
        public Vector3 ellipsoidRadii;   // Radii for ellipsoid/sphere, or general size parameters
        public Vector3 rectangleSize;    // Size for box/rectangle shapes
        public Vector2 donutParams;      // Major and minor radius for torus shapes
        
        public static FieldParameters Default => new FieldParameters
        {
            ellipsoidRadii = Vector3.one,
            rectangleSize = Vector3.one,
            donutParams = new Vector2(1f, 0.3f)
        };
        
        // Convenience constructors
        public static FieldParameters Sphere(float radius) => new FieldParameters
        {
            fieldType = FieldType.Sphere,
            ellipsoidRadii = Vector3.one * radius,
            rectangleSize = Vector3.one,
            donutParams = new Vector2(1f, 0.3f)
        };
        
        public static FieldParameters Ellipsoid(Vector3 radii) => new FieldParameters
        {
            fieldType = FieldType.Ellipsoid,
            ellipsoidRadii = radii,
            rectangleSize = Vector3.one,
            donutParams = new Vector2(1f, 0.3f)
        };
        
        public static FieldParameters Box(Vector3 size) => new FieldParameters
        {
            fieldType = FieldType.Rectangle,
            ellipsoidRadii = Vector3.one,
            rectangleSize = size,
            donutParams = new Vector2(1f, 0.3f)
        };
        
        public static FieldParameters Torus(float majorRadius, float minorRadius) => new FieldParameters
        {
            fieldType = FieldType.VerticalTorus,
            ellipsoidRadii = Vector3.one,
            rectangleSize = Vector3.one,
            donutParams = new Vector2(majorRadius, minorRadius)
        };
        
        public static FieldParameters Cylinder(float radius, float height) => new FieldParameters
        {
            fieldType = FieldType.Cylinder,
            ellipsoidRadii = new Vector3(radius, radius, radius),
            rectangleSize = new Vector3(1f, height, 1f),
            donutParams = new Vector2(1f, 0.3f)
        };
    }
    
    public class DistanceFieldGenerator
    {
        public ComputeShader ComputeInstance => computeShader;
        
        private ComputeShader computeShader;
        private int kernelIndex;
        
        public RenderTexture Field { get; private set; }
        
        // Shader property IDs for better performance
        private static readonly int FieldPropertyId = Shader.PropertyToID("_Field");
        private static readonly int DimsPropertyId = Shader.PropertyToID("_Dims");
        private static readonly int ScalePropertyId = Shader.PropertyToID("_Scale");
        private static readonly int TypePropertyId = Shader.PropertyToID("_Type");
        private static readonly int EllipsoidRadiiPropertyId = Shader.PropertyToID("_EllipsoidRadii");
        private static readonly int RectangleSizePropertyId = Shader.PropertyToID("_RectangleSize");
        private static readonly int DonutParamsPropertyId = Shader.PropertyToID("_DonutParams");

        private ComputeBuffer solidsBuffer;
        
        public DistanceFieldGenerator(ComputeShader compute, Vector3Int dimensions, FieldParameters parameters = default)
        {
            if (parameters.Equals(default(FieldParameters)))
                parameters = FieldParameters.Default;
                
            Initialize(compute, dimensions, parameters);
        }
        
        private void Initialize(ComputeShader compute, Vector3Int dimensions, FieldParameters parameters)
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
        
        private void GenerateField(Vector3Int dimensions, FieldParameters parameters, 
            Matrix4x4 transform, List<SculptSolid> solids)
        {
            // Set shader parameters
            SetSculptSolids(solids);
            computeShader.SetTexture(kernelIndex, FieldPropertyId, Field);
            computeShader.SetInts(DimsPropertyId, dimensions.x, dimensions.y, dimensions.z);
            computeShader.SetFloat(ScalePropertyId, parameters.scale);
            computeShader.SetInt(TypePropertyId, (int)parameters.fieldType);
            computeShader.SetMatrix("_Transform", transform);
            
            // Set shape parameters
            computeShader.SetVector(EllipsoidRadiiPropertyId, parameters.ellipsoidRadii);
            computeShader.SetVector(RectangleSizePropertyId, parameters.rectangleSize);
            computeShader.SetVector(DonutParamsPropertyId, parameters.donutParams);
            
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
        public void Run(FieldParameters parameters, Matrix4x4 transform, List<SculptSolid> solids)
        {
            if (Field != null)
            {
                Vector3Int dimensions = new Vector3Int(Field.width, Field.height, Field.volumeDepth);
                GenerateField(dimensions, parameters, transform, solids);
            }
        }

        public void SetSculptSolids(List<SculptSolid> solids)
        {
            if (solidsBuffer != null && solidsBuffer.count != solids.Count)
            {
                solidsBuffer.Release();
                solidsBuffer = null;
            }

            if (solidsBuffer != null)
            {
                solidsBuffer.SetData(solids);
                return;
            }

            solidsBuffer = new ComputeBuffer(solids.Count, Marshal.SizeOf(typeof(SculptSolid)));
            
            computeShader.SetBuffer(kernelIndex, "_Solids", solidsBuffer);
            computeShader.SetInt("_SolidCount", solidsBuffer.count);
            solidsBuffer.SetData(solids);
            
        }
        
        /// <summary>
        /// Update only the scale parameter and regenerate
        /// </summary>
        public void UpdateScale(float scale)
        {
            if (Field != null)
            {
                computeShader.SetFloat(ScalePropertyId, scale);
                DispatchShader(Field.width, Field.height, Field.volumeDepth);
            }
        }
        
        /// <summary>
        /// Update only the field type and regenerate
        /// </summary>
        public void UpdateFieldType(FieldType fieldType)
        {
            if (Field != null)
            {
                computeShader.SetInt(TypePropertyId, (int)fieldType);
                DispatchShader(Field.width, Field.height, Field.volumeDepth);
            }
        }
        
        
        /// <summary>
        /// Copy current field values to a new RenderTexture
        /// </summary>
        public RenderTexture CopyField()
        {
            if (Field == null) return null;
            
            RenderTexture copy = new RenderTexture(Field.width, Field.height, 0, Field.format)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = Field.volumeDepth,
                enableRandomWrite = true,
                wrapMode = Field.wrapMode,
                filterMode = Field.filterMode
            };
            
            copy.Create();
            Graphics.CopyTexture(Field, copy);
            
            return copy;
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
            
            Object.Destroy(computeShader);
        }
        
        ~DistanceFieldGenerator()
        {
            Dispose();
        }
    }
}

