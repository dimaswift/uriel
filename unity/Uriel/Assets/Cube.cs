using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Uriel.Behaviours
{
    public class Cube : MonoBehaviour
    {
        [Header("Rotation Settings")]  
        [Tooltip("Sensitivity of mouse drag rotation")]  
        public float mouseSensitivity = 5.0f;  
        
        [Tooltip("Duration of the snapping animation in seconds")]  
        [Range(0.1f, 1.0f)]  
        public float snapDuration = 0.3f;  
        
        [Tooltip("Easing curve for the snap rotation")]  
        public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  
        

        // Internal state tracking  
        private bool isDragging = false;  
        private bool isSnapping = false;  
        private Vector3 lastMousePosition;  
        private Quaternion targetRotation;  

        void Update()  
        {  
            // Handle mouse input  
            if (Input.GetMouseButtonDown(0) && !isSnapping)  
            {  
                // Start dragging  
                isDragging = true;  
                lastMousePosition = Input.mousePosition;  
            }  
            else if (Input.GetMouseButtonUp(0) && isDragging)  
            {  
                // Stop dragging and snap to nearest 90 degrees  
                isDragging = false;  
                SnapToNearestRightAngle();  
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                transform.Rotate(Vector3.forward, 90, Space.World);  
                SnapToNearestRightAngle();
            }  
            // Rotate while dragging  
            if (isDragging)  
            {  
                Vector3 delta = Input.mousePosition - lastMousePosition;  
                
                // Convert mouse movement to rotation  
                // (Invert X axis so dragging left rotates left)  
                transform.Rotate(Vector3.up, delta.x * mouseSensitivity * Time.deltaTime, Space.World);  
                transform.Rotate(Vector3.right, -delta.y * mouseSensitivity * Time.deltaTime, Space.World);  
                
                lastMousePosition = Input.mousePosition;  
            }  
        }  

        private void SnapToNearestRightAngle()  
        {  
            // Calculate the nearest 90-degree rotation  
            targetRotation = FindNearestRightAngleRotation(transform.rotation);  
            
            // Start the snapping animation  
            StartCoroutine(SnapAnimation());  
        }  

        private Quaternion FindNearestRightAngleRotation(Quaternion currentRotation)  
        {  
            // Convert to Euler angles for easier manipulation  
            Vector3 euler = currentRotation.eulerAngles;  
            
            // Round each component to the nearest 90 degrees  
            euler.x = Mathf.Round(euler.x / 90f) * 90f;  
            euler.y = Mathf.Round(euler.y / 90f) * 90f;  
            euler.z = Mathf.Round(euler.z / 90f) * 90f;  
            
            // Convert back to Quaternion  
            return Quaternion.Euler(euler);  
        }  

        private IEnumerator SnapAnimation()  
        {  
            isSnapping = true;  
 
            // Store starting rotation  
            Quaternion startRotation = transform.rotation;  
            float timeElapsed = 0;  
            
            while (timeElapsed < snapDuration)  
            {  
                // Calculate progress with curve for easing  
                float t = timeElapsed / snapDuration;  
                float curvedT = snapCurve.Evaluate(t);  
                
                // Apply rotation  
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curvedT);  
                
                // Increment time  
                timeElapsed += Time.deltaTime;  
                
                // Wait for next frame  
                yield return null;  
            }  
            
            // Ensure exact rotation at the end  
            transform.rotation = targetRotation;  
            
            // Reset state  
            isSnapping = false;  
        }  
    }

}
