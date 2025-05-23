using UnityEngine;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class MarchingCube : MonoBehaviour
    {
        [SerializeField] private bool update;
        [SerializeField] private float target, scale;
        [SerializeField] private int budget = 64;
        [SerializeField] private int resolution = 64;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private MeshFilter meshFilter;

        private MeshBuilder builder;
        
        private void Start()
        {
            builder = 
                new MeshBuilder(resolution, resolution, resolution, budget, compute);
           
            meshFilter.mesh = builder.Mesh;

            GetComponent<PhotonBuffer>()
                .LinkComputeKernel(compute);

            builder.Build(target, scale);
        }

        private void Update()
        {
            if (update)
            {
                builder.Build(target, scale);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                //FileUtils.MeshToObjFile(builder.Mesh, Application.dataPath + "/Export/adam.obj", false);
            }
        }

        private void OnDestroy()
        {
            builder?.Dispose();
        }
    }
}
