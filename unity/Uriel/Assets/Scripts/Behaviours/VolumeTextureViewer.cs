namespace Uriel.Behaviours
{ 
    
    using UnityEngine;  

 
    public class VolumeTextureViewer : MonoBehaviour  
    {
       
        [Range(0.001f, 1.0f)]  
        public float alpha = 0.137f;  
        
        [Range(0.0f, 1.0f)]  
        public float threshold = 0.1f;  
        
        [Range(1, 20)]  
        public int quality = 8;  
        
        [Range(0.001f, 0.1f)]  
        public float stepSize = 0.01f;  
        
        [Header("Slice Settings")]  
        public bool enableSlicing = false;  
        
        [Range(0.0f, 1.0f)]  
        public float slicePosition = 0.5f;  
        
        [Range(0.0f, 0.5f)]  
        public float sliceThickness = 0.1f;  
        
        [Header("Rotation Controls")]  
        public float rotationSpeed = 1.0f;  
        private Vector3 lastMousePosition;  
        private bool isDragging = false;  
        
        // Material reference  
        public Material viewerMaterial;  
        
        void OnEnable()  
        {  
            
          
        }  
        
        void Update()  
        {  
            if (viewerMaterial == null)  
                return;  
                
            // Update material properties  
     
            viewerMaterial.SetFloat("_Alpha", alpha);  
            viewerMaterial.SetFloat("_Threshold", threshold);  
            viewerMaterial.SetInt("_Quality", quality);  
            viewerMaterial.SetFloat("_StepSize", stepSize);  
            viewerMaterial.SetFloat("_SlicePosition", slicePosition);  
            viewerMaterial.SetFloat("_SliceThickness", sliceThickness);  
            viewerMaterial.SetFloat("_SliceEnabled", enableSlicing ? 1.0f : 0.0f);  
            
            // Handle rotation in editor  
            #if UNITY_EDITOR  
            HandleRotation();  
            #endif  
        }  
        
        void HandleRotation()  
        {
            // Game mode rotation handling  
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                transform.Rotate(Vector3.up, -delta.x * rotationSpeed * 0.1f, Space.World);
                transform.Rotate(Vector3.right, delta.y * rotationSpeed * 0.1f, Space.World);
                lastMousePosition = Input.mousePosition;
            }  
        }  
      
        void OnDestroy()  
        {  
            if (viewerMaterial != null)  
            {  
                if (Application.isPlaying)  
                {  
                    Destroy(viewerMaterial);  
                }  
                else  
                {   
                    DestroyImmediate(viewerMaterial);  
                }  
            }  
        }  
    }  
}