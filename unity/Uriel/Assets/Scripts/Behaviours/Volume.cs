using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Volume : MonoBehaviour, ISelectable, IModifiable
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
        public Mesh GeneratedMesh => marchingCubes?.Mesh;
        
        [SerializeField] private int budget = 1000000;
        [SerializeField] private bool initOnAwake;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private string id = Id.Short;
        [SerializeField] private MarchingCubesConfig config = MarchingCubesConfig.Default;
        [SerializeField] private ComputeShader marchingCubesCompute;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material regularMaterial;
        [SerializeField] private WaveEmitter emitter;
        private MarchingCubes marchingCubes;
        private readonly List<SculptSolidBehaviour> solids = new();
        private readonly List<SculptSolid> solidsBuffer = new();
        private readonly List<SculptSolidState> solidsStateBuffer = new();

        private bool selected;
        
        private MeshRenderer meshRenderer;
        
        private void Awake()
        {
            meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (initOnAwake)
            {
                Initialize(Id.Short, budget, config);
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
        
        public void Initialize(string newId, int triangleBudget, MarchingCubesConfig configuration)
        {
            id = newId;
            name = $"VOL_{id}";
            budget = triangleBudget;
            if (marchingCubes != null)
            {
                marchingCubes.Dispose();
                marchingCubes = null;
            }
            
            config = configuration;
       
            marchingCubes = new MarchingCubes(budget, marchingCubesCompute);
            meshFilter.mesh = marchingCubes.Mesh;
        }

        public void Regenerate(WaveEmitter waveEmitter)
        {
            emitter = waveEmitter;
            CollectSolids();
            marchingCubes.SetSculptSolids(solidsBuffer);
            marchingCubes.Run(config, emitter);
        }

        public VolumeSnapshot CreateSnapshot()
        {
            CollectSolids();
            return new VolumeSnapshot
            {
                id = id,
                position = transform.position,
                rotation = transform.eulerAngles,
                scale = transform.localScale,
                solids = new List<SculptSolidState>(solidsStateBuffer)
            };
        }

        public void Restore(VolumeSnapshot snapshot)
        {
            id = snapshot.id;
            transform.position = snapshot.position;
            transform.eulerAngles = snapshot.rotation;
            transform.localScale = snapshot.scale;
            
            LoadSolidStates(snapshot.solids);
        }
        
        private void CollectSolids()
        {
            solidsBuffer.Clear();
            solidsStateBuffer.Clear();
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
                solidsStateBuffer.Add(solid.GetState());
            }
            solidsBuffer.Sort((s1, s2) => s1.priority.CompareTo(s2.priority));
        }

        
        public void LoadSolidStates(List<SculptSolidState> solidStates)
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
            for (int i = 0; i < solidStates.Count; i++)
            {
                var s = solidStates[i];
                var b = solids[i];
                b.RestoreState(s);
            }
            solidStates.Clear();
            solidStates.AddRange(solidStates);
        }

        private void OnDestroy()
        {
            marchingCubes?.Dispose();
        }
    }
}
