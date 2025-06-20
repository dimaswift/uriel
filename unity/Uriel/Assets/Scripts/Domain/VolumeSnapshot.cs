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

        public string TargetType => nameof(Volume);
        public MarchingCubesConfig marchingCubes = MarchingCubesConfig.Default;
        public string id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public List<SculptSolidState> solids = new();
    }
    
    [System.Serializable]
    public class WaveEmitterSnapshot  : ISnapshot
    {
        public string TargetType => nameof(WaveEmitter);
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

            h += saturate.GetHashCode();
            return h;
        } 
    }
    
    [Serializable]
    public class StudioState
    {
        public List<WaveEmitterSnapshot> waveEmitters = new();
        public List<VolumeSnapshot> volumes = new ();
        public string name;
    }
}