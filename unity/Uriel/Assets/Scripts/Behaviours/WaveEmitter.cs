using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class WaveEmitter : MonoBehaviour, IMovable, IModifiable
    {
        public ISnapshot Current => snapshot;
        public Vector3 position
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }
        public string ID => snapshot.id;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selectionGizmo?.SetSelected(value);
                selected = value;
            }
        }
        public Bounds Bounds => new(transform.position, Vector3.one * 0.1f);
        public int LastHash => lastSourcesHash;
        public RenderTexture Field => generator.Field;
        
        [SerializeField] private bool runInUpdate;
        [SerializeField] private bool initOnAwake;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private SelectionGizmo selectionGizmo;
        [SerializeField] private WaveEmitterSnapshot snapshot = new() {id = Id.Short};
        
        private FieldGenerator generator;

        private int lastSourcesHash = 0;

        private bool selected;

        private Vector3Int currentResolution;
        
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
            if (generator != null && currentResolution != snapshot.resolution)
            {
                generator.Dispose();
                generator = null;
            }

            if (generator == null)
            {
                generator = new FieldGenerator(compute, snapshot.resolution);
            }

            currentResolution = snapshot.resolution;
            
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
        
        public void Restore(WaveEmitterSnapshot waveSnapshot)
        {
            snapshot.saturate = waveSnapshot.saturate;
            snapshot.sources = new List<WaveSource>(waveSnapshot.sources);
            snapshot.resolution = waveSnapshot.resolution;
            snapshot.id = waveSnapshot.id;
            InvalidateCache();
        }
        
        private void OnDestroy()
        {
            generator?.Dispose();
        }
        
        public WaveEmitterSnapshot CreateSnapshot()
        {
            return new WaveEmitterSnapshot()
            {
                sources = new List<WaveSource>(snapshot.sources),
                saturate = snapshot.saturate,
                resolution = snapshot.resolution,
                id = snapshot.id
            };
        }

        public void Restore(ISnapshot s)
        {
            Restore(s as WaveEmitterSnapshot);
        }
        
        ISnapshot IModifiable.CreateSnapshot()
        {
            return CreateSnapshot();
        }
    }
}
