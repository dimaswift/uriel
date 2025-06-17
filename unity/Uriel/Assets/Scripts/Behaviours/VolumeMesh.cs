using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class VolumeMesh : MonoBehaviour
    {
        public bool Selected
        {
            get => selected;
            set
            {
                meshRenderer.sharedMaterial = value ? selectedMaterial : regularMaterial;
                selected = value;
            }
        }

        public Bounds Bounds
        {
            get => meshRenderer.bounds;
        }
        
        public string ID => id;
        public string DisplayName { get; set; } = "Volume Field";
        public Mesh GeneratedMesh => marchingCubes?.Mesh;
        
        [SerializeField] private int budget = 1000000;
        [SerializeField] private bool initOnAwake;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private string id = Guid.NewGuid().ToString();
        [SerializeField] private SculptState state;
        [SerializeField] private MarchingCubesConfig config = MarchingCubesConfig.Default;
        [SerializeField] private ComputeShader marchingCubesCompute;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private WaveEmitter waveEmitter;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material regularMaterial;
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
                Initialize(budget, config, waveEmitter);
            }

            Selected = false;
        }

        private void Update()
        {
            if (runInUpdate)
            {
                RegenerateVolume();
            }
        }
        
        public void ChangeResolution(int triangleBudget)
        {
            budget = triangleBudget;
            if (marchingCubes != null)
            {
                marchingCubes.Dispose();
                marchingCubes = null;
            }
            marchingCubes = new MarchingCubes(budget, marchingCubesCompute);
            meshFilter.mesh = marchingCubes.Mesh;
        }
        
        public void Initialize(int triangleBudget, MarchingCubesConfig configuration, WaveEmitter emitter)
        {
            waveEmitter = emitter;
            budget = triangleBudget;
            if (marchingCubes != null)
            {
                marchingCubes.Dispose();
                marchingCubes = null;
            }
            
            config = configuration;
       
            marchingCubes = new MarchingCubes(budget, marchingCubesCompute);
            meshFilter.mesh = marchingCubes.Mesh;
            
            LoadState();
            RegenerateVolume();
        }

        public void SetEmitter(WaveEmitter emitter)
        {
            waveEmitter = emitter;
        }
        
        public void RegenerateVolume()
        {
            CollectSolids();
            marchingCubes.SetSculptSolids(solidsBuffer);
            marchingCubes.Run(config, waveEmitter);
        }

        public VolumeFieldSnapshot CreateSnapshot()
        {
            CollectSolids();
            SaveState();
            
            return new VolumeFieldSnapshot
            {
                id = this.id,
                name = DisplayName,
                position = transform.position,
                rotation = transform.eulerAngles,
                scale = transform.localScale,
                sculptState = state
            };
        }

        public void RestoreFromSnapshot(VolumeFieldSnapshot snapshot)
        {
            this.id = snapshot.id;
            this.DisplayName = snapshot.name;
            transform.position = snapshot.position;
            transform.eulerAngles = snapshot.rotation;
            transform.localScale = snapshot.scale;
            this.state = snapshot.sculptState;
            
            LoadState();
            RegenerateVolume();
        }
        
        private void CollectSolids()
        {
            solidsBuffer.Clear();
            GetComponentsInChildren(solids);

            if (solids.Count == 0)
            {
                var solid = new GameObject("DefaultSolid").AddComponent<SculptSolidBehaviour>();
                solid.transform.SetParent(transform);
                solids.Add(solid);
            }
            
            foreach (var solid in solids)
            {
                solidsBuffer.Add(solid.GetSolid());
            }
            solidsBuffer.Sort((s1, s2) => s1.priority.CompareTo(s2.priority));
        }

      
        private void SaveState()
        {
            if (state == null) return;
            
            CollectSolids();
            state.solids.Clear();
            foreach (var behaviour in solids)
            {
                state.solids.Add(behaviour.GetState());
            }
        }

        private void LoadState()
        {
            if (state?.solids == null || state.solids.Count == 0) return;
            
            CollectSolids();
            while (solids.Count > state.solids.Count)
            {
                Destroy(solids[0].gameObject);
                solids.RemoveAt(0);
            }
            while (solids.Count < state.solids.Count)
            {
                var s = new GameObject($"Solid_{solids.Count}").AddComponent<SculptSolidBehaviour>();
                s.transform.SetParent(transform);
                solids.Add(s);
            }
            for (int i = 0; i < state.solids.Count; i++)
            {
                var s = state.solids[i];
                var b = solids[i];
                b.RestoreState(s);
            }
        }

        private void OnDestroy()
        {
            marchingCubes?.Dispose();
        }
    }
    
    public class AddSolidCommand : ICommand
    {
        private VolumeMesh volumeMesh;
        private SculptSolidBehaviour solid;
        
        public string Description => $"Add {solid.name}";

        public AddSolidCommand(VolumeMesh mesh, SculptSolidBehaviour solid)
        {
            this.volumeMesh = mesh;
            this.solid = solid;
        }

        public void Execute()
        {
            solid.transform.SetParent(volumeMesh.transform);
            volumeMesh.RegenerateVolume();
        }

        public void Undo()
        {
            if (solid != null)
            {
                solid.transform.SetParent(null);
                volumeMesh.RegenerateVolume();
            }
        }
    }
    
    public class RemoveSolidCommand : ICommand
    {
        private VolumeMesh volumeMesh;
        private SculptSolidBehaviour solid;
        private Transform originalParent;
        
        public string Description => $"Remove {solid.name}";

        public RemoveSolidCommand(VolumeMesh mesh, SculptSolidBehaviour solid)
        {
            this.volumeMesh = mesh;
            this.solid = solid;
            this.originalParent = solid.transform.parent;
        }

        public void Execute()
        {
            solid.transform.SetParent(null);
            solid.gameObject.SetActive(false);
            volumeMesh.RegenerateVolume();
        }

        public void Undo()
        {
            solid.gameObject.SetActive(true);
            solid.transform.SetParent(originalParent);
            volumeMesh.RegenerateVolume();
        }
    }
}
