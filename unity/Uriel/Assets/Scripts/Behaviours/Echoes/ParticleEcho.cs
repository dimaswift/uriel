using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(PhotonBuffer), typeof(ParticleRenderer))]
    public class ParticleEcho : MonoBehaviour
    {
        [SerializeField] private float scale = 1;
        [SerializeField] private float radius = 5;
        [SerializeField] private float speed = 1f;
        [SerializeField] private float acceleration = 1f;
        [SerializeField] private float sampleRadius = 0.1f;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material mat;
        [SerializeField] private int capacity = 64;
        [SerializeField] private Solid type;
        [SerializeField] private float particleSize = 1f;
        [SerializeField] private Lumen lumen;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Matrix4x4 reference;
        private PhotonBuffer photonBuffer;
        private ParticleRenderer particleRenderer;
        private ComputeBuffer particlesBuffer;
  
        private int initKernel, processKernel;

        private int CubedCapacity => capacity * capacity * capacity;
        
        private void Awake()
        {
            initKernel = compute.FindKernel("Init");
            processKernel = compute.FindKernel("Process");
            
            photonBuffer = gameObject.GetComponent<PhotonBuffer>();
            particleRenderer = gameObject.GetComponent<ParticleRenderer>();

            particleRenderer.SetUp(mesh, mat, CubedCapacity)
                .Init()
                .LinkComputeKernel(compute, initKernel)
                .LinkComputeKernel(compute, processKernel);
            
            photonBuffer.LinkComputeKernel(compute, initKernel).LinkComputeKernel(compute, processKernel);
            
            particleRenderer.Randomize(radius, particleSize);
        }
        

        private void Update()
        {
            compute.SetMatrix(ShaderProps.Reference, reference);
            compute.SetVector(ShaderProps.Offset, transform.position);
            compute.SetFloat(ShaderProps.Scale, scale);
            compute.SetFloat(ShaderProps.Size, particleSize);
            compute.SetFloat(ShaderProps.SampleRadius, sampleRadius);
            compute.SetInt(ShaderProps.SolidType, (int) type);
            compute.SetFloat(ShaderProps.DeltaTime, Time.deltaTime);
            compute.SetFloat(ShaderProps.Speed, speed);
            compute.SetFloat(ShaderProps.Time, Time.time);
            compute.SetFloat(ShaderProps.Acceleration, acceleration);
            compute.Dispatch(processKernel, Mathf.CeilToInt(CubedCapacity / 512f), 1, 1);

            if (Input.GetKeyDown(KeyCode.R))
            {
                particleRenderer.Randomize(radius, particleSize);
                compute.Dispatch(initKernel, Mathf.CeilToInt(CubedCapacity / 512f), 1, 1);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                particleRenderer.ArrangeInACube(capacity, radius, particleSize);
                compute.Dispatch(initKernel, Mathf.CeilToInt(CubedCapacity / 512f), 1, 1);
            }

            particleRenderer.Draw();
        }
    }
}