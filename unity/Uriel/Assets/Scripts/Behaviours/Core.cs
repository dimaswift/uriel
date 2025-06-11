using System;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Core : MonoBehaviour
    {
        [SerializeField] private string exportPath = "Assets/Exports/";
        [SerializeField] private PhotonBuffer fieldBuffer;
        [SerializeField] private PhotonBuffer coreBuffer;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private SculptorConfig config;
        [SerializeField] private ComputeShader cubeMarchCompute, volumeCompute, recursiveVolumeCompute;
        [SerializeField] private MeshFilter meshFilter;

        [SerializeField] private RenderTexture coreTexture;
        [SerializeField] private RenderTexture fieldTexture;
        
        private CubeMarch cubeMarch;
        private RecursiveVolumeWriter fieldVolumeWriter;
        private VolumeWriter coreVolumeWriter;
        
        
        private void Start()
        {
    
            STLExporter.OnExportProgress += OnExportProgress;
            STLExporter.OnExportCompleted += OnExportCompleted;
            coreVolumeWriter = new VolumeWriter(volumeCompute, coreBuffer, 8, FilterMode.Point);
            fieldVolumeWriter = new RecursiveVolumeWriter(recursiveVolumeCompute, fieldBuffer, coreVolumeWriter.Texture, config.resolution.x);
           
            cubeMarch = new CubeMarch(
                    config.resolution.x, 
                    config.resolution.y, 
                    config.resolution.z,
                    config.budget, 
                    cubeMarchCompute,
                    fieldVolumeWriter.Texture);
            
            meshFilter.mesh = cubeMarch.Mesh;
            fieldBuffer.LinkMaterial(meshFilter.GetComponent<MeshRenderer>().material);

            coreTexture = coreVolumeWriter.Texture;
            fieldTexture = fieldVolumeWriter.Texture;
        }

        private void Update()
        {
            if (runInUpdate)
            {
                coreVolumeWriter.Run(config.sculpt.innerRadius);
                fieldVolumeWriter.Run(config.sculpt.innerRadius);
                cubeMarch.Run(config.sculpt);
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                ExportCurrentMesh();
            }
        }

        public async void ExportCurrentMesh()
        {
            if (cubeMarch?.Mesh == null)
            {
                Debug.LogWarning("No mesh to export");
                return;
            }

            string fileName = $"procedural_mesh_{System.DateTime.Now:yyyyMMdd_HHmmss}.stl";
            string fullPath = System.IO.Path.Combine(exportPath, fileName);

            try
            {
                // Export with optimization
                await STLExporter.ExportMeshToSTLAsync(
                    name: Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                    mesh: cubeMarch.Mesh,
                    binary: true, // Binary STL is smaller and faster
                    optimizeVertices: true // Remove duplicate vertices
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Export failed: {ex.Message}");
            }
        }

        private void OnExportProgress(float progress)
        {
            Debug.Log($"Export progress: {progress * 100:F1}%");
            // Update progress bar in UI
        }

        private void OnExportCompleted(string filePath, int originalTriangles, int finalTriangles)
        {
            float efficiency = (float)finalTriangles / originalTriangles * 100f;
            Debug.Log($"Export completed!\n" +
                      $"File: {filePath}\n" +
                      $"Triangles: {originalTriangles} -> {finalTriangles} ({efficiency:F1}% used)\n" +
                      $"File size: {new System.IO.FileInfo(filePath).Length / 1024f:F1} KB");
        }

        // Alternative: Synchronous export for immediate use

        private void OnDestroy()
        {
            STLExporter.OnExportProgress -= OnExportProgress;
            STLExporter.OnExportCompleted -= OnExportCompleted;
            cubeMarch?.Dispose();
            fieldVolumeWriter?.Dispose();
            coreVolumeWriter?.Dispose();
        }
    }
}