using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;
using Uriel.Domain;
using Uriel.Rendering;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(WireframeRenderer))]
    public class SculptSolidBehaviour : MonoBehaviour, IMovable, IScalable
    {
        public string ID => snapshot.id;

        public bool Selected
        {
            get => selected;
            set
            {
                Init();
                gizmo.SetState(value ? SelectableState.Selected : SelectableState.None);
                wireframeRenderer.SetColor(value ? Color.green : Color.grey);
                selected = value;
            }
        }
        
        public Bounds Bounds => new (transform.position, transform.localScale);
        

        public Vector3 Position
        {
            get => transform.localPosition;
            set => transform.localPosition = value;
        }

        public Vector3 Scale
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }

        public ISnapshot Current => snapshot;
        
        [SerializeField] private SculptSolidSnapshot snapshot = new();
        
        private WireframeRenderer wireframeRenderer;
        private bool selected;
        private SculptSolidType? renderedType;
        private SelectableGizmo gizmo;

        private bool initialized = false;

        private void Init()
        {
            if (initialized) return;
            gizmo = GetComponentInChildren<SelectableGizmo>();
            wireframeRenderer = GetComponent<WireframeRenderer>();
            initialized = true;
        }
        
        private void Awake()
        {
            Init();
            Selected = false;
           
        }
        
        public void SetState(SelectableState state)
        {
            gizmo?.SetState(state);
        }
        private void Update()
        {
            if (renderedType != snapshot.solid.type)
            {
                renderedType = snapshot.solid.type;
                wireframeRenderer.SetType(renderedType.Value);
            }
        }

        public SculptSolid GetSolid()
        {
            var m = new Matrix4x4();
            m.SetTRS(transform.localPosition, transform.localRotation, transform.localScale);
            snapshot.solid.invTransform = m.inverse;
            return snapshot.solid;
        }

        
        public SculptSolidSnapshot CreateSnapshot()
        {
            return new SculptSolidSnapshot()
            {   
                parentId = snapshot.parentId,
                id = snapshot.id,
                scale = transform.localScale,
                solid = snapshot.solid,
                position = transform.localPosition,
                rotation = transform.localEulerAngles
            };
        }

        ISnapshot IModifiable.CreateSnapshot() => CreateSnapshot();
        
        public void Restore(ISnapshot snap)
        {
            if (snap is not SculptSolidSnapshot s)
            {
                return;
            }

            transform.localScale = s.scale;
            transform.localEulerAngles = s.rotation;
            transform.localPosition = s.position;
    
            snapshot = s;
        }

    }
}