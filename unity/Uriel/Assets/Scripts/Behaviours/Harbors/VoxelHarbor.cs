using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(HarborBehaviour))]
    public class VoxelHarbor : MonoBehaviour
    {
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;
        [SerializeField] private int resolution = 32;

        private ParticleRenderer particles;

        private int ResolutionCubed => resolution * resolution * resolution;
        
        
        private void Start()
        {
            particles = gameObject.AddComponent<ParticleRenderer>();
            particles.Init(mesh, material, ResolutionCubed);
            particles.LinkComputeKernel(compute);
        }

       
        private void Update()
        {
            compute.Dispatch(0, Mathf.CeilToInt(ResolutionCubed / 1024f), 1, 1);
        }
    }
}
