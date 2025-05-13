using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]  
    [System.Serializable]
    public struct Photon
    {
        public Matrix4x4 transform;
        public uint iterations;
        public Solid type;
        public float frequency;
        public float amplitude;
        public float phase;
        public float radius;
        public float density;
    }

    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct Modulation
    {
        public float time;
        public float frequency;
        public float phase;
        public float amplitude;
    }

    public enum Solid
    {
        Tetrahedron = 0,
        Octahedron = 1,
        Cube = 2,
        Icosahedron = 3,
        Dodecahedron = 4,
        Matrix = 5
    }
}