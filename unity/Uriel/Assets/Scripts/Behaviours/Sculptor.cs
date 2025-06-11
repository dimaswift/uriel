using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Sculptor : MonoBehaviour
    {
        [SerializeField] private string exportPath = "Assets/Exports/";
        [SerializeField] private PhotonBuffer buffer;
        [SerializeField] private bool updateDistanceFields;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private SculptorConfig config;
        [SerializeField] private ComputeShader cubeMarchCompute, distanceFieldCompute;
        [SerializeField] private MeshFilter meshFilter;
      
        private CubeMarch cubeMarch;
        private DistanceFieldGenerator generator;
        private Combine combine;
        private VolumeWriter thresholdVolumeWriter;
        
        private void Start()
        {
            STLExporter.OnExportProgress += OnExportProgress;
            STLExporter.OnExportCompleted += OnExportCompleted;

            generator = new DistanceFieldGenerator(distanceFieldCompute, config.resolution, config.field);
          
            buffer.LinkComputeKernel(generator.ComputeInstance);
            
            cubeMarch = new CubeMarch(
                    config.resolution.x, 
                    config.resolution.y, 
                    config.resolution.z,
                    config.budget, 
                    cubeMarchCompute,
                    generator.Field);
            
            meshFilter.mesh = cubeMarch.Mesh;
            
            generator.Run(config.field, transform.localToWorldMatrix.inverse);
            cubeMarch.Run(config.sculpt);
        }

        private void Update()
        {
            if (runInUpdate)
            {
                generator.Run(config.field, transform.localToWorldMatrix.inverse);
                cubeMarch.Run(config.sculpt);
            }

            //meshFilter.transform.localScale =
            //    new Vector3(64f / config.resolution.x, 64f / config.resolution.x, 64f / config.resolution.x);
            if (Input.GetKeyDown(KeyCode.S) &&  runInUpdate)
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
            try
            {
                await STLExporter.ExportMeshToSTLAsync(
                    name: Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                    mesh: cubeMarch.Mesh,
                    binary: true,
                    optimizeVertices: true
                );
            }
            catch (Exception ex)
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
                      $"File size: {new FileInfo(filePath).Length / 1024f:F1} KB");
        }
        
        private void OnDestroy()
        {
            STLExporter.OnExportProgress -= OnExportProgress;
            STLExporter.OnExportCompleted -= OnExportCompleted;
            cubeMarch?.Dispose();
            generator?.Dispose();
            thresholdVolumeWriter?.Dispose();
        }
    }
}
