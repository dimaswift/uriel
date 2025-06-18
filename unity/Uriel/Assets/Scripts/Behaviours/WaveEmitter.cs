
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class WaveEmitter : MonoBehaviour, ISelectable
    {
        public string ID => snapshot.id;
        public bool Selected { get; set; }
        public Bounds Bounds => new(transform.position, Vector3.one);
        public int LastHash => lastSourcesHash;
        public RenderTexture Field => generator.Field;
        
        [SerializeField] private bool runInUpdate;
        [SerializeField] private bool initOnAwake;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private WaveEmitterSnapshot snapshot = new() {id = Id.Short};
        
        private FieldGenerator generator;

        private int lastSourcesHash = 0;
        
        private void Awake()
        {
            if (!initOnAwake)
            {
                return;
            }
            
            Restore(snapshot);
        }

        private void Update()
        {
            if (!runInUpdate)
            {
                return;
            }
            Run();
        }

        public void Run(bool forceRegeneration = false)
        {
            int currentHash = ComputeSourcesHash();
            
            if (currentHash == lastSourcesHash && !forceRegeneration)
            {
                return;
            }

            if (snapshot.sources.Count == 0)
            {
                return;
            }
            lastSourcesHash = currentHash;
            generator.SetSources(snapshot.sources);
            generator.Run(snapshot.saturate);
        }
        
        public void InvalidateCache()
        {
            lastSourcesHash = 0;
        }
        
        private int ComputeSourcesHash()
        {
            int hash = snapshot.sources.Count.GetHashCode();
            
            hash = hash * 31 + snapshot.CalculateHash();

            foreach (var source in snapshot.sources)
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
        
        public void SetResolution(Vector3Int res)
        {
            snapshot.resolution = res;
        }
        
        public void Restore(WaveEmitterSnapshot waveSnapshot)
        {
            if (generator != null && waveSnapshot.resolution != snapshot.resolution)
            {
                generator.Dispose();
                generator = null;
            }

            if (generator == null)
            {
                generator = new FieldGenerator(compute, waveSnapshot.resolution);
            }
            
            snapshot = waveSnapshot;
   
            InvalidateCache();
        }
        
        private void OnDestroy()
        {
            generator?.Dispose();
        }
        
        public WaveEmitterSnapshot CreateSnapshot()
        {
            return snapshot;
        }
    }
}
