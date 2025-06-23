using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Volume : MonoBehaviour, IMovable
    {
        
        public event Action OnRestored = () => { };
        public ISnapshot Current => snapshot;
        public bool Selected
        {
            get => selected;
            set
            {
                gizmo.SetState(value ? SelectableState.Selected : SelectableState.None);
                selected = value;
            }
        }

        public Bounds Bounds
        {
            get => meshRenderer.bounds;
        }
        
        public string ID => snapshot.id;
        public Mesh GeneratedMesh => marchingCubes?.Mesh;
        
        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
            set
            {
                snapshot.position = value;
                transform.position = value;
            }
        }
        
        [SerializeField] private bool initOnAwake;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private ComputeShader marchingCubesCompute;
        [SerializeField] private MeshFilter meshFilter;
     
        [SerializeField] private WaveEmitter emitter;
        [SerializeField] private VolumeSnapshot snapshot = new() { id = Id.Short };
 
        private MarchingCubes marchingCubes;
        private readonly List<SculptSolidBehaviour> solids = new();
        private readonly List<SculptSolid> solidsBuffer = new();
      
        private bool selected;
        
        private MeshRenderer meshRenderer;
        private SelectableGizmo gizmo;

        private void Init()
        {
            gizmo = GetComponentInChildren<SelectableGizmo>();
            meshRenderer = meshFilter.GetComponent<MeshRenderer>();
        }
        
        private void Awake()
        {
            Init();
            if (initOnAwake)
            {
                Restore(snapshot);
            }
            Selected = false;
        }
        
        public void SetState(SelectableState state)
        {
            gizmo?.SetState(state);
        }
        
        private void Update()
        {
            if (runInUpdate && emitter)
            {
                Regenerate(emitter);
            }
        }
        
        public void InitializeMesh(MarchingCubesConfig configuration)
        {
            if (marchingCubes != null && configuration.budget != marchingCubes.Budget)
            {
                marchingCubes.Dispose();
                marchingCubes = null;
            }
            
            if (marchingCubes == null)
            {
                marchingCubes = new MarchingCubes(configuration.budget, marchingCubesCompute);
                meshFilter.mesh = marchingCubes.Mesh;
            }
        }

        public void Regenerate(WaveEmitter waveEmitter)
        {
            emitter = waveEmitter;
            CollectSolids();
            marchingCubes.SetSculptSolids(solidsBuffer);
            marchingCubes.Run(snapshot.marchingCubes, emitter);
        }

        public VolumeSnapshot CreateSnapshot()
        {
            return new VolumeSnapshot()
            {   
                position = transform.position,
                rotation = transform.localEulerAngles,
                scale = transform.localScale,
                id = snapshot.id,
                marchingCubes = snapshot.marchingCubes
            };
        }

        public void Restore(ISnapshot s)
        {
            Restore(s as VolumeSnapshot);
        }

        public void Restore(VolumeSnapshot newSnapshot)
        {
            snapshot = newSnapshot;
            transform.position = newSnapshot.position;
            transform.eulerAngles = newSnapshot.rotation;
            transform.localScale = newSnapshot.scale;
            InitializeMesh(snapshot.marchingCubes);
            OnRestored();
        }
        
        private void CollectSolids()
        {
            solidsBuffer.Clear();
            solids.Clear();
            GetComponentsInChildren(solids);
            foreach (var solid in solids)
            {
                solidsBuffer.Add(solid.GetSolid());
            }
            solidsBuffer.Sort((s1, s2) => s1.priority.CompareTo(s2.priority));
        }
        
        private void OnDestroy()
        {
            marchingCubes?.Dispose();
        }

        ISnapshot IModifiable.CreateSnapshot()
        {
            return CreateSnapshot();
        }


    }
}
