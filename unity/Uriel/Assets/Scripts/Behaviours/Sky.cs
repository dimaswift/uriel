using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class Sky : MonoBehaviour
    {
        [SerializeField] private SkyConfig config;
       
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material mat;
       
        private ComputeBuffer starBuffer;
        private ComputeBuffer protoStarsBuffer;
        private ComputeBuffer meshBuffer;
        
        private Star[] stars;

        private int groupsX;
        private int tickKernel;
        private int initKernel;


        private void Start()
        {
            initKernel = compute.FindKernel("Init");
            tickKernel = compute.FindKernel("Tick");
            compute.GetKernelThreadGroupSizes(tickKernel, out var x, out _, out _);
            groupsX =  Mathf.CeilToInt(config.capacity / (float)x);
            stars = new Star[config.capacity];
            starBuffer = new ComputeBuffer(config.capacity, Marshal.SizeOf(typeof(Star)));
            protoStarsBuffer = new ComputeBuffer(config.protoStars.Length, Marshal.SizeOf(typeof(Star)));
          
            compute.SetBuffer(initKernel, "_ProtoStars", protoStarsBuffer);
            
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)config.capacity;
            meshBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            meshBuffer.SetData(args);
            
            SetComputeProps();
            
            LinkMaterial(mat);
            LinkComputeKernel(compute, initKernel);
            LinkComputeKernel(compute, tickKernel);
            
            Init();
            
        }

        private void Init()
        {
            UpdateProtoStars();
            compute.Dispatch(initKernel, groupsX, 1, 1);
        }

        private void LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            shader.SetBuffer(id, ShaderProps.Stars, starBuffer);
            shader.SetInt(ShaderProps.StarCount, starBuffer.count);
        }

        private void Tick(float dt)
        {
            compute.SetFloat(ShaderProps.DeltaTime, dt);
            compute.Dispatch(tickKernel, groupsX, 1, 1);
        }

        private void UpdateProtoStars()
        {
            protoStarsBuffer.SetData(config.protoStars);
        }
        
        private void LinkMaterial(Material material)
        {
            material.SetBuffer(ShaderProps.Stars, starBuffer);
            material.SetInt(ShaderProps.StarCount, starBuffer.count);
        }

        private void Draw()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, mat,
                new Bounds(transform.position, Vector3.one * (float.MaxValue)), meshBuffer);
        }

        private void SetComputeProps()
        {
            compute.SetVector(ShaderProps.Offset, config.offset);
            compute.SetFloat(ShaderProps.Radius, config.radius);
            compute.SetFloat(ShaderProps.Speed, config.simulationSpeed);
        }

        private void Update()
        {
            Init();
            if (Input.GetKeyDown(KeyCode.I))
            {
                Init();
            }
            SetComputeProps();
            Tick(Time.deltaTime);
            Draw();
        }

        private void OnDestroy()
        {
            starBuffer?.Release();
            meshBuffer?.Release();
        }
    }
}