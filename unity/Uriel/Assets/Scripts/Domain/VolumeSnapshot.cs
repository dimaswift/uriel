using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;
using Uriel.Commands;

namespace Uriel.Domain
{
    [System.Serializable]
    public class VolumeSnapshot : ISnapshot
    {
        public string ID
        {
            get => id;
            set => id = value;
        }

        public string ParentID { get; set; }
        public string TargetType => nameof(Volume);
        public bool ValueEquals(ISnapshot snapshot)
        {
            if (snapshot is not VolumeSnapshot s)
            {
                return false;
            }

            return s.id == id && s.scale == scale && s.position == position && s.rotation == rotation &&
                   s.marchingCubes.Equals(marchingCubes);
        }

        public MarchingCubesConfig marchingCubes = MarchingCubesConfig.Default;
        public string id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
    
    [System.Serializable]
    public class WaveEmitterSnapshot  : ISnapshot
    {
        public string ParentID { get; set; }
        public string TargetType => nameof(WaveEmitter);
        public bool ValueEquals(ISnapshot snapshot)
        {
            if (snapshot is not WaveEmitterSnapshot s)
            {
                return false;
            }

            var hash = s.CalculateHash();
            return hash == CalculateHash();
        }

        public string ID
        {
            get => id;
            set => id = value;
        }
        public string id;
        public Vector3Int resolution;
        public bool saturate;
        public List<WaveSource> sources = new();

        public int CalculateHash()
        {
            int h = id.GetHashCode();
            foreach (var source in sources)
            {
                h += source.GetHashCode();
            }
            h += resolution.GetHashCode();
            h += saturate.GetHashCode();
            return h;
        } 
    }
    
    [Serializable]
    public class StudioState
    {
        public List<WaveEmitterSnapshot> waveEmitters = new();
        public List<VolumeSnapshot> volumes = new ();
        public List<SculptSolidSnapshot> solids = new ();
        public string name;
        public bool showGrid;
    }
}