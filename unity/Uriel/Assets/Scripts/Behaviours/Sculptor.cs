using System;
using System.IO;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Sculptor : MonoBehaviour
    {
        [SerializeField] private PhotonBuffer fieldBuffer;
        [SerializeField] private PhotonBuffer thresholdBuffer;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private SculptorConfig config;
        [SerializeField] private ComputeShader cubeMarchCompute, volumeCompute;
        [SerializeField] private MeshFilter meshFilter;

        private CubeMarch cubeMarch;
        private VolumeWriter fieldVolumeWriter;
        private VolumeWriter thresholdVolumeWriter;

        [SerializeField] private RenderTexture tex;
        
        private void Start()
        {
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

            tex = fieldVolumeWriter.Texture;
            
            meshFilter.mesh = cubeMarch.Mesh;
        }

        private void Update()
        {
            if (runInUpdate)
            {
                fieldVolumeWriter.Run();
                thresholdVolumeWriter.Run();
                cubeMarch.Run(config.target, config.range, config.flipNormals, config.invertTriangles);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                FileUtils.ExportMeshToASCIISTL(cubeMarch.Mesh,
                    Path.Combine(Application.dataPath,
                        "Generated/STL/" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper()));
            }
        }

        private void OnDestroy()
        {
            cubeMarch?.Dispose();
            fieldVolumeWriter?.Dispose();
            thresholdVolumeWriter?.Dispose();
        }
    }
}
