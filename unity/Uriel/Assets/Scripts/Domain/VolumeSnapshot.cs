using System;
using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    [System.Serializable]
    public class VolumeSnapshot
    {
        public string id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public List<SculptSolidState> solids = new();
    }
    
    [System.Serializable]
    public class StudioState
    {
        public List<VolumeSnapshot> volumes = new ();
        public DateTime lastSaved;
        public string name;
    }
}