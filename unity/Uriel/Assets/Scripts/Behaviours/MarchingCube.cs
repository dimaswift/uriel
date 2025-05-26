using UnityEngine;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class MarchingCube : MonoBehaviour
    {
        [SerializeField] private bool update;
        [SerializeField] private float target, range;
        [SerializeField] private int budget = 64;
        [SerializeField] private int resolution = 64;
        [SerializeField] private ComputeShader cubeMarchCompute, floodFillCompute;
        [SerializeField] private MeshFilter meshFilter;

        private CubeMarch cube;
        private FloodFill floodFill;
        
        private void Start()
        {
            floodFill = new FloodFill(floodFillCompute, resolution, resolution, resolution);
            
            cube = 
                new CubeMarch(resolution, resolution, resolution, 
                    budget, cubeMarchCompute, floodFill.FieldTexture);
           
            meshFilter.mesh = cube.Mesh;

            GetComponent<PhotonBuffer>()
                .LinkComputeKernel(cubeMarchCompute)
                .LinkComputeKernel(floodFillCompute);
            
        }

        private void Update()
        {
            if (update)
            {
                floodFill.Run(target, VolumePicker.Main.Selection);
                cube.Run(target, range);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                //FileUtils.MeshToObjFile(builder.Mesh, Application.dataPath + "/Export/adam.obj", false);
            }
        }

        private void OnDestroy()
        {
            cube?.Dispose();
            floodFill?.Dispose();
        }
    }
}
