using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct Star
    {
        public float frequency;
        public float amplitude;
        public float phase;
        public float dutyCycle;
        public float velocity;
        public float time;
        public Vector3 location;
    }
}