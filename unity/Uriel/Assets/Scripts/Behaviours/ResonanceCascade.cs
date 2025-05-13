using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(BreathBuffer))]
    [RequireComponent(typeof(PhotonBuffer))]
    public class ResonanceCascade : MonoBehaviour
    {
        [SerializeField] private int dimensions = 5;
        [SerializeField] [Range(1, 32)] private uint scanRadius = 1;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private uint resolution = 128;
        [SerializeField] private Vector3 threshold;
        [SerializeField] private Vector3 multiplier;
        [SerializeField] private MeshRenderer screenRenderer;
        [SerializeField] private MeshRenderer particlesRenderer;
        [SerializeField] private Vector3Int steps = new (1,1,1);
        [SerializeField] private float scale = 1f;
        [SerializeField] private int spawnRate = 1;
        [SerializeField] private int particleRadius = 1;
        [SerializeField] private float canvasFadeSpeed = 1f;
        [SerializeField] private float phaseSpeed;
        [SerializeField] private float lifetime;
        [SerializeField] private float gravity;
        [SerializeField] private float acceleration;
        [SerializeField] private float speed;
        [SerializeField] private float attraction = 1;
        [SerializeField] private float repulsion = 1;
        
        private RenderTexture screen;
        private RenderTexture field;
        private RenderTexture particlePositions;
        private RenderTexture particleVelocities;
        private RenderTexture particleCanvas;
   
        private ComputeBuffer resonanceBuffer;
        private ComputeBuffer modulationBuffer;
        
        private int clearScreenKernel;
        private int clearFieldKernel;
        private int computeFieldKernel;
        private int computeParticlesKernel;
        private int renderScreenKernel;
        private int renderParticlesKernel;
        private int spawnParticlesKernel;
        private int fadeParticlesKernel;
        private int collapseFieldKernel;

        private Resonance[] resonances;

        struct Resonance
        {
            public int current;
        }
        
        private int currentScreen;

        private uint threads;
        private int groups;

        private void Start()
        {
            collapseFieldKernel = compute.FindKernel("CollapseField");
            renderParticlesKernel = compute.FindKernel("RenderParticles");
            clearScreenKernel = compute.FindKernel("ClearScreen");
            clearFieldKernel = compute.FindKernel("ClearField");
            computeFieldKernel = compute.FindKernel("ComputeField");
            renderScreenKernel = compute.FindKernel("RenderScreen");
            computeParticlesKernel = compute.FindKernel("ComputeParticles");
            spawnParticlesKernel = compute.FindKernel("SpawnParticles");
            fadeParticlesKernel = compute.FindKernel("FadeParticles");
            
            
            compute.GetKernelThreadGroupSizes(computeFieldKernel, out threads, out _, out _);
         
            groups = Mathf.CeilToInt(resolution / (float)threads);
            
            screen = CreateTexture();
            particleCanvas = CreateTexture();
            
            field = CreateTexture3D();
            particlePositions = CreateTexture3D();
            particleVelocities = CreateTexture3D();

            modulationBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(Modulation)));
            resonanceBuffer = new ComputeBuffer(dimensions, Marshal.SizeOf(typeof(Resonance)));
            resonances = new Resonance[dimensions];
            resonanceBuffer.SetData(resonances);
            
            compute.SetTexture(clearScreenKernel, ShaderProps.Screen, screen);
            compute.SetTexture(clearFieldKernel, ShaderProps.Field, field);
            compute.SetTexture(computeFieldKernel, ShaderProps.Field, field);
            compute.SetTexture(computeFieldKernel, ShaderProps.ParticlePositions, particlePositions);
            
            compute.SetTexture(clearFieldKernel, ShaderProps.Field, field);
            compute.SetTexture(clearFieldKernel, ShaderProps.ParticlePositions, particlePositions);
            compute.SetTexture(clearFieldKernel, ShaderProps.ParticleVelocities, particleVelocities);

            compute.SetTexture(computeParticlesKernel, ShaderProps.ParticlePositions, particlePositions);
            compute.SetTexture(computeParticlesKernel, ShaderProps.ParticleVelocities, particleVelocities);
            compute.SetTexture(computeParticlesKernel, ShaderProps.ParticleCanvas, particleCanvas);

            compute.SetTexture(computeParticlesKernel, ShaderProps.Field, field);

            compute.SetTexture(fadeParticlesKernel, ShaderProps.ParticleCanvas, particleCanvas);
            
            compute.SetTexture(renderScreenKernel, ShaderProps.Screen, screen);
            compute.SetTexture(renderScreenKernel, ShaderProps.Field, field);
            compute.SetTexture(renderParticlesKernel, ShaderProps.ParticleCanvas, particleCanvas);
            compute.SetTexture(renderParticlesKernel, ShaderProps.ParticlePositions, particlePositions);
            compute.SetTexture(renderParticlesKernel, ShaderProps.ParticleVelocities, particleVelocities);

            compute.SetBuffer(computeFieldKernel, ShaderProps.ResonanceBuffer, resonanceBuffer);
            compute.SetBuffer(collapseFieldKernel, ShaderProps.ResonanceBuffer, resonanceBuffer);
            
            
            compute.SetTexture(collapseFieldKernel, ShaderProps.Field, field);
            compute.SetTexture(collapseFieldKernel, ShaderProps.ParticlePositions, particlePositions);
            compute.SetTexture(collapseFieldKernel, ShaderProps.ParticleVelocities, particleVelocities);

            
            compute.SetTexture(clearScreenKernel, ShaderProps.ParticleCanvas, particleCanvas);

            
            compute.SetTexture(spawnParticlesKernel, ShaderProps.ParticlePositions, particlePositions);
            compute.SetTexture(spawnParticlesKernel, ShaderProps.ParticleVelocities, particleVelocities);

            
            compute.SetInt(ShaderProps.Resolution, (int) resolution);
            
            screenRenderer.material.mainTexture = screen;
            particlesRenderer.material.mainTexture = particleCanvas;
            
            GetComponent<PhotonBuffer>()
                .LinkComputeKernel(compute, computeFieldKernel)
                .LinkComputeKernel(compute, computeParticlesKernel);
            
            GetComponent<BreathBuffer>()
                .LinkComputeKernel(compute, computeFieldKernel)
                .LinkComputeKernel(compute, computeParticlesKernel);
            
            SetVariables();

            ClearScreen();

            ComputeField();
            
            RenderScreen();
        }

        private void SetVariables()
        {
            compute.SetFloat(ShaderProps.Attraction, attraction);
            compute.SetFloat(ShaderProps.Repulsion, repulsion);
            compute.SetFloat(ShaderProps.Scale, scale);
            compute.SetVector(ShaderProps.Steps, new Vector4(steps.x, steps.y, steps.z, 0));
            compute.SetVector(ShaderProps.Threshold, threshold);
            compute.SetVector(ShaderProps.Multiplier, multiplier);
            compute.SetMatrix(ShaderProps.Matrix, transform.localToWorldMatrix);
            compute.SetVector(ShaderProps.Offset, transform.position);
            compute.SetInt(ShaderProps.ScanRadius, (int)scanRadius);

            compute.SetFloat(ShaderProps.DeltaTime, Time.deltaTime);
            compute.SetFloat(ShaderProps.CanvasFadeSpeed, canvasFadeSpeed);
   
            compute.SetFloat(ShaderProps.Speed, speed);
            compute.SetFloat(ShaderProps.Acceleration, acceleration);
            compute.SetFloat(ShaderProps.Gravity, gravity);
            compute.SetFloat(ShaderProps.Lifetime, lifetime);
            
            compute.SetInt(ShaderProps.Dimensions, dimensions);
            compute.SetInt(ShaderProps.ParticleRadius, particleRadius);
            
            compute.SetFloat(ShaderProps.PhaseSpeed, phaseSpeed);
        }

        private RenderTexture CreateTexture3D(GraphicsFormat format = GraphicsFormat.R32G32B32A32_SFloat)
        {
            var tex = new RenderTexture((int)resolution, (int)resolution, 0, format)
            {
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                volumeDepth = dimensions,
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            };

            tex.Create();

            return tex;
        }

        private RenderTexture CreateTexture(GraphicsFormat format = GraphicsFormat.R32G32B32A32_SFloat)
        {
            var tex = new RenderTexture((int)resolution, (int)resolution, 1, format)
            {
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point
            };

            tex.Create();

            return tex;
        }
        
        private void ComputeField()
        {
            for (int i = 0; i < dimensions; i++)
            {
                compute.SetInt(ShaderProps.CurrentDimension, i);
                compute.Dispatch(computeFieldKernel, groups, groups, 1);
            }
        }

        private void ComputeParticles()
        {
            for (int i = 0; i < dimensions; i++)
            {
                compute.SetInt(ShaderProps.CurrentDimension, i);
                compute.Dispatch(computeParticlesKernel, groups, groups, 1);
            }

            compute.SetInt(ShaderProps.CurrentScreen, 0);
            compute.Dispatch(renderParticlesKernel, groups, groups, 1);
        }

        private void ClearScreen()
        {
            compute.Dispatch(clearScreenKernel, groups, groups, 1);
        }
        
        private void ClearField()
        {
            compute.Dispatch(clearFieldKernel, groups, groups, dimensions);
        }

        private void RenderScreen()
        {
            compute.SetInt(ShaderProps.CurrentScreen, currentScreen);
            compute.Dispatch(renderScreenKernel, groups, groups, 1);
        }

        private void SpawnParticle(int amount, Vector3 worldPos)
        {
            var uv = particlesRenderer.transform.InverseTransformPoint(worldPos)
                     + new Vector3(0.5f, 0.5f, 0);
            
            Vector2Int pixelPos = new Vector2Int((int)math.round(uv.x * resolution), 
                (int)math.round(uv.y * resolution));
            
            compute.SetInt(ShaderProps.SpawnCounter, amount);
           
            compute.SetVector(ShaderProps.SpawnPoint, new Vector4(pixelPos.x, pixelPos.y));
            compute.Dispatch(spawnParticlesKernel, 1, 1, 1);
        }
        
        private void CollapseField()
        {
            compute.SetInt(ShaderProps.CurrentDimension, 0);
            compute.Dispatch(collapseFieldKernel, groups, groups, 1);
        }

        private void FadeParticleCanvas()
        {
            compute.Dispatch(fadeParticlesKernel, groups, groups, 1);
        }

        
        private void Update()
        {
            if (Input.GetMouseButton(1))
            {
                SpawnParticle(spawnRate, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }

            if (Input.GetMouseButtonDown(0))
            {
                SpawnParticle(spawnRate, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                ClearField();
                ClearScreen();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CollapseField();
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                currentScreen++;
                if (currentScreen >= dimensions)
                {
                    currentScreen = 0;
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                currentScreen--;
                if (currentScreen < 0)
                {
                    currentScreen = dimensions - 1;
                }
            }
            
            SetVariables();
            
            ComputeField();
            
            ComputeParticles();
            
            RenderScreen();

            FadeParticleCanvas();
        }

        private void OnDestroy()
        {
            if (field) field.Release();
            if (screen) screen.Release();
            if (particlePositions) particlePositions.Release();
            if (particleVelocities) particleVelocities.Release();
            if (particleCanvas) particleCanvas.Release();
            if (resonanceBuffer != null) resonanceBuffer.Release();
            if (modulationBuffer != null) modulationBuffer.Release();
        }
    }
}