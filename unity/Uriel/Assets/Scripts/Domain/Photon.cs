using System;
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
        public int frequency;
        public float amplitude;
        public float phase;
        public float radius;
        public float density;
        public float scale;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Photon other)
        {
            return transform.Equals(other.transform) 
                   && iterations == other.iterations 
                   && type == other.type 
                   && frequency == other.frequency 
                   && amplitude.Equals(other.amplitude) 
                   && phase.Equals(other.phase) 
                   && radius.Equals(other.radius) 
                   && density.Equals(other.density)
                   && scale.Equals(other.scale);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(transform);
            hashCode.Add(iterations);
            hashCode.Add((int) type);
            hashCode.Add(frequency);
            hashCode.Add(amplitude);
            hashCode.Add(phase);
            hashCode.Add(radius);
            hashCode.Add(density);
            hashCode.Add(scale);
            return hashCode.ToHashCode();
        }
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