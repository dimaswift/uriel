using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]  
    [System.Serializable]
    public struct Gene
    {
        public uint iterations;
        public float frequency;
        public float amplitude;
        public Vector3 source;
        public float scale;
        public float phase;
    }
} 