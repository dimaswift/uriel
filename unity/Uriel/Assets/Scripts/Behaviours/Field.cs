using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class Field : MonoBehaviour
    {
        [SerializeField] private int shellCount = 5;
        [SerializeField] private Transform quad;
        [SerializeField] private FilterMode textureMode;
        [SerializeField] private int width = 64;
        [SerializeField] private int height = 64;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Material targetMat;
        [SerializeField] private float frequency = 1;
        [SerializeField] private float amplitude = 1;
        [SerializeField] private float colorScale = 1;
        [SerializeField] private float colorOffset = 1;
        [SerializeField] private Vector4 colorSteps;
        [SerializeField] private int sourceCount = 1;
        [SerializeField] private float threshold = 1;
        [SerializeField] private float time = 1;
        [Range(0f, 1f)] [SerializeField] private float speed = 1;
        [SerializeField] private Vector2 offset;

        [Range(0f, 1f)] [SerializeField] private float angle = 1;
        [Range(0f, 1f)] [SerializeField] private float frequencyFine;
        [Range(0f, 1f)] [SerializeField] private float amplitudeFine;
        
        private RenderTexture result;
        private int initKernel;
        private int tickKernel;
        private int threadGroupsX;
        private int threadGroupsY;

        private static readonly int SourcesKeyword = Shader.PropertyToID("Sources");
        private static readonly int ResultKeyword = Shader.PropertyToID("Result");
        private static readonly int WidthKeyword = Shader.PropertyToID("Width");
        private static readonly int HeightKeyword = Shader.PropertyToID("Height");
        private static readonly int SourcesCountKeyword = Shader.PropertyToID("SourcesCount");
        private static readonly int FrequencyKeyword = Shader.PropertyToID("Frequency");
        private static readonly int AmplitudeKeyword = Shader.PropertyToID("Amplitude");
        private static readonly int ThresholdKeyword = Shader.PropertyToID("Threshold");
        private static readonly int OffsetKeyword = Shader.PropertyToID("Offset");
        private static readonly int AngleKeyword = Shader.PropertyToID("Angle");
        private static readonly int TimeKeyword = Shader.PropertyToID("Time");
        private static readonly int ColorScaleKeyword = Shader.PropertyToID("ColorScale");
        private static readonly int ColorOffsetKeyword = Shader.PropertyToID("ColorOffset");
        private static readonly int ColorStepsKeyword = Shader.PropertyToID("ColorSteps");
        private static readonly int ShellCountKeyword = Shader.PropertyToID("ShellCount");
        private void Start()
        {
            threadGroupsX = Mathf.FloorToInt(width / 8f);
            threadGroupsY = Mathf.FloorToInt(height / 8f);
            tickKernel = compute.FindKernel("Tick");
            initKernel = compute.FindKernel("Init");
            quad.localScale = new Vector3(width, height, 1) * 0.1f;
            FindFirstObjectByType<CameraController>()?.SetSize(height * 0.5f * 0.1f);
            result = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB64);
            result.enableRandomWrite = true;
            result.wrapMode = TextureWrapMode.Repeat;
            result.filterMode = textureMode;
            result.Create();

            compute.SetTexture(tickKernel, ResultKeyword, result);
            compute.SetTexture(initKernel, ResultKeyword, result);
            compute.SetInt(WidthKeyword, width);
            compute.SetInt(HeightKeyword, height);
            targetMat.SetTexture("_BaseMap", result);
            
            UpdateSources();
   
            compute.Dispatch(initKernel, threadGroupsX, threadGroupsY, 1);
        }



        private void UpdateSources()
        {
            compute.SetInt(SourcesCountKeyword, sourceCount);
        }


        private void SaveTexture()
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = result;
            Texture2D tex = new Texture2D(result.width, result.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, result.width, result.height), 0, 0);
            tex.Apply();
            byte[] pngData = tex.EncodeToPNG();
            RenderTexture.active = currentActiveRT;
            Destroy(tex);
            string filePath = Application.dataPath + $"/{Guid.NewGuid()}.png";
            System.IO.File.WriteAllBytes(filePath, pngData);
            Debug.Log("RenderTexture saved to: " + filePath);
        }
        
        private void Update()
        {
            time += Time.deltaTime * speed;
            compute.SetFloat(FrequencyKeyword, frequency + frequencyFine);
            compute.SetFloat(AmplitudeKeyword, amplitude + amplitudeFine);
            compute.SetFloat(ThresholdKeyword, threshold);
            compute.SetVector(OffsetKeyword, offset);
            compute.SetFloat(AngleKeyword, angle);
            compute.SetFloat(TimeKeyword, time);
            compute.SetFloat(ColorScaleKeyword, colorScale);
            compute.SetFloat(ColorOffsetKeyword, colorOffset);
            compute.SetVector(ColorStepsKeyword, colorSteps);
            compute.SetInt(ShellCountKeyword, shellCount);
            UpdateSources();
            compute.Dispatch(tickKernel, threadGroupsX, threadGroupsY, 1);

            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveTexture();
            }
        }

        private void OnDestroy()
        {
            if (result != null) result.Release();
        }
    }
}
