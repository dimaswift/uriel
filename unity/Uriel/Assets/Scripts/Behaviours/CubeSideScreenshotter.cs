using UnityEngine;  
using System.Collections;  
using System.IO;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(Camera))]
    public class CubeSideScreenshotter : MonoBehaviour  
    {  
        [Header("Setup")]  
        [Tooltip("The cube GameObject to capture screenshots of.")]  
        public GameObject targetCube;  

        [Tooltip("The orthographic camera to use. If null, tries to use this GameObject's Camera.")]  
        public Camera captureCamera;  

        [Header("Output Settings")]  
        [Tooltip("Folder path relative to the Assets folder.")]  
        public string saveFolderPath = "CubeScreenshots";  
        [Tooltip("Padding around the cube bounds (percentage). 0 = tight fit, 0.1 = 10% padding.")]  
        [Range(0f, 0.5f)]  
        public float padding = 0.1f;  
        [Tooltip("Background color for the camera during capture.")]  
        public Color backgroundColor = Color.clear;
        
        private Renderer targetRenderer;  

        private struct ViewInfo  
        {  
            public string name;  
            public Vector3 direction; 
            public Vector3 up; 

            public ViewInfo(string n, Vector3 dir, Vector3 u)  
            {  
                name = n;  
                direction = dir;  
                up = u;  
            }  
        }
        
        private readonly ViewInfo[] views = {  
            new ("front",  Vector3.forward, Vector3.up),   
            new ("back",   Vector3.back,    Vector3.up),   
            new ("top",    Vector3.up,      Vector3.forward),
            new ("bottom", Vector3.down,    Vector3.back), 
            new ("right",  Vector3.right,   Vector3.up),   
            new ("left",   Vector3.left,    Vector3.up)    
        };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                TriggerCapture();
            }
        }

        void Awake()  
        {  
            if (captureCamera == null)  
            {  
                captureCamera = GetComponent<Camera>();  
            }  

            if (captureCamera == null)  
            {  
                Debug.LogError("Screenshotter: No Capture Camera assigned or found on this GameObject.", this);  
                enabled = false;
                return;  
            }  

            if (!captureCamera.orthographic)  
            {  
                Debug.LogWarning("Screenshotter: Capture Camera is not set to Orthographic. Setting it now.", this);  
                captureCamera.orthographic = true;  
            }  
        }  
        
        [ContextMenu("Capture Cube Sides")]  
        public void TriggerCapture()  
        {  
            if (!ValidateSetup()) return;  

            string fullFolderPath = Path.Combine(Application.dataPath, saveFolderPath);  
            if (!Directory.Exists(fullFolderPath))  
            {  
                Directory.CreateDirectory(fullFolderPath);  
                Debug.Log($"Created directory: {fullFolderPath}");
            }
            
            StartCoroutine(CaptureSidesCoroutine(fullFolderPath));  
        }  

        private bool ValidateSetup()  
        {  
            if (targetCube == null)  
            {  
                Debug.LogError("Screenshotter: Target Cube is not assigned.", this);  
                return false;  
            }  

            targetRenderer = targetCube.GetComponent<Renderer>();  
            
            if (targetRenderer == null || !targetRenderer.enabled)  
            {  
                Debug.LogError($"Screenshotter: Target Cube '{targetCube.name}' does not have an enabled Renderer component.", targetCube);  
                return false;  
            }  

            if (captureCamera == null)  
            {  
                 Debug.LogError("Screenshotter: Capture Camera is not assigned or found.", this);  
                 return false;  
            }  

            if (!captureCamera.orthographic)  
            {  
                 Debug.LogError("Screenshotter: Capture Camera must be Orthographic.", captureCamera);  
                 return false;  
            }  
            return true;  
        }  
        
        private IEnumerator CaptureSidesCoroutine(string fullSavePath)  
        {  
            if (!targetRenderer)
            {  
               Debug.LogError("Coroutine cannot run, renderer not found.");  
               yield break;  
            }  

            Vector3 originalPosition = captureCamera.transform.position;  
            Quaternion originalRotation = captureCamera.transform.rotation;  
            float originalOrthoSize = captureCamera.orthographicSize;  
            
            CameraClearFlags originalClearFlags = captureCamera.clearFlags;  
            Color originalBackgroundColor = captureCamera.backgroundColor;  
            bool cameraWasEnabled = captureCamera.enabled;  
            
            captureCamera.clearFlags = CameraClearFlags.SolidColor;  
            captureCamera.backgroundColor = this.backgroundColor;  
            captureCamera.enabled = true;

            Bounds bounds = targetRenderer.bounds;  
            
            float objectSizeMax = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);  

            float cameraDistance = objectSizeMax * 1.5f;  
            
            Debug.Log($"Starting capture sequence for '{targetCube.name}'...");

            var camController = GetComponent<CameraController>();

            if(camController) camController.enabled = false;
            
            foreach (ViewInfo view in views)
            {
                captureCamera.transform.position = bounds.center - view.direction * cameraDistance;  
                captureCamera.transform.rotation = Quaternion.LookRotation(view.direction, view.up);  
                
                float boundsWidth, boundsHeight;  
                
                if (view.name == "top" || view.name == "bottom")  
                {
                    boundsWidth = bounds.size.x;
                    boundsHeight = bounds.size.z;
                }  
                else if (view.name == "left" || view.name == "right")  
                {
                    boundsWidth = bounds.size.z;
                    boundsHeight = bounds.size.y;
                }  
                else
                {  
             
                    boundsWidth = bounds.size.x;
                    boundsHeight = bounds.size.y;
                }  

                float requiredVerticalHalfSize = boundsHeight * 0.5f;  
                
                float requiredHorizontalHalfSize = boundsWidth * 0.5f;

                captureCamera.orthographicSize = Mathf.Max(requiredVerticalHalfSize,
                    requiredHorizontalHalfSize / captureCamera.aspect);
        
                yield return new WaitForEndOfFrame();  

                string fileName = $"{targetCube.name}_{view.name}.png";  
                string filePath = Path.Combine(fullSavePath, fileName);  

                ScreenCapture.CaptureScreenshot(filePath);  
                Debug.Log($"Captured: {filePath}");  
 
                yield return null;  
            }

            if (camController) camController.enabled = true;
            
            captureCamera.transform.position = originalPosition;  
            captureCamera.transform.rotation = originalRotation;  
            captureCamera.orthographicSize = originalOrthoSize;  
            captureCamera.clearFlags = originalClearFlags;  
            captureCamera.backgroundColor = originalBackgroundColor;  
            captureCamera.enabled = cameraWasEnabled;
        }  
    }  
}