using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct WaveSource
    {
        public Vector3 position;
        public int frequency;
        public float amplitude;
        public float phase;
        public float radius;
        public float scale;

        public static WaveSource Default => new ()
        {
            frequency = 10,
            amplitude = 0.5f,
            radius = 1,
            scale = 2,
            phase = 2
        };
    }
}