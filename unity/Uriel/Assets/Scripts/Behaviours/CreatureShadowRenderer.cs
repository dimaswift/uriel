using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(CreatureProcessor))]
    public class CreatureShadowRenderer : MonoBehaviour
    {
        [SerializeField] private Material targetMaterial;  
        [SerializeField] private int resolution = 512;  
        [SerializeField] private ComputeShader compute;  
        
        [Header("Ray Configuration")]  
        [SerializeField] private Transform source;  
        [SerializeField] private Transform target;  
        [SerializeField] private float size = 1.0f;  
        [SerializeField] private int steps = 64;
        [SerializeField] private float frequency = 3.2f;
        [Range(0f, 0.1f)] [SerializeField] private float frequencyFine = 0f;
        [Range(0f, 1f)] [SerializeField] private float strength = 1.0f;
        [Range(-5f, 5f)] [SerializeField] private float min = 0.0f;
        [Range(-5f, 5f)] [SerializeField] private float max = 1.0f;
        [SerializeField] private bool grayscale;
        [SerializeField] private bool saturate; 
        private RenderTexture texture;
        private int kernelIndex;
        private CreatureProcessor processor;

        [Header("Capture Settings")] [SerializeField]
        private string saveDirectory = "Shadows";

        [SerializeField] private string filePrefix = "Adam";  
        
        private void Start()  
        {
            processor = GetComponent<CreatureProcessor>();
            processor.OnBufferCreated += OnBufferCreated;
            InitializeTexture();
        }

        private void OnBufferCreated(ComputeBuffer buffer)
        {
            compute.SetInt("_GeneCount", processor.GeneCount);
            compute.SetBuffer(kernelIndex, "_GeneBuffer", buffer);  
        }

        private void OnDestroy()  
        {  
            CleanupResources();  
        }  
        
        private void Update()  
        {  
            if (compute == null || texture == null || targetMaterial == null)  
                return;
            processor.UpdateGeneBuffer();
            UpdateShaderParameters();  
            DispatchShader();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SaveTextureAsPNG();
            }
        }  
        
        private void SaveTextureAsPNG()  
        {  
            // Don't save if texture doesn't exist  
            if (texture == null)  
                return;  
                
            try  
            {  
                // Create a temporary RenderTexture to handle conversion of floating point values if needed  
                RenderTexture tempRT = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);  
                Graphics.Blit(texture, tempRT);  
                
                // Create a Texture2D and read pixels from the RenderTexture  
                Texture2D tex2D = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGBA32, false);  
                RenderTexture.active = tempRT;  
                tex2D.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);  
                tex2D.Apply();  
                RenderTexture.active = null;  
                RenderTexture.ReleaseTemporary(tempRT);  
                
                // Create directory if it doesn't exist  
                string directory = Path.Combine(Application.dataPath, saveDirectory);  
                if (!Directory.Exists(directory))  
                {  
                    Directory.CreateDirectory(directory);  
                }  
                
                // Generate unique filename with timestamp  
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");  
                string filename = $"{filePrefix}_{timestamp}.png";  
                string filePath = Path.Combine(directory, filename);  
                
                // Convert to PNG and save  
                byte[] pngBytes = tex2D.EncodeToPNG();  
                File.WriteAllBytes(filePath, pngBytes);  
                
                // Cleanup  
                Destroy(tex2D);  
                
                Debug.Log($"Saved texture to: {filePath}");  
                
                // Show a visual indicator in the editor that the capture was successful  
                #if UNITY_EDITOR  
                UnityEditor.EditorApplication.Beep();  
                #endif  
            }  
            catch (Exception e)  
            {  
                Debug.LogError($"Error saving texture: {e.Message}");  
            }  
        }  
        
        private void InitializeTexture()  
        {  
           
            if (texture != null)  
            {  
                texture.Release();  
            }  
            
            texture = new RenderTexture(resolution, resolution, 0, GraphicsFormat.R32G32B32A32_SFloat)  
            {  
                enableRandomWrite = true,  
                useMipMap = false,  
                autoGenerateMips = false,  
                filterMode = FilterMode.Bilinear  
            };  
            texture.Create();  
            
            if (targetMaterial != null)  
            {  
                targetMaterial.SetTexture("_BaseMap", texture);  
            }  
            
            kernelIndex = compute.FindKernel("CSMain");  
        }  
        

        
        private void UpdateShaderParameters()  
        {
            compute.SetInt("_Resolution", resolution);  
            compute.SetInt("_Steps", steps);  
            compute.SetFloat("_Size", size);  
            compute.SetFloat("_Frequency", frequency + frequencyFine);
            compute.SetBool("_Grayscale", grayscale);
            compute.SetBool("_Saturate", saturate);
            compute.SetFloat("_Strength", strength);
            compute.SetFloat("_Min", min);
            compute.SetFloat("_Max", max);
            if (source != null)  
                compute.SetVector("_Source", source.position);  
            if (target != null)  
            {  
                compute.SetVector("_Target", target.position);  
                compute.SetVector("_Normal", target.forward);  
            }  
            
            compute.SetTexture(kernelIndex, "_Result", texture);  
        }  
        
        private void DispatchShader()  
        {
            int threadGroupsX = Mathf.CeilToInt(resolution / 32.0f);  
            int threadGroupsY = Mathf.CeilToInt(resolution / 32.0f);  
            
            compute.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);  
        }  
        
        private void CleanupResources()  
        {  
            if (texture != null)  
            {  
                texture.Release();  
                texture = null;  
            }
        }  
    }
}