using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(BreathBuffer))]
    [RequireComponent(typeof(PhotonBuffer))]
    public class ResonanceCascade : MonoBehaviour
    {
        [SerializeField] private ComputeShader compute;
        [SerializeField] private MeshRenderer screenRenderer;
        [SerializeField] private MeshRenderer particlesRenderer;
        [SerializeField] private ResonanceCascadeConfig config;
        
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
            
            compute.SetTexture(renderParticlesKernel, ShaderProps.Gradient, config.gradient);
            compute.SetFloat(ShaderProps.GradientSize, config.gradient.width);
            
            compute.GetKernelThreadGroupSizes(computeFieldKernel, out threads, out _, out _);
         
            groups = Mathf.CeilToInt(config.resolution / (float)threads);
            
            screen = CreateTexture();
            particleCanvas = CreateTexture();
            
            field = CreateTexture3D();
            particlePositions = CreateTexture3D();
            particleVelocities = CreateTexture3D();

            modulationBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(Modulation)));
            resonanceBuffer = new ComputeBuffer(config.dimensions, Marshal.SizeOf(typeof(Resonance)));
            resonances = new Resonance[config.dimensions];
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

            
            compute.SetInt(ShaderProps.Resolution, (int)config.resolution);
            
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
           
            compute.SetFloat(ShaderProps.GradientMultiplier, config.gradientMultiplier);
            compute.SetFloat(ShaderProps.GradientThreshold, config.gradientThreshold);
            compute.SetFloat(ShaderProps.Radius, config.radius);
            compute.SetFloat(ShaderProps.DeltaTime, Time.deltaTime);
            
            compute.SetFloat(ShaderProps.Attraction, config.attraction);
            compute.SetFloat(ShaderProps.Repulsion, config.repulsion);
            compute.SetFloat(ShaderProps.Scale, config.scale);
    
            compute.SetVector(ShaderProps.Threshold, config.threshold);
            compute.SetVector(ShaderProps.Multiplier, config.multiplier);
            compute.SetMatrix(ShaderProps.Matrix, transform.localToWorldMatrix);
            compute.SetVector(ShaderProps.Offset, transform.position);
            compute.SetInt(ShaderProps.ScanRadius, (int)config.scanRadius);

          
            compute.SetFloat(ShaderProps.CanvasFadeSpeed, config.canvasFadeSpeed);
   
            compute.SetFloat(ShaderProps.Speed, config.speed);
            compute.SetFloat(ShaderProps.Acceleration, config.acceleration);
            compute.SetFloat(ShaderProps.Gravity, config.gravity);
            compute.SetFloat(ShaderProps.Lifetime, config.lifetime);
            
            compute.SetInt(ShaderProps.Dimensions, config.dimensions);
            compute.SetInt(ShaderProps.ParticleRadius, config.particleRadius);
            
            compute.SetFloat(ShaderProps.PhaseSpeed, config.phaseSpeed);
        }

        private RenderTexture CreateTexture3D(GraphicsFormat format = GraphicsFormat.R32G32B32A32_SFloat)
        {
            var tex = new RenderTexture((int)config.resolution, (int)config.resolution, 0, format)
            {
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                volumeDepth = config.dimensions,
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            };

            tex.Create();

            return tex;
        }

        private RenderTexture CreateTexture(GraphicsFormat format = GraphicsFormat.R32G32B32A32_SFloat)
        {
            var tex = new RenderTexture((int)config.resolution, (int)config.resolution, 1, format)
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
            for (int i = 0; i < config.dimensions; i++)
            {
                compute.SetInt(ShaderProps.CurrentDimension, i);
                compute.Dispatch(computeFieldKernel, groups, groups, 1);
            }
        }

        private void ComputeParticles()
        {
            for (int i = 0; i < config.dimensions; i++)
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
            compute.Dispatch(clearFieldKernel, groups, groups, config.dimensions);
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
            
            Vector2Int pixelPos = new Vector2Int((int)math.round(uv.x * config.resolution), 
                (int)math.round(uv.y * config.resolution));
            
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
                SpawnParticle(config.spawnRate, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }

            if (Input.GetMouseButtonDown(0))
            {
                SpawnParticle(config.spawnRate, Camera.main.ScreenToWorldPoint(Input.mousePosition));
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
                if (currentScreen >= config.dimensions)
                {
                    currentScreen = 0;
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                currentScreen--;
                if (currentScreen < 0)
                {
                    currentScreen = config.dimensions - 1;
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                FileUtils.SaveTextureAsPNG(particleCanvas, "Cascades", Guid.NewGuid()
                    .ToString()
                    .ToUpper()
                    .Substring(0,3));
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