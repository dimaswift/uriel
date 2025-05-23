using System;
using UnityEngine;
using UnityEngine.Rendering;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(BreathBuffer), typeof(PhotonBuffer), typeof(ParticleRenderer))]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Sculptor : MonoBehaviour
    {
        [SerializeField] private SculptorConfig config;
        [SerializeField] private Material projection;
        [SerializeField] private ComputeShader compute;
    
        private ComputeBuffer vertexBuffer;
        private ComputeBuffer triangleBuffer;
        private ComputeBuffer normalBuffer;

        [SerializeField] private RenderTexture field;

        private int initKernel, computeParticlesKernel, computeFieldKernel, spawnParticleKernel;
        private int particleGroups;
        private Vector3Int fieldGroups;

        private Mesh mesh;
        private ParticleRenderer particleRenderer;

        private Vector3[] vertices, normals;
        private int[] triangles;
        private int vertexCount;
        
        
        private void Awake()
        {
            initKernel = compute.FindKernel("Init");
            computeParticlesKernel = compute.FindKernel("ComputeParticles");
            computeFieldKernel = compute.FindKernel("ComputeField");
            spawnParticleKernel = compute.FindKernel("SpawnParticle");
            
            mesh = new Mesh();
            mesh.MarkDynamic();

            vertexCount = config.fieldResolution * config.fieldResolution * config.fieldResolution;

            vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            triangleBuffer = new ComputeBuffer(vertexCount, sizeof(int) * 3);
            normalBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);

            triangles = new int[vertexCount * 3];
            normals = new Vector3[vertexCount];
            vertices = new Vector3[vertexCount];

            field = CreateTexture3D(config.fieldResolution);
            
            compute.SetTexture(computeFieldKernel, ShaderProps.Field, field);
            compute.SetInt(ShaderProps.FieldResolution, config.fieldResolution);

            triangleBuffer.SetData(triangles);
            
            compute.GetKernelThreadGroupSizes(computeParticlesKernel, out var x, out _, out _);
            particleGroups = Mathf.CeilToInt((float)vertexCount / x);

            fieldGroups = compute.GetGroups(config.fieldResolution, computeFieldKernel);
            
            compute.SetBuffer(initKernel, ShaderProps.VertexBuffer, vertexBuffer);
            compute.SetBuffer(initKernel, ShaderProps.TriangleBuffer, triangleBuffer);
            compute.SetBuffer(initKernel, ShaderProps.NormalBuffer, normalBuffer);

            compute.SetBuffer(computeParticlesKernel, ShaderProps.VertexBuffer, vertexBuffer);
            compute.SetBuffer(computeParticlesKernel, ShaderProps.TriangleBuffer, triangleBuffer);
            compute.SetBuffer(computeParticlesKernel, ShaderProps.NormalBuffer, normalBuffer);
            compute.SetTexture(computeParticlesKernel, ShaderProps.Field, field);
            
            compute.SetInt(ShaderProps.VertexCount, vertexCount);
            compute.SetInt(ShaderProps.Capacity, config.fieldResolution);
            
            particleRenderer = gameObject.GetComponent<ParticleRenderer>();
            particleRenderer
                .SetUp(vertexCount)
                .Init()
                .LinkComputeKernel(compute, initKernel)
                .LinkComputeKernel(compute, computeParticlesKernel)
                .LinkComputeKernel(compute, spawnParticleKernel)
                .LinkComputeKernel(compute, computeFieldKernel)
                .LinkMaterial(projection);

            var photonBuffer = GetComponent<PhotonBuffer>();
            
            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                photonBuffer.LinkMaterial(meshRenderer.sharedMaterial);
            }

            photonBuffer.LinkComputeKernel(compute, computeFieldKernel);
            photonBuffer.LinkComputeKernel(compute, computeParticlesKernel);
            photonBuffer.LinkComputeKernel(compute, initKernel);

            photonBuffer.LinkMaterial(particleRenderer.GetMaterial());
            
            GetComponent<MeshFilter>().mesh = mesh;
            
            Init();
        }

        private void SetVariables()
        {
            compute.SetInt(ShaderProps.ParticleRadius, config.particleRadius);
            compute.SetFloat(ShaderProps.Bounds, config.bounds);
            compute.SetFloat(ShaderProps.Gravity, config.gravity);
            compute.SetFloat(ShaderProps.Acceleration, config.acceleration);
            compute.SetFloat(ShaderProps.Speed, config.speed);
            compute.SetFloat(ShaderProps.SampleRadius, config.sampleRadius);
            compute.SetFloat(ShaderProps.ParticleSize, config.particleSize);
            compute.SetInt(ShaderProps.Frequency, config.frequency);
        }

        private void SpawnParticle(Vector3 point)
        {
            compute.SetVector(ShaderProps.SpawnPoint, point);
            compute.Dispatch(spawnParticleKernel, 1, 1, 1);
        }

        private void ComputeField()
        {
            compute.Dispatch(computeFieldKernel, fieldGroups.x, fieldGroups.y, fieldGroups.z);
        }

        private RenderTexture CreateTexture3D(int resolution)
        {
            var tex = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB64)
            {
                enableRandomWrite = true,
                dimension = TextureDimension.Tex3D,
                volumeDepth = resolution,
                filterMode = FilterMode.Point,
                autoGenerateMips = false,
                useMipMap = false
            };
            
            tex.Create();
            return tex;
        }

        private void Update()
        {
            Init();

            if (Input.GetMouseButtonDown(0))
            {
                var mouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                new Plane(Vector3.forward, Vector3.zero).Raycast(mouse, out float d);
                //SpawnParticle(mouse.GetPoint(d));
            }
            
            SetVariables();
            ComputeField();
            Process();
            
            particleRenderer.Draw();
        }

        private void Process()
        {
            compute.Dispatch(computeParticlesKernel, particleGroups, 1, 1);
            vertexBuffer.GetData(vertices);
            normalBuffer.GetData(normals);
            triangleBuffer.GetData(triangles);
            
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
        }
        
        private void Init()
        {
            compute.Dispatch(initKernel, fieldGroups.x, fieldGroups.y, fieldGroups.z);
        }
        
        private void OnDestroy()
        {
            if (vertexBuffer != null) vertexBuffer.Release();
            if (triangleBuffer != null) triangleBuffer.Release();
            if (normalBuffer != null) normalBuffer.Release();
            if (mesh) Destroy(mesh);
            if (field) field.Release();
        }
    }
}