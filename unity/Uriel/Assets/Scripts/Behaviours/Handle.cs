using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;

namespace Uriel.Behaviours
{
    public abstract class Handle<T> : MonoBehaviour where T : class, IModifiable, ISelectable
    {
        [SerializeField] private AxisGizmo axisGizmo;
        public bool IsDragging => dragging;

        public bool Enabled
        {
            get => isEnabled;
            set
            {
                axisGizmo.gameObject.SetActive(value && selectionSize > 0 && Selected);
                isEnabled = value;
            }
        }

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
               
                if (selected)
                {
                    OnSelectionChanged();
                }
                else
                {
                    Enabled = false;
                }
            }
        }

        protected CommandHistory CommandHistory;
        private Selector selector;
        private Camera cam;
        private bool selected;
        
        private readonly List<T> buffer = new();
        private readonly List<Func<bool>> blockers = new();
        
        private bool dragging;
        private Vector3 moveStartPoint;
        private Axis axis;

        private Vector3 gizmoClickPoint;
        private Plane plane;
        private bool isEnabled;
        private int selectionSize;

        protected IReadOnlyList<T> Targets => buffer;
        
        public void AddBlocker(Func<bool> shouldBlock)
        {
            blockers.Add(shouldBlock);
        }

        private void Awake()
        {
            cam = Camera.main;
            selector = GetComponent<Selector>();
            CommandHistory = GetComponent<CommandHistory>();
            selector.OnSelectionChanged += OnSelectionChanged;
            axisGizmo.gameObject.SetActive(false);
            selector.AddBlocker(() => IsHoveringGizmo);
            CommandHistory.OnUndoOrRedo += OnSelectionChanged;
            Enabled = false;
        }

        protected Vector3 GetCenter()
        {
            var c = 0;
            var center = new Vector3();
            foreach (var modifiable in selector.GetSelected<T>())
            {
                c++;
                center += modifiable.transform.position;
            }

            if (c == 0)
            {
                return center;
            }

            return center / c;
        }

        private void OnSelectionChanged()
        {
            selectionSize = 0;
            var center = new Vector3();
            buffer.Clear();
            foreach (var modifiable in selector.GetSelected<T>())
            {
                selectionSize++;
                center += modifiable.transform.position;
                buffer.Add(modifiable);
            }

            if (selectionSize == 0)
            {
                Enabled = false;
                return;
            }

            Enabled = true;

            axisGizmo.transform.position = center / selectionSize;
        }

        private bool IsBlocked()
        {
            foreach (var blocker in blockers)
            {
                if (blocker())
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract void OnFinishDragging();

        protected abstract void OnDrag(Vector3 delta, Axis axis);
        
        private void FinishDragging()
        {
            OnFinishDragging();
            dragging = false;
        }
        
        private Vector3 GetPlanePointer()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out var dist))
            {
                return ray.GetPoint(dist);
            }
            return Vector3.zero;
        }


        private void BeginDrag()
        {
            if (!Enabled || IsBlocked())
            {
                return;
            }

            if (axisGizmo.SelectedAxis == null)
            {
                return;
            }
          
            axis = axisGizmo.SelectedAxis.Value;
  
            if (buffer.Count == 0)
            {
                return;
            }
            
            plane = new Plane(Vector3.up, gizmoClickPoint);
            switch (axisGizmo.SelectedAxis)
            {
                case Axis.X:
                    plane = new Plane(Vector3.up, gizmoClickPoint);
                    if (Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.up)) <= 0.01f)
                    {
                        plane = new Plane(Vector3.forward, gizmoClickPoint);
                    }
                    break;
                case Axis.Y:
                    plane = new Plane(Vector3.forward, gizmoClickPoint);
                    if (Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.forward)) <= 0.01f)
                    {
                        plane = new Plane(Vector3.right, gizmoClickPoint);
                    }
                    break;
                case Axis.Z:
                    plane = new Plane(Vector3.up, gizmoClickPoint);
                    if (Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.up)) <= 0.01f)
                    {
                        plane = new Plane(Vector3.right, gizmoClickPoint);
                    }
                    break;
            }
            

            gizmoClickPoint = axisGizmo.transform.position;
            dragging = buffer.Count > 0;
            moveStartPoint = GetPlanePointer();
            OnBeginDrag();
        }

        protected abstract void OnBeginDrag();

        public void Reset()
        {
            dragging = false;
        }

        public bool IsHoveringGizmo => axisGizmo.gameObject.activeSelf && axisGizmo.SelectedAxis != null;

        public void Update()
        {
            if (!IsDragging && Input.GetMouseButton(0) && Input.mousePositionDelta.magnitude > 0.1f)
            {
                BeginDrag();
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                if (dragging)
                {
                    FinishDragging();
                }
            }

            if (IsDragging)
            {
                var mouse = GetPlanePointer();
                var mouseDelta = mouse - moveStartPoint;
                switch (axis)
                {
                    case Axis.X:
                        mouseDelta.y = 0;
                        mouseDelta.z = 0;
                        break;
                    case Axis.Y:
                        mouseDelta.x = 0;
                        mouseDelta.z = 0;
                        break;
                    case Axis.Z:
                        mouseDelta.y = 0;
                        mouseDelta.x = 0;
                        break;
                }
                OnDrag(mouseDelta, axis);

            }
        }

        protected void MoveGizmoToCenter()
        {
            axisGizmo.transform.position = GetCenter();
        }
        
        protected void MoveGizmo(Vector3 delta)
        {
            axisGizmo.transform.position = gizmoClickPoint + delta;
        }
    }
}