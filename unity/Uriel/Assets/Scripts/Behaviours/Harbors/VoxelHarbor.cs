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
        private ComputeBuffer meshBuffer;

        private ComputeBuffer particlesBuffer;

        private HarborBehaviour harbor;
        
        private int ResolutionCubed => resolution * resolution * resolution;

        private void Awake()
        {
            harbor = GetComponent<HarborBehaviour>();
            harbor.OnBufferCreated += SetBuffer;
        }

        private void SetBuffer(ComputeBuffer b)
        {
            compute.SetInt(ShaderProps.WaveCount, harbor.WaveCount);
            compute.SetBuffer(0, ShaderProps.WaveBuffer, b);
            material.SetInt(ShaderProps.WaveCount, harbor.WaveCount);
            material.SetBuffer(ShaderProps.WaveBuffer, b);
        }
        
        private void Start()
        {
            particlesBuffer = new ComputeBuffer(ResolutionCubed, sizeof(float) * 4 * 4);
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)ResolutionCubed;
            meshBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            meshBuffer.SetData(args);
            compute.SetBuffer(0, ShaderProps.Particles, particlesBuffer);
            compute.SetInt(ShaderProps.Resolution, resolution);
            material.SetBuffer(ShaderProps.Particles, particlesBuffer);
            
            
        }

       
        private void Update()
        {
            harbor.UpdateWaveBuffer();
            compute.Dispatch(0, Mathf.CeilToInt(ResolutionCubed / 1024f), 1, 1);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material,
                new Bounds(transform.position, Vector3.one * (float.MaxValue)), meshBuffer);
        }

        private void OnDestroy()
        {
            harbor.OnBufferCreated -= SetBuffer;
        }
    }
}
