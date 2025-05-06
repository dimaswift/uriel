using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class XRaySky : MonoBehaviour
    {
        [SerializeField] private Sky sky;
        [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private Material targetMaterial;
        [SerializeField] private Texture gradient;
        [SerializeField] private uint steps = 64;
        [SerializeField] private Vector2Int resolution = new(128, 128);
        [SerializeField] private Vector2Int captureResolution = new(2048, 2048);
        [SerializeField] private uint depth = 5;
        [SerializeField] private bool grayscale;
        [SerializeField] private float gradientMultiplier = 2f;
        [SerializeField] private float gradientThreshold = 0.2f;
        [SerializeField] private float frequency = 0.5f;
        [SerializeField] private float amplitude = 0.015f;
        [SerializeField] private float min = 2f;
        [SerializeField] private float max = 2.1f;
        [SerializeField] private float focus = 1f;

        [SerializeField] private Transform target;
        [SerializeField] private Transform source;

        private PhotonBuffer photonBuffer;
        
        private RenderTexture texture;
        private int kernelIndex = 0;

        private Vector2Int currentResolution;
        private bool isCapturing;

        private void Start()
        {
            photonBuffer = gameObject.AddComponent<PhotonBuffer>();
            photonBuffer.Init(sky);
            photonBuffer.LinkComputeKernel(computeShader);
        }


        private void SetResolution(Vector2Int res)
        {
            currentResolution = res;
            if (texture != null) texture.Release();
            texture = new RenderTexture(res.x, res.y, 0, GraphicsFormat.R32G32B32A32_SFloat)
            {
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = filterMode
            };
            texture.Create();
            if (targetMaterial != null)
            {
                targetMaterial.SetTexture(ShaderProps.MainTex, texture);
                
            }

            computeShader.SetTexture(kernelIndex, ShaderProps.Result, texture);
            computeShader.SetInt(ShaderProps.Width, currentResolution.x);
            computeShader.SetInt(ShaderProps.Height, currentResolution.y);
        }

        private IEnumerator Capture()
        {
            isCapturing = true;
            yield return new WaitForEndOfFrame();
            SetResolution(captureResolution);
            yield return new WaitForEndOfFrame();
            computeShader.Dispatch(kernelIndex, Mathf.FloorToInt(captureResolution.x / 8f),
                Mathf.FloorToInt(captureResolution.y / 8f), 1);
           
            GraphicsFence fence = Graphics.CreateGraphicsFence(GraphicsFenceType.AsyncQueueSynchronisation,
                SynchronisationStageFlags.ComputeProcessing);
            Graphics.WaitOnAsyncGraphicsFence(fence);
            yield return new WaitForEndOfFrame();
            FileUtils.SaveTextureAsPNG(texture, name, "XRAY_" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper());
            isCapturing = false;
        }
        
        private void Update()
        {
            if (isCapturing)
            {
                return;
            }
  
            if (currentResolution != resolution && !isCapturing)
            {
                SetResolution(resolution);
            }
                
            computeShader.SetVector(ShaderProps.Source, source.position);
            computeShader.SetVector(ShaderProps.Normal, target.forward);
            computeShader.SetFloat(ShaderProps.Amplitude, amplitude);
            computeShader.SetFloat(ShaderProps.Max, max);
            computeShader.SetFloat(ShaderProps.Min, min);
            computeShader.SetFloat(ShaderProps.Frequency, frequency);
            computeShader.SetInt(ShaderProps.Steps, (int)steps);
            computeShader.SetInt(ShaderProps.Depth, (int)depth);
            computeShader.SetTexture(kernelIndex, ShaderProps.Gradient, gradient);
            computeShader.SetBool(ShaderProps.Grayscale, grayscale);
            computeShader.SetFloat(ShaderProps.GradientMultiplier, gradientMultiplier);
            computeShader.SetFloat(ShaderProps.GradientThreshold, gradientThreshold);
         
            computeShader.SetVector(ShaderProps.Target, target.position);
    
            computeShader.Dispatch(kernelIndex, Mathf.FloorToInt(currentResolution.x / 8f),
                Mathf.FloorToInt(currentResolution.y / 8f), 1);
    
            target.localScale = new Vector3(1.0f, (float)currentResolution.y / currentResolution.x, 1);
            computeShader.SetVector(ShaderProps.Size, target.localScale);
            computeShader.SetFloat(ShaderProps.Focus, focus);
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(Capture());
            }
        }

        private void OnDrawGizmos()
        {
            if (target == null || source == null)
            {
                return;
            }

            Vector3 targetPos = target.position;
            Vector3 forward = target.forward.normalized;
            Vector3 right = Vector3.Cross(new Vector3(0, 1, 0), forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            var sourcePos = source.position;
            var size = target.localScale;
            Vector3 a = (targetPos + right * 0.5f * size.x * focus + up * 0.5f *  size.y * focus);
            Vector3 b = (targetPos + right * -0.5f * size.x * focus + up * -0.5f * size.y * focus);
            Vector3 c = (targetPos + right * 0.5f * size.x * focus + up * -0.5f * size.y * focus);
            Vector3 d = (targetPos + right * -0.5f * size.x * focus + up * 0.5f * size.y * focus);
            Gizmos.DrawLine(sourcePos, a);
            Gizmos.DrawLine(sourcePos, b);
            Gizmos.DrawLine(sourcePos, c);
            Gizmos.DrawLine(sourcePos, d);
            Gizmos.DrawLine(a, c);
            Gizmos.DrawLine(d, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(a, d);
            
            Gizmos.DrawSphere(sourcePos, 1);
        }

        private void OnDestroy()
        {
            if(texture) texture.Release();
        }
    }

}
