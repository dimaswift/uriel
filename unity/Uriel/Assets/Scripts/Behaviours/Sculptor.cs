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
        [SerializeField] private PhotonBuffer[] layerBuffers;
        [SerializeField] private bool updateDistanceFields;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private SculptorConfig config;
        [SerializeField] private ComputeShader cubeMarchCompute, distanceFieldCompute, combineCompute;
        [SerializeField] private MeshFilter meshFilter;

        private CubeMarch cubeMarch;
        private DistanceFieldGenerator[] generators;
        private Combine combine;
        private VolumeWriter thresholdVolumeWriter;

        [SerializeField] private RenderTexture l0, l1, res;
        
        private void Start()
        {
    
            STLExporter.OnExportProgress += OnExportProgress;
            STLExporter.OnExportCompleted += OnExportCompleted;

            generators = new DistanceFieldGenerator[layerBuffers.Length];
            
            List<RenderTexture> layerTextures = new();
            for (int i = 0; i < generators.Length; i++)
            {
                var gen = new DistanceFieldGenerator(distanceFieldCompute, config.resolution, config.layers[i]);
                generators[i] = gen;
                layerTextures.Add(gen.Field);
                layerBuffers[i].LinkComputeKernel(gen.ComputeInstance);
            }

            combine = new Combine(combineCompute, layerTextures.ToArray());
            
            cubeMarch = new CubeMarch(
                    config.resolution.x, 
                    config.resolution.y, 
                    config.resolution.z,
                    config.budget, 
                    cubeMarchCompute,
                    combine.Result);
            
            meshFilter.mesh = cubeMarch.Mesh;

            l0 = generators[0].Field;
            l1 = generators[1].Field;
            
            res = combine.Result;
        }

        private void Update()
        {
            if (runInUpdate)
            {
                if (updateDistanceFields)
                {
                    for (int i = 0; i < config.layers.Length; i++)
                    {
                        generators[i].Run(config.layers[i]);
                        combine.UpdateLayer(i, config.combines[i]);
                    }
                
                    combine.Run();
                }
               
                
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

            foreach (var generator in generators)
            {
                generator.Dispose();
            }
            
            thresholdVolumeWriter?.Dispose();
        }
    }
}
