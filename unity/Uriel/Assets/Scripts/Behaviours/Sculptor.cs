using System;
using System.IO;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Sculptor : MonoBehaviour
    {
        [SerializeField] private string exportPath = "Assets/Exports/";
        [SerializeField] private PhotonBuffer fieldBuffer;
        [SerializeField] private PhotonBuffer thresholdBuffer;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private SculptorConfig config;
        [SerializeField] private ComputeShader cubeMarchCompute, volumeCompute;
        [SerializeField] private MeshFilter meshFilter;

        private CubeMarch cubeMarch;
        private VolumeWriter fieldVolumeWriter;
        private VolumeWriter thresholdVolumeWriter;
        
        
        
        private void Start()
        {
    
            STLExporter.OnExportProgress += OnExportProgress;
            STLExporter.OnExportCompleted += OnExportCompleted;
            fieldVolumeWriter = new VolumeWriter(volumeCompute, fieldBuffer, config.resolution);
            thresholdVolumeWriter = new VolumeWriter(volumeCompute, thresholdBuffer, config.resolution);
            cubeMarch = new CubeMarch(
                    config.resolution, 
                    config.resolution, 
                    config.resolution,
                    config.budget, 
                    cubeMarchCompute,
                    fieldVolumeWriter.Texture,
                    thresholdVolumeWriter.Texture);
            
            meshFilter.mesh = cubeMarch.Mesh;
            fieldBuffer.LinkMaterial(meshFilter.GetComponent<MeshRenderer>().material);
        }

        private void Update()
        {
            if (runInUpdate)
            {
                fieldVolumeWriter.Run(config.sculpt.innerRadius);
                thresholdVolumeWriter.Run(config.sculpt.innerRadius);
                cubeMarch.Run(config.sculpt, config.shells);
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
            thresholdVolumeWriter?.Dispose();
        }
    }
}
