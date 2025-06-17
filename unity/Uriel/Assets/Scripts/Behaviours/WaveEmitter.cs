
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class WaveEmitter : MonoBehaviour
    {
        public int LastHash => lastSourcesHash;
        public RenderTexture Field => generator.Field;
        [SerializeField] private bool runInUpdate;
        [SerializeField] private bool initOnAwake;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private FieldConfig fieldConfig;
        [SerializeField] private Vector3Int resolution = new(64,64,64);
        private FieldGenerator generator;
        private readonly List<WaveSource> sourcesBuffer = new();
        private readonly List<WaveSourceBehaviour> sources = new();
        
        // Hash-based change detection
        private int lastSourcesHash = 0;
        private bool forceRegeneration = false;
 
        private void Awake()
        {
            if (!initOnAwake)
            {
                return;
            }
            Initialize(fieldConfig, resolution);
        }

        private void Update()
        {
            if (!runInUpdate)
            {
                return;
            }
            Run();
        }

        public void Run()
        {
            CollectSources();
            
            // Compute hash of current sources state
            int currentHash = ComputeSourcesHash();
            
            // Skip generation if sources haven't changed and no forced regeneration
            if (currentHash == lastSourcesHash && !forceRegeneration)
            {
                return;
            }
            
            // Update hash and run generation
            lastSourcesHash = currentHash;
            forceRegeneration = false;
            
            generator.SetSources(sourcesBuffer);
            generator.Run(fieldConfig);
        }
        
        /// <summary>
        /// Forces the next Run() call to regenerate regardless of source changes
        /// </summary>
        public void ForceRegeneration()
        {
            forceRegeneration = true;
        }
        
        /// <summary>
        /// Invalidates the cached hash, forcing regeneration on next Run()
        /// </summary>
        public void InvalidateCache()
        {
            lastSourcesHash = 0;
            forceRegeneration = true;
        }
        
        private int ComputeSourcesHash()
        {
            int hash = sourcesBuffer.Count.GetHashCode();
            
            // Hash the field config as it affects generation
            hash = hash * 31 + fieldConfig.GetHashCode();
            
            // Hash each wave source's properties
            foreach (var source in sourcesBuffer)
            {
                hash = hash * 31 + source.position.GetHashCode();
                hash = hash * 31 + source.frequency.GetHashCode();
                hash = hash * 31 + source.amplitude.GetHashCode();
                hash = hash * 31 + source.phase.GetHashCode();
                hash = hash * 31 + source.radius.GetHashCode();
                hash = hash * 31 + source.scale.GetHashCode();
            }
            
            return hash;
        }
        
        private void AddDefaultSource(Vector3 point)
        {
            var source = new GameObject("DefaultSource").AddComponent<WaveSourceBehaviour>();
            source.transform.SetParent(transform);
            source.transform.localPosition = point;
            sources.Add(source);
        }
        
        private void CollectSources()
        {
            sourcesBuffer.Clear();
            GetComponentsInChildren(sources);
            
            if (sources.Count == 0)
            {
                AddDefaultSource(new Vector3(0.35355339f, 0.35355339f, 0.35355339f));
                AddDefaultSource(new Vector3(0.35355339f, -0.35355339f, -0.35355339f));
                AddDefaultSource(new Vector3(-0.35355339f, 0.35355339f, -0.35355339f));
                AddDefaultSource(new Vector3(-0.35355339f, -0.35355339f, 0.35355339f));
                
                // Force regeneration when default sources are added
                forceRegeneration = true;
            }
            
            foreach (var s in sources)
            {
                sourcesBuffer.Add(s.GetWaveSource());
            }
        }
        
        public void Initialize(FieldConfig config, Vector3Int res)
        {
            resolution = res;
            if (generator != null)
            {
                generator.Dispose();
                generator = null;
            }
            fieldConfig = config;
            generator = new FieldGenerator(compute, res);
            
            // Invalidate cache when reinitializing
            InvalidateCache();
        }
        
        private void OnDestroy()
        {
            generator?.Dispose();
        }
    }
}
