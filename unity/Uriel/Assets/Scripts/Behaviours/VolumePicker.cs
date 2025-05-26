using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Uriel.Behaviours
{
    public class VolumePicker : MonoBehaviour
    {
        public Vector3 Selection => gizmoVoxel.position;
        
        public static VolumePicker Main
        {
            get
            {
                if (main == null)
                {
                    main = FindFirstObjectByType<VolumePicker>();
                }
                return main;
            }
        }

        private static VolumePicker main;
        
        public event Action<Vector3Int> OnSelectionChanged = v => { }; 
        
        [SerializeField] private Material mat;
        [SerializeField] private Transform gizmoVoxel;
       
        private Camera cam;
        
        private Vector3Int current;

        private RaycastHit[] hitBuffer = new RaycastHit[32];

        private Transform[] planes;

        private Label positionField;

        private Material gizmoMat;

        private bool movingGizmo;


        private void Start()
        {
            cam = Camera.main;
            planes = new Transform[3];
            planes[0] = CreatePlane(Vector3.up);
            planes[1] = CreatePlane(Vector3.forward);
            planes[2] = CreatePlane(Vector3.right);
            gizmoMat = new Material(Shader.Find("Sprites/Default"));
            gizmoMat.color = Color.blue;
            gizmoVoxel.GetComponent<MeshRenderer>().material = gizmoMat;
       
        }

        private void OnEnable()
        {
            ConfigureUI();
        }

        
        
        void ConfigureUI()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var container = root.Q<VisualElement>();
            positionField = container.Q<Label>("Position");
            container.Q<Button>("Reset").RegisterCallback<ClickEvent>(c => ResetPlanes());
            UpdatePositionText(Vector3.zero);
        }

        private void ResetPlanes()
        {
            planes[0].forward = Vector3.up;
            planes[1].forward = Vector3.forward;
            planes[2].forward = Vector3.right;
            foreach (Transform plane in planes)
            {
                plane.localPosition = Vector3.zero;
            }
        }

        private bool CastRay(out Transform collider, out RaycastHit result)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastNonAlloc(ray, hitBuffer);
            Transform closestPlane = null;
            float maxDist = float.MaxValue;
            RaycastHit finalHit = new RaycastHit();
            for (int i = 0; i < hits; i++)
            {
                var hit = hitBuffer[i];
        
                if (hit.distance < maxDist)
                {
                    closestPlane = hit.transform;
                    maxDist = hit.distance;
                    finalHit = hit;
                }
            }
            result = finalHit;
            collider = closestPlane;
            return closestPlane != null;
        }

        private bool IsMouseOverGizmo()
        {
            if (CastRay(out var c, out _) && c == gizmoVoxel.transform)
            {
                return true;
            }
            return false;
        }
        
        private Transform CreatePlane(Vector3 forward)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.transform.SetParent(transform);
            plane.transform.forward = forward;
            plane.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Destroy(plane.GetComponent<MeshCollider>());
            var box = plane.AddComponent<BoxCollider>();
            box.size = Vector3.one - Vector3.forward * 0.999f;
         
            return plane.transform;
        }

        private void MoveGizmo()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastNonAlloc(ray, hitBuffer);
            Transform closestPlane = null;
            float maxDist = float.MaxValue;
            RaycastHit finalHit = new RaycastHit();
            for (int i = 0; i < hits; i++)
            {
                foreach (var plane in planes)
                {
                    var hit = hitBuffer[i];
                    if (plane == hit.transform)
                    {
                        if (hit.distance < maxDist)
                        {
                            closestPlane = plane;
                            maxDist = hit.distance;
                            finalHit = hit;
                        }
                    }
                }
            }

            if (closestPlane)
            {
                gizmoVoxel.position = finalHit.point;
                UpdatePositionText(finalHit.point);
            }
        }
        
        private void Update()
        {
            bool mouseOverGizmo = IsMouseOverGizmo();

            gizmoMat.color = mouseOverGizmo || movingGizmo ? Color.green : Color.blue;

            if (Input.GetMouseButtonDown(0) && mouseOverGizmo)
            {
                movingGizmo = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                movingGizmo = false;
            }
             
            if (Input.GetMouseButton(0) && movingGizmo)
            {
                MoveGizmo();
            }
           
            
        }

        private void UpdatePositionText(Vector3 point)
        {
            positionField.text = $"X:{point.x:F} Y:{point.y:F} Z:{point.z:F}";
        }
    }
}
