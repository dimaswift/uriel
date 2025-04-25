using UnityEngine;  
using System.Collections;  
using System.IO; // Required for Directory and Path operations  

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(Camera))] // Ensure there's a Camera component  
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
        public Color backgroundColor = Color.clear; // Use clear for potential transparency in PNG  

        // --- Internal ---  
        private Renderer targetRenderer;  

        // Define the 6 sides  
        private struct ViewInfo  
        {  
            public string name;  
            public Vector3 direction; // Direction the camera should look FROM  
            public Vector3 up;        // Camera's up vector  

            public ViewInfo(string n, Vector3 dir, Vector3 u)  
            {  
                name = n;  
                direction = dir;  
                up = u;  
            }  
        }  

        private readonly ViewInfo[] views = {  
            new ViewInfo("front",  Vector3.forward, Vector3.up),    // Looking forward (-Z axis view)  
            new ViewInfo("back",   Vector3.back,    Vector3.up),     // Looking backward (+Z axis view)  
            new ViewInfo("top",    Vector3.up,      Vector3.forward),// Looking down (-Y axis view)  
            new ViewInfo("bottom", Vector3.down,    Vector3.back),   // Looking up (+Y axis view)  
            new ViewInfo("right",  Vector3.right,   Vector3.up),    // Looking left (-X axis view)  
            new ViewInfo("left",   Vector3.left,    Vector3.up)     // Looking right (+X axis view)  
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
                enabled = false; // Disable script if no camera  
                return;  
            }  

            if (!captureCamera.orthographic)  
            {  
                Debug.LogWarning("Screenshotter: Capture Camera is not set to Orthographic. Setting it now.", this);  
                captureCamera.orthographic = true;  
            }  
        }  

        // Allows triggering from the Inspector context menu (right-click the script component)  
        [ContextMenu("Capture Cube Sides")]  
        public void TriggerCapture()  
        {  
            if (!ValidateSetup()) return;  

            // Ensure the target directory exists  
            string fullFolderPath = Path.Combine(Application.dataPath, saveFolderPath);  
            if (!Directory.Exists(fullFolderPath))  
            {  
                Directory.CreateDirectory(fullFolderPath);  
                Debug.Log($"Created directory: {fullFolderPath}");  
    #if UNITY_EDITOR  
                UnityEditor.AssetDatabase.Refresh(); // Refresh Assets if in Editor  
    #endif  
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
            if (!targetRenderer) // Ensure renderer is valid  
            {  
               Debug.LogError("Coroutine cannot run, renderer not found.");  
               yield break;  
            }  

            // Store original camera settings  
            Vector3 originalPosition = captureCamera.transform.position;  
            Quaternion originalRotation = captureCamera.transform.rotation;  
            float originalOrthoSize = captureCamera.orthographicSize;  
            CameraClearFlags originalClearFlags = captureCamera.clearFlags;  
            Color originalBackgroundColor = captureCamera.backgroundColor;  
            bool cameraWasEnabled = captureCamera.enabled;  

            // Prepare camera for capture  
            captureCamera.clearFlags = CameraClearFlags.SolidColor;  
            captureCamera.backgroundColor = this.backgroundColor;  
            captureCamera.enabled = true; // Ensure camera is active for rendering  

            Bounds bounds = targetRenderer.bounds;  
            float objectSizeMax = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);  
            // Calculate base distance - far enough to avoid clipping  
            float cameraDistance = objectSizeMax * 1.5f;  
            if (captureCamera.nearClipPlane >= cameraDistance) {  
                 Debug.LogWarning($"Camera's near clip plane ({captureCamera.nearClipPlane}) might be too far. Adjusting distance calculation.", captureCamera);  
                 cameraDistance = captureCamera.nearClipPlane * 1.1f + objectSizeMax; // Ensure it's beyond near clip  
            }  


            Debug.Log($"Starting capture sequence for '{targetCube.name}'...");  

            foreach (ViewInfo view in views)  
            {  
                // --- Position Camera ---  
                // Place camera centered on bounds, offset by direction  
                captureCamera.transform.position = bounds.center - view.direction * cameraDistance;  
                captureCamera.transform.rotation = Quaternion.LookRotation(view.direction, view.up);  

                // --- Adjust Orthographic Size ---  
                // Calculate required size based on the bounds dimensions visible from this angle  
                float boundsWidth, boundsHeight;  
                if (view.name == "top" || view.name == "bottom")  
                {  
                    // Looking along Y axis: Width is X, Height is Z  
                    boundsWidth = bounds.size.x;  
                    boundsHeight = bounds.size.z;  
                }  
                else if (view.name == "left" || view.name == "right")  
                {  
                     // Looking along X axis: Width is Z, Height is Y  
                    boundsWidth = bounds.size.z;  
                    boundsHeight = bounds.size.y;  
                }  
                else // Front or Back  
                {  
                     // Looking along Z axis: Width is X, Height is Y  
                    boundsWidth = bounds.size.x;  
                    boundsHeight = bounds.size.y;  
                }  

                // Ortho size is half the vertical view size.  
                // We need to ensure *both* width and height fit.  
                float requiredVerticalHalfSize = boundsHeight * 0.5f;  
                float requiredHorizontalHalfSize = boundsWidth * 0.5f;  

                // Calculate the orthographic size needed to fit the larger dimension, considering aspect ratio.  
                if (captureCamera.aspect >= 1.0f) // Wider than tall or square  
                {  
                    // Width is the limiting factor if boundsWidth/aspect > boundsHeight  
                    captureCamera.orthographicSize = Mathf.Max(requiredVerticalHalfSize, requiredHorizontalHalfSize / captureCamera.aspect);  
                }  
                else // Taller than wide  
                {  
                     // Height is the limiting factor if boundsHeight > boundsWidth/aspect  
                     captureCamera.orthographicSize = Mathf.Max(requiredVerticalHalfSize, requiredHorizontalHalfSize / captureCamera.aspect);  
                     // Alternative: Fit Height (simpler but might crop width)  
                     // captureCamera.orthographicSize = requiredVerticalHalfSize;  
                }  


                // Apply padding  
                captureCamera.orthographicSize *= (1f + padding);  


                // --- Capture ---  
                // Wait until the end of the frame AFTER camera settings are applied  
                yield return new WaitForEndOfFrame();  

                string fileName = $"{targetCube.name}_{view.name}.png";  
                string filePath = Path.Combine(fullSavePath, fileName);  

                ScreenCapture.CaptureScreenshot(filePath);  
                Debug.Log($"Captured: {filePath}");  

                // Optional: Wait a frame to allow capture process to potentially finish file IO  
                // before the next camera move, though WaitForEndOfFrame is the critical one.  
                yield return null;  
            }  

            // --- Restore Original Settings ---  
            captureCamera.transform.position = originalPosition;  
            captureCamera.transform.rotation = originalRotation;  
            captureCamera.orthographicSize = originalOrthoSize;  
            captureCamera.clearFlags = originalClearFlags;  
            captureCamera.backgroundColor = originalBackgroundColor;  
            captureCamera.enabled = cameraWasEnabled; // Restore original enabled state  

            Debug.Log("Capture sequence finished.");
        }  
    }  
}
