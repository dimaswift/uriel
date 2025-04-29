using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]  
    [System.Serializable]
    public struct Wave
    {
        public Vector3 source;
        public uint ripples;
        public uint harmonic;
        public float frequency;
        public float amplitude;
        public float density;
        public float phase;
    }
}