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
            
            meshFilter.mesh = cubeMarch.Mesh;
            fieldBuffer.LinkMaterial(meshFilter.GetComponent<MeshRenderer>().material);
        }

        private void Update()
        {
            if (runInUpdate)
            {
                fieldVolumeWriter.Run();
                thresholdVolumeWriter.Run();
                cubeMarch.Run(config.sculpt, config.holes);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                MeshSaveUtility.SaveMeshAsset(cubeMarch.Mesh, "Adam");
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
