using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(PhotonBuffer))]
    public class Painter : MonoBehaviour
    {
        [SerializeField] private int dimensions = 5;
        [SerializeField] [Range(1,32)] private uint brushSize = 4;
        [SerializeField] [Range(1, 32)] private uint paintRate = 1;
        [SerializeField] [Range(1, 32)] private uint scanRadius = 1;
        [SerializeField] private int particlesCount = 32;
        [SerializeField] private Lumen lumen;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private uint resolution = 128;
        [SerializeField] private Vector3 threshold;
        [SerializeField] private Vector3 multiplier;
        [SerializeField] private MeshRenderer fieldRenderer;
        [SerializeField] private MeshRenderer canvasRenderer;
        [SerializeField] private MeshRenderer interferenceRenderer;
        [SerializeField] private Vector3Int steps = new (1,1,1);
        [SerializeField] private float scale = 1f;
        [SerializeField] private float canvasFadeSpeed = 1f;
        [SerializeField] private float phaseSpeed;
        [SerializeField] private float lifetime;
        [SerializeField] private float gravity;
        [SerializeField] private float acceleration;
        [SerializeField] private float speed;
        [SerializeField] private float attraction = 1;
        [SerializeField] private float repulsion = 1;
        
        private RenderTexture canvas;
        private RenderTexture field;
        private ComputeBuffer particlesBuffer;
        private ComputeBuffer brushBuffer;

        private int calculateFieldKernel;
        private int simulateParticlesKernel;
        private int clearCanvasKernel;
        private int processBrushKernel;
        private int canvasFadeKernel;
        
        private Particle[] particles;
        private readonly Brush[] brush = new Brush[1];
        

        private void Start()
        {
            simulateParticlesKernel = compute.FindKernel("SimulateParticles");
            calculateFieldKernel = compute.FindKernel("CalculateField");
            clearCanvasKernel = compute.FindKernel("ClearCanvas");
            processBrushKernel = compute.FindKernel("ProcessBrush");
            canvasFadeKernel = compute.FindKernel("FadeCanvas");
            
            gameObject.GetComponent<PhotonBuffer>().LinkComputeKernel(compute);
            
       
            canvas = CreateTexture();
            field = CreateTexture();

            brushBuffer = new ComputeBuffer((int) resolution, Marshal.SizeOf(typeof(Brush)));
            particlesBuffer = new ComputeBuffer(particlesCount, Marshal.SizeOf(typeof(Particle)));
            particles = new Particle[particlesCount];
            
            compute.SetTexture(calculateFieldKernel, ShaderProps.Field, field);
            compute.SetTexture(calculateFieldKernel, ShaderProps.Interference, field);
            compute.SetBuffer(processBrushKernel, ShaderProps.Brush, brushBuffer);
            compute.SetBuffer(processBrushKernel, ShaderProps.Particles, particlesBuffer);
            compute.SetTexture(canvasFadeKernel, ShaderProps.Canvas, canvas);
            
            compute.SetTexture(simulateParticlesKernel, ShaderProps.Canvas, canvas);
            compute.SetTexture(simulateParticlesKernel, ShaderProps.Interference, field);
            compute.SetBuffer(simulateParticlesKernel, ShaderProps.Particles, particlesBuffer);
            
            compute.SetTexture(clearCanvasKernel, ShaderProps.Canvas, canvas);
            
            compute.SetInt(ShaderProps.ParticlesCount, particlesCount);
            compute.SetInt(ShaderProps.Resolution, (int) resolution);

            
            fieldRenderer.material.mainTexture = field;
            canvasRenderer.material.mainTexture = canvas;
            interferenceRenderer.material.mainTexture = field;

            SetVariables();
            
            ResetParticles();
            DrawField();
            ClearCanvas();
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
            compute.SetInt(ShaderProps.BrushSize, (int)brushSize);

            compute.SetFloat(ShaderProps.Speed, speed);
            compute.SetFloat(ShaderProps.Acceleration, acceleration);
            compute.SetFloat(ShaderProps.Gravity, gravity);
            compute.SetFloat(ShaderProps.Lifetime, lifetime);

            compute.SetFloat(ShaderProps.PhaseSpeed, phaseSpeed);
        }

        private RenderTexture CreateTexture(GraphicsFormat format = GraphicsFormat.R32G32B32A32_SFloat)
        {
            var tex = new RenderTexture((int)resolution, (int)resolution, dimensions, format)
            {
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                volumeDepth = dimensions,
            };

            tex.Create();

            return tex;
        }

        private void ClearCanvas()
        {
            compute.Dispatch(clearCanvasKernel, Mathf.CeilToInt(resolution / 8f), Mathf.CeilToInt(resolution / 8f), 1);
        }
        
        private void DrawField()
        {
            compute.Dispatch(calculateFieldKernel, Mathf.CeilToInt(resolution / 8f), Mathf.CeilToInt(resolution / 8f), 1);
        }

        private void ResetParticles()
        {
            for (int i = 0; i < particlesCount; i++)
            {
                particles[i] = new Particle()
                {
                    mass = 1,
                    size = 0,
                    position = new Vector3(),
                    velocity = new Vector3(),
                    charge = 0
                };
            }
            particlesBuffer.SetData(particles);
        }

        private void Draw(Vector3 worldPos)
        {
            var localPos =
                canvasRenderer.transform.parent.InverseTransformPoint(worldPos);
            brushBuffer.GetData(brush);
            brush[0].amount = paintRate;
            brush[0].position = (localPos + new Vector3(0.5f, 0.5f, 0)) * resolution;
            brushBuffer.SetData(brush);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ResetParticles();
                ClearCanvas();
            }
            
            if (Input.GetMouseButton(0))
            {
                Draw(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }

            if (Input.GetMouseButtonDown(1))
            {
                Draw(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            
            SetVariables();
            DrawField();
            
            compute.Dispatch(processBrushKernel, 1, 1, 1);
            compute.Dispatch(simulateParticlesKernel, Mathf.CeilToInt(particlesCount / 8f), 1, 1);
            compute.Dispatch(canvasFadeKernel, Mathf.CeilToInt(resolution / 8f), Mathf.CeilToInt(resolution / 8f), 1);

            if (Input.GetKeyDown(KeyCode.R))
            {
                DrawField();
            }

        }

        private void OnDestroy()
        {
            if (field) field.Release();
            if (canvas) canvas.Release();
            if (particlesBuffer != null) particlesBuffer.Release();
            if (brushBuffer != null) brushBuffer.Release();
            
        }
    }
}