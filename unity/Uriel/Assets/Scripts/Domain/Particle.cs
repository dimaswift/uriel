using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct Particle
    {
        public Vector3 position;
        public float charge;
    }
}