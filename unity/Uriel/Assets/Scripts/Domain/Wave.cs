using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]  
    [System.Serializable]
    public struct Wave
    {
        public Vector3 source;
        public Vector2 rotation;
        public uint ripples;
        public uint harmonic;
        public Solid type;
        public float frequency;
        public float amplitude;
        public float density;
        public float phase;
        public float depth;
    }

    public enum Solid
    {
        Tetrahedron = 0,
        Octahedron = 1,
        Cube = 2,
        Icosahedron = 3,
        Dodecahedron = 4
    }
}