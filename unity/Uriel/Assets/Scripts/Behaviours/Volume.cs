using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Volume : MonoBehaviour, IMovable
    {
        public event Action OnRestored = () => { };
        public VolumeSnapshot Snapshot => snapshot;
        public bool Selected
        {
            get => selected;
            set
            {
                selectionGizmo?.SetSelected(value);
                selected = value;
            }
        }

        public Bounds Bounds
        {
            get => meshRenderer.bounds;
        }
        
        public string ID => snapshot.id;
        public Mesh GeneratedMesh => marchingCubes?.Mesh;
        
        [SerializeField] private bool initOnAwake;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private ComputeShader marchingCubesCompute;
        [SerializeField] private MeshFilter meshFilter;
     
        [SerializeField] private WaveEmitter emitter;
        [SerializeField] private VolumeSnapshot snapshot = new() { id = Id.Short };
        [SerializeField] private SelectionGizmo selectionGizmo;
        
        private MarchingCubes marchingCubes;
        private readonly List<SculptSolidBehaviour> solids = new();
        private readonly List<SculptSolid> solidsBuffer = new();
      
        private bool selected;
        
         private MeshRenderer meshRenderer;
        
        private void Awake()
        {
            meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (initOnAwake)
            {
                Restore(snapshot);
            }
            Selected = false;
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

        public void Regenerate()
        {
            if (emitter == null)
            {
                return;
            }
            CollectSolids();
            marchingCubes.SetSculptSolids(solidsBuffer);
            marchingCubes.Run(snapshot.marchingCubes, emitter);
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
            CollectSolids();
            return new VolumeSnapshot()
            {   
                position = transform.position,
                rotation = transform.localEulerAngles,
                scale = transform.localScale,
                solids = new List<SculptSolidState>(snapshot.solids),
                id = snapshot.id,
                marchingCubes = snapshot.marchingCubes
            };
        }

        public void Restore(VolumeSnapshot newSnapshot)
        {
            snapshot = newSnapshot;
            transform.position = newSnapshot.position;
            transform.eulerAngles = newSnapshot.rotation;
            transform.localScale = newSnapshot.scale;
            InitializeMesh(snapshot.marchingCubes);
            LoadSolidStates(newSnapshot.solids);
            OnRestored();
        }
        
        private void CollectSolids()
        {
            solidsBuffer.Clear();
            snapshot.solids.Clear();
            GetComponentsInChildren(solids);
            foreach (var solid in solids)
            {
                solidsBuffer.Add(solid.GetSolid());
                snapshot.solids.Add(solid.GetState());
            }
            solidsBuffer.Sort((s1, s2) => s1.priority.CompareTo(s2.priority));
        }
        
        private void LoadSolidStates(List<SculptSolidState> solidStates)
        {
            CollectSolids();
            while (solids.Count > solidStates.Count)
            {
                Destroy(solids[0].gameObject);
                solids.RemoveAt(0);
            }
            while (solids.Count < solidStates.Count)
            {
                var s = new GameObject($"Solid_{solids.Count}").AddComponent<SculptSolidBehaviour>();
                s.transform.SetParent(transform);
                solids.Add(s);
            }
            solidsBuffer.Clear();
            for (int i = 0; i < solidStates.Count; i++)
            {
                var s = solidStates[i];
                var b = solids[i];
                b.RestoreState(s);
                solidsBuffer.Add(s.solid);
            }
        }

        private void OnDestroy()
        {
            marchingCubes?.Dispose();
        }

        public Vector3 position
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
    }
}
