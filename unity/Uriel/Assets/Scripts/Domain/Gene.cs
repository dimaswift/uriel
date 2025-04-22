using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]  
    [System.Serializable]
    public struct Gene
    {
        public int iterations;
        public int shift;
        public float frequency;
        public float amplitude;
        public int operation;
        public Vector3 offset;
        public float scale;
        public float phase;
    }
}